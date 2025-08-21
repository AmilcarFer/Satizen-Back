using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Proyec_Satizen_Api;
using Satizen_Api;
using Satizen_Api.Custom;
using Satizen_Api.Data;
using Satizen_Api.Hubs;
using Satizen_Api.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddScoped<Utilidades>(); // Acá se agregan las utilidades
builder.Services.AddSignalR(); // Acá se agregan las utilidades
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// ------------- Seguridad JWT para los usuarios -------------------

builder.Services.AddAuthentication(config =>
{
    config.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    config.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

}).AddJwtBearer(config => {
    config.RequireHttpsMetadata = false;
    config.SaveToken = true;
    config.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero,
        IssuerSigningKey = new SymmetricSecurityKey
        (Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
    };

});
// ------------------------------------------------------------------

builder.Services.AddControllers().AddNewtonsoftJson();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

//Acá se agrega el contexto de la base de datos y se define el nombre de la cadena de conexion
builder.Services.AddDbContext<ApplicationDbContext>(option =>
{
    option.UseSqlServer(builder.Configuration.GetConnectionString("Conexion"));
});

//--------------------------------------------------------------------------------------------

//Configuración de roles
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy => policy.RequireClaim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "1"));
    options.AddPolicy("Doctor", policy => policy.RequireClaim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "2"));
    options.AddPolicy("Enfermero", policy => policy.RequireClaim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "3"));

    options.AddPolicy("AdminDoctorEnfermero", policy =>
        policy.RequireAssertion(context =>
            context.User.HasClaim(c => (c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role" && (c.Value == "1" || c.Value == "2" || c.Value == "3")))));

    options.AddPolicy("AdminDoctor", policy =>
        policy.RequireAssertion(context =>
            context.User.HasClaim(c => (c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role" && (c.Value == "1" || c.Value == "2")))));

    options.AddPolicy("DoctorOrEnfermero", policy =>
        policy.RequireAssertion(context =>
            context.User.HasClaim(c => (c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role" && (c.Value == "2" || c.Value == "3")))));

});

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", builder =>
    {
        builder.AllowAnyMethod()
               .AllowAnyHeader()
               .SetIsOriginAllowed(_ => true) // Permite cualquier origen
               .AllowCredentials();
    });
});



var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseCors("CorsPolicy");  // Asegúrate que está antes de UseAuthorization
app.UseAuthentication();    // Asegúrate de que UseAuthentication está antes
app.UseAuthorization();
app.MapHub<ChatHub>("/chatHub");
app.MapHub<AlertaHub>("/alertaHub");
app.MapControllers();
app.Run();

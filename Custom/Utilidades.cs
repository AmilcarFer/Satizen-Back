using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

using Satizen_Api.Models;
using Satizen_Api.Data;
using Satizen_Api.Models.Dto.Usuarios;

namespace Satizen_Api.Custom
{
    public class Utilidades
    {
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _applicationDbContext;


        public Utilidades(ApplicationDbContext applicationDbContext, IConfiguration configuration)
        {
            _configuration = configuration;
            _applicationDbContext = applicationDbContext;

        }

        // Clase que encripta la contraseña 
        public string encriptarSHA256(string texto)
        {
            using(SHA256 sha256Hash = SHA256.Create())
            {
                //Computar hash
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(texto));

                //Convertir el array de bytes en string
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }

                return builder.ToString();
            }
        }

        //Clase que genera el token
        public string generarJWT(Usuario modelo)
        {
            //Crear la información del usuario para el token
            var userClaims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, modelo.idUsuario.ToString()),
                //new Claim("idUsuario", modelo.idUsuario.ToString()),
                new Claim(ClaimTypes.Name, modelo.nombreUsuario!),
                //new Claim("nombreUsuario", modelo.nombreUsuario),
                new Claim(ClaimTypes.Role, modelo.idRoles.ToString())
                //new Claim("Rol", modelo.idRoles.ToString())


                //Acá se puede agregar más información relevante del usuario y
                //también los roles para después agregar un sistema basado en roles

            };


            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);

            //Crear detalle del token 
            var jwtConfig = new JwtSecurityToken(
                claims: userClaims,
                expires: DateTime.UtcNow.AddMinutes(10), //Acá se define cuanto va a durar el token
                signingCredentials: credentials
                );
            return new JwtSecurityTokenHandler().WriteToken(jwtConfig);
        }

        //----------------------Clase que genera el refresh token
        public string generarRefreshJWT()
        {
            var byteArray = new byte[64];
            var refreshToken = "";

            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(byteArray);
                refreshToken = Convert.ToBase64String(byteArray);
            }
            return refreshToken;
        }

        public async Task<AutorizacionResponseDto> GuardarRefreshToken(int idUsuario, string token, string refreshToken)
        {
            var oldTokens = await _applicationDbContext.RefreshTokens.Where(rt => rt.idUsuario == idUsuario && rt.esActivo).ToListAsync();
            foreach (var oldToken in oldTokens)
            {
                oldToken.esActivo = false;
                _applicationDbContext.RefreshTokens.Update(oldToken);
            }

            var newRefreshToken = new RefreshToken
            {
                idUsuario = idUsuario,
                token = token,
                refreshToken = refreshToken,
                fechaCreacion = DateTime.UtcNow,
                fechaExpiracion = DateTime.UtcNow.AddMinutes(60), // Duración del refresh token
                esActivo = true
            };

            await _applicationDbContext.RefreshTokens.AddAsync(newRefreshToken);
            await _applicationDbContext.SaveChangesAsync();

            return new AutorizacionResponseDto { Token = token, RefreshToken = refreshToken, Resultado = true, Msg = "ok" };
        }

    }
}

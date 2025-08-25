using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Prueba_Tecnica_Api.Models.Dto.Usuarios;
using Satizen_Api.Custom;
using Satizen_Api.Data;
using Satizen_Api.Models;
using Satizen_Api.Models.Dto.Usuarios;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Diagnostics;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Satizen_Api.Controllers
{
    [Route("api/[controller]")]
    [AllowAnonymous] //Esta sentencia define que no es necesario estar autorizado para usar este controlador
    [ApiController]
    public class AccesoController : ControllerBase
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly Utilidades _utilidades;
        protected ApiResponse _response;
        private readonly IConfiguration _configuration;

        public AccesoController(ApplicationDbContext applicationDbContext, Utilidades utilidades, IConfiguration configuration)
        {
            _applicationDbContext = applicationDbContext;
            _utilidades = utilidades;
            _response = new();
            _configuration = configuration;
        }


        // EndPoint que registra un usuario
        [HttpPost]
        [Route("RegistrarUsuario")]
        public async Task<ActionResult<ApiResponse>> Registrarse(UsuarioCreateDto objeto)
        {
            try
            {
                //Acá se valida que no exista el nombre de usuario
                if (await _applicationDbContext.Usuarios.AnyAsync(u => u.nombreUsuario == objeto.nombreUsuario))
                {
                    _response.statusCode = HttpStatusCode.BadRequest;
                    _response.IsExitoso = false;
                    _response.ErrorMessages = new List<string> { "El nombre de usuarios ya existe" };
                    return StatusCode((int)_response.statusCode, _response);
                }


                var modeloUsuario = new Usuario
                {
                    nombreUsuario = objeto.nombreUsuario,
                    correo = objeto.correo,
                    password = _utilidades.encriptarSHA256(objeto.password),
                    idRoles = objeto.idRoles,
                    fechaCreacion = DateTime.Now,
                };

                await _applicationDbContext.Usuarios.AddAsync(modeloUsuario);
                await _applicationDbContext.SaveChangesAsync();

                if (modeloUsuario.idUsuario != 0)
                {
                    _response.statusCode = HttpStatusCode.OK;
                    _response.IsExitoso = true;
                    _response.Resultado = modeloUsuario;
                }
                else
                {
                    _response.statusCode = HttpStatusCode.InternalServerError;
                    _response.IsExitoso = false;
                    _response.ErrorMessages = new List<string> { "Error al registrar el usuario." };
                }

                return StatusCode((int)_response.statusCode, _response);
            }
            catch (Exception ex)
            {
                _response.IsExitoso = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
                return StatusCode((int)_response.statusCode, _response);
            }

        }

        // EndPoint que loggea al usuario
        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login(LoginDto objeto)
        {
            var usuarioEncontrado = await _applicationDbContext.Usuarios
                                        .Where(u =>
                                        u.nombreUsuario == objeto.nombreUsuario &&
                                        u.password == _utilidades.encriptarSHA256(objeto.password) &&
                                        u.fechaEliminacion == null
                                        ).FirstOrDefaultAsync();

            if (usuarioEncontrado == null)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new { isSuccess = false, token = "", refreshToken = "" });
            }
            else
            {
                // Generar JWT
                var token = _utilidades.generarJWT(usuarioEncontrado);

                // Generar Refresh Token
                var refreshToken = _utilidades.generarRefreshJWT();

                // Guardar el refresh token en la base de datos
                var response = await _utilidades.GuardarRefreshToken(usuarioEncontrado.idUsuario, token, refreshToken);

                return StatusCode(StatusCodes.Status200OK, new { isSuccess = true, token = response.Token, refreshToken = response.RefreshToken });
            }
        }


        //Generar refresh token
        [HttpPost]
        [Route("RefreshToken")]
        public async Task<IActionResult> RefreshToken(RefreshTokenRequestDto refreshTokenRequest)
        {
            var storedRefreshToken = await _applicationDbContext.RefreshTokens
                                            .FirstOrDefaultAsync(r => r.refreshToken == refreshTokenRequest.RefreshToken);

            if (storedRefreshToken == null || storedRefreshToken.fechaExpiracion <= DateTime.UtcNow || !storedRefreshToken.esActivo)
            {
                return Unauthorized(new { isSuccess = false, message = "Invalid or expired refresh token" });
            }

            var usuario = await _applicationDbContext.Usuarios
                                .FirstOrDefaultAsync(u => u.idUsuario == storedRefreshToken.idUsuario);

            if (usuario == null)
            {
                return Unauthorized(new { isSuccess = false, message = "Invalid user" });
            }

            // Generar nuevo JWT
            var newToken = _utilidades.generarJWT(usuario);

            // Generar nuevo Refresh Token
            var newRefreshToken = _utilidades.generarRefreshJWT();

            // Desactivar el refresh token anterior
            storedRefreshToken.esActivo = false;
            _applicationDbContext.RefreshTokens.Update(storedRefreshToken);

            // Guardar el nuevo refresh token
            var response = await _utilidades.GuardarRefreshToken(usuario.idUsuario, newToken, newRefreshToken);

            return Ok(new { isSuccess = true, token = response.Token, refreshToken = response.RefreshToken });
        }


        //Recuperar contraseña
        [HttpPatch]
        [Route("RecuperarContraseña")]
        public async Task<ActionResult<ApiResponse>> Restaurar(RecuperarPasswordDto recuperarDto)
        {
            try
            {
                var usuario = await _applicationDbContext.Usuarios.FirstOrDefaultAsync(u => u.correo == recuperarDto.correo && u.fechaEliminacion == null);
                if (usuario == null)
                {
                    _response.IsExitoso = false;
                    _response.statusCode = HttpStatusCode.BadRequest;
                    _response.ErrorMessages = new List<string> { "El correo no existe" };

                    return BadRequest(_response);
                }

                //var token = GetSha256(Guid.NewGuid().ToString());

                usuario.recoveryToken = GetSha256(Guid.NewGuid().ToString());


                _applicationDbContext.Usuarios.Update(usuario);
                await _applicationDbContext.SaveChangesAsync();
                //Enviar Email
                await SendEmail(usuario.correo, usuario.recoveryToken, _configuration);

                _response.statusCode = HttpStatusCode.OK;
                _response.IsExitoso = true;
                _response.Resultado = usuario;

                return StatusCode((int)_response.statusCode, _response);

            }
            catch (Exception ex)
            {
                _response.IsExitoso = false;
                _response.ErrorMessages = new List<string> { ex.ToString() };
                return StatusCode((int)_response.statusCode, _response);
            }
        }

        //Cambiar contraseña
        [HttpPatch]
        [Route("CambiarContraseña")]
        public async Task<ActionResult<ApiResponse>> Cambiar(CambiarPasswordDto cambiarDto)
        {
            try
            {
                var usuario = await _applicationDbContext.Usuarios.FirstOrDefaultAsync(u => u.recoveryToken == cambiarDto.recoveryToken);
                if (usuario == null)
                {
                    _response.IsExitoso = false;
                    _response.statusCode = HttpStatusCode.BadRequest;
                    _response.ErrorMessages = new List<string> { "El token no es correcto o ha vencido" };

                    return BadRequest(_response);
                }

                usuario.password = _utilidades.encriptarSHA256(cambiarDto.password);
                usuario.recoveryToken = null;

                _applicationDbContext.Usuarios.Update(usuario);
                await _applicationDbContext.SaveChangesAsync();

                _response.statusCode = HttpStatusCode.OK;
                _response.IsExitoso = true;
                _response.Resultado = usuario;

                return StatusCode((int)_response.statusCode, _response);

            }
            catch (Exception ex)
            {
                _response.IsExitoso = false;
                _response.ErrorMessages = new List<string> { ex.ToString() };
                return StatusCode((int)_response.statusCode, _response);
            }
        }

        //Generar recovery token
        private string GetSha256(string cadena)
        {
            SHA256 sha256 = SHA256.Create();
            ASCIIEncoding encoding = new ASCIIEncoding();
            byte[] stream = null;
            StringBuilder sb = new StringBuilder();
            stream = sha256.ComputeHash(encoding.GetBytes(cadena));
            for (int i = 0; i < stream.Length; i++) sb.AppendFormat("{0:x2}", stream[i]);
            return sb.ToString();
        }

        //Generar correo electronico
        static async Task SendEmail(string emailDestino, string token, IConfiguration configuration)
        {
            //var apiKey = Environment.GetEnvironmentVariable("SatizenCorreoClave");
            string apiKey = configuration["SendGrid:ApiKey"];
            var client = new SendGridClient(apiKey);

            //string url = "http://localhost:8081/cambiarPassword/?token=" + token;
            //string url = "https://animated-panda-c5f730.netlify.app/cambiarPassword/?token=" + token;
            string url = "http://168.226.219.57:3004/cambiarPassword/?token=" + token;

            var from = new EmailAddress("satizenexcelenciadigital@gmail.com", "Satizen");
            var subject = "Recuperar Contraseña - Satizen"; //Este es el asunto del correo
            var to = new EmailAddress(emailDestino);
            var plainTextContent = "Recuperar contraseña - Satizen";
            var htmlContent = "<p>Correo para recuperar la contraseña de Satizen</p> </br> <a href='" + url + "'>Haz click aquí</a>"; //Este es el cuerpo del correo
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);

            var response = await client.SendEmailAsync(msg);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                Debug.WriteLine("Correo enviado correctamente");
            }
            else
            {
                Debug.WriteLine($"Error al enviar el correo: {response.StatusCode}");
            }
        }

    }
}

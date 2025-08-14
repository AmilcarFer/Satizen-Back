using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Extensions.Logging;

using System.Net;
using System.Threading.Tasks;
using System.Collections.Generic;

using Satizen_Api.Data;
using Satizen_Api.Models;
using Satizen_Api.Models.Dto.Usuarios;
using Microsoft.AspNetCore.Authorization;
using Satizen_Api.Custom;

namespace Satizen_Api.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class UsuariosController : ControllerBase
    {

        private readonly ILogger<UsuariosController> _logger;
        private readonly ApplicationDbContext _db;
        private readonly Utilidades _utilidades;
        protected ApiResponse _response;

        public UsuariosController(ILogger<UsuariosController> logger, ApplicationDbContext db, Utilidades utilidades)
        {
            _logger = logger;
            _db = db;
            _response = new();
            _utilidades = utilidades;
        }

        //--------------- EndPoint que trae la lista completa de usuarios -------------------
        [Authorize(Policy = "Admin")]
        [HttpGet]
        [Route("ListarUsuarios")]
        public async Task<ActionResult<ApiResponse>> GetUsuarios()
        {
            try
            {
                _logger.LogInformation("Obtener los usuarios"); // Esto solo muestra en consola que se ejecutó este endpoint

                _response.Resultado = await _db.Usuarios
                                              .Where(u => u.fechaEliminacion == null)
                                              .Include( r => r.Roles)
                                              .Select(r => new
                                              {
                                                  r.idUsuario,
                                                  r.nombreUsuario,
                                                  r.correo,
                                                  Roles = r.Roles.nombre,
                                                  r.fechaCreacion,
                                                  r.fechaActualizacion,
                                                  r.fechaEliminacion
                                              })
                                              .ToListAsync();
                _response.statusCode = HttpStatusCode.OK;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsExitoso = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
            }
            return _response;
        }

        //------------- EndPoint que trae un usuario a través de la id --------------
        [Authorize(Policy = "Admin")]
        [HttpGet]
        [Route("ListarPorId/{id}")]

        public async Task<ActionResult<ApiResponse>> GetUsuario(int id)
        {
            try
            {
                var usuario = await _db.Usuarios.FirstOrDefaultAsync(e => e.idUsuario == id && e.fechaEliminacion == null );

                if (id == 0)
                {
                    _response.statusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }

                if (usuario == null)
                {
                    _response.statusCode = HttpStatusCode.NotFound;
                    return NotFound(_response);
                }

                _response.Resultado = usuario;
                _response.statusCode = HttpStatusCode.OK;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsExitoso = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
            }
            return BadRequest(_response);
        }

        // -------------- EndPoint que actualiza un registro en la base de datos -----------
        [Authorize(Policy = "Admin")]
        [HttpPut]
        [Route("ActualizarUsuario/{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]

        public async Task<ActionResult<ApiResponse>> ActualizaUsuario(int id, [FromBody] UsuarioUpdateDto usuarioDto)
        {
            try
            {
                if (usuarioDto == null)
                {
                    _response.IsExitoso = false;
                    _response.statusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }

                var usuarioExistente = await _db.Usuarios.FirstOrDefaultAsync(e => e.idUsuario == id);

                if (usuarioExistente == null)
                {
                    _response.IsExitoso = false;
                    _response.statusCode = HttpStatusCode.NotFound;
                    _response.ErrorMessages = new List<string> { "El usuario no existe" };
                    return _response;
                }

                usuarioExistente.nombreUsuario = usuarioDto.nombreUsuario;
                usuarioExistente.correo = usuarioDto.correo;
                //usuarioExistente.password = _utilidades.encriptarSHA256(usuarioDto.password);
                usuarioExistente.idRoles = usuarioDto.idRoles;
                usuarioExistente.fechaActualizacion = DateTime.Now;

                _db.Usuarios.Update(usuarioExistente);
                await _db.SaveChangesAsync();

                _response.statusCode = HttpStatusCode.NoContent;
                return _response;
            }
            catch (Exception ex)
            {
                _response.IsExitoso = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
            }
            return _response;

        }



        //--------------- EndPoint que desactiva un registro en la base de datos ------------
        [Authorize(Policy = "Admin")]
        [HttpPatch]
        [Route("EliminarUsuario/{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DesactivarUsuario(int id)
        {
            if (id == 0)
            {
                return BadRequest();
            }

            var usuario = await _db.Usuarios.FirstOrDefaultAsync(v => v.idUsuario == id);

            if (usuario == null)
            {
                return NotFound();
            }

            // Desactivar el usuario estableciendo la fecha actual en estadoUsuario
            usuario.fechaEliminacion = DateTime.Now;

            _db.Usuarios.Update(usuario);
            await _db.SaveChangesAsync();

            _response.statusCode = HttpStatusCode.NoContent;
            return Ok(_response);
        }


        [HttpGet]
        [Route("ListarRoles")]
        public async Task<ActionResult<ApiResponse>> ListarRoles()
        {
            var roles = await _db.Roles.ToListAsync();
            return Ok(new ApiResponse
            {
                Resultado = roles,
                statusCode = HttpStatusCode.OK,
                IsExitoso = true
            });
        }

    }
}

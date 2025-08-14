using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

using Satizen_Api.Data;
using Satizen_Api.Hubs;
using Satizen_Api.Models;
using Satizen_Api.Models.Dto.Llamados;
using SatizenLlamados.Modelos.Dto;
using System.Net;

namespace Satizen_Api.Controllers
{
    //[Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class LlamadoController : ControllerBase
    {
        private readonly ILogger<LlamadoController> _logger;
        private readonly ApplicationDbContext _llamadoContext;
        protected ApiResponse _response;
        private IHubContext<AlertaHub> _hubContext;

        public LlamadoController(ILogger<LlamadoController> logger, ApplicationDbContext llamadoContext, IHubContext<AlertaHub> hubContext)
        {
            _logger = logger;
            _llamadoContext = llamadoContext;
            _response = new();
            _hubContext = hubContext;
        }


        //[Authorize(Policy = "AdminDoctor")]
        [HttpGet]
        [Route("ListarLlamados")]
        public async Task<ActionResult<ApiResponse>> GetLlamados()
        {
            try
            {
                _logger.LogInformation("Obtener los usuarios"); // Esto solo muestra en consola que se ejecutó este endpoint

                _response.Resultado = await _llamadoContext.Llamados
                                              .Where(u => u.fechaEliminacion == null)
                                              .Include(p => p.Pacientes)
                                              .Include(u => u.Personals)
                                              .Select(p => new
                                              {
                                                  p.idLlamado,
                                                  Pacientes = p.Pacientes.nombrePaciente,
                                                  Personals = p.Personals.nombrePersonal,
                                                  p.fechaHoraLlamado,
                                                  p.estadoLlamado,
                                                  p.prioridadLlamado,
                                                  p.observacionLlamado
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


        //[Authorize(Policy = "AdminDoctor")]
        [HttpGet]
        [Route("ListarPorID/{id:int} ")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse>> GetLlamado(int id)
        {
            try
            {
                var llamado = await _llamadoContext.Llamados.FindAsync(id);
                if (llamado == null)
                {
                    _response.IsExitoso = false;
                    _response.statusCode = HttpStatusCode.NotFound;
                    _response.ErrorMessages = new List<string> { "Llamado not found" };
                    return NotFound(_response);
                }

                _response.Resultado = llamado;
                _response.statusCode = HttpStatusCode.OK;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsExitoso = false;
                _response.ErrorMessages = new List<string> { ex.ToString() };
                _response.statusCode = HttpStatusCode.InternalServerError;
            }
            return StatusCode((int)_response.statusCode, _response);
        }



        //[Authorize(Policy = "AdminDoctor")]
        [HttpPost]
        [Route("AgregarLlamado")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse>> CrearLlamado([FromBody] LlamadoCreateDto createDto)
        {
            try
            {
                if (createDto == null)
                {
                    _response.IsExitoso = false;
                    _response.statusCode = HttpStatusCode.BadRequest;
                    _response.ErrorMessages = new List<string> { "Invalid input data" };
                    return BadRequest(_response);
                }

                var llamado = new Llamado
                {
                    idPaciente = createDto.idPaciente,
                    idPersonal = createDto.idPersonal,
                    fechaHoraLlamado = DateTime.Now,
                    estadoLlamado = createDto.estadoLlamado,
                    prioridadLlamado = createDto.prioridadLlamado,
                    observacionLlamado = createDto.observacionLlamado
                };

                await _llamadoContext.Llamados.AddAsync(llamado);
                await _llamadoContext.SaveChangesAsync();
                await _hubContext.Clients.All.SendAsync("nuevoLlamado");

                _response.Resultado = llamado;
                _response.statusCode = HttpStatusCode.Created;
                return StatusCode((int) _response.statusCode, _response) ;
            }
            catch (Exception ex)
            {
                _response.IsExitoso = false;
                _response.ErrorMessages = new List<string> { ex.ToString() };
                _response.statusCode = HttpStatusCode.InternalServerError;
                return StatusCode((int)_response.statusCode, _response);
            }
        }



        //[Authorize(Policy = "AdminDoctor")]
        [HttpPatch]
        [Route("EliminarLlamado/{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DesactivarUsuario(int id)
        {
            if (id == 0)
            {
                return BadRequest();
            }

            var llamado = await _llamadoContext.Llamados.FirstOrDefaultAsync(v => v.idLlamado == id);

            if (llamado == null)
            {
                return NotFound();
            }

            llamado.fechaEliminacion = DateTime.Now;

            _llamadoContext.Llamados.Update(llamado);
            await _llamadoContext.SaveChangesAsync();

            _response.statusCode = HttpStatusCode.NoContent;
            return Ok(_response);
        }



        //[Authorize(Policy = "AdminDoctor")]
        [HttpPut]
        [Route("ActualizarLlamado/{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse>> UpdateLlamado(int id, [FromBody] LlamadoUpdateDto updateDto)
        {
            try
            {
                if (updateDto == null)
                {
                    _response.IsExitoso = false;
                    _response.statusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }

                var llamadoExistente = await _llamadoContext.Llamados.FirstOrDefaultAsync(e => e.idLlamado == id);

                if (llamadoExistente == null)
                {
                    _response.IsExitoso = false;
                    _response.statusCode = HttpStatusCode.NotFound;
                    _response.ErrorMessages = new List<string> { "El llamado no existe" };
                }

                llamadoExistente.idPaciente = updateDto.idPaciente;
                llamadoExistente.idPersonal = updateDto.idPersonal;
                llamadoExistente.estadoLlamado = updateDto.estadoLlamado;
                llamadoExistente.prioridadLlamado = updateDto.prioridadLlamado;
                llamadoExistente.observacionLlamado = updateDto.observacionLlamado;

                _llamadoContext.Llamados.Update(llamadoExistente);
                await _llamadoContext.SaveChangesAsync();

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


        //[Authorize(Policy = "AdminDoctor")]
        [HttpPatch]
        [Route("AsignarLlamado/{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AsignarLlamado(int id, [FromBody] AsignarLlamadoDto asignarDto)
        {
            if (id == 0)
            {
                return BadRequest();
            }

            var llamado = await _llamadoContext.Llamados.FirstOrDefaultAsync(v => v.idLlamado == id);

            if (llamado == null)
            {
                return NotFound();
            }

            llamado.idPersonal = asignarDto.idPersonal;
            llamado.estadoLlamado = "Atendido";

            _llamadoContext.Llamados.Update(llamado);
            await _llamadoContext.SaveChangesAsync();

            _response.statusCode = HttpStatusCode.NoContent;
            return Ok(_response);
        }

        //[Authorize(Policy = "AdminDoctor")]
        [HttpGet]
        [Route("ListarLlamadosNoAsignados")]
        public async Task<ActionResult<ApiResponse>> GetLlamadosNoAsignados()
        {
            try
            {
                _logger.LogInformation("Obtener los usuarios"); // Esto solo muestra en consola que se ejecutó este endpoint

                _response.Resultado = await _llamadoContext.Llamados
                                              .Where(u => u.fechaEliminacion == null && u.idPersonal == null)
                                              .Include(p => p.Pacientes)
                                              .Include(u => u.Personals)
                                              .Select(p => new
                                              {
                                                  p.idLlamado,
                                                  Pacientes = p.Pacientes.nombrePaciente,
                                                  Personals = p.Personals.nombrePersonal,
                                                  p.fechaHoraLlamado,
                                                  p.estadoLlamado,
                                                  p.prioridadLlamado,
                                                  p.observacionLlamado
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

        [HttpGet]
        [Route("ListarPacientes")]
        public async Task<ActionResult<ApiResponse>> ListarPacientes()
        {
            var pacientes = await _llamadoContext.Pacientes
                                                            .Where(u => u.estadoPaciente == null)
                                                            .ToListAsync();
            return Ok(new ApiResponse
            {
                Resultado = pacientes,
                statusCode = HttpStatusCode.OK,
                IsExitoso = true
            });
        }

        [HttpGet]
        [Route("ListarPersonal")]
        public async Task<ActionResult<ApiResponse>> ListarPersonal()
        {
            var personals = await _llamadoContext.Personals
                                                            .Where(u => u.fechaEliminacion == null)
                                                            .ToListAsync();
            return Ok(new ApiResponse
            {
                Resultado = personals,
                statusCode = HttpStatusCode.OK,
                IsExitoso = true
            });
        }



    }


}




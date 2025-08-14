using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using System.Net;
using System.Threading.Tasks;
using System.Collections.Generic;

using Satizen_Api.Data;
using Satizen_Api.Models;
using Satizen_Api.Models.Dto.Asignaciones;


namespace Satizen_Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AsignacionesController : ControllerBase
    {
        private readonly ApplicationDbContext _applicationDbContext;
        protected ApiResponse _response;

        public AsignacionesController(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
            _response = new ApiResponse();
        }

        //--------------- EndPoint que trae la lista completa de asignaciones -------------------
        [Authorize(Policy = "Admin")]
        [HttpGet]
        [Route("listarAsignaciones")]
        public async Task<ActionResult<ApiResponse>> GetAsignaciones()
        {
            try
            {
                var asignaciones = await _applicationDbContext.Asignaciones
                                          .Where(a => a.fechaEliminacion == null)
                                          .Include(a => a.Turnos)  // Incluyendo el turno
                                          .ToListAsync();

                _response.Resultado = asignaciones;
                _response.statusCode = HttpStatusCode.OK;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsExitoso = false;
                _response.ErrorMessages = new List<string> { ex.ToString() };
                return BadRequest(_response);
            }
        }

        //------------- EndPoint que trae una asignacion a través de la id --------------
        [Authorize(Policy = "Admin")]
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse>> GetAsignacion(int id)
        {
            try
            {
                var asignacion = await _applicationDbContext.Asignaciones.FirstOrDefaultAsync(a => a.idAsignacion == id);
               
                
                if (id == 0)
                {
                    _response.statusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }

                if (asignacion == null)
                {
                    _response.statusCode = HttpStatusCode.NotFound;
                    return NotFound(_response);
                }

                _response.Resultado = asignacion;
                _response.statusCode = HttpStatusCode.OK;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsExitoso = false;
                _response.ErrorMessages = new List<string> { ex.ToString() };
                return BadRequest(_response);
            }
        }

        //--------------- EndPoint que crea una nueva asignacion ------------
        [Authorize(Policy = "Admin")]
        [HttpPost]
        [Route("AgregarAsignacion")]
        public async Task<ActionResult<ApiResponse>> PostAsignacion([FromBody] CrearAsigcionDto crearAsignacionDto)
        {
            try
            {
                var asignacion = new Asignacion
                {
                    idPersonal = crearAsignacionDto.idPersonal,
                    idSector = crearAsignacionDto.idSector,
                    diaSemana = crearAsignacionDto.diaSemana,
                    idTurno = crearAsignacionDto.idTurno,
                    horaInicio = crearAsignacionDto.horaInicio,
                    horaFinalizacion = crearAsignacionDto.horaFinalizacion
                };

                _applicationDbContext.Asignaciones.Add(asignacion);
                await _applicationDbContext.SaveChangesAsync();

                _response.Resultado = asignacion;
                _response.statusCode = HttpStatusCode.Created;
                return CreatedAtAction("GetAsignacion", new { id = asignacion.idAsignacion }, _response);
            }
            catch (Exception ex)
            {
                _response.IsExitoso = false;
                _response.ErrorMessages = new List<string> { ex.ToString() };
                return BadRequest(_response);
            }
        }

        //--------------- EndPoint que actualiza una asignacion existente ------------
        [Authorize(Policy = "Admin")]
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse>> PutAsignacion(int id, [FromBody] ActualizarAsignacionDto actualizarAsignacionDto)
        {
            try
            {
                var asignacion = await _applicationDbContext.Asignaciones.FindAsync(id);
                if (asignacion == null)
                {
                    _response.statusCode = HttpStatusCode.NotFound;
                    return NotFound(_response);
                }

                asignacion.idPersonal = actualizarAsignacionDto.idPersonal;
                asignacion.idSector = actualizarAsignacionDto.idSector;
                asignacion.diaSemana = actualizarAsignacionDto.diaSemana;
                asignacion.idTurno = actualizarAsignacionDto.idTurno;
                asignacion.horaInicio = actualizarAsignacionDto.horaInicio;
                asignacion.horaFinalizacion = actualizarAsignacionDto.horaFinalizacion;

                _applicationDbContext.Entry(asignacion).State = EntityState.Modified;
                await _applicationDbContext.SaveChangesAsync();

                _response.statusCode = HttpStatusCode.NoContent;
                return Ok(_response);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AsignacionExists(id))
                {
                    _response.statusCode = HttpStatusCode.NotFound;
                    return NotFound(_response);
                }
                else
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                _response.IsExitoso = false;
                _response.ErrorMessages = new List<string> { ex.ToString() };
                return BadRequest(_response);
            }
        }

        [Authorize(Policy = "Admin")]
        [HttpPatch]
        [Route("EliminarAsignacion/{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> EliminarAsignacion(int id)
        {
            if (id == 0)
            {
                return BadRequest();
            }

            var asignacion = await _applicationDbContext.Asignaciones.FirstOrDefaultAsync(v => v.idAsignacion == id);

            if (asignacion == null)
            {
                return NotFound();
            }

            asignacion.fechaEliminacion = DateTime.Now;

            _applicationDbContext.Asignaciones.Update(asignacion);
            await _applicationDbContext.SaveChangesAsync();

            return NoContent();
        }

        private bool AsignacionExists(int id)
        {
            return _applicationDbContext.Asignaciones.Any(e => e.idAsignacion == id);
        }
    }
}

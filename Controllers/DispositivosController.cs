using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Satizen_Api.Data;
using Satizen_Api.Models.Dto.Dispositivos;
using Satizen_Api.Models;

using System.Net;
using Microsoft.AspNetCore.Authorization;

namespace Satizen_Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class DispositivosController : ControllerBase
    {

        private readonly ApplicationDbContext _db;
        protected ApiResponse _response;

        public DispositivosController(ApplicationDbContext db)
        {
            _db = db;
            _response = new();
        }

        //--------------- EndPoint que trae la lista completa de sectores -------------------
        [Authorize(Policy = "Admin")]
        [HttpGet]
        [Route("ListarDispositivos")]
        public async Task<ActionResult<ApiResponse>> GetDispositivos()
        {
            try
            {
                _response.Resultado = await _db.DispositivosLaborales
                                                .Where(d => d.fechaEliminacion == null)
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


        //------------- EndPoint que trae un sector a través de la id --------------
        [Authorize(Policy = "Admin")]
        [HttpGet]
        [Route("ListarPorId/{id}")]

        public async Task<ActionResult<ApiResponse>> GetSector(int id)
        {
            try
            {
                var dispositivo = await _db.DispositivosLaborales.FirstOrDefaultAsync(e => e.idDispositivo == id && e.fechaEliminacion == null);

                if (id == 0)
                {
                    _response.statusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }

                if (dispositivo == null)
                {
                    _response.statusCode = HttpStatusCode.NotFound;
                    return NotFound(_response);
                }

                _response.Resultado = dispositivo;
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

        //------------------------ EndPoint que crea nuevos doctores -------------------------
        [Authorize(Policy = "Admin")]
        [HttpPost]
        [Route("CrearDispositivo")]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse>> CrearSector([FromBody] DispositivoCreateDto dispositivoDto)
        {
            try
            {
                if (dispositivoDto == null)
                {
                    return BadRequest(dispositivoDto);
                }

                DispositivoLaboral modelo = new()
                {
                    idPersonal = dispositivoDto.idPersonal,
                    numDispositivo = dispositivoDto.numDispositivo,
                    observacionDispositivo = dispositivoDto.observacionDispositivo,
                    estadoDispositivo = dispositivoDto.estadoDispositivo,
                    fechaCreacion = DateTime.Now
                };

                await _db.DispositivosLaborales.AddAsync(modelo);
                await _db.SaveChangesAsync();
                _response.Resultado = modelo;
                _response.statusCode = HttpStatusCode.Created;

                return (_response);
            }
            catch (Exception ex)
            {
                _response.IsExitoso = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };

            }
            return _response;
        }


        //--------------- EndPoint que actualiza un registro en la base de datos ------------
        [Authorize(Policy = "Admin")]
        [HttpPut]
        [Route("ActualizarDispositivo/{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]

        public async Task<ActionResult<ApiResponse>> ActualizarDispositivo(int id, [FromBody] DispositivoUpdateDto dispositivoDto)
        {
            try
            {
                if (dispositivoDto == null)
                {
                    _response.IsExitoso = false;
                    _response.statusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }

                var dispositivoExistente = await _db.DispositivosLaborales.FirstOrDefaultAsync(e => e.idDispositivo == id);

                if (dispositivoExistente == null)
                {
                    _response.IsExitoso = false;
                    _response.statusCode = HttpStatusCode.NotFound;
                    _response.ErrorMessages = new List<string> { "El dispositivo no existe" };
                    return _response;
                }

                dispositivoExistente.idPersonal = dispositivoDto.idPersonal;
                dispositivoExistente.numDispositivo = dispositivoDto.numDispositivo;
                dispositivoExistente.observacionDispositivo = dispositivoDto.observacionDispositivo;
                dispositivoExistente.estadoDispositivo = dispositivoDto.estadoDispositivo;
                dispositivoExistente.fechaActualizacion = DateTime.Now;

                _db.DispositivosLaborales.Update(dispositivoExistente);
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


        //--------------- EndPoint que elimina (desactiva) un dispositivo en la base de datos ------------
        [Authorize(Policy = "Admin")]
        [HttpPatch]
        [Route("EliminarDispositivo/{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse>> DesactivarDispositivo(int id)
        {
            try
            {
                if (id == 0)
                {
                    return BadRequest();
                }

                var dispositivo = await _db.DispositivosLaborales.FirstOrDefaultAsync(v => v.idDispositivo == id);

                if (dispositivo == null)
                {
                    return NotFound();
                }

                // Desactivar el usuario estableciendo la fecha actual en fechaEliminacion
                dispositivo.fechaEliminacion = DateTime.Now;

                _db.DispositivosLaborales.Update(dispositivo);
                await _db.SaveChangesAsync();

                _response.statusCode = HttpStatusCode.NoContent;

                return (_response);
            }
            catch (Exception ex)
            {
                _response.IsExitoso = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };

            }
            return _response;
        }
    }
}

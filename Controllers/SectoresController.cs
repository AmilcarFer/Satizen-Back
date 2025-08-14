using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

using System.Net;
using System.Numerics;

using Satizen_Api.Data;
using Satizen_Api.Models;
using Satizen_Api.Models.Dto.Sectores;

namespace Satizen_Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class SectoresController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        protected ApiResponse _response;

        public SectoresController(ApplicationDbContext db)
        {
            _db = db;
            _response = new();
        }

        //--------------- EndPoint que trae la lista completa de sectores -------------------
        [Authorize(Policy = "Admin")]
        [HttpGet]
        [Route("ListarSectores")]
        public async Task<ActionResult<ApiResponse>> GetSectores()
        {
            try
            {

                _response.Resultado = await _db.Sectores
                                                .Where(d => d.fechaEliminacion == null)
                                                .Select(d => new
                                                {
                                                    d.idSector,
                                                    d.idInstitucion,
                                                    d.nombreSector,
                                                    d.descripcionSector,
                                                    d.fechaCreacion,
                                                    d.fechaActualizacion,
                                                    d.fechaEliminacion
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


        //------------- EndPoint que trae un sector a través de la id --------------
        [Authorize(Policy = "Admin")]
        [HttpGet]
        [Route("ListarPorId/{id}")]

        public async Task<ActionResult<ApiResponse>> GetSector(int id)
        {
            try
            {
                var sector = await _db.Sectores.FirstOrDefaultAsync(e => e.idSector == id && e.fechaEliminacion == null);

                if (id == 0)
                {
                    _response.statusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }

                if (sector == null)
                {
                    _response.statusCode = HttpStatusCode.NotFound;
                    return NotFound(_response);
                }

                _response.Resultado = sector;
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
        [Route("CrearSector")]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse>> CrearSector([FromBody] SectoresCreateDto sectorDto)
        {
            try
            {
                if (sectorDto == null)
                {
                    return BadRequest(sectorDto);
                }

                Sector modelo = new()
                {
                    idInstitucion = sectorDto.idInstitucion,
                    nombreSector = sectorDto.nombreSector,
                    descripcionSector = sectorDto.descripcionSector,
                    fechaCreacion = DateTime.Now
                };

                await _db.Sectores.AddAsync(modelo);
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
        [Route("ActualizarSector/{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]

        public async Task<ActionResult<ApiResponse>> ActualizaSector(int id, [FromBody] SectoresUpdateDto sectorDto)
        {
            try
            {
                if (sectorDto == null)
                {
                    _response.IsExitoso = false;
                    _response.statusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }

                var sectorExistente = await _db.Sectores.FirstOrDefaultAsync(e => e.idSector == id);

                if (sectorExistente == null)
                {
                    _response.IsExitoso = false;
                    _response.statusCode = HttpStatusCode.NotFound;
                    _response.ErrorMessages = new List<string> { "El sector no existe" };
                    return _response;
                }

                sectorExistente.idInstitucion = sectorDto.idInstitucion;
                sectorExistente.nombreSector = sectorDto.nombreSector;
                sectorExistente.descripcionSector = sectorDto.descripcionSector;
                sectorExistente.fechaActualizacion = DateTime.Now;

                _db.Sectores.Update(sectorExistente);
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


        //--------------- EndPoint que elimina (desactiva) un sector en la base de datos ------------
        [Authorize(Policy = "Admin")]
        [HttpPatch]
        [Route("EliminarSector/{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse>> DesactivarSector(int id)
        {
            try
            {
                if (id == 0)
                {
                    return BadRequest();
                }

                var sector = await _db.Sectores.FirstOrDefaultAsync(v => v.idSector == id);

                if (sector == null)
                {
                    return NotFound();
                }

                // Desactivar el usuario estableciendo la fecha actual en fechaEliminacion
                sector.fechaEliminacion = DateTime.Now;

                _db.Sectores.Update(sector);
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

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
using Satizen_Api.Models.Dto.Institucion;
using Microsoft.AspNetCore.Authorization;

namespace Proyec_Satizen_Api.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class InstitucionController : ControllerBase
    {
        private readonly ILogger<InstitucionController> _logger;
        private readonly ApplicationDbContext _db;
        protected ApiResponse _response;

        public InstitucionController(ILogger<InstitucionController> logger, ApplicationDbContext db)
        {
            _logger = logger;
            _db = db;
            _response = new ApiResponse();
        }


        [Authorize(Policy = "Admin")]
        [HttpGet]
        [Route("ListarInstituciones")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse>> GetInstituciones()
        {
            try
            {
                _logger.LogInformation("Obtener las Instituciones");

                _response.Resultado = await _db.Instituciones
                                              .Where(u => u.eliminado == null)
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

        [Authorize(Policy = "Admin")]
        [HttpGet]
        [Route("ListarPorId/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<InstitucionDto>> GetInstitucion(int id)
        {
            var institucion = _db.Instituciones.FirstOrDefault(c => c.idInstitucion == id);

            if (institucion == null)
            {
                return NotFound();
            }
            return Ok(institucion);
        }

        [Authorize(Policy = "Admin")]
        [HttpPost]
        [Route("CrearInstitucion")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse>> CrearInstitucion([FromBody] InstitucionCreateDto institucioncreateDto)
        {
            try
            {
                if (institucioncreateDto == null)
                {
                    return BadRequest(institucioncreateDto);
                }

                Institucion modelo = new()
                {
                    nombreInstitucion = institucioncreateDto.nombreInstitucion,
                    direccionInstitucion = institucioncreateDto.direccionInstitucion,
                    telefonoInstitucion = institucioncreateDto.telefonoInstitucion,
                    correoInstitucion = institucioncreateDto.correoInstitucion,
                    celularInstitucion = institucioncreateDto.celularInstitucion,

                };

                await _db.Instituciones.AddAsync(modelo);
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

        [Authorize(Policy = "Admin")]
        [HttpPut]
        [Route("ActualizarInstitucion/{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse>> ActualizarInstitucion(int id, [FromBody] InstitucionUpdateDto institucionDto)
        {
            try
            {
                if(institucionDto == null)
                {
                    _response.IsExitoso = false;
                    _response.statusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }

                var institucionExistente = await _db.Instituciones.FirstOrDefaultAsync(e => e.idInstitucion == id);

                if(institucionExistente == null)
                {
                    _response.IsExitoso = false;
                    _response.statusCode = HttpStatusCode.NotFound;
                    _response.ErrorMessages = new List<string> { "La institución no existe" };
                    return _response;
                }

                institucionExistente.nombreInstitucion = institucionDto.nombreInstitucion;
                institucionExistente.direccionInstitucion = institucionDto.direccionInstitucion;
                institucionExistente.telefonoInstitucion = institucionDto.telefonoInstitucion;
                institucionExistente.correoInstitucion = institucionDto.correoInstitucion;
                institucionExistente.celularInstitucion = institucionDto.celularInstitucion;

                _db.Instituciones.Update(institucionExistente);
                await _db.SaveChangesAsync();

                _response.statusCode = HttpStatusCode.NoContent;
                return _response;
            }
            catch(Exception ex)
            {
                _response.IsExitoso = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
            }
            return _response;
        }


        [Authorize(Policy = "Admin")]
        [HttpPatch]
        [Route("EliminarInstitucion/{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> EliminarInstitucion(int id)
        {
            if (id == 0)
            {
                return BadRequest();
            }

            var institucion = await _db.Instituciones.FirstOrDefaultAsync(v => v.idInstitucion == id);

            if (institucion == null)
            {
                return NotFound();
            }

            institucion.eliminado = DateTime.Now;

            _db.Instituciones.Update(institucion);
            await _db.SaveChangesAsync();

            return NoContent();
        }

    }
}

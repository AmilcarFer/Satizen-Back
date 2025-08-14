using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Satizen_Api.Data;
using Satizen_Api.Models;
using Satizen_Api.Models.Dto.ContactoPaciente;

using System.Net;

namespace Satizen_Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ContactoPacienteController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<ContactoPacienteController> _logger;
        private readonly ApiResponse _response;

        public ContactoPacienteController(ApplicationDbContext dbContext, ILogger<ContactoPacienteController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
            _response = new ApiResponse();
        }

        [Authorize(Policy = "AdminDoctorEnfermero")]
        [HttpGet]
        [Route("ListarContactos")]
        public async Task<ActionResult<ApiResponse>> GetContactos()
        {
            try
            {
                _logger.LogInformation("Obtener los Contactos");

                _response.Resultado = await _dbContext.Contactos
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

        [Authorize(Policy = "AdminDoctorEnfermero")]
        [HttpGet("{id}", Name = "GetContacto")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ContactoDto>> GetContacto(int id)
        {
            var contacto = await _dbContext.Contactos.FirstOrDefaultAsync(c => c.idContacto == id);
            if (contacto == null)
            {
                return NotFound();
            }
            return Ok(contacto);
        }

        [Authorize(Policy = "AdminDoctorEnfermero")]
        [HttpPost]
        [Route("CrearContacto")]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse>> PostContactos(CreateContactoPacienteDto contactoDto)
        {
            try
            {
                var contacto = new Contacto
                {
                    idPaciente = contactoDto.idPaciente,
                    celularPaciente = contactoDto.celularPaciente,
                    celularAcompananteP = contactoDto.celularAcompananteP,
                    FechaInicioValidez = contactoDto.FechaInicioValidez,
                    estadoContacto = contactoDto.estadoContacto
                };

                await _dbContext.Contactos.AddAsync(contacto);
                await _dbContext.SaveChangesAsync();

                _response.Resultado = contacto;
                _response.statusCode = HttpStatusCode.Created;
                return CreatedAtAction("GetContacto", new { id = contacto.idContacto }, _response);
            }
            catch (Exception ex)
            {
                _response.IsExitoso = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
                return BadRequest(_response);
            }
        }

        [Authorize(Policy = "AdminDoctorEnfermero")]
        [HttpPatch]
        [Route("EliminarUsuario/{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DesactivarContacto(int id)
        {
            if (id == 0)
            {
                return BadRequest();
            }

            var contacto = await _dbContext.Contactos.FirstOrDefaultAsync(v => v.idContacto == id);

            if (contacto == null)
            {
                return NotFound();
            }

            contacto.eliminado = DateTime.Now;

            _dbContext.Contactos.Update(contacto);
            await _dbContext.SaveChangesAsync();

            return NoContent();
        }

        [Authorize(Policy = "AdminDoctorEnfermero")]
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse>> UpdateContacto(int id, [FromBody] UpdateContactoPacienteDto contactoDto)
        {
            try
            {
                if (contactoDto == null)
                {
                    _response.IsExitoso = false;
                    _response.statusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }

                var contacto = await _dbContext.Contactos.FirstOrDefaultAsync(v => v.idContacto == id);

                if (contacto == null)
                {
                    _response.IsExitoso = false;
                    _response.statusCode = HttpStatusCode.NotFound;
                    _response.ErrorMessages = new List<string> { "El contacto no existe" };
                }

                contacto.idPaciente = contactoDto.idPaciente;
                contacto.celularPaciente = contactoDto.celularPaciente;
                contacto.celularAcompananteP = contactoDto.celularAcompananteP;
                contacto.estadoContacto = contactoDto.estadoContacto;

                _dbContext.Contactos.Update(contacto);
                await _dbContext.SaveChangesAsync();

                return NoContent();
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

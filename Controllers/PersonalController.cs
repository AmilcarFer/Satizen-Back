using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Satizen_Api.Data;
using Satizen_Api.Models;
using Satizen_Api.Models.Dto.Personal;

using System.Net;

namespace Satizen_Api.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class PersonalController : ControllerBase
    {
        private readonly ApplicationDbContext _applicationDbContext;
        protected ApiResponse _response;

        public PersonalController(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
            _response = new();
        }

        //--------------- EndPoint que trae la lista completa de personal -------------------
        [Authorize(Policy = "Admin")]
        [HttpGet]
        [Route("ListarPersonal")]
        public async Task<ActionResult<ApiResponse>> GetPersonal()
        {
            try
            {
                _response.Resultado = await _applicationDbContext.Personals
                                              .Where(u => u.fechaEliminacion == null)
                                              .Include(i => i.Instituciones)
                                              .Include(u => u.Usuarios)
                                              .Select(i => new
                                              {
                                                  i.idPersonal,
                                                  Instituciones = i.Instituciones.nombreInstitucion,
                                                  Usuarios = i.Usuarios.nombreUsuario,
                                                  i.nombrePersonal,
                                                  i.celularPersonal,
                                                  i.telefonoPersonal,
                                                  i.correoPersonal
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

        [Authorize(Policy = "Admin")]
        [HttpGet]
        [Route("ListarPorId/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PersonalDto>> GetPersonal(int id)
        {
            var institucion = _applicationDbContext.Personals.FirstOrDefault(c => c.idPersonal == id);

            if (institucion == null)
            {
                return NotFound();
            }
            return Ok(institucion);
        }

        [Authorize(Policy = "Admin")]
        [HttpPost]
        [Route("CrearPersonal")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(Personal))]
        public async Task<ActionResult<ApiResponse>> PostPersonals(AddPersonalDto Personal)
        {
            try
            {
                if (Personal == null)
                {
                    return BadRequest(Personal);
                }


                Personal modelo = new()
                {
                    idInstitucion = Personal.idInstitucion,
                    idUsuario = Personal.idUsuario,
                    nombrePersonal = Personal.nombrePersonal,
                    //rolPersonal = Personal.rolPersonal,
                    celularPersonal = Personal.celularPersonal,
                    telefonoPersonal = Personal.telefonoPersonal,
                    correoPersonal = Personal.correoPersonal

                };

                await _applicationDbContext.Personals.AddAsync(modelo);
                await _applicationDbContext.SaveChangesAsync();
                _response.Resultado = modelo;
                _response.statusCode = HttpStatusCode.Created;

                return _response;
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
        [Route("ActualizarPersonal/{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdatePersonal(int id, [FromBody] UpdatePersonalDto personalupdatedto)
        {
            if (personalupdatedto == null || id != personalupdatedto.idPersonal)
            {
                return BadRequest();
            }

            var personal = await _applicationDbContext.Personals.FirstOrDefaultAsync(v => v.idPersonal == id);

            if (personal == null)
            {
                return NotFound();
            }

            personal.idInstitucion = personalupdatedto.idInstitucion;
            personal.idUsuario = personalupdatedto.idUsuario;
            personal.nombrePersonal = personalupdatedto.nombrePersonal;
            //personal.rolPersonal = personalupdatedto.rolPersonal;
            personal.celularPersonal = personalupdatedto.celularPersonal;
            personal.telefonoPersonal = personalupdatedto.telefonoPersonal;
            personal.correoPersonal = personalupdatedto.correoPersonal;

            
            _applicationDbContext.SaveChanges();

            return NoContent();
        }

        [Authorize(Policy = "Admin")]
        [HttpPatch]
        [Route("EliminarPersonal/{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> EliminarInstitucion(int id)
        {
            if (id == 0)
            {
                return BadRequest();
            }

            var personal = await _applicationDbContext.Personals.FirstOrDefaultAsync(v => v.idPersonal == id);

            if (personal == null)
            {
                return NotFound();
            }

            personal.fechaEliminacion = DateTime.Now;

            _applicationDbContext.Personals.Update(personal);
            await _applicationDbContext.SaveChangesAsync();

            return NoContent();
        }

        //------- EndPoints para listar -------
        [HttpGet]
        [Route("ListarInstituciones")]
        public async Task<ActionResult<ApiResponse>> ListarInstituciones()
        {
            var instituciones = await _applicationDbContext.Instituciones.ToListAsync();
            return Ok(new ApiResponse
            {
                Resultado = instituciones,
                statusCode = HttpStatusCode.OK,
                IsExitoso = true
            });
        }

        [HttpGet]
        [Route("ListarUsuarios")]
        public async Task<ActionResult<ApiResponse>> ListarUsuarios()
        {
            var usuarios = await _applicationDbContext.Usuarios.ToListAsync();
            return Ok(new ApiResponse
            {
                Resultado = usuarios,
                statusCode = HttpStatusCode.OK,
                IsExitoso = true
            });
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Satizen_Api.Models.Dto.ContactoPaciente 
{ 
    public class UpdateContactoPacienteDto
    {
        public int idContacto { get; set; }
        public int idPaciente { get; set; }
        public long celularPaciente { get; set; }
        public long celularAcompananteP { get; set; }
        public string estadoContacto { get; set; }
    }
}

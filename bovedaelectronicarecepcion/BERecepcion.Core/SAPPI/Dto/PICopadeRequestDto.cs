using BERecepcion.Core.Admin.Dto;
using BERecepcion.Core.Consulta.Copades.Dto;
using BERecepcion.Core.Correos.Dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace BERecepcion.Core.SAPPI.Dto
{
    public class PICopadeRequestDto
    {
        public string Clave { get; set; }
        public string SapOrder { get; set; }
        public string Reception { get; set; }
        public string Exercise { get; set; }
    }

    public class DtoPdfPreviewData
    {
        public PICopadeRequestDto data { get; set; }
        public IEnumerable<string> result { get; set; }
        public string UsuarioModificador { get; set; }
    }

    public class DtoPdfEmail
    {
        public PICopadeRequestDto data { get; set; }
        public string subject { get; set; }
        public string result { get; set; }
        public string UsuarioModificador { get; set; }
    }

    public class DtoPdfFilesEmail
    {
        public IEnumerable<string> archivos { get; set; }
        public IEnumerable<UsersDto> users { get; set; }
        public IEnumerable<UsersDto> representatives { get; set; }
        public CopadeDto copade { get; set; }
        public AnaliticoPagoDto analitico { get; set; }
        public CorreoDto correoMensaje { get; set; }
    }
}

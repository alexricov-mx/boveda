using System;
using System.Collections.Generic;
using System.Text;

namespace BERecepcion.Core.SAPPI.Dto
{
    public class OSResponseDto
    {
        //{
        //  item : {
        //  \"ORGANISMO\":\"PEP\",
        //  \"TIPO_DOCUMENTO\":\"S\",
        //  \"CONTRATO\":\"6410078012\",
        //  \"ORDEN_SAP\":\"4304025174\"}
        //}
        public OSResponseItemDto item { get; set; }
    }

    public class CopadeResponseDto
    {
        public CopadeResponseItemDto item { get; set; }
    }
}

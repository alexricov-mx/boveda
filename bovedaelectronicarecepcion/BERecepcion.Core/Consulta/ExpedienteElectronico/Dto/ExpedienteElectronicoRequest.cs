using System;

namespace BERecepcion.Core.Consulta.ExpedienteElectronico.Dto;

public class ExpedienteElectronicoRequest
{
    public string? SAPOrder { get; set; }
    public Guid? CopadeID { get; set; }
    public Guid? AnaliticoPagoID { get; set; }
}

using System;

namespace BERecepcion.Core.Estimaciones.Dtos;

public class SOEstimationInternoDto
{
    public Guid EstimacionId { get; set; }
    public Guid OrganismId { get; set; }
    public string Contract { get; set; } = default!;
    public string SapOrder { get; set; } = default!;
    public string DocumentType { get; set; } = default!;
    public string Type { get; set; } = default!;
    public string CreditorNumber { get; set; } = default!;
    public string Total { get; set; } = default!;
    public string Currency { get; set; } = default!;
    public string Signer { get; set; } = default!;
    public DateTime? FunctionarySignDate { get; set; }
    public string FunctionaryEmail { get; set; } = default!;
    public DateTime? FunctionaryEmailSendDate { get; set; }
    public DateTime? ProviderSignDate { get; set; }
    public string ProviderEmail { get; set; } = default!;
    public DateTime? ProviderEmailSendDate { get; set; }
    public string OrganismName { get; set; } = default!;
    public string OrganismClave { get; set; } = default!;
}

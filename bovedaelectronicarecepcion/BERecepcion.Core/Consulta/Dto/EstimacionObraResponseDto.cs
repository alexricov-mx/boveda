using System;
using System.Collections.Generic;
using System.Text;

namespace BERecepcion.Core.Consulta.Dto;

public class EstimacionObraResponseDto
{
    public Guid EstimacionID { get; set; }
    public Guid OrganismID { get; set; }
    public string Contract { get; set; }
    public string DocumentType { get; set; }
    public string SAPOrder { get; set; }
    public string Type { get; set; }
    public string CreditorNumber { get; set; }
    public decimal? Total { get; set; }
    public string Currency { get; set; }
    public string Representative { get; set; }
    public string Signer { get; set; }
    public bool? EsTRI { get; set; }
    public DateTime? ReceptionDate { get; set; }
    public string FunctionaryEmail { get; set; }
    public DateTime? FunctionaryEmailSendDate { get; set; }
    public DateTime? FunctionarySignDate { get; set; }
    public DateTime? FunctionaryNotifyPemexDate { get; set; }
    public string ProviderEmail { get; set; }
    public DateTime? ProviderEmailSendDate { get; set; }
    public DateTime? ProviderSignDate { get; set; }
    public DateTime? ProviderNotifyPemexDate { get; set; }
    public bool? IsFullSigned { get; set; }
    public bool? IsCancel { get; set; }
    public DateTime? CancelDate { get; set; }
    public string CancelBy { get; set; }
    public string FunctionarySignerName { get; set; }
    public string ProviderSignerName { get; set; }
    public string FunctionaryCancelName { get; set; }
    public string Clave { get; set; }
    public string OrganismName { get; set; }
    public string OrganismClave { get; set; }
}

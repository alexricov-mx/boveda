using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BERRecepcion.Front.Models.Dto
{
    public class SOEstimationDto
    {
        public Guid EstimacionID { get; set; }
        public Guid OrganismID { get; set; }
        public string Contract { get; set; }
        public string SapOrder { get; set; }
        public string Type { get; set; }
        public string DocumentType { get; set; }
        public string Signer { get; set; }
        public string CreditorNumber { get; set; }
        public string Currency { get; set; }
        public bool EsTRI { get; set; }
        public DateTime ReceptionDate { get; set; }
        public string Representative { get; set; }
        public string Total { get; set; }
        public string FunctionaryEmailReason { get; set; }
        public string FunctionaryEmails { get; set; }
        public DateTime? FunctionaryEmailSendDate { get; set; }
        public DateTime FunctionaryNotifyPemexDate { get; set; }
        public DateTime? FunctionarySignDate { get; set; }
        public string ProviderEmailReason { get; set; }
        public string ProviderEmails { get; set; }
        public DateTime ProviderEmailSendDate { get; set; }
        public DateTime ProviderNotifyPemexDate { get; set; }
        public DateTime? ProviderSignDate { get; set; }
        public bool IsFullSigned { get; set; }
        public bool IsCancel { get; set; }
        public DateTime CancelDate { get; set; }
        public string CancelBy { get; set; }

        public virtual string OrganismName { get; set; }
        public virtual string OrganismClave { get; set; }
        public virtual string FunctionarySignerName { get; set; }
        public virtual string ProviderSignerName { get; set; }
        public virtual string FunctionaryCancelName { get; set; }
        public virtual IEnumerable<DateHelperModel> Seguimiento { get; set; }
    }
}

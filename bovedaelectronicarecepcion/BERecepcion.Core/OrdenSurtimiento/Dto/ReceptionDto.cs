using System;
using System.Collections.Generic;
using System.Text;

namespace BERecepcion.Core.OrdenSurtimiento.Dto
{
    public class ReceptionDto
    {
        public Guid ReceptionID { get; set; }
        public Guid OrganismID { get; set; }
        public string OrganismClave { get; set; }
        public string Contract { get; set; }
        public string SapOrder { get; set; }
        public string SiafOrder { get; set; }
        public string Reception { get; set; }
        public DateTime ReceptionDate { get; set; }
        public string Exercise { get; set; }
        public string IvaAmount { get; set; }
        public string AmountWithIva { get; set; }
        public string AmountWithoutIva { get; set; }
        public string BriefText { get; set; }
        public string Currency { get; set; }
        public string MedicalUnit { get; set; }
        public string NoExternal { get; set; }
        public string Penalty { get; set; }
        public string Provider { get; set; }
        public string ReceptionDateContab { get; set; }
        public string ReceptionDateRegistry { get; set; }
        public string Signer { get; set; }
        public string Total { get; set; }
        public string MadeBy { get; set; }
        public string OrganismName { get; set; }
        public string Clave { get; set; }
        public bool EmailFunctionary { get; set; }
        public string EmailFunctionaryReason { get; set; }
        public DateTime EmailFunctionarySendDate { get; set; }
        public bool Functionary { get; set; }
        public DateTime FunctionarySignDate { get; set; }
        public bool NotifyPemex { get; set; }
        public DateTime NotifyPemexDate { get; set; }
        public string ProviderEmail { get; set; }
        public bool EsTRI { get; set; }
        public bool Historical { get; set; }
        public IEnumerable<RecepcionDetail> Detail { get; set; }
    }

    public class RecepcionDetail
    {
        public string Cantidad { get; set; }
        public string CorpPrecioUnitario { get; set; }
        public string Descripcion { get; set; }
        public string Importe { get; set; }
        public string Iva { get; set; }
        public string Partida { get; set; }
    }
}

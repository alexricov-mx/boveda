using BERecepcion.Core.Admin.Dto;
using BERecepcion.Core.Consulta.Copades.Dto;
using BERecepcion.Core.Correos.Dto;
using BERecepcion.Core.Dto;
using BERecepcion.Core.Facturas.Dto;
using BERecepcion.Core.Instrucciones.Dto;
using BERecepcion.Core.OrdenSurtimiento.Dto;
using BERecepcion.Core.SAPPI.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BERecepcion.Core.Correos.Interfaces.Repositories
{
    public interface ICorreoRepository
    {
        Task<DataResult<string>> EnvioCorreoAsync(CorreoDto correoMensaje);
        Task<DataResult<string>> EnvioCorreoUserAsync(CorreoDto correoMensaje);
        Task<bool> NotificacionOSAsync(IEnumerable<UsersDto> users, SupplyOrderDto supplyOrder, string subject);

        #region
        //Task<DataResult<string>> NotificacionCopadeEmailAsync(DtoPdfFilesEmail dtoPdfFilesEmail);
        Task<DataResult<NotificacionFacturaEmailDto>> NotificacionFacturaEmailAsync(string reception, IEnumerable<ValidationError> validationErrors, UsersDto user, bool logProcess, string repositoryName, string methodName);
        Task<DataResult<NotificacionFacturaEmailDto>> NotificacionFacturaAPEmailAsync(string IdAnaliticoPago, IEnumerable<ValidationError> validationErrors, UsersDto user, Guid documentoBEId, string Correo, bool logProcess, string repositoryName, string methodName);
        Task<bool> EnvioCorreoArchivosAsync(DtoPdfFilesEmail dtoPdfFilesEmail);
        Task<bool> NotificacionCOPADEAsync(IEnumerable<UsersDto> users, CopadeDto copade, string subject);
        #endregion

        Task<bool> NotificacionESAsync(IEnumerable<UsersDto> users, IEnumerable<UsersDto> usersRepresentative, SOEstimationDto estimation, string subject);
        Task<bool> NotificacionREAsync(IEnumerable<UsersDto> users, ReceptionDto recepcion, string subject);
        Task<bool> NotificacionPPAsync(IEnumerable<UsersDto> users, PaymentScheduleDto programaPago, string subject);
        Task<bool> NotificacionLPAsync(IEnumerable<UsersDto> users, PaymentListDto listaPago, string subject);
        Task<bool> NotificacionAPAsync(IEnumerable<UsersDto> users, AnaliticoPagoDto analiticoPago, string subject);
        Task<bool> EnvioCorreoAPArchivosAsync(DtoPdfFilesEmail dtoPdfFilesEmail);

        Task<bool> NotificacionDesvioFirma(string Correo, string UserName, System.Net.HttpStatusCode status, string Documento, string Signer, string SignerNew);
    }
}

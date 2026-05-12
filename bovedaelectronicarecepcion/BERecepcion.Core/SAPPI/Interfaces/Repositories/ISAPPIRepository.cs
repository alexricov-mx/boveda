using BERecepcion.Core.Consulta.Copades.Dto;
using BERecepcion.Core.Dto;
using BERecepcion.Core.Facturas.Dto;
using BERecepcion.Core.Instrucciones.Dto;
using BERecepcion.Core.OrdenSurtimiento.Dto;
using BERecepcion.Core.SAPPI.Dto;
using BERecepcion.Core.Utils;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BERecepcion.Core.SAPPI.Interfaces.Repositories
{
    public interface ISAPPIRepository
    {
        Task<DataResult<OSResponseItemDto>> PostOS_SYNC_IB(SupplyOrderDto dto);
        Task<DataResult<REResponseDto>> PostRE_SYNC_IB(ReceptionDto dto);
        Task<DataResult<PagoResponseDto>> PostPP_SYNC_IB(PaymentScheduleDto dto);
        Task<DataResult<PagoResponseDto>> PostLP_SYNC_IB(PaymentListDto dto);
        Task<DataResult<OSResponseDto>> PostOS_Response(OSResponseDto dto);
        Task<DataResult<AnaliticoPagoResponseDto>> NotificaPreFactura(AnaliticoPagoPreFacturaDto dto);
        Task<DataResult<OSLiberacionVPDto>> PostOSLiberacionVP(OSLiberacionVPDto dto);
        Task<DataResult<REResponseItemDto>> PostRE_Response(REResponseDto dto);
        Task<DataResult<AnaliticoPagoResponseDto>> PostAP_SYNC_IB(AnaliticoPagoDto dto);
        Task<DataResult<ValidaMiro_responseDto>> PostValidacionCxP(ValidacionCXPDto dto);
        Task<DataResult<CXPDto>> PostCxP(CXPDto dto);
        Task<DataResult<string>> InsertaCopadeAsync(CopadeDto copade);
        Task<DataResult<CopadeDto>> RecuperaCopadeAsync(PICopadeRequestDto copadeReq);
        Task<DataResult<string>> InsertaLlaveCopadeAsync(CopadeDto copade);
        Task<DataResult<CopadeDto>> RecuperaCopadeBDAsync(PICopadeRequestDto copadeReq);
        Task<Guid> GetCopadeIdAsync(PICopadeRequestDto copadeReq);
        Task<DataResult<string>> GetPreFacturaById(Guid CopadeId);
        Task<DataResult<string>> GetNotaCreditoById(Guid CopadeId);
        Task<string> GetLogo(string clave);
        Task<DataResult<CopadeResponseDto>> PostCopade_Response(CopadeResponseDto dto);
        Task<DataResult<CopadeDto>> RecuperaCopadeEmailsync(Guid copade);

    }
}

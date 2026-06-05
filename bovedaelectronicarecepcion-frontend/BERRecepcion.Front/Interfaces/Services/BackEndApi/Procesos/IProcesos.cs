using System.Collections.Generic;
using System.Threading.Tasks;
using BERRecepcion.Front.Models.Dto;

namespace BERRecepcion.Front.Interfaces.Services.BackEndApi.Procesos;

public interface IProcesos
{
    Task<IEnumerable<ReactivaProcesosResponseDto>> GetReactivateProcessAsync(string SAPOrder);
    Task<ReactivaProcesosDto> GetSAPSignatureDeliveryAsync(string SAPOrder);
}
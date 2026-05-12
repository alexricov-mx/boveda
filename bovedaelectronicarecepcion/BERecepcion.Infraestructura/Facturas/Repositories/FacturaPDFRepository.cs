using BERecepcion.Core.Consulta.Copades.Dto;
using BERecepcion.Core.Dto;
using BERecepcion.Core.Facturas.Dto;
using BERecepcion.Core.Facturas.Interfaces.Repositories;
using BERecepcion.Core.Models;
using BERecepcion.Infraestructura.Repositories;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BERecepcion.Infraestructura.Facturas.Repositories
{
    public class FacturaPDFRepository : BaseSQLServerSqlRepository, IFacturaPDFRepository
    {
        public FacturaPDFRepository(string cnnString) : base(cnnString)
        {

        }
        public async Task<DataResult<IEnumerable<InvoiceDto>>> GetFacturaPDFAsync(Guid CopadeID)
        {
            DataResult<IEnumerable<InvoiceDto>> resultItem = new DataResult<IEnumerable<InvoiceDto>>
            {
                Status = System.Net.HttpStatusCode.OK,
            };

            try
            {

                using (IDbConnection db = GetConnection())
                {
                    DynamicParameters par = new DynamicParameters();
                    par.Add("DocumentoBEId", CopadeID);

                    var result = await db.QueryAsync<InvoiceDto>(sql: "SP_FacturaPDF_GetInvoice", param: par, commandType: CommandType.StoredProcedure);

                    resultItem.Data = result;

                    return resultItem;
                }
            }
            catch (Exception)
            {
                //resultItem.Status = System.Net.HttpStatusCode.BadRequest;
                //resultItem.Message = $"Ocurrio un problema: {ex.Message}";   
                //return resultItem;
                throw;
            }
        }
        public async Task<DataResult<IEnumerable<InvoiceNotaCreditoDto>>> GetNotaCreditoPDFAsync(Guid InvoiceId)
        {
            DataResult<IEnumerable<InvoiceNotaCreditoDto>> resultItem = new DataResult<IEnumerable<InvoiceNotaCreditoDto>>
            {
                Status = System.Net.HttpStatusCode.OK,
            };

            try
            {
                using (IDbConnection db = GetConnection())
                {
                    DynamicParameters par = new DynamicParameters();
                    par.Add("InvoiceId", InvoiceId);

                    var result = await db.QueryAsync<InvoiceNotaCreditoDto>(sql: "SP_FacturaPDF_GetNotaCredito", param: par, commandType: CommandType.StoredProcedure);

                    resultItem.Data = result;

                    return resultItem;
                }
            }
            catch (Exception)
            {
                //resultItem.Status = System.Net.HttpStatusCode.BadRequest;
                //resultItem.Message = $"Ocurrio un problema: {ex.Message}";   
                //return resultItem;
                throw;
            }
        }
        public async Task<DataResult<IEnumerable<CopadeDto>>> GetComprobantePDFAsync(Guid PagoUUID)
        {
            DataResult<IEnumerable<CopadeDto>> resultItem = new DataResult<IEnumerable<CopadeDto>>
            {
                Status = System.Net.HttpStatusCode.OK,
            };

            try
            {
                using (IDbConnection db = GetConnection())
                {
                    DynamicParameters par = new DynamicParameters();
                    par.Add("PagoUUID", PagoUUID);

                    var result = await db.QueryAsync<CopadeDto>(sql: "SP_FacturaPDF_GetComprobante", param: par, commandType: CommandType.StoredProcedure);

                    resultItem.Data = result;

                    return resultItem;
                }
            }
            catch (Exception)
            {
                //resultItem.Status = System.Net.HttpStatusCode.BadRequest;
                //resultItem.Message = $"Ocurrio un problema: {ex.Message}";   
                //return resultItem;
                throw;
            }
        }
    }
}

using BERecepcion.Core.Dto;
using BERecepcion.Core.Facturas.Dto;
using BERecepcion.Core.Facturas.Interfaces.Repositories;
using BERecepcion.Infraestructura.Repositories;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace BERecepcion.Infraestructura.Facturas.Repositories
{
    public class InvoiceRepository : BaseSQLServerSqlRepository, IInvoiceRepository
    {

        public InvoiceRepository(string cnnString) : base(cnnString)
        {

        }
        public async Task<DataResult<InvoiceDto>> GetInvoiceByReception(string claveOrganismo, string reception, string exercise)
        {
            DataResult<InvoiceDto> resultItem = new DataResult<InvoiceDto> { Status = System.Net.HttpStatusCode.OK };

            try
            {
                DynamicParameters par = new DynamicParameters();
                par.Add("@clave", claveOrganismo);
                par.Add("@reception", reception);
                par.Add("@exercise", exercise);

                using (IDbConnection db = GetConnection())
                {
                    var data = await db.QueryFirstOrDefaultAsync<InvoiceDto>(sql: "SP_invoice_get_invoice_by_reception", param: par, commandType: CommandType.StoredProcedure);
                    if (data != null)
                    {
                        resultItem.Status = System.Net.HttpStatusCode.BadRequest;
                        resultItem.Message = "El copade ya tiene una factura asignada.";
                        return resultItem;
                    }
                    resultItem.Data = data;
                    return resultItem;
                }
            }
            catch (Exception ex)
            {
                resultItem.Status = System.Net.HttpStatusCode.BadRequest;
                resultItem.Message = $"Ocurrio un problema: {ex.Message}";
                return resultItem;
            }
        }
        public async Task<DataResult<InvoiceDto>> GetInvoiceIDByReception(string claveOrganismo, string reception, string exercise)
        {
            DataResult<InvoiceDto> resultItem = new DataResult<InvoiceDto> { Status = System.Net.HttpStatusCode.OK };

            try
            {
                DynamicParameters par = new DynamicParameters();
                par.Add("@clave", claveOrganismo);
                par.Add("@reception", reception);
                par.Add("@exercise", exercise);

                using (IDbConnection db = GetConnection())
                {
                    var data = await db.QueryFirstOrDefaultAsync<InvoiceDto>(sql: "SP_invoice_get_invoice_by_reception", param: par, commandType: CommandType.StoredProcedure);
                    if (data == null)
                    {
                        resultItem.Status = System.Net.HttpStatusCode.BadRequest;
                        resultItem.Message = "No exite una factura asociada.";
                        return resultItem;
                    }
                    resultItem.Data = data;
                    return resultItem;
                }
            }
            catch (Exception ex)
            {
                resultItem.Status = System.Net.HttpStatusCode.BadRequest;
                resultItem.Message = $"Ocurrio un problema: {ex.Message}";
                return resultItem;
            }
        }

        #region AP

        public async Task<DataResult<InvoiceDto>> GetInvoiceByAnaliticoPago(string IdAnaliticoPago)
        {
            DataResult<InvoiceDto> resultItem = new DataResult<InvoiceDto> { Status = System.Net.HttpStatusCode.OK };

            try
            {
                DynamicParameters par = new DynamicParameters();
                par.Add("@IdAnaliticoPago", IdAnaliticoPago);

                using (IDbConnection db = GetConnection())
                {
                    var data = await db.QueryFirstOrDefaultAsync<InvoiceDto>(sql: "SP_invoice_get_invoice_by_analiticopago", param: par, commandType: CommandType.StoredProcedure);
                    if (data != null)
                    {
                        resultItem.Status = System.Net.HttpStatusCode.BadRequest;
                        resultItem.Message = "El analitico pago ya tiene una factura asignada.";
                        return resultItem;
                    }
                    resultItem.Data = data;
                    return resultItem;
                }
            }
            catch (Exception ex)
            {
                resultItem.Status = System.Net.HttpStatusCode.BadRequest;
                resultItem.Message = $"Ocurrio un problema: {ex.Message}";
                return resultItem;
            }
        }
        public async Task<DataResult<InvoiceDto>> GetInvoiceIDByAnaliticoPago(string IdAnaliticoPago)
        {
            DataResult<InvoiceDto> resultItem = new DataResult<InvoiceDto> { Status = System.Net.HttpStatusCode.OK };

            try
            {
                DynamicParameters par = new DynamicParameters();
                par.Add("@IdAnaliticoPago", IdAnaliticoPago);

                using (IDbConnection db = GetConnection())
                {
                    var data = await db.QueryFirstOrDefaultAsync<InvoiceDto>(sql: "SP_invoice_get_invoice_by_analiticopago", param: par, commandType: CommandType.StoredProcedure);
                    if (data == null)
                    {
                        resultItem.Status = System.Net.HttpStatusCode.BadRequest;
                        resultItem.Message = "El analitico pago ya tiene una factura asignada.";
                        return resultItem;
                    }
                    resultItem.Data = data;
                    return resultItem;
                }
            }
            catch (Exception ex)
            {
                resultItem.Status = System.Net.HttpStatusCode.BadRequest;
                resultItem.Message = $"Ocurrio un problema: {ex.Message}";
                return resultItem;
            }
        }
        #endregion
    }
}

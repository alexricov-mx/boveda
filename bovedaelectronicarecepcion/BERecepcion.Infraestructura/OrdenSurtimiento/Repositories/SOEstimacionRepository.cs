using BERecepcion.Core.Dto;
using BERecepcion.Core.eSignDto;
using BERecepcion.Core.OrdenSurtimiento.Dto;
using BERecepcion.Core.OrdenSurtimiento.Interfaces.Repositories;
using BERecepcion.Infraestructura.Repositories;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;


namespace BERecepcion.Infraestructura.OrdenSurtimiento.Repositories
{
    public class SOEstimacionRepository : BaseSQLServerSqlRepository, ISOEstimationRepository
    {

        public SOEstimacionRepository(string cnnString) : base(cnnString)
        {
        }

        DataResult<SOEstimationDto> resultItem = new DataResult<SOEstimationDto>()
        {
            Status = HttpStatusCode.OK,
            Message = "Actualizado con Exito"
        };

        public async Task<IEnumerable<SOEstimationDto>> GetSOEstimacionAsync(string Contract)
        {
            // Parametros
            DynamicParameters par = new DynamicParameters();
            par.Add("@Contract", Contract);

            //using para levantar la conexion al BD
            using (IDbConnection db = GetConnection())
            {
                return await db.QueryAsync<SOEstimationDto>(sql: "SP_estimacion_seleccion", param: par, commandType: CommandType.StoredProcedure);
            }
        }

        public async Task<DataResult<IEnumerable<SOEstimationDto>>> GetSOEInternoAsync(string Token, int pageSize, int pageNum = 1)
        {
            DataResult<IEnumerable<SOEstimationDto>> resultItem = new DataResult<IEnumerable<SOEstimationDto>>()
            {
                Status = HttpStatusCode.OK
            };
            try
            {
                using (IDbConnection db = GetConnection())
                {
                    DynamicParameters par = new DynamicParameters();
                    par.Add("@Token", Token);
                    par.Add("@pagenum", pageNum);
                    par.Add("@pagesize", pageSize);

                    var result = await db.QueryMultipleAsync(sql: "SP_SOEstimation_Interno_selecciona ", param: par, commandType: CommandType.StoredProcedure);

                    var paging = await result.ReadAsync<Pager>();
                    var supplyOrders = await result.ReadAsync<SOEstimationDto>();

                    resultItem.Data = supplyOrders;
                    resultItem.Pager = paging.FirstOrDefault();
                }
                return resultItem;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<DataResult<IEnumerable<SOEstimationDto>>> GetSOEProveedorAsync(string CreditorNumber, int pageSize, int pageNum = 1)
        {
            DataResult<IEnumerable<SOEstimationDto>> resultItem = new DataResult<IEnumerable<SOEstimationDto>>()
            {
                Status = HttpStatusCode.OK
            };
            try
            {
                using (IDbConnection db = GetConnection())
                {
                    DynamicParameters par = new DynamicParameters();
                    par.Add("@CreditorNumber", CreditorNumber);
                    par.Add("@pagenum", pageNum);
                    par.Add("@pagesize", pageSize);

                    var result = await db.QueryMultipleAsync(sql: "SP_SOEstimation_Proveedor_selecciona ", param: par, commandType: CommandType.StoredProcedure);

                    var paging = await result.ReadAsync<Pager>();
                    var supplyOrders = await result.ReadAsync<SOEstimationDto>();

                    resultItem.Data = supplyOrders;
                    resultItem.Pager = paging.FirstOrDefault();
                }
                return resultItem;
            }
            catch (Exception)
            {
                throw;
            }
        }
        // Dto documentoFirma

        public async Task<DataResult<SOEstimationDto>> EstimationFirmaAsync(Guid EstimacionID, string UserType)
        {
            try
            {
                using (IDbConnection db = GetConnection())
                {
                    //db.Open();
                    //using (var tran = db.BeginTransaction())
                    //{
                        try
                        {
                            DynamicParameters par = new DynamicParameters();
                            par.Add("@EstimacionID", EstimacionID);
                            par.Add("@UserType", UserType);
                            var asdf = await db.QueryAsync(sql: "SP_SOEstimation_firma", param: par, commandType: CommandType.StoredProcedure);

                            //tran.Commit();
                            resultItem.Message = "Firmado exitosamente";
                        }
                        catch (Exception ext)
                        {
                            //tran.Rollback();
                            resultItem.Message = ext.Message;
                            resultItem.Status = HttpStatusCode.BadRequest;

                            return resultItem;

                        }
                    //}
                    return resultItem;
                }
            }
            catch (Exception ext)
            {
                resultItem.Message = ext.Message;
                resultItem.Status = HttpStatusCode.BadRequest;

                return resultItem;
            }
        }

        public async Task<DataResult<SOEstimationDto>> EstimationFirmaCorreoAsync(Guid EstimacionID, string UserType, string ProviderEmail)
        {
            try
            {
                using (IDbConnection db = GetConnection())
                {
                    //db.Open();
                    //using (var tran = db.BeginTransaction())
                    //{
                        try
                        {
                            // hay que avisarle al siguiente firmante
                            DynamicParameters par = new DynamicParameters();
                            par.Add("@EstimacionID", EstimacionID);
                            par.Add("@UserType", UserType);
                            par.Add("@Email", ProviderEmail);

                            var asdf = await db.QueryAsync(sql: "SP_SOEstimation_firma_correo", param: par, commandType: CommandType.StoredProcedure);

                            //tran.Commit();
                            resultItem.Message = "Firmado exitosamente";
                        }
                        catch (Exception ext)
                        {
                            //tran.Rollback();
                            resultItem.Message = ext.Message;
                            resultItem.Status = HttpStatusCode.BadRequest;

                            return resultItem;

                        }
                    //}
                    return resultItem;
                }
            }
            catch (Exception ext)
            {
                resultItem.Message = ext.Message;
                resultItem.Status = HttpStatusCode.BadRequest;

                return resultItem;
            }
        }

        public async Task<DataResult<IEnumerable<SOEstimationDto>>> GetSOEstimacionFiltroAsync(string Token, string Filtro)
        {
            DataResult<IEnumerable<SOEstimationDto>> resultItem = new DataResult<IEnumerable<SOEstimationDto>>
            {
                Status = HttpStatusCode.OK,
                Message = "GetListaFiltroCopadesAsync"
            };

            try
            {
                DynamicParameters par = new DynamicParameters();
                par.Add("Token", Token);
                par.Add("Filtro", Filtro);

                using (IDbConnection db = GetConnection())
                {
                    //var test = await db.QueryAsync<SOEstimationDto>(sql: "SP_SOEstimation_Interno_filtro", param: par, commandType: CommandType.StoredProcedure);
                    var result = await db.QueryAsync<SOEstimationDto>(sql: "SP_SOEstimation_Interno_filtro", param: par, commandType: CommandType.StoredProcedure);
                    resultItem.Message = "Datos Insertados correctamente";
                    resultItem.Data = result;


                    return resultItem;
                }
            }
            catch (Exception ex)
            {
                resultItem.Status = HttpStatusCode.BadRequest;
                resultItem.Message = $"Ocurrio un problema: {ex.Message}";
                return resultItem;
            }
        }

        public async Task<DataResult<IEnumerable<SOEstimationDto>>> GetListaEstimacionesBancariasAsync(DateTime start, DateTime end, string search, string UserID, string claveOrganismo, string creditorNumber, int pageSize, int pageNum = 1, bool esDescarga = false)
        {
            DataResult<IEnumerable<SOEstimationDto>> resultItem = new DataResult<IEnumerable<SOEstimationDto>>
            {
                Status = HttpStatusCode.OK,
            };

            if (!string.IsNullOrWhiteSpace(search))
                search = Core.Utils.SearchText.GetWhereClause(search, new List<string> { "OrganismClave", "Contract", "saporder", "CreditorNumber" });

            try
            {

                using (IDbConnection db = GetConnection())
                {
                    DynamicParameters par = new DynamicParameters();
                    par.Add("UserID", UserID);
                    par.Add("@start", start);
                    par.Add("@end", end);
                    par.Add("@search", search);
                    par.Add("@pagenum", pageNum);
                    par.Add("@pagesize", pageSize);
                    par.Add("@claveOrganismo", claveOrganismo);
                    par.Add("@creditorNumber", creditorNumber);
                    par.Add("@esDescarga", esDescarga);

                    var result = await db.QueryMultipleAsync(sql: "SP_SOEstimation_bancario_tabla_seleccion", param: par, commandType: CommandType.StoredProcedure);

                    var paging = await result.ReadAsync<Pager>();
                    var ordensurtimientoresult = await result.ReadAsync<SOEstimationDto>();

                    resultItem.Pager = paging.FirstOrDefault();

                    resultItem.Data = ordensurtimientoresult;

                    return resultItem;
                }
            }
            catch (Exception ex)
            {
                resultItem.Status = HttpStatusCode.BadRequest;
                resultItem.Message = $"Ocurrio un problema: {ex.Message}";
                return resultItem;
            }
        }

        #region Consulta
        public async Task<DataResult<IEnumerable<SOEstimationDto>>> GetESAsync(int pageSize, Guid userId, int pageNum = 1, DateTime? fechaInicial = null, DateTime? fechaFinal = null, string search = null, bool esDescarga = false)
        {
            DataResult<IEnumerable<SOEstimationDto>> resultItem = new DataResult<IEnumerable<SOEstimationDto>>()
            {
                Status = HttpStatusCode.OK
            };

            if (!string.IsNullOrWhiteSpace(search))
                search = Core.Utils.SearchText.GetWhereClause(search, new List<string> { "SAPOrder", "CreditorNumber", "Contract", "OrganismClave", "DocumentType", "Currency" });

            try
            {
                using (IDbConnection db = GetConnection())
                {
                    DynamicParameters par = new DynamicParameters();
                    par.Add("@pagenum", pageNum);
                    par.Add("@pagesize", pageSize);
                    par.Add("@userId", userId);
                    par.Add("@fechaInicial", fechaInicial);
                    par.Add("@fechaFinal", fechaFinal);
                    par.Add("@search", search);
                    par.Add("@esDescarga", esDescarga);

                    var result = await db.QueryMultipleAsync(sql: "SP_SOEstimation_Consulta_Selecciona", param: par, commandType: CommandType.StoredProcedure);

                    var paging = await result.ReadAsync<Pager>();
                    var estimaciones = await result.ReadAsync<SOEstimationDto>();

                    resultItem.Data = estimaciones;
                    resultItem.Pager = paging.FirstOrDefault();
                }
                return resultItem;
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion
        public async Task<DataResult<SupplyOrderDto>> EstimacionCorreo1Async(Guid EstimacionID, string SignerEmail)
        {
            DataResult<SupplyOrderDto> resultItem = new DataResult<SupplyOrderDto>()
            {
                Status = HttpStatusCode.OK,
                Message = "Firmado exitosamente"
            };
            try
            {
                using (IDbConnection db = GetConnection())
                {
                    DynamicParameters par = new DynamicParameters();
                    par.Add("@EstimacionID", EstimacionID);
                    par.Add("@SignerEmail", SignerEmail);
                    await db.QueryAsync(sql: "SP_SOEstimation_correo_1", param: par, commandType: CommandType.StoredProcedure);
                    resultItem.Message = "Se actualizaron los datos correctamente";
                }
                return resultItem;
            }
            catch (Exception ext)
            {
                resultItem.Message = ext.Message;
                resultItem.Status = HttpStatusCode.BadRequest;
                return resultItem;
            }
        }

        public async Task<DataResult<SupplyOrderDto>> EstimacionCorreo2Async(Guid EstimacionID, string SignerEmail)
        {
            DataResult<SupplyOrderDto> resultItem = new DataResult<SupplyOrderDto>()
            {
                Status = HttpStatusCode.OK,
                Message = "Firmado exitosamente"
            };
            try
            {
                using (IDbConnection db = GetConnection())
                {
                    DynamicParameters par = new DynamicParameters();
                    par.Add("@EstimacionID", EstimacionID);
                    par.Add("@SignerEmail", SignerEmail);
                    await db.QueryAsync(sql: "SP_SOEstimation_correo_2", param: par, commandType: CommandType.StoredProcedure);
                    resultItem.Message = "Se actualizaron los datos correctamente";
                }
                return resultItem;
            }
            catch (Exception ext)
            {
                resultItem.Message = ext.Message;
                resultItem.Status = HttpStatusCode.BadRequest;
                return resultItem;
            }
        }

        public async Task<Guid> GetEstimacionIdAsync(SOEstimationDto dto)
        {
            try
            {
                using (IDbConnection db = GetConnection())
                {
                    DynamicParameters par = new DynamicParameters();
                    par.Add("@clave", dto.OrganismClave);
                    par.Add("@Contract", dto.Contract);
                    par.Add("@SapOrder", dto.SapOrder);
                    var res = await db.QueryFirstOrDefaultAsync<Guid>(sql: "SP_SOEstimation_guid", param: par, commandType: CommandType.StoredProcedure);
                    return res;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<DataResult<IEnumerable<SupplyOrderDto>>> GetListaOrdenesBancariasAsync(DateTime start, DateTime end, string search, string UserID, string claveOrganismo, string creditorNumber, int pageSize, int pageNum = 1, bool esDescarga = false)
        {
            DataResult<IEnumerable<SupplyOrderDto>> resultItem = new DataResult<IEnumerable<SupplyOrderDto>>
            {
                Status = HttpStatusCode.OK,
            };

            if (!string.IsNullOrWhiteSpace(search))
                search = Core.Utils.SearchText.GetWhereClause(search, new List<string> { "OrganismClave", "Contract", "saporder", "CreditorNumber" });

            try
            {

                using (IDbConnection db = GetConnection())
                {
                    DynamicParameters par = new DynamicParameters();
                    par.Add("UserID", UserID);
                    par.Add("@start", start);
                    par.Add("@end", end);
                    par.Add("@search", search);
                    par.Add("@pagenum", pageNum);
                    par.Add("@pagesize", pageSize);
                    par.Add("@claveOrganismo", claveOrganismo);
                    par.Add("@creditorNumber", creditorNumber);
                    par.Add("@esDescarga", esDescarga);

                    var result = await db.QueryMultipleAsync(sql: "SP_SupplyOrder_representante_tabla_seleccion", param: par, commandType: CommandType.StoredProcedure);

                    var paging = await result.ReadAsync<Pager>();
                    var ordensurtimientoresult = await result.ReadAsync<SupplyOrderDto>();

                    resultItem.Pager = paging.FirstOrDefault();

                    resultItem.Data = ordensurtimientoresult;

                    return resultItem;
                }
            }
            catch (Exception ex)
            {
                resultItem.Status = HttpStatusCode.BadRequest;
                resultItem.Message = $"Ocurrio un problema: {ex.Message}";
                return resultItem;
            }
        }

        public async Task<DataResult<SOEstimationDto>> EstimationFirmaNotificacionAsync(Guid EstimacionID, string UserType)
        {
            DataResult<SOEstimationDto> resultItem = new DataResult<SOEstimationDto>()
            {
                Status = HttpStatusCode.OK,
                Message = "Firmado exitosamente"
            };
            try
            {
                using (IDbConnection db = GetConnection())
                {
                    DynamicParameters par = new DynamicParameters();
                    par.Add("@EstimacionID", EstimacionID);
                    par.Add("@UserType", UserType);
                    await db.QueryAsync(sql: "SP_Estimacion_firmanotificacion", param: par, commandType: CommandType.StoredProcedure);
                    resultItem.Message = "Se actualizaron los datos correctamente";
                    return resultItem;
                }
            }
            catch (Exception ext)
            {
                resultItem.Message = ext.Message;
                resultItem.Status = HttpStatusCode.BadRequest;
                return resultItem;
            }
        }
    }
}

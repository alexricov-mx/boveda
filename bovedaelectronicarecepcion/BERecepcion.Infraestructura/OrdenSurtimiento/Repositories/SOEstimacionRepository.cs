using BERecepcion.Core.Common.Results;
using BERecepcion.Core.Consulta.Dto;
using BERecepcion.Core.Dto;
using BERecepcion.Core.Estimaciones.Dtos;
using BERecepcion.Core.Interfaces;
using BERecepcion.Core.OrdenSurtimiento.Dto;
using BERecepcion.Core.OrdenSurtimiento.Interfaces.Repositories;
using BERecepcion.Infraestructura.Repositories;
using Dapper;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing.Printing;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;


namespace BERecepcion.Infraestructura.OrdenSurtimiento.Repositories
{
    public class SOEstimacionRepository : BaseSQLServerSqlRepository, ISOEstimationRepository
    {

        public SOEstimacionRepository(IDbConnectionFactory connectionFactory)
            : base(connectionFactory)
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

        public async Task<PagedResult<SOEstimationDto>> GetSOEInternoRefactorAsync(
            string token, int pageSize, int pageNum = 1,
            CancellationToken cancellationToken = default)
        {
            if (pageNum < 1) pageNum = 1;
            if (pageSize < 1) pageSize = 5;

            using IDbConnection db = GetConnection();

            var par = new DynamicParameters();
            par.Add("@Token", token);
            par.Add("@pagenum", pageNum);
            par.Add("@pagesize", pageSize);

            var multi = await db.QueryMultipleAsync(
                sql: "SP_SOEstimation_Interno_selecciona",
                param: par,
                commandType: CommandType.StoredProcedure);

            var pager = (await multi.ReadAsync<Pager>()).FirstOrDefault();
            var items = (await multi.ReadAsync<SOEstimationDto>()).ToList();

            return new PagedResult<SOEstimationDto>(
                items,
                totalItems: pager?.TotalItems ?? items.Count,
                pageNumber: pager?.CurrentPage ?? pageNum,
                pageSize: pager?.PageSize ?? pageSize);
        }

        public async Task<DataResult<IEnumerable<SOEstimationDto>>> GetSOEInternoAsync(
            string token, int pageSize, int pageNum = 1,
            CancellationToken cancellationToken = default)
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
                    par.Add("@Token", token);
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

        public async Task<PagedResult<SOEstimationDto>> GetSOEProveedorPaginatorAsync(SOEstimationProveedorRequestDto request, CancellationToken cancellationToken = default)
        {
            if (request.PageNumber < 1) request.PageNumber = 1;
            if (request.PageSize < 1) request.PageSize = 5;

            using IDbConnection db = GetConnection();

            DynamicParameters par = new();
            par.Add("@CreditorNumber", request.CreditorNumber);
            par.Add("@pagenum", request.PageNumber);
            par.Add("@pagesize", request.PageSize);

            var result = await db.QueryMultipleAsync(
                sql: "SP_SOEstimation_Proveedor_selecciona ", 
                param: par, 
                commandType: CommandType.StoredProcedure
                );

            var paging = (await result.ReadAsync<Pager>()).FirstOrDefault();
            var items = (await result.ReadAsync<SOEstimationDto>()).ToList();

            return new PagedResult<SOEstimationDto>(
                items,
                totalItems: paging?.TotalItems ?? items.Count,
                pageNumber: paging?.CurrentPage ?? request.PageNumber,
                pageSize: paging?.PageSize ?? request.PageSize);
        }

        public async Task<PagedResult<SupplyOrderDto>> GetPagedOrdenesBancariasAsync(
            OrdenBancariaRequest request, 
            CancellationToken cancellationToken = default)
        {

                if (request.PageNumber < 1) request.PageNumber = 1;
                if (request.PageSize < 1) request.PageSize = 5;

                using IDbConnection db = GetConnection();

                {
                    DynamicParameters par = new ();
                    par.Add("UserID", request.UserId);
                    par.Add("@start", request.FechaInicial);
                    par.Add("@end", request.FechaFinal);
                    par.Add("@search", request.Search);
                    par.Add("@pagenum", request.PageNumber);
                    par.Add("@pagesize", request.PageSize);
                    par.Add("@claveOrganismo", request.ClaveOrganismo);
                    par.Add("@creditorNumber", request.CreditorNumber);
                    par.Add("@esDescarga", request.EsDescarga);

                    var result = await db.QueryMultipleAsync(
                        sql: "SP_SupplyOrder_representante_tabla_seleccion", 
                        param: par, 
                        commandType: CommandType.StoredProcedure
                        );

                    var totalItems = (await result.ReadAsync<int>()).FirstOrDefault();
                    var items = (await result.ReadAsync<SupplyOrderDto>()).ToList();
                    var pager = new Pager(totalItems, request.PageNumber, request.PageSize);

                    return new PagedResult<SupplyOrderDto>(
                        items: items,
                        totalItems: pager.TotalItems,
                        pageNumber: pager.CurrentPage,
                        pageSize: pager.PageSize
                        );
                }
            
        }
    }
}

using BERecepcion.Core.Common.Results;
using BERecepcion.Core.Dto;
using BERecepcion.Core.Interfaces;
using BERecepcion.Core.OrdenSurtimiento.Dto;
using BERecepcion.Core.OrdenSurtimiento.Interfaces.Repositories;
using BERecepcion.Infraestructura.Repositories;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BERecepcion.Infraestructura.OrdenSurtimiento.Repositories
{
    public class SupplyOrderRepository : BaseSQLServerSqlRepository, ISupplyOrderRepository
    {
        public SupplyOrderRepository(IDbConnectionFactory connectionFactory)
            : base(connectionFactory)
        {
        }

        #region Ordenes de surtimiento

        public async Task<DataResult<IEnumerable<SupplyOrderDto>>> GetOSInternoAsync(string Token, int pageSize, string search = null, int pageNum = 1)
        {
            DataResult<IEnumerable<SupplyOrderDto>> resultItem = new DataResult<IEnumerable<SupplyOrderDto>>()
            {
                Status = System.Net.HttpStatusCode.OK
            };
            try
            {
                using (IDbConnection db = GetConnection())
                {
                    DynamicParameters par = new DynamicParameters();
                    par.Add("@Token", Token);
                    par.Add("@pagenum", pageNum);
                    par.Add("@search", search);
                    par.Add("@pagesize", pageSize);

                    var result = await db.QueryMultipleAsync(sql: "SP_SupplyOrder_Interno_selecciona ", param: par, commandType: CommandType.StoredProcedure);

                    var paging = await result.ReadAsync<Pager>();
                    var supplyOrders = await result.ReadAsync<SupplyOrderDto>();

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

        public async Task<DataResult<IEnumerable<SupplyOrderDto>>> GetOSProveedorAsync(string CreditorNumber, int pageSize, string search = null, int pageNum = 1)
        {
            DataResult<IEnumerable<SupplyOrderDto>> resultItem = new DataResult<IEnumerable<SupplyOrderDto>>()
            {
                Status = System.Net.HttpStatusCode.OK
            };
            try
            {
                using (IDbConnection db = GetConnection())
                {
                    DynamicParameters par = new DynamicParameters();
                    par.Add("@CreditorNumber", CreditorNumber);
                    par.Add("@pagenum", pageNum);
                    par.Add("@search", search);
                    par.Add("@pagesize", pageSize);

                    var result = await db.QueryMultipleAsync(sql: "SP_SupplyOrder_Proveedor_selecciona ", param: par, commandType: CommandType.StoredProcedure);

                    var paging = await result.ReadAsync<Pager>();
                    var supplyOrders = await result.ReadAsync<SupplyOrderDto>();

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
        public async Task<DataResult<SupplyOrderDto>> SupplyOrderFirmaAsync(Guid SupplyOrderID, string UserType, string Ficha)
        {
            DataResult<SupplyOrderDto> resultItem = new DataResult<SupplyOrderDto>()
            {
                Status = System.Net.HttpStatusCode.OK,
                Message = "Firmado exitosamente"
            };
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
                        par.Add("@SupplyOrderID", SupplyOrderID);
                        par.Add("@UserType", UserType);
                        par.Add("@Ficha", Ficha);
                        await db.QueryAsync(sql: "SP_SupplyOrder_firma", param: par, commandType: CommandType.StoredProcedure);
                        //tran.Commit();
                        resultItem.Message = "Se actualizaron los datos correctamente";
                    }
                    catch (Exception ext)
                    {
                        //tran.Rollback();
                        resultItem.Message = ext.Message;
                        resultItem.Status = System.Net.HttpStatusCode.BadRequest;
                        return resultItem;
                    }
                    //}
                    return resultItem;
                }
            }
            catch (Exception ext)
            {
                resultItem.Message = ext.Message;
                resultItem.Status = System.Net.HttpStatusCode.BadRequest;
                return resultItem;
            }
        }
        public async Task<DataResult<SupplyOrderDto>> SupplyOrderFirmaCorreoAsync(Guid SupplyOrderID, string UserType, string SignerEmail)
        {
            DataResult<SupplyOrderDto> resultItem = new DataResult<SupplyOrderDto>()
            {
                Status = System.Net.HttpStatusCode.OK,
                Message = "Firmado exitosamente"
            };
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
                        par.Add("@SupplyOrderID", SupplyOrderID);
                        par.Add("@UserType", UserType);
                        par.Add("@SignerEmail", SignerEmail);
                        await db.QueryAsync(sql: "SP_SupplyOrder_firma_correo", param: par, commandType: CommandType.StoredProcedure);
                        //tran.Commit();
                        resultItem.Message = "Se actualizaron los datos correctamente";
                    }
                    catch (Exception ext)
                    {
                        //tran.Rollback();
                        resultItem.Message = ext.Message;
                        resultItem.Status = System.Net.HttpStatusCode.BadRequest;
                        return resultItem;
                    }
                    //}
                    return resultItem;
                }
            }
            catch (Exception ext)
            {
                resultItem.Message = ext.Message;
                resultItem.Status = System.Net.HttpStatusCode.BadRequest;
                return resultItem;
            }
        }

        public async Task<DataResult<SupplyOrderDto>> SupplyOrderCorreo1Async(Guid SupplyOrderID, string SignerEmail)
        {
            DataResult<SupplyOrderDto> resultItem = new DataResult<SupplyOrderDto>()
            {
                Status = System.Net.HttpStatusCode.OK,
                Message = "Firmado exitosamente"
            };
            try
            {
                using (IDbConnection db = GetConnection())
                {
                    DynamicParameters par = new DynamicParameters();
                    par.Add("@SupplyOrderID", SupplyOrderID);
                    par.Add("@SignerEmail", SignerEmail);
                    await db.QueryAsync(sql: "SP_SupplyOrder_correo_1", param: par, commandType: CommandType.StoredProcedure);
                    resultItem.Message = "Se actualizaron los datos correctamente";
                }
                return resultItem;
            }
            catch (Exception ext)
            {
                resultItem.Message = ext.Message;
                resultItem.Status = System.Net.HttpStatusCode.BadRequest;
                return resultItem;
            }
        }

        public async Task<DataResult<SupplyOrderDto>> SupplyOrderCorreo2Async(Guid SupplyOrderID, string SignerEmail)
        {
            DataResult<SupplyOrderDto> resultItem = new DataResult<SupplyOrderDto>()
            {
                Status = System.Net.HttpStatusCode.OK,
                Message = "Firmado exitosamente"
            };
            try
            {
                using (IDbConnection db = GetConnection())
                {
                    DynamicParameters par = new DynamicParameters();
                    par.Add("@SupplyOrderID", SupplyOrderID);
                    par.Add("@SignerEmail", SignerEmail);
                    await db.QueryAsync(sql: "SP_SupplyOrder_correo_2", param: par, commandType: CommandType.StoredProcedure);
                    resultItem.Message = "Se actualizaron los datos correctamente";
                }
                return resultItem;
            }
            catch (Exception ext)
            {
                resultItem.Message = ext.Message;
                resultItem.Status = System.Net.HttpStatusCode.BadRequest;
                return resultItem;
            }
        }
        #endregion

        #region Consulta
        public async Task<DataResult<IEnumerable<SupplyOrderDto>>> GetOSAsync(int pageSize, Guid userId, int pageNum = 1, DateTime? fechaInicial = null, DateTime? fechaFinal = null, string search = null, bool esDescarga = false)
        {
            DataResult<IEnumerable<SupplyOrderDto>> resultItem = new DataResult<IEnumerable<SupplyOrderDto>>()
            {
                Status = System.Net.HttpStatusCode.OK
            };

            if (!string.IsNullOrWhiteSpace(search))
                search = Core.Utils.SearchText.GetWhereClause(search, new List<string> { "SAPOrder", "Creditor", "DocumentType", "Currency", "Clave", "Contract" });

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

                    var result = await db.QueryMultipleAsync(sql: "SP_SupplyOrder_Consulta_Selecciona", param: par, commandType: CommandType.StoredProcedure);

                    var paging = await result.ReadAsync<Pager>();
                    var supplyOrders = await result.ReadAsync<SupplyOrderDto>();

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

        public async Task<Guid> GetOSSupplyOrderIdAsync(SupplyOrderDto dto)
        {
            try
            {
                using (IDbConnection db = GetConnection())
                {
                    DynamicParameters par = new DynamicParameters();
                    par.Add("@clave", dto.OrganismClave);
                    par.Add("@Contract", dto.Contract);
                    par.Add("@SapOrder", dto.SAPOrder);
                    var res = await db.QueryFirstOrDefaultAsync<Guid>(sql: "SP_SupplyOrder_guid", param: par, commandType: CommandType.StoredProcedure);
                    return res;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<DataResult<SupplyOrderDto>> SupplyOrderFirmaNotificacionAsync(Guid SupplyOrderID, string UserType)
        {
            DataResult<SupplyOrderDto> resultItem = new DataResult<SupplyOrderDto>()
            {
                Status = System.Net.HttpStatusCode.OK,
                Message = "Firmado exitosamente"
            };
            try
            {
                using (IDbConnection db = GetConnection())
                {
                    DynamicParameters par = new DynamicParameters();
                    par.Add("@SupplyOrderID", SupplyOrderID);
                    par.Add("@UserType", UserType);
                    await db.QueryAsync(sql: "SP_SupplyOrder_firmanotificacion", param: par, commandType: CommandType.StoredProcedure);
                    resultItem.Message = "Se actualizaron los datos correctamente";
                    return resultItem;
                }
            }
            catch (Exception ext)
            {
                resultItem.Message = ext.Message;
                resultItem.Status = System.Net.HttpStatusCode.BadRequest;
                return resultItem;
            }
        }


        public async Task<PagedResult<SupplyOrderDto>> GetListPaginatedSupplyOrderByProveedorAsync(
            string creditorNumber,
            int pageSize,
            int pageNumber,
            string search,
            CancellationToken cancellationToken
            )
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            using IDbConnection db = GetConnection();

            DynamicParameters par = new();
            par.Add("@CreditorNumber", creditorNumber);
            par.Add("@pagenum", pageNumber);
            par.Add("@search", search);
            par.Add("@pagesize", pageSize);

            var multi = await db.QueryMultipleAsync(
                sql: "SP_SupplyOrder_Proveedor_selecciona ",
                param: par,
                commandType: CommandType.StoredProcedure
                );


            var pager = (await multi.ReadAsync<Pager>()).FirstOrDefault();
            var items = (await multi.ReadAsync<SupplyOrderDto>()).ToList();

            return new PagedResult<SupplyOrderDto>(
                items: items,
                totalItems: pager?.TotalItems ?? items.Count,
                pageNumber: pager?.CurrentPage ?? pageNumber,
                pageSize: pager?.PageSize ?? pageSize
                );
        }

        public async Task<PagedResult<SupplyOrderDto>> GetListPaginatedSupplyOrderAsync(
            string token,
            int pageSize,
            int pageNumber,
            string search,
            CancellationToken cancellationToken
            )
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            using IDbConnection db = GetConnection();

            DynamicParameters par = new();
            par.Add("@Token", token);
            par.Add("@pagenum", pageNumber);
            par.Add("@search", search);
            par.Add("@pagesize", pageSize);

            var multi = await db.QueryMultipleAsync(
                sql: "SP_SupplyOrder_Interno_selecciona",
                param: par,
                commandType: CommandType.StoredProcedure
                );

            var pager = (await multi.ReadAsync<Pager>()).FirstOrDefault();
            var items = (await multi.ReadAsync<SupplyOrderDto>()).ToList();

            return new PagedResult<SupplyOrderDto>(
                items: items,
                totalItems: pager?.TotalItems ?? items.Count,
                pageNumber: pager?.CurrentPage ?? pageNumber,
                pageSize: pager?.PageSize ?? pageSize
                );
        }
        #endregion
    }
}

using BERecepcion.Core.Dto;
using BERecepcion.Core.Facturas.Dto;
using BERecepcion.Core.Facturas.Interfaces.Repositories;
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
    public class InvoiceCxPRepository : BaseSQLServerSqlRepository, IInvoiceCxPRepository
    {

        public InvoiceCxPRepository(string cnnString) : base(cnnString)
        {
        }

        public async Task<DataResult<IEnumerable<InvoiceCxPList>>> GetInvoiceCxPAsync(int pageSize, int pageNum = 1)
        {
            DataResult<IEnumerable<InvoiceCxPList>> resultItem = new DataResult<IEnumerable<InvoiceCxPList>>()
            {
                Status = System.Net.HttpStatusCode.OK
            };
            try
            {
                using (IDbConnection db = GetConnection())
                {

                    DynamicParameters par = new DynamicParameters();
                    par.Add("@pagenum", pageNum);
                    par.Add("@pagesize", pageSize);

                    var result = await db.QueryMultipleAsync(sql: "SP_InvoiceCxPList_selecciona ", param: par, commandType: CommandType.StoredProcedure);
                    var paging = await result.ReadAsync<Pager>();
                    var reception = await result.ReadAsync<InvoiceCxPList>();

                    resultItem.Data = reception;
                    resultItem.Pager = paging.FirstOrDefault();

                }
                return resultItem;
            }
            catch (Exception)
            {
                throw;
            }
        }


        public async Task<DataResult<IEnumerable<CXPDto>>> GetInvoiceCxPAPAsync()
        {
            DataResult<IEnumerable<CXPDto>> resultItem = new DataResult<IEnumerable<CXPDto>>()
            {
                Status = System.Net.HttpStatusCode.OK
            };
            try
            {
                using (IDbConnection db = GetConnection())
                {


                    var result = await db.QueryMultipleAsync(sql: "SP_InvoiceCxP_selecciona ", commandType: CommandType.StoredProcedure);
                    var InvoiceCxP = await result.ReadAsync<CXPDto>();

                    resultItem.Data = InvoiceCxP;

                }
                return resultItem;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<DataResult<IEnumerable<InvoiceCxPList>>> GetInvoiceCxPFiltroAsync(string Filtro, int pageSize, int pageNum = 1)
        {
            DataResult<IEnumerable<InvoiceCxPList>> resultItem = new DataResult<IEnumerable<InvoiceCxPList>>()
            {
                Status = System.Net.HttpStatusCode.OK
            };
            try
            {
                using (IDbConnection db = GetConnection())
                {

                    DynamicParameters par = new DynamicParameters();

                    par.Add("@pagenum", pageNum);
                    par.Add("@pagesize", pageSize);
                    par.Add("Filtro", Filtro);
                    var result = await db.QueryMultipleAsync(sql: "SP_InvoiceCxPList_filtro_selecciona ", param: par, commandType: CommandType.StoredProcedure);

                    var paging = await result.ReadAsync<Pager>();
                    var reception = await result.ReadAsync<InvoiceCxPList>();

                    resultItem.Data = reception;
                    resultItem.Pager = paging.FirstOrDefault();
                    return resultItem;

                }

            }
            catch (Exception)
            {
                throw;
            }
        }

    }
}

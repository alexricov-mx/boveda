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
    public class InvoiceAPCxPRepository : BaseSQLServerSqlRepository, IInvoiceAPCxPRepository
    {
        public InvoiceAPCxPRepository(string cnnString) : base(cnnString)
        {
        }

        public async Task<DataResult<IEnumerable<InvoiceAPCxPList>>> GetInvoiceAPCxPAsync(int pageSize, int pageNum = 1)
        {
            DataResult<IEnumerable<InvoiceAPCxPList>> resultItem = new DataResult<IEnumerable<InvoiceAPCxPList>>()
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

                    var result = await db.QueryMultipleAsync(sql: "SP_InvoiceAPCxPList_selecciona ", param: par, commandType: CommandType.StoredProcedure);
                    var paging = await result.ReadAsync<Pager>();
                    var InvoiceapCxP = await result.ReadAsync<InvoiceAPCxPList>();

                    resultItem.Pager = paging.FirstOrDefault();
                    resultItem.Data = InvoiceapCxP;

                }
                return resultItem;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<DataResult<IEnumerable<InvoiceAPCxPList>>> GetInvoiceAPCxPFiltroAsync(string Filtro, int pageSize, int pageNum = 1)
        {
            DataResult<IEnumerable<InvoiceAPCxPList>> resultItem = new DataResult<IEnumerable<InvoiceAPCxPList>>()
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
                    var result = await db.QueryMultipleAsync(sql: "SP_InvoiceAPCxPList_filtro_selecciona ", param: par, commandType: CommandType.StoredProcedure);

                    var paging = await result.ReadAsync<Pager>();
                    var InvoiceapCxP = await result.ReadAsync<InvoiceAPCxPList>();

                    resultItem.Data = InvoiceapCxP;
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

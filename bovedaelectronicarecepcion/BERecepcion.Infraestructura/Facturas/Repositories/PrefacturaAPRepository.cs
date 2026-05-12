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

    public class PrefacturaAPRepository : BaseSQLServerSqlRepository, IPrefacturaAPRepository
    {
        DataResult<IEnumerable<PrefacturaAPDto>> resultItemIenum = new DataResult<IEnumerable<PrefacturaAPDto>>()
        {
            Status = System.Net.HttpStatusCode.OK,
            Message = "Todo bien"
        };

        public PrefacturaAPRepository(string cnnString) : base(cnnString)
        {

        }

        public async Task<DataResult<IEnumerable<PrefacturaAPDto>>> GetPreFacturaAsync(string start, string end, string search, string creditorNumber, Guid userId, int pageSize, int pageNum = 1, bool esDescarga = false)
        {
            if (!string.IsNullOrWhiteSpace(search))
                search = Core.Utils.SearchText.GetWhereClause(search, new List<string> { "Analitico", "Clave", "NumAcreedor", "NumCliente" });

            try
            {

                //using para levantar la conexion al BD
                using (IDbConnection db = GetConnection())
                {
                    DynamicParameters par = new DynamicParameters();
                    par.Add("@CreditorNumber", creditorNumber);
                    par.Add("@start", start);
                    par.Add("@end", end);
                    par.Add("@search", search);
                    par.Add("@pagenum", pageNum);
                    par.Add("@pagesize", pageSize);
                    par.Add("@userId", userId);
                    par.Add("@esDescarga", esDescarga);

                    var result = await db.QueryMultipleAsync(sql: "SP_PreFacturaAP_Selecciona", param: par, commandType: CommandType.StoredProcedure);

                    var paging = await result.ReadAsync<Pager>();
                    var reception = await result.ReadAsync<PrefacturaAPDto>();

                    resultItemIenum.Data = reception;
                    resultItemIenum.Pager = paging.FirstOrDefault();
                }

                return resultItemIenum;
            }
            catch (Exception ext)
            {
                resultItemIenum.Message = ext.Message;
                resultItemIenum.Status = System.Net.HttpStatusCode.BadRequest;

                return resultItemIenum;
            }
        }

        public async Task<DataResult<PrefacturaAPDto>> GetPreFacturaXmlAPAsync(Guid AnaliticoPagoID)
        {

            DataResult<PrefacturaAPDto> resultItemValida = new DataResult<PrefacturaAPDto>()
            {
                Status = System.Net.HttpStatusCode.OK,
                Message = "Todo bien"
            };

            try
            {

                DynamicParameters par = new DynamicParameters();
                par.Add("@AnaliticoPagoID", AnaliticoPagoID);
                //using para levantar la conexion al BD
                using (IDbConnection db = GetConnection())
                {
                    //var result = await db.QueryAsync(sql: "SP_adefas_selecciona", commandType: CommandType.StoredProcedure);
                    var result = await db.QueryFirstAsync<PrefacturaAPDto>(sql: "SP_PreFacturaAPXml_selecciona", param: par, commandType: CommandType.StoredProcedure);

                    resultItemValida.Data = result;

                    return resultItemValida;
                }
            }
            catch (Exception ext)
            {
                resultItemValida.Message = ext.Message;
                resultItemValida.Status = System.Net.HttpStatusCode.BadRequest;

                return resultItemValida;
            }
            // llamado al storedProcedured -> cachas la respuesta del SP
            //throw new NotImplementedException();
        }

        public async Task<DataResult<IEnumerable<PrefacturaAPDto>>> GetPreFacturaXmlAPMasAsync(IEnumerable<PrefacturaAPDto> dto)
        {

            DataResult<IEnumerable<PrefacturaAPDto>> resultItemValida = new DataResult<IEnumerable<PrefacturaAPDto>>()
            {
                Status = System.Net.HttpStatusCode.OK,
                Message = "Todo bien"
            };

            try
            {

                DynamicParameters par = new DynamicParameters();

                //using para levantar la conexion al BD
                using (IDbConnection db = GetConnection())
                {
                    List<PrefacturaAPDto> data = new List<PrefacturaAPDto>();

                    var xmlNot = "";
                    foreach (var item in dto)
                    {
                        par.Add("@AnaliticoPagoID", item.AnaliticoPagoID);
                        var result = await db.QueryFirstAsync<PrefacturaAPDto>(sql: "SP_PreFacturaAPXml_selecciona", param: par, commandType: CommandType.StoredProcedure);
                        data.Add(result);
                        if (result.NotaCreditoXML != null)
                            xmlNot = xmlNot + result.NotaCreditoXML + ",";

                    }

                    resultItemValida.Data = data;

                    return resultItemValida;
                }
            }
            catch (Exception ext)
            {
                resultItemValida.Message = ext.Message;
                resultItemValida.Status = System.Net.HttpStatusCode.BadRequest;

                return resultItemValida;
            }

        }

    }
}

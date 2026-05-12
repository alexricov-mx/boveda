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
    public class PreFacturaRepository : BaseSQLServerSqlRepository, IPreFacturaRepository
    {

        DataResult<IEnumerable<PreFacturaDto>> resultItemIenum = new DataResult<IEnumerable<PreFacturaDto>>()
        {
            Status = System.Net.HttpStatusCode.OK,
            Message = "Todo bien"
        };

        public PreFacturaRepository(string cnnString) : base(cnnString)
        {

        }

        public async Task<DataResult<IEnumerable<PreFacturaDto>>> GetPreFacturaAsync(string start, string end, string search, string creditorNumber, Guid userId, int pageSize, int pageNum = 1, bool esDescarga = false)
        {

            if (!string.IsNullOrWhiteSpace(search))
                search = Core.Utils.SearchText.GetWhereClause(search, new List<string> { "SapOrder", "Reception", "Clave", "Creditor", "CreditorNumber" });

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

                    var result = await db.QueryMultipleAsync(sql: "SP_PreFactura_Selecciona", param: par, commandType: CommandType.StoredProcedure);

                    var paging = await result.ReadAsync<Pager>();
                    var reception = await result.ReadAsync<PreFacturaDto>();

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


        public async Task<DataResult<PreFacturaDto>> GetPreFacturaXmlAsync(Guid CopadeID)
        {

            DataResult<PreFacturaDto> resultItemValida = new DataResult<PreFacturaDto>()
            {
                Status = System.Net.HttpStatusCode.OK,
                Message = "Todo bien"
            };

            try
            {

                DynamicParameters par = new DynamicParameters();
                par.Add("@CopadeID", CopadeID);
                //using para levantar la conexion al BD
                using (IDbConnection db = GetConnection())
                {
                    var result = await db.QueryFirstAsync<PreFacturaDto>(sql: "SP_PreFacturaXml_selecciona", param: par, commandType: CommandType.StoredProcedure);

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

        }

        public async Task<DataResult<IEnumerable<PreXmlMasDto>>> GetPreFacturaXmlMasAsync(IEnumerable<PreXmlMasDto> dto)
        {

            DataResult<IEnumerable<PreXmlMasDto>> resultItemValida = new DataResult<IEnumerable<PreXmlMasDto>>()
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
                    List<PreXmlMasDto> data = new List<PreXmlMasDto>();


                    var xmlFac = "";
                    var xmlNot = "";
                    foreach (var item in dto)
                    {
                        if (item.Status)
                        {
                            par.Add("@CopadeID", item.CopadeID);
                            var result = await db.QueryFirstAsync<PreXmlMasDto>(sql: "SP_PreFacturaXml_selecciona", param: par, commandType: CommandType.StoredProcedure);
                            data.Add(result);

                            xmlFac = xmlFac + result.PreFacturaXML + ",";

                            if (result.NotaCreditoXML != null)
                                xmlNot = xmlNot + result.NotaCreditoXML + ",";
                        }
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

        public async Task<DataResult<IEnumerable<PreFacturaDto>>> GetPreFacturaBuscarAsync(string creditorNumber, int pageSize, int pageNum = 1, string busqueda = null, DateTime? fechaInicial = null, DateTime? fechaFinal = null)
        {

            try
            {

                //using para levantar la conexion al BD
                using (IDbConnection db = GetConnection())
                {
                    DynamicParameters par = new DynamicParameters();
                    par.Add("@CreditorNumber", creditorNumber);
                    par.Add("@FechaInicial", fechaInicial);
                    par.Add("@FechaFinal", fechaFinal);
                    par.Add("@Busqueda", busqueda);
                    par.Add("@pagenum", pageNum);
                    par.Add("@pagesize", pageSize);

                    var result = await db.QueryMultipleAsync(sql: "SP_PreFactura_Busqueda_Selecciona", param: par, commandType: CommandType.StoredProcedure);

                    var paging = await result.ReadAsync<Pager>();
                    var reception = await result.ReadAsync<PreFacturaDto>();

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


    }
}

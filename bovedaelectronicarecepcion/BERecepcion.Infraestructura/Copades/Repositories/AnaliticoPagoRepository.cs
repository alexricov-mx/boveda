using BERecepcion.Core.Admin.Dto;
using BERecepcion.Core.Consulta.Copades.Dto;
using BERecepcion.Core.Copades.Interfaces.Repositories;
using BERecepcion.Core.Dto;
using BERecepcion.Core.Facturas.Dto;
using BERecepcion.Core.Models;
using BERecepcion.Infraestructura.Repositories;
using Dapper;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace BERecepcion.Infraestructura.Copades.Repositories
{
    public class AnaliticoPagoRepository : BaseSQLServerSqlRepository, IAnaliticoPagoRepository
    {
        public AnaliticoPagoRepository(string cnnString) : base(cnnString)
        {
        }
        public async Task<DataResult<IEnumerable<AnaliticoPagoDto>>> ConsultaAPGetAllAsync(int pageSize, string ficha, int pageNum = 1, string search = null)
        {
            DataResult<IEnumerable<AnaliticoPagoDto>> resultItem = new DataResult<IEnumerable<AnaliticoPagoDto>>()
            {
                Status = System.Net.HttpStatusCode.OK,
            };
            try
            {
                using (IDbConnection db = GetConnection())
                {
                    DynamicParameters par = new DynamicParameters();
                    par.Add("@ficha", ficha);
                    par.Add("@search", search);
                    par.Add("@pagenum", pageNum);
                    par.Add("@pagesize", pageSize);

                    var result = await db.QueryMultipleAsync(sql: "SP_AnaliticoPago_selecciona_filtro", param: par, commandType: CommandType.StoredProcedure);

                    var paging = await result.ReadAsync<Pager>();
                    var apresult = await result.ReadAsync<AnaliticoPagoDto>();
                    resultItem.Pager = paging.FirstOrDefault();
                    resultItem.Data = apresult;
                    return resultItem;
                }
            }
            catch (Exception ex)
            {
                resultItem.Message = $"Ocurrio un problema. Contacta a tu administrador. {ex.Message}";
                resultItem.Status = System.Net.HttpStatusCode.BadRequest;
                return resultItem;
            }
        }
        public async Task<DataResult<IEnumerable<UsersDto>>> GetEmailAP(Guid AnaliticoPagoID)
        {
            DataResult<IEnumerable<UsersDto>> resultItem = new DataResult<IEnumerable<UsersDto>>()
            {
                Status = System.Net.HttpStatusCode.OK,
            };

            try
            {
                //using para levantar la conexion al BD
                using (IDbConnection db = GetConnection())
                {
                    DynamicParameters par = new DynamicParameters();
                    par.Add("@AnaliticoPagoID", AnaliticoPagoID);

                    var result = await db.QueryAsync<UsersDto>(sql: "SP_AnaliticoPago_signer_email", param: par, commandType: CommandType.StoredProcedure);

                    resultItem.Data = result;
                    return resultItem;
                }
            }
            catch (Exception ex)
            {
                resultItem.Message = $"Ocurrio un problema. Contacta a tu administrador. {ex.Message}";
                resultItem.Status = System.Net.HttpStatusCode.BadRequest;
                return resultItem;
            }
        }

        public async Task<DataResult<AnaliticoPagoDto>> GetAPByIdAnaliticoAsync(string idAnalitico)
        {
            DataResult<AnaliticoPagoDto> resultItem = new DataResult<AnaliticoPagoDto>()
            {
                Status = System.Net.HttpStatusCode.OK,
            };

            try
            {
                //using para levantar la conexion al BD
                using (IDbConnection db = GetConnection())
                {
                    DynamicParameters par = new DynamicParameters();
                    par.Add("@IdAnalitico", idAnalitico);

                    var result = await db.QueryFirstOrDefaultAsync<AnaliticoPagoDto>(sql: "SP_AnaliticoPago_byIdAnalitico", param: par, commandType: CommandType.StoredProcedure);
                    if (result == null)
                    {
                        resultItem.Message = $"No se encontro el Analitico de Pago {idAnalitico}";
                        resultItem.Status = System.Net.HttpStatusCode.NotFound;
                        return resultItem;
                    }

                    resultItem.Data = result;
                    resultItem.Data.vPreFactura = JsonConvert.DeserializeObject<APPreFactura>(result.PreFactura);
                    resultItem.Data.vImpuestos = JsonConvert.DeserializeObject<APImpuestos>(result.Impuestos);
                    resultItem.Data.vAddenda = JsonConvert.DeserializeObject<APAddenda>(result.Addenda);
                    return resultItem;
                }
            }
            catch (Exception ex)
            {
                resultItem.Message = $"Ocurrio un problema. Contacta a tu administrador. {ex.Message}";
                resultItem.Status = System.Net.HttpStatusCode.BadRequest;
                return resultItem;
            }
        }

        public async Task<DataResult<AnaliticoPagoDto>> APFirmaAsync(Guid AnaliticoPagoID, string Token)
        {
            DataResult<AnaliticoPagoDto> resultItem = new DataResult<AnaliticoPagoDto>()
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
                        par.Add("@AnaliticoPagoID", AnaliticoPagoID);
                        par.Add("@Token", Token);
                        await db.QueryAsync(sql: "SP_AnaliticoPago_firma", param: par, commandType: CommandType.StoredProcedure);
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

        public async Task<DataResult<AnaliticoPagoDto>> APFirmaCorreoAsync(Guid AnaliticoPagoID, IEnumerable<UsersDto> users)
        {
            DataResult<AnaliticoPagoDto> resultItem = new DataResult<AnaliticoPagoDto>()
            {
                Status = System.Net.HttpStatusCode.OK,
                Message = "Firmado exitosamente"
            };

            try
            {
                var usersList = string.Empty;
                foreach (var user in users)
                {
                    usersList += $"{user.Email},";
                }

                using (IDbConnection db = GetConnection())
                {
                    DynamicParameters par = new DynamicParameters();
                    par.Add("@AnaliticoPagoID", AnaliticoPagoID);
                    par.Add("@Email", usersList);
                    await db.QueryAsync(sql: "SP_AnaliticoPago_firma_correo", param: par, commandType: CommandType.StoredProcedure);
                    resultItem.Message = "Se actualizaron los datos correctamente";
                }
                return resultItem;
            }
            catch (Exception ex)
            {
                resultItem.Message = ex.Message;
                resultItem.Status = System.Net.HttpStatusCode.BadRequest;
                return resultItem;
            }
        }

        public async Task<DataResult<AnaliticoPagoDto>> GetAPByIdAnaliticoClaveAsync(string idAnalitico, string clave)
        {
            DataResult<AnaliticoPagoDto> resultItem = new DataResult<AnaliticoPagoDto>()
            {
                Status = System.Net.HttpStatusCode.OK,
            };

            try
            {
                //using para levantar la conexion al BD
                using (IDbConnection db = GetConnection())
                {
                    DynamicParameters par = new DynamicParameters();
                    par.Add("@Clave", clave);
                    par.Add("@IdAnalitico", idAnalitico);

                    var result = await db.QueryFirstOrDefaultAsync<AnaliticoPagoDto>(sql: "SP_AnaliticoPago_byIdAnaliticoClave", param: par, commandType: CommandType.StoredProcedure);
                    if (result == null)
                    {
                        resultItem.Message = $"No se encontro el Analitico de Pago {idAnalitico}";
                        resultItem.Status = System.Net.HttpStatusCode.NotFound;
                        return resultItem;
                    }

                    resultItem.Data = result;
                    resultItem.Data.vPreFactura = JsonConvert.DeserializeObject<APPreFactura>(result.PreFactura);
                    resultItem.Data.vImpuestos = JsonConvert.DeserializeObject<APImpuestos>(result.Impuestos);
                    resultItem.Data.vAddenda = JsonConvert.DeserializeObject<APAddenda>(result.Addenda);
                    return resultItem;
                }
            }
            catch (Exception ex)
            {
                resultItem.Message = $"Ocurrio un problema. Contacta a tu administrador. {ex.Message}";
                resultItem.Status = System.Net.HttpStatusCode.BadRequest;
                return resultItem;
            }
        }

        public async Task<ComprobanteDto> GetXMLAPAsync(Guid InvoiceId)
        {
            ComprobanteDto resultItem = new ComprobanteDto();

            try
            {
                //using para levantar la conexion al BD
                using (IDbConnection db = GetConnection())
                {
                    DynamicParameters par = new DynamicParameters();
                    par.Add("@InvoiceId", InvoiceId);


                    var result = await db.QueryMultipleAsync(sql: "InvoiceXML", param: par, commandType: CommandType.StoredProcedure);

                    var apresult = await result.ReadAsync<ComprobanteDto>();
                    string datos = apresult.FirstOrDefault().OriginalXml;

                    //resultItem.PreFacturaXML= datos;
                    resultItem.OriginalXml = datos;
                    return resultItem;
                }
            }
            catch (Exception) // ex)
            {
                //resultItem.Message = $"Ocurrio un problema. Contacta a tu administrador. {ex.Message}";
                //resultItem.Status = System.Net.HttpStatusCode.BadRequest;
                return resultItem;
            }
        }
        public async Task<ComprobanteDto> GetXMLAPAsync2(Guid NotaCreditoId)
        {
            ComprobanteDto resultItem = new ComprobanteDto();

            try
            {
                //using para levantar la conexion al BD
                using (IDbConnection db = GetConnection())
                {
                    DynamicParameters par = new DynamicParameters();
                    par.Add("@NotaCreditoId", NotaCreditoId);


                    var result = await db.QueryMultipleAsync(sql: "NotaCreditoXML", param: par, commandType: CommandType.StoredProcedure);

                    var apresult = await result.ReadAsync<ComprobanteDto>();
                    string datos = apresult.FirstOrDefault().OriginalXml;

                    //resultItem.PreFacturaXML= datos;
                    resultItem.OriginalXml = datos;
                    return resultItem;
                }
            }
            catch (Exception)// ex)
            {
                //resultItem.Message = $"Ocurrio un problema. Contacta a tu administrador. {ex.Message}";
                //resultItem.Status = System.Net.HttpStatusCode.BadRequest;
                return resultItem;
            }
        }
        public async Task<ComprobanteDto> GetXMLAPAsync3(Guid PagosId)
        {
            ComprobanteDto resultItem = new ComprobanteDto();

            try
            {
                //using para levantar la conexion al BD
                using (IDbConnection db = GetConnection())
                {
                    DynamicParameters par = new DynamicParameters();
                    par.Add("@PagosID", PagosId);


                    var result = await db.QueryMultipleAsync(sql: "ComprobanteXML", param: par, commandType: CommandType.StoredProcedure);

                    var apresult = await result.ReadAsync<ComprobanteDto>();
                    string datos = apresult.FirstOrDefault().OriginalXml;

                    //resultItem.PreFacturaXML= datos;
                    resultItem.OriginalXml = datos;
                    return resultItem;
                }
            }
            catch (Exception) // ex)
            {
                //resultItem.Message = $"Ocurrio un problema. Contacta a tu administrador. {ex.Message}";
                //resultItem.Status = System.Net.HttpStatusCode.BadRequest;
                return resultItem;
            }
        }

        public async Task<DataResult<IEnumerable<AnaliticoPagoDto>>> GetAPAsync(int pageSize, string ficha, int pageNum = 1, DateTime? fechaInicial = null, DateTime? fechaFinal = null, string search = null, bool esDescarga = false)
        {
            DataResult<IEnumerable<AnaliticoPagoDto>> resultItem = new DataResult<IEnumerable<AnaliticoPagoDto>>()
            {
                Status = System.Net.HttpStatusCode.OK,
            };

            try
            {
                //using para levantar la conexion al BD
                using (IDbConnection db = GetConnection())
                {
                    DynamicParameters par = new DynamicParameters();
                    par.Add("@pagenum", pageNum);
                    par.Add("@pagesize", pageSize);
                    par.Add("@ficha", ficha);
                    par.Add("@fechaInicial", fechaInicial);
                    par.Add("@fechaFinal", fechaFinal);
                    par.Add("@search", search);
                    par.Add("@esDescarga", esDescarga);


                    var result = await db.QueryMultipleAsync(sql: "SP_AnaliticoPago_Consulta_selecciona", param: par, commandType: CommandType.StoredProcedure);

                    var paging = await result.ReadAsync<Pager>();
                    var apresult = await result.ReadAsync<AnaliticoPagoDto>();

                    resultItem.Pager = paging.FirstOrDefault();

                    resultItem.Data = apresult;
                    return resultItem;
                }
            }
            catch (Exception ex)
            {
                resultItem.Message = $"Ocurrio un problema. Contacta a tu administrador. {ex.Message}";
                resultItem.Status = System.Net.HttpStatusCode.BadRequest;
                return resultItem;
            }
        }
    }
}


using BERecepcion.Core.Consulta.Copades.Dto;
using BERecepcion.Core.Dto;
using BERecepcion.Core.Facturas.Dto;
using BERecepcion.Core.Instrucciones.Dto;
using BERecepcion.Core.OrdenSurtimiento.Dto;
using BERecepcion.Core.SAPPI.Dto;
using BERecepcion.Core.SAPPI.Interfaces.Repositories;
using BERecepcion.Core.Utils;
using BERecepcion.Infraestructura.Repositories;
using BERecepcion.Infraestructura.Utils;
using Dapper;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace BERecepcion.Infraestructura.SAPPI.Repositories
{
    public class SAPPIRepository : BaseSQLServerSqlRepository, ISAPPIRepository
    {
        private readonly IConfiguration _configuration;
        public SAPPIRepository(string cnnString, IConfiguration configuration) : base(cnnString)
        {
            _configuration = configuration;
        }
        public async Task<DataResult<OSResponseDto>> PostOS_Response(OSResponseDto dto)
        {
            DataResult<OSResponseDto> resultItem = new DataResult<OSResponseDto>()
            {
                Status = System.Net.HttpStatusCode.OK,
                Message = "Envío Exitoso a SAP PI"
            };

            var client = new RestClient(_configuration["PI:OS_Response"]);
            var request = new RestRequest("", Method.Post);
            request.AddHeader("content-type", "application/json");
            request.AddHeader("authorization", $"Basic {Utils.Encoding.Base64Encode(_configuration["PI:usuario_pass"])}");

            // Convertimos a JSON el dto que se recibe
            var mensaje = JsonConvert.SerializeObject(dto);
            request.AddParameter("application/json", mensaje, ParameterType.RequestBody);
            var response = await client.ExecuteAsync(request);

            // Se Deserealiza la respuesta
            try
            {
                OSResponseItemDto resp = JsonConvert.DeserializeObject<OSResponseItemDto>(response.Content);
                OSResponseDto respDto = new OSResponseDto();
                respDto.item = resp;
                resultItem.Data = respDto;

                return resultItem;
            }
            catch (Exception)
            {
                resultItem.Message = $"Ocurrio un problema. Contacta a tu administrador. {response.Content}";
                resultItem.Status = System.Net.HttpStatusCode.BadRequest;
                return resultItem;
            }

        }
        public async Task<DataResult<REResponseItemDto>> PostRE_Response(REResponseDto dto)
        {
            DataResult<REResponseItemDto> resultItem = new DataResult<REResponseItemDto>()
            {
                Status = System.Net.HttpStatusCode.OK,
                Message = "Envío Exitoso a SAP PI"
            };

            var client = new RestClient(_configuration["PI:RE_Response"]);
            var request = new RestRequest("",Method.Post);
            request.AddHeader("content-type", "application/json");
            request.AddHeader("authorization", $"Basic {Utils.Encoding.Base64Encode(_configuration["PI:usuario_pass"])}");

            // Convertimos a JSON el dto que se recibe
            var mensaje = JsonConvert.SerializeObject(dto);
            request.AddParameter("application/json", mensaje, ParameterType.RequestBody);
            var response = await client.ExecuteAsync(request);

            // Se Deserealiza la respuesta
            try
            {
                var resp = JsonConvert.DeserializeObject<REResponseItemDto>(response.Content);
                resultItem.Data = resp;

                return resultItem;
            }
            catch (Exception)
            {
                resultItem.Message = $"Ocurrio un problema. Contacta a tu administrador. {response.Content}";
                resultItem.Status = System.Net.HttpStatusCode.BadRequest;
                return resultItem;
            }
        }
        public async Task<DataResult<ValidaMiro_responseDto>> PostValidacionCxP(ValidacionCXPDto dto)
        {
            DataResult<ValidaMiro_responseDto> resultItem = new DataResult<ValidaMiro_responseDto>()
            {
                Status = System.Net.HttpStatusCode.OK,
                Message = "Envío Exitoso a SAP PI"
            };

            var client = new RestClient(_configuration["PI:ValidaMiro"]);
            var request = new RestRequest("", Method.Post);
            request.AddHeader("content-type", "application/json");
            request.AddHeader("authorization", $"Basic {Utils.Encoding.Base64Encode(_configuration["PI:usuario_pass"])}");

            // Convertimos a JSON el dto que se recibe
            var mensaje = JsonConvert.SerializeObject(dto);
            request.AddParameter("application/json", mensaje, ParameterType.RequestBody);
            var response = await client.ExecuteAsync(request);

            // Se Deserealiza la respuesta
            try
            {
                ValidaMiro_responseDto resp = JsonConvert.DeserializeObject<ValidaMiro_responseDto>(response.Content);

                resultItem.Data = resp;

                return resultItem;
            }
            catch (Exception)
            {
                resultItem.Message = $"Ocurrio un problema. Contacta a tu administrador. {response.Content}";
                resultItem.Status = System.Net.HttpStatusCode.BadRequest;
                return resultItem;
            }
        }
        public async Task<DataResult<CXPDto>> PostCxP(CXPDto dto)
        {
            DataResult<CXPDto> resultItem = new DataResult<CXPDto>()
            {
                Status = System.Net.HttpStatusCode.OK,
                Message = "Envío Exitoso a SAP PI"
            };

            var client = new RestClient(_configuration["PI:CXP"]);
            var request = new RestRequest("",Method.Post);
            request.AddHeader("content-type", "application/json");
            request.AddHeader("authorization", $"Basic {Utils.Encoding.Base64Encode(_configuration["PI:usuario_pass"])}");

            // Convertimos a JSON el dto que se recibe
            var mensaje = JsonConvert.SerializeObject(dto);
            request.AddParameter("application/json", mensaje, ParameterType.RequestBody);
            try
            {
                var response = await client.ExecuteAsync(request);

                // Se Deserealiza la respuesta
                switch (response.StatusCode)
                {
                    case System.Net.HttpStatusCode.OK:
                        try
                        {
                            //  Terminamos
                            //  si tiene FI terminamos
                            //      Es porque hay un problema de negocio
                            //  si no tiene FI
                            //      Se manda a un estatus de Pendiente de CxP (no tiene reintentos)
                            //      Esta factura se le dara seguimiento via la funcionalidad de Envio Manual de CxP
                            CXPDto resp = JsonConvert.DeserializeObject<CXPDto>(response.Content);
                            resultItem.Data = resp;
                        }
                        catch (Exception)
                        {
                            resultItem.Message = $"Ocurrio un problema. Contacta a tu administrador. {response.Content}";
                            resultItem.Status = System.Net.HttpStatusCode.BadRequest;
                            return resultItem;
                        }
                        break;
                    case System.Net.HttpStatusCode.InternalServerError:
                        //  Entrar al esquema de reintentos.
                        //  Background Service
                        //  3 reintentos cada 15 mins
                        //  Estatus intermedio
                        resultItem.Status = System.Net.HttpStatusCode.InternalServerError;
                        resultItem.Message = "Ocurrio un problema.";
                        break;
                    default:
                        resultItem.Status = System.Net.HttpStatusCode.BadRequest;
                        resultItem.Message = "Ocurrio un problema.";
                        break;
                }
            }
            catch (Exception)
            {
                resultItem.Status = System.Net.HttpStatusCode.InternalServerError;
                resultItem.Message = "Ocurrio un problema.";
            }
            return resultItem;

        }
        public async Task<DataResult<string>> InsertaCopadeAsync(CopadeDto copade)
        {
            DataResult<string> resultItem = new DataResult<string>()
            {
                Status = System.Net.HttpStatusCode.OK,
                Message = "Envío Exitoso a SAP PI"
            };

            // Parametros
            try
            {
                DynamicParameters par = new DynamicParameters();
                par.Add("@clave", copade.clave);
                par.Add("@Contract", copade.Contract);
                par.Add("@SapOrder", copade.SapOrder);
                par.Add("@Reception", copade.Reception);
                par.Add("@Exercise", copade.Exercise);
                par.Add("@DocumentType", copade.DocumentType);
                par.Add("@Center", copade.Center);
                par.Add("@FechaEmision", copade.FechaEmision);
                par.Add("@Ficha", copade.Ficha);
                par.Add("@Agree", copade.Agree);
                par.Add("@Authorizes", copade.Authorizes);
                par.Add("@Creditor", copade.Creditor);
                par.Add("@CreditorBanking", copade.CreditorBanking);
                par.Add("@CreditorNumber", copade.CreditorNumber);
                par.Add("@CreditorRfc", copade.CreditorRfc);
                par.Add("@Rfc", copade.Rfc);
                par.Add("@Society", copade.Society);
                par.Add("@CertificateNumber", copade.CertificateNumber);
                par.Add("@ChangeType", copade.ChangeType);
                par.Add("@Comments", copade.Comments);
                par.Add("@Currency", copade.Currency);
                par.Add("@Amount", copade.Amount);
                par.Add("@EsTRI", copade.EsTRI);
                par.Add("@Receipt", copade.Receipt);
                par.Add("@Iva", copade.Iva);
                par.Add("@PayEquity", copade.PayEquity);
                par.Add("@PointAmount", copade.PointAmount);
                par.Add("@ReleaseDate", copade.ReleaseDate);
                par.Add("@Subtotal", copade.Subtotal);
                par.Add("@Total", copade.Total);
                par.Add("@NoCad", copade.NoCad);
                par.Add("@Assignment", copade.Assignment);
                par.Add("@Desc", copade.Desc);
                par.Add("@Esteem", copade.Esteem);
                par.Add("@Macroproject", copade.Macroproject);
                par.Add("@RefPayment", copade.RefPayment);
                par.Add("@Trust", copade.Trust);
                par.Add("@AcceptDocument", copade.AcceptDocument);
                par.Add("@ApplicablePunishment", copade.ApplicablePunishment);
                par.Add("@DeliveryLimitDate", copade.DeliveryLimitDate);
                par.Add("@DeliveryRealDate", copade.DeliveryRealDate);
                par.Add("@DiferenceDays", copade.DiferenceDays);
                par.Add("@Division", copade.Division);
                par.Add("@EntryForm", copade.EntryForm);
                par.Add("@EstimatedNumber", copade.EstimatedNumber);
                par.Add("@EstimationPeriod", copade.EstimationPeriod);
                par.Add("@Pospre", copade.Pospre);
                par.Add("@TaxSubtotal", copade.TaxSubtotal);
                par.Add("@TaxTotal", copade.TaxTotal);
                par.Add("@TaxTotalRetained", copade.TaxTotalRetained);
                par.Add("@TaxTotalTransferred", copade.TaxTotalTransferred);
                par.Add("@TaxTotalLocalRetained", copade.TaxTotalLocalRetained);
                par.Add("@TaxTotalLocalTransferred", copade.TaxTotalLocalTransferred);
                par.Add("@Detalle", JsonConvert.SerializeObject(copade.vDetalle));
                par.Add("@PreFactura", JsonConvert.SerializeObject(copade.vPreFactura));
                par.Add("@Addenda", JsonConvert.SerializeObject(copade.vAddenda));

                if (!JsonConvert.SerializeObject(copade.vDetalleAvion).Equals("null"))
                    par.Add("@DetalleAvion", JsonConvert.SerializeObject(copade.vDetalleAvion));
                else
                    par.Add("@DetalleAvion", null);

                if (!JsonConvert.SerializeObject(copade.vDetalleObra).Equals("null"))
                    par.Add("@DetalleObra", JsonConvert.SerializeObject(copade.vDetalleObra));
                else
                    par.Add("@DetalleObra", null);

                if (!JsonConvert.SerializeObject(copade.vDetallePep).Equals("null"))
                    par.Add("@DetallePep", JsonConvert.SerializeObject(copade.vDetallePep));
                else
                    par.Add("@DetallePep", null);

                if (!JsonConvert.SerializeObject(copade.vImpuestos).Equals("null"))
                    par.Add("@Impuestos", JsonConvert.SerializeObject(copade.vImpuestos));
                else
                    par.Add("@Impuestos", null);

                if (!JsonConvert.SerializeObject(copade.vComplemento).Equals("null"))
                    par.Add("@Complemento", JsonConvert.SerializeObject(copade.vComplemento));
                else
                    par.Add("@Complemento", null);

                if (!JsonConvert.SerializeObject(copade.vNotasCredito).Equals("null"))
                    par.Add("@NotasCredito", JsonConvert.SerializeObject(copade.vNotasCredito));
                else
                    par.Add("@NotasCredito", null);

                ArmaPreFacturaPemex tmpPF = new ArmaPreFacturaPemex(copade.vPreFactura);
                par.Add("@PreFacturaXML", tmpPF.xmlResult);

                ArmaNotaCreditoPemex tmpNC = new ArmaNotaCreditoPemex(copade.vNotasCredito);
                par.Add("@NotaCreditoXML", tmpNC.xmlResult);

                //using para levantar la conexion al BD
                using (IDbConnection db = GetConnection())
                {
                    var res = await db.QueryFirstAsync<string>(sql: "SP_Copade_interface", param: par, commandType: CommandType.StoredProcedure);
                    if (res == "ACEPTADO")
                    {
                        resultItem.Data = $"Se recibio con exito el copade {copade.Reception}";
                    }
                    else
                    {
                        resultItem.Data = $"Ocurrio un problema con el copade {copade.Reception}";
                        resultItem.Status = System.Net.HttpStatusCode.BadRequest;
                        resultItem.Message = res;
                    }

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
        public async Task<DataResult<string>> InsertaLlaveCopadeAsync(CopadeDto copade)
        {

            DataResult<string> resultItem = new DataResult<string>()
            {
                Status = System.Net.HttpStatusCode.OK,
                Message = "Envío Exitoso a SAP PI"
            };

            try
            {
                // Parametros
                DynamicParameters par = new DynamicParameters();
                par.Add("@clave", copade.clave);
                par.Add("@Contract", copade.Contract);
                par.Add("@SapOrder", copade.SapOrder);
                par.Add("@Reception", copade.Reception);
                par.Add("@Exercise", copade.Exercise);
                par.Add("@Creditor", copade.Creditor);
                par.Add("@CreditorRfc", copade.CreditorRfc);
                par.Add("@CreditorNumber", copade.CreditorNumber);
                par.Add("@Total", copade.Total);
                par.Add("@Ficha", copade.Ficha);
                par.Add("@Center", copade.Center);
                par.Add("@DocumentType", copade.DocumentType);

                //using para levantar la conexion al BD
                using (IDbConnection db = GetConnection())
                {
                    var res = await db.QueryFirstAsync<string>(sql: "SP_LlaveCopade_interface", param: par, commandType: CommandType.StoredProcedure);
                    if (res == "ACEPTADO")
                    {
                        resultItem.Data = $"Se recibio con exito el copade {copade.Reception}";
                    }
                    else
                    {
                        resultItem.Data = $"Ocurrio un problema con el copade {copade.Reception}";
                        resultItem.Status = System.Net.HttpStatusCode.BadRequest;
                        resultItem.Message = res;
                    }

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
        public async Task<DataResult<CopadeDto>> RecuperaCopadeAsync(PICopadeRequestDto copadeReq)
        {
            DataResult<CopadeDto> resultItem = new DataResult<CopadeDto>()
            {
                Status = System.Net.HttpStatusCode.OK,
                Message = "Envío Exitoso RecuperaCopadeAsync"
            };

            try
            {
                var client = new RestClient(_configuration["PI:RecuperaCopade"]);//baseUrl
                var request = new RestRequest("",Method.Post);
                request.AddHeader("content-type", "application/json");
                request.AddHeader("authorization", $"Basic {Utils.Encoding.Base64Encode(_configuration["PI:usuario_pass"])}");
                var mensaje = JsonConvert.SerializeObject(copadeReq);
                request.AddParameter("application/json", mensaje, ParameterType.RequestBody);
                var response = await client.ExecuteAsync(request);

                try
                {
                    CopadeDto resp = JsonConvert.DeserializeObject<CopadeDto>(response.Content);
                    if (resp.Reception == null)
                    {
                        resultItem.Status = System.Net.HttpStatusCode.NoContent;
                        resultItem.Message = "No se pudo obtener informacion del COPADE";
                    }
                    resultItem.Data = resp;
                    return resultItem;
                }
                catch (Exception ex)
                {
                    resultItem.Message = $"Ocurrio un problema. Contacta a tu administrador. {response.Content} " + ex.Message;
                    resultItem.Status = System.Net.HttpStatusCode.BadRequest;
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
        public async Task<DataResult<CopadeDto>> RecuperaCopadeBDAsync(PICopadeRequestDto copadeReq)
        {
            DataResult<CopadeDto> resultItem = new DataResult<CopadeDto>()
            {
                Data = new CopadeDto(),
                Status = System.Net.HttpStatusCode.OK,
                Message = "Se obtuvo el copade con exito de la BD"
            };

            try
            {
                // Parametros
                DynamicParameters par = new DynamicParameters();
                par.Add("@clave", copadeReq.Clave);
                par.Add("@Reception", copadeReq.Reception);
                par.Add("@Exercise", copadeReq.Exercise);

                //using para levantar la conexion al BD
                using (IDbConnection db = GetConnection())
                {
                    var res = await db.QueryFirstOrDefaultAsync<CopadeDto>(sql: "SP_Copade_by_reception", param: par, commandType: CommandType.StoredProcedure);

                    if (res is null)
                    {
                        resultItem.Message = "El COPADE no existe";
                        resultItem.Status = System.Net.HttpStatusCode.BadRequest;
                        return resultItem;
                    }
                    resultItem.Data = res;
                    resultItem.Data.vPreFactura = string.IsNullOrWhiteSpace(res.PreFactura) ? null : JsonConvert.DeserializeObject<CopadePreFactura>(res.PreFactura);
                    resultItem.Data.vAddenda = string.IsNullOrWhiteSpace(res.Addenda) ? null : JsonConvert.DeserializeObject<CopadeAddendaPemex>(res.Addenda);
                    resultItem.Data.vNotasCredito = string.IsNullOrWhiteSpace(res.NotasCredito) ? null : JsonConvert.DeserializeObject<CopadeNotasCredito>(res.NotasCredito);
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
        public async Task<Guid> GetCopadeIdAsync(PICopadeRequestDto copadeReq)
        {
            try
            {
                // Parametros
                DynamicParameters par = new DynamicParameters();
                par.Add("@clave", copadeReq.Clave);
                par.Add("@SapOrder", copadeReq.SapOrder);
                par.Add("@Reception", copadeReq.Reception);
                par.Add("@Exercise", copadeReq.Exercise);

                //using para levantar la conexion al BD
                using (IDbConnection db = GetConnection())
                {
                    return await db.QueryFirstAsync<Guid>(sql: "SP_CopadeGetID_interface", param: par, commandType: CommandType.StoredProcedure);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<DataResult<string>> GetPreFacturaById(Guid CopadeId)
        {
            DataResult<string> resultItem = new DataResult<string>()
            {
                Status = System.Net.HttpStatusCode.OK,
                Message = "Se obtuvo la PreFactura XML con exito"
            };
            try
            {
                DynamicParameters par = new DynamicParameters();
                par.Add("@CopadeId", CopadeId);
                using (IDbConnection db = GetConnection())
                {
                    var copadePreFactura = await db.QueryFirstAsync<string>(sql: "SP_CopadeGetPreFacturaById_interface", param: par, commandType: CommandType.StoredProcedure);
                    ArmaPreFacturaPemex tmp = new ArmaPreFacturaPemex(JsonConvert.DeserializeObject<CopadePreFactura>(copadePreFactura));
                    resultItem.Data = tmp.xmlResult;
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
        public async Task<DataResult<string>> GetPreFacturaAPById(Guid AnaliticoPagoID)
        {
            DataResult<string> resultItem = new DataResult<string>()
            {
                Status = System.Net.HttpStatusCode.OK,
                Message = "Se obtuvo la PreFactura XML con exito"
            };
            try
            {
                DynamicParameters par = new DynamicParameters();
                par.Add("@AnaliticoPagoID", AnaliticoPagoID);
                using (IDbConnection db = GetConnection())
                {
                    var apPreFactura = await db.QueryFirstAsync<string>(sql: "SP_AnaliticoPagoGetPreFacturaById_interface", param: par, commandType: CommandType.StoredProcedure);
                    ArmaPreFacturaAPPemex tmp = new ArmaPreFacturaAPPemex(JsonConvert.DeserializeObject<APPreFactura>(apPreFactura));
                    resultItem.Data = tmp.xmlResult;
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
        public async Task<string> GetLogo(string clave)
        {
            try
            {
                DynamicParameters par = new DynamicParameters();
                par.Add("@clave", clave);
                using (IDbConnection db = GetConnection())
                {
                    return await db.QueryFirstAsync<string>(sql: "SP_CopadeGetLogoByClave", param: par, commandType: CommandType.StoredProcedure);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<DataResult<OSResponseItemDto>> PostOS_SYNC_IB(SupplyOrderDto dto)
        {
            DataResult<OSResponseItemDto> resultItem = new DataResult<OSResponseItemDto>()
            {
                Status = System.Net.HttpStatusCode.OK,
                Message = "Se obtuvo el copade con exito de la BD"
            };

            try
            {
                // Parametros
                DynamicParameters par = new DynamicParameters();
                par.Add("@Contract", dto.Contract);
                par.Add("@SapOrder", dto.SAPOrder);
                par.Add("@SiafOrder", dto.SIAFOrder);
                par.Add("@Type", dto.Type);
                par.Add("@DocumentType", dto.DocumentType);
                par.Add("@Organismo", dto.OrganismClave);
                par.Add("@Signer", dto.Signer);
                par.Add("@Currency", dto.Currency);
                par.Add("@Total", dto.Total);
                par.Add("@Creditor", dto.Creditor);
                par.Add("@CreditorNumber", dto.CreditorNumber);
                par.Add("@CreditorRFC", dto.CreditorRFC);
                par.Add("@Representative", dto.Representative);
                par.Add("@MadeBy", dto.MadeBy);
                if (!JsonConvert.SerializeObject(dto.detalle).Equals("null"))
                    par.Add("@detalle", JsonConvert.SerializeObject(dto.detalle));
                else
                    par.Add("@detalle", null);

                //using para levantar la conexion al BD
                using (IDbConnection db = GetConnection())
                {
                    var res = await db.QueryFirstAsync<OSResponseItemDto>(sql: "SP_SupplyOrder_interface", param: par, commandType: CommandType.StoredProcedure);

                    if (res != null)
                    {
                        resultItem.Data = res;
                        resultItem.Message = $"Se recibio con exito la Orden {dto.SAPOrder}";
                    }
                    else
                    {
                        resultItem.Message = $"Ocurrio un problema con la Orden {dto.SAPOrder}";
                        resultItem.Status = System.Net.HttpStatusCode.BadRequest;
                    }

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
        public async Task<DataResult<REResponseDto>> PostRE_SYNC_IB(ReceptionDto dto)
        {

            DataResult<REResponseDto> resultItem = new DataResult<REResponseDto>()
            {
                Status = System.Net.HttpStatusCode.OK,
                Message = "Se obtuvo el copade con exito de la BD"
            };

            try
            {
                // Parametros
                DynamicParameters par = new DynamicParameters();
                par.Add("@Organismo", dto.OrganismClave);
                par.Add("@Contract", dto.Contract);
                par.Add("@SapOrder", dto.SapOrder);
                par.Add("@SiafOrder", dto.SapOrder);
                par.Add("@Reception", dto.Reception);
                par.Add("@Exercise", dto.Exercise);
                par.Add("@BriefText", dto.BriefText);
                par.Add("@IvaAmount", dto.IvaAmount);
                par.Add("@AmountWithIva", dto.AmountWithIva);
                par.Add("@AmountWithoutIva", dto.AmountWithoutIva);
                par.Add("@MedicalUnit", dto.MedicalUnit);
                par.Add("@Penalty", dto.Penalty);
                par.Add("@Provider", dto.Provider);
                par.Add("@Signer", dto.Signer);
                par.Add("@MadeBy", dto.MadeBy);


                if (!JsonConvert.SerializeObject(dto.Detail).Equals("null"))
                    par.Add("@detail", JsonConvert.SerializeObject(dto.Detail));
                else
                    par.Add("@detail", null);

                //using para levantar la conexion al BD
                using (IDbConnection db = GetConnection())
                {
                    var res = await db.QueryFirstAsync<REResponseItemDto>(sql: "SP_Reception_interface", param: par, commandType: CommandType.StoredProcedure);

                    if (res != null)
                    {
                        REResponseDto ReceptionResponse = new REResponseDto()
                        {
                            item = new REResponseItemDto()
                            {
                                Reception = dto.Reception,
                                Exercise = dto.Exercise,
                                Status = res.Status
                            }
                        };

                        resultItem.Data = ReceptionResponse;
                        resultItem.Message = $"Se recibio con exito la entrada al almacen {dto.Reception}";
                    }
                    else
                    {
                        resultItem.Message = $"Ocurrio un problema con la entrada al almacen {dto.Reception}";
                        resultItem.Status = System.Net.HttpStatusCode.BadRequest;
                    }

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
        public async Task<DataResult<ReceptionDto>> RecuperaREBDAsync(ReceptionDto reception)
        {
            DataResult<ReceptionDto> resultItem = new DataResult<ReceptionDto>()
            {
                Status = System.Net.HttpStatusCode.OK,
                Message = "Se obtuvo el copade con exito de la BD"
            };

            try
            {
                // Parametros
                DynamicParameters par = new DynamicParameters();
                par.Add("@clave", reception.OrganismClave);
                par.Add("@SapOrder", reception.SapOrder);
                par.Add("@Reception", reception.Reception);
                par.Add("@Exercise", reception.Exercise);

                //using para levantar la conexion al BD
                using (IDbConnection db = GetConnection())
                {
                    var res = await db.QueryMultipleAsync(sql: "SP_CopadeCorp_interface", param: par, commandType: CommandType.StoredProcedure);
                    var receptionResult = await res.ReadFirstAsync<ReceptionDto>();
                    var receptioDetalles = await res.ReadFirstAsync<string>();


                    resultItem.Data = receptionResult;
                    resultItem.Data.Detail = JsonConvert.DeserializeObject<IEnumerable<RecepcionDetail>>(receptioDetalles);

                    if (res != null)
                    {
                        //resultItem.Data.copade = res;
                        resultItem.Message = $"Se recibio con exito el copade {reception.Reception}";
                    }
                    else
                    {
                        resultItem.Message = $"Ocurrio un problema con el copade {reception.Reception}";
                        resultItem.Status = System.Net.HttpStatusCode.BadRequest;
                    }

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
        public async Task<DataResult<COPADEResponseDto>> PostCOPADE_Response(COPADEResponseDto dto)
        {
            DataResult<COPADEResponseDto> resultItem = new DataResult<COPADEResponseDto>()
            {
                Status = System.Net.HttpStatusCode.OK,
                Message = "Envío Exitoso a SAP PI"
            };

            var client = new RestClient(_configuration["PI:RecuperaCopade"]);
            var request = new RestRequest("", Method.Post);
            request.AddHeader("content-type", "application/json");
            request.AddHeader("authorization", $"Basic {Utils.Encoding.Base64Encode(_configuration["PI:usuario_pass"])}");

            // Convertimos a JSON el dto que se recibe
            var mensaje = JsonConvert.SerializeObject(dto);
            request.AddParameter("application/json", mensaje, ParameterType.RequestBody);
            var response = await client.ExecuteAsync(request);

            // Se Deserealiza la respuesta
            try
            {
                var resp = JsonConvert.DeserializeObject<COPADEResponseDto>(response.Content);
                resultItem.Data = resp;

                return resultItem;
            }
            catch (Exception)
            {
                resultItem.Message = $"Ocurrio un problema. Contacta a tu administrador. {response.Content}";
                resultItem.Status = System.Net.HttpStatusCode.BadRequest;
                return resultItem;
            }
        }
        public async Task<DataResult<CopadeDto>> RecuperaCopadeEmailsync(Guid CopadeId)
        {
            DataResult<CopadeDto> resultItem = new DataResult<CopadeDto>()
            {
                Data = new CopadeDto(),
                Status = System.Net.HttpStatusCode.OK,
                Message = "Se obtuvo el copade con exito de la BD"
            };

            try
            {
                // Parametros
                DynamicParameters par = new DynamicParameters();
                //par.Add("@CopadeID", CopadeId);
                var CopadeId1 = CopadeId.ToString();
                par.Add("@CopadeID", CopadeId1);


                //using para levantar la conexion al BD
                using (IDbConnection db = GetConnection())
                {
                    var res = await db.QueryMultipleAsync(sql: "SP_copade_signer_email", param: par, commandType: CommandType.StoredProcedure);
                    var copade = await res.ReadFirstAsync<CopadeDto>();

                    resultItem.Data = copade;

                    if (res != null)
                    {
                        //resultItem.Data.copade = res;
                        resultItem.Message = $"Se recibio con exito el copade {CopadeId} ";
                    }
                    else
                    {
                        resultItem.Message = $"Ocurrio un problema con el copade {CopadeId}";
                        resultItem.Status = System.Net.HttpStatusCode.BadRequest;
                    }

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
        public async Task<DataResult<CopadeResponseDto>> PostCopade_Response(CopadeResponseDto dto)
        {
            DataResult<CopadeResponseDto> resultItem = new DataResult<CopadeResponseDto>()
            {
                Status = System.Net.HttpStatusCode.OK,
                Message = "Envío Exitoso a SAP PI"
            };

            var client = new RestClient(_configuration["PI:RecuperaCopade"]);
            var request = new RestRequest("", Method.Post);
            request.AddHeader("content-type", "application/json");
            request.AddHeader("authorization", $"Basic {Utils.Encoding.Base64Encode(_configuration["PI:usuario_pass"])}");

            // Convertimos a JSON el dto que se recibe
            var mensaje = JsonConvert.SerializeObject(dto);
            request.AddParameter("application/json", mensaje, ParameterType.RequestBody);
            var response = await client.ExecuteAsync(request);

            // Se Deserealiza la respuesta
            try
            {
                CopadeResponseItemDto resp = JsonConvert.DeserializeObject<CopadeResponseItemDto>(response.Content);
                CopadeResponseDto respDto = new CopadeResponseDto();
                respDto.item = resp;
                resultItem.Data = respDto;

                return resultItem;
            }
            catch (Exception)
            {
                resultItem.Message = $"Ocurrio un problema. Contacta a tu administrador. {response.Content}";
                resultItem.Status = System.Net.HttpStatusCode.BadRequest;
                return resultItem;
            }

        }
        public async Task<DataResult<string>> GetNotaCreditoById(Guid CopadeId)
        {
            DataResult<string> resultItem = new DataResult<string>()
            {
                Status = System.Net.HttpStatusCode.OK,
                Message = "Se obtuvo la Nota de Credito XML con exito"
            };
            try
            {
                DynamicParameters par = new DynamicParameters();
                par.Add("@CopadeId", CopadeId);
                using (IDbConnection db = GetConnection())
                {
                    var copadePreFactura = await db.QueryFirstAsync<string>(sql: "SP_CopadeGetNotaCreditoById_interface", param: par, commandType: CommandType.StoredProcedure);
                    //Utils.ArmaPreFacturaPemex tmp = new Utils.ArmaPreFacturaPemex(JsonConvert.DeserializeObject<CopadePreFactura>(copadePreFactura));
                    resultItem.Data = copadePreFactura;
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
        public async Task<DataResult<AnaliticoPagoResponseDto>> PostAP_SYNC_IB(AnaliticoPagoDto dto)
        {
            DataResult<AnaliticoPagoResponseDto> resultItem = new DataResult<AnaliticoPagoResponseDto>()
            {
                Status = System.Net.HttpStatusCode.OK,
                Message = "Se obtuvo el copade con exito de la BD"
            };

            try
            {
                // Parametros
                DynamicParameters par = new DynamicParameters();

                par.Add("@Clave", dto.clave);
                par.Add("@Analitico", dto.Analitico);
                par.Add("@Cedula", dto.Cedula);
                par.Add("@Centro", dto.Centro);
                par.Add("@Contrato", dto.Contrato);
                par.Add("@CveTransportista", dto.CveTransportista);
                par.Add("@Ejercicio", dto.Ejercicio);
                par.Add("@EsTRI", dto.EsTRI);
                par.Add("@Faltante", dto.Faltante);
                par.Add("@FechaEmision", dto.FechaEmision);
                par.Add("@IdAnalitico", dto.IdAnalitico);
                par.Add("@Iva", dto.Iva);
                par.Add("@Moneda", dto.Moneda);
                par.Add("@NumAcreedor", dto.NumAcreedor);
                par.Add("@NumCliente", dto.NumCliente);
                par.Add("@SubTotal", dto.SubTotal);
                par.Add("@Total", dto.Total);
                if (!JsonConvert.SerializeObject(dto.vAddenda).Equals("null"))
                    par.Add("@Addenda", JsonConvert.SerializeObject(dto.vAddenda));
                else
                    par.Add("@Addenda", null);

                if (!JsonConvert.SerializeObject(dto.vImpuestos).Equals("null"))
                    par.Add("@Impuestos", JsonConvert.SerializeObject(dto.vImpuestos));
                else
                    par.Add("@Impuestos", null);

                if (!JsonConvert.SerializeObject(dto.vPreFactura).Equals("null"))
                    par.Add("@PreFactura", JsonConvert.SerializeObject(dto.vPreFactura));
                else
                    par.Add("@PreFactura", null);

                ArmaPreFacturaAPPemex tmpPF = new ArmaPreFacturaAPPemex(dto.vPreFactura);
                par.Add("@PreFacturaXML", tmpPF.xmlResult);

                //using para levantar la conexion al BD
                using (IDbConnection db = GetConnection())
                {
                    var res = await db.QueryFirstOrDefaultAsync<AnaliticoPagoResponseDto>(sql: "SP_AnaliticoPago_interface", param: par, commandType: CommandType.StoredProcedure);

                    if (res != null)
                    {
                        resultItem.Data = res;
                        resultItem.Message = $"Se recibio con exito el analitico de pago {dto.IdAnalitico}";
                    }
                    else
                    {
                        resultItem.Message = $"Ocurrio un problema con el analitico de pago  {dto.IdAnalitico}";
                        resultItem.Status = System.Net.HttpStatusCode.BadRequest;
                    }

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
        public async Task<DataResult<OSLiberacionVPDto>> PostOSLiberacionVP(OSLiberacionVPDto dto)
        {
            //"SP_SupplyOrder_OSLiberacionVP_interface"
            DataResult<OSLiberacionVPDto> resultItem = new DataResult<OSLiberacionVPDto>()
            {
                Status = System.Net.HttpStatusCode.OK,
                Message = "Se realizo la liberacionVP con exito"
            };

            try
            {
                // Parametros
                DynamicParameters par = new DynamicParameters();

                par.Add("@Contract", dto.Contrato);
                par.Add("@SapOrder", dto.OrdenSap);
                par.Add("@SiafOrder", dto.OrdenSiaf);
                par.Add("@FechaLiberacionVP", dto.FechaLiberacion);
                using (IDbConnection db = GetConnection())
                {
                    var res = await db.QueryFirstAsync<OSLiberacionVPDto>(sql: "SP_SupplyOrder_OSLiberacionVP_interface", param: par, commandType: CommandType.StoredProcedure);
                    if (res.Status == "ACEPTADO")
                    {
                        resultItem.Data = res;
                    }
                    else
                    {
                        resultItem.Status = System.Net.HttpStatusCode.BadRequest;
                        resultItem.Message = res.Status;
                    }
                    return resultItem;
                }
            }
            catch (Exception ex)
            {
                resultItem.Status = System.Net.HttpStatusCode.InternalServerError;
                resultItem.Message = $"Ocurrio un problema, {ex.Message}";
                return resultItem;
            }
        }
        public async Task<DataResult<AnaliticoPagoResponseDto>> NotificaPreFactura(AnaliticoPagoPreFacturaDto dto)
        {
            DataResult<AnaliticoPagoResponseDto> resultItem = new DataResult<AnaliticoPagoResponseDto>()
            {
                Status = System.Net.HttpStatusCode.OK,
                Message = "Envío Exitoso a SAP PI"
            };

            var client = new RestClient(_configuration["PI:NotificaPreFactura"]);
            var request = new RestRequest("", Method.Post);
            request.AddHeader("content-type", "application/json");
            request.AddHeader("authorization", $"Basic {Utils.Encoding.Base64Encode(_configuration["PI:usuario_pass"])}");

            // Convertimos a JSON el dto que se recibe
            var mensaje = JsonConvert.SerializeObject(dto);
            request.AddParameter("application/json", mensaje, ParameterType.RequestBody);
            var response = await client.ExecuteAsync(request);

            // Se Deserealiza la respuesta
            try
            {
                AnaliticoPagoResponseDto resp = JsonConvert.DeserializeObject<AnaliticoPagoResponseDto>(response.Content);
                resultItem.Data = resp;

                return resultItem;
            }
            catch (Exception)
            {
                resultItem.Message = $"Ocurrio un problema. Contacta a tu administrador. {response.Content}";
                resultItem.Status = System.Net.HttpStatusCode.BadRequest;
                return resultItem;
            }
        }

        public async Task<DataResult<PagoResponseDto>> PostPP_SYNC_IB(PaymentScheduleDto dto)
        {
            DataResult<PagoResponseDto> resultItem = new DataResult<PagoResponseDto>()
            {
                Message = "Se recibio el Programa de Pago con exito",
                Status = System.Net.HttpStatusCode.OK
            };

            try
            {
                DynamicParameters par = new DynamicParameters();
                par.Add("@ProgramaPago_Id", dto.ProgramaPago_Id);
                par.Add("@Clave", dto.OrganismClave);
                par.Add("@PaymentDate", dto.PaymentDate);
                par.Add("@ScheduleDate", dto.ScheduleDate);
                par.Add("@Authorizes", dto.Authorizes);
                par.Add("@Mail", dto.Mail);
                par.Add("@PositionAuthorizes", dto.PositionAuthorizes);
                par.Add("@PositionTo", dto.PositionTo);
                par.Add("@To", dto.To);
                par.Add("@TokenAuthorizes", dto.TokenAuthorizes);
                par.Add("@TokenTo", dto.TokenTo);

                if (!JsonConvert.SerializeObject(dto.vDetalle).Equals("null"))
                    par.Add("@Detalle", JsonConvert.SerializeObject(dto.vDetalle));
                else
                    par.Add("@Detalle", null);

                if (!JsonConvert.SerializeObject(dto.vDetallePep).Equals("null"))
                    par.Add("@DetallePep", JsonConvert.SerializeObject(dto.vDetallePep));
                else
                    par.Add("@DetallePep", null);

                //using para levantar la conexion al BD
                using (IDbConnection db = GetConnection())
                {
                    PagoResponseDto resPago = new PagoResponseDto();
                    var res = await db.QueryFirstAsync<Guid?>(sql: "SP_PaymentSchedule_interface", param: par, commandType: CommandType.StoredProcedure);
                    if (res != null)
                    {
                        resPago.ID = res.Value;
                        resPago.Status = "ACEPTADO";
                        resPago.Mensaje = $"Se recibio con exito el Programa de Pago {dto.ProgramaPago_Id}";
                        resultItem.Data = resPago;
                    }
                    else
                    {
                        resPago.Mensaje = $"Ocurrio un problema con el Programa de Pago {dto.ProgramaPago_Id}";
                        resultItem.Data = resPago;
                        resultItem.Status = System.Net.HttpStatusCode.BadRequest;
                    }
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

        public async Task<DataResult<PagoResponseDto>> PostLP_SYNC_IB(PaymentListDto dto)
        {
            DataResult<PagoResponseDto> resultItem = new DataResult<PagoResponseDto>()
            {
                Message = "Se recibio la Lista de Pago con exito",
                Status = System.Net.HttpStatusCode.OK
            };
            try
            {
                DynamicParameters par = new DynamicParameters();
                par.Add("@ListaPago_Id", dto.ListaPago_Id);
                par.Add("@OrganismID", dto.OrganismID);
                par.Add("@Date", dto.Date);
                par.Add("@Authorizes", dto.Authorizes);
                par.Add("@PositionAuthorizes", dto.PositionAuthorizes);
                par.Add("@PositionTo", dto.PositionTo);
                par.Add("@To", dto.To);
                par.Add("@TokenAuthorizes", dto.TokenAuthorizes);
                par.Add("@TokenTo", dto.TokenTo);

                if (!JsonConvert.SerializeObject(dto.vDetalle).Equals("null"))
                    par.Add("@Detalle", JsonConvert.SerializeObject(dto.vDetalle));
                else
                    par.Add("@Detalle", null);

                if (!JsonConvert.SerializeObject(dto.vDetallePep).Equals("null"))
                    par.Add("@DetallePep", JsonConvert.SerializeObject(dto.vDetallePep));
                else
                    par.Add("@DetallePep", null);

                //using para levantar la conexion al BD
                using (IDbConnection db = GetConnection())
                {
                    PagoResponseDto resPago = new PagoResponseDto();
                    var res = await db.QueryFirstAsync<Guid?>(sql: "SP_PaymentList_interface", param: par, commandType: CommandType.StoredProcedure);
                    if (res != null)
                    {
                        resPago.ID = res.Value;
                        resPago.Status = "ACEPTADO";
                        resPago.Mensaje = $"Se recibio con exito el Lista de Pago {dto.ListaPago_Id}";
                        resultItem.Data = resPago;
                    }
                    else
                    {
                        resPago.Mensaje = $"Ocurrio un problema con el Lista de Pago {dto.ListaPago_Id}";
                        resultItem.Data = resPago;
                        resultItem.Status = System.Net.HttpStatusCode.BadRequest;
                    }
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
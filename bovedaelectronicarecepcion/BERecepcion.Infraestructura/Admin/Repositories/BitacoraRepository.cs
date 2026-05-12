using BERecepcion.Core.Admin.Dto;
using BERecepcion.Core.Admin.Interfaces.Repositories;
using BERecepcion.Core.Consulta.Dto;
using BERecepcion.Core.Dto;
using BERecepcion.Core.Interfaces.Repositories;
using BERecepcion.Infraestructura.Repositories;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BERecepcion.Infraestructura.Admin.Repositories
{
    public class BitacoraRepository : BaseSQLServerSqlRepository, IBitacoraRepository
    {
        public BitacoraRepository(string cnnString) : base(cnnString)
        {
        }

        public async Task<DataResult<IEnumerable<BitacoraDto>>> GetBitacoraAsync(DateTime fechaInicial, DateTime fechaFinal, string busqueda, int pageSize, int pageNum)
        {
            DataResult<IEnumerable<BitacoraDto>> resultItem = new DataResult<IEnumerable<BitacoraDto>>()
            {
                Status = System.Net.HttpStatusCode.OK
            };
            try
            {
                using (IDbConnection db = GetConnection())
                {
                    DynamicParameters par = new DynamicParameters();
                    par.Add("@FechaInicial", fechaInicial);
                    par.Add("@FechaFinal", fechaFinal);
                    par.Add("@Busqueda", busqueda);
                    par.Add("@pagenum", pageNum);
                    par.Add("@pagesize", pageSize);

                    var result = await db.QueryMultipleAsync(sql: "SP_bitacora_selecciona ", param: par, commandType: CommandType.StoredProcedure);

                    var paging = await result.ReadAsync<Pager>();
                    var perfiles = await result.ReadAsync<BitacoraDto>();

                    resultItem.Data = perfiles;
                    resultItem.Pager = paging.FirstOrDefault();
                }
                return resultItem;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<DataResult<IEnumerable<BitacoraDto>>> GetAllBitacoraAsync(DateTime fechaInicial, DateTime fechaFinal, string busqueda)
        {
            DataResult<IEnumerable<BitacoraDto>> resultItem = new DataResult<IEnumerable<BitacoraDto>>()
            {
                Status = System.Net.HttpStatusCode.OK
            };
            try
            {
                using (IDbConnection db = GetConnection())
                {
                    DynamicParameters par = new DynamicParameters();
                    par.Add("@FechaInicial", fechaInicial);
                    par.Add("@FechaFinal", fechaFinal);
                    par.Add("@Busqueda", busqueda);
                    var result = await db.QueryAsync<BitacoraDto>(sql: "SP_bitacora_report_selecciona ", param: par, commandType: CommandType.StoredProcedure);
                    resultItem.Data = result;
                }
                return resultItem;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<DataResult<BitacoraDto>> InsertaBitacoraAsync(BitacoraDto dto)
        {
            DataResult<BitacoraDto> resultItem = new DataResult<BitacoraDto>()
            {
                Status = System.Net.HttpStatusCode.OK,
                Message = "Bitácora guardada con éxito."
            };
            dto.UserID = dto.UserID == Guid.Empty ? null : dto.UserID;
            try
            {
                using (IDbConnection db = GetConnection())
                {
                    DynamicParameters par = new DynamicParameters();
                    par.Add("@UserID", dto.UserID);
                    par.Add("@Seccion", dto.Seccion);
                    par.Add("@Accion", dto.Accion);
                    par.Add("@Descripcion", dto.Descripcion);
                    await db.QueryAsync(sql: "SP_bitacora_inserta", param: par, commandType: CommandType.StoredProcedure);
                }
                return resultItem;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<DataResult<BitacoraInvoiceDto>> InsertaBitacoraInvoiceAsync(BitacoraInvoiceDto dto)
        {
            DataResult<BitacoraInvoiceDto> resultItem = new DataResult<BitacoraInvoiceDto>()
            {
                Status = System.Net.HttpStatusCode.OK,
                Message = "Bitácora guardada con éxito."
            };
            try
            {
                if (string.IsNullOrEmpty(dto.userId))
                {
                    dto.userId = Guid.Empty.ToString();
                }

                using (IDbConnection db = GetConnection())
                {
                    db.Open();
                    DynamicParameters par = new DynamicParameters();
                    par.Add("@medioProceso", dto.medioProceso);
                    par.Add("@reception", dto.reception);
                    par.Add("@clave", dto.clave);
                    par.Add("@userId", dto.userId);
                    var result = await db.QueryFirstOrDefaultAsync<int>(sql: "SP_bitacora_invoice_inserta", param: par, commandType: CommandType.StoredProcedure);
                }

                resultItem.Data = dto;
                return resultItem;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<DataResult<BitacoraInvoiceDto>> InsertaBitacoraInvoiceAPAsync(BitacoraInvoiceDto dto)
        {
            DataResult<BitacoraInvoiceDto> resultItem = new DataResult<BitacoraInvoiceDto>()
            {
                Status = System.Net.HttpStatusCode.OK,
                Message = "Bitácora guardada con éxito."
            };
            try
            {
                if (string.IsNullOrEmpty(dto.userId))
                {
                    dto.userId = Guid.Empty.ToString();
                }

                using (IDbConnection db = GetConnection())
                {
                    db.Open();
                    DynamicParameters par = new DynamicParameters();
                    par.Add("@medioProceso", dto.medioProceso);
                    par.Add("@reception", dto.reception);
                    par.Add("@documentbeid", dto.documentBeId);
                    par.Add("@clave", dto.clave);
                    par.Add("@userId", dto.userId);
                    var result = await db.QueryFirstOrDefaultAsync<int>(sql: "SP_bitacora_invoiceAP_inserta", param: par, commandType: CommandType.StoredProcedure);
                }

                resultItem.Data = dto;
                return resultItem;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<DataResult<BitacoraInvoiceErrorInsertDto>> InsertaBitacoraInvoiceErrorAsync(BitacoraInvoiceErrorInsertDto dtoList)
        {
            DataResult<BitacoraInvoiceErrorInsertDto> resultItem = new DataResult<BitacoraInvoiceErrorInsertDto>()
            {
                Status = System.Net.HttpStatusCode.OK,
                Message = "Bitácora guardada con éxito."
            };
            try
            {
                Guid lote = Guid.NewGuid();

                foreach (var dto in dtoList.errors)
                {
                    using (IDbConnection db = GetConnection())
                    {
                        db.Open();
                        DynamicParameters par = new DynamicParameters();
                        par.Add("@medioProceso", dtoList.medioProceso);
                        par.Add("@reception", dtoList.reception);
                        par.Add("@clave", dto.clave);
                        par.Add("@userId", dtoList.UserId);
                        if (dto.mensaje != null)
                        {
                            par.Add("@mensaje", dto.mensaje.Length > 300 ? dto.mensaje.Substring(0, 300) : dto.mensaje);
                        }
                        else
                        {
                            par.Add("@mensaje", null);
                        }
                        par.Add("@lote", lote);
                        var result = await db.QueryFirstOrDefaultAsync<int>(sql: "SP_bitacora_invoice_error_inserta", param: par, commandType: CommandType.StoredProcedure);
                    }
                }

                resultItem.Data = dtoList;
                return resultItem;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> BitacoraInvoice(string reception, string clave, UsersDto user, bool process)
        {
            DataResult<BitacoraInvoiceDto> resultItem = new DataResult<BitacoraInvoiceDto>()
            {
                Status = System.Net.HttpStatusCode.OK,
                Message = "bitacora invoice guardado con éxito."
            };
            try
            {
                if (!process) return true;

                string UserId = "";
                if (user != null)
                    UserId = user.UserID.ToString();
                BitacoraInvoiceDto dtoB = new BitacoraInvoiceDto();
                dtoB.clave = clave;
                dtoB.reception = reception;
                dtoB.userId = UserId;
                dtoB.medioProceso = 0;
                var result = await InsertaBitacoraInvoiceAsync(dtoB);
            }
            catch (Exception ex)
            {
                resultItem.Status = System.Net.HttpStatusCode.BadRequest;
                resultItem.Message = ex.Message;    
            }

            return true;
        }

        public async Task<bool> BitacoraInvoiceAP(string IdAnaliticoPago, Guid documentBeId, string clave, UsersDto user, bool process)
        {
            DataResult<BitacoraInvoiceDto> resultItem = new DataResult<BitacoraInvoiceDto>()
            {
                Status = System.Net.HttpStatusCode.OK,
                Message = "bitacora invoice guardado con éxito."
            };
            try
            {
                if (!process) return true;

                string UserId = "";
                if (user != null)
                    UserId = user.UserID.ToString();
                BitacoraInvoiceDto dtoB = new BitacoraInvoiceDto();
                dtoB.clave = clave;
                dtoB.reception = IdAnaliticoPago;
                dtoB.documentBeId = documentBeId;
                dtoB.userId = UserId;
                dtoB.medioProceso = 0;
                var result = await InsertaBitacoraInvoiceAPAsync(dtoB);
            }
            catch (Exception ex)
            {
                resultItem.Status = System.Net.HttpStatusCode.BadRequest;
                resultItem.Message = ex.Message;
            }

            return true;
        }

        public async Task<bool> BitacoraInvoiceError(BitacoraInvoiceErrorInsertDto dto)
        {
            DataResult<List<BitacoraInvoiceErrorInsertDto>> resultItem = new DataResult<List<BitacoraInvoiceErrorInsertDto>>()
            {
                Status = System.Net.HttpStatusCode.OK,
                Message = "bitacora invoice error guardado con éxito."
            };
            try
            {
                var result = await InsertaBitacoraInvoiceErrorAsync(dto);
            }
            catch (Exception ex)
            {
                resultItem.Status = System.Net.HttpStatusCode.BadRequest;
                resultItem.Message = ex.Message;
            }

            return true;
        }

        public async Task<bool> InvoiceSentMail(bool sentMail, string reception, string Email)
        {
            bool result = true;
            try
            {
                using (IDbConnection db = GetConnection())
                {
                    db.Open();
                    DynamicParameters par = new DynamicParameters();
                    par.Add("@sentmail", sentMail);
                    par.Add("@reception", reception);
                    par.Add("@email", Email);
                    await db.QueryFirstOrDefaultAsync<int>(sql: "SP_bitacora_invoice_sentemail", param: par, commandType: CommandType.StoredProcedure);
                }
            }
            catch (Exception)
            {
                result = false;
            }

            return result;
        }

        public async Task<bool> InvoiceSentMailByDocumentoBEId(bool sentMail, Guid documentoBEId, string Email)
        {
            bool result = true;
            try
            {
                using (IDbConnection db = GetConnection())
                {
                    db.Open();
                    DynamicParameters par = new DynamicParameters();
                    par.Add("@sentmail", sentMail);
                    par.Add("@documentoBEId", documentoBEId);
                    par.Add("@email", Email);
                    await db.QueryFirstOrDefaultAsync<int>(sql: "SP_bitacora_invoice_documentobeid_sentemail", param: par, commandType: CommandType.StoredProcedure);
                }
            }
            catch (Exception)
            {
                result = false;
            }

            return result;
        }

        public async Task<bool> logInitialRequest(string claveOrganismo, string reception, string user, string xml, List<string> ncXml, bool esCopade, bool esDocumental)
        {
            bool result = true;

            using (IDbConnection db = GetConnection())
            {
                db.Open();
                using (var tran = db.BeginTransaction())
                {
                    try
                    {
                        DynamicParameters par = new DynamicParameters();
                        par.Add("@ClaveOrganismo", claveOrganismo);
                        par.Add("@Reception", reception);
                        par.Add("@UserId", user);
                        par.Add("@XmlContent", xml);
                        par.Add("@EsCopade", esCopade);
                        par.Add("@EsDocumental", esDocumental);
                        var id = await db.QueryFirstOrDefaultAsync<int>(sql: "SP_invoice_initialrequest_inserta", param: par, transaction: tran, commandType: CommandType.StoredProcedure);

                        if (ncXml.Count > 0)
                        {
                            for (int i = 0; i < ncXml.Count; i++)
                            {
                                DynamicParameters parnc = new DynamicParameters();
                                parnc.Add("@InvoiceInitialRequestId", id);
                                parnc.Add("@XmlContent", ncXml[i]);
                                await db.QueryFirstOrDefaultAsync<int>(sql: "SP_invoice_initialrequestnotacredito_inserta", param: parnc, transaction: tran, commandType: CommandType.StoredProcedure);
                            }
                        }

                        tran.Commit();
                    }
                    catch (Exception)
                    {
                        tran.Rollback();
                        result = false;
                    }
                }
            }

            return result;
        }

        public async Task<DataResult<IEnumerable<ReporteRechazosDto>>> GetListaRechazosAsync(DateTime start, DateTime end, string search, Guid userId, int source, int pageSize, int pageNum = 1, bool esDescarga = false)
        {
            DataResult<IEnumerable<ReporteRechazosDto>> resultItem = new DataResult<IEnumerable<ReporteRechazosDto>>
            {
                Status = System.Net.HttpStatusCode.OK,
            };
            List<ReporteRechazosDto> result = new List<ReporteRechazosDto>();

            if (!string.IsNullOrWhiteSpace(search))
                search = Core.Utils.SearchText.GetWhereClause(search, new List<string> { "Documento", "Correo", "Organismo", "RazonSocial", "MotivoRechazo", "RFCAcreedor", "RFCReceptor" });

            try
            {
                DateTime dt = new DateTime(end.Year, end.Month, end.Day, 23, 59, 59);

                using (IDbConnection db = GetConnection())
                {
                    DynamicParameters par = new DynamicParameters();
                    par.Add("@start", start);
                    par.Add("@end", dt);
                    par.Add("@search", search);
                    par.Add("@pagenum", pageNum);
                    par.Add("@pagesize", pageSize);
                    par.Add("@source", source);
                    par.Add("@userId", userId);
                    par.Add("@esDescarga", esDescarga);
                    var resultPre = await db.QueryAsync<ReporteRechazosItemDto>(sql: "SP_bitacora_invoice_error_selecciona_factura", param: par, commandType: CommandType.StoredProcedure);

                    var resultPr = resultPre.Select(x => new ReporteRechazosDto() { Correo = x.Correo, FechaRechazo = x.FechaRechazo, MotivoRechazo = new List<string>(), Organismo = x.Organismo, RazonSocial = x.RazonSocial, RFCAcreedor = x.RFCAcreedor, RFCReceptor = x.RFCReceptor, Documento = x.Documento, lote = x.lote });

                    foreach (var r in resultPr)
                    {
                        bool found = false;
                        var rr = result.Where(x => x.lote == r.lote).FirstOrDefault();
                        if (rr == null)
                        {
                            result.Add(r);
                        }
                    }

                    foreach (var x in result)
                    {
                        x.MotivoRechazo = resultPre.Where(p => p.lote == x.lote).Select(p => p.MotivoRechazo).ToList();
                    }

                    resultItem.Data = result;

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
    }
}

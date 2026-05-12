using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using BERecepcion.Core.Dto;
using BERecepcion.Core.eSignDto;
using BERecepcion.Core.Facturas.Dto;
using BERecepcion.Core.Facturas.Interfaces.Repositories;
using BERecepcion.Infraestructura.Repositories;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RestSharp;

namespace BERecepcion.Infraestructura.Facturas.Repositories
{
    public class FacturaElectronicaRepository : BaseSQLServerSqlRepository, IFacturaElectronicaRepository
    {
        public FacturaElectronicaRepository(string cnnString) : base(cnnString)
        {

        }

        public async Task<DataResult<InvoiceDto>> GetInvoice(Guid DocumentoBEId, bool IsCopade)
        {
            DataResult<InvoiceDto> resultItem = new DataResult<InvoiceDto>
            {
                Status = HttpStatusCode.OK,
                Message = "GetInvoice"
            };

            try
            {
                InvoiceDto invoiceDto = null;

                DynamicParameters par = new DynamicParameters();
                par.Add("@DocumentoBEId", DocumentoBEId);
                par.Add("@IsCopade", IsCopade);

                using (IDbConnection db = GetConnection())
                {
                    var result = await db.QueryFirstOrDefaultAsync<InvoiceDto>(sql: "SP_invoice_get_invoice", param: par, commandType: CommandType.StoredProcedure);

                    if (result != null)
                    {
                        invoiceDto = new InvoiceDto();

                        invoiceDto.InvoiceId = result.InvoiceId;
                        invoiceDto.DocumentoBEId = result.DocumentoBEId;
                        invoiceDto.UserId = result.UserId;
                        invoiceDto.EmailProvider = result.EmailProvider;
                        invoiceDto.EmailProviderSendDate = result.EmailProviderSendDate;
                        invoiceDto.Assignment = result.Assignment;
                        invoiceDto.CompensationDocument = result.CompensationDocument;
                        invoiceDto.InvoiceDate = result.InvoiceDate;
                        invoiceDto.Serie = result.Serie;
                        invoiceDto.Folio = result.Folio;
                        invoiceDto.TipoComprobante = result.TipoComprobante;
                        invoiceDto.Total = result.Total;
                        invoiceDto.Subtotal = result.Subtotal;
                        invoiceDto.TotalImpuestosTrasladados = result.TotalImpuestosTrasladados;
                        invoiceDto.TotalImpuestosRetenidos = result.TotalImpuestosRetenidos;
                        invoiceDto.Uuid = result.Uuid;
                        invoiceDto.SapDocument = result.SapDocument;
                        invoiceDto.IsCopade = result.IsCopade;
                        invoiceDto.ElectronicReception = result.ElectronicReception;
                        invoiceDto.ReceptionDate = result.ReceptionDate;
                        invoiceDto.RutaArchivo = result.RutaArchivo;
                        invoiceDto.OriginalXML = result.OriginalXML;
                        invoiceDto.LastStatus = result.LastStatus;
                        invoiceDto.LastStatusDate = result.LastStatusDate;
                        invoiceDto.Estatus = result.Estatus;
                        invoiceDto.FechaEmision = result.FechaEmision;
                        invoiceDto.ViaPago = result.ViaPago;
                        invoiceDto.ImporteOriginal = result.ImporteOriginal;
                        invoiceDto.DiferencialCargo = result.DiferencialCargo;
                        invoiceDto.DiferencialAbono = result.DiferencialAbono;
                    }

                    resultItem.Data = invoiceDto;

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

        public async Task<DataResult<InvoiceFullDataDto>> GetInvoiceFullData(Guid DocumentoBEId, bool IsCopade)
        {
            DataResult<InvoiceFullDataDto> resultItem = new DataResult<InvoiceFullDataDto>
            {
                Status = HttpStatusCode.OK,
                Message = "GetInvoiceFullData"
            };

            try
            {
                InvoiceFullDataDto invoiceFullDataDto = new InvoiceFullDataDto();
                var result = GetInvoice(DocumentoBEId, IsCopade).Result;
                invoiceFullDataDto.invoiceDto = result.Data;
                invoiceFullDataDto.invoiceCxPDtos = new List<InvoiceCxPDto>();

                if (invoiceFullDataDto.invoiceDto != null)
                {
                    var cxps = GetInvoiceCxPList(invoiceFullDataDto.invoiceDto.InvoiceId).Result;
                    if (cxps != null)
                    {
                        foreach (var r in cxps.Data.CxPs)
                        {
                            invoiceFullDataDto.invoiceCxPDtos.Add(r);
                        }
                    }
                }

                resultItem.Data = invoiceFullDataDto;

                return resultItem;
            }
            catch (Exception ex)
            {
                resultItem.Status = HttpStatusCode.BadRequest;
                resultItem.Message = $"Ocurrio un problema: {ex.Message}";
                return resultItem;
            }
        }

        public async Task<DataResult<InvoiceDto>> SaveInvoice(InvoiceDto dto)
        {
            DataResult<InvoiceDto> resultItem = new DataResult<InvoiceDto>()
            {
                Status = HttpStatusCode.OK,
                Message = "Factura guardada con éxito."
            };
            try
            {
                using (IDbConnection db = GetConnection())
                {
                    db.Open();
                    DynamicParameters par = new DynamicParameters();

                    par.Add("@InvoiceId", dto.InvoiceId);
                    par.Add("@DocumentoBEId", dto.DocumentoBEId);
                    par.Add("@UserId", dto.UserId);
                    par.Add("@Assignment", dto.Assignment);
                    par.Add("@CompensationDocument", dto.CompensationDocument);
                    par.Add("@InvoiceDate", dto.InvoiceDate);
                    par.Add("@Serie", dto.Serie);
                    par.Add("@Folio", dto.Folio);
                    par.Add("@TipoComprobante", dto.TipoComprobante);
                    par.Add("@Total", dto.Total);
                    par.Add("@Subtotal", dto.Subtotal);
                    par.Add("@TotalImpuestosTrasladados", dto.TotalImpuestosTrasladados);
                    par.Add("@TotalImpuestosRetenidos", dto.TotalImpuestosRetenidos);
                    par.Add("@Uuid", dto.Uuid);
                    par.Add("@SapDocument", dto.SapDocument);
                    par.Add("@IsCopade", dto.IsCopade);
                    par.Add("@ElectronicReception", dto.ElectronicReception);
                    par.Add("@RutaArchivo", dto.RutaArchivo == null ? "" : dto.RutaArchivo);
                    par.Add("@CFDIXML", dto.OriginalXML);
                    par.Add("@Uuid", dto.Uuid);
                    par.Add("@SapDocument", dto.SapDocument);
                    par.Add("@IsCopade", dto.IsCopade);
                    par.Add("@FechaEmision", dto.FechaEmision);
                    par.Add("@ViaPago", dto.ViaPago);
                    par.Add("@ImporteOriginal", dto.ImporteOriginal);
                    par.Add("@DiferencialCargo", dto.DiferencialCargo);
                    par.Add("@DiferencialAbono", dto.DiferencialAbono);

                    Guid result = await db.QueryFirstOrDefaultAsync<Guid>(sql: "SP_invoice_inserta", param: par, commandType: CommandType.StoredProcedure);
                    dto.InvoiceId = result;
                    resultItem.Data = dto;
                }
                return resultItem;
            }
            catch (Exception ex)
            {
                resultItem.Status = HttpStatusCode.BadRequest;
                resultItem.Message = $"Ocurrio un problema: {ex.Message}";
                return resultItem;
            }
        }

        public async Task<DataResult<InvoiceEstatusDto>> SetInvoiceEstatus(InvoiceEstatusDto dto)
        {
            DataResult<InvoiceEstatusDto> resultItem = new DataResult<InvoiceEstatusDto>()
            {
                Status = HttpStatusCode.OK,
                Message = "Factura actualizada con éxito."
            };
            try
            {
                using (IDbConnection db = GetConnection())
                {
                    db.Open();
                    DynamicParameters par = new DynamicParameters();

                    par.Add("@InvoiceId", dto.InvoiceId);
                    par.Add("@Estatus", dto.Estatus);

                    var result = await db.QueryFirstAsync(sql: "SP_invoice_estatus", param: par, commandType: CommandType.StoredProcedure);
                    dto.result = true;
                    resultItem.Data = dto;
                }
                return resultItem;
            }
            catch (Exception ex)
            {
                resultItem.Status = HttpStatusCode.BadRequest;
                resultItem.Message = $"Ocurrio un problema: {ex.Message}";
                return resultItem;
            }
        }

        public async Task<DataResult<InvoiceEstatusDto>> SetInvoiceLastStatus(InvoiceEstatusDto dto)
        {
            DataResult<InvoiceEstatusDto> resultItem = new DataResult<InvoiceEstatusDto>()
            {
                Status = HttpStatusCode.OK,
                Message = "Factura actualizada con éxito."
            };
            try
            {
                using (IDbConnection db = GetConnection())
                {
                    db.Open();
                    DynamicParameters par = new DynamicParameters();

                    par.Add("@InvoiceId", dto.InvoiceId);
                    par.Add("@LastStatus", dto.LastStatus);

                    var result = db.Query(sql: "SP_invoice_last_status", param: par, commandType: CommandType.StoredProcedure);
                    dto.result = true;
                    resultItem.Data = dto;
                }
                return resultItem;
            }
            catch (Exception ex)
            {
                resultItem.Status = HttpStatusCode.BadRequest;
                resultItem.Message = $"Ocurrio un problema: {ex.Message}";
                return resultItem;
            }
        }

        public async Task<DataResult<InvoiceSapDocumentDto>> SetInvoiceSapDocument(InvoiceSapDocumentDto dto)
        {
            DataResult<InvoiceSapDocumentDto> resultItem = new DataResult<InvoiceSapDocumentDto>()
            {
                Status = HttpStatusCode.OK,
                Message = "Factura actualizada con éxito."
            };
            try
            {
                using (IDbConnection db = GetConnection())
                {
                    db.Open();
                    DynamicParameters par = new DynamicParameters();

                    par.Add("@InvoiceId", dto.InvoiceId);
                    par.Add("@SapDocument", dto.SapDocument);

                    var result = db.Query(sql: "SP_invoice_sapdocument", param: par, commandType: CommandType.StoredProcedure);
                    dto.result = true;
                    resultItem.Data = dto;
                }
                return resultItem;
            }
            catch (Exception ex)
            {
                resultItem.Status = HttpStatusCode.BadRequest;
                resultItem.Message = $"Ocurrio un problema: {ex.Message}";
                return resultItem;
            }
        }

        public async Task<DataResult<InvoiceCxPDto>> SaveInvoiceCxP(InvoiceCxPDto dto)
        {
            DataResult<InvoiceCxPDto> resultItem = new DataResult<InvoiceCxPDto>()
            {
                Status = HttpStatusCode.OK,
                Message = "CxP guardado con éxito."
            };
            try
            {
                using (IDbConnection db = GetConnection())
                {
                    db.Open();
                    DynamicParameters par = new DynamicParameters();

                    par.Add("@InvoiceId", dto.InvoiceId);
                    par.Add("@UserId", dto.UserId);
                    par.Add("@Res", dto.Res);

                    var result = await db.QueryFirstOrDefaultAsync<Guid>(sql: "SP_invoice_invoicecxp_inserta", param: par, commandType: CommandType.StoredProcedure);
                    dto.CxPId = result;
                    resultItem.Data = dto;
                }
                return resultItem;
            }
            catch (Exception ex)
            {
                resultItem.Status = HttpStatusCode.BadRequest;
                resultItem.Message = $"Ocurrio un problema: {ex.Message}";
                return resultItem;
            }
        }

        public async Task<DataResult<InvoiceNotaCreditoDto>> SaveInvoiceNotaCredito(InvoiceNotaCreditoDto dto)
        {
            DataResult<InvoiceNotaCreditoDto> resultItem = new DataResult<InvoiceNotaCreditoDto>()
            {
                Status = HttpStatusCode.OK,
                Message = "InvoiceNotaCredito guardado con éxito."
            };
            try
            {
                using (IDbConnection db = GetConnection())
                {
                    db.Open();
                    DynamicParameters par = new DynamicParameters();

                    par.Add("@InvoiceId", dto.InvoiceId);
                    par.Add("@DocumentoBEId", dto.DocumentoBEId);
                    par.Add("@isCopade", dto.IsCopade);
                    par.Add("@UUID", dto.Uuid);
                    par.Add("@Iva", dto.Iva);
                    par.Add("@Amount", dto.Amount);
                    par.Add("@Total", dto.Total);
                    par.Add("@CFDIXML", dto.OriginalXML);
                    par.Add("@Folio", dto.Folio);
                    par.Add("@Serie", dto.Serie);
                    par.Add("@NotaCreditoDate", dto.NotaCreditoDate);

                    var result = await db.QueryFirstOrDefaultAsync<Guid>(sql: "SP_invoice_invoicenotacredito_inserta", param: par, commandType: CommandType.StoredProcedure);
                    dto.NotaCreditoId = result;
                    resultItem.Data = dto;
                }
                return resultItem;
            }
            catch (Exception ex)
            {
                resultItem.Status = HttpStatusCode.BadRequest;
                resultItem.Message = $"Ocurrio un problema: {ex.Message}";
                return resultItem;
            }
        }

        public async Task<DataResult<InvoiceCxPListDto>> GetInvoiceCxPList(Guid InvoiceId)
        {
            DataResult<InvoiceCxPListDto> resultItem = new DataResult<InvoiceCxPListDto>
            {
                Status = HttpStatusCode.OK,
                Message = "GetInvoiceCxPList"
            };

            try
            {
                DynamicParameters par = new DynamicParameters();
                par.Add("@InvoiceId", InvoiceId);

                using (IDbConnection db = GetConnection())
                {
                    var result = await db.QueryMultipleAsync(sql: "SP_invoice_invoicecxp_list", param: par, commandType: CommandType.StoredProcedure);
                    var resultList = await result.ReadAsync<InvoiceCxPDto>();

                    List<InvoiceCxPDto> dato = new List<InvoiceCxPDto>();
                    foreach (var f in resultList)
                    {
                        InvoiceCxPDto m = new InvoiceCxPDto();
                        m.CxPId = f.CxPId;
                        m.InvoiceId = f.InvoiceId;
                        m.Res = f.Res;
                        m.UserId = f.UserId;
                        dato.Add(m);
                    }

                    resultItem.Data = new InvoiceCxPListDto();
                    resultItem.Data.InvoiceId = InvoiceId;
                    resultItem.Data.CxPs = dato;

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

        public async Task<DataResult<InvoiceDto>> GetInvoiceByUUID(Guid UUID, string folio = "")
        {
            DataResult<InvoiceDto> resultItem = new DataResult<InvoiceDto>
            {
                Status = HttpStatusCode.OK,
                Message = "GetInvoiceByUUID"
            };

            try
            {
                InvoiceDto invoiceDto = null;

                DynamicParameters par = new DynamicParameters();
                par.Add("@UUID", UUID);
                par.Add("@folio", folio);

                using (IDbConnection db = GetConnection())
                {
                    var result = await db.QueryFirstOrDefaultAsync<InvoiceDto>(sql: "SP_invoice_get_invoice_by_uuid", param: par, commandType: CommandType.StoredProcedure);

                    if (result != null)
                    {
                        invoiceDto = new InvoiceDto();

                        invoiceDto.InvoiceId = result.InvoiceId;
                        invoiceDto.DocumentoBEId = result.DocumentoBEId;
                        invoiceDto.UserId = result.UserId;
                        invoiceDto.EmailProvider = result.EmailProvider;
                        invoiceDto.EmailProviderSendDate = result.EmailProviderSendDate;
                        invoiceDto.Assignment = result.Assignment;
                        invoiceDto.CompensationDocument = result.CompensationDocument;
                        invoiceDto.InvoiceDate = result.InvoiceDate;
                        invoiceDto.Serie = result.Serie;
                        invoiceDto.Folio = result.Folio;
                        invoiceDto.TipoComprobante = result.TipoComprobante;
                        invoiceDto.Total = result.Total;
                        invoiceDto.Subtotal = result.Subtotal;
                        invoiceDto.TotalImpuestosTrasladados = result.TotalImpuestosTrasladados;
                        invoiceDto.TotalImpuestosRetenidos = result.TotalImpuestosRetenidos;
                        invoiceDto.Uuid = result.Uuid;
                        invoiceDto.SapDocument = result.SapDocument;
                        invoiceDto.IsCopade = result.IsCopade;
                        invoiceDto.ElectronicReception = result.ElectronicReception;
                        invoiceDto.ReceptionDate = result.ReceptionDate;
                        invoiceDto.RutaArchivo = result.RutaArchivo;
                        invoiceDto.OriginalXML = result.OriginalXML;
                        invoiceDto.LastStatus = result.LastStatus;
                        invoiceDto.LastStatusDate = result.LastStatusDate;
                        invoiceDto.Estatus = result.Estatus;
                        invoiceDto.FechaEmision = result.FechaEmision;
                        invoiceDto.ViaPago = result.ViaPago;
                        invoiceDto.ImporteOriginal = result.ImporteOriginal;
                        invoiceDto.DiferencialCargo = result.DiferencialCargo;
                        invoiceDto.DiferencialAbono = result.DiferencialAbono;
                    }

                    resultItem.Data = invoiceDto;

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

        public async Task<DataResult<InvoiceNotaCreditoDto>> GetInvoiceNotaCreditoByUUID(Guid UUID, string folio = "")
        {
            DataResult<InvoiceNotaCreditoDto> resultItem = new DataResult<InvoiceNotaCreditoDto>
            {
                Status = HttpStatusCode.OK,
                Message = "GetInvoiceNotaCreditoByUUID"
            };

            try
            {
                InvoiceNotaCreditoDto invoiceNotaCreditoDto = null;

                DynamicParameters par = new DynamicParameters();
                par.Add("@UUID", UUID);
                par.Add("@folio", folio);

                using (IDbConnection db = GetConnection())
                {
                    var result = await db.QueryFirstOrDefaultAsync<InvoiceNotaCreditoDto>(sql: "SP_invoice_get_notacredito_by_uuid", param: par, commandType: CommandType.StoredProcedure);

                    if (result != null)
                    {
                        invoiceNotaCreditoDto = new InvoiceNotaCreditoDto();

                        invoiceNotaCreditoDto.InvoiceId = result.InvoiceId;
                        invoiceNotaCreditoDto.Amount = result.Amount;
                        invoiceNotaCreditoDto.OriginalXML = result.OriginalXML;
                        invoiceNotaCreditoDto.DocumentoBEId = result.DocumentoBEId;
                        invoiceNotaCreditoDto.IsCopade = result.IsCopade;
                        invoiceNotaCreditoDto.Iva = result.Iva;
                        invoiceNotaCreditoDto.NotaCreditoId = result.NotaCreditoId;
                        invoiceNotaCreditoDto.Total = result.Total;
                        invoiceNotaCreditoDto.Uuid = result.Uuid;
                        invoiceNotaCreditoDto.Serie = result.Serie;
                        invoiceNotaCreditoDto.Folio = result.Folio;
                        invoiceNotaCreditoDto.NotaCreditoDate = result.NotaCreditoDate;
                    }

                    resultItem.Data = invoiceNotaCreditoDto;

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

        public async Task<DataResult<InvoiceNotaCreditoByReceptionDto>> GetInvoiceNotaCreditoByReception(InvoiceNotaCreditoByReceptionDto dto)
        {
            DataResult<InvoiceNotaCreditoByReceptionDto> resultItem = new DataResult<InvoiceNotaCreditoByReceptionDto>
            {
                Status = HttpStatusCode.OK,
                Message = "GetInvoiceNotaCreditoByReception"
            };

            try
            {
                InvoiceNotaCreditoDto invoiceNotaCreditoDto = null;

                DynamicParameters par = new DynamicParameters();
                par.Add("@reception", dto.reception);

                using (IDbConnection db = GetConnection())
                {
                    var result = await db.QueryFirstOrDefaultAsync<InvoiceNotaCreditoDto>(sql: "SP_invoice_get_notacredito_by_reception", param: par, commandType: CommandType.StoredProcedure);

                    if (result != null)
                    {
                        invoiceNotaCreditoDto = new InvoiceNotaCreditoDto();

                        invoiceNotaCreditoDto.InvoiceId = result.InvoiceId;
                        invoiceNotaCreditoDto.Amount = result.Amount;
                        invoiceNotaCreditoDto.OriginalXML = result.OriginalXML;
                        invoiceNotaCreditoDto.DocumentoBEId = result.DocumentoBEId;
                        invoiceNotaCreditoDto.IsCopade = result.IsCopade;
                        invoiceNotaCreditoDto.Iva = result.Iva;
                        invoiceNotaCreditoDto.NotaCreditoId = result.NotaCreditoId;
                        invoiceNotaCreditoDto.Total = result.Total;
                        invoiceNotaCreditoDto.Uuid = result.Uuid;
                        invoiceNotaCreditoDto.Serie = result.Serie;
                        invoiceNotaCreditoDto.Folio = result.Folio;
                        invoiceNotaCreditoDto.NotaCreditoDate = result.NotaCreditoDate;
                    }

                    InvoiceNotaCreditoByReceptionDto incr = new InvoiceNotaCreditoByReceptionDto();
                    incr.reception = dto.reception;
                    incr.invoiceNotaCreditoDto = invoiceNotaCreditoDto;
                    resultItem.Data = incr;

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

        public async Task<DataResult<InvoiceSaveGralDto>> SaveInvoiceGral(InvoiceSaveGralDto dto)
        {
            DataResult<InvoiceSaveGralDto> resultItem = new DataResult<InvoiceSaveGralDto>()
            {
                Status = HttpStatusCode.OK,
                Message = "Factura guardada con éxito."
            };


            using (IDbConnection db = GetConnection())
            {
                db.Open();

                using (var tran = db.BeginTransaction())
                {
                    try
                    {
                        DynamicParameters par = new DynamicParameters();

                        InvoiceDto idto = dto.invoiceDto;
                        par.Add("@InvoiceId", idto.InvoiceId);
                        par.Add("@DocumentoBEId", idto.DocumentoBEId);
                        par.Add("@UserId", idto.UserId);
                        par.Add("@Assignment", idto.Assignment == null ? "" : idto.Assignment);
                        par.Add("@CompensationDocument", idto.CompensationDocument);
                        par.Add("@InvoiceDate", idto.InvoiceDate);
                        par.Add("@Serie", idto.Serie == null ? "" : idto.Serie);
                        par.Add("@Folio", idto.Folio == null ? "" : idto.Folio);
                        par.Add("@TipoComprobante", idto.TipoComprobante);
                        par.Add("@Total", idto.Total);
                        par.Add("@Subtotal", idto.Subtotal);
                        par.Add("@TotalImpuestosTrasladados", idto.TotalImpuestosTrasladados);
                        par.Add("@TotalImpuestosRetenidos", idto.TotalImpuestosRetenidos);
                        par.Add("@Uuid", idto.Uuid);
                        par.Add("@IsCopade", idto.IsCopade);
                        par.Add("@ElectronicReception", idto.ElectronicReception);
                        par.Add("@RutaArchivo", idto.RutaArchivo == null ? "" : idto.RutaArchivo);
                        par.Add("@Uuid", idto.Uuid);
                        par.Add("@SapDocument", idto.SapDocument);
                        par.Add("@IsCopade", idto.IsCopade);
                        par.Add("@FechaEmision", idto.FechaEmision);
                        par.Add("@ViaPago", idto.ViaPago == null ? "" : idto.ViaPago);
                        par.Add("@ImporteOriginal", idto.ImporteOriginal);
                        par.Add("@DiferencialCargo", idto.DiferencialCargo);
                        par.Add("@DiferencialAbono", idto.DiferencialAbono);
                        par.Add("@OriginalXML", dto.invoiceXML.Replace("InvoiceAP", "Invoice"));
                        par.Add("@CFDIVersion", dto.invoiceDto.CFDIVersion);

                        Guid result = await db.QueryFirstOrDefaultAsync<Guid>(sql: "SP_invoice_inserta", param: par, transaction: tran, commandType: CommandType.StoredProcedure);
                        idto.InvoiceId = result;
                        dto.invoiceDto.InvoiceId = result;

                        // cxp

                        DynamicParameters cpar = new DynamicParameters();

                        InvoiceCxPDto cdto = dto.invoiceCxPDto;
                        cdto.InvoiceId = dto.invoiceDto.InvoiceId;
                        cpar.Add("@InvoiceId", cdto.InvoiceId);
                        cpar.Add("@UserId", cdto.UserId);
                        cpar.Add("@Res", cdto.Res);

                        var cresult = await db.QueryFirstOrDefaultAsync<Guid>(sql: "SP_invoice_invoicecxp_inserta", param: cpar, transaction: tran, commandType: CommandType.StoredProcedure);
                        dto.invoiceCxPDto.CxPId = cresult;

                        //nota de credito

                        if (dto.invoiceNotasCreditoDto != null)
                        {
                            if (dto.invoiceNotasCreditoDto.Count > 0)
                            {
                                for (int i = 0; i < dto.invoiceNotasCreditoDto.Count; i++)
                                {
                                    DynamicParameters npar = new DynamicParameters();
                                    npar.Add("@InvoiceId", dto.invoiceDto.InvoiceId);
                                    npar.Add("@isCopade", dto.invoiceNotasCreditoDto[i].IsCopade);
                                    npar.Add("@UUID", dto.invoiceNotasCreditoDto[i].Uuid);
                                    npar.Add("@Iva", dto.invoiceNotasCreditoDto[i].Iva);
                                    npar.Add("@Amount", dto.invoiceNotasCreditoDto[i].Amount);
                                    npar.Add("@Total", dto.invoiceNotasCreditoDto[i].Total);
                                    npar.Add("@Folio", dto.invoiceNotasCreditoDto[i].Folio);
                                    npar.Add("@Serie", dto.invoiceNotasCreditoDto[i].Serie);
                                    npar.Add("@NotaCreditoDate", dto.invoiceNotasCreditoDto[i].NotaCreditoDate);
                                    npar.Add("@OriginalXML", dto.InvoiceNotasCreditoXML[i]);
                                    npar.Add("@CFDIVersion", dto.invoiceNotasCreditoDto[i].CFDIVersion);

                                    var nresult = await db.QueryFirstOrDefaultAsync<Guid>(sql: "SP_invoice_invoicenotacredito_inserta", param: npar, transaction: tran, commandType: CommandType.StoredProcedure);
                                    dto.invoiceNotasCreditoDto[i].NotaCreditoId = nresult;
                                }
                            }
                        }

                        tran.Commit();

                        resultItem.Data = dto;
                    }
                    catch (Exception ex)
                    {
                        resultItem.Status = HttpStatusCode.BadRequest;
                        resultItem.Message = $"Ocurrio un problema: {ex.Message}";
                        tran.Rollback();
                    }
                }
            }

            return resultItem;
        }

    }
}

using BERRecepcion.Front.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BERRecepcion.Front.Utilities
{
    public static class ComprobanteConvert
    {
        public static ComprobanteBE ComprobanteBE40ToComprobanteBE(ComprobanteBE40 c40)
        {
            Addenda _addenda = null;
            if (c40.Addenda != null)
            {
                _addenda = new Addenda();
                _addenda.Addenda_Pemex = null;
                if (c40.Addenda.Addenda_Pemex != null)
                {
                    Addenda_Pemex _addenda_pemex = new Addenda_Pemex()
                    {
                        ANALITICO = c40.Addenda.Addenda_Pemex.ANALITICO,
                        AUTORIZA = c40.Addenda.Addenda_Pemex.AUTORIZA,
                        A_RELACION = c40.Addenda.Addenda_Pemex.A_RELACION,
                        CEDULA = c40.Addenda.Addenda_Pemex.CEDULA,
                        CEJECUTOR = c40.Addenda.Addenda_Pemex.CEJECUTOR,
                        CLAVE_TRANSP = c40.Addenda.Addenda_Pemex.CLAVE_TRANSP,
                        CONTRATO = c40.Addenda.Addenda_Pemex.CONTRATO,
                        CONTRATO_SIIC = c40.Addenda.Addenda_Pemex.CONTRATO_SIIC,
                        CORREOPMI = c40.Addenda.Addenda_Pemex.CORREOPMI,
                        C_GESTOR = c40.Addenda.Addenda_Pemex.C_GESTOR,
                        DOSALMILLAR = c40.Addenda.Addenda_Pemex.DOSALMILLAR,
                        EJERCICIO = c40.Addenda.Addenda_Pemex.EJERCICIO,
                        ENTRADA = c40.Addenda.Addenda_Pemex.ENTRADA,
                        FICHAE = c40.Addenda.Addenda_Pemex.FICHAE,
                        FICHAF = c40.Addenda.Addenda_Pemex.FICHAF,
                        FINIQUITO = c40.Addenda.Addenda_Pemex.FINIQUITO,
                        FONDO = c40.Addenda.Addenda_Pemex.FONDO,
                        ID_ANALITICO = c40.Addenda.Addenda_Pemex.ID_ANALITICO,
                        MONEDA = c40.Addenda.Addenda_Pemex.MONEDA,
                        NREMISION = c40.Addenda.Addenda_Pemex.NREMISION,
                        N_ACREEDOR = c40.Addenda.Addenda_Pemex.N_ACREEDOR,
                        N_ESTIMACION = c40.Addenda.Addenda_Pemex.N_ESTIMACION,
                        OCOMERCIAL = c40.Addenda.Addenda_Pemex.OCOMERCIAL,
                        O_SURTIMIENTO = c40.Addenda.Addenda_Pemex.O_SURTIMIENTO,
                        PLAZO = c40.Addenda.Addenda_Pemex.PLAZO,
                        Pm = c40.Addenda.Addenda_Pemex.Pm,
                        POSICIONAP = c40.Addenda.Addenda_Pemex.POSICIONAP,
                        POSICIONF = c40.Addenda.Addenda_Pemex.POSICIONF,
                        P_ESTIMACION = c40.Addenda.Addenda_Pemex.P_ESTIMACION,
                        RECEPSAP = c40.Addenda.Addenda_Pemex.RECEPSAP,
                        REMESA = c40.Addenda.Addenda_Pemex.REMESA,
                        RFCPROVEEDOR = c40.Addenda.Addenda_Pemex.RFCPROVEEDOR,
                        SchemaLocation = c40.Addenda.Addenda_Pemex.SchemaLocation,
                        SERVICIOA = c40.Addenda.Addenda_Pemex.SERVICIOA,
                        SERVICIOG = c40.Addenda.Addenda_Pemex.SERVICIOG,
                        TIPO_PRODUCTO = c40.Addenda.Addenda_Pemex.TIPO_PRODUCTO,
                        VUREGION = c40.Addenda.Addenda_Pemex.VUREGION,
                        Xsi = c40.Addenda.Addenda_Pemex.Xsi
                    };
                    _addenda.Addenda_Pemex = _addenda_pemex;
                };
            };

            List<DoctoRelacionado> _doctoRelacionados = null;
            if (c40.Complemento != null)
            {
                if (c40.Complemento.Pagos != null)
                {
                    if (c40.Complemento.Pagos.Pago != null)
                    {
                        if (c40.Complemento.Pagos.Pago.DoctoRelacionado != null)
                        {
                            _doctoRelacionados = new List<DoctoRelacionado>() { };
                            foreach (var it in c40.Complemento.Pagos.Pago.DoctoRelacionado)
                            {
                                DoctoRelacionado d = new DoctoRelacionado()
                                {
                                    ExisteEnBD = it.ExisteEnBD,
                                    Folio = it.Folio,
                                    IdDocumento = it.IdDocumento,
                                    ImpPagado = it.ImpPagado,
                                    ImpSaldoAnt = it.ImpSaldoAnt,
                                    ImpSaldoInsoluto = it.ImpSaldoInsoluto,
                                    MetodoDePagoDR = it.MetodoDePagoDR,
                                    MonedaDR = it.MonedaDR,
                                    NumParcialidad = it.NumParcialidad
                                };
                                _doctoRelacionados.Add(d);
                            }
                        }
                    }
                }
            }

            Pago _pago = null;
            if (c40.Complemento != null)
            {
                if (c40.Complemento.Pagos != null)
                {
                    if (c40.Complemento.Pagos.Pago != null)
                        _pago = new Pago()
                        {
                            CtaBeneficiario = c40.Complemento.Pagos.Pago.CtaBeneficiario,
                            CtaOrdenante = c40.Complemento.Pagos.Pago.CtaOrdenante,
                            DoctoRelacionado = _doctoRelacionados,
                            FechaPago = c40.Complemento.Pagos.Pago.FechaPago,
                            FormaDePagoP = c40.Complemento.Pagos.Pago.FormaDePagoP,
                            MonedaP = c40.Complemento.Pagos.Pago.MonedaP,
                            Monto = c40.Complemento.Pagos.Pago.Monto,
                            NomBancoOrdExt = c40.Complemento.Pagos.Pago.NomBancoOrdExt,
                            NumOperacion = c40.Complemento.Pagos.Pago.NumOperacion,
                            RfcEmisorCtaBen = c40.Complemento.Pagos.Pago.RfcEmisorCtaBen,
                            RfcEmisorCtaOrd = c40.Complemento.Pagos.Pago.RfcEmisorCtaOrd
                        };
                }
            }


            Pagos _pagos = null;
            if (c40.Complemento != null)
            {
                if (c40.Complemento.Pagos != null)
                {
                    _pagos = new Pagos()
                    {
                        Pago = _pago,
                        Version = c40.Complemento.Pagos.Version
                    };
                }
            }

            TimbreFiscalDigital _timbreFiscalDigital = null;
            if (c40.Complemento != null)
            {
                if (c40.Complemento.TimbreFiscalDigital != null)
                {
                    _timbreFiscalDigital = new TimbreFiscalDigital()
                    {
                        FechaTimbrado = c40.Complemento.TimbreFiscalDigital.FechaTimbrado,
                        NoCertificadoSAT = c40.Complemento.TimbreFiscalDigital.NoCertificadoSAT,
                        RfcProvCertif = c40.Complemento.TimbreFiscalDigital.RfcProvCertif,
                        SchemaLocation = c40.Complemento.TimbreFiscalDigital.SchemaLocation,
                        SelloCFD = c40.Complemento.TimbreFiscalDigital.SelloCFD,
                        SelloSAT = c40.Complemento.TimbreFiscalDigital.SelloSAT,
                        Tfd = c40.Complemento.TimbreFiscalDigital.Tfd,
                        UUID = c40.Complemento.TimbreFiscalDigital.UUID,
                        Version = c40.Complemento.TimbreFiscalDigital.Version
                    };
                }
            }

            CartaPorte _cartaporte = null;
            if (c40.Complemento != null)
            {
                if (c40.Complemento.CartaPorte != null)
                {
                    _cartaporte = c40.Complemento.CartaPorte;
                }
            }

            Complemento _complemento = new Complemento()
            {
                CartaPorte = _cartaporte,
                Pagos = _pagos,
                TimbreFiscalDigital = _timbreFiscalDigital
            };

            List<Concepto> _concepto = null;
            if (c40.Conceptos != null)
            {
                _concepto = new List<Concepto>() { };
                foreach (var it in c40.Conceptos)
                {
                    //nota: en v.4 las retenciones son multiples
                    Retencion _retencion3 = null;

                    if (it.Impuestos != null)
                    {
                        if (it.Impuestos.Retenciones != null)
                        {
                            _retencion3 = new Retencion()
                            {
                                Base = Convert.ToDouble(it.Impuestos.Retenciones[0].Base),
                                Importe = Convert.ToDouble(it.Impuestos.Retenciones[0].Importe),
                                Impuesto = it.Impuestos.Retenciones[0].Impuesto,
                                TasaOCuota = Convert.ToDouble(it.Impuestos.Retenciones[0].TasaOCuota),
                                TipoFactor = it.Impuestos.Retenciones[0].TipoFactor
                            };
                        }
                    }

                    Retenciones _retenciones3 = new Retenciones()
                    {
                        Retencion = _retencion3,
                    };

                    //nota: en v.4 los traslados son multiples
                    Traslado _traslado3 = null;

                    if (it.Impuestos != null)
                    {
                        if (it.Impuestos.Traslados != null)
                        {
                            _traslado3 = new Traslado()
                            {
                                Base = Convert.ToDouble(it.Impuestos.Traslados[0].Base),
                                Importe = Convert.ToDouble(it.Impuestos.Traslados[0].Importe),
                                Impuesto = Convert.ToInt32(it.Impuestos.Traslados[0].Impuesto),
                                TasaOCuota = Convert.ToDouble(it.Impuestos.Traslados[0].TasaOCuota),
                                TipoFactor = it.Impuestos.Traslados[0].TipoFactor
                            };
                        }
                    }

                    Traslados _traslados3 = new Traslados()
                    {
                        Traslado = _traslado3
                    };

                    Impuestos _impuestos3 = new Impuestos()
                    {
                        Retenciones = _retenciones3,
                        Traslados = _traslados3
                    };

                    Concepto con = new Concepto()
                    {
                        Cantidad = Convert.ToDouble(it.Cantidad),
                        ClaveProdServ = Convert.ToInt32(it.ClaveProdServ),
                        ClaveUnidad = it.ClaveUnidad,
                        Descripcion = it.Descripcion,
                        Importe = Convert.ToDouble(it.Importe),
                        Impuestos = _impuestos3,
                        NoIdentificacion = it.NoIdentificacion,
                        Unidad = it.Unidad,
                        ValorUnitario = Convert.ToDouble(it.ValorUnitario)
                    };
                    _concepto.Add(con);
                }
            }

            Conceptos _conceptos = new Conceptos()
            {
                Concepto = _concepto
            };

            Emisor _emisor = new Emisor()
            {
                Nombre = c40.Emisor.Nombre,
                RegimenFiscal = Convert.ToInt32(c40.Emisor.RegimenFiscal),
                Rfc = c40.Emisor.Rfc
            };

            Receptor _receptor = new Receptor()
            {
                Nombre = c40.Receptor.Nombre,
                Rfc = c40.Receptor.Rfc,
                UsoCFDI = c40.Receptor.UsoCFDI
            };

            CfdiRelacionado _cfdiRelacionado = null;
            //nota: en v.4 las cfdirelacionado son multiples
            if (c40.CfdiRelacionados != null)
            {
                if (c40.CfdiRelacionados.Length > 0)
                {
                    _cfdiRelacionado = new CfdiRelacionado()
                    {
                        UUID = c40.CfdiRelacionados[0].UUID
                    };
                }
            }

            CfdiRelacionados _cfdiRelacionados = new CfdiRelacionados()
            {
                CfdiRelacionado = _cfdiRelacionado,
                TipoRelacion = ""
            };

            Retencion _retencion = null;
            //nota: en v.4 las retenciones van multiples
            if (c40.Impuestos != null)
            {
                if (c40.Impuestos.Retenciones != null)
                {
                    _retencion = new Retencion()
                    {
                        Base = 0,
                        Importe = Convert.ToDouble(c40.Impuestos.Retenciones[0].Importe),
                        Impuesto = c40.Impuestos.Retenciones[0].Impuesto,
                        TasaOCuota = 0,
                        TipoFactor = ""
                    };
                }
            }

            Retenciones _retenciones = null;
            _retenciones = new Retenciones()
            {
                Retencion = _retencion
            };

            Traslado _traslado = null;
            //nota: en v.4 los traslados van multiples
            if (c40.Impuestos != null)
            {
                if (c40.Impuestos.Traslados != null)
                {
                    _traslado = new Traslado()
                    {
                        Base = Convert.ToDouble(c40.Impuestos.Traslados[0].Base),
                        Importe = Convert.ToDouble(c40.Impuestos.Traslados[0].Importe),
                        Impuesto = Convert.ToInt32(c40.Impuestos.Traslados[0].Impuesto),
                        TasaOCuota = Convert.ToDouble(c40.Impuestos.Traslados[0].TasaOCuota),
                        TipoFactor = c40.Impuestos.Traslados[0].TipoFactor
                    };
                }
            }

            Traslados _traslados = null;
            _traslados = new Traslados()
            {
                Traslado = _traslado
            };

            Impuestos _impuestos = new Impuestos()
            {
                Retenciones = _retenciones,
                Traslados = _traslados,
                TotalImpuestosRetenidos = Convert.ToDouble(c40.Impuestos.TotalImpuestosRetenidos),
                TotalImpuestosTrasladados = Convert.ToDouble(c40.Impuestos.TotalImpuestosTrasladados)
            };

            ComprobanteBE c = new ComprobanteBE()
            {
                Addenda = _addenda,
                Complemento = _complemento,
                Conceptos = _conceptos,
                Certificado = c40.Certificado,
                Cfdi = "",
                CfdiRelacionados = _cfdiRelacionados,
                CondicionesDePago = c40.CondicionesDePago,
                Emisor = _emisor,
                Fecha = c40.Fecha.ToString("yyyy-MM-dd hh:mm:ss"),
                FileName = "",
                Folio = c40.Folio,
                FormaPago = c40.FormaPago,
                Impuestos = _impuestos,
                LugarExpedicion = c40.LugarExpedicion,
                MetodoPago = c40.MetodoPago,
                Moneda = c40.Moneda,
                NoCertificado = c40.NoCertificado,
                Receptor = _receptor,
                SchemaLocation = "",
                Sello = c40.Sello,
                Serie = c40.Serie,
                SubTotal = Convert.ToDouble(c40.SubTotal),
                TipoDeComprobante = c40.TipoDeComprobante,
                Total = Convert.ToDouble(c40.Total),
                Version = c40.Version,
                Xsi = ""
            };

            return c;
        }
    }
}

using BERecepcion.Core.Consulta.Copades.Dto;
using BERecepcion.Core.Dto;
using BERecepcion.Core.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace BERecepcion.Infraestructura.Utils
{
    public class ArmaPreFacturaPemex
    {
        public string xmlResult { get; set; }
        public ArmaPreFacturaPemex(CopadePreFactura resultItem)
        {
            //XmlSerializerNamespaces xmlNamespace = new XmlSerializerNamespaces();
            //xmlNamespace.Add("cfdi", "http://www.sat.gob.mx/cfd/3");            
            //xmlNamespace.Add("xsi", "http://www.w3.org/2001/XMLSchema.instance");

            PreFactura_item PreFacturaXML = new PreFactura_item();
            PreFactura_itemComprobante Comprobante = new PreFactura_itemComprobante();
            Comprobante.SubTotal = resultItem.comprobante.subtotal;
            Comprobante.Total = resultItem.comprobante.total;
            Comprobante.Moneda = resultItem.comprobante.moneda;
            PreFacturaXML.Comprobante = Comprobante;            
            List<PreFactura_itemComprobanteConcepto> Conceptos = new List<PreFactura_itemComprobanteConcepto>();

            foreach (var item in resultItem.comprobante.conceptos)
            {
                PreFactura_itemComprobanteConcepto concepto = new PreFactura_itemComprobanteConcepto()
                {
                    Posicion = item.posicion,
                    Descripcion = item.descripcion,
                    Importe = item.importe,
                    Cantidad = item.cantidad,
                    Unidad = item.unidad,
                    ValorUnitario = item.valorUnitario
                };
                Conceptos.Add(concepto);
            }
            PreFacturaXML.Comprobante.Conceptos = Conceptos.ToArray();

            if (resultItem.comprobante.impuestos!=null)
            {
                List<PreFactura_itemComprobanteImpuestosRetencion> ListRetenciones = new List<PreFactura_itemComprobanteImpuestosRetencion>();
                if (resultItem.comprobante.impuestos.retenciones != null)
                    foreach (var item in resultItem.comprobante.impuestos.retenciones.Retencion)
                    {
                        PreFactura_itemComprobanteImpuestosRetencion retencion = new PreFactura_itemComprobanteImpuestosRetencion();
                        retencion.Importe = item.importe;
                        retencion.Impuesto = item.impuesto;
                        ListRetenciones.Add(retencion);
                    }

                List<PreFactura_itemComprobanteImpuestosTraslado> ListTraslados = new List<PreFactura_itemComprobanteImpuestosTraslado>();
                if (resultItem.comprobante.impuestos.traslados != null)
                    foreach (var item in resultItem.comprobante.impuestos.traslados.Traslado)
                    {
                        PreFactura_itemComprobanteImpuestosTraslado tralsado = new PreFactura_itemComprobanteImpuestosTraslado();
                        tralsado.Importe = item.importe;
                        tralsado.Impuesto = item.impuesto;
                        tralsado.Tasa = item.tasa;
                        ListTraslados.Add(tralsado);
                    }

                PreFactura_itemComprobanteImpuestos impuesto = new PreFactura_itemComprobanteImpuestos()
                {
                    TotalImpuestosRetenidos = resultItem.comprobante.impuestos.totalImpuestosRetenidos,
                    TotalImpuestosTrasladados = resultItem.comprobante.impuestos.totalImpuestosTrasladados,
                    retenciones = ListRetenciones.ToArray(),
                    traslados = ListTraslados.ToArray()
                };

                PreFacturaXML.Comprobante.impuestos = impuesto; 
            }

            PreFacturaXML.Addenda = new PreFactura_itemAddenda() { 
                Addenda_Pemex = new Addenda_item()
                {
                    CONTRATO = resultItem.Addenda.Addenda_Pemex.CONTRATO,
                    O_SURTIMIENTO = resultItem.Addenda.Addenda_Pemex.O_SURTIMIENTO,
                    N_ESTIMACION = resultItem.Addenda.Addenda_Pemex.N_ESTIMACION,
                    P_ESTIMACION = resultItem.Addenda.Addenda_Pemex.P_ESTIMACION,
                    N_ACREEDOR = resultItem.Addenda.Addenda_Pemex.N_ACREEDOR,
                    C_GESTOR = resultItem.Addenda.Addenda_Pemex.C_GESTOR,
                    DOSALMILLAR = resultItem.Addenda.Addenda_Pemex.DOSALMILLAR,
                    FINIQUITO = resultItem.Addenda.Addenda_Pemex.FINIQUITO,
                    POSICIONAP = resultItem.Addenda.Addenda_Pemex.POSICIONAP,
                    AUTORIZA = resultItem.Addenda.Addenda_Pemex.AUTORIZA,
                    EJERCICIO = resultItem.Addenda.Addenda_Pemex.EJERCICIO,
                    ENTRADA = resultItem.Addenda.Addenda_Pemex.ENTRADA,
                    CEJECUTOR = resultItem.Addenda.Addenda_Pemex.CEJECUTOR,
                    RECEPSAP = resultItem.Addenda.Addenda_Pemex.RECEPSAP,
                    PLAZO = resultItem.Addenda.Addenda_Pemex.PLAZO,
                    RFCPROVEEDOR = resultItem.Addenda.Addenda_Pemex.RFCPROVEEDOR,
                    REMESA = resultItem.Addenda.Addenda_Pemex.REMESA,
                    NREMISION = resultItem.Addenda.Addenda_Pemex.NREMISION,
                    VUREGION = resultItem.Addenda.Addenda_Pemex.VUREGION,
                    FICHAE = resultItem.Addenda.Addenda_Pemex.FICHAE,
                    FICHAF = resultItem.Addenda.Addenda_Pemex.FICHAF,
                    MONEDA = resultItem.Addenda.Addenda_Pemex.MONEDA,
                    FONDO = resultItem.Addenda.Addenda_Pemex.FONDO,
                    POSICIONF = resultItem.Addenda.Addenda_Pemex.POSICIONF,
                    OCOMERCIAL = resultItem.Addenda.Addenda_Pemex.OCOMERCIAL,
                    SERVICIOG = resultItem.Addenda.Addenda_Pemex.SERVICIOG,
                    SERVICIOA = resultItem.Addenda.Addenda_Pemex.SERVICIOA,
                    CORREOPMI = resultItem.Addenda.Addenda_Pemex.CORREOPMI
                }
            };

            XmlSerializer oxmlSerializer = new XmlSerializer(typeof(PreFactura_item));
            using (var sww = new StringWriterWithEncoding(System.Text.Encoding.UTF8))
            {
                using (XmlWriter writter = XmlWriter.Create(sww))
                {
                    //oxmlSerializer.Serialize(writter, PreFacturaXML, xmlNamespace);
                    oxmlSerializer.Serialize(writter, PreFacturaXML);
                    //byte[] bytes = System.Text.Encoding.Default.GetBytes(sww.ToString());
                    //xmlResult = System.Text.Encoding.UTF8.GetString(bytes);
                    xmlResult = sww.ToString();
                }
            }
        }
    }
}

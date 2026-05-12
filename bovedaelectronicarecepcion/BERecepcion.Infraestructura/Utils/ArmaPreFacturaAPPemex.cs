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
    public class ArmaPreFacturaAPPemex
    {
        public string xmlResult { get; set; }

        public ArmaPreFacturaAPPemex(APPreFactura resultItem)
        {
            //XmlSerializerNamespaces xmlNamespace = new XmlSerializerNamespaces();
            //xmlNamespace.Add("cfdi", "http://www.sat.gob.mx/cfd/3");            
            //xmlNamespace.Add("xsi", "http://www.w3.org/2001/XMLSchema.instance");
            PreFacturaAP_item PreFacturaXML = new PreFacturaAP_item();
            PreFacturaAP_itemComprobante Comprobante = new PreFacturaAP_itemComprobante();
            Comprobante.Subtotal = resultItem.comprobante.subtotal;
            Comprobante.Total = resultItem.comprobante.total;
            Comprobante.Moneda = resultItem.comprobante.moneda;
            PreFacturaXML.Comprobante = Comprobante;
            List<PreFacturaAP_itemComprobanteConcepto> Conceptos = new List<PreFacturaAP_itemComprobanteConcepto>();

            foreach (var item in resultItem.comprobante.conceptos)
            {
                PreFacturaAP_itemComprobanteConcepto concepto = new PreFacturaAP_itemComprobanteConcepto()
                {
                    Cve_Producto = item.cve_producto,
                    Descripcion = item.descripcion,
                    Importe = item.importe,
                    Cantidad = item.cantidad,
                    Unidad = item.unidad,
                    ValorUnitario = item.valorUnitario
                };
                Conceptos.Add(concepto);
            }
            PreFacturaXML.Comprobante.Conceptos = Conceptos.ToArray();

            List<PreFacturaAP_itemComprobanteImpuestosRetencion> ListRetenciones = new List<PreFacturaAP_itemComprobanteImpuestosRetencion>();
            if (resultItem.comprobante.impuestos.retenciones != null)
                foreach (var item in resultItem.comprobante.impuestos.retenciones.Retencion)
                {
                    PreFacturaAP_itemComprobanteImpuestosRetencion retencion = new PreFacturaAP_itemComprobanteImpuestosRetencion();
                    retencion.Importe = item.importe;
                    retencion.Impuesto = item.impuesto;
                    ListRetenciones.Add(retencion);
                }

            List<PreFacturaAP_itemComprobanteImpuestosTraslado> ListTraslados = new List<PreFacturaAP_itemComprobanteImpuestosTraslado>();
            if (resultItem.comprobante.impuestos.traslados != null)
                foreach (var item in resultItem.comprobante.impuestos.traslados.Traslado)
                {
                    PreFacturaAP_itemComprobanteImpuestosTraslado tralsado = new PreFacturaAP_itemComprobanteImpuestosTraslado();
                    tralsado.Importe = item.importe;
                    tralsado.Impuesto = item.impuesto;
                    tralsado.Tasa = item.tasa;
                    ListTraslados.Add(tralsado);
                }

            PreFacturaAP_itemComprobanteImpuestos impuesto = new PreFacturaAP_itemComprobanteImpuestos()
            {
                TotalImpuestosRetenidos = resultItem.comprobante.impuestos.totalImpuestosRetenidos,
                TotalImpuestosTrasladados = resultItem.comprobante.impuestos.totalImpuestosTrasladados,
                Retenciones = ListRetenciones.ToArray(),
                Traslados = ListTraslados.ToArray()
            };

            PreFacturaXML.Comprobante.Impuestos = impuesto;

            PreFacturaXML.Addenda = new PreFacturaAP_itemAddenda()
            {
                Addenda_Pemex = new AddendaAP_item()
                {
                    N_ACREEDOR = resultItem.Addenda.Addenda_Pemex.N_ACREEDOR,
                    EJERCICIO = resultItem.Addenda.Addenda_Pemex.EJERCICIO,
                    CLAVE_TRANSP = resultItem.Addenda.Addenda_Pemex.CLAVE_TRANSP,
                    A_RELACION = resultItem.Addenda.Addenda_Pemex.A_RELACION,
                    ID_ANALITICO = resultItem.Addenda.Addenda_Pemex.ID_ANALITICO,
                    TIPO_PRODUCTO = resultItem.Addenda.Addenda_Pemex.TIPO_PRODUCTO,
                    CEDULA = resultItem.Addenda.Addenda_Pemex.CEDULA,
                    CONTRATO_SIIC = resultItem.Addenda.Addenda_Pemex.CONTRATO_SIIC,
                    ANALITICO = resultItem.Addenda.Addenda_Pemex.ANALITICO
                }
            };

            XmlSerializer oxmlSerializer = new XmlSerializer(typeof(PreFacturaAP_item));
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

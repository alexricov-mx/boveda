using BERecepcion.Core.Consulta.Copades.Dto;
using BERecepcion.Core.Dto;
using BERecepcion.Core.Facturas.Dto;
using BERecepcion.Core.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace BERecepcion.Infraestructura.Utils
{
    public class ArmaNotaCreditoPemex
    {
        public string xmlResult { get; set; }

        public ArmaNotaCreditoPemex(CopadeNotasCredito resultList)
        {

            if (resultList == null)
            {
                xmlResult = null;
                return ;
            }            

            NotasCreditoList NotaCreditoXML = new NotasCreditoList();
            
            foreach(var item in resultList.notaCredito)
            {
                NotaCredito notaItem = new NotaCredito() {                     
                    Concepto = item.Concepto,
                    Tipo = item.Tipo,
                    Cantidad = item.Cantidad,
                    Importe = item.Importe,
                    Iva = item.Iva,
                    Total = item.Total
                };

                NotaCreditoXML.NotasCredito.Add(notaItem);
            }
           
            XmlSerializer oxmlSerializer = new XmlSerializer(typeof(NotasCreditoList));
            using (var sww = new StringWriterWithEncoding(System.Text.Encoding.UTF8))
            {
                using (XmlWriter writter = XmlWriter.Create(sww))
                {
                    //oxmlSerializer.Serialize(writter, PreFacturaXML, xmlNamespace);
                    oxmlSerializer.Serialize(writter, NotaCreditoXML);
                    //byte[] bytes = System.Text.Encoding.Default.GetBytes(sww.ToString());
                    //xmlResult = System.Text.Encoding.UTF8.GetString(bytes);
                    xmlResult = sww.ToString();
                }
            }
        }
    }
}

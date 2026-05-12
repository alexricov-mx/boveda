using BERecepcion.Core.Correos.Dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace BERecepcion.Infraestructura.Utils
{
    public class BodyCorreos
    {
        //Creacion de usuario
        //Desbloqueo de Usuario
        public static string adminUsuarios(CorreoDto correoDto, string URLSitio)
        {
            string bodyMensaje = "";
            if (correoDto.Accion.Equals("Creacion de Usuario"))
                bodyMensaje = "<p>Se le comunica que se le ha asignado una cuenta del sistema de Bóveda Electrónica Recepción On Premise.</p>";
            else if (correoDto.Accion.Equals("Desbloqueo de Usuario"))
                bodyMensaje = "<p>Se le comunica que su cuenta del sistema de Bóveda Electrónica Recepción On Premise se encuentra activa.</p>";

            string Body = "<!DOCTYPE html>" +
                        "<html lang='en'>" +
                            "<head>" +
                                "<meta charset='UTF-8'>" +
                                "<meta http-equiv='X-UA-Compatible' content='IE=edge'>" +
                                "<meta name='viewport' content='width=device-width, initial-scale=1.0'>" +
                                "<title>BER</title>" +
                                    "<style type='text/css'>" +
                                        "body {" +
                                            "color: black;" +
                                            "background-color: rgb(236, 236, 236)" +
                                        "}" +
                                        "span {" +
                                            "font-weight: bold;" +
                                        "}" +
                                        "#franja {" +
                                            "color: rgb(158, 40, 40); "+
                                            "background-color: red; " +
                                            "height: 10px;" + 
                                            "width: 1000px;" +
                                        "}" +
                                    "</style>" +
                            "</head>" +
                            "<body>" +
                                "<div>" +
                                    "<h1>Bóveda Electrónica Recepción</h1>" +
                                    "<div id='franja'></div>" +
                                    $"<h2>{correoDto.Accion}</h2>" +
                                "</div>" +
                                "<div>" +
                                    $"<p>Estimado: <span>{correoDto.Nombre}</span></p>" +
                                    $"<p>{bodyMensaje}</p>" +                                    
                                    $"<p>Usuario: <span>{correoDto.userName}</span></p>" +
                                "</div>" +                               
                            "</body>" +
                            "<footer>" +
                                "<div>" +
                                    "<p>Atentamente:</p>" +
                                    $"<a href='{URLSitio}'>Bóveda Electrónica Recepción On Premise</a>" +
                                "</div>" +
                            "</footer>" +
                            "</html>";
            return Body;
        }
        
        
        //Contrato Pendiente de Firma
        //Orden de Surtimiento Pendiente de Firma
        //Estimacion de Obra
        //Recepcion
        public static string documentoPemexPendiente(CorreoDto correoDto, string URLSitio)
        {
            string bodyDocumento = $"<p>Contrato: <span>{correoDto.Contract}</span></p>";
            if (correoDto.SapOrder != null)
                bodyDocumento = $"<p>Orden de Surtimiento: <span>{correoDto.Contract}</span></p>";

            if (correoDto.Estimacion != null)
                bodyDocumento = $"<p>Estimacion de Obra: <span>{correoDto.Estimacion}</span></p>";

            if (correoDto.Reception != null)
                bodyDocumento = $"<p>Recepción: <span>{correoDto.Reception}</span></p>";

            if (correoDto.Copade != null)
                bodyDocumento = $"<p>COPADE: <span>{correoDto.Copade}</span></p>";

            if (correoDto.AnaliticoPago != null)
                bodyDocumento = $"<p>Analitico de Pago: <span>{correoDto.AnaliticoPago}</span></p>";

            if (correoDto.ProgramaPago != null)
                bodyDocumento = $"<p>Programa de Pago: <span>{correoDto.ProgramaPago}</span></p>";

            if (correoDto.ListaPago != null)
                bodyDocumento = $"<p>Lista de Pago: <span>{correoDto.ListaPago}</span></p>";

            string Body = "<!DOCTYPE html>" +
                        "<html lang='en'>" +
                            "<head>" +
                                "<meta charset='UTF-8'>" +
                                "<meta http-equiv='X-UA-Compatible' content='IE=edge'>" +
                                "<meta name='viewport' content='width=device-width, initial-scale=1.0'>" +
                                "<title>BER</title>" +
                                    "<style type='text/css'>" +
                                        "body {" +
                                            "color: black;" +
                                            "background-color: rgb(236, 236, 236)" +
                                        "}" +
                                        "span {" +
                                            "font-weight: bold;" +
                                        "}" +
                                        "#franja {" +
                                            "color: rgb(158, 40, 40); " +
                                            "background-color: red; " +
                                            "height: 10px;" +
                                            "width: 1000px;" +
                                        "}" +
                                    "</style>" +
                            "</head>" +
                            "<body>" +
                                "<div>" +
                                    "<h1>Bóveda Electrónica Recepción</h1>" +
                                    "<div id='franja'></div>" +
                                    $"<h2>{correoDto.Accion}</h2>" +
                                "</div>" +
                                "<div>" +
                                    $"<p>Estimado: <span>{correoDto.Nombre}</span></p>" +
                                    "<p>Se le comunica que tiene un documento pendiente de firma en el sistema de Bóveda Electrónica Recepción On Premise.</p>" +
                                    $"<p>{bodyDocumento}</p>" +                                    
                                "</div>" +
                            "</body>" +
                            "<footer>" +
                                "<div>" +
                                    "<p>Atentamente:</p>" +
                                    $"<a href='{URLSitio}'>Bóveda Electrónica Recepción On Premise</a>" +
                                "</div>" +
                            "</footer>" +
                            "</html>";

            return Body;
        }
        //Contrato Pendiente de Firma Proveedor
        //Orden de Surtimiento Pendiente de Firma Proveedor

        //Estimacion de Obra Proveedor

        //Copade Firma 1/Suplente 1

        //Copade Firma 2/Suplente 2

        //Analitico de Pago

        //Correos de Recepcion de Factura - Revisar cuales se necesitan

        //Programa de Pago

        //Lista de Pago
    }
}

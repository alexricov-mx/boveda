using BERecepcion.Core.Dto;
using BERecepcion.Infraestructura.Utils;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Data;
using Dapper;
using Serilog;
using BERecepcion.Core.Correos.Dto;
using BERecepcion.Core.Correos.Interfaces.Repositories;
using BERecepcion.Core.Instrucciones.Dto;
using BERecepcion.Core.Facturas.Dto;
using BERecepcion.Core.OrdenSurtimiento.Dto;
using BERecepcion.Core.SAPPI.Dto;
using BERecepcion.Infraestructura.Repositories;
using BERecepcion.Core.Admin.Interfaces.Repositories;
using BERecepcion.Core.Admin.Dto;
using BERecepcion.Core.Consulta.Copades.Dto;

namespace BERecepcion.Infraestructura.Correos.Repositories
{
    public class CorreoRepository : BaseSQLServerSqlRepository, ICorreoRepository
    {
        private readonly IConfiguration _configuration;
        private readonly IBitacoraRepository _bitacoraRepository;
        public CorreoRepository(string cnnString, IConfiguration configuration, IBitacoraRepository bitacoraRepository) : base(cnnString)
        {
            _configuration = configuration;
            _bitacoraRepository = bitacoraRepository;
        }
        public async Task<DataResult<string>> EnvioCorreoAsync(CorreoDto correoMensaje)
        {
            string sitiodireccion, politicadeprivacidaddireccion, politicadeprivacidaddescripcion;
            sitiodireccion = politicadeprivacidaddireccion = politicadeprivacidaddescripcion = "";

            try
            {
                sitiodireccion = _configuration["Email:SitioDireccion"];
                politicadeprivacidaddireccion = _configuration["Email:PoliticaDePrivacidadDireccion"];
                politicadeprivacidaddescripcion = _configuration["Email:PoliticaDePrivacidadDescripcion"];
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }

            string strBody = "";
            DataResult<string> resultItemIenum = new DataResult<string>()
            {
                Status = System.Net.HttpStatusCode.OK,
                Message = "Envío de Correo Exitoso"
            };

            using (var client = new SmtpClient())
            {
                client.Host = _configuration["Email:Host"];
                using (var emailMessage = new MailMessage())
                {
                    string textBody = "Contrato: <span>" + correoMensaje.Contract + "</span>";
                    if (!string.IsNullOrEmpty(correoMensaje.SapOrder))
                        textBody = "Orden de Surtimiento: <span>" + correoMensaje.Contract + "</span>";

                    if (!string.IsNullOrEmpty(correoMensaje.Estimacion))
                        textBody = "Estimacion de Obra: <span>" + correoMensaje.Estimacion + "</span>";

                    if (!string.IsNullOrEmpty(correoMensaje.Reception))
                        textBody = "Recepción: <span>" + correoMensaje.Reception + "</span>";

                    if (!string.IsNullOrEmpty(correoMensaje.Copade))
                        textBody = "COPADE: <span>" + correoMensaje.Copade + "</span>";

                    if (!string.IsNullOrEmpty(correoMensaje.AnaliticoPago))
                        textBody = "Analitico de Pago: <span>" + correoMensaje.AnaliticoPago + "</span>";

                    if (!string.IsNullOrEmpty(correoMensaje.ProgramaPago))
                        textBody = "Programa de Pago: <span>" + correoMensaje.ProgramaPago + "</span>";

                    if (!string.IsNullOrEmpty(correoMensaje.ListaPago))
                        textBody = "Lista de Pago: <span>" + correoMensaje.ListaPago + "</span>";

                    strBody = await NotificacionTemplate("CorreoRepository", "EnvioCorreoAsync", false);
                    emailMessage.To.Add(new MailAddress(correoMensaje.To));
                    strBody = strBody.Replace("[Nombre]", correoMensaje.Nombre);
                    strBody = strBody.Replace("[Accion]", correoMensaje.Accion);
                    strBody = strBody.Replace("[Detalle]", textBody);
                    strBody = strBody.Replace("[sitio-direccion]", sitiodireccion);
                    strBody = strBody.Replace("[politica-de-privacidad-direccion]", politicadeprivacidaddireccion);
                    strBody = strBody.Replace("[politica-de-privacidad-descripcion]", politicadeprivacidaddescripcion);
                    emailMessage.From = new MailAddress(_configuration["Email:From"]);
                    emailMessage.Bcc.Add(new MailAddress(_configuration["Email:Bcc"]));
                    emailMessage.Priority = MailPriority.High;
                    emailMessage.Subject = correoMensaje.Subject;
                    emailMessage.Body = strBody;
                    emailMessage.IsBodyHtml = true;
                    client.Send(emailMessage);
                    resultItemIenum.Data = "EXITO";
                }
            }
            await Task.CompletedTask;
            return resultItemIenum;
        }
        public async Task<bool> NotificacionOSAsync(IEnumerable<UsersDto> users, SupplyOrderDto supplyOrder, string subject)
        {
            string sitiodireccion, politicadeprivacidaddireccion, politicadeprivacidaddescripcion;
            sitiodireccion = politicadeprivacidaddireccion = politicadeprivacidaddescripcion = "";

            try
            {
                sitiodireccion = _configuration["Email:SitioDireccion"];
                politicadeprivacidaddireccion = _configuration["Email:PoliticaDePrivacidadDireccion"];
                politicadeprivacidaddescripcion = _configuration["Email:PoliticaDePrivacidadDescripcion"];
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }

            //string strBody = System.IO.File.ReadAllText("HtmlTemplates/NotificacionFirmasOS.html");
            string strBody = await NotificacionTemplate("CorreoRepository", "NotificacionOSAsync", false);

            using (var client = new SmtpClient())
            {
                client.Host = _configuration["Email:Host"];
                var emailMessage = new MailMessage();

                strBody = strBody.Replace("[TipoNotificacion]", subject);
                strBody = strBody.Replace("[Contrato]", supplyOrder.Contract);
                strBody = strBody.Replace("[Orden]", supplyOrder.SAPOrder);
                strBody = strBody.Replace("[sitio-direccion]", sitiodireccion);
                strBody = strBody.Replace("[politica-de-privacidad-direccion]", politicadeprivacidaddireccion);
                strBody = strBody.Replace("[politica-de-privacidad-descripcion]", politicadeprivacidaddescripcion);

                string tempBody = strBody;
                try
                {
                    foreach (var item in users)
                    {
                        emailMessage.From = new MailAddress(_configuration["Email:From"]);
                        emailMessage.Priority = MailPriority.High;
                        emailMessage.IsBodyHtml = true;
                        emailMessage.Bcc.Add(new MailAddress(_configuration["Email:Bcc"]));
                        emailMessage.Subject = subject;
                        emailMessage.To.Clear();
                        emailMessage.To.Add(new MailAddress(item.Email));
                        tempBody = tempBody.Replace("[Usuario]", item.Name);
                        emailMessage.Body = tempBody;
                        client.Send(emailMessage);
                        await Task.CompletedTask;
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex.Message);
                    return false;
                }
                return true;
            }
        }

        public async Task<bool> EnvioCorreoArchivosAsync(DtoPdfFilesEmail dtoPdfFilesEmail)
        {
            bool EmailResponse = true;
            using (var client = new SmtpClient())
            {
                client.Host = _configuration["Email:Host"];

                string sitiodireccion, politicadeprivacidaddireccion, politicadeprivacidaddescripcion;
                sitiodireccion = politicadeprivacidaddireccion = politicadeprivacidaddescripcion = "";

                try
                {
                    sitiodireccion = _configuration["Email:SitioDireccion"];
                    politicadeprivacidaddireccion = _configuration["Email:PoliticaDePrivacidadDireccion"];
                    politicadeprivacidaddescripcion = _configuration["Email:PoliticaDePrivacidadDescripcion"];
                }
                catch (Exception ex)
                {
                    string msg = ex.Message;
                }

                //string strBody = System.IO.File.ReadAllText("HtmlTemplates/NotificacionCopadeArchivos.html");
                string strBody = await NotificacionTemplate("CorreoRepository", "EnvioCorreoArchivosAsync", false);

                try
                {
                    foreach (var item in dtoPdfFilesEmail.users)
                    {
                        using (var emailMessage = new MailMessage())
                        {
                            string documento = "";
                            if (dtoPdfFilesEmail.copade != null)
                            {
                                documento = dtoPdfFilesEmail.copade.Reception;
                            }
                            else
                            {
                                if (dtoPdfFilesEmail.analitico != null)
                                {
                                    documento = dtoPdfFilesEmail.analitico.IdAnalitico;
                                }
                            }

                            emailMessage.To.Add(new MailAddress(item.Email));
                            strBody = strBody.Replace("[Usuario]", item.Name);
                            strBody = strBody.Replace("[Documento]", documento);
                            strBody = strBody.Replace("[sitio-direccion]", sitiodireccion);
                            strBody = strBody.Replace("[politica-de-privacidad-direccion]", politicadeprivacidaddireccion);
                            strBody = strBody.Replace("[politica-de-privacidad-descripcion]", politicadeprivacidaddescripcion);

                            foreach (var arch in dtoPdfFilesEmail.archivos)
                            {
                                string filename = @arch;
                                Attachment data = new Attachment(filename, MediaTypeNames.Application.Octet);
                                emailMessage.Attachments.Add(data);
                            }

                            emailMessage.From = new MailAddress(_configuration["Email:From"]);
                            emailMessage.Bcc.Add(new MailAddress(_configuration["Email:Bcc"]));
                            if (dtoPdfFilesEmail.representatives != null)
                            {
                                foreach (var rpr in dtoPdfFilesEmail.representatives)
                                {
                                    emailMessage.Bcc.Add(new MailAddress(rpr.Email));
                                }
                            }
                            emailMessage.Priority = MailPriority.High;
                            emailMessage.Subject = dtoPdfFilesEmail.correoMensaje.Subject;
                            emailMessage.Body = strBody;
                            emailMessage.IsBodyHtml = true;

                            try
                            {
                                client.Send(emailMessage);
                            }
                            catch (Exception ex)
                            {
                                EmailResponse = false;
                                Log.Error(ex.Message);
                            }
                            await Task.CompletedTask;
                        }
                    }
                }
                catch (Exception exx)
                {
                    Log.Error(exx.Message);
                    return EmailResponse;
                }
                return EmailResponse;
            }
        }
        public async Task<bool> NotificacionCOPADEAsync(IEnumerable<UsersDto> users, CopadeDto copade, string subject)
        {
            //string strBody = System.IO.File.ReadAllText("HtmlTemplates/NotificacionFirmasCOPADE.html");
            string strBd = await NotificacionTemplate("CorreoRepository", "NotificacionCOPADEAsync", false);

            using (var client = new SmtpClient())
            {
                client.Host = _configuration["Email:Host"];

                string sitiodireccion, politicadeprivacidaddireccion, politicadeprivacidaddescripcion;
                sitiodireccion = politicadeprivacidaddireccion = politicadeprivacidaddescripcion = "";

                try
                {
                    sitiodireccion = _configuration["Email:SitioDireccion"];
                    politicadeprivacidaddireccion = _configuration["Email:PoliticaDePrivacidadDireccion"];
                    politicadeprivacidaddescripcion = _configuration["Email:PoliticaDePrivacidadDescripcion"];
                }
                catch (Exception ex)
                {
                    Log.Error(ex.Message);
                }

                var emailMessage = new MailMessage();

                try
                {
                    foreach (var item in users)
                    {
                        emailMessage.From = new MailAddress(_configuration["Email:From"]);
                        emailMessage.Priority = MailPriority.High;
                        emailMessage.IsBodyHtml = true;
                        emailMessage.Bcc.Add(new MailAddress(_configuration["Email:Bcc"]));
                        emailMessage.Subject = subject;
                        emailMessage.To.Clear();
                        emailMessage.To.Add(new MailAddress(item.Email));
                        string strBody = strBd;
                        strBody = strBody.Replace("[TipoNotificacion]", subject);
                        strBody = strBody.Replace("[Reception]", copade.Reception);
                        strBody = strBody.Replace("[sitio-direccion]", sitiodireccion);
                        strBody = strBody.Replace("[politica-de-privacidad-direccion]", politicadeprivacidaddireccion);
                        strBody = strBody.Replace("[politica-de-privacidad-descripcion]", politicadeprivacidaddescripcion);
                        strBody = strBody.Replace("[Usuario]", item.Name);
                        emailMessage.Body = strBody;
                        client.Send(emailMessage);
                        await Task.CompletedTask;
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex.Message);
                    return false;
                }
                return true;
            }
        }

        public async Task<bool> NotificacionESAsync(IEnumerable<UsersDto> users, IEnumerable<UsersDto> usersRepresentative, SOEstimationDto estimation, string subject)
        {
            string sitiodireccion, politicadeprivacidaddireccion, politicadeprivacidaddescripcion;
            sitiodireccion = politicadeprivacidaddireccion = politicadeprivacidaddescripcion = "";

            try
            {
                sitiodireccion = _configuration["Email:SitioDireccion"];
                politicadeprivacidaddireccion = _configuration["Email:PoliticaDePrivacidadDireccion"];
                politicadeprivacidaddescripcion = _configuration["Email:PoliticaDePrivacidadDescripcion"];
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }

            //string strBody = System.IO.File.ReadAllText("HtmlTemplates/NotificacionFirmasES.html");
            string strBody = await NotificacionTemplate("CorreoRepository", "NotificacionESAsync", false);

            using (var client = new SmtpClient())
            {
                client.Host = _configuration["Email:Host"];
                var emailMessage = new MailMessage();

                strBody = strBody.Replace("[TipoNotificacion]", subject);
                strBody = strBody.Replace("[Contrato]", estimation.Contract);
                strBody = strBody.Replace("[Estimacion]", estimation.SapOrder);
                strBody = strBody.Replace("[NoAcreedor]", estimation.CreditorNumber);
                strBody = strBody.Replace("[sitio-direccion]", sitiodireccion);
                strBody = strBody.Replace("[politica-de-privacidad-direccion]", politicadeprivacidaddireccion);
                strBody = strBody.Replace("[politica-de-privacidad-descripcion]", politicadeprivacidaddescripcion);

                string tempBody = strBody;
                try
                {
                    foreach (var item in users)
                    {
                        emailMessage.From = new MailAddress(_configuration["Email:From"]);
                        emailMessage.Priority = MailPriority.High;
                        emailMessage.IsBodyHtml = true;
                        emailMessage.Bcc.Add(new MailAddress(_configuration["Email:Bcc"]));
                        if (usersRepresentative != null)
                        {
                            foreach (var ur in usersRepresentative)
                            {
                                var u = users.Where(x => x.Email == ur.Email).FirstOrDefault();
                                if (u == null)
                                {
                                    emailMessage.Bcc.Add(new MailAddress(ur.Email));
                                }
                            }
                        }
                        emailMessage.Subject = subject;
                        emailMessage.To.Clear();
                        emailMessage.To.Add(new MailAddress(item.Email));
                        tempBody = tempBody.Replace("[Proveedor]", item.Company);
                        emailMessage.Body = tempBody;
                        client.Send(emailMessage);
                        await Task.CompletedTask;
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex.Message);
                    return false;
                }
                return true;
            }
        }
        public async Task<bool> NotificacionREAsync(IEnumerable<UsersDto> users, ReceptionDto recepcion, string subject)
        {
            string sitiodireccion, politicadeprivacidaddireccion, politicadeprivacidaddescripcion;
            sitiodireccion = politicadeprivacidaddireccion = politicadeprivacidaddescripcion = "";

            try
            {
                sitiodireccion = _configuration["Email:SitioDireccion"];
                politicadeprivacidaddireccion = _configuration["Email:PoliticaDePrivacidadDireccion"];
                politicadeprivacidaddescripcion = _configuration["Email:PoliticaDePrivacidadDescripcion"];
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }

            //string strBody = System.IO.File.ReadAllText("HtmlTemplates/NotificacionFirmasRE.html");
            string strBody = await NotificacionTemplate("CorreoRepository", "NotificacionREAsync", false);

            using (var client = new SmtpClient())
            {
                client.Host = _configuration["Email:Host"];
                var emailMessage = new MailMessage();

                strBody = strBody.Replace("[TipoNotificacion]", subject);
                strBody = strBody.Replace("[Contrato]", recepcion.Contract);
                strBody = strBody.Replace("[SapOrder]", recepcion.SapOrder);
                strBody = strBody.Replace("[Reception]", recepcion.Reception);
                strBody = strBody.Replace("[sitio-direccion]", sitiodireccion);
                strBody = strBody.Replace("[politica-de-privacidad-direccion]", politicadeprivacidaddireccion);
                strBody = strBody.Replace("[politica-de-privacidad-descripcion]", politicadeprivacidaddescripcion);

                string tempBody = strBody;
                try
                {
                    foreach (var item in users)
                    {
                        emailMessage.From = new MailAddress(_configuration["Email:From"]);
                        emailMessage.Priority = MailPriority.High;
                        emailMessage.IsBodyHtml = true;
                        emailMessage.Bcc.Add(new MailAddress(_configuration["Email:Bcc"]));
                        emailMessage.Subject = subject;

                        emailMessage.To.Add(new MailAddress(item.Email));
                        emailMessage.Body = tempBody;
                        client.Send(emailMessage);
                        await Task.CompletedTask;
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex.Message);
                    return false;
                }
                return true;
            }
        }
        public async Task<bool> NotificacionAPAsync(IEnumerable<UsersDto> users, AnaliticoPagoDto analiticoPago, string subject)
        {
            //string strBody = System.IO.File.ReadAllText("HtmlTemplates/NotificacionFirmasAP.html");
            string strBody = await NotificacionTemplate("CorreoRepository", "NotificacionAPAsync", false);
            using (var client = new SmtpClient())
            {
                client.Host = _configuration["Email:Host"];
                string sitiodireccion, politicadeprivacidaddireccion, politicadeprivacidaddescripcion;
                sitiodireccion = politicadeprivacidaddireccion = politicadeprivacidaddescripcion = "";

                try
                {
                    sitiodireccion = _configuration["Email:SitioDireccion"];
                    politicadeprivacidaddireccion = _configuration["Email:PoliticaDePrivacidadDireccion"];
                    politicadeprivacidaddescripcion = _configuration["Email:PoliticaDePrivacidadDescripcion"];
                }
                catch (Exception ex)
                {
                    string msg = ex.Message;
                }

                var emailMessage = new MailMessage();

                strBody = strBody.Replace("[TipoNotificacion]", subject);
                strBody = strBody.Replace("[Analitico]", analiticoPago.IdAnalitico);
                strBody = strBody.Replace("[sitio-direccion]", sitiodireccion);
                strBody = strBody.Replace("[politica-de-privacidad-direccion]", politicadeprivacidaddireccion);
                strBody = strBody.Replace("[politica-de-privacidad-descripcion]", politicadeprivacidaddescripcion);

                string tempBody = strBody;
                try
                {
                    foreach (var item in users)
                    {
                        emailMessage.From = new MailAddress(_configuration["Email:From"]);
                        emailMessage.Priority = MailPriority.High;
                        emailMessage.IsBodyHtml = true;
                        emailMessage.Bcc.Add(new MailAddress(_configuration["Email:Bcc"]));
                        emailMessage.Subject = subject;
                        emailMessage.To.Clear();
                        emailMessage.To.Add(new MailAddress(item.Email));
                        tempBody = tempBody.Replace("[Usuario]", item.Name);
                        emailMessage.Body = tempBody;
                        client.Send(emailMessage);
                        await Task.CompletedTask;
                        //tempBody = strBody;
                        //emailMessage = new MailMessage();
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex.Message);
                    return false;
                }
                return true;
            }
        }
        public async Task<bool> EnvioCorreoAPArchivosAsync(DtoPdfFilesEmail dtoPdfFilesEmail)
        {
            try
            {
                using (var client = new SmtpClient())
                {
                    client.Host = _configuration["Email:Host"];

                    string sitiodireccion, politicadeprivacidaddireccion, politicadeprivacidaddescripcion;
                    sitiodireccion = politicadeprivacidaddireccion = politicadeprivacidaddescripcion = "";

                    try
                    {
                        sitiodireccion = _configuration["Email:SitioDireccion"];
                        politicadeprivacidaddireccion = _configuration["Email:PoliticaDePrivacidadDireccion"];
                        politicadeprivacidaddescripcion = _configuration["Email:PoliticaDePrivacidadDescripcion"];
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex.Message);
                    }
                    //string strBody = System.IO.File.ReadAllText("HtmlTemplates/NotificacionAPArchivos.html");
                    string strBody = await NotificacionTemplate("CorreoRepository", "EnvioCorreoAPArchivosAsync", false);

                    foreach (var item in dtoPdfFilesEmail.users)
                    {
                        using (var emailMessage = new MailMessage())
                        {
                            emailMessage.To.Add(new MailAddress(item.Email));
                            strBody = strBody.Replace("[Usuario]", item.Name);
                            strBody = strBody.Replace("[Documento]", dtoPdfFilesEmail.analitico.IdAnalitico);

                            foreach (var arch in dtoPdfFilesEmail.archivos)
                            {
                                string filename = @arch;
                                Attachment data = new Attachment(filename, MediaTypeNames.Application.Octet);
                                emailMessage.Attachments.Add(data);
                            }

                            emailMessage.From = new MailAddress(_configuration["Email:From"]);
                            emailMessage.Bcc.Add(new MailAddress(_configuration["Email:Bcc"]));
                            strBody = strBody.Replace("[sitio-direccion]", sitiodireccion);
                            strBody = strBody.Replace("[politica-de-privacidad-direccion]", politicadeprivacidaddireccion);
                            strBody = strBody.Replace("[politica-de-privacidad-descripcion]", politicadeprivacidaddescripcion);
                            emailMessage.Priority = MailPriority.High;
                            emailMessage.Subject = dtoPdfFilesEmail.correoMensaje.Subject;
                            emailMessage.Body = strBody;
                            emailMessage.IsBodyHtml = true;
                            client.Send(emailMessage);
                            await Task.CompletedTask;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return false;
            }
            return true;
        }
        public async Task<DataResult<NotificacionFacturaEmailDto>> NotificacionFacturaEmailAsync(string reception, IEnumerable<ValidationError> validationErrors, UsersDto user, bool logProcess, string repositoryName, string methodName)
        {
            DataResult<NotificacionFacturaEmailDto> resultItemIenum = new DataResult<NotificacionFacturaEmailDto>()
            {
                Status = System.Net.HttpStatusCode.OK
            };

            using (var client = new SmtpClient())
            {
                List<string> steps = new List<string>() { };

                steps.Add("500");
                client.Host = _configuration["Email:Host"];
                bool isError = false;

                if (validationErrors.ToList().Count > 0)
                {
                    var v = validationErrors.Where(x => x.clave == "60000" || x.clave == "60001").FirstOrDefault();
                    if (v == null)
                    {
                        isError = true;
                    }
                }

                string strBody = await NotificacionTemplate(repositoryName, methodName, isError);

                string detalleErrores = "";
                steps.Add("501");
                if (validationErrors.ToList().Count > 0)
                {
                    var documents = validationErrors.Select(x => x.documento).Distinct();
                    int ii = 0;
                    var recep = reception.Split(",");
                    foreach (var d in documents)
                    {
                        var r = d.Split(' ');
                        string r1, r2;
                        r1 = r2 = "";
                        if (ii < recep.Length)
                        {
                            r1 = "Documento: " + recep[ii].Trim().Replace("_", " ");
                        }
                        else
                        {
                            r1 = "Documento: " + r[0].Trim().Replace("_", " ");
                        }
                        if (r.Length > 1)
                        {
                            r2 = "<br />UUID: " + r[r.Length - 1].Trim().ToUpper();
                        }
                        detalleErrores += "<b>" + r1 + " " + r2 + "</b><hr/>";
                        var detail = validationErrors.Where(x => x.documento == d).Select(x => x.descripcion);
                        detalleErrores += "<ul>";
                        foreach (var dd in detail)
                        {
                            detalleErrores += "<li>" + dd + "</li>";
                        }
                        detalleErrores += "</ul>";
                        ii++;
                    }
                }
                else
                {
                    detalleErrores += "<b>";
                    detalleErrores += "<ul>";
                    var r = reception.Split(",");
                    foreach (var ri in r)
                    {
                        detalleErrores += "<li>" + ri + "</li>";
                    }
                    detalleErrores += "</ul>";
                    detalleErrores += "</b>";
                }
                steps.Add("502");

                string sitiodireccion, politicadeprivacidaddireccion, politicadeprivacidaddescripcion;
                sitiodireccion = politicadeprivacidaddireccion = politicadeprivacidaddescripcion = "";

                try
                {
                    sitiodireccion = _configuration["Email:SitioDireccion"];
                    politicadeprivacidaddireccion = _configuration["Email:PoliticaDePrivacidadDireccion"];
                    politicadeprivacidaddescripcion = _configuration["Email:PoliticaDePrivacidadDescripcion"];
                }
                catch (Exception ex)
                {
                    Log.Error(ex.Message);
                }

                using (var emailMessage = new MailMessage())
                {
                    steps.Add("503");
                    emailMessage.To.Add(new MailAddress(user.Email));
                    strBody = strBody.Replace("[Usuario]", user.Name);
                    strBody = strBody.Replace("[DetalleErrores]", detalleErrores);
                    strBody = strBody.Replace("[sitio-direccion]", sitiodireccion);
                    strBody = strBody.Replace("[politica-de-privacidad-direccion]", politicadeprivacidaddireccion);
                    strBody = strBody.Replace("[politica-de-privacidad-descripcion]", politicadeprivacidaddescripcion);

                    emailMessage.From = new MailAddress(_configuration["Email:From"]);
                    emailMessage.Bcc.Add(new MailAddress(_configuration["Email:Bcc"]));
                    emailMessage.Priority = MailPriority.High;
                    emailMessage.Subject = "Recepcion Electronica";
                    emailMessage.Body = strBody;
                    emailMessage.IsBodyHtml = true;
                    steps.Add("504");
                    bool sentMail = false;
                    try
                    {
                        client.Send(emailMessage);
                        await Task.CompletedTask;
                        steps.Add("505");
                        sentMail = true;
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex.Message);
                        steps.Add("506");
                    }

                    string rec = "";
                    if(reception.Contains("_"))
                    {
                        rec = reception.Split("_")[0];
                    }
                    else
                    {
                        rec = reception;
                    }
                    await _bitacoraRepository.InvoiceSentMail(sentMail, reception, user.Email);
                    steps.Add("510");

                    if (!string.IsNullOrEmpty(reception))
                    {
                        var rc = reception.Split(",");
                        foreach (var r in rc)
                        {
                            for (int i = 0; i < steps.Count; i++)
                            {
                                await _bitacoraRepository.BitacoraInvoice(r, steps[i], user, logProcess);
                            }
                        }
                    }
                }

                NotificacionFacturaEmailDto nfe = new NotificacionFacturaEmailDto();
                nfe.reception = reception;
                nfe.result = "EXITO";
                nfe.user = user;
                nfe.validationError = validationErrors;
                resultItemIenum.Data = nfe;

            }

            return resultItemIenum;

        }
        public async Task<DataResult<NotificacionFacturaEmailDto>> NotificacionFacturaAPEmailAsync(string IdAnaliticoPago, IEnumerable<ValidationError> validationErrors, UsersDto user, Guid documentoBEId, string Correo, bool logProcess, string repositoryName, string methodName)
        {
            DataResult<NotificacionFacturaEmailDto> resultItemIenum = new DataResult<NotificacionFacturaEmailDto>()
            {
                Status = System.Net.HttpStatusCode.OK
            };

            using (var client = new SmtpClient())
            {
                List<string> steps = new List<string>() { };

                steps.Add("500");
                client.Host = _configuration["Email:Host"];
                bool isError = false;

                if (validationErrors.ToList().Count > 0)
                {
                    var v = validationErrors.Where(x => x.clave == "60000" || x.clave == "60001").FirstOrDefault();
                    if (v == null)
                    {
                        isError = true;
                    }
                }

                string strBody = await NotificacionTemplate(repositoryName, methodName, isError);

                string detalleErrores = "";
                steps.Add("501");
                if (validationErrors.ToList().Count > 0)
                {
                    var documents = validationErrors.Select(x => x.documento).Distinct();
                    foreach (var d in documents)
                    {
                        detalleErrores += "<br/><br/><b>" + d + "</b><br/><br/>";
                        var detail = validationErrors.Where(x => x.documento == d).Select(x => x.descripcion);
                        detalleErrores += "<ul>";
                        foreach (var dd in detail)
                        {
                            detalleErrores += "<li>" + dd + "</li>";
                        }
                        detalleErrores += "</ul>";
                    }
                }
                else
                {
                    detalleErrores += "<br/><br/><b>";
                    detalleErrores += "<ul>";
                    var r = IdAnaliticoPago.Split(",");
                    foreach (var ri in r)
                    {
                        detalleErrores += "<li>" + r + "</li>";
                    }
                    detalleErrores += "</ul>";
                    detalleErrores += "</b><br/><br/>";
                }
                steps.Add("502");

                string sitiodireccion, politicadeprivacidaddireccion, politicadeprivacidaddescripcion;
                sitiodireccion = politicadeprivacidaddireccion = politicadeprivacidaddescripcion = "";

                try
                {
                    sitiodireccion = _configuration["Email:SitioDireccion"];
                    politicadeprivacidaddireccion = _configuration["Email:PoliticaDePrivacidadDireccion"];
                    politicadeprivacidaddescripcion = _configuration["Email:PoliticaDePrivacidadDescripcion"];
                }
                catch (Exception ex)
                {
                    Log.Error(ex.Message);
                }

                using (var emailMessage = new MailMessage())
                {
                    steps.Add("503");
                    emailMessage.To.Add(new MailAddress(Correo));
                    strBody = strBody.Replace("[Usuario]", user.Name);
                    strBody = strBody.Replace("[DetalleErrores]", detalleErrores);
                    strBody = strBody.Replace("[sitio-direccion]", sitiodireccion);
                    strBody = strBody.Replace("[politica-de-privacidad-direccion]", politicadeprivacidaddireccion);
                    strBody = strBody.Replace("[politica-de-privacidad-descripcion]", politicadeprivacidaddescripcion);

                    emailMessage.From = new MailAddress(_configuration["Email:From"]);
                    emailMessage.Bcc.Add(new MailAddress(_configuration["Email:Bcc"]));
                    emailMessage.Priority = MailPriority.High;
                    emailMessage.Subject = "Recepcion Electronica";
                    emailMessage.Body = strBody;
                    emailMessage.IsBodyHtml = true;

                    steps.Add("504");
                    bool sentMail = false;
                    try
                    {
                        client.Send(emailMessage);
                        await Task.CompletedTask;
                        steps.Add("505");
                        sentMail = true;
                    }
                    catch (Exception )
                    {
                        steps.Add("506");
                    }
                    await _bitacoraRepository.InvoiceSentMailByDocumentoBEId(sentMail, documentoBEId, Correo);
                    steps.Add("510");

                    if (!string.IsNullOrEmpty(IdAnaliticoPago))
                    {
                        var rc = IdAnaliticoPago.Split(",");
                        foreach (var r in rc)
                        {
                            for (int i = 0; i < steps.Count; i++)
                            {
                                await _bitacoraRepository.BitacoraInvoiceAP(r, documentoBEId, steps[i], user, logProcess);
                            }
                        }
                    }
                }

                NotificacionFacturaEmailDto nfe = new NotificacionFacturaEmailDto();
                nfe.reception = IdAnaliticoPago;
                nfe.result = "EXITO";
                nfe.user = user;
                nfe.validationError = validationErrors;
                resultItemIenum.Data = nfe;

            }

            return resultItemIenum;

        }
        public async Task<bool> NotificacionPPAsync(IEnumerable<UsersDto> users, PaymentScheduleDto programaPago, string subject)
        {
            string sitiodireccion, politicadeprivacidaddireccion, politicadeprivacidaddescripcion;
            sitiodireccion = politicadeprivacidaddireccion = politicadeprivacidaddescripcion = "";

            try
            {
                sitiodireccion = _configuration["Email:SitioDireccion"];
                politicadeprivacidaddireccion = _configuration["Email:PoliticaDePrivacidadDireccion"];
                politicadeprivacidaddescripcion = _configuration["Email:PoliticaDePrivacidadDescripcion"];
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }

            //string strBody = System.IO.File.ReadAllText("HtmlTemplates/NotificacionFirmasPP.html");
            string strBody = await NotificacionTemplate("CorreoRepository", "NotificacionPPAsync", false);

            using (var client = new SmtpClient())
            {
                client.Host = _configuration["Email:Host"];
                var emailMessage = new MailMessage();

                strBody = strBody.Replace("[sitio-direccion]", sitiodireccion);
                strBody = strBody.Replace("[politica-de-privacidad-direccion]", politicadeprivacidaddireccion);
                strBody = strBody.Replace("[politica-de-privacidad-descripcion]", politicadeprivacidaddescripcion);

                string tempBody = strBody;
                try
                {
                    foreach (var item in users)
                    {
                        emailMessage.From = new MailAddress(_configuration["Email:From"]);
                        emailMessage.Priority = MailPriority.High;
                        emailMessage.IsBodyHtml = true;
                        emailMessage.Bcc.Add(new MailAddress(_configuration["Email:Bcc"]));
                        emailMessage.Subject = subject;

                        emailMessage.To.Add(new MailAddress(item.Email));
                        tempBody = tempBody.Replace("[Usuario]", item.Name);
                        emailMessage.Body = tempBody;
                        client.Send(emailMessage);
                        await Task.CompletedTask;
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex.Message);
                    return false;
                }
                return true;
            }
        }
        public Task<bool> NotificacionLPAsync(IEnumerable<UsersDto> users, PaymentListDto listaPago, string subject)
        {
            throw new NotImplementedException();
        }
        public async Task<string> NotificacionTemplate(string repositoryName, string methodName, bool isError)
        {
            string result = "";

            if (!string.IsNullOrEmpty(repositoryName) && !string.IsNullOrEmpty(methodName))
            {
                try
                {

                    using (IDbConnection db = GetConnection())
                    {
                        DynamicParameters par = new DynamicParameters();
                        par.Add("@repositoryName", repositoryName);
                        par.Add("@methodName", methodName);
                        par.Add("@isError", isError);
                        result = await db.QueryFirstOrDefaultAsync<string>(sql: "SP_invoice_get_filenotification", param: par, commandType: CommandType.StoredProcedure);
                        if (result == null)
                        {
                            result = "";
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex.Message);
                    result = "";
                }
            }

            return result;
        }
        public async Task<bool> NotificacionDesvioFirma(string Correo, string UserName, System.Net.HttpStatusCode status, string Documento, string Signer, string SignerNew)
        {
            bool result = true;

            using (var client = new SmtpClient())
            {
                List<string> steps = new List<string>() { };

                client.Host = _configuration["Email:Host"];

                //string fileName = "NotificacionDesvioFirma.html";
                //if(status!=System.Net.HttpStatusCode.OK)
                //{
                //    fileName = "NotificacionDesvioFirmaError.html";
                //}

                string sitiodireccion, politicadeprivacidaddireccion, politicadeprivacidaddescripcion;
                sitiodireccion = politicadeprivacidaddireccion = politicadeprivacidaddescripcion = "";

                try
                {
                    sitiodireccion = _configuration["Email:SitioDireccion"];
                    politicadeprivacidaddireccion = _configuration["Email:PoliticaDePrivacidadDireccion"];
                    politicadeprivacidaddescripcion = _configuration["Email:PoliticaDePrivacidadDescripcion"];
                }
                catch (Exception ex)
                {
                    string msg = ex.Message;
                }

                string strBody = "";
                try
                {
                    //strBody = System.IO.File.ReadAllText("HtmlTemplates/" + fileName);
                    bool isError = false;
                    if (status != System.Net.HttpStatusCode.OK) isError = true;
                    strBody = await NotificacionTemplate("CorreoRepository", "NotificacionDesvioFirma", isError);
                }
                catch (Exception )
                {
                    strBody = "";
                }

                string tempBody = strBody;
                using (var emailMessage = new MailMessage())
                {
                    emailMessage.To.Add(new MailAddress(Correo));
                    strBody = strBody.Replace("[Usuario]", UserName);
                    strBody = strBody.Replace("[Documento]", Documento);
                    strBody = strBody.Replace("[Signer]", Signer);
                    strBody = strBody.Replace("[SignerNew]", SignerNew);
                    strBody = strBody.Replace("[sitio-direccion]", sitiodireccion);
                    strBody = strBody.Replace("[politica-de-privacidad-direccion]", politicadeprivacidaddireccion);
                    strBody = strBody.Replace("[politica-de-privacidad-descripcion]", politicadeprivacidaddescripcion);
                    emailMessage.From = new MailAddress(_configuration["Email:From"]);
                    emailMessage.Bcc.Add(new MailAddress(_configuration["Email:Bcc"]));
                    emailMessage.Priority = MailPriority.High;
                    emailMessage.Subject = "Desvio de Firma";
                    emailMessage.Body = strBody;
                    emailMessage.IsBodyHtml = true;

                    try
                    {
                        client.Send(emailMessage);
                        await Task.CompletedTask;
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex.Message);
                        result = false;
                    }
                }
            }

            return result;
        }

        public async Task<DataResult<string>> EnvioCorreoBodyCorreosAsync(CorreoDto correoMensaje)
        {
            string strBody = "";
            DataResult<string> resultItemIenum = new DataResult<string>()
            {
                Status = System.Net.HttpStatusCode.OK,
                Message = "Envío de Correo Exitoso"
            };

            using (var client = new SmtpClient())
            {
                client.Host = _configuration["Email:Host"];
                using (var emailMessage = new MailMessage())
                {
                    if (correoMensaje.Accion.Equals("Creacion de Usuario") || correoMensaje.Accion.Equals("Desbloqueo de Usuario"))
                    {
                        strBody = BodyCorreos.adminUsuarios(correoMensaje, _configuration["Email:Home"]);
                    }
                    else
                    {
                        strBody = BodyCorreos.documentoPemexPendiente(correoMensaje, _configuration["Email:Home"]);
                    }

                    emailMessage.To.Add(new MailAddress(correoMensaje.To));
                    emailMessage.From = new MailAddress(_configuration["Email:From"]);
                    emailMessage.Bcc.Add(new MailAddress(_configuration["Email:Bcc"]));
                    emailMessage.Priority = MailPriority.High;
                    emailMessage.Subject = correoMensaje.Subject;
                    emailMessage.Body = strBody;
                    emailMessage.IsBodyHtml = true;
                    client.Send(emailMessage);
                    resultItemIenum.Data = "EXITO";
                }
            }
            await Task.CompletedTask;
            return resultItemIenum;
        }

        public async Task<DataResult<string>> EnvioCorreoUserAsync(CorreoDto correoMensaje)
        {
            string sitiodireccion, politicadeprivacidaddireccion, politicadeprivacidaddescripcion;
            sitiodireccion = politicadeprivacidaddireccion = politicadeprivacidaddescripcion = "";

            try
            {
                sitiodireccion = _configuration["Email:SitioDireccion"];
                politicadeprivacidaddireccion = _configuration["Email:PoliticaDePrivacidadDireccion"];
                politicadeprivacidaddescripcion = _configuration["Email:PoliticaDePrivacidadDescripcion"];
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }

            string strBody = "";

            DataResult<string> resultItemIenum = new DataResult<string>()
            {
                Status = System.Net.HttpStatusCode.OK,
                Message = "Envío de Correo Exitoso"
            };

            using (var client = new SmtpClient())
            {
                client.Host = _configuration["Email:Host"];
                using (var emailMessage = new MailMessage())
                {
                    string textBody = "";
                    if (correoMensaje.Accion.ToUpper() == "CREACION DE USUARIO" || correoMensaje.Accion.ToUpper() == "CREACIÓN DE USUARIO")
                    {
                        textBody = "Se le comunica que se le ha asignado una cuenta del sistema de Bóveda Electrónica Recepción On Premise";
                    }
                    else if (correoMensaje.Accion.ToUpper() == "DESBLOQUEO DE USUARIO")
                    {
                        textBody = "Se le comunica que su cuenta del sistema de Bóveda Electrónica Recepción On Premise se encuentra activa";
                    }
                    else
                    {
                        resultItemIenum.Data = "EXITO";
                        return resultItemIenum;
                    }

                    strBody = await NotificacionTemplate("CorreoRepository", "EnvioCorreoUserAsync", false);
                    emailMessage.To.Add(new MailAddress(correoMensaje.To));
                    strBody = strBody.Replace("[Usuario]", correoMensaje.userName);
                    strBody = strBody.Replace("[Nombre]", correoMensaje.Nombre);
                    strBody = strBody.Replace("[Accion]", correoMensaje.Accion);
                    strBody = strBody.Replace("[Detalle]", textBody);
                    strBody = strBody.Replace("[sitio-direccion]", sitiodireccion);
                    strBody = strBody.Replace("[politica-de-privacidad-direccion]", politicadeprivacidaddireccion);
                    strBody = strBody.Replace("[politica-de-privacidad-descripcion]", politicadeprivacidaddescripcion);
                    emailMessage.From = new MailAddress(_configuration["Email:From"]);
                    emailMessage.Bcc.Add(new MailAddress(_configuration["Email:Bcc"]));
                    emailMessage.Priority = MailPriority.High;
                    emailMessage.Subject = correoMensaje.Subject;
                    emailMessage.Body = strBody;
                    emailMessage.IsBodyHtml = true;
                    client.Send(emailMessage);
                    resultItemIenum.Data = "EXITO";
                }
            }
            await Task.CompletedTask;
            return resultItemIenum;
        }
    }
}
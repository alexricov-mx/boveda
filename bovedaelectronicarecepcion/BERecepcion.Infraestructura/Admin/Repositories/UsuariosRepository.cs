using BERecepcion.Core.Dto;
using BERecepcion.Core.Interfaces.Repositories;
using Dapper;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Serilog;
using BERecepcion.Infraestructura.Repositories;
using BERecepcion.Core.Admin.Interfaces.Repositories;
using BERecepcion.Core.Admin.Dto;
using BERecepcion.Core.OrdenSurtimiento.Dto;
using System.IO;

namespace BERecepcion.Infraestructura.Admin.Repositories
{
    public class UsuariosRepository : BaseSQLServerSqlRepository, IUsuariosRepository
    {
        private readonly IConfiguration _configuration;
        private readonly IBitacoraAdmonRepository _bitacoraAdmonRepository;

        public DataResult<UsersDto> itemResponse = new DataResult<UsersDto>();
        public DataResult<IEnumerable<UsersDto>> listResponse = new DataResult<IEnumerable<UsersDto>>();
        public UsuariosRepository(string cnnString, IConfiguration configuration, IBitacoraAdmonRepository bitacoraAdmonRepository) : base(cnnString)
        {
            _configuration = configuration;
            _bitacoraAdmonRepository = bitacoraAdmonRepository;
        }

        public async Task<DataResult<UsuariosFichaResponseDto>> GetUsuarioFichaAsync(string UserName, string Token)
        {
            // Parametros
            DynamicParameters par = new DynamicParameters();
            par.Add("@usuarioLogeado", UserName);
            par.Add("@Ficha", Token);



            //using para levantar la conexion al BD
            using (IDbConnection db = GetConnection())
            {

                var getUser = await db.QueryFirstOrDefaultAsync<UsuariosFichaResponseDto>(sql: "SP_usuario_seleccion_ficha", param: par, commandType: CommandType.StoredProcedure);

                if (getUser != null)
                {

                    DataResult<UsuariosFichaResponseDto> responseDto = new DataResult<UsuariosFichaResponseDto>();

                    responseDto.Message = "Datos encontrados correctamente";
                    responseDto.Status = System.Net.HttpStatusCode.OK;

                    UsuariosFichaResponseDto fichaResponseDto = new UsuariosFichaResponseDto();


                    fichaResponseDto.UserId = getUser.UserId;
                    fichaResponseDto.Ficha = getUser.Ficha;

                    fichaResponseDto.Usuario = getUser.Usuario;
                    fichaResponseDto.Nombre = getUser.Nombre;
                    fichaResponseDto.Email = getUser.Email;
                    fichaResponseDto.TipoUsuario = getUser.TipoUsuario;
                    fichaResponseDto.ProfileId = getUser.ProfileId;
                    fichaResponseDto.Perfil = "Funcionario";

                    fichaResponseDto.Centro = getUser.Centro;
                    fichaResponseDto.RFC = getUser.RFC;
                    fichaResponseDto.FechaCreacion = getUser.FechaCreacion;
                    fichaResponseDto.ValidoDesde = getUser.ValidoDesde;
                    fichaResponseDto.ValidoHasta = getUser.ValidoHasta;
                    fichaResponseDto.UltimoAcceso = DateTime.Now;
                    fichaResponseDto.IsBlocked = getUser.IsBlocked;
                    fichaResponseDto.IsDeleted = getUser.IsDeleted;
                    fichaResponseDto.TotalRegistros = 10;

                    responseDto.Data = fichaResponseDto;

                    return responseDto;
                }
                else
                {
                    DataResult<UsuariosFichaResponseDto> responseDto = new DataResult<UsuariosFichaResponseDto>();
                    responseDto.Message = "No se encontraron datos del usuario";
                    responseDto.Status = System.Net.HttpStatusCode.BadRequest;
                    responseDto.Data = new UsuariosFichaResponseDto();
                    return responseDto;
                }
            }

        }

        public async Task<DataResult<UsuarioSIODto>> GetUsuarioSIO(string ficha)
        {
            DataResult<UsuarioSIODto> result = new DataResult<UsuarioSIODto>();
            string usuario = _configuration["WebServiceSio:usuario"];
            string pass = _configuration["WebServiceSio:pass"];
            string token = _configuration["WebServiceSio:token"];

            string requestXML = "<soap:Envelope xmlns:soap =\"http://www.w3.org/2003/05/soap-envelope\" " +
                                "xmlns:tem=\"http://tempuri.org/\">" +
                                "<soap:Header>" +
                                    "<tem:CredencialesHeader>" +
                                        $"<tem:ApiKeyToken>{token}</tem:ApiKeyToken>" +
                                        $"<tem:Password>{pass}</tem:Password>" +
                                        $"<tem:Username>{usuario}</tem:Username>" +
                                    "</tem:CredencialesHeader>" +
                                "</soap:Header>" +
                                "<soap:Body>" +
                                    "<tem:Ficha>" +
                                        $"<tem:Ficha_>{ficha}</tem:Ficha_>" +
                                    "</tem:Ficha>" +
                                "</soap:Body>" +
                                "</soap:Envelope>";

            var client = new RestClient(_configuration["WebServiceSio:urlEndpoint"]);
            var request = new RestRequest("", Method.Post);
            request.AddHeader("content-type", "text/xml");
            request.AddParameter("text/xml", requestXML, ParameterType.RequestBody);
            var response = await client.ExecuteAsync(request);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(response.Content);
                XmlNodeList elemList = doc.GetElementsByTagName("FichaResult");
                XmlNode nodo = elemList.Item(0);
                if (nodo != null)
                {
                    string jsonText = JsonConvert.SerializeXmlNode(nodo);
                    UsuarioSIODto sioRes = System.Text.Json.JsonSerializer.Deserialize<UsuarioSIODto>(jsonText);
                    result.Message = "Llamado exitoso al servicio";
                    result.Status = response.StatusCode;
                    result.Data = sioRes;
                }
                else
                {
                    result.Message = $"No existe un usuario con la ficha: {ficha}";
                    result.Status = System.Net.HttpStatusCode.BadRequest;
                }

                return result;
            }
            else
            {
                result.Message = "Ocurrio un problema en el webservice de SIO";
                result.Status = System.Net.HttpStatusCode.ServiceUnavailable;
                return result;
            }
        }

        public async Task<DataResult<IEnumerable<UsuariosNaResponseDto>>> GetUsuarioNAAsync(string UserName, string creditorNumber)
        {
            // Parametros
            DynamicParameters par = new DynamicParameters();
            par.Add("@usuarioLogeado", UserName);
            par.Add("@Nacreedor", creditorNumber);

            //using para levantar la conexion al BD
            using (IDbConnection db = GetConnection())
            {
                var getUser = await db.QueryAsync<UsuariosNaResponseDto>(sql: "SP_usuario_seleccion_na", param: par, commandType: CommandType.StoredProcedure);

                if (getUser != null)
                {
                    DataResult<IEnumerable<UsuariosNaResponseDto>> result = new DataResult<IEnumerable<UsuariosNaResponseDto>>();
                    result.Message = "Se encontraron los siguientes proveedores";
                    result.Status = System.Net.HttpStatusCode.OK;

                    List<UsuariosNaResponseDto> responseDto = new List<UsuariosNaResponseDto>();

                    foreach (var obj in getUser)
                    {
                        UsuariosNaResponseDto usuarioDto = new UsuariosNaResponseDto();

                        usuarioDto.UserId = obj.UserId;
                        usuarioDto.Nacreedor = obj.Nacreedor;

                        DynamicParameters parO = new DynamicParameters();
                        parO.Add("@UserID", usuarioDto.UserId);
                        usuarioDto.Organismo = await db.QueryFirstAsync<Guid>(sql: "SP_usuario_seleccion_organismo_proveedor", param: parO, commandType: CommandType.StoredProcedure);

                        usuarioDto.Usuario = obj.Usuario;
                        usuarioDto.Compania = obj.Compania;
                        usuarioDto.Nombre = obj.Nombre;
                        usuarioDto.Email = obj.Email;
                        usuarioDto.TipoUsuario = obj.TipoUsuario;
                        usuarioDto.ProfileId = obj.ProfileId;
                        usuarioDto.Perfil = obj.Perfil;

                        usuarioDto.RFC = obj.RFC;
                        usuarioDto.PhoneNumber = obj.PhoneNumber;
                        usuarioDto.FechaCreacion = obj.FechaCreacion;
                        usuarioDto.ValidoDesde = obj.ValidoDesde;
                        usuarioDto.ValidoHasta = obj.ValidoHasta;
                        usuarioDto.UltimoAcceso = DateTime.Now;
                        usuarioDto.IsBlocked = obj.IsBlocked;
                        usuarioDto.IsDeleted = obj.IsDeleted;
                        usuarioDto.CreditorRFC = obj.CreditorRFC;
                        usuarioDto.TotalRegistros = 10;

                        responseDto.Add(usuarioDto);
                    }

                    result.Data = responseDto;
                    return result;
                }
                else
                {
                    DataResult<IEnumerable<UsuariosNaResponseDto>> result = new DataResult<IEnumerable<UsuariosNaResponseDto>>();
                    result.Message = "No se encontraron resultados de la búsqueda";
                    result.Status = System.Net.HttpStatusCode.BadRequest;
                    result.Data = new List<UsuariosNaResponseDto>();
                    return result;
                }
            }

        }

        public async Task<DataResult<IEnumerable<UsuariosNaResponseDto>>> GetUsuarioUiDAsync(string UserName, Guid userId)
        {
            // Parametros
            DynamicParameters par = new DynamicParameters();
            par.Add("@usuarioLogeado", UserName);
            par.Add("@UserID", userId);

            //using para levantar la conexion al BD
            using (IDbConnection db = GetConnection())
            {
                var getUser = await db.QueryAsync<UsuariosNaResponseDto>(sql: "SP_usuario_seleccion_user", param: par, commandType: CommandType.StoredProcedure);

                if (getUser != null)
                {
                    DataResult<IEnumerable<UsuariosNaResponseDto>> result = new DataResult<IEnumerable<UsuariosNaResponseDto>>();
                    result.Message = "Se encontraron los siguientes proveedores";
                    result.Status = System.Net.HttpStatusCode.OK;

                    List<UsuariosNaResponseDto> responseDto = new List<UsuariosNaResponseDto>();

                    foreach (var obj in getUser)
                    {
                        UsuariosNaResponseDto usuarioDto = new UsuariosNaResponseDto();

                        usuarioDto.UserId = obj.UserId;
                        usuarioDto.Nacreedor = obj.Nacreedor;

                        DynamicParameters parO = new DynamicParameters();
                        parO.Add("@UserID", usuarioDto.UserId);
                        usuarioDto.Organismo = await db.QueryFirstAsync<Guid>(sql: "SP_usuario_seleccion_organismo_proveedor", param: parO, commandType: CommandType.StoredProcedure);

                        usuarioDto.Usuario = obj.Usuario;
                        usuarioDto.Compania = obj.Compania;
                        usuarioDto.Nombre = obj.Nombre;
                        usuarioDto.Email = obj.Email;
                        usuarioDto.TipoUsuario = obj.TipoUsuario;
                        usuarioDto.ProfileId = obj.ProfileId;
                        usuarioDto.Perfil = obj.Perfil;

                        usuarioDto.RFC = obj.RFC;
                        usuarioDto.PhoneNumber = obj.PhoneNumber;
                        usuarioDto.FechaCreacion = obj.FechaCreacion;
                        usuarioDto.ValidoDesde = obj.ValidoDesde;
                        usuarioDto.ValidoHasta = obj.ValidoHasta;
                        usuarioDto.UltimoAcceso = DateTime.Now;
                        usuarioDto.IsBlocked = obj.IsBlocked;
                        usuarioDto.IsDeleted = obj.IsDeleted;
                        usuarioDto.TotalRegistros = 10;

                        responseDto.Add(usuarioDto);
                    }

                    result.Data = responseDto;
                    return result;
                }
                else
                {
                    DataResult<IEnumerable<UsuariosNaResponseDto>> result = new DataResult<IEnumerable<UsuariosNaResponseDto>>();
                    result.Message = "No se encontraron resultados de la búsqueda";
                    result.Status = System.Net.HttpStatusCode.BadRequest;
                    result.Data = new List<UsuariosNaResponseDto>();
                    return result;
                }
            }

        }

        public async Task<DataResult<UsersDto>> GetUsuarioEmailAsync(string UserName, string Email)
        {
            // Parametros
            DynamicParameters par = new DynamicParameters();
            par.Add("@usuarioLogeado", UserName);
            par.Add("@Email", Email);

            //using para levantar la conexion al BD
            using (IDbConnection db = GetConnection())
            {

                var getUser = await db.QueryFirstOrDefaultAsync<UsersDto>(sql: "SP_usuario_seleccion_email", param: par, commandType: CommandType.StoredProcedure);

                if (getUser != null)
                {

                    DataResult<UsersDto> result = new DataResult<UsersDto>();

                    result.Message = "Usuario encontrado";
                    result.Status = System.Net.HttpStatusCode.OK;


                    UsersDto responseDto = new UsersDto();

                    responseDto.UserID = getUser.UserID;
                    responseDto.UserName = getUser.UserName;
                    responseDto.Name = getUser.Name;
                    responseDto.UserType = getUser.UserType;
                    responseDto.Token = getUser.Token;
                    responseDto.ManagementCenter = getUser.ManagementCenter;
                    responseDto.CreditorNumber = getUser.CreditorNumber;
                    responseDto.RFC = getUser.RFC;
                    responseDto.IsBlocked = getUser.IsBlocked;
                    responseDto.IsDeleted = getUser.IsDeleted;
                    responseDto.ProfileID = getUser.ProfileID;
                    responseDto.Email = getUser.Email;
                    responseDto.Company = getUser.Company;
                    responseDto.PhoneNumber = getUser.PhoneNumber;
                    responseDto.CreationDate = getUser.CreationDate;
                    responseDto.DateInitialValid = getUser.DateInitialValid;
                    responseDto.DateEndValid = getUser.DateEndValid;
                    responseDto.UltimoAcceso = DateTime.Now;
                    responseDto.CreditorRFC = getUser.CreditorRFC;

                    result.Data = responseDto;

                    return result;
                }
                else
                {
                    DataResult<UsersDto> result = new DataResult<UsersDto>();

                    result.Message = "Usuario encontrado";
                    result.Status = System.Net.HttpStatusCode.BadRequest;
                    result.Data = new UsersDto();
                    return result;
                }
            }

        }

        public async Task<DataResult<UsuariosPemexInDto>> ActualizaCambioAsync(UsuariosPemexInDto pemexDto)
        {
            DataResult<UsuariosPemexInDto> responseDto = new DataResult<UsuariosPemexInDto>()
            {
                Message = "Actualización exitosa",
                Status = System.Net.HttpStatusCode.OK
            };


            // Parametros
            DynamicParameters par = new DynamicParameters();

            par.Add("@userId", pemexDto.userId);
            par.Add("@ManagementCenter", pemexDto.Centro);
            par.Add("@DateInitialValid", Convert.ToDateTime(pemexDto.ValidoDesde));
            par.Add("@DateEndValid", Convert.ToDateTime(pemexDto.ValidoHasta));
            par.Add("@ProfileID", pemexDto.Perfil);
            par.Add("@usuarioLogeado", pemexDto.usuarioLogeado);
            par.Add("@Name", pemexDto.Name);
            par.Add("@RFC", pemexDto.RFC);
            par.Add("@Email", pemexDto.Email);
            par.Add("@UserName", pemexDto.Ficha.ToString());
            par.Add("@Company", pemexDto.Compania);

            //using para levantar la conexion al BD
            using (IDbConnection db = GetConnection())
            {
                db.Open();
                using (var tran = db.BeginTransaction())
                {
                    try
                    {
                        string resultUpdateUser = await db.QueryFirstAsync<string>(sql: "SP_usuario_cambio_upemex", param: par, tran, commandType: CommandType.StoredProcedure);
                        if (resultUpdateUser == "EXITO")
                        {
                            var dt = new DataTable("dbo.UsrRefOrg");
                            dt.Columns.Add("UserID");
                            dt.Columns.Add("organismID");

                            foreach (var organismo in pemexDto.Organismos)
                            {
                                dt.Rows.Add(pemexDto.userId, organismo.OrganismID);
                            }

                            var cg = await db.QueryFirstAsync<ManagementCentersDto>(sql: "SP_UsrRefOrg_inserta_table", new { Table = dt }, tran, commandType: CommandType.StoredProcedure);
                            if (cg.status == "EXITO")
                            {
                                responseDto.Data = pemexDto;
                                tran.Commit();
                            }
                            else
                            {
                                responseDto.Status = System.Net.HttpStatusCode.BadRequest;
                                responseDto.Message = "No se pudo realizar la actualización del usuario, favor de validar los datos";
                                tran.Rollback();
                            }
                        }
                        else
                        {
                            responseDto.Status = System.Net.HttpStatusCode.BadRequest;
                            responseDto.Message = "No se pudo realizar la actualización del usuario, favor de validar los datos";
                            tran.Rollback();
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex.Message);
                        await _bitacoraAdmonRepository.InsertaBitacoraAdmonAsync(new BitacoraAdmonDto() { Descripcion = "excepcion a revisar con TI", Evento = "Error", FechaCambio = DateTime.Now, Usuario = pemexDto.nombreLogeado });
                        responseDto.Status = System.Net.HttpStatusCode.BadRequest;
                        responseDto.Message = "No se pudo realizar la actualización del usuario, favor de validar los datos";
                        throw;
                    }
                    return responseDto;
                }
            }

        }

        public async Task<DataResult<UsuariosPemexInDto>> ActualizaCambioNaAsync(UsuariosPemexInDto proveedorDto)
        {

            DataResult<UsuariosPemexInDto> responseDto = new DataResult<UsuariosPemexInDto>()
            {
                Status = System.Net.HttpStatusCode.OK
            };
            // Parametros
            DynamicParameters par = new DynamicParameters();
            par.Add("@usuarioLogeado", proveedorDto.usuarioLogeado);
            par.Add("@userId", proveedorDto.userId);
            par.Add("@Company", proveedorDto.Compania);
            par.Add("@RFC", proveedorDto.RFC);
            par.Add("@ProfileID", proveedorDto.Perfil);
            par.Add("@Name", proveedorDto.Name);
            par.Add("@Email", proveedorDto.Email);
            par.Add("@PhoneNumber", proveedorDto.PhoneNumber);
            par.Add("@DateInitialValid", Convert.ToDateTime(proveedorDto.ValidoDesde));
            par.Add("@DateEndValid", Convert.ToDateTime(proveedorDto.ValidoHasta));
            par.Add("@OrganismID", proveedorDto.Organismo);
            par.Add("@CreditorRFC", proveedorDto.CreditorRFC);

            //using para levantar la conexion al BD
            try
            {
                using (IDbConnection db = GetConnection())
                {
                    UsuariosPemexInDto result = await db.QueryFirstAsync<UsuariosPemexInDto>(sql: "SP_usuario_cambio_uproveedor", param: par, commandType: CommandType.StoredProcedure);
                    if (result.status == "EXITO")
                    {
                        responseDto.Message = "Se actualizó el usuario con exito";
                    }
                    else
                    {
                        responseDto.Status = System.Net.HttpStatusCode.BadRequest;
                        responseDto.Message = "No se pudo realizar la actualización del usuario, favor de validar los datos";
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                await _bitacoraAdmonRepository.InsertaBitacoraAdmonAsync(new BitacoraAdmonDto() { Descripcion = "excepcion a revisar con TI", Evento = "Error", FechaCambio = DateTime.Now, Usuario = proveedorDto.nombreLogeado });
                responseDto.Status = System.Net.HttpStatusCode.BadRequest;
                responseDto.Message = "No se pudo realizar la actualización del usuario, favor de validar los datos";
                throw;
            }
            return responseDto;
        }

        public async Task<DataResult<UsersDto>> ActualizaAdministracionAsync(string UserName, Guid userId)
        {
            try
            {
                DynamicParameters par = new DynamicParameters();
                par.Add("@usuarioLogeado", UserName);
                par.Add("@userId", userId);

                using (IDbConnection db = GetConnection())
                {
                    var response = await db.QueryFirstAsync<UsersDto>(sql: "SP_usuario_administra_upemex", param: par, commandType: CommandType.StoredProcedure);

                    if (response == null)
                    {
                        itemResponse.Status = System.Net.HttpStatusCode.BadRequest;
                        itemResponse.Message = "Ocurrió un error al procesar la solicitud, favor de intentar más tarde.";
                        return itemResponse;
                    }
                    itemResponse.Data = response;
                    itemResponse.Status = System.Net.HttpStatusCode.OK;
                    itemResponse.Message = "Actualización exitosa";
                    return itemResponse;
                }
            }
            catch (Exception ex)
            {
                itemResponse.Status = System.Net.HttpStatusCode.BadRequest;
                itemResponse.Message = ex.Message;
            }
            return itemResponse;
        }

        public async Task<DataResult<UsuarioActualizaResponseDto>> ActualizaAdministracionNaAsync(string UserName, Guid userId)
        {
            // Parametros
            DynamicParameters par = new DynamicParameters();
            par.Add("@usuarioLogeado", UserName);
            par.Add("@userId", userId);

            //using para levantar la conexion al BD
            using (IDbConnection db = GetConnection())
            {
                int blocked = await db.ExecuteAsync(sql: "SP_usuario_administra_uproveedor", param: par, commandType: CommandType.StoredProcedure);

                UsuarioActualizaResponseDto responseDto = new UsuarioActualizaResponseDto();

                if (blocked > 0)
                {

                    DataResult<UsuarioActualizaResponseDto> result = new DataResult<UsuarioActualizaResponseDto>();
                    result.Message = "Actualización exitosa";
                    result.Status = System.Net.HttpStatusCode.OK;

                    responseDto.Resultado = true;
                    responseDto.Mensaje = "Actualización exitosa";

                    result.Data = responseDto;
                    return result;
                }
                else
                {
                    DataResult<UsuarioActualizaResponseDto> result = new DataResult<UsuarioActualizaResponseDto>();
                    result.Message = "No se pudo realizar la actualización, favor de validar los datos";
                    result.Status = System.Net.HttpStatusCode.BadRequest;

                    responseDto.Resultado = false;
                    responseDto.Mensaje = "No se pudo realizar la actualización, favor de validar los datos";

                    result.Data = responseDto;
                    return result;
                }
            }

        }


        //public async Task<DataResult<UsuarioActualizaResponseDto>> ActualizaAdminAsync(string UserName, Guid userId)
        //{
        //    // Parametros
        //    DynamicParameters par = new DynamicParameters();
        //    par.Add("@usuarioLogeado", UserName);
        //    par.Add("@userId", userId);

        //    //using para levantar la conexion al BD
        //    using (IDbConnection db = GetConnection())
        //    {
        //        string rslt = await db.QueryFirstAsync(sql: "SP_usuario_administra_admin", param: par, commandType: CommandType.StoredProcedure);

        //        UsuarioActualizaResponseDto responseDto = new UsuarioActualizaResponseDto();

        //        if (!string.IsNullOrEmpty(rslt))
        //        {

        //            DataResult<UsuarioActualizaResponseDto> result = new DataResult<UsuarioActualizaResponseDto>();
        //            result.Message = "Actualización exitosa";
        //            result.Status = System.Net.HttpStatusCode.OK;

        //            responseDto.Resultado = true;
        //            responseDto.Mensaje = rslt;

        //            result.Data = responseDto;
        //            return result;
        //        }
        //        else
        //        {
        //            DataResult<UsuarioActualizaResponseDto> result = new DataResult<UsuarioActualizaResponseDto>();
        //            result.Message = "No se pudo realizar la actualización, favor de validar los datos";
        //            result.Status = System.Net.HttpStatusCode.BadRequest;

        //            responseDto.Resultado = false;
        //            responseDto.Mensaje = "No se pudo realizar la actualización, favor de validar los datos";

        //            result.Data = responseDto;
        //            return result;
        //        }
        //    }

        //}

        public async Task<DataResult<UsersDto>> ActualizaAdminAsync(string UserName, Guid userId)
        {
            try
            {
                DynamicParameters par = new DynamicParameters();
                par.Add("@usuarioLogeado", UserName);
                par.Add("@userId", userId);

                using (IDbConnection db = GetConnection())
                {
                    var response = await db.QueryFirstAsync<UsersDto>(sql: "SP_usuario_administra_admin", param: par, commandType: CommandType.StoredProcedure);
                    if (response == null)
                    {
                        itemResponse.Status = System.Net.HttpStatusCode.BadRequest;
                        itemResponse.Message = "Ocurrió un error al procesar la solicitud, favor de intentar más tarde.";
                        return itemResponse;
                    }
                    itemResponse.Data = response;
                    itemResponse.Status = System.Net.HttpStatusCode.OK;
                    itemResponse.Message = "Actualización exitosa";
                    return itemResponse;
                }
            }
            catch (Exception ex)
            {
                itemResponse.Status = System.Net.HttpStatusCode.BadRequest;
                itemResponse.Message = ex.Message;
            }
            return itemResponse;
        }

        public async Task<DataResult<UsersDto>> ActualizaBloqueaAsync(string UserNameModifier, Guid userId)
        {

            try
            {
                // Parametros
                DynamicParameters par = new DynamicParameters();
                par.Add("@usuarioLogeado", UserNameModifier);
                par.Add("@userId", userId);

                //using para levantar la conexion al BD
                using (IDbConnection db = GetConnection())
                {
                    var result = await db.QueryFirstAsync<UsersDto>(sql: "SP_usuario_bloquea", param: par, commandType: CommandType.StoredProcedure);
                    if(result == null)
                    {
                        itemResponse.Status = System.Net.HttpStatusCode.BadRequest;
                        itemResponse.Message = "Ocurrió un error inesperado, favor de intentar más tarde.";
                        return itemResponse;
                    }
                    itemResponse.Message = "Actualización exitosa";
                    itemResponse.Status = System.Net.HttpStatusCode.OK;
                    itemResponse.Data = result;
                    return itemResponse;
                }
            }
            catch (Exception ex)
            {
                itemResponse.Status = System.Net.HttpStatusCode.BadRequest;
                itemResponse.Message = ex.Message;
            }
            return itemResponse;
            
        }

        public async Task<DataResult<UsuariosPemexInDto>> InsertaUsuarioAsync(UsuariosPemexInDto pemexInDto)
        {

            DataResult<UsuariosPemexInDto> responseDto = new DataResult<UsuariosPemexInDto>()
            {
                Status = System.Net.HttpStatusCode.OK,
                Message = "Usuario creado exitosamente"
            };
            // Parametros
            DynamicParameters par = new DynamicParameters();
            par.Add("@usuarioLogeado", pemexInDto.usuarioLogeado);
            par.Add("@Name", pemexInDto.Name);
            par.Add("@Ficha", pemexInDto.Ficha);
            par.Add("@ManagementCenter", pemexInDto.Centro);
            par.Add("@RFC", pemexInDto.RFC);
            par.Add("@ProfileID", pemexInDto.Perfil);
            par.Add("@Email", pemexInDto.Email);
            par.Add("@userName", pemexInDto.userName == null ? "" : pemexInDto.userName);
            par.Add("@DateInitialValid", Convert.ToDateTime(pemexInDto.ValidoDesde));
            par.Add("@DateEndValid", Convert.ToDateTime(pemexInDto.ValidoHasta));
            par.Add("@Company", pemexInDto.Compania);
            par.Add("@CreditorRFC", pemexInDto.CreditorRFC);

            //using para levantar la conexion al BD
            using (IDbConnection db = GetConnection())
            {
                db.Open();
                using (var tran = db.BeginTransaction())
                {
                    try
                    {
                        Guid UserId = await db.QueryFirstAsync<Guid>(sql: "SP_usuario_inserta_pemex", param: par, tran, commandType: CommandType.StoredProcedure);

                        var dt = new DataTable("dbo.TypeUsrRefOrg");
                        dt.Columns.Add("UserID");
                        dt.Columns.Add("OrganismID");
                        dt.Columns.Add("Company");
                        dt.Columns.Add("CreditorNumber");
                        dt.Columns.Add("CreditorRFC");
                        dt.Columns.Add("EmailAlternate");

                        foreach (var organismo in pemexInDto.Organismos)
                        {
                            dt.Rows.Add(UserId, organismo.OrganismID);
                        }

                        var cg = await db.QueryFirstAsync<ManagementCentersDto>(sql: "SP_UsrRefOrg_inserta_table", new { Table = dt }, tran, commandType: CommandType.StoredProcedure);
                        if (cg.status == "EXITO")
                        {
                            responseDto.Data = pemexInDto;
                            tran.Commit();
                        }
                        else
                        {
                            responseDto.Status = System.Net.HttpStatusCode.BadRequest;
                            responseDto.Message = "No se pudo realizar el alta del usuario, favor de validar los datos";
                            tran.Rollback();
                        }

                        responseDto.Data = pemexInDto;
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex.Message);
                        await _bitacoraAdmonRepository.InsertaBitacoraAdmonAsync(new BitacoraAdmonDto() { Descripcion = "excepcion a revisar con TI", Evento = "Error", FechaCambio = DateTime.Now, Usuario = pemexInDto.nombreLogeado });
                        responseDto.Status = System.Net.HttpStatusCode.BadRequest;
                        responseDto.Message = "No se pudo realizar la creación del usuario, favor de validar los datos";
                        tran.Rollback();
                        throw;
                    }
                }
            }
            return responseDto;
        }

        public async Task<DataResult<UsuariosPemexInDto>> InsertaUsuarioNaAsync(UsuariosPemexInDto proveedorInDto)
        {

            DataResult<UsuariosPemexInDto> responseDto = new DataResult<UsuariosPemexInDto>()
            {
                Status = System.Net.HttpStatusCode.OK
            };
            // Parametros
            DynamicParameters par = new DynamicParameters();
            par.Add("@usuarioLogeado", proveedorInDto.usuarioLogeado);
            par.Add("@Company", proveedorInDto.Compania);
            par.Add("@RFC", proveedorInDto.RFC); 
            par.Add("@CreditorNumber", proveedorInDto.CreditorNumber);
            par.Add("@userName", proveedorInDto.userName == null ? "" : proveedorInDto.userName);
            par.Add("@DateInitialValid", Convert.ToDateTime(proveedorInDto.ValidoDesde));
            par.Add("@DateEndValid", Convert.ToDateTime(proveedorInDto.ValidoHasta));
            par.Add("@ProfileID", proveedorInDto.Perfil);
            par.Add("@Name", proveedorInDto.Name);
            par.Add("@Email", proveedorInDto.Email);
            par.Add("@PhoneNumber", proveedorInDto.PhoneNumber);
            par.Add("@CreditorRFC", proveedorInDto.CreditorRFC);
            par.Add("@FileDI", proveedorInDto.NameFileDI);
            par.Add("@FileDP", proveedorInDto.NameFileDP);

            try
            {
                using (IDbConnection db = GetConnection())
                {
                    db.Open();
                    using (var tran = db.BeginTransaction())
                    {
                        try
                        {
                            Guid UserId = await db.QueryFirstAsync<Guid>(sql: "SP_usuario_inserta_proveedor", param: par, tran, commandType: CommandType.StoredProcedure);
                            
                            var dt = new DataTable("dbo.TypeUsrRefOrg");
                            dt.Columns.Add("UserID");
                            dt.Columns.Add("OrganismID");
                            dt.Columns.Add("Company");
                            dt.Columns.Add("CreditorNumber");
                            dt.Columns.Add("CreditorRFC");
                            dt.Columns.Add("EmailAlternate");

                            foreach (var organismo in proveedorInDto.Organismos)
                            {
                                dt.Rows.Add(UserId, organismo.OrganismID);
                            }

                            var cg = await db.QueryFirstAsync<ManagementCentersDto>(sql: "SP_UsrRefOrg_inserta_table", new { Table = dt }, tran, commandType: CommandType.StoredProcedure);

                            if (proveedorInDto.FileDI is not null)
                            {
                                File.WriteAllBytes(_configuration["NAS:DocumentoIdentificacion"] + "/" + UserId.ToString() + "/" + proveedorInDto.NameFileDI, proveedorInDto.FileDI);
                            }

                            if (proveedorInDto.FileDP is not null)
                            {
                                File.WriteAllBytes(_configuration["NAS:DocumentoProvatorio"] + "/" + UserId.ToString() + "/" + proveedorInDto.NameFileDP, proveedorInDto.FileDP);
                            }


                            if (cg.status == "EXITO")
                            {
                                responseDto.Message = "Se creo del usuario";
                            }
                            else
                            {
                                responseDto.Status = System.Net.HttpStatusCode.BadRequest;
                                responseDto.Message = "No se pudo realizar la creación del usuario, favor de validar los datos";
                                tran.Rollback();
                            }
                            responseDto.Data = proveedorInDto;
                            tran.Commit();
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex.Message);
                            await _bitacoraAdmonRepository.InsertaBitacoraAdmonAsync(new BitacoraAdmonDto() { Descripcion = "excepcion a revisar con TI", Evento = "Error", FechaCambio = DateTime.Now, Usuario = proveedorInDto.nombreLogeado });
                            responseDto.Status = System.Net.HttpStatusCode.BadRequest;
                            responseDto.Message = "No se pudo realizar la creación del usuario, favor de validar los datos";
                            tran.Rollback();
                            throw;
                        }
                        return responseDto;
                    }
                }
            }
            catch (Exception)
            {
                responseDto.Status = System.Net.HttpStatusCode.BadRequest;
                responseDto.Message = "No se pudo realizar la creación del usuario, favor de validar los datos";

                return responseDto;
            }

        }
        public async Task<DataResult<UsuariosPemexInDto>> InsertaUsuarioAuditorPPIAsync(UsuariosPemexInDto auditorPpiDto)
        {
            DataResult<UsuariosPemexInDto> responseDto = new DataResult<UsuariosPemexInDto>()
            {
                Status = System.Net.HttpStatusCode.OK
            };
            // Parametros
            DynamicParameters par = new DynamicParameters();
            par.Add("@usuarioLogeado", auditorPpiDto.usuarioLogeado);
            par.Add("@userType", auditorPpiDto.userType);
            par.Add("@userName", auditorPpiDto.userName == null ? "" : auditorPpiDto.userName);
            par.Add("@Name", auditorPpiDto.Name);
            par.Add("@Email", auditorPpiDto.Email);
            par.Add("@Company", auditorPpiDto.Compania);
            par.Add("@ProfileID", auditorPpiDto.Perfil);
            par.Add("@RFC", auditorPpiDto.RFC == null ? "" : auditorPpiDto.RFC);
            par.Add("@CreditorRFC", auditorPpiDto.CreditorRFC);

            try
            {
                using (IDbConnection db = GetConnection())
                {
                    db.Open();
                    using (var tran = db.BeginTransaction())
                    {
                        try
                        {

                            Guid UserId = await db.QueryFirstAsync<Guid>(sql: "SP_usuario_inserta_auditor", param: par, tran, commandType: CommandType.StoredProcedure);
                            DynamicParameters parI = new DynamicParameters();
                            parI.Add("@UserId", UserId);
                            parI.Add("@organismID", auditorPpiDto.Organismo);

                            UsuariosPemexInDto result = await db.QueryFirstAsync<UsuariosPemexInDto>(sql: "SP_UsrRefOrg_inserta_uproveedor", param: parI, tran, commandType: CommandType.StoredProcedure);

                            if (result.status == "EXITO")
                            {
                                responseDto.Message = "Se creo del usuario";
                            }
                            else
                            {
                                responseDto.Status = System.Net.HttpStatusCode.BadRequest;
                                responseDto.Message = "No se pudo realizar la creación del usuario, favor de validar los datos";
                                tran.Rollback();
                            }
                            responseDto.Data = result;
                            tran.Commit();
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex.Message);
                            await _bitacoraAdmonRepository.InsertaBitacoraAdmonAsync(new BitacoraAdmonDto() { Descripcion = "excepcion a revisar con TI", Evento = "Error", FechaCambio = DateTime.Now, Usuario = auditorPpiDto.nombreLogeado });
                            responseDto.Status = System.Net.HttpStatusCode.BadRequest;
                            responseDto.Message = "No se pudo realizar la creación del usuario, favor de validar los datos";
                            tran.Rollback();
                            throw;
                        }
                        return responseDto;
                    }
                }
            }
            catch (Exception)
            {
                responseDto.Status = System.Net.HttpStatusCode.BadRequest;
                responseDto.Message = "No se pudo realizar la creación del usuario, favor de validar los datos";

                return responseDto;
            }
        }

        public async Task<DataResult<UsuarioSimpleDto>> GetFichaNombreAsync(string token)
        {
            DataResult<UsuarioSimpleDto> responseDto = new DataResult<UsuarioSimpleDto>();

            responseDto.Message = "Datos encontrados correctamente";
            responseDto.Status = System.Net.HttpStatusCode.OK;

            DynamicParameters par = new DynamicParameters();
            par.Add("@token", token);
            using (IDbConnection db = GetConnection())
            {
                var result = await db.QueryFirstOrDefaultAsync<UsuarioSimpleDto>("SP_usuario_seleccion_simple", param: par, commandType: CommandType.StoredProcedure);
                responseDto.Data = result;
            }
            return responseDto;
        }

        public async Task<DataResult<UsuariosPemexInDto>> ActualizaUsuarioAuditorPPIAsync(UsuariosPemexInDto auditorPpiDto)
        {
            DataResult<UsuariosPemexInDto> responseDto = new DataResult<UsuariosPemexInDto>()
            {
                Status = System.Net.HttpStatusCode.OK
            };
            // Parametros
            DynamicParameters par = new DynamicParameters();
            par.Add("@usuarioLogeado", auditorPpiDto.usuarioLogeado);
            par.Add("@UserID", auditorPpiDto.userId);
            par.Add("@userName", "Auditor");
            par.Add("@Name", auditorPpiDto.Name);
            par.Add("@Email", auditorPpiDto.Email);
            par.Add("@Company", auditorPpiDto.Compania);
            par.Add("@ProfileID", auditorPpiDto.Perfil);
            par.Add("@RFC", auditorPpiDto.RFC == null ? "" : auditorPpiDto.RFC);
            par.Add("@CreditorRFC", auditorPpiDto.CreditorRFC);

            try
            {
                using (IDbConnection db = GetConnection())
                {
                    db.Open();
                    using (var tran = db.BeginTransaction())
                    {
                        try
                        {

                            UsuariosPemexInDto rslt = await db.QueryFirstAsync<UsuariosPemexInDto>(sql: "SP_usuario_cambio_auditor", param: par, tran, commandType: CommandType.StoredProcedure);

                            //va a borrar el registro

                            DynamicParameters parD = new DynamicParameters();
                            parD.Add("@UserId", auditorPpiDto.userId);
                            parD.Add("@organismID", auditorPpiDto.Organismo);

                            int deleteOrganism = await db.ExecuteAsync(sql: "SP_UsrRefOrg_borra_uproveedor", param: parD, tran, commandType: CommandType.StoredProcedure);

                            DynamicParameters parI = new DynamicParameters();
                            parI.Add("@UserId", auditorPpiDto.userId);
                            parI.Add("@organismID", auditorPpiDto.Organismo);
                            UsuariosPemexInDto result = await db.QueryFirstAsync<UsuariosPemexInDto>(sql: "SP_UsrRefOrg_inserta_uproveedor", param: parI, tran, commandType: CommandType.StoredProcedure);

                            if (result.status == "EXITO")
                            {
                                responseDto.Message = "Se actualizo del usuario";
                            }
                            else
                            {
                                responseDto.Status = System.Net.HttpStatusCode.BadRequest;
                                responseDto.Message = "No se pudo realizar la actualización del usuario, favor de validar los datos";
                                tran.Rollback();
                            }
                            responseDto.Data = result;
                            tran.Commit();
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex.Message);
                            await _bitacoraAdmonRepository.InsertaBitacoraAdmonAsync(new BitacoraAdmonDto() { Descripcion = "excepcion a revisar con TI", Evento = "Error", FechaCambio = DateTime.Now, Usuario = auditorPpiDto.nombreLogeado });
                            responseDto.Status = System.Net.HttpStatusCode.BadRequest;
                            responseDto.Message = "No se pudo realizar la actuaización del usuario, favor de validar los datos";
                            tran.Rollback();
                            throw;
                        }
                        return responseDto;
                    }
                }
            }
            catch (Exception)
            {
                responseDto.Status = System.Net.HttpStatusCode.BadRequest;
                responseDto.Message = "No se pudo realizar la actualización del usuario, favor de validar los datos";
                return responseDto;
            }
        }

        public async Task<DataResult<IEnumerable<UsersDto>>> GetUsuariosByCreditorNumber(string CreditorNumber)
        {
            DataResult<IEnumerable<UsersDto>> response = new DataResult<IEnumerable<UsersDto>>()
            {
                Status = System.Net.HttpStatusCode.OK
            };

            DynamicParameters par = new DynamicParameters();
            par.Add("@CreditorNumber", CreditorNumber);

            try
            {
                using (IDbConnection db = GetConnection())
                {
                    db.Open();
                    var result = await db.QueryAsync<UsersDto>(sql: "SP_usuario_seleccion_CreditorNumber", param: par, commandType: CommandType.StoredProcedure);
                    response.Data = result;
                    return response;
                }
            }
            catch (Exception)
            {
                response.Status = System.Net.HttpStatusCode.BadRequest;
                response.Message = "Error al obtener los usuarios.";
                return response;
            }
        }

        public async Task<DataResult<IEnumerable<UsersDto>>> GetUsuariosByCreditorBanking(string CreditorBanking)
        {
            DataResult<IEnumerable<UsersDto>> response = new DataResult<IEnumerable<UsersDto>>()
            {
                Status = System.Net.HttpStatusCode.OK
            };

            DynamicParameters par = new DynamicParameters();
            par.Add("@CreditorBanking", CreditorBanking);

            try
            {
                using (IDbConnection db = GetConnection())
                {
                    db.Open();
                    var result = await db.QueryAsync<UsersDto>(sql: "SP_usuario_seleccion_CreditorBanking", param: par, commandType: CommandType.StoredProcedure);
                    response.Data = result;
                    return response;
                }
            }
            catch (Exception)
            {
                response.Status = System.Net.HttpStatusCode.BadRequest;
                response.Message = "Error al obtener los usuarios.";
                return response;
            }
        }

        public async Task<DataResult<IEnumerable<UsersDto>>> GetUsuariosByToken(string Token)
        {
            DataResult<IEnumerable<UsersDto>> response = new DataResult<IEnumerable<UsersDto>>()
            {
                Status = System.Net.HttpStatusCode.OK
            };

            DynamicParameters par = new DynamicParameters();
            par.Add("@Token", Token);

            try
            {
                using (IDbConnection db = GetConnection())
                {
                    db.Open();
                    var result = await db.QueryAsync<UsersDto>(sql: "SP_usuario_seleccion_Token", param: par, commandType: CommandType.StoredProcedure);
                    response.Data = result;
                    return response;
                }
            }
            catch (Exception)
            {
                response.Status = System.Net.HttpStatusCode.BadRequest;
                response.Message = "Error al obtener los usuarios.";
                return response;
            }
        }

        public async Task<DataResult<IEnumerable<UsersDto>>> GetUsuariosByCopadeId(string CopadeId)
        {
            DataResult<IEnumerable<UsersDto>> response = new DataResult<IEnumerable<UsersDto>>()
            {
                Status = System.Net.HttpStatusCode.OK
            };

            DynamicParameters par = new DynamicParameters();
            par.Add("@Token", CopadeId);

            try
            {
                using (IDbConnection db = GetConnection())
                {
                    db.Open();
                    var result = await db.QueryAsync<UsersDto>(sql: "SP_usuario_seleccion_Token", param: par, commandType: CommandType.StoredProcedure);
                    response.Data = result;
                    return response;
                }
            }
            catch (Exception)
            {
                response.Status = System.Net.HttpStatusCode.BadRequest;
                response.Message = "Error al obtener los usuarios.";
                return response;
            }
        }

        public async Task<DataResult<IEnumerable<UsersDto>>> GetUsuariosAsync(string search = null)
        {
            try
            {
                DynamicParameters par = new DynamicParameters();
                par.Add("@search", search);
                using (IDbConnection db = GetConnection())
                {
                    var users = await db.QueryAsync<UsersDto>(sql: "SP_Usuario_Consulta_Selecciona", param: par, commandType: CommandType.StoredProcedure);

                    listResponse.Status = System.Net.HttpStatusCode.OK;
                    listResponse.Data = users;
                    return listResponse;
                    //foreach (var getUser in users)
                    //{
                    //    UsuariosSearchResponseDto searchResponseDto = new UsuariosSearchResponseDto();

                    //    searchResponseDto.UserId = getUser.UserId;
                    //    searchResponseDto.Ficha = getUser.Ficha;
                    //    searchResponseDto.Usuario = getUser.Usuario;
                    //    searchResponseDto.Nombre = getUser.Nombre;
                    //    searchResponseDto.Email = getUser.Email;
                    //    searchResponseDto.TipoUsuario = getUser.TipoUsuario;
                    //    searchResponseDto.ProfileId = getUser.ProfileId;
                    //    searchResponseDto.ProfileName = getUser.ProfileName;
                    //    searchResponseDto.Perfil = getDescriptionType(getUser.TipoUsuario);

                    //    searchResponseDto.Centro = getUser.Centro;
                    //    searchResponseDto.RFC = getUser.RFC;
                    //    searchResponseDto.FechaCreacion = getUser.FechaCreacion;
                    //    searchResponseDto.ValidoDesde = getUser.ValidoDesde;
                    //    searchResponseDto.ValidoHasta = getUser.ValidoHasta;
                    //    searchResponseDto.UltimoAcceso = getUser.UltimoAcceso;
                    //    searchResponseDto.IsBlocked = getUser.IsBlocked;
                    //    searchResponseDto.IsDeleted = getUser.IsDeleted;
                    //    searchResponseDto.Company = getUser.Company;
                    //    searchResponseDto.CreditorNumber = getUser.CreditorNumber;
                    //    searchResponseDto.PhoneNumber = getUser.PhoneNumber;
                    //    searchResponseDto.OrganismosId = "";
                    //    searchResponseDto.CreditorRFC = getUser.CreditorRFC;

                    //    listResponse.Add(searchResponseDto);


                    //    if (listResponse.Count > 0)
                    //    {
                    //        foreach (var item in listResponse)
                    //        {
                    //            string r = "";
                    //            DynamicParameters parUserId = new DynamicParameters();
                    //            parUserId.Add("@UserID", item.UserId);
                    //            var organismos = await db.QueryAsync<Guid>(sql: "SP_usuario_seleccion_organismo_proveedor", param: parUserId, commandType: CommandType.StoredProcedure);
                    //            if (organismos != null)
                    //            {
                    //                foreach (var org in organismos)
                    //                {
                    //                    r = r + org.ToString() + ",";
                    //                }
                    //            }
                    //            if (r != "") r = r.Substring(0, r.Length - 1);
                    //            item.OrganismosId = r;
                    //        }
                    //    }

                    //    listResponse.Data = responseDataDto;

                    //    return listResponse;
                    //}

                }
            }
            catch (Exception ex)
            {
                listResponse.Message = ex.Message;
                listResponse.Status = System.Net.HttpStatusCode.BadRequest;
            }
            return listResponse;
        }

        public string getDescriptionType(string type)
        {
            string r = "Funcionario";
            switch (type)
            {
                case "UserTypeF":
                    r = "Funcionario";
                    break;
                case "UserTypeP":
                    r = "Proveedor";
                    break;
                case "UserTypeA":
                    r = "Administrador";
                    break;
                case "UserTypeS":
                    r = "Super Administrador";
                    break;
                case "Auditor":
                    r = "Auditor";
                    break;
                case "UsuarioPPI":
                    r = "Usuario PPI";
                    break;
            }
            return r;
        }

        public async Task<DataResult<IEnumerable<UsersDto>>> GetUsuariosByUserId(Guid UserId)
        {
            DataResult<IEnumerable<UsersDto>> response = new DataResult<IEnumerable<UsersDto>>()
            {
                Status = System.Net.HttpStatusCode.OK
            };

            DynamicParameters par = new DynamicParameters();
            par.Add("@UserId", UserId);

            try
            {
                using (IDbConnection db = GetConnection())
                {
                    db.Open();
                    var result = await db.QueryAsync<UsersDto>(sql: "SP_usuario_seleccion_guiduserid", param: par, commandType: CommandType.StoredProcedure);
                    response.Data = result;
                    return response;
                }
            }
            catch (Exception)
            {
                response.Status = System.Net.HttpStatusCode.BadRequest;
                response.Message = "Error al obtener los usuarios.";
                return response;
            }
        }

        public async Task<UsersDto> GetUsuarioByESignId(int usuarioId)
        {
            DynamicParameters par = new DynamicParameters();
            par.Add("@usuarioId", usuarioId);

            try
            {
                using (IDbConnection db = GetConnection())
                {
                    return await db.QueryFirstOrDefaultAsync<UsersDto>(sql: "SP_UsuarioByESignId_Selecciona", param: par, commandType: CommandType.StoredProcedure);
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<DataResult<UsersDto>> GetUsuarioByIdEdit(Guid UserID)
        {
            DynamicParameters par = new DynamicParameters();
            par.Add("@UserID", UserID);
            try
            {
                using (IDbConnection db = GetConnection())
                {
                    db.Open();
                    var result = await db.QueryMultipleAsync(sql: "SP_User_ById_Edit_Seleccion", param: par, commandType: CommandType.StoredProcedure);
                    var user = await result.ReadFirstOrDefaultAsync<UsersDto>();
                    var organisms = await result.ReadAsync<OrganismDto>();
                    user.Organisms = organisms;
                    itemResponse.Status = System.Net.HttpStatusCode.OK;
                    itemResponse.Data = user;
                    return itemResponse;
                }
            }
            catch (Exception ex)
            {
                itemResponse.Status = System.Net.HttpStatusCode.BadRequest;
                itemResponse.Message = ex.ToString(); ;
                return itemResponse;
            }
        }
    }
}

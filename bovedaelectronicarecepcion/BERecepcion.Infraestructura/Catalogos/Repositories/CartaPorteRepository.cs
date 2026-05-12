using BERecepcion.Core.Catalogos.Dto;
using BERecepcion.Core.Catalogos.Interfaces.Repositories;
using BERecepcion.Core.Dto;
using BERecepcion.Infraestructura.Repositories;
using Dapper;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;

namespace BERecepcion.Infraestructura.Catalogos.Repositories
{
    public class CartaPorteRepository : BaseSQLServerSqlRepository, ICartaPorteRepository
    {
        private readonly IConfiguration _configuration;

        public CartaPorteRepository(string cnnString, IConfiguration configuration) : base(cnnString)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Select a las tablas de Catálogos Carta Porte
        /// </summary>
        /// <param name="tableName">Nombre de la tabla</param>
        /// <param name="pageNumber">Numero de la pagina</param>
        /// <param name="pageSize">Tamaño de la pagina</param>
        /// <param name="filtroBusqueda">Por si se desea implementar la busqueda</param>
        /// <returns>Tabla a consultar</returns>
        public async Task<DataResult<IEnumerable<CatalogosCartaPorteDto>>> SeleccionarAsync(string tableName, int pageNumber = 1, int pageSize = 10, string filtroBusqueda = null)
        {
            DataResult<IEnumerable<CatalogosCartaPorteDto>> resultItem = new DataResult<IEnumerable<CatalogosCartaPorteDto>>()
            {
                Status = System.Net.HttpStatusCode.OK,
                Message = "Seleccionar Catálogos Carta Porte exitoso."
            };

            try
            {
                using (IDbConnection db = GetConnection())
                {
                    DynamicParameters par = new DynamicParameters();
                    par.Add("@TableName", tableName);
                    par.Add("@PageNumber", pageNumber);
                    par.Add("@PageSize", pageSize);

                    var result = db.QueryMultiple(sql: "SP_CatalogosCartaPorte_Seleccionar", param: par, commandType: CommandType.StoredProcedure);

                    var paging = result.Read<Pager>();
                    var select = result.Read<CatalogosCartaPorteDto>();

                    resultItem.Pager = paging.Single();
                    resultItem.Data = select;
                }
            }
            catch (Exception ex)
            {
                resultItem.Message = $"Ocurrio un problema al seleccionar Catálogos Carta Porte. Contacta a tu administrador. {ex.Message}";
                resultItem.Status = System.Net.HttpStatusCode.BadRequest;
            }

            return resultItem;
        }

        /// <summary>
        /// Verifica que el proceso no se esté corriendo e inserta a las tablas Catálogos Carta Porte de paso
        /// </summary>
        /// <returns>Estado del proceso</returns>
        public async Task<DataResult<CatalogosCartaPorteDto>> CargarAsync(DataResult<CatalogosCartaPorteDto> dataResult)
        {
            try
            {
                using (IDbConnection db = GetConnection())
                {
                    var result = db.ExecuteScalar<string>(
                        sql: "SP_CatalogosCartaPorte_Cargar",
                        new
                        {
                            c_ClaveProdServCP_Paso = dataResult.Data.CatalogosXLS.Tables["c_ClaveProdServCP_Paso"],
                            c_ClaveTipoCarga_Paso = dataResult.Data.CatalogosXLS.Tables["c_ClaveTipoCarga_Paso"],
                            c_ClaveUnidadPeso_Paso = dataResult.Data.CatalogosXLS.Tables["c_ClaveUnidadPeso_Paso"],
                            c_CodigoTransporteAereo_Paso = dataResult.Data.CatalogosXLS.Tables["c_CodigoTransporteAereo_Paso"],
                            c_Colonia_Paso = dataResult.Data.CatalogosXLS.Tables["c_Colonia_Paso"],
                            c_ConfigAutotransporte_Paso = dataResult.Data.CatalogosXLS.Tables["c_ConfigAutotransporte_Paso"],
                            c_ConfigMaritima_Paso = dataResult.Data.CatalogosXLS.Tables["c_ConfigMaritima_Paso"],
                            c_Contenedor_Paso = dataResult.Data.CatalogosXLS.Tables["c_Contenedor_Paso"],
                            c_ContenedorMaritimo_Paso = dataResult.Data.CatalogosXLS.Tables["c_ContenedorMaritimo_Paso"],
                            c_CveTransporte_Paso = dataResult.Data.CatalogosXLS.Tables["c_CveTransporte_Paso"],
                            c_DerechosDePaso_Paso = dataResult.Data.CatalogosXLS.Tables["c_DerechosDePaso_Paso"],
                            c_Estaciones_Paso = dataResult.Data.CatalogosXLS.Tables["c_Estaciones_Paso"],
                            c_FiguraTransporte_Paso = dataResult.Data.CatalogosXLS.Tables["c_FiguraTransporte_Paso"],
                            c_Localidad_Paso = dataResult.Data.CatalogosXLS.Tables["c_Localidad_Paso"],
                            c_MaterialPeligroso_Paso = dataResult.Data.CatalogosXLS.Tables["c_MaterialPeligroso_Paso"],
                            c_Municipio_Paso = dataResult.Data.CatalogosXLS.Tables["c_Municipio_Paso"],
                            c_NumAutorizacionNaviero_Paso = dataResult.Data.CatalogosXLS.Tables["c_NumAutorizacionNaviero_Paso"],
                            c_ParteTransporte_Paso = dataResult.Data.CatalogosXLS.Tables["c_ParteTransporte_Paso"],
                            c_SubTipoRem_Paso = dataResult.Data.CatalogosXLS.Tables["c_SubTipoRem_Paso"],
                            c_TipoCarro_Paso = dataResult.Data.CatalogosXLS.Tables["c_TipoCarro_Paso"],
                            c_TipoDeServicio_Paso = dataResult.Data.CatalogosXLS.Tables["c_TipoDeServicio_Paso"],
                            c_TipoDeTrafico_Paso = dataResult.Data.CatalogosXLS.Tables["c_TipoDeTrafico_Paso"],
                            c_TipoEmbalaje_Paso = dataResult.Data.CatalogosXLS.Tables["c_TipoEmbalaje_Paso"],
                            c_TipoEstacion_Paso = dataResult.Data.CatalogosXLS.Tables["c_TipoEstacion_Paso"],
                            c_TipoPermiso_Paso = dataResult.Data.CatalogosXLS.Tables["c_TipoPermiso_Paso"],
                            UserID = dataResult.User.UserID,
                            BitacoraDescripcion = "Cargar Catálogos Carta Porte"
                        },
                        commandType: CommandType.StoredProcedure
                    );

                    if (Boolean.Parse(result))
                    {
                        dataResult.Status = System.Net.HttpStatusCode.OK;
                        dataResult.Message = "Cargar Catálogos Carta Porte exitoso.";
                    }
                    else
                    {
                        dataResult.Status = System.Net.HttpStatusCode.BadRequest;
                        dataResult.Message = $"Ocurrio un problema al cargar Catálogos Carta Porte. Contacta a tu administrador.";
                    }
                }
            }
            catch (Exception ex)
            {
                dataResult.Status = System.Net.HttpStatusCode.BadRequest;
                dataResult.Message = $"Ocurrio un problema al cargar Catálogos Carta Porte. Contacta a tu administrador. {ex.Message}";
            }

            return dataResult;
        }

        /// <summary>
        /// Valida las tablas Catálogos Carta Porte de paso
        /// </summary>
        /// <returns>Estado de la validación</returns>
        public async Task<DataResult<CatalogosCartaPorteDto>> ValidarAsync(DataResult<CatalogosCartaPorteDto> dataResult)
        {
            try
            {
                using (IDbConnection db = GetConnection())
                {
                    var result = db.ExecuteScalar<string>(
                        sql: "SP_CatalogosCartaPorte_Validar",
                        commandType: CommandType.StoredProcedure
                    );

                    if (Boolean.Parse(result))
                    {
                        dataResult.Data.isValid = true;

                        dataResult.Status = System.Net.HttpStatusCode.OK;
                        dataResult.Message = "Validar Catálogos Carta Porte exitoso.";
                    }
                    else
                    {
                        dataResult.Status = System.Net.HttpStatusCode.OK;
                        dataResult.Message = $"No existen registros de mantenimiento de Catálogos Carta Porte.";
                    }
                }
            }
            catch (Exception ex)
            {
                dataResult.Status = System.Net.HttpStatusCode.BadRequest;
                dataResult.Message = $"Ocurrio un problema al validar Catálogos Carta Porte. Contacta a tu administrador. {ex.Message}";
            }

            return dataResult;
        }

        /// <summary>
        /// Acepta el proceso de mantenimiento de Catálogos Carta Porte
        /// </summary>
        /// <returns>Estado de la aceptación del proceso</returns>
        public async Task<DataResult<CatalogosCartaPorteDto>> AceptarAsync(DataResult<CatalogosCartaPorteDto> dataResult)
        {
            try
            {
                using (IDbConnection db = GetConnection())
                {
                    var result = db.ExecuteScalar<string>(
                        sql: "SP_CatalogosCartaPorte_Aceptar",
                        new
                        {
                            UserID = dataResult.User.UserID,
                            BitacoraDescripcion = "Aceptar Catálogos Carta Porte"
                        },
                        commandType: CommandType.StoredProcedure
                    );

                    if (Boolean.Parse(result))
                    {
                        dataResult.Status = System.Net.HttpStatusCode.OK;
                        dataResult.Message = "Aceptar Catálogos Carta Porte exitoso.";
                    }
                    else
                    {
                        dataResult.Status = System.Net.HttpStatusCode.BadRequest;
                        dataResult.Message = $"Ocurrio un problema al aceptar Catálogos Carta Porte. Contacta a tu administrador.";
                    }
                }
            }
            catch (Exception ex)
            {
                dataResult.Status = System.Net.HttpStatusCode.BadRequest;
                dataResult.Message = $"Ocurrio un problema al aceptar Catálogos Carta Porte. Contacta a tu administrador. {ex.Message}";
            }

            return dataResult;
        }

        /// <summary>
        /// Select a la tabla de Bitacora Catálogos Carta Porte
        /// </summary>
        /// <param name="pageNumber">Numero de la pagina</param>
        /// <param name="pageSize">Tamaño de la pagina</param>
        /// <param name="esDescarga">Descarga la tabla completa o la paginación</param>
        /// <returns>Tabla de bitacora</returns>
        public async Task<DataResult<IEnumerable<CatalogosCartaPorteDto>>> BitacoraSeleccionarAsync(int pageNumber = 1, int pageSize = 10, bool esDescarga = false)
        {
            DataResult<IEnumerable<CatalogosCartaPorteDto>> resultItem = new DataResult<IEnumerable<CatalogosCartaPorteDto>>()
            {
                Status = System.Net.HttpStatusCode.OK,
                Message = "Seleccionar Bitacora Catálogos Carta Porte exitoso."
            };

            try
            {
                using (IDbConnection db = GetConnection())
                {
                    DynamicParameters par = new DynamicParameters();
                    par.Add("@PageNumber", pageNumber);
                    par.Add("@PageSize", pageSize);
                    par.Add("@esDescarga", esDescarga);

                    var result = db.QueryMultiple(sql: "SP_CatalogosCartaPorte_BitacoraSeleccionar", param: par, commandType: CommandType.StoredProcedure);

                    var paging = result.Read<Pager>();
                    var select = result.Read<CatalogosCartaPorteDto>();

                    resultItem.Pager = paging.Single();
                    resultItem.Data = select;
                }
            }
            catch (Exception ex)
            {
                resultItem.Message = $"Ocurrio un problema al seleccionar Bitacora Catálogos Carta Porte. Contacta a tu administrador. {ex.Message}";
                resultItem.Status = System.Net.HttpStatusCode.BadRequest;
            }

            return resultItem;
        }

        /// <summary>
        /// Envía el correo electrónico del resultado del proceso de carga de archivo xls del SAT
        /// </summary>
        /// <param name="mailBody">Cuerpo del correo electrónico</param>
        /// <returns>Tabla de bitacora</returns>
        public async Task<DataResult<CatalogosCartaPorteDto>> EnviarCorreo(DataResult<CatalogosCartaPorteDto> dataResult, string mailBody)
        {
            try
            {
                using (var smtpClient = new SmtpClient())
                {
                    smtpClient.Host = _configuration["Email:Host"];

                    using (var mailMessage = new MailMessage())
                    {
                        mailMessage.To.Add(new MailAddress(dataResult.User.Email));
                        mailMessage.From = new MailAddress(_configuration["Email:From"]);
                        mailMessage.Subject = "Catálogos Carta Porte";
                        mailMessage.Body = mailBody;

                        smtpClient.Send(mailMessage);

                        dataResult.Status = System.Net.HttpStatusCode.OK;
                        dataResult.Message = "Envíar Correo Catálogos Carta Porte exitoso.";
                    }
                }
            }
            catch (Exception ex)
            {
                dataResult.Status = System.Net.HttpStatusCode.BadRequest;
                dataResult.Message = $"Ocurrio un problema al envíar correo Catálogos Carta Porte. Contacta a tu administrador. {ex.Message}";
            }

            return dataResult;
        }
    }
}

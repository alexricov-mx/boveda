using BERecepcion.Core.Admin.Dto;
using BERecepcion.Core.Common.Results;
using BERecepcion.Core.Consulta.Copades.Dto;
using BERecepcion.Core.Copades.Interfaces.Repositories;
using BERecepcion.Core.Dto;
using BERecepcion.Core.eSignDto;
using BERecepcion.Core.FirmaDocumentos.Dto;
using BERecepcion.Core.Interfaces;
using BERecepcion.Infraestructura.Repositories;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.Ocsp;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BERecepcion.Infraestructura.Copades.Repositories
{
    public class CopadeRepository 
        : BaseSQLServerSqlRepository, ICopadeRepository
    {
        private readonly IConfiguration _configuration;

        public CopadeRepository(
           IDbConnectionFactory connectionFactory
            ) : base(connectionFactory)
        {

        }
        public async Task<DataResult<IEnumerable<CopadeDto>>> GetListaFiltroCopadesAsync(string UserID, int pageSize, int pageNum = 1, string search = null)
        {
            DataResult<IEnumerable<CopadeDto>> resultItem = new DataResult<IEnumerable<CopadeDto>>
            {
                Status = HttpStatusCode.OK,
            };

            try
            {

                using (IDbConnection db = GetConnection())
                {
                    DynamicParameters par = new DynamicParameters();
                    par.Add("@UserID", UserID);
                    par.Add("@search", search);
                    par.Add("@pagenum", pageNum);
                    par.Add("@pagesize", pageSize);

                    var result = await db.QueryMultipleAsync(sql: "SP_copade_filtro_seleccion", param: par, commandType: CommandType.StoredProcedure);

                    var paging = await result.ReadAsync<Pager>();
                    var copaderesultfiltro = await result.ReadAsync<CopadeDto>();

                    resultItem.Pager = paging.FirstOrDefault();

                    resultItem.Data = copaderesultfiltro;

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
        public async Task<PagedResult<CopadeDto>> GetPagedFiltroCopadesAsync(
            string userID,
            int pageSize,
            int pageNum,
            string search,
            CancellationToken cancellationToken = default)
        {
            if (pageNum < 1) pageNum = 1;
            if (pageSize < 1) pageSize = 5;

            using IDbConnection db = GetConnection();

            var par = new DynamicParameters();
            par.Add("@UserID", userID);
            par.Add("@search", search);
            par.Add("@pagenum", pageNum);
            par.Add("@pagesize", pageSize);

            var multi = await db.QueryMultipleAsync(
                sql: "SP_copade_filtro_seleccion",
                param: par,
                commandType: CommandType.StoredProcedure);

            var totalItems = (await multi.ReadAsync<int>()).FirstOrDefault();
            var items = (await multi.ReadAsync<CopadeDto>()).ToList();
            var pager = new Pager(totalItems, pageNum, pageSize);



            return new PagedResult<CopadeDto>(
                items,
                totalItems: pager.TotalItems,
                pageNumber: pager.CurrentPage,
                pageSize: pager.PageSize);
        }

        public async Task<DataResult<CopadeDto>> CopadeFirmaAsync(Guid CopadeID, string Token)
        {
            DataResult<CopadeDto> resultItem = new DataResult<CopadeDto>()
            {
                Status = HttpStatusCode.OK,
                Message = "Firmado exitosamente"
            };
            try
            {
                using (IDbConnection db = GetConnection())
                {
                    //db.Open();
                    //using (var tran = db.BeginTransaction())
                    //{
                        try
                        {
                            DynamicParameters par = new DynamicParameters();
                            par.Add("@CopadeID", CopadeID);
                            par.Add("@Token", Token);
                            await db.QueryAsync(sql: "SP_Copade_firma", param: par, commandType: CommandType.StoredProcedure);
                            //tran.Commit();
                            resultItem.Message = "Se actualizaron los datos correctamente";
                        }
                        catch (Exception ext)
                        {
                            //tran.Rollback();
                            resultItem.Message = ext.Message;
                            resultItem.Status = HttpStatusCode.BadRequest;
                            return resultItem;
                        }
                    //}
                    return resultItem;
                }
            }
            catch (Exception ext)
            {
                resultItem.Message = ext.Message;
                resultItem.Status = HttpStatusCode.BadRequest;
                return resultItem;
            }
        }
        public async Task<DataResult<CopadeDto>> CopadeFirmaCorreoAsync(Guid CopadeID, string Email, string Paso)
        {
            DataResult<CopadeDto> resultItem = new DataResult<CopadeDto>()
            {
                Status = HttpStatusCode.OK,
                Message = "Firmado exitosamente"
            };

            string QuerySP = string.Empty;
            switch (Paso)
            {
                case "1":
                    QuerySP = "SP_copade_correo_1";
                    break;
                case "2":
                    QuerySP = "SP_copade_correo_2";
                    break;
                case "P":
                    QuerySP = "SP_copade_correo_PreFactura";
                    break;
            };

            try
            {
                using (IDbConnection db = GetConnection())
                {
                    DynamicParameters par = new DynamicParameters();
                    par.Add("@CopadeID", CopadeID.ToString());
                    par.Add("@Email", Email);
                    await db.QueryAsync(sql: QuerySP, param: par, commandType: CommandType.StoredProcedure);
                    resultItem.Message = "Se actualizaron los datos correctamente";
                    return resultItem;
                }
            }
            catch (Exception ext)
            {
                resultItem.Message = ext.Message;
                resultItem.Status = HttpStatusCode.BadRequest;
                return resultItem;
            }
        }
        public async Task<DataResult<ExternosDto>> PostFirmaAsync(ExternosDto externos)
        {
            DataResult<ExternosDto> resultItemIenum = new DataResult<ExternosDto>()
            {
                Status = HttpStatusCode.OK,
                Message = "Envío Exitoso PostFirmaAsync"
            };

            try
            {
                //var client = new RestClient("https://localhost:62117/api/");
                var client = new RestClient(_configuration["eSign:Endpoint:url"]);
                var request = new RestRequest("Externos/Firma", Method.Post);
                request.AddHeader("content-type", "application/json");
                request.AddHeader("Accept", "application/json");
                request.AddParameter("Externos", JsonConvert.SerializeObject(externos), ParameterType.RequestBody);
                var response = await client.ExecuteAsync(request);

                try
                {
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        resultItemIenum.Message = $"Ocurrio un problema. Contacta a tu administrador. {response.ErrorMessage}";
                        resultItemIenum.Status = HttpStatusCode.BadRequest;
                        return resultItemIenum;
                    }

                    // La firma fue exitosa, entonces insertamos en documentofirmado en BE

                    DynamicParameters par = new DynamicParameters();
                    par.Add("@paqueteId", externos.documentoFirma.PaqueteId);
                    par.Add("@documentoId", externos.documentoFirma.DocumentoId);
                    par.Add("@usuarioId", externos.documentoFirma.UsuarioId);
                    par.Add("@Orden", externos.paqueteFirmante.OrdenFirmado);
                    // DocumentType
                    // CO - Contratos
                    // OS - Orden de Surtimiento
                    // ES - Estimacion de obra
                    // CP - Copade
                    // AP - Analitico de Pago
                    // PP - Programa de Pago
                    // LP - Lista de Pago
                    par.Add("@DocumentType", externos.documentoFirmaBE.DocumentType);
                    par.Add("@usuarioBEId", externos.documentoFirmaBE.usuarioBEId);
                    par.Add("@DocumentoBEId", externos.documentoFirmaBE.documentoBEId);

                    /*
                     * 	@paqueteId					integer , 
                        @documentoId				integer,
                        @usuarioId					integer , 
                        @DocumentType				VARCHAR(2) , 
                        @Orden						integer , 
                        @usuarioBEId				uniqueidentifier 
                     * 
                     */

                    //using para levantar la conexion al BD
                    using (IDbConnection db = GetConnection())
                    {
                        var resultDocumento = await db.QueryFirstOrDefaultAsync<string>(sql: "SP_DocumentoFirmado_inserta", param: par, commandType: CommandType.StoredProcedure);
                        resultItemIenum.Message = "Documento firma registrado con exito";
                        return resultItemIenum;
                    }
                }
                catch (Exception)
                {
                    resultItemIenum.Message = $"Ocurrio un problema. Contacta a tu administrador. {response.Content}";
                    resultItemIenum.Status = HttpStatusCode.BadRequest;
                    return resultItemIenum;
                }
            }
            catch (Exception ex)
            {
                resultItemIenum.Message = $"Ocurrio un problema. Contacta a tu administrador. {ex.Message}";
                resultItemIenum.Status = HttpStatusCode.BadRequest;
                return resultItemIenum;
            }
        }
        public async Task<DataResult<ExternosDto>> PostDocumentoAsync(ExternosDto externos, IFormFile documentoPDF)
        {
            DataResult<ExternosDto> resultItemIenum = new DataResult<ExternosDto>()
            {
                Status = HttpStatusCode.OK,
                Message = "Envío Exitoso PostDocumentoAsync"
            };
            try
            {
                var client = new RestClient(_configuration["eSign:Endpoint:url"]);
                var request = new RestRequest("Externos/Documento", Method.Post) { RequestFormat = DataFormat.Json, AlwaysMultipartFormData = true };
                request.AddHeader("Content-Type", "multipart/form-data");
                request.AddHeader("Accept", "application/json");
                using (var ms = new MemoryStream())
                {
                    documentoPDF.CopyTo(ms);
                    var fileBytes = ms.ToArray();
                    request.AddFile("model.DocumentoPDF", fileBytes, documentoPDF.FileName);// Byte Array
                }
                request.AddParameter("model.Externos", JsonConvert.SerializeObject(externos));
                var response = await client.ExecuteAsync(request);
                try
                {
                    ExternosDto resp = JsonConvert.DeserializeObject<ExternosDto>(response.Content);
                    resultItemIenum.Data = resp;
                    return resultItemIenum;
                }
                catch (Exception)
                {
                    resultItemIenum.Message = $"Ocurrio un problema. Contacta a tu administrador. {response.Content}";
                    resultItemIenum.Status = HttpStatusCode.BadRequest;
                    return resultItemIenum;
                }
            }
            catch (Exception ex)
            {
                resultItemIenum.Message = $"Ocurrio un problema. Contacta a tu administrador. {ex.Message}";
                resultItemIenum.Status = HttpStatusCode.BadRequest;
                return resultItemIenum;
            }
        }
        public async Task<DataResult<IEnumerable<UsersDto>>> GetSignersByCopadeID(Guid CopadeID)
        {
            DataResult<IEnumerable<UsersDto>> resultItem = new DataResult<IEnumerable<UsersDto>>
            {
                Status = HttpStatusCode.OK,
                Message = "GetSignersByCopadeID"
            };

            try
            {
                DynamicParameters par = new DynamicParameters();
                par.Add("@CopadeID", CopadeID);

                using (IDbConnection db = GetConnection())
                {
                    var result = await db.QueryMultipleAsync(sql: "SP_copade_signer_email", param: par, commandType: CommandType.StoredProcedure);
                    var copaderesultfiltro = await result.ReadAsync<SignersDto>();

                    List<SignersDto> dato = new List<SignersDto>();
                    foreach (var f in copaderesultfiltro)
                    {
                        SignersDto m = new SignersDto();
                        m.signer1 = f.signer1;
                        m.signer1Email = f.signer1Email;
                        m.alternate1 = f.alternate1;
                        m.alternate1Email = f.alternate1Email;
                        m.signer2 = f.signer2;
                        m.signer2Email = f.signer2Email;
                        m.alternate2 = f.alternate2;
                        m.alternate2Email = f.alternate2Email;
                        m.functionary1signdate = f.functionary1signdate;
                        m.functionary2signdate = f.functionary2signdate;
                        dato.Add(m);
                    }

                    List<UsersDto> datoUser = new List<UsersDto>();
                    if (dato.Count > 0)
                    {
                        if (dato[0].functionary1signdate == null)
                        {
                            if (!string.IsNullOrEmpty(dato[0].signer1))
                            {
                                //UsersDto u = new UsersDto() { Email = dato[0].signer1Email, Token = dato[0].signer1, Name = dato[0].signer1Email };
                                UsersDto u = new UsersDto() { Email = dato[0].signer1Email, Name = dato[0].signer1 };
                                datoUser.Add(u);
                            }
                            if (!string.IsNullOrEmpty(dato[0].alternate1))
                            {
                                UsersDto u = new UsersDto() { Email = dato[0].alternate1Email, Name = dato[0].signer1 };
                                datoUser.Add(u);
                            }
                        }

                        if (dato[0].functionary2signdate == null)
                        {
                            if (!string.IsNullOrEmpty(dato[0].signer2))
                            {
                                UsersDto u = new UsersDto() { Email = dato[0].signer2Email, Name = dato[0].signer2 };
                                datoUser.Add(u);
                            }
                            if (!string.IsNullOrEmpty(dato[0].alternate2))
                            {
                                UsersDto u = new UsersDto() { Email = dato[0].alternate2Email, Name = dato[0].alternate2 };
                                datoUser.Add(u);
                            }
                        }
                    }
                    resultItem.Data = datoUser;

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
        public async Task<DataResult<SignersDto>> GetSigners2ByCopadeID(Guid CopadeID)
        {
            DataResult<SignersDto> resultItem = new DataResult<SignersDto>
            {
                Status = HttpStatusCode.OK,
                Message = "GetSignersByCopadeID"
            };

            try
            {
                DynamicParameters par = new DynamicParameters();
                par.Add("@CopadeID", CopadeID);

                using (IDbConnection db = GetConnection())
                {
                    var result = await db.QueryFirstOrDefaultAsync<SignersDto>(sql: "SP_copade_signer_email", param: par, commandType: CommandType.StoredProcedure);
                    resultItem.Data = result;
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


        public async Task<DataResult<IEnumerable<CopadeDto>>> GetListaCopadeBancarioAsync(DateTime start, DateTime end, string search, string UserID, string claveOrganismo, string creditorNumber, int pageSize, int pageNum = 1, bool esDescarga = false)
        {
            DataResult<IEnumerable<CopadeDto>> resultItem = new DataResult<IEnumerable<CopadeDto>>
            {
                Status = HttpStatusCode.OK,
            };

            if (!string.IsNullOrWhiteSpace(search))
                search = Core.Utils.SearchText.GetWhereClause(search, new List<string> { "clave", "Contract", "SapOrder", "Reception", "Center", "Exercise" });

            try
            {

                using (IDbConnection db = GetConnection())
                {
                    DynamicParameters par = new DynamicParameters();
                    par.Add("UserID", UserID);
                    par.Add("@start", start);
                    par.Add("@end", end);
                    par.Add("@search", search);
                    par.Add("@pagenum", pageNum);
                    par.Add("@pagesize", pageSize);
                    par.Add("@claveOrganismo", claveOrganismo);
                    par.Add("@creditorNumber", creditorNumber);
                    par.Add("@esDescarga", esDescarga);

                    var result = await db.QueryMultipleAsync(sql: "SP_copade_bancario_tabla_seleccion", param: par, commandType: CommandType.StoredProcedure);

                    var paging = await result.ReadAsync<Pager>();
                    var copaderesultfiltro = await result.ReadAsync<CopadeDto>();

                    resultItem.Pager = paging.FirstOrDefault();


                    resultItem.Data = copaderesultfiltro;

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
    }
}

using BERecepcion.Core.Dto;
using BERecepcion.Core.eSignDto;
using BERecepcion.Core.FirmaDocumentos.Dto;
using BERecepcion.Core.FirmaDocumentos.Interfaces.Repositories;
using BERecepcion.Infraestructura.Repositories;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace BERecepcion.Infraestructura.FirmaDocumentos.Repositories
{
    public class DocumentoFirmadoRepository : BaseSQLServerSqlRepository, IDocumentoFirmadoRepository
    {
        public DocumentoFirmadoRepository(string cnnString) : base(cnnString)
        {
        }

        public async Task<string> ActualizaFirmaAsync(ExternosDto externos, string orden)
        {
            // La firma fue exitosa, entonces insertamos en documentofirmado en BE

            // El orden se determinará por la fecha del primer firmante
            //string orden = string.IsNullOrWhiteSpace(externos.supplyOrder.FunctionarySignDate.ToString()) ? "1" : "2";

            DynamicParameters par = new DynamicParameters();
            par.Add("@paqueteId", externos.documentoFirma.PaqueteId);
            par.Add("@documentoId", externos.documentoFirma.DocumentoId);
            par.Add("@usuarioId", externos.documentoFirma.UsuarioId);
            par.Add("@Orden", orden);
            // DocumentType
            // CO - Contratos
            // OS - Orden de Surtimiento
            // ES - Estimacion de obra
            // CP - Copade
            // AP - Analitico de Pago
            // PP - Programa de Pago
            // LP - Lista de Pago
            //par.Add("@DocumentType", externos.documentoFirmaBE.DocumentType);
            //par.Add("@usuarioBEId", externos.documentoFirmaBE.usuarioBEId);
            //par.Add("@DocumentoBEId", externos.documentoFirmaBE.documentoBEId);

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
                return await db.QueryFirstOrDefaultAsync<string>(sql: "SP_DocumentoFirmado_actualiza", param: par, commandType: CommandType.StoredProcedure);
            }
        }

        public async Task<string> ActualizaFirmaByCorrelationIdAsync(Guid correlationId)
        {
            // La firma fue exitosa, entonces actualizamos con la fecha el documentofirmado en BE

            DynamicParameters par = new DynamicParameters();
            par.Add("@DocumentoFirmadoID", correlationId);
               
            //using para levantar la conexion al BD
            using (IDbConnection db = GetConnection())
            {
                return await db.QueryFirstOrDefaultAsync<string>(sql: "SP_DocumentoFirmado_actualiza_firma", param: par, commandType: CommandType.StoredProcedure);
            }
        }

        public async Task<string> CreaPaqueteAsync(ExternosDto externos, string orden)
        {

            /*
             * 
             *  Cambio para implementar eFirma
             *  ALTER TABLE DocumentoFirmado ALTER COLUMN paqueteId INT NULL;
             *  ALTER TABLE DocumentoFirmado ALTER COLUMN documentoId INT NULL;
             *  ALTER TABLE DocumentoFirmado ALTER COLUMN usuarioId INT NULL;
             *  ALTER TABLE DocumentoFirmado ALTER COLUMN orden INT NULL;
             *  ALTER TABLE DocumentoFirmado ALTER COLUMN DocumentType varchar NULL;
             *
             */

            // La firma fue exitosa, entonces insertamos en documentofirmado en BE

            // El orden se determinará por la fecha del primer firmante
            //string orden = string.IsNullOrWhiteSpace(externos.supplyOrder.FunctionarySignDate.ToString()) ? "1" : "2";

            DynamicParameters par = new DynamicParameters();
            par.Add("@paqueteId", externos.paqueteDocumento.PaqueteId);
            par.Add("@documentoId", externos.paqueteDocumento.DocumentoId);
            par.Add("@usuarioId", externos.usuario.Id.ToString());
            par.Add("@Orden", orden);
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
                return await db.QueryFirstOrDefaultAsync<string>(sql: "SP_DocumentoFirmado_inserta", param: par, commandType: CommandType.StoredProcedure);
            }
        }

        /// <summary>
        /// Crear el paquete inicial
        /// </summary>
        /// <param name="externos"></param>
        /// <param name="orden"></param>
        /// <returns>
        /// regresaria el CorrelationID
        /// </returns>
        public async Task<Guid> CreaPaqueteInicialAsync(string usuarioId, string DocumentType, Guid usuarioBEId, Guid documentoBEId, string orden)
        {
                       
            DynamicParameters par = new DynamicParameters();
            // DocumentType
            // CO - Contratos
            // OS - Orden de Surtimiento
            // ES - Estimacion de obra
            // CP - Copade
            // AP - Analitico de Pago
            // PP - Programa de Pago
            // LP - Lista de Pago
            par.Add("@usuarioId", usuarioId);
            par.Add("@DocumentType", DocumentType);
            par.Add("@usuarioBEId", usuarioBEId);
            par.Add("@DocumentoBEId", documentoBEId);
            par.Add("@Orden", orden);

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
                return await db.QueryFirstOrDefaultAsync<Guid>(sql: "SP_DocumentoFirmado_inserta_inicial", param: par, commandType: CommandType.StoredProcedure);
            }
        }

        public async Task ActualizaPaqueteInicialAsync(Guid correlationId,string paqueteId, string documentoId, string orden)
        {

            DynamicParameters par = new DynamicParameters();
            // DocumentType
            // CO - Contratos
            // OS - Orden de Surtimiento
            // ES - Estimacion de obra
            // CP - Copade
            // AP - Analitico de Pago
            // PP - Programa de Pago
            // LP - Lista de Pago            
            par.Add("@DocumentoFirmadoID", correlationId);
            par.Add("@paqueteId", paqueteId);
            par.Add("@documentoId", documentoId);
            par.Add("@Orden", orden);

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
                await db.QueryFirstOrDefaultAsync(sql: "SP_DocumentoFirmado_actualiza_inicial", param: par, commandType: CommandType.StoredProcedure);
            }
        }

        public async Task<DataResult<DocumentoFirmadoDto>> GetDocumentoFirmadoAsync(Guid DocumentoBEId, int? Orden = null)
        {
            DataResult<DocumentoFirmadoDto> resultItem = new DataResult<DocumentoFirmadoDto>()
            {
                Status = System.Net.HttpStatusCode.OK
            };
            try
            {
                using (IDbConnection db = GetConnection())
                {
                    DynamicParameters par = new DynamicParameters();
                    par.Add("@DocumentoBEId", DocumentoBEId);
                    par.Add("@Orden", Orden);

                    var documento = await db.QueryFirstOrDefaultAsync<DocumentoFirmadoDto>(sql: "SP_DocumentoFirmado_Selecciona ", param: par, commandType: CommandType.StoredProcedure);
                    if (documento == null)
                    {
                        resultItem.Status = System.Net.HttpStatusCode.BadRequest;
                        resultItem.Message = "No se encontró el documento en la bóveda electrónica.";
                        return resultItem;
                    }
                    resultItem.Data = documento;
                }
                return resultItem;
            }
            catch (Exception ex)
            {
                resultItem.Status = System.Net.HttpStatusCode.BadRequest;
                resultItem.Message = $"Ocurrio un problema: {ex.Message}";
                return resultItem;
            }
        }

        public async Task<DataResult<DocumentoFirmadoDto>> GetDocumentoAPFirmadoAsync(Guid DocumentoBEId)
        {
            DataResult<DocumentoFirmadoDto> resultItem = new DataResult<DocumentoFirmadoDto>()
            {
                Status = System.Net.HttpStatusCode.OK
            };
            try
            {
                using (IDbConnection db = GetConnection())
                {
                    DynamicParameters par = new DynamicParameters();
                    par.Add("@DocumentoBEId", DocumentoBEId);

                    var documento = await db.QueryFirstOrDefaultAsync<DocumentoFirmadoDto>(sql: "SP_DocumentoFirmadoAP_Selecciona ", param: par, commandType: CommandType.StoredProcedure);
                    if (documento == null)
                    {
                        resultItem.Status = System.Net.HttpStatusCode.BadRequest;
                        resultItem.Message = "No se encontró el documento en la bóveda electrónica.";
                        return resultItem;
                    }
                    resultItem.Data = documento;
                }
                return resultItem;
            }
            catch (Exception ex)
            {
                resultItem.Status = System.Net.HttpStatusCode.BadRequest;
                resultItem.Message = $"Ocurrio un problema: {ex.Message}";
                return resultItem;
            }
        }
    }
}

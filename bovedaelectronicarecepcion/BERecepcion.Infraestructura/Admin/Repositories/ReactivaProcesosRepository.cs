using BERecepcion.Core.Admin.Dto;
using BERecepcion.Core.Admin.Interfaces.Repositories;
using BERecepcion.Core.Common.Results;
using BERecepcion.Core.Dto;
using BERecepcion.Core.Interfaces;
using BERecepcion.Core.SAPPI.Interfaces.Repositories;
using BERecepcion.Infraestructura.Repositories;
using Dapper;
using RestSharp;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace BERecepcion.Infraestructura.Admin.Repositories
{
    public class ReactivaProcesosRepository 
        : BaseSQLServerSqlRepository, IReactivaProcesosRepository
    {
        private readonly ISAPPIRepository _sapPIRepository;
        public ReactivaProcesosRepository(IDbConnectionFactory connectionFactory)
            : base(connectionFactory)
        {

        }
        public ReactivaProcesosRepository(string cnnString, ISAPPIRepository sAPPIRepository) : base(cnnString)
        {
            _sapPIRepository = sAPPIRepository;
        }

        DataResult<IEnumerable<ReactivaProcesosResponseDto>> resultItemIenum = new DataResult<IEnumerable<ReactivaProcesosResponseDto>>()
        {
            Status = System.Net.HttpStatusCode.OK,
            Message = "Todo bien"
        };

        DataResult<ReactivaProcesosDto> resultItem = new DataResult<ReactivaProcesosDto>()
        {
            Status = System.Net.HttpStatusCode.OK,
            Message = "Todo bien"
        };

        public async Task<DataResult<ReactivaProcesosDto>> GetEnvioFirmaAsync(string SAPOrder)
        {

            try
            {
                DynamicParameters par = new DynamicParameters();
                par.Add("@SAPOrder", SAPOrder);
                //using para levantar la conexion al BD
                using (IDbConnection db = GetConnection())
                {
                    var result = await db.QueryFirstOrDefaultAsync<ReactivaProcesosDto>(sql: "SP_envio_firma_selecciona", param: par, commandType: CommandType.StoredProcedure);
                    resultItem.Data = result;

                    return resultItem;
                }
            }
            catch (Exception ext)
            {
                resultItem.Message = ext.Message;
                resultItem.Status = System.Net.HttpStatusCode.BadRequest;
                Log.Error(ext.Message);
                return resultItem;
            }
        }

        public async Task<DataResult<JsonRP>> PIEnvioReactivaProcesosAsync(DataResult<JsonRP> datosRect)
        {
            DataResult<JsonRP> resultItem = new DataResult<JsonRP>()
            {
                Status = System.Net.HttpStatusCode.OK,
                Message = "Todo bien"
            };

            try
            {
                var respPI = await _sapPIRepository.PostOS_Response(datosRect.Data.oSResponse);
                resultItem.Status = respPI.Status;
                using (IDbConnection db = GetConnection())
                {
                    DynamicParameters par = new DynamicParameters();
                    par.Add("@SAPOrderRP", datosRect.Data.SAPOrderRP);
                    par.Add("@OrganisName", datosRect.Data.Name);
                    par.Add("@DocumentType", datosRect.Data.TypeDocument);
                    par.Add("@Type", datosRect.Data.Type);
                    par.Add("@Contract", datosRect.Data.Contract);
                    par.Add("@Result", respPI.Status == System.Net.HttpStatusCode.OK ? "Exitoso" : "No exitoso");
                    par.Add("@MensajeResult", respPI.Status == System.Net.HttpStatusCode.OK ? respPI.Data.item.STATUS : respPI.Message);
                    par.Add("@Usuario_Modificador", datosRect.Data.Usuario_Modificador);
                    par.Add("@valida", datosRect.Data.valida);

                    await db.ExecuteAsync(sql: "SP_ReactivaProcesos_inserta", param: par, commandType: CommandType.StoredProcedure);
                    resultItem.Data = datosRect.Data;
                    return resultItem;
                }
            }
            catch (Exception ex)
            {
                resultItemIenum.Message = $"Ocurrio un problema. Contacta a tu administrador. {ex.Message}";
                resultItem.Status = System.Net.HttpStatusCode.BadRequest;
                resultItem.Data = null;
                Log.Error(ex.Message);
                return resultItem;
            }
        }

        public async Task<DataResult<IEnumerable<ReactivaProcesosResponseDto>>> GetReactivaProcesosAsync(string OrderSAP)
        {

            try
            {
                DynamicParameters par = new DynamicParameters();
                par.Add("@SAPOrder", OrderSAP);

                using (IDbConnection db = GetConnection())
                {
                    var result = await db.QueryAsync<ReactivaProcesosResponseDto>(sql: "SP_envio_ReactivaProcesos_selecciona ", param: par, commandType: CommandType.StoredProcedure);
                    if (result != null)
                    {
                        foreach (var r in result)
                        {
                            if (!string.IsNullOrEmpty(r.ResultadoTarea1)) r.ResultadoTarea1 = r.ResultadoTarea1.Replace("Ocurrio un problema. Contacta a tu administrador.", "").Trim();
                            if (!string.IsNullOrEmpty(r.ResultadoTarea2)) r.ResultadoTarea2 = r.ResultadoTarea2.Replace("Ocurrio un problema. Contacta a tu administrador.", "").Trim();
                        }
                    }
                    resultItemIenum.Data = result;

                    return resultItemIenum;
                }
            }
            catch (Exception ext)
            {
                resultItemIenum.Message = ext.Message;
                resultItemIenum.Status = System.Net.HttpStatusCode.BadRequest;

                return resultItemIenum;
            }

        }

        public async Task<IEnumerable<ReactivaProcesosResponseDto>> GetReactivaProcesosBySAPOrderAsync(string OrderSAP, CancellationToken cancellationToken)
        {

                using IDbConnection db = GetConnection();

                DynamicParameters par = new();
                par.Add("@SAPOrder", OrderSAP);

                var result = await db.QueryAsync<ReactivaProcesosResponseDto>(
                    sql: "SP_envio_ReactivaProcesos_selecciona ", 
                    param: par, 
                    commandType: CommandType.StoredProcedure 
                    );
                if (result != null)
                {
                    foreach (var r in result)
                    {
                        if (!string.IsNullOrEmpty(r.ResultadoTarea1)) r.ResultadoTarea1 = r.ResultadoTarea1.Replace("Ocurrio un problema. Contacta a tu administrador.", "").Trim();
                        if (!string.IsNullOrEmpty(r.ResultadoTarea2)) r.ResultadoTarea2 = r.ResultadoTarea2.Replace("Ocurrio un problema. Contacta a tu administrador.", "").Trim();
                    }
                }
            return result;

        }

    }
}

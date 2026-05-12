using BERecepcion.Core.Dto;
using BERecepcion.Core.Interfaces.Repositories;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using BERecepcion.Infraestructura.Repositories;
using BERecepcion.Core.Admin.Interfaces.Repositories;
using BERecepcion.Core.Admin.Dto;

namespace BERecepcion.Infraestructura.Admin.Repositories
{
    public class AltaContratosRepository : BaseSQLServerSqlRepository, IAltaContratosRepository
    {
        public AltaContratosRepository(string cnnString) : base(cnnString)
        {

        }

        public async Task<DataResult<IEnumerable<AltaContratosDto>>> ConsultaACAsync(int pageSize, int pageNum = 1, string search = null, bool esDescarga = false)
        {
            DataResult<IEnumerable<AltaContratosDto>> resultItem = new DataResult<IEnumerable<AltaContratosDto>>()
            {
                Status = System.Net.HttpStatusCode.OK,
                Message = "Todo bien"
            };
            try
            {
                // Parametros
                DynamicParameters par = new DynamicParameters();
                par.Add("@pagenum", pageNum);
                par.Add("@pagesize", pageSize);
                par.Add("@search", search);

                //using para levantar la conexion al BD
                using (IDbConnection db = GetConnection())
                {
                    var result = await db.QueryMultipleAsync(sql: "SP_altacontratos_seleccion_Get", param: par, commandType: CommandType.StoredProcedure);

                    var paging = await result.ReadAsync<Pager>();
                    var acCresult = await result.ReadAsync<AltaContratosDto>();

                    resultItem.Pager = paging.FirstOrDefault();

                    resultItem.Data = acCresult;
                    return resultItem;
                }
            }
            catch (Exception ex)
            {
                resultItem.Message = $"Ocurrio un problema. Contacta a tu administrador. {ex.Message}";
                resultItem.Status = System.Net.HttpStatusCode.BadRequest;
                return resultItem;
            }
        }
        public async Task<DataResult<AltaContratosDto>> GetACByIdAsync(Guid AltaContratoID)
        {
            DataResult<AltaContratosDto> dataResult = new DataResult<AltaContratosDto>()
            {
                Status = System.Net.HttpStatusCode.OK,
                Message = "Seleccion de Contrato exitosa"
            };
            try
            {
                DynamicParameters par = new DynamicParameters();
                par.Add("@AltaContratoID", AltaContratoID);
                using (IDbConnection db = GetConnection())
                {
                    var result = await db.QueryFirstOrDefaultAsync<AltaContratosDto>(sql: "SP_altacontratos_ById_Selecciona", param: par, commandType: CommandType.StoredProcedure);
                    if (result == null)
                    {
                        dataResult.Message = "No se encontró el contrato seleccionada, favor de intentar más tarde.";
                        dataResult.Status = System.Net.HttpStatusCode.BadRequest;
                        return dataResult;
                    }
                    dataResult.Data = result;
                    return dataResult;
                }
            }
            catch (Exception ex)
            {
                dataResult.Message = $"Ocurrio un problema. Contacta a tu administrador. {ex.Message}";
                dataResult.Status = System.Net.HttpStatusCode.BadRequest;
                return dataResult;
            }
        }

        public async Task<DataResult<AltaContratosDto>> InsertaACAsync(AltaContratosDto altaContratosDto)
        {
            DataResult<AltaContratosDto> dataResult = new DataResult<AltaContratosDto>()
            {
                Status = System.Net.HttpStatusCode.OK,
                Message = "Registro de Contrato agregado de manera exitosa"
            };
            try
            {
                DynamicParameters par = new DynamicParameters();
                par.Add("@Contract", altaContratosDto.Contract);
                par.Add("@Firma1", altaContratosDto.Firma1);
                par.Add("@Suplente1", altaContratosDto.Suplente1);
                par.Add("@Firma2", altaContratosDto.Firma2);
                par.Add("@Suplente2", altaContratosDto.Suplente2);
                par.Add("@Usuario_alta", altaContratosDto.Usuario_alta);

                //using para levantar la conexion al BD
                using (IDbConnection db = GetConnection())
                {
                    var result = await db.QueryMultipleAsync(sql: "SP_altacontratos_inserta", param: par, commandType: CommandType.StoredProcedure);
                    bool existe = await result.ReadFirstOrDefaultAsync<bool>();
                    if (existe)
                    {
                        dataResult.Message = "El Contrato ingresado ya existe.";
                        dataResult.Status = System.Net.HttpStatusCode.BadRequest;
                        return dataResult;
                    }
                    var contrato = await result.ReadFirstOrDefaultAsync<AltaContratosDto>();
                    if (contrato == null || contrato.AltaContratoID == null || contrato.AltaContratoID == Guid.Empty)
                    {
                        dataResult.Message = "Ocurrió un error al guardar el contrato, si el problema persiste contacte a su administrador.";
                        dataResult.Status = System.Net.HttpStatusCode.BadRequest;
                        return dataResult;
                    }
                    dataResult.Data = contrato;
                    return dataResult;
                }
            }
            catch (Exception ex)
            {
                dataResult.Message = $"Ocurrio un problema. Contacta a tu administrador " + ex.Message.ToString();
                dataResult.Status = System.Net.HttpStatusCode.InternalServerError;
                return dataResult;
            }
        }
        public async Task<DataResult<AltaContratosDto>> ActualizaACAsync(AltaContratosDto ACDto)
        {
            DataResult<AltaContratosDto> dataResult = new DataResult<AltaContratosDto>()
            {
                Status = System.Net.HttpStatusCode.OK,
                Message = "Actualización de Contrato exitosa"
            };
            try
            {
                DynamicParameters par = new DynamicParameters();
                par.Add("@AltaContratoID", ACDto.AltaContratoID);
                par.Add("@Firma1", ACDto.Firma1);
                par.Add("@Suplente1", ACDto.Suplente1);
                par.Add("@Firma2", ACDto.Firma2);
                par.Add("@Suplente2", ACDto.Suplente2);
                par.Add("@Usuario_alta", ACDto.Usuario_alta);

                using (IDbConnection db = GetConnection())
                {
                    var result = await db.QueryMultipleAsync(sql: "SP_altacontratos_update", param: par, commandType: CommandType.StoredProcedure);
                    bool existe = await result.ReadFirstOrDefaultAsync<bool>();
                    if (!existe)
                    {
                        dataResult.Message = "Ocurrió un problema al encontrar el Contrato, si el problema persiste favor de contactar a su administrador.";
                        dataResult.Status = System.Net.HttpStatusCode.BadRequest;
                        return dataResult;
                    }
                    var contrato = await result.ReadFirstOrDefaultAsync<AltaContratosDto>();
                    if (contrato == null || contrato.AltaContratoID == null || contrato.AltaContratoID == Guid.Empty)
                    {
                        dataResult.Message = "Ocurrió un error al editar el contrato, si el problema persiste contacte a su administrador.";
                        dataResult.Status = System.Net.HttpStatusCode.BadRequest;
                        return dataResult;
                    }
                    dataResult.Data = contrato;
                    return dataResult;
                }
            }
            catch (Exception ex)
            {
                dataResult.Message = $"Ocurrio un problema. Contacta a tu administrador. {ex.Message}";
                dataResult.Status = System.Net.HttpStatusCode.InternalServerError;
                return dataResult;
            }
        }
        public async Task<DataResult<AltaContratosDto>> BorraACAsync(Guid AltaContratoID)
        {
            DataResult<AltaContratosDto> dataResult = new DataResult<AltaContratosDto>()
            {
                Status = System.Net.HttpStatusCode.OK,
                Message = "Registro de Contrato eliminado de manera exitosa"
            };
            try
            {
                DynamicParameters par = new DynamicParameters();
                par.Add("@AltaContratoID", AltaContratoID);

                using (IDbConnection db = GetConnection())
                {
                    var result = await db.QueryFirstOrDefaultAsync<AltaContratosDto>(sql: "SP_altacontratos_borra", param: par, commandType: CommandType.StoredProcedure);
                    if (result == null)
                    {
                        dataResult.Message = "Ocurrió un error al eliminar el contrato, si el problema persiste contacte a su administrador.";
                        dataResult.Status = System.Net.HttpStatusCode.BadRequest;
                        return dataResult;
                    }
                    dataResult.Data = result;
                    return dataResult;
                }
            }
            catch (Exception ex)
            {
                dataResult.Message = $"Ocurrio un problema. Contacta a tu administrador. {ex.Message}";
                dataResult.Status = System.Net.HttpStatusCode.BadRequest;
                return dataResult;
            }
        }



        public async Task<DataResult<IEnumerable<AltaContratosDto>>> ConsultaACGetAllAsync(int pageSize, int pageNum)
        {
            DataResult<IEnumerable<AltaContratosDto>> resultItem = new DataResult<IEnumerable<AltaContratosDto>>()
            {
                Status = System.Net.HttpStatusCode.OK,
            };

            try
            {
                //using para levantar la conexion al BD
                using (IDbConnection db = GetConnection())
                {
                    DynamicParameters par = new DynamicParameters();
                    par.Add("@pagenum", pageNum);
                    par.Add("@pagesize", pageSize);


                    var result = await db.QueryMultipleAsync(sql: "SP_altacontratos_seleccion", param: par, commandType: CommandType.StoredProcedure);

                    var paging = await result.ReadAsync<Pager>();
                    var acresult = await result.ReadAsync<AltaContratosDto>();

                    resultItem.Pager = paging.FirstOrDefault();

                    resultItem.Data = acresult;
                    return resultItem;
                }
            }
            catch (Exception ex)
            {
                resultItem.Message = $"Ocurrio un problema. Contacta a tu administrador. {ex.Message}";
                resultItem.Status = System.Net.HttpStatusCode.BadRequest;
                return resultItem;
            }
        }
    }
}

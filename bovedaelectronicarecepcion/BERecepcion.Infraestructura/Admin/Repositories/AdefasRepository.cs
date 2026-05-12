using BERecepcion.Core.Admin.Dto;
using BERecepcion.Core.Admin.Interfaces.Repositories;
using BERecepcion.Core.Dto;
using BERecepcion.Core.Models;
using BERecepcion.Infraestructura.Repositories;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace BERecepcion.Infraestructura.Admin.Repositories
{
    public class AdefasRepository : BaseSQLServerSqlRepository, IAdefasRepository
    {
        public AdefasRepository(string cnnString) : base(cnnString)
        {

        }
        public async Task<DataResult<IEnumerable<AdefasDto>>> GetAdefasAsync(int pageSize, int pageNum = 1, string search = null, bool esDescarga = false)
        {
            var dataResult = new DataResult<IEnumerable<AdefasDto>> { Status = System.Net.HttpStatusCode.OK };
            try
            {
                using (IDbConnection db = GetConnection())
                {
                    DynamicParameters par = new DynamicParameters();
                    par.Add("@pagenum", pageNum);
                    par.Add("@pagesize", pageSize);
                    par.Add("@search", search);
                    par.Add("@esDescarga", esDescarga);

                    var result = await db.QueryMultipleAsync(sql: "SP_adefas_selecciona", param: par, commandType: CommandType.StoredProcedure);
                    var paging = await result.ReadAsync<Pager>();
                    var reception = await result.ReadAsync<AdefasDto>();
                    dataResult.Data = reception;
                    dataResult.Pager = paging.FirstOrDefault();
                }
                return dataResult;
            }
            catch (Exception ext)
            {
                dataResult.Message = ext.Message;
                dataResult.Status = System.Net.HttpStatusCode.BadRequest;
                return dataResult;
            }
        }

        public async Task<DataResult<AdefasDto>> InsertaAdefaAsync(AdefasDto adefaDto)
        {
            DataResult<AdefasDto> resultInsert = new DataResult<AdefasDto>()
            {
                Status = System.Net.HttpStatusCode.OK,
                Message = "Todo bien"
            };
            try
            {
                DynamicParameters par = new DynamicParameters();
                par.Add("@OrganismID", adefaDto.OrganismID);
                par.Add("@AnhioFactura", adefaDto.AnhioFactura);
                par.Add("@InicioVentana", Convert.ToDateTime(adefaDto.InicioVentana));
                par.Add("@FinVentana", Convert.ToDateTime(adefaDto.FinVentana));
                using (IDbConnection db = GetConnection())
                {
                    var result = await db.QueryMultipleAsync(sql: "SP_adefa_inserta", param: par, commandType: CommandType.StoredProcedure);
                    bool existe = await result.ReadFirstOrDefaultAsync<bool>();
                    if (existe)
                    {
                        resultInsert.Message = "Ya existe un periodo de adefa con la información ingresada, favor de verificar.";
                        resultInsert.Status = System.Net.HttpStatusCode.BadRequest;
                        return resultInsert;
                    }
                    var adefa = await result.ReadFirstOrDefaultAsync<AdefasDto>();
                    if (adefa == null || adefa.AdefaID == null || adefa.AdefaID == Guid.Empty)
                    {
                        resultInsert.Message = "Ocurrió un error al guardar el periodo de adefa seleccionado.";
                        resultInsert.Status = System.Net.HttpStatusCode.BadRequest;
                        return resultInsert;
                    }
                    resultInsert.Message = "Datos Insertados correctamente";
                    resultInsert.Data = adefa;
                    return resultInsert;
                }
            }
            catch (Exception ext)
            {
                resultInsert.Message = ext.Message;
                resultInsert.Status = System.Net.HttpStatusCode.BadRequest;
                return resultInsert;
            }
        }

        public async Task<DataResult<AdefasDto>> ActualizaAdefaAsync(AdefasDto adefaDto)
        {
            var dataResult = new DataResult<AdefasDto> { Status = System.Net.HttpStatusCode.OK };
            try
            {
                DynamicParameters par = new DynamicParameters();
                par.Add("@AdefaID", adefaDto.AdefaID);
                par.Add("@InicioVentana", Convert.ToDateTime(adefaDto.InicioVentana));
                par.Add("@FinVentana", Convert.ToDateTime(adefaDto.FinVentana));
                using (IDbConnection db = GetConnection())
                {
                    var result = await db.QueryFirstOrDefaultAsync<AdefasDto>(sql: "SP_adefa_cambio", param: par, commandType: CommandType.StoredProcedure);
                    if (result == null)
                    {
                        dataResult.Message = "Ocurrió un error al editar el periodo de adefa seleccionado.";
                        dataResult.Status = System.Net.HttpStatusCode.BadRequest;
                        return dataResult;
                    }
                    dataResult.Message = "Datos Actualizados Correctamente";
                    dataResult.Data = result;
                    return dataResult;
                }
            }
            catch (Exception ext)
            {
                dataResult.Message = ext.Message;
                dataResult.Status = System.Net.HttpStatusCode.BadRequest;
                return dataResult;
            }
        }

        public async Task<DataResult<AdefasDto>> BorraAdefaAsync(Guid AdefaID)
        {
            var resultDelete = new DataResult<AdefasDto> { Status = System.Net.HttpStatusCode.OK };
            try
            {
                DynamicParameters par = new DynamicParameters();
                par.Add("@AdefaID", AdefaID);
                using (IDbConnection db = GetConnection())
                {
                    var result = await db.QueryFirstOrDefaultAsync<AdefasDto>(sql: "SP_adefa_borra", param: par, commandType: CommandType.StoredProcedure);
                    if (result == null)
                    {
                        resultDelete.Message = "Ocurrió un error al eliminar el periodo de adefa seleccionado.";
                        resultDelete.Status = System.Net.HttpStatusCode.BadRequest;
                        return resultDelete;
                    }
                    resultDelete.Message = "Adefa Borrada Correctamente";
                    resultDelete.Data = result;

                    return resultDelete;
                }
            }
            catch (Exception ext)
            {
                resultDelete.Message = ext.Message;
                resultDelete.Status = System.Net.HttpStatusCode.BadRequest;
                return resultDelete;
            }
        }

        public async Task<DataResult<ValidaPeriodoAdefaDto>> GetValidaPeriodoAdefaAsync(string clave, string AnhiFactura, string FechaFactura)
        {

            DataResult<ValidaPeriodoAdefaDto> resultItemValida = new DataResult<ValidaPeriodoAdefaDto>()
            {
                Status = System.Net.HttpStatusCode.OK,
                Message = "Todo bien"
            };

            try
            {
                DynamicParameters par = new DynamicParameters();
                par.Add("@clave", clave);
                par.Add("@FecAnio", AnhiFactura);
                par.Add("@AnhioFactura", FechaFactura);
                //using para levantar la conexion al BD
                using (IDbConnection db = GetConnection())
                {
                    //var result = await db.QueryAsync(sql: "SP_adefas_selecciona", commandType: CommandType.StoredProcedure);
                    var result = await db.QueryFirstAsync<ValidaPeriodoAdefaDto>(sql: "SP_adefa_ValidaPeriodo_selecciona", param: par, commandType: CommandType.StoredProcedure);
                    resultItemValida.Data = result;

                    return resultItemValida;
                }
            }
            catch (Exception ext)
            {
                resultItemValida.Message = ext.Message;
                resultItemValida.Status = System.Net.HttpStatusCode.BadRequest;

                return resultItemValida;
            }
        }
        public async Task<DataResult<AdefasDto>> GetAdefaByIdAsync(Guid AdefaID)
        {
            var dataResult = new DataResult<AdefasDto> { Status = System.Net.HttpStatusCode.OK };
            try
            {
                using (IDbConnection db = GetConnection())
                {
                    DynamicParameters par = new DynamicParameters();
                    par.Add("@adefaID", AdefaID);

                    var result = await db.QueryFirstOrDefaultAsync<AdefasDto>(sql: "SP_adefa_ById_selecciona", param: par, commandType: CommandType.StoredProcedure);
                    if (result == null)
                    {
                        dataResult.Message = "No se encontró el periodo de Adefa seleccionado.";
                        dataResult.Status = System.Net.HttpStatusCode.BadRequest;
                        return dataResult;
                    }
                    dataResult.Data = result;
                }
                return dataResult;
            }
            catch (Exception ext)
            {
                dataResult.Message = ext.Message;
                dataResult.Status = System.Net.HttpStatusCode.BadRequest;
                return dataResult;
            }
        }
    }
}

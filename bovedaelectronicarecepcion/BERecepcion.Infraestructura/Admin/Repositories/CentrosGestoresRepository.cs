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
    public class CentrosGestoresRepository : BaseSQLServerSqlRepository, ICentrosGestoresRepository
    {
        public CentrosGestoresRepository(string cnnString) : base(cnnString)
        {
        }

        public async Task<DataResult<IEnumerable<ManagementCentersDto>>> GetCGAsync(int pageSize, int pageNum = 1, string search = null, bool esDescarga = false)
        {
            DataResult<IEnumerable<ManagementCentersDto>> resultItem = new DataResult<IEnumerable<ManagementCentersDto>>()
            {
                Status = System.Net.HttpStatusCode.OK,
                Message = "Consulta de Centros Gestores exitosa"
            };
            try
            {
                using (IDbConnection db = GetConnection())
                {
                    DynamicParameters par = new DynamicParameters();
                    par.Add("@pagenum", pageNum);
                    par.Add("@pagesize", pageSize);
                    par.Add("@search", search);
                    par.Add("@esDescarga", esDescarga);
                    var result = await db.QueryMultipleAsync(sql: "SP_cg_selecciona_GetAll", param: par, commandType: CommandType.StoredProcedure);
                    var paging = await result.ReadAsync<Pager>();
                    var cgresult = await result.ReadAsync<ManagementCentersDto>();
                    resultItem.Pager = paging.FirstOrDefault();
                    resultItem.Data = cgresult;
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
        public async Task<DataResult<ManagementCentersDto>> GetCGByIdAsync(Guid ManagementCenterID)
        {
            DataResult<ManagementCentersDto> dataResult = new DataResult<ManagementCentersDto>()
            {
                Status = System.Net.HttpStatusCode.OK,
                Message = "Consulta de Centros Gestores exitosa"
            };
            try
            {
                using (IDbConnection db = GetConnection())
                {
                    DynamicParameters par = new DynamicParameters();
                    par.Add("@ManagementCenterID", ManagementCenterID);
                    var result = await db.QueryFirstOrDefaultAsync<ManagementCentersDto>(sql: "SP_CG_ById_selecciona", param: par, commandType: CommandType.StoredProcedure);
                    if (result == null)
                    {
                        dataResult.Message = "No se encontró el periodo de Adefa seleccionado.";
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
        public async Task<DataResult<ManagementCentersDto>> InsertaCGAsync(ManagementCentersDto dto)
        {
            DataResult<ManagementCentersDto> dataResult = new DataResult<ManagementCentersDto>()
            {
                Status = System.Net.HttpStatusCode.OK,
                Message = "Registro de Centro Gestor agregado de manera exitosa"
            };
            try
            {
                DynamicParameters par = new DynamicParameters();
                par.Add("@Number", dto.Number);
                par.Add("@Description", dto.Description);
                par.Add("@Signer1", dto.Signer1);
                par.Add("@Alternate1", dto.Alternate1);
                par.Add("@Signer2", dto.Signer2);
                par.Add("@Alternate2", dto.Alternate2);
                par.Add("@Type", dto.Type);
                par.Add("@OrganismID", dto.OrganismID);
                par.Add("@UsuarioModificador", dto.UsuarioModificador);

                using (IDbConnection db = GetConnection())
                {
                    var result = await db.QueryMultipleAsync(sql: "SP_cg_inserta", param: par, commandType: CommandType.StoredProcedure);
                    bool existe = await result.ReadFirstOrDefaultAsync<bool>();
                    if (existe)
                    {
                        dataResult.Message = "El Centro Gestor ingresado ya existe.";
                        dataResult.Status = System.Net.HttpStatusCode.BadRequest;
                        return dataResult;
                    }
                    var managementCenter = await result.ReadFirstOrDefaultAsync<ManagementCentersDto>();
                    if (managementCenter == null || managementCenter.ManagementCenterID == null || managementCenter.ManagementCenterID == Guid.Empty)
                    {
                        dataResult.Message = "Ocurrió un error al guardar el Centro Gestor, si el problema persiste contacte a su administrador.";
                        dataResult.Status = System.Net.HttpStatusCode.BadRequest;
                        return dataResult;
                    }
                    dataResult.Data = managementCenter;
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
        public async Task<DataResult<ManagementCentersDto>> ActualizaCGAsync(ManagementCentersDto dto)
        {
            DataResult<ManagementCentersDto> dataResult = new DataResult<ManagementCentersDto>()
            {
                Status = System.Net.HttpStatusCode.OK,
                Message = "Actualización de Centro Gestor exitosa"
            };
            try
            {
                DynamicParameters par = new DynamicParameters();
                par.Add("@ManagementCenterID", dto.ManagementCenterID);
                par.Add("@Description", dto.Description);
                par.Add("@Signer1", dto.Signer1);
                par.Add("@Alternate1", dto.Alternate1);
                par.Add("@Signer2", dto.Signer2);
                par.Add("@Alternate2", dto.Alternate2);
                par.Add("@UsuarioModificador", dto.UsuarioModificador);

                using (IDbConnection db = GetConnection())
                {
                    var result = await db.QueryMultipleAsync(sql: "SP_cg_update", param: par, commandType: CommandType.StoredProcedure);
                    bool existe = await result.ReadFirstOrDefaultAsync<bool>();
                    if (!existe)
                    {
                        dataResult.Message = "Ocurrió un problema al encontrar el Centro Gestor, si el problema persiste favor de contactar a su administrador.";
                        dataResult.Status = System.Net.HttpStatusCode.BadRequest;
                        return dataResult;
                    }
                    var managementCenter = await result.ReadFirstOrDefaultAsync<ManagementCentersDto>();
                    if (managementCenter == null || managementCenter.ManagementCenterID == null || managementCenter.ManagementCenterID == Guid.Empty)
                    {
                        dataResult.Message = "Ocurrió un error al editar el Centro Gestor, si el problema persiste contacte a su administrador.";
                        dataResult.Status = System.Net.HttpStatusCode.BadRequest;
                        return dataResult;
                    }
                    dataResult.Data = managementCenter;
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
        public async Task<DataResult<ManagementCentersDto>> BorraCGAsync(Guid ManagementCenterId)
        {
            DataResult<ManagementCentersDto> dataResult = new DataResult<ManagementCentersDto>()
            {
                Status = System.Net.HttpStatusCode.OK,
                Message = "Registro de Centro Gestor eliminado de manera exitosa"
            };
            try
            {
                DynamicParameters par = new DynamicParameters();
                par.Add("@managementCenterID", ManagementCenterId);

                using (IDbConnection db = GetConnection())
                {
                    var result = await db.QueryFirstAsync<ManagementCentersDto>(sql: "SP_cg_delete", param: par, commandType: CommandType.StoredProcedure);
                    if (result == null)
                    {
                        dataResult.Message = "Ocurrió un error al eliminar el Centro Gestor, si el problema persiste contacte a su administrador.";
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
        public async Task<DataResult<IEnumerable<ManagementCentersDto>>> CargaCentrosGAsync(IEnumerable<ManagementCentersDto> result)
        {

            DataResult<IEnumerable<ManagementCentersDto>> resultItemIenum = new DataResult<IEnumerable<ManagementCentersDto>>()
            {
                Status = System.Net.HttpStatusCode.OK,
                Message = "Carga de Centros Gestores exitosa"
            };

            using (IDbConnection db = GetConnection())
            {
                DynamicParameters par = new DynamicParameters();

                try
                {
                    var dt = new DataTable("dbo.TypeCentrosGestores");
                    dt.Columns.Add("OrganismoClave");
                    dt.Columns.Add("Number");
                    dt.Columns.Add("Description");
                    dt.Columns.Add("Signer1");
                    dt.Columns.Add("Alternate1");
                    dt.Columns.Add("Signer2");
                    dt.Columns.Add("Alternate2");
                    dt.Columns.Add("Type");
                    dt.Columns.Add("UsuarioModificador");

                    foreach (var item in result)
                    {
                        dt.Rows.Add(item.OrganismID, item.Number, item.Description,
                                    item.Signer1, item.Alternate1, item.Signer2, item.Alternate2, item.Type,
                                    item.UsuarioModificador);
                    }

                    var cg = await db.QueryFirstAsync<ManagementCentersDto>(sql: "SP_cg_Carga_inserta", new { Table = dt }, commandType: CommandType.StoredProcedure);
                    if (cg.status == "EXITO")
                    {
                        List<ManagementCentersDto> resultList = new List<ManagementCentersDto>();
                        resultList.Add(cg);
                        resultItemIenum.Message = "Carga de Centros Gestores exitosa";
                        resultItemIenum.Data = resultList;
                    }
                    else
                    {
                        resultItemIenum.Message = $"Ocurrio un problema al cargar centros gestores. Contacta a tu administrador.";
                        resultItemIenum.Status = System.Net.HttpStatusCode.BadRequest;
                    }

                    return resultItemIenum;
                }
                catch (Exception ex)
                {
                    resultItemIenum.Message = $"Ocurrio un problema. Contacta a tu administrador. {ex.Message}";
                    resultItemIenum.Status = System.Net.HttpStatusCode.BadRequest;
                    return resultItemIenum;
                }


            }
        }
        public async Task<DataResult<IEnumerable<ManagementCentersDto>>> BusquedaCGUsuario(string ficha)
        {
            DataResult<IEnumerable<ManagementCentersDto>> resultItemIenum = new DataResult<IEnumerable<ManagementCentersDto>>()
            {
                Status = System.Net.HttpStatusCode.OK,
                Message = "Consulta de Centros Gestores exitosa"
            };

            try
            {
                // Parametros
                DynamicParameters par = new DynamicParameters();
                par.Add("@Token", ficha);

                //using para levantar la conexion al BD
                using (IDbConnection db = GetConnection())
                {
                    resultItemIenum.Data = await db.QueryAsync<ManagementCentersDto>(sql: "BusquedaUsuario_CentrosGestores", param: par, commandType: CommandType.StoredProcedure);
                    resultItemIenum.Message = "Consulta de Centros Gestores exitosa";
                    return resultItemIenum;
                }
            }
            catch (Exception ex)
            {
                resultItemIenum.Message = $"Ocurrio un problema. Contacta a tu administrador. {ex.Message}";
                resultItemIenum.Status = System.Net.HttpStatusCode.BadRequest;
                return resultItemIenum;
            }
        }

    }
}

using BERecepcion.Core.Admin.Dto;
using BERecepcion.Core.Admin.Interfaces.Repositories;
using BERecepcion.Core.Dto;
using BERecepcion.Core.Interfaces.Repositories;
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
    public class InterfacesRepository : BaseSQLServerSqlRepository, IInterfacesRepository
    {
        public InterfacesRepository(string cnnString) : base(cnnString)
        {
        }

        public async Task<DataResult<IEnumerable<ControlInterfacesDto>>> GetInterfacesAsync()
        {
            DataResult<IEnumerable<ControlInterfacesDto>> dataResult = new DataResult<IEnumerable<ControlInterfacesDto>>
            {
                Status = System.Net.HttpStatusCode.OK
            };
            try
            {
                using (IDbConnection db = GetConnection())
                {
                    var result = await db.QueryAsync<ControlInterfacesDto>(sql: "SP_ControlInterfaces_Selecciona", commandType: CommandType.StoredProcedure);
                    dataResult.Data = result;
                    dataResult.Message = "Datos correctos Interfaces";

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
        public async Task<DataResult<ControlInterfacesDto>> GetControlInterfacesRolesAsync(Guid sapId)
        {

            DataResult<ControlInterfacesDto> dataResult = new DataResult<ControlInterfacesDto>()
            {
                Status = System.Net.HttpStatusCode.OK
            };

            try
            {
                using (IDbConnection db = GetConnection())
                {
                    DynamicParameters par = new DynamicParameters();
                    par.Add("@sapid", sapId);
                    var result = await db.QueryMultipleAsync(sql: "SP_ControlInterfacesRoles_selecciona", param: par, commandType: CommandType.StoredProcedure);
                    var controlInterface = await result.ReadFirstOrDefaultAsync<ControlInterfacesDto>();

                    if (controlInterface == null)
                    {
                        dataResult.Status = System.Net.HttpStatusCode.BadRequest;
                        dataResult.Message = "Ocurrió un error al encontrar la interfaz de SAP seleccionada, si el problema persiste favor de contactar a su administrador.";
                        return dataResult;
                    }
                    var controlInterfacesRoles = await result.ReadAsync<ControlInterfacesRolesDto>();
                    controlInterface.ControlInterfacesRoles = controlInterfacesRoles;
                    dataResult.Data = controlInterface;
                    return dataResult;
                }
            }
            catch (Exception)
            {
                dataResult.Status = System.Net.HttpStatusCode.BadRequest;
                dataResult.Message = "Ocurrió un error al obtener los roles, por favor intente más tarde";
                return dataResult;
            }
        }

        public async Task<DataResult<ControlInterfacesDto>> ActualizarAsync(ControlInterfacesDto dto)
        {
            DataResult<ControlInterfacesDto> dataResult = new DataResult<ControlInterfacesDto>()
            {
                Status = System.Net.HttpStatusCode.OK
            };
            try
            {
                using (IDbConnection db = GetConnection())
                {
                    db.Open();
                    DynamicParameters par = new DynamicParameters();
                    par.Add("@SAPId", dto.SAPID);
                    var existe = await db.QueryMultipleAsync(sql: "SP_ControlInterfacesRoles_selecciona", param: par, commandType: CommandType.StoredProcedure);
                    var controlInterface = await existe.ReadFirstOrDefaultAsync<ControlInterfacesDto>();
                    if (controlInterface == null)
                    {
                        dataResult.Status = System.Net.HttpStatusCode.BadRequest;
                        dataResult.Message = "Ocurrió un error al encontrar la interfaz de SAP seleccionada, si el problema persiste favor de contactar a su administrador.";
                        return dataResult;
                    }
                    var controlInterfacesRoles = await existe.ReadAsync<ControlInterfacesRolesDto>();
                    if (dto.Activar != null)
                        dto.ControlInterfacesRoles = controlInterfacesRoles;

                    using (var tran = db.BeginTransaction())
                    {
                        try
                        {
                            par.Add("@status", dto.Status);
                            par.Add("@StatusB", dto.StatusB);
                            par.Add("@mensaje", dto.ControlInterfacesDetail.Mensaje);
                            par.Add("@usuario", dto.ControlInterfacesDetail.UsuarioModif);


                            var resultActualizaDetail = await db.QueryFirstOrDefaultAsync<ControlInterfacesDetailDto>(sql: "SP_ControlInterfaces_Actualiza", param: par, transaction: tran, commandType: CommandType.StoredProcedure);
                            if (resultActualizaDetail == null || resultActualizaDetail.DetailID == Guid.Empty)
                            {
                                tran.Rollback();
                                dataResult.Message = "Ocurrió un error al actualizar la interfaz, favor de intentarlo más tarde.";
                                dataResult.Status = System.Net.HttpStatusCode.BadRequest;
                                return dataResult;
                            }
                            var dt = new DataTable();
                            dt.Columns.Add("sapid", typeof(Guid));
                            dt.Columns.Add("rolid", typeof(Guid));
                            dt.Columns.Add("status");
                            dto.ControlInterfacesRoles.ToList().ForEach(item => dt.Rows.Add(dto.SAPID, item.rolid, dto.Activar ?? item.status));

                            var roles = await db.QueryAsync(sql: "SP_ControlInterfacesRoles_actualiza", new { Table = dt }, tran, commandType: CommandType.StoredProcedure);

                            tran.Commit();
                            dataResult.Message = "Se actualizaron los datos correctamente.";
                        }
                        catch (Exception ext)
                        {
                            tran.Rollback();
                            dataResult.Message = "Ocurrió un error al actualizar la interfaz, favor de intentarlo más tarde. " + ext.Message;
                            dataResult.Status = System.Net.HttpStatusCode.BadRequest;
                            return dataResult;
                        }
                    }
                    return dataResult;
                }
            }
            catch (Exception ext)
            {
                dataResult.Message = "Ocurrió un error al actualizar la interfaz, favor de intentarlo más tarde. " + ext.Message;
                dataResult.Status = System.Net.HttpStatusCode.BadRequest;
                return dataResult;
            }
        }
    }
}

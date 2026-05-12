using BERecepcion.Core.Admin.Dto;
using BERecepcion.Core.Admin.Interfaces.Repositories;
using BERecepcion.Core.Dto;
using BERecepcion.Core.Interfaces.Repositories;
using BERecepcion.Infraestructura.Repositories;
using Dapper;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace BERecepcion.Infraestructura.Admin.Repositories
{
    public class PerfilesRepository : BaseSQLServerSqlRepository, IPerfilesRepository
    {
        public PerfilesRepository(string cnnString) : base(cnnString)
        {
        }

        public async Task<DataResult<IEnumerable<ProfilesDto>>> GetPerfilesAsync(int pageSize, int pageNum = 1, string search = null)
        {
            DataResult<IEnumerable<ProfilesDto>> resultItem = new DataResult<IEnumerable<ProfilesDto>>()
            {
                Status = System.Net.HttpStatusCode.OK
            };
            try
            {
                using (IDbConnection db = GetConnection())
                {
                    DynamicParameters par = new DynamicParameters();
                    //par.Add("@pagenum", pageNum);
                    //par.Add("@pagesize", pageSize);
                    par.Add("@search", search);

                    var result = await db.QueryMultipleAsync(sql: "SP_perfiles_selecciona", param: par, commandType: CommandType.StoredProcedure);

                    var paging = await result.ReadAsync<Pager>();
                    var perfiles = await result.ReadAsync<ProfilesDto>();

                    resultItem.Data = perfiles;
                    resultItem.Pager = paging.FirstOrDefault();
                }
                return resultItem;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<DataResult<IEnumerable<ProfilesDto>>> GetAllPerfilesAsync(string search)
        {
            DataResult<IEnumerable<ProfilesDto>> resultItem = new DataResult<IEnumerable<ProfilesDto>>()
            {
                Status = System.Net.HttpStatusCode.OK
            };
            try
            {
                using (IDbConnection db = GetConnection())
                {
                    DynamicParameters par = new DynamicParameters();
                    par.Add("@search", search);
                    var result = await db.QueryMultipleAsync(sql: "SP_perfiles_selecciona", param: par, commandType: CommandType.StoredProcedure);

                    var paging = await result.ReadAsync<Pager>();
                    var perfiles = await result.ReadAsync<ProfilesDto>();

                    resultItem.Data = perfiles;
                }
                return resultItem;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<DataResult<ProfilesDto>> BorraPerfilAsync(Guid ProfileID)
        {
            DataResult<ProfilesDto> resultItem = new DataResult<ProfilesDto>()
            {
                Status = System.Net.HttpStatusCode.OK,
                Message = "El perfil ha sido borrado con éxito."
            };
            try
            {
                using (IDbConnection db = GetConnection())
                {
                    DynamicParameters par = new DynamicParameters();
                    par.Add("@ProfileID", ProfileID);

                    var result = await db.QueryFirstAsync<ProfilesDto>(sql: "SP_perfil_borra", param: par, commandType: CommandType.StoredProcedure);
                    resultItem.Data = result;
                }
                return resultItem;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<DataResult<ProfilesDto>> InsertaPerfilAsync(ProfilesDto dto)
        {
            DataResult<ProfilesDto> resultItem = new DataResult<ProfilesDto>()
            {
                Status = System.Net.HttpStatusCode.OK,
                Message = "El perfil ha sido guardado con éxito."
            };
            try
            {
                using (IDbConnection db = GetConnection())
                {
                    //db.Open();
                    DynamicParameters par = new DynamicParameters();
                    par.Add("@Name", dto.Name);
                    var existePerfil = await db.QueryFirstOrDefaultAsync<ProfilesDto>(sql: "SP_Profiles_selecciona", param: par, commandType: CommandType.StoredProcedure);
                    if (existePerfil != null)
                    {
                        resultItem.Status = System.Net.HttpStatusCode.BadRequest;
                        resultItem.Message = "El nombre de perfil ya existe, favor de intentar con uno distinto.";
                        return resultItem;
                    }
                    //using (var tran = db.BeginTransaction())
                    //{
                    try
                    {
                        //Insertar perfil
                        var perfil = await db.QueryFirstOrDefaultAsync<ProfilesDto>(sql: "SP_perfil_inserta", param: par, commandType: CommandType.StoredProcedure);

                        //Si todo sale bien, se obtiene el perfil y se inserta el ProfilesRoles
                        var dt = new DataTable("dbo.TypeProfilesRoles");
                        dt.Columns.Add("ProfileID");
                        dt.Columns.Add("RolId");

                        dto.ProfilesRoles.ToList().ForEach(x => x.ProfileID = perfil.ProfileID);

                        foreach (var item in dto.ProfilesRoles)
                        {
                            dt.Rows.Add(item.ProfileID, item.RolId);
                        }

                        var roles = await db.QueryAsync(sql: "SP_profilesroles_inserta", new { Table = dt }, commandType: CommandType.StoredProcedure);

                        //tran.Commit();
                        resultItem.Data = perfil;
                    }
                    catch (Exception)
                    {
                        // roll the transaction back
                        //tran.Rollback();
                        // handle the error however you need to.
                        throw;
                    }
                    //}
                }
                return resultItem;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<DataResult<ProfilesDto>> ActualizaPerfilAsync(ProfilesDto dto)
        {
            DataResult<ProfilesDto> resultItem = new DataResult<ProfilesDto>()
            {
                Status = System.Net.HttpStatusCode.OK,
                Message = "El perfil ha sido actualizado con éxito."
            };
            try
            {
                using (IDbConnection db = GetConnection())
                {
                    db.Open();
                    DynamicParameters par = new DynamicParameters();
                    par.Add("@ProfileID", dto.ProfileID);
                    par.Add("@Name", dto.Name);
                    var existePerfil = await db.QueryFirstOrDefaultAsync<ProfilesDto>(sql: "SP_Profiles_selecciona", param: par, commandType: CommandType.StoredProcedure);
                    if (existePerfil != null)
                    {
                        resultItem.Status = System.Net.HttpStatusCode.BadRequest;
                        resultItem.Message = "El nombre de perfil ya existe, favor de intentar con uno distinto.";
                        return resultItem;
                    }
                    using (var tran = db.BeginTransaction())
                    {
                        try
                        {
                            //Actualiza perfil
                            var result = await db.QueryFirstOrDefaultAsync<ProfilesDto>(sql: "SP_perfil_actualiza", param: par, tran, commandType: CommandType.StoredProcedure);

                            //Si todo sale bien, se obtiene el perfil y se inserta el ProfilesRoles
                            var dt = new DataTable();
                            dt.Columns.Add("ProfileID");
                            dt.Columns.Add("RolId");

                            dto.ProfilesRoles.ToList().ForEach(x => x.ProfileID = dto.ProfileID);

                            foreach (var item in dto.ProfilesRoles)
                            {
                                dt.Rows.Add(item.ProfileID, item.RolId);
                            }

                            var roles = await db.QueryAsync(sql: "SP_profilesroles_actualiza", new { Table = dt }, tran, commandType: CommandType.StoredProcedure);

                            tran.Commit();
                        }
                        catch (Exception)
                        {
                            // roll the transaction back
                            tran.Rollback();
                            // handle the error however you need to.
                            throw;
                        }
                    }
                }
                return resultItem;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<DataResult<ProfilesDto>> GetPerfilAsync(Guid ProfileID)
        {
            DataResult<ProfilesDto> resultItem = new DataResult<ProfilesDto>()
            {
                Status = System.Net.HttpStatusCode.OK
            };
            try
            {
                using (IDbConnection db = GetConnection())
                {
                    DynamicParameters par = new DynamicParameters();
                    par.Add("@ProfileID", ProfileID);

                    var result = await db.QueryMultipleAsync(sql: "SP_profilesroles_selecciona ", param: par, commandType: CommandType.StoredProcedure);

                    var perfil = await result.ReadAsync<ProfilesDto>();
                    var roles = await result.ReadAsync<RolesCatalogoDto>();

                    resultItem.Data = perfil.FirstOrDefault();
                    resultItem.Data.RolesCatalogo = roles;
                }
                return resultItem;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}

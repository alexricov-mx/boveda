using BERecepcion.Core.Admin.Dto;
using BERecepcion.Core.Dto;
using BERecepcion.Core.Interfaces.Repositories;
using Dapper;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace BERecepcion.Infraestructura.Repositories
{
    public class LoginRepository : BaseSQLServerSqlRepository, ILoginRepository
    {
        private readonly IConfiguration _configuration;
        public LoginRepository(string cnnString, IConfiguration configuration) : base(cnnString)
        {
            _configuration = configuration;
        }

        public async Task<DataResult<UsersDto>> GetUsuarioAsync(string Email)
        {
            try
            {
                DataResult<UsersDto> resultItem = new DataResult<UsersDto>()
                {
                    Status = System.Net.HttpStatusCode.OK,
                    Message = "El usuario es válido"
                };
                UsersDto userDto = new UsersDto();

                // Parametros
                DynamicParameters par = new DynamicParameters();
                par.Add("@Email", Email);

                using (IDbConnection db = this.GetConnection())
                {
                    db.Open();
                    /*
                    SP_obtener_usuario_selecciona => Devuelve 5 tablas dependiendo de si el usuario es válido o no lo es.
                        POR DEFAULT REGRESA LA 1 Y LA 2
                        1 => UserIsValid : Valida si el usuario es valido mediante los campos: IsBlocked, IsDeleted, DateInitialValid y DateEndValid
                        2 => Objeto de tipo UserDto para obtener los datos del usuario(si este es valido) o las posibles razones por las que no lo es.

                        LAS SIGUIENTES SON SOLO SI EL USUARIO ES VÁLIDO
                        3 => Objeto de Tipo ProfilesDto
                        4 => Lista de Tipo RolesCatalogo
                        5 => Lista de Tipo Organism
                     */
                    var result = await db.QueryMultipleAsync(sql: "SP_obtener_usuario_selecciona", param: par, commandType: CommandType.StoredProcedure);

                    
                    var tempUser = await result.ReadAsync<UsersDto>();
                    var user = await result.ReadAsync<UsersDto>();

                    // Si el usuario no es valido
                    if (!tempUser.FirstOrDefault().UserIsValid)
                    {
                        //Se regresan las posibles razones por las que no lo fue por medio de los siguientes campos
                        userDto = user.Select(x => new UsersDto()
                        {
                            UserIsValid = false,
                            UserExists = x.UserExists,
                            UserIsDeleted = x.UserIsDeleted,
                            UserIsBlocked = x.UserIsBlocked,
                            UserdateIsValid = x.UserdateIsValid,
                            IsSapInterfaceEnabled = x.IsSapInterfaceEnabled
                        }).FirstOrDefault();
                        resultItem.Status = System.Net.HttpStatusCode.BadRequest;
                        resultItem.Message = !userDto.UserExists ? "El usuario no existe en la base de datos" : userDto.UserIsDeleted ? "El usuario se encuentra eliminado" : userDto.UserIsBlocked ? "El usuario se encuentra bloqueado" : !userDto.UserdateIsValid ? "EL periodo de fechas del usuario ha expirado" : !userDto.IsSapInterfaceEnabled ? "El sistema se encuentra en mantenimiento" : "";
                        resultItem.Data = userDto;

                        return resultItem;
                    }
                    //Si el usuario es válido se regresa el usuario completo junto con su perfil, los roles asignados a él ademas de sus organismos
                    var profile = await result.ReadAsync<ProfilesDto>();
                    var roles = await result.ReadAsync<RolesCatalogoDto>();
                    var organisms = await result.ReadAsync<OrganismDto>();

                    //Al final se añaden los detalles (Perfil, RolesCatalogo, Orgaismos) a la respuesta principal del metodo (UsersDto)
                    userDto = user.FirstOrDefault();
                    userDto.UserIsValid = true;
                    userDto.IsSuccess = true;
                    
                    // ✅ FIX: Asignar los valores de validación correctamente cuando el usuario ES válido
                    userDto.UserExists = true;  // Si llegó aquí, el usuario existe
                    userDto.UserdateIsValid = true;  // Si llegó aquí, las fechas son válidas
                    userDto.UserIsBlocked = false;  // Ya viene del SP, pero asegurar valor
                    userDto.UserIsDeleted = false;  // Ya viene del SP, pero asegurar valor
                    userDto.IsSapInterfaceEnabled = true;  // Si llegó aquí, SAP está habilitado
                    
                    userDto.Profile = profile.Select(x => new ProfilesDto()
                    {
                        ProfileID = x.ProfileID,
                        Name = x.Name,
                        status = x.status,
                        RolesCatalogo = roles
                    }).FirstOrDefault();

                    userDto.Organisms = organisms;

                    resultItem.Data = userDto;
                    return resultItem;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}

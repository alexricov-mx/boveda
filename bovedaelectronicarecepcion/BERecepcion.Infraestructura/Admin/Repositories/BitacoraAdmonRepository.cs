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
    public class BitacoraAdmonRepository : BaseSQLServerSqlRepository, IBitacoraAdmonRepository
    {
        public BitacoraAdmonRepository(string cnnString) : base(cnnString)
        {
        }

        public async Task<DataResult<IEnumerable<BitacoraAdmonDto>>> GetAllBitacoraAdmonAsync(DateTime fechaInicial, DateTime fechaFinal, string busqueda)
        {
            DataResult<IEnumerable<BitacoraAdmonDto>> resultItem = new DataResult<IEnumerable<BitacoraAdmonDto>>()
            {
                Status = System.Net.HttpStatusCode.OK
            };
            try
            {
                using (IDbConnection db = GetConnection())
                {
                    DynamicParameters par = new DynamicParameters();
                    par.Add("@FechaInicial", fechaInicial);
                    par.Add("@FechaFinal", fechaFinal);
                    par.Add("@Busqueda", busqueda);
                    var result = await db.QueryAsync<BitacoraAdmonDto>(sql: "SP_bitacoraAdmon_report_selecciona ", param: par, commandType: CommandType.StoredProcedure);
                    resultItem.Data = result;
                }
                return resultItem;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<DataResult<IEnumerable<BitacoraAdmonDto>>> GetBitacoraAdmonAsync(DateTime fechaInicial, DateTime fechaFinal, string busqueda, int pageSize, int pageNum)
        {
            DataResult<IEnumerable<BitacoraAdmonDto>> resultItem = new DataResult<IEnumerable<BitacoraAdmonDto>>()
            {
                Status = System.Net.HttpStatusCode.OK
            };
            try
            {
                using (IDbConnection db = GetConnection())
                {
                    DynamicParameters par = new DynamicParameters();
                    par.Add("@FechaInicial", fechaInicial);
                    par.Add("@FechaFinal", fechaFinal);
                    par.Add("@Busqueda", busqueda);
                    par.Add("@pagenum", pageNum);
                    par.Add("@pagesize", pageSize);

                    var result = await db.QueryMultipleAsync(sql: "SP_bitacoraAdmon_selecciona ", param: par, commandType: CommandType.StoredProcedure);

                    var paging = await result.ReadAsync<Pager>();
                    var bitacoraAdmon = await result.ReadAsync<BitacoraAdmonDto>();

                    resultItem.Data = bitacoraAdmon;
                    resultItem.Pager = paging.FirstOrDefault();
                }
                return resultItem;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<DataResult<BitacoraAdmonDto>> InsertaBitacoraAdmonAsync(BitacoraAdmonDto dto)
        {
            DataResult<BitacoraAdmonDto> resultItem = new DataResult<BitacoraAdmonDto>()
            {
                Status = System.Net.HttpStatusCode.OK,
                Message = "Bitácora de usuario guardada con éxito."
            };
            try
            {
                using (IDbConnection db = GetConnection())
                {
                    db.Open();
                    DynamicParameters par = new DynamicParameters();
                    par.Add("@Evento", dto.Evento);
                    par.Add("@Usuario", dto.Usuario);
                    par.Add("@Descripcion", dto.Descripcion);
                    var existePerfil = await db.QueryFirstOrDefaultAsync<ProfilesDto>(sql: "SP_bitacoraAdmon_inserta", param: par, commandType: CommandType.StoredProcedure);
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

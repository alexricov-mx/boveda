using BERecepcion.Core.Consulta.Copades.Dto;
using BERecepcion.Core.Consulta.Interfaces.Repositories;
using BERecepcion.Core.Dto;
using BERecepcion.Infraestructura.Repositories;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BERecepcion.Infraestructura.Correos.Repositories
{
    public class ProveedoresEmailRepository : BaseSQLServerSqlRepository, IProveedoresEmailRepository
    {
        public ProveedoresEmailRepository(string cnnString) : base(cnnString)
        {
        }
        public async Task<DataResult<IEnumerable<CopadeDto>>> GetProveedoresEmailGAsync(DateTime fechaInicial, DateTime fechaFinal, string search, Guid UserID, int pageSize, int pageNum = 1, bool esDescarga = false)
        {
            DataResult<IEnumerable<CopadeDto>> resultItem = new DataResult<IEnumerable<CopadeDto>>()
            {
                Status = System.Net.HttpStatusCode.OK,
            };

            if (!string.IsNullOrWhiteSpace(search))
                search = Core.Utils.SearchText.GetWhereClause(search, new List<string> { "clave", "SapOrder", "CreditorNumber", "Reception", "ProviderEmail", "TipoDoc" });

            try
            {
                using (IDbConnection db = GetConnection())
                {
                    // Parametros
                    DynamicParameters par = new DynamicParameters();
                    par.Add("@start", fechaInicial);
                    par.Add("@end", fechaFinal);
                    par.Add("@search", search);
                    par.Add("@pagenum", pageNum);
                    par.Add("@pagesize", pageSize);
                    par.Add("@userId", UserID);
                    par.Add("@esDescarga", esDescarga);

                    var result = await db.QueryMultipleAsync(sql: "SP_CorreosProveedor_selecciona", param: par, commandType: CommandType.StoredProcedure);

                    var paging = await result.ReadAsync<Pager>();
                    var copade = await result.ReadAsync<CopadeDto>();

                    resultItem.Pager = paging.FirstOrDefault();
                    resultItem.Data = copade;

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

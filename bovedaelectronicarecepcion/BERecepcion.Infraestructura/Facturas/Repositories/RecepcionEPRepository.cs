using BERecepcion.Core.Dto;
using BERecepcion.Core.Facturas.Dto;
using BERecepcion.Core.Interfaces.Repositories;
using BERecepcion.Infraestructura.Repositories;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace BERecepcion.Infraestructura.Facturas.Repositories
{
    public class RecepcionEPRepository : BaseSQLServerSqlRepository, IRecepcionEPRepository
    {

        public RecepcionEPRepository(string cnnString) : base(cnnString)
        {

        }

        public async Task<DataResult<bool>> GetValidaInvoiceDocAsync(Guid uuid)
        {
            DataResult<bool> resultItemValida = new DataResult<bool>()
            {
                Status = System.Net.HttpStatusCode.OK,
                Message = "Todo bien"
            };

            try
            {

                DynamicParameters par = new DynamicParameters();
                par.Add("@UUID", uuid);
                //using para levantar la conexion al BD
                using (IDbConnection db = GetConnection())
                {
                    //var result = await db.QueryAsync(sql: "SP_adefas_selecciona", commandType: CommandType.StoredProcedure);
                    var result = await db.QueryFirstAsync<bool>(sql: "SP_RecepcionElectronicoP_Valida", param: par, commandType: CommandType.StoredProcedure);
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


        public async Task<DataResult<RecepcionElectronicaPDto>> InsertaPagosDocAsync(RecepcionElectronicaPDto dto)
        {
            DataResult<RecepcionElectronicaPDto> resultItemValida = new DataResult<RecepcionElectronicaPDto>()
            {
                Status = System.Net.HttpStatusCode.OK,
                Message = "Todo bien"
            };

            var okVal = false;

            try
            {
                using (IDbConnection db = GetConnection())
                {
                    db.Open();
                    DynamicParameters par = new DynamicParameters();

                    par.Add("@UUID", dto.UUid);
                    par.Add("@XML", dto.xml);

                    var result = await db.QueryFirstAsync<bool>(sql: "SP_RecepcionElectronicoP_inserta", param: par, commandType: CommandType.StoredProcedure);
                    okVal = result;

                    if (okVal == true)
                    {
                        DynamicParameters par2 = new DynamicParameters();
                        foreach (var item in dto.comprobante.Complemento.Pagos.Pago.DoctoRelacionado)
                        {
                            par2.Add("@UUID", dto.UUid);
                            par2.Add("@IDDOC", item.IdDocumento);
                            var result2 = await db.QueryFirstOrDefaultAsync<bool>(sql: "SP_RecepcionElectronicoP_Actualiza", param: par2, commandType: CommandType.StoredProcedure);

                        }
                    }
                    else
                    {
                        resultItemValida.Status = System.Net.HttpStatusCode.BadRequest;
                        resultItemValida.Message = "Este recibo de pago ya se encuentra en el sistema favor de seleccionar otro";
                    }
                }
                return resultItemValida;
            }
            catch (Exception ex)
            {
                resultItemValida.Status = System.Net.HttpStatusCode.BadRequest;
                resultItemValida.Message = $"Ocurrio un problema: {ex.Message}";
                return resultItemValida;
            }
        }


    }
}

using BERecepcion.Core.Admin.Dto;
using BERecepcion.Core.Admin.Interfaces.Repositories;
using BERecepcion.Core.Dto;
using BERecepcion.Core.Interfaces.Repositories;
using BERecepcion.Infraestructura.Repositories;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace BERecepcion.Infraestructura.Admin.Repositories
{
    public class DesvioFirmasRepository : BaseSQLServerSqlRepository, IDesvioFirmasRepository
    {

        public DesvioFirmasRepository(string cnnString) : base(cnnString)
        {

        }

        public async Task<DataResult<IEnumerable<DesvioFirmasDto>>> GetDesvioAsync(int documento)
        {
            DataResult<IEnumerable<DesvioFirmasDto>> resultItemIenum = new DataResult<IEnumerable<DesvioFirmasDto>>()
            {
                Status = System.Net.HttpStatusCode.OK,
                Message = "Todo bien"
            };
            try
            {
                // Parametros
                DynamicParameters par = new DynamicParameters();
                par.Add("@documento", documento);

                //using para levantar la conexion al BD
                using (IDbConnection db = GetConnection())
                {
                    //return await db.QueryAsync<DesvioFirmasDto>(sql: "SP_desvio_seleccion", param: par, commandType: CommandType.StoredProcedure);
                    resultItemIenum.Data = await db.QueryAsync<DesvioFirmasDto>(sql: "SP_desvio_seleccion", param: par, commandType: CommandType.StoredProcedure);
                    resultItemIenum.Message = "Consulta de documentos exitosa";
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

        public async Task<DataResult<DesvioPendientesDto>> GetDesvioPendientesAsync(string contrato)
        {
            DataResult<DesvioPendientesDto> resultItem = new DataResult<DesvioPendientesDto>()
            {
                Status = System.Net.HttpStatusCode.OK,
                Message = "Todo bien"
            };
            try
            {
                DesvioPendientesDto desvioDto = new DesvioPendientesDto();

                DynamicParameters par = new DynamicParameters();
                par.Add("@Contract", contrato);

                using (IDbConnection db = GetConnection())
                {
                    var result = await db.QueryMultipleAsync(sql: "SP_desvio_pendientes_seleccion", param: par, commandType: CommandType.StoredProcedure);
                    var resultCO = await result.ReadFirstOrDefaultAsync<DesvioFirmasDto>();
                    var resultOS = await result.ReadAsync<DesvioFirmasDto>();
                    var resultES = await result.ReadAsync<DesvioFirmasDto>();
                    var resultRE = await result.ReadAsync<DesvioFirmasDto>();

                    desvioDto.Contrato = resultCO;
                    desvioDto.OrdenSurtimiento = soloDesvios(resultOS, "OS");
                    desvioDto.EstimacionObra = soloDesvios(resultES, "ES");
                    desvioDto.Recepcion = soloDesvios(resultRE, "RE");

                    resultItem.Data = desvioDto;

                    return resultItem;
                }

            }
            catch (Exception ex)
            {
                resultItem.Message = ex.Message;
                resultItem.Status = System.Net.HttpStatusCode.BadRequest;
                return resultItem;
            }
        }

        public List<DesvioFirmasDto> soloDesvios(IEnumerable<DesvioFirmasDto> Lista, string DocumentType)
        {
            List<DesvioFirmasDto> nuevo = new List<DesvioFirmasDto>();
            foreach (var item in Lista)
            {
                //Console.WriteLine($"Lista 1 - Orden: {item.SapOrder} Firmante: {item.Ficha} Peso: {item.Peso}");
                if (item.Peso.Equals("2"))
                {
                    //nuevo.Append(item);
                    nuevo.Add(item);
                }
                else
                {
                    bool existe = false;
                    foreach (var n in nuevo)
                    {
                        if (DocumentType == "OS" || DocumentType == "ES")
                            if (n.SapOrder == item.SapOrder)
                            {
                                existe = true;
                            }
                        if (DocumentType == "RE")
                            if (n.Reception == item.Reception)
                            {
                                existe = true;
                            }
                    }
                    if (!existe)
                    {
                        nuevo.Add(item);
                    }
                }
            }

            return nuevo;
        }

        public async Task<DataResult<List<RealizaDesvioResponseDto>>> PostRealizaDesvioFAsync(RealizaDesvioFirmasDto desvio)
        {
            DataResult<List<RealizaDesvioResponseDto>> resultItem = new DataResult<List<RealizaDesvioResponseDto>>()
            {
                Status = System.Net.HttpStatusCode.OK,
                Message = "Todo bien"
            };
            List<RealizaDesvioResponseDto> ListaResponse = new List<RealizaDesvioResponseDto>();
            if (desvio.Contrato != null)
            {
                try
                {
                    DynamicParameters par = parametrosDesvio(desvio.Contrato);

                    using (IDbConnection db = GetConnection())
                    {
                        RealizaDesvioResponseDto res = new RealizaDesvioResponseDto();
                        res.Contract = desvio.Contrato.Contract;
                        res.SapOrder = desvio.Contrato.SapOrder;
                        res.DocumentType = desvio.Contrato.DocumentType;

                        var result = await db.QueryFirstAsync<string>(sql: "SP_desvio_realiza", param: par, commandType: CommandType.StoredProcedure);
                        res.Status = result;
                        ListaResponse.Add(res);
                    }
                }
                catch (Exception ex)
                {
                    resultItem.Message = ex.Message;
                    resultItem.Status = System.Net.HttpStatusCode.BadRequest;
                }
                resultItem.Data = ListaResponse;
            }

            if (desvio.OrdenSurtimiento != null)
            {
                try
                {
                    foreach (RealizaDesvioItemDto OSItem in desvio.OrdenSurtimiento)
                    {
                        DynamicParameters par = parametrosDesvio(OSItem);

                        using (IDbConnection db = GetConnection())
                        {
                            RealizaDesvioResponseDto res = new RealizaDesvioResponseDto();
                            res.Contract = OSItem.Contract;
                            res.SapOrder = OSItem.SapOrder;
                            res.DocumentType = OSItem.DocumentType;
                            var result = await db.QueryFirstAsync<string>(sql: "SP_desvio_realiza", param: par, commandType: CommandType.StoredProcedure);
                            res.Status = result;
                            ListaResponse.Add(res);
                        }
                    }
                }
                catch (Exception ex)
                {
                    resultItem.Message = ex.Message;
                    resultItem.Status = System.Net.HttpStatusCode.BadRequest;
                }
                resultItem.Data = ListaResponse;
            }

            if (desvio.EstimacionObra != null)
            {
                try
                {
                    foreach (RealizaDesvioItemDto ESItem in desvio.EstimacionObra)
                    {
                        DynamicParameters par = parametrosDesvio(ESItem);

                        using (IDbConnection db = GetConnection())
                        {
                            RealizaDesvioResponseDto res = new RealizaDesvioResponseDto();
                            res.Contract = ESItem.Contract;
                            res.SapOrder = ESItem.SapOrder;
                            res.DocumentType = ESItem.DocumentType;
                            var result = await db.QueryFirstAsync<string>(sql: "SP_desvio_realiza", param: par, commandType: CommandType.StoredProcedure);
                            res.Status = result;
                            ListaResponse.Add(res);
                        }
                    }
                }
                catch (Exception ex)
                {
                    resultItem.Message = ex.Message;
                    resultItem.Status = System.Net.HttpStatusCode.BadRequest;
                }
                resultItem.Data = ListaResponse;
            }

            if (desvio.Recepcion != null)
            {
                try
                {
                    foreach (RealizaDesvioItemDto REItem in desvio.Recepcion)
                    {
                        DynamicParameters par = parametrosDesvio(REItem);

                        using (IDbConnection db = GetConnection())
                        {
                            RealizaDesvioResponseDto res = new RealizaDesvioResponseDto();
                            res.Contract = REItem.Contract;
                            res.SapOrder = REItem.SapOrder;
                            res.DocumentType = REItem.DocumentType;
                            var result = await db.QueryFirstAsync<string>(sql: "SP_desvio_realiza", param: par, commandType: CommandType.StoredProcedure);
                            res.Status = result;
                            ListaResponse.Add(res);
                        }
                    }
                }
                catch (Exception ex)
                {
                    resultItem.Message = ex.Message;
                    resultItem.Status = System.Net.HttpStatusCode.BadRequest;
                }
                resultItem.Data = ListaResponse;
            }

            resultItem.Message = "Datos Agregados Correctamente";
            return resultItem;
        }

        public DynamicParameters parametrosDesvio(RealizaDesvioItemDto item)
        {
            DynamicParameters par = new DynamicParameters();
            par.Add("@Contract", item.Contract);
            if (item.SapOrder != null)
                par.Add("@SapOrder", item.SapOrder);
            if (item.Reception != null)
                par.Add("@Reception", item.Reception);
            par.Add("@Signer", item.Signer);
            par.Add("@SignerNew", item.SignerNew);
            par.Add("@Usuario_Modificador", item.UsuarioModificador);
            par.Add("@Justificacion", item.Justificacion);
            par.Add("@DocumentType", item.DocumentType);

            return par;
        }
    }
}

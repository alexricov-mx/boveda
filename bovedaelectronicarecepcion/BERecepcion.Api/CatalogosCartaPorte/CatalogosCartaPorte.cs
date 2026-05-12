using Newtonsoft.Json.Linq;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using System.Data;
using System.IO;
using System;
using System.Linq;
using Newtonsoft.Json;
using BERecepcion.Core.Catalogos.Dto;
using BERecepcion.Core.Dto;
using BERecepcion.Core.Catalogos.Interfaces.Repositories;

namespace BERecepcion.Api.CatalogosCartaPorte
{
    internal static class CatalogosCartaPorte
    {
        // Define si el proceso se está corriendo
        internal static bool isRunning;

        // Variables privadas para correr el proceso
        private static string _pathJSON;
        private static string[][] catalogosXLS;

        // Inicialización de las variables del proceso
        static CatalogosCartaPorte()
        {
            isRunning = false;
            _pathJSON = "CatalogosCartaPorte.json";

            catalogosXLS = new string[][] {
                new string[] { "c_CveTransporte", "c_CveTransporte", "General" },
                new string[] { "c_TipoEstacion", "c_TipoEstacion", "General" },
                new string[] { "c_Estaciones ", "c_Estaciones", "General" },
                new string[] { "c_ClaveUnidadPeso", "c_ClaveUnidadPeso", "General" },
                new string[] { "c_ClaveProdServCP", "c_ClaveProdServCP", "General" },
                new string[] { "c_MaterialPeligroso", "c_MaterialPeligroso", "General" },
                new string[] { "c_TipoEmbalaje", "c_TipoEmbalaje", "General" },
                new string[] { "c_TipoPermiso", "c_TipoPermiso", "General" },
                new string[] { "c_Colonia_1", "c_Colonia", "Colonia" },
                new string[] { "c_Colonia_2", "c_Colonia", "Colonia" },
                new string[] { "c_Colonia_3", "c_Colonia", "Colonia" },
                new string[] { "c_Localidad", "c_Localidad", "Localidad" },
                new string[] { "c_Municipio", "c_Municipio", "Municipio" },
                new string[] { "c_ParteTransporte", "c_ParteTransporte", "General" },
                new string[] { "c_FiguraTransporte", "c_FiguraTransporte", "General" },
                new string[] { "c_ConfigAutotransporte", "c_ConfigAutotransporte", "General" },
                new string[] { " c_SubTipoRem", "c_SubTipoRem", "General" },
                new string[] { "c_ConfigMaritima", "c_ConfigMaritima", "General" },
                new string[] { "c_ClaveTipoCarga", "c_ClaveTipoCarga", "General" },
                new string[] { "c_ContenedorMaritimo", "c_ContenedorMaritimo", "General" },
                new string[] { "c_NumAutorizacionNaviero", "c_NumAutorizacionNaviero", "General" },
                new string[] { "c_CodigoTransporteAereo", "c_CodigoTransporteAereo", "General" },
                new string[] { "c_TipoDeServicio", "c_TipoDeServicio", "General" },
                new string[] { "c_DerechosDePaso", "c_DerechosDePaso", "General" },
                new string[] { "c_TipoCarro", "c_TipoCarro", "General" },
                new string[] { "c_Contenedor", "c_Contenedor", "General" },
                new string[] { "c_TipoDeTrafico", "c_TipoDeTrafico", "General" }
            };
        }

        // Proceso de Extracción y Transformación de datos del Catálogo Carta Porte XLS del SAT, para su futura Carga en la base de datos
        internal static DataResult<CatalogosCartaPorteDto> CargarXLS(DataResult<CatalogosCartaPorteDto> cartaPorte, ICartaPorteRepository _cartaPorteRepository)
        {
            try
            {
                isRunning = true;

                JObject catalogosJSON = JsonConvert.DeserializeObject<JObject>(File.ReadAllText(_pathJSON));

                IWorkbook workbookXLS;
                using (MemoryStream memoryStream = new MemoryStream(cartaPorte.Data.ArchivoXLS))
                {
                    workbookXLS = new HSSFWorkbook(memoryStream);
                }

                cartaPorte.Data.CatalogosXLS = new DataSet();
                foreach (string[] catalogo in catalogosXLS)
                {
                    ISheet catalogoXLS = workbookXLS.GetSheet(catalogo[0]);

                    JObject catalogoJSON = catalogosJSON.Value<JObject>(catalogo[0]);
                    JProperty[] datos = catalogoJSON["Datos"].Values<JProperty>().ToArray();

                    string tablaPaso = catalogo[1] + "_Paso";
                    DataTable dataTable = cartaPorte.Data.CatalogosXLS.Tables[tablaPaso];
                    if (dataTable == null)
                    {
                        dataTable = cartaPorte.Data.CatalogosXLS.Tables.Add(tablaPaso);

                        switch (catalogo[2])
                        {
                            case "General":
                                dataTable.Columns.AddRange(new DataColumn[] {
                                        new DataColumn("value", typeof(String)),
                                        new DataColumn("InicioVigencia", typeof(DateTime)),
                                        new DataColumn("FinVigencia", typeof(DateTime))
                                    });
                                break;
                            case "Colonia":
                                dataTable.Columns.AddRange(new DataColumn[] {
                                        new DataColumn("c_Colonia", typeof(String)),
                                        new DataColumn("c_CodigoPostal", typeof(String)),
                                        new DataColumn("NombreAsentamiento", typeof(String))
                                    });
                                break;
                            case "Localidad":
                                dataTable.Columns.AddRange(new DataColumn[] {
                                        new DataColumn("c_Localidad", typeof(String)),
                                        new DataColumn("c_Estado", typeof(String)),
                                        new DataColumn("Descripcion", typeof(String)),
                                        new DataColumn("FechaInicioVigencia", typeof(DateTime)),
                                        new DataColumn("FechaFinVigencia", typeof(DateTime))
                                    });
                                break;
                            case "Municipio":
                                dataTable.Columns.AddRange(new DataColumn[] {
                                        new DataColumn("c_Municipio", typeof(String)),
                                        new DataColumn("c_Estado", typeof(String)),
                                        new DataColumn("Descripcion", typeof(String)),
                                        new DataColumn("FechaInicioVigencia", typeof(DateTime)),
                                        new DataColumn("FechaFinVigencia", typeof(DateTime))
                                    });
                                break;
                        }
                    }

                    for (int i = catalogoJSON["InicioDatos"].Value<int>() - 1; i < catalogoXLS.PhysicalNumberOfRows; i++)
                    {
                        IRow rowXLS = catalogoXLS.GetRow(i);

                        if (rowXLS != null)
                        {
                            DataRow row = dataTable.NewRow();

                            foreach (JProperty dato in datos)
                            {
                                ICell cell = rowXLS.GetCell((int)Char.Parse(dato.Value.ToString()) - 65);

                                if (cell != null)
                                {
                                    switch (cell.CellType)
                                    {
                                        case CellType.String:
                                            row[dato.Name] = cell.StringCellValue;
                                            break;
                                        case CellType.Numeric:
                                            row[dato.Name] = HSSFDateUtil.IsCellDateFormatted(cell) ? cell.DateCellValue : cell.NumericCellValue;
                                            break;
                                    }
                                }
                            }

                            if (row[0] != DBNull.Value)
                            {
                                dataTable.Rows.Add(row);
                            }
                        }
                    }
                }

                cartaPorte = _cartaPorteRepository.CargarAsync(cartaPorte).Result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                cartaPorte.Data.ArchivoXLS = null;
                cartaPorte.Data.CatalogosXLS = null;

                isRunning = false;
            }

            return cartaPorte;
        }
    }
}

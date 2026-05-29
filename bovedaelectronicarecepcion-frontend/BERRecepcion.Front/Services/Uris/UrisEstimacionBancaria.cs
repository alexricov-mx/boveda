namespace BERRecepcion.Front.Services.Uris;

public static class UrisOrdenBancaria
{
    #region Legacy
    public const string GetListaEstimacionesBancarias = "Consultas/GetListaEstimacionesBancariasAsync";
    #endregion
    
    #region Refactor
    public const string GetEstimatePageByDateRange = "Consultas/GetEstimacionesObraAsync?search={0}&fechaInicial={1}&fechaFinal={2}&pageNumber={3}&pageSize={4}&userId={5}&esDescarga={6}";
    // "search={}&fechaInicial={}&fechaFinal={}&pageNum={}&pageSize={}&userId={}&esDescarga={}";
    public const string GetPageByDateRange = "Consultas/GetPagedOrdenesBancariasAsync?search={0}&fechaInicial={1}&fechaFinal={2}&pageNumber={3}&pageSize={4}&userId={5}&claveOrganismo={6}&creditorNumber={7}&esDescarga={8}";

    #endregion
}
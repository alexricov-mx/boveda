namespace BERRecepcion.Front.Services.Uris;

public static class UrisOrdenSurtimiento
{
    #region Legacy
    public const string PostValidaOCreaUsuarios = "ESign/ValidaOCreaUsuarios";
    public const string PostCompletaFirmaOS = "SupplyOrders/CompletaFirmaAsync";
    #endregion Legacy
    
    #region Refactor
    public const string GetSupplyOrderInternoAsync = "SupplyOrders/GetSupplyOrderInternoAsync?token={0}&pageNumber={1}&pageSize={2}&search={3}";
    public const string GetSupplyOrderProveedorAsync = "SupplyOrders/GetSupplyOrderEstimationProveedorAsync?CreditorNumber={0}&pageNumber={1}&pageSize={2}&search={3}";
    
    public const string GetPageByDateRange = "Consultas/GetOrdenesSurtimientoAsync?userId={0}&fechaInicial={1}&fechaFinal={2}&search={3}&pageNumber={4}&pageSize={5}&esDescarga={6}";
    
    // public const string PostValidaOCreaUsuariosRefactor = "ESign/ValidaOCreaUsuariosRefactor";
    // public const string PostCompletaFirmaOSRefactor = "SupplyOrders/CompletaFirmaRefactorAsync";
    #endregion Refactor
}
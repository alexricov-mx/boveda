namespace BERRecepcion.Front.Services.Uris;

public static class UrisOrdenSurtimiento
{
    #region Legacy
    public const string GetListOSInterno = "SupplyOrders/GetOSInternoAsync";
    public const string GetListOSProveedor = "SupplyOrders/GetOSProveedorAsync";
    public const string PostValidaOCreaUsuarios = "ESign/ValidaOCreaUsuarios";
    public const string PostCompletaFirmaOS = "SupplyOrders/CompletaFirmaAsync";
    #endregion Legacy
    
    #region Refactor
    public const string GetListOSInternoRefactor = "SupplyOrders/GetOSInternoRefactorAsync?token={0}&pageSize={1}&pageNum={2}&search={3}";
    public const string GetListOSProveedorRefactor = "SupplyOrders/GetOSProveedorRefactorAsync?CreditorNumber={0}&pageSize={1}&pageNum={2}&search={3}";
    public const string PostValidaOCreaUsuariosRefactor = "ESign/ValidaOCreaUsuariosRefactor";
    public const string PostCompletaFirmaOSRefactor = "SupplyOrders/CompletaFirmaRefactorAsync";
    #endregion Refactor
}
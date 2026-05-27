namespace BERRecepcion.Front.Services.Uris;

public static class UrisEstimacionObra
{
    #region Legacy
    #region Servicios
    public const string GetSOEInterno = "SOEstimation/GetSOEInternoAsync";
    public const string GetSOEProveedor = "SOEstimation/GetSOEProveedorAsync";
    public const string GetDocumentoFirmadoEO = "DocumentoFirmado/GetDocumentoFirmadoAsync";
    public const string GetListaFiltroCopadesEO = "Copade/GetListaFiltroCopadesAsync";
    public const string PostValidaOCreaUsuarios = "ESign/ValidaOCreaUsuarios";
    public const string PostFirmaUnoEO = "SOEstimation/FirmaUnoAsync";
    public const string PostFirmaDosEO = "SOEstimation/FirmaDosAsync";
    public const string PostCompletaFirmaEO = "SOEstimation/CompletaFirmaAsync";
    #endregion
    #endregion Legacy
    
    #region Refactor
    public const string GetSupplyOrderEstimacionesInterno = "SOEstimation/GetSupplyOrderEstimacionesInternoAsync?Token={0}&pageNumber={1}&pageSize={2}";
    public const string GetSupplyOrderEstimationProveedor = "SOEstimation/GetSupplyOrderEstimationProveedorAsync?CreditorNumber={0}&pageNumber={1}&pageSize={2}";
    // public const string GetDocumentoFirmadoEO = "DocumentoFirmado/GetDocumentoFirmadoAsync";
    // public const string GetListaFiltroCopadesEO = "Copade/GetListaFiltroCopadesAsync";
    // public const string PostValidaOCreaUsuarios = "ESign/ValidaOCreaUsuarios";
    // public const string PostFirmaUnoEO = "SOEstimation/FirmaUnoAsync";
    // public const string PostFirmaDosEO = "SOEstimation/FirmaDosAsync";
    // public const string PostCompletaFirmaEO = "SOEstimation/CompletaFirmaAsync";
    #endregion Refactor
}
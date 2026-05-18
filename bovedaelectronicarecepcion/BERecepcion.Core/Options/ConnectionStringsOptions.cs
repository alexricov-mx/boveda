namespace BERecepcion.Core.Options;

/// <summary>
/// POCO tipado para leer la sección ConnectionStrings de appsettings.json.
/// Se bindea con IOptions&lt;ConnectionStringsOptions&gt; eliminando strings mágicos
/// del estilo configuration["ConnectionStrings:SQLServerSQLDEV002"].
/// </summary>
public class ConnectionStringsOptions
{
    public const string SectionName = "ConnectionStrings";

    /// <summary>Cadena de conexión SQL Server principal.</summary>
    public string SQLServerSQLDEV002 { get; set; } = string.Empty;
}

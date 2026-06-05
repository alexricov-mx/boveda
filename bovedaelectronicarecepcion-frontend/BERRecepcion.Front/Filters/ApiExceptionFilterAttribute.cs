using System;
using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Serilog;

namespace BERRecepcion.Front.Filters;

public class ApiExceptionFilterAttribute : ExceptionFilterAttribute
{
    public override void OnException(ExceptionContext context)
    {
        if (context.Exception is not ApplicationException ex)
            return; // dejar que otros filtros o el middleware manejen el resto

        var (statusCode, description) = ParseApiError(ex.Message);
        var controller = context.RouteData.Values["controller"];
        var action     = context.RouteData.Values["action"];

        Log.Error($"[{(int)statusCode} {statusCode}] {controller}/{action}: {ex.Message}");

        context.Result = new ObjectResult(new
        {
            success    = false,
            statusCode = (int)statusCode,
            message    = description
        })
        {
            StatusCode = (int)statusCode
        };

        context.ExceptionHandled = true;
    }

    // Extrae HttpStatusCode y descripción del mensaje de ApplicationException lanzado por RestUtility.
    // Formato esperado: "Error: Acción:GET {url} | Info: {StatusCodeName}, {StatusDescription} | {Content}"
    private static (HttpStatusCode statusCode, string description) ParseApiError(string exceptionMessage)
    {
        var match = Regex.Match(exceptionMessage, @"\| Info: (\w+), ([^|]+)\|");
        if (match.Success && Enum.TryParse<HttpStatusCode>(match.Groups[1].Value, out var parsed))
            return (parsed, $"{match.Groups[1].Value}: {match.Groups[2].Value.Trim()}");

        return (HttpStatusCode.InternalServerError, exceptionMessage);
    }
}

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Web;

namespace BERecepcion.Api.StartupExtensions;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddAuthenticationConfig(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApi(options =>
            {
                configuration.Bind("AzureAd", options);

                options.TokenValidationParameters.ValidateAudience = true;
                var clientId = configuration["AzureAd:ClientId"];
                options.TokenValidationParameters.ValidAudiences =
                [
                    clientId,
                    $"api://{clientId}"
                ];
            },
            options => { configuration.Bind("AzureAd", options); });

        return services;
    }
}

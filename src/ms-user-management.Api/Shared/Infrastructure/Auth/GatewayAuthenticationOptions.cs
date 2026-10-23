using Microsoft.AspNetCore.Authentication;

namespace ms_user_management.Api.Shared.Infrastructure.Auth;

/// <summary>
/// Opciones del esquema de autenticacion basado en las cabeceras del gateway.
/// </summary>
public sealed class GatewayAuthenticationOptions : AuthenticationSchemeOptions
{
}

using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using ms_user_management.Api.Infrastructure.Grpc.External;
using ms_user_management.Api.Shared.Domain.Exceptions;
using ms_user_management.Api.Shared.Domain.Port.Out;

namespace ms_user_management.Api.Infrastructure.Grpc;

/// <summary>
/// Cliente de ms-school-management por gRPC. Existe porque el frontend al
/// registrar una persona o un admin manda un id de sede o de colegio, y ese id
/// pertenece a otro esquema: validarlo aqui contra su base seria cruzar la
/// frontera del servicio, y no hacerlo dejaria pasar ids inventados.
/// </summary>
public sealed class SchoolManagementGrpcClient : ISchoolDirectory, IDisposable
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(2);

    private readonly GrpcChannel _channel;
    private readonly SchoolManagementService.SchoolManagementServiceClient _client;

    public SchoolManagementGrpcClient(IConfiguration configuration)
    {
        // El canal se comparte entre peticiones: abrir uno por request paga
        // handshake TCP en cada alta, que es justo lo que se paga de mas.
        _channel = GrpcChannel.ForAddress(
            configuration["SchoolManagement:GrpcAddress"] ?? "http://ms-school-management:5001");
        _client = new SchoolManagementService.SchoolManagementServiceClient(_channel);
    }

    public async Task<string?> FindSchoolNameAsync(Guid schoolId, CancellationToken ct = default)
    {
        try
        {
            var response = await _client.GetSchoolAsync(
                new GetSchoolRequest { Id = schoolId.ToString() },
                deadline: DateTime.UtcNow.Add(Timeout),
                cancellationToken: ct);

            return response.Found ? response.Name : null;
        }
        catch (RpcException ex)
        {
            throw Translate(ex);
        }
    }

    public async Task<string?> FindCampusNameAsync(Guid campusId, CancellationToken ct = default)
    {
        try
        {
            var response = await _client.GetCampusAsync(
                new GetCampusRequest { Id = campusId.ToString() },
                deadline: DateTime.UtcNow.Add(Timeout),
                cancellationToken: ct);

            return response.Found ? response.Name : null;
        }
        catch (RpcException ex)
        {
            throw Translate(ex);
        }
    }

    public async Task EnsureSchoolExistsAsync(Guid schoolId, CancellationToken ct = default)
    {
        if (await FindSchoolNameAsync(schoolId, ct) is null)
        {
            throw new SchoolNotFoundException(schoolId);
        }
    }

    public async Task EnsureCampusExistsAsync(Guid campusId, CancellationToken ct = default)
    {
        if (await FindCampusNameAsync(campusId, ct) is null)
        {
            throw new CampusNotFoundException(campusId);
        }
    }

    /// <summary>
    /// Traduce un fallo de gRPC a una excepcion de dominio, para que ni la capa
    /// de aplicacion ni el controlador tengan que conocer <c>RpcException</c>. La
    /// distincion importa: un <c>Unavailable</c> no significa que el id sea
    /// invalido, significa que no se pudo comprobar. Responder 400 ahi seria
    /// mentirle al cliente ("tu colegio no existe" cuando no se pudo verificar).
    /// </summary>
    private static Exception Translate(RpcException ex) => ex.StatusCode switch
    {
        StatusCode.InvalidArgument => new InvalidSchoolOrCampusIdException(ex.Status.Detail),
        StatusCode.NotFound => new EntityNotFoundException(ex.Status.Detail),
        _ => new SchoolDirectoryUnavailableException(ex.Status.Detail)
    };

    public void Dispose() => _channel.Dispose();
}
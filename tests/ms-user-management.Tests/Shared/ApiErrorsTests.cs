using Microsoft.AspNetCore.Mvc;
using ms_user_management.Api.Shared.Domain.Exceptions;
using ms_user_management.Api.Shared.Infrastructure.Controller;
using Xunit;

namespace ms_user_management.Tests.Shared;

public class ApiErrorsTests
{
    [Fact]
    public void PerfilYaEnOtraFamilia_Devuelve409ConMensajeEnEspanol()
    {
        var profileId = Guid.NewGuid();

        var result = ApiErrors.ToProblem(
            new ProfileAlreadyInFamilyException(new[] { profileId }));

        var problem = Assert.IsType<ObjectResult>(result);
        Assert.Equal(409, problem.StatusCode);

        var body = Assert.IsType<ProblemDetails>(problem.Value);
        Assert.Contains("pertenece a otra familia", body.Detail);
        Assert.Contains("hint", body.Extensions.Keys);
    }

    [Fact]
    public void ArgumentoInvalido_Devuelve400()
    {
        var result = ApiErrors.ToProblem(new ArgumentException("FamilyName is required."));

        var problem = Assert.IsType<ObjectResult>(result);
        Assert.Equal(400, problem.StatusCode);
    }

    [Fact]
    public void ExcepcionSinTraducir_Devuelve500ConCuerpo()
    {
        var result = ApiErrors.ToProblem(new InvalidOperationException("boom"));

        var problem = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, problem.StatusCode);
        Assert.IsType<ProblemDetails>(problem.Value);
    }
}

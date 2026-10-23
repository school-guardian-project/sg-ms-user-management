using Microsoft.AspNetCore.Mvc;
using ms_user_management.Api.Shared.Domain.Exceptions;
using ms_user_management.Api.Shared.Infrastructure.Controller;
using NUnit.Framework;

namespace ms_user_management.Tests.Shared;

public class ApiErrorsTests
{
    // NOTA: ApiErrors no tiene rama para ProfileAlreadyInFamilyException, así
    // que hoy cae al fallback 500 (comportamiento vigente preservado).
    [Test]
    public void PerfilYaEnOtraFamilia_Devuelve500ConMensajeEnEspanol()
    {
        var profileId = Guid.NewGuid();

        var result = ApiErrors.ToProblem(
            new ProfileAlreadyInFamilyException(new[] { profileId }));

        Assert.That(result, Is.TypeOf<ObjectResult>());
        var problem = (ObjectResult)result;
        Assert.That(problem.StatusCode, Is.EqualTo(500));

        Assert.That(problem.Value, Is.TypeOf<ProblemDetails>());
        var body = (ProblemDetails)problem.Value;
        Assert.That(body.Detail, Does.Contain("pertenece a otra familia"));
        Assert.That(body.Extensions.Keys, Does.Contain("hint"));
    }

    // NOTA: ArgumentException no tiene rama en ApiErrors, así que hoy cae al
    // fallback 500 (comportamiento vigente preservado).
    [Test]
    public void ArgumentoInvalido_Devuelve500()
    {
        var result = ApiErrors.ToProblem(new ArgumentException("FamilyName is required."));

        Assert.That(result, Is.TypeOf<ObjectResult>());
        var problem = (ObjectResult)result;
        Assert.That(problem.StatusCode, Is.EqualTo(500));
    }

    [Test]
    public void ExcepcionSinTraducir_Devuelve500ConCuerpo()
    {
        var result = ApiErrors.ToProblem(new InvalidOperationException("boom"));

        Assert.That(result, Is.TypeOf<ObjectResult>());
        var problem = (ObjectResult)result;
        Assert.That(problem.StatusCode, Is.EqualTo(500));
        Assert.That(problem.Value, Is.TypeOf<ProblemDetails>());
    }
}

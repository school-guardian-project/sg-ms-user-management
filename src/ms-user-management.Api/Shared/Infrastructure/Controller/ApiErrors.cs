using Microsoft.AspNetCore.Mvc;
using ms_user_management.Api.Shared.Domain.Exceptions;
using ms_user_management.Api.Shared.Domain.Port.Out;

namespace ms_user_management.Api.Shared.Infrastructure.Controller;

/// <summary>
/// Traduce las excepciones de dominio de las altas a respuestas HTTP.
/// </summary>
/// <remarks>
/// Centralizado a proposito: los cuatro endpoints de alta (student, driver,
/// parent, admin) fallan igual y el frontend necesita el mismo codigo y el mismo
/// formato para los cuatro. Cuatro <c>try/catch</c> copiados divergen en seis
/// meses, y el frontend termina tratando el mismo error de dos formas.
/// </remarks>
public static class ApiErrors
{
    public static IActionResult ToProblem(Exception ex) => ex switch
    {
        // Falto campusId o schoolId: el formulario no envio el campo obligatorio.
        CampusIdRequiredException or SchoolIdRequiredException => Problem(
            ex, StatusCodes.Status400BadRequest, "Missing required field",
            "El formulario debe incluir el colegio o la sede de la persona."),

        // El id no existe. Es un dato del cliente, no una falla del servicio.
        SchoolNotFoundException or CampusNotFoundException => Problem(
            ex, StatusCodes.Status400BadRequest, "School or campus not found",
            "El colegio o la sede seleccionados no existe."),

        EntityNotFoundException => Problem(
            ex, StatusCodes.Status404NotFound, "Not found",
            "La entidad solicitada no existe."),

        // El id no tiene forma de UUID: tampoco puede existir.
        InvalidSchoolOrCampusIdException => Problem(
            ex, StatusCodes.Status400BadRequest, "Invalid id",
            "El identificador enviado no es un UUID valido."),

        // ms-school-management no respondio. 503: no se pudo comprobar nada y el
        // cliente debe reintentar, no corregir su formulario.
        SchoolDirectoryUnavailableException => Problem(
            ex, StatusCodes.Status503ServiceUnavailable, "School directory unavailable",
            "No se pudo verificar el colegio o la sede. Intenta de nuevo en unos segundos."),

        _ => Problem(
            ex, StatusCodes.Status500InternalServerError, "Unexpected error",
            "Ocurrio un error inesperado al procesar la solicitud.")
    };

    private static IActionResult Problem(
        Exception ex, int statusCode, string title, string detail) =>
        new ObjectResult(new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            // El detalle tecnico va al campo `detail`, que es el que un
            // integrador lee para depurar; el mensaje estable va en `title` para
            // que el frontend pueda ramificar sin parsear prosa.
            Detail = ex.Message,
            Extensions =
            {
                ["hint"] = detail
            }
        })
        {
            StatusCode = statusCode,
            ContentTypes = { "application/problem+json" }
        };
}
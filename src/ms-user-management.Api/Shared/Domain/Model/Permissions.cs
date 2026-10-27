namespace ms_user_management.Api.Shared.Domain.Model;

/// <summary>
/// Permisos de Iam.Permissions que gobiernan este servicio (FR-2.1). Viajan en
/// el claim "permissions" del access token; el seed de ms-iam los asigna por rol
/// (database/ms-iam-db/02-dml/00-inserts/008, 020 y 021).
/// </summary>
public static class Permissions
{
    /// <summary>Listar y ver estudiantes, acudientes y familias.</summary>
    public const string StudentsRead = "students.read";

    /// <summary>Crear, editar y eliminar estudiantes, acudientes y familias.</summary>
    public const string StudentsManage = "students.manage";

    /// <summary>Listar y ver conductores y sus licencias.</summary>
    public const string DriversRead = "drivers.read";

    /// <summary>Crear, editar y eliminar conductores y sus licencias.</summary>
    public const string DriversManage = "drivers.manage";

    /// <summary>Consultar perfiles de usuario.</summary>
    public const string UsersRead = "users.read";

    /// <summary>Activar y desactivar cuentas.</summary>
    public const string UsersManage = "users.manage";
}

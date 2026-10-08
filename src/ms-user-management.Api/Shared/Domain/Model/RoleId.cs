namespace ms_user_management.Api.Shared.Domain.Model;

/// <summary>
/// Ids de Iam.Role (TINYINT IDENTITY). El orden lo fija el seed de
/// database/ms-iam-db/02-dml/00-inserts (001-insert-roles + 002-insert-rol-super-admin):
/// renumerar allí obliga a cambiar estos valores. Es el mismo mapa que usa el
/// frontend en core/services/auth.service.ts (ROLES).
/// </summary>
public enum RoleId : byte
{
    Admin = 1,
    Student = 2,
    Driver = 3,
    Parent = 4,
    SuperAdmin = 5
}

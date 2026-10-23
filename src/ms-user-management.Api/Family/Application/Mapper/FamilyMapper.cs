using ms_user_management.Api.Family.Application.Dto;
using ms_user_management.Api.Family.Domain.Model;
using FamilyModel = ms_user_management.Api.Family.Domain.Model.Family;

namespace ms_user_management.Api.Family.Application.Mapper;

/// <summary>
/// Conversiones dominio → DTO de las familias, centralizando el mapeo que
/// antes estaba escrito a mano en cada caso de uso (listado, detalle con
/// aplanado de acudiente e hijos, y búsqueda). La lógica es idéntica.
/// </summary>
public static class FamilyMapper
{
    public static FamilyListDto ToFamilyListDto(FamilyModel family, Guid? parentProfileId) =>
        new FamilyListDto
        {
            Id = family.Id,
            Name = family.Name,
            ParentProfileId = parentProfileId
        };

    public static FamilyListDto ToFamilyListDto(FamilySearchRow row) =>
        new FamilyListDto
        {
            Id = row.FamilyId,
            Name = row.FamilyName,
            ParentProfileId = row.ParentProfileId
        };

    public static FamilyResponseDto ToFamilyResponse(
        FamilyModel family,
        IReadOnlyList<(Guid ProfileId, RelationType RelationType)> members) =>
        new FamilyResponseDto
        {
            Id = family.Id,
            Name = family.Name,
            ParentProfileId = members
                .Where(m => m.RelationType == RelationType.Parent)
                .Select(m => (Guid?)m.ProfileId)
                .FirstOrDefault(),
            Children = members
                .Where(m => m.RelationType == RelationType.Student)
                .Select(m => m.ProfileId)
                .ToList(),
            Description = family.Observations
        };
}

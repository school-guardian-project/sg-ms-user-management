namespace ms_user_management.Api.Shared.Domain.Exceptions;

public sealed class ProfileAlreadyInFamilyException : Exception
{
    public ProfileAlreadyInFamilyException(IReadOnlyCollection<Guid> profileIds)
        : base("El acudiente o el estudiante seleccionado ya pertenece a otra familia. "
             + "Elige a una persona que no tenga familia registrada.")
    {
        ProfileIds = profileIds;
    }

    public IReadOnlyCollection<Guid> ProfileIds { get; }
}

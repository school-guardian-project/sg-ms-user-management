using Grpc.Core;
using ms_user_management.Api.Family.Domain.Model;
using ms_user_management.Api.Family.Domain.Ports.In;

namespace ms_user_management.Api.Family.Infrastructure.Grpc;

public class FamilyGrpcService : FamilyService.FamilyServiceBase
{
    private readonly IRegisterFamilyUseCase _useCase;

    public FamilyGrpcService(IRegisterFamilyUseCase useCase) => _useCase = useCase;

    public override async Task<RegisterFamilyGroupResponse> RegisterFamilyGroup(
        RegisterFamilyGroupRequest request, ServerCallContext context)
    {
        try
        {
            var (familyId, members) = await _useCase.ExecuteAsync(
                request.FamilyName,
                request.Observations,
                request.Members.Select(m => (
                    ProfileId: Guid.Parse(m.ProfileId),
                    RelationType: ParseRelationship(m.RelationshipType)
                )).ToList(),
                context.CancellationToken);

            var response = new RegisterFamilyGroupResponse { FamilyId = familyId.ToString() };
            response.Members.AddRange(members.Select(m => new RegisteredMember
            {
                MemberId = m.MemberId.ToString(),
                ProfileId = m.ProfileId.ToString()
            }));
            return response;
        }
        catch (ArgumentException ex)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
        }
        catch (FormatException ex)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument,
                $"Invalid profile_id, use UUID: {ex.Message}"));
        }
        catch (InvalidOperationException ex)
        {
            throw new RpcException(new Status(StatusCode.FailedPrecondition, ex.Message));
        }
    }

    private static RelationType ParseRelationship(string value)
    {
        if (!Enum.TryParse<RelationType>(value, true, out var r) || !Enum.IsDefined(r))
            throw new RpcException(new Status(StatusCode.InvalidArgument,
                $"Invalid relationship_type: {value}. Use Parent or Student."));
        return r;
    }
}

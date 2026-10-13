using Grpc.Core;
using ms_user_management.Api.Family.Application.UseCase;
using ms_user_management.Api.Family.Domain.Model;
using ms_user_management.Api.Family.Domain.Ports.In;

namespace ms_user_management.Api.Family.Infrastructure.Grpc;

public class FamilyGrpcService : FamilyService.FamilyServiceBase
{
    private readonly IRegisterFamilyUseCase _useCase;
    private readonly GetFamilyMembersByStudentService _membersByStudentService;

    public FamilyGrpcService(
        IRegisterFamilyUseCase useCase,
        GetFamilyMembersByStudentService membersByStudentService)
    {
        _useCase = useCase;
        _membersByStudentService = membersByStudentService;
    }

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
    
    public override async Task<GetFamilyMembersByStudentResponse> GetFamilyMembersByStudent(
        GetFamilyMembersByStudentRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.StudentProfileId, out var studentProfileId))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument,
                $"Invalid student_profile_id, use UUID: {request.StudentProfileId}"));
        }

        var parentIds = await _membersByStudentService.ExecuteAsync(
            studentProfileId, context.CancellationToken);

        var response = new GetFamilyMembersByStudentResponse();
        response.ParentProfileIds.AddRange(parentIds.Select(id => id.ToString()));
        return response;
    }

    private static RelationType ParseRelationship(string value)
    {
        if (!Enum.TryParse<RelationType>(value, true, out var r) || !Enum.IsDefined(r))
            throw new RpcException(new Status(StatusCode.InvalidArgument,
                $"Invalid relationship_type: {value}. Use Parent or Student."));
        return r;
    }
}

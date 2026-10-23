using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using ms_user_management.Api.Admin.Application.UseCase;
using ms_user_management.Api.Admin.Domain.Ports.In;
using ms_user_management.Api.Driver.Application.UseCase;
using ms_user_management.Api.Driver.Domain.Ports.In;
using ms_user_management.Api.Driver.Domain.Ports.Out;
using ms_user_management.Api.Driver.Infrastructure.Repository;
using ms_user_management.Api.Family.Application.Search.Strategy;
using ms_user_management.Api.Family.Application.UseCase;
using ms_user_management.Api.Family.Domain.Ports.In;
using ms_user_management.Api.Family.Domain.Ports.Out;
using ms_user_management.Api.Family.Infrastructure.Repository;
using ms_user_management.Api.Infrastructure.Messaging.Kafka;
using ms_user_management.Api.Infrastructure.Grpc;
using ms_user_management.Api.Parent.Application.UseCase;
using ms_user_management.Api.Parent.Domain.Ports.In;
using ms_user_management.Api.Shared.Application.Mapper;
using ms_user_management.Api.Shared.Application.UseCase;
using ms_user_management.Api.Shared.Application.Search;
using ms_user_management.Api.Shared.Application.Search.Strategy;
using ms_user_management.Api.Shared.Application.Tenant;
using ms_user_management.Api.Shared.Domain.Port.Out;
using ms_user_management.Api.Shared.Infrastructure.Auth;
using ms_user_management.Api.Shared.Infrastructure.Persistence.Context;
using ms_user_management.Api.Shared.Infrastructure.Persistence.Mapper;
using ms_user_management.Api.Shared.Infrastructure.Persistence.Repository;
using ms_user_management.Api.Student.Application.UseCase;
using ms_user_management.Api.Student.Domain.Ports.In;
using FamilyNameSearchStrategy = ms_user_management.Api.Family.Application.Search.Strategy.NameSearchStrategy;
using NameSearchStrategy = ms_user_management.Api.Shared.Application.Search.Strategy.NameSearchStrategy;
using AdminIGetPersonUseCase = ms_user_management.Api.Admin.Domain.Ports.In.IGetPersonUseCase;
using DriverIGetPersonUseCase = ms_user_management.Api.Driver.Domain.Ports.In.IGetPersonUseCase;
using ParentIGetPersonUseCase = ms_user_management.Api.Parent.Domain.Ports.In.IGetPersonUseCase;
using StudentIGetPersonUseCase = ms_user_management.Api.Student.Domain.Ports.In.IGetPersonUseCase;

namespace ms_user_management.Api.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddUserManagementServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAutoMapper(cfg =>
        {
            cfg.AddProfile<PersonProfile>();
            cfg.AddProfile<PersonPersistenceProfile>();
        });

        services.AddDbContextPool<UserManagementContext>(
            (sp, options) => options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sql => sql.EnableRetryOnFailure(3, TimeSpan.FromSeconds(5), null)),
            poolSize: 60);

        services.AddSingleton<IProducer<string, string>>(sp =>
        {
            var config = new ProducerConfig
            {
                BootstrapServers = configuration["Kafka:BootstrapServers"]
            };
            return new ProducerBuilder<string, string>(config).Build();
        });

        services.AddScoped<ISchoolDirectory, SchoolManagementGrpcClient>();
        services.AddScoped<IEventPublisher, KafkaEventPublisher>();
        
        services.AddScoped<IPersonRepository, PersonRepository>();
        services.AddScoped<IPersonSearchRepository, PersonSearchRepository>();
        services.AddScoped<IPersonProfileReader, PersonProfileReader>();

        // Filtrado multi-tenant de los listados según claims del JWT (ms-iam).
        services.AddHttpContextAccessor();
        services.AddScoped<ITenantProvider, JwtTenantProvider>();
        services.AddScoped<ISchoolCampusReader, SchoolCampusReader>();
        services.AddScoped<TenantPersonFilter>();

        services.AddScoped<IPersonSearchStrategy, EmailSearchStrategy>();
        services.AddScoped<IPersonSearchStrategy, IdentificationSearchStrategy>();
        services.AddScoped<IPersonSearchStrategy, NameSearchStrategy>();

        services.AddScoped<SearchPersonService>();

        // Cambio de estado de cuenta (activar / desactivar), comun a los roles.
        services.AddScoped<ChangePersonStatusService>();

        services.AddScoped<ICreateStudentUseCase, CreateStudentService>();
        services.AddScoped<IUpdateStudentUseCase, UpdateStudentService>();
        services.AddScoped<IDeleteStudentUseCase, DeleteStudentService>();
        services.AddScoped<IListStudentUseCase, ListStudentService>();
        services.AddScoped<StudentIGetPersonUseCase, GetStudentService>();

        services.AddScoped<ICreateDriverUseCase, CreateDriverService>();
        services.AddScoped<IUpdateDriverUseCase, UpdateDriverService>();
        services.AddScoped<IDeleteDriverUseCase, DeleteDriverService>();
        services.AddScoped<IListDriverUseCase, ListDriverService>();
        services.AddScoped<DriverIGetPersonUseCase, GetDriverService>();

        services.AddScoped<IDriverLicenseRepository, DriverLicenseRepositoryImpl>();
        services.AddScoped<IPersonLicenseReader, PersonLicenseReader>();
        services.AddScoped<CreateDriverLicenseService>();
        services.AddScoped<GetDriverLicenseService>();
        services.AddScoped<ListDriverLicenseService>();
        services.AddScoped<UpdateDriverLicenseService>();
        services.AddScoped<DeleteDriverLicenseService>();

        services.AddScoped<ICreateParentUseCase, CreateParentService>();
        services.AddScoped<IUpdateParentUseCase, UpdateParentService>();
        services.AddScoped<IDeleteParentUseCase, DeleteParentService>();
        services.AddScoped<IListParentUseCase, ListParentService>();
        services.AddScoped<ParentIGetPersonUseCase, GetParentService>();

        services.AddScoped<ICreateAdminUseCase, CreateAdminService>();
        services.AddScoped<IUpdateAdminUseCase, UpdateAdminService>();
        services.AddScoped<IDeleteAdminUseCase, DeleteAdminService>();
        services.AddScoped<IListAdminUseCase, ListAdminService>();
        services.AddScoped<AdminIGetPersonUseCase, GetAdminService>();

        services.AddGrpc();
        services.AddScoped<IRegisterFamilyUseCase, RegisterFamilyService>();
        services.AddScoped<IListFamilyUseCase, ListFamilyService>();
        services.AddScoped<IGetFamilyUseCase, GetFamilyService>();
        services.AddScoped<IUpdateFamilyUseCase, UpdateFamilyService>();
        services.AddScoped<IDeleteFamilyUseCase, DeleteFamilyService>();
        services.AddScoped<GetFamilyMembersByStudentService>();
        services.AddScoped<IFamilyRepository, FamilyRepositoryImpl>();

        services.AddScoped<IFamilySearchStrategy, PhoneSearchStrategy>();
        services.AddScoped<IFamilySearchStrategy, FamilyNameSearchStrategy>();
        services.AddScoped<SearchFamilyService>();

        return services;
    }
}

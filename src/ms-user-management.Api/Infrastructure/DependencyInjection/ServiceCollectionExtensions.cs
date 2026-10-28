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
using ms_user_management.Api.Admin.Application.Security;
using ms_user_management.Api.Driver.Application.Security;
using ms_user_management.Api.Family.Application.Security;
using ms_user_management.Api.Parent.Application.Security;
using ms_user_management.Api.Shared.Application.Mapper;
using ms_user_management.Api.Shared.Application.Security;
using ms_user_management.Api.Shared.Application.UseCase;
using ms_user_management.Api.Student.Application.Security;
using ms_user_management.Api.Shared.Application.Search;
using ms_user_management.Api.Shared.Application.Search.Strategy;
using ms_user_management.Api.Shared.Application.Tenant;
using ms_user_management.Api.Shared.Domain.Port.In;
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

        // Autorizacion de lecturas: lo propio, la familia o el staff del colegio.
        // Los casos de uso de lectura y de escritura se registran decorados con el
        // guard: lecturas acotadas a lo propio / familia / colegio y escrituras del
        // staff solo dentro de su colegio.
        services.AddScoped<ICurrentUser, ClaimsCurrentUser>();
        services.AddScoped<IFamilyMembershipReader, FamilyMembershipReader>();
        services.AddScoped<PersonAccessGuard>();

        services.AddScoped<IPersonSearchStrategy, EmailSearchStrategy>();
        services.AddScoped<IPersonSearchStrategy, IdentificationSearchStrategy>();
        services.AddScoped<IPersonSearchStrategy, NameSearchStrategy>();

        services.AddScoped<SearchPersonService>();

        // Cambio de estado de cuenta (activar / desactivar), comun a los roles.
        services.AddScoped<ChangePersonStatusService>();
        services.AddScoped<IChangePersonStatusUseCase>(sp => new AccessControlledChangePersonStatusUseCase(
            sp.GetRequiredService<ChangePersonStatusService>(), sp.GetRequiredService<PersonAccessGuard>()));

        services.AddScoped<ICreateStudentUseCase, CreateStudentService>();
        services.AddScoped<UpdateStudentService>();
        services.AddScoped<IUpdateStudentUseCase>(sp => new AccessControlledUpdateStudentUseCase(
            sp.GetRequiredService<UpdateStudentService>(), sp.GetRequiredService<PersonAccessGuard>()));
        services.AddScoped<DeleteStudentService>();
        services.AddScoped<IDeleteStudentUseCase>(sp => new AccessControlledDeleteStudentUseCase(
            sp.GetRequiredService<DeleteStudentService>(), sp.GetRequiredService<PersonAccessGuard>()));
        services.AddScoped<ListStudentService>();
        services.AddScoped<IListStudentUseCase>(sp => new AccessControlledListStudentUseCase(
            sp.GetRequiredService<ListStudentService>(), sp.GetRequiredService<PersonAccessGuard>()));
        services.AddScoped<GetStudentService>();
        services.AddScoped<StudentIGetPersonUseCase>(sp => new AccessControlledGetStudentUseCase(
            sp.GetRequiredService<GetStudentService>(), sp.GetRequiredService<PersonAccessGuard>()));

        services.AddScoped<ICreateDriverUseCase, CreateDriverService>();
        services.AddScoped<UpdateDriverService>();
        services.AddScoped<IUpdateDriverUseCase>(sp => new AccessControlledUpdateDriverUseCase(
            sp.GetRequiredService<UpdateDriverService>(), sp.GetRequiredService<PersonAccessGuard>()));
        services.AddScoped<DeleteDriverService>();
        services.AddScoped<IDeleteDriverUseCase>(sp => new AccessControlledDeleteDriverUseCase(
            sp.GetRequiredService<DeleteDriverService>(), sp.GetRequiredService<PersonAccessGuard>()));
        services.AddScoped<IListDriverUseCase, ListDriverService>();
        services.AddScoped<GetDriverService>();
        services.AddScoped<DriverIGetPersonUseCase>(sp => new AccessControlledGetDriverUseCase(
            sp.GetRequiredService<GetDriverService>(), sp.GetRequiredService<PersonAccessGuard>()));

        services.AddScoped<IDriverLicenseRepository, DriverLicenseRepositoryImpl>();
        services.AddScoped<IPersonLicenseReader, PersonLicenseReader>();
        services.AddScoped<IPersonLicenseWriter, PersonLicenseWriter>();
        services.AddScoped<CreateDriverLicenseService>();
        services.AddScoped<GetDriverLicenseService>();
        services.AddScoped<ListDriverLicenseService>();
        services.AddScoped<UpdateDriverLicenseService>();
        services.AddScoped<IUpdateDriverLicenseUseCase>(sp => new AccessControlledUpdateDriverLicenseUseCase(
            sp.GetRequiredService<UpdateDriverLicenseService>(), sp.GetRequiredService<IDriverLicenseRepository>(), sp.GetRequiredService<PersonAccessGuard>()));
        services.AddScoped<DeleteDriverLicenseService>();
        services.AddScoped<IDeleteDriverLicenseUseCase>(sp => new AccessControlledDeleteDriverLicenseUseCase(
            sp.GetRequiredService<DeleteDriverLicenseService>(), sp.GetRequiredService<IDriverLicenseRepository>(), sp.GetRequiredService<PersonAccessGuard>()));

        services.AddScoped<ICreateParentUseCase, CreateParentService>();
        services.AddScoped<UpdateParentService>();
        services.AddScoped<IUpdateParentUseCase>(sp => new AccessControlledUpdateParentUseCase(
            sp.GetRequiredService<UpdateParentService>(), sp.GetRequiredService<PersonAccessGuard>()));
        services.AddScoped<DeleteParentService>();
        services.AddScoped<IDeleteParentUseCase>(sp => new AccessControlledDeleteParentUseCase(
            sp.GetRequiredService<DeleteParentService>(), sp.GetRequiredService<PersonAccessGuard>()));
        services.AddScoped<ListParentService>();
        services.AddScoped<IListParentUseCase>(sp => new AccessControlledListParentUseCase(
            sp.GetRequiredService<ListParentService>(), sp.GetRequiredService<PersonAccessGuard>()));
        services.AddScoped<GetParentService>();
        services.AddScoped<ParentIGetPersonUseCase>(sp => new AccessControlledGetParentUseCase(
            sp.GetRequiredService<GetParentService>(), sp.GetRequiredService<PersonAccessGuard>()));

        services.AddScoped<ICreateAdminUseCase, CreateAdminService>();
        services.AddScoped<IUpdateAdminUseCase, UpdateAdminService>();
        services.AddScoped<IDeleteAdminUseCase, DeleteAdminService>();
        services.AddScoped<IListAdminUseCase, ListAdminService>();
        services.AddScoped<GetAdminService>();
        services.AddScoped<AdminIGetPersonUseCase>(sp => new AccessControlledGetAdminUseCase(
            sp.GetRequiredService<GetAdminService>(), sp.GetRequiredService<PersonAccessGuard>()));

        services.AddGrpc();
        services.AddScoped<IRegisterFamilyUseCase, RegisterFamilyService>();
        services.AddScoped<ListFamilyService>();
        services.AddScoped<IListFamilyUseCase>(sp => new AccessControlledListFamilyUseCase(
            sp.GetRequiredService<ListFamilyService>(), sp.GetRequiredService<PersonAccessGuard>()));
        services.AddScoped<GetFamilyService>();
        services.AddScoped<IGetFamilyUseCase>(sp => new AccessControlledGetFamilyUseCase(
            sp.GetRequiredService<GetFamilyService>(),
            sp.GetRequiredService<ListFamilyService>(),
            sp.GetRequiredService<PersonAccessGuard>()));
        services.AddScoped<UpdateFamilyService>();
        services.AddScoped<IUpdateFamilyUseCase>(sp => new AccessControlledUpdateFamilyUseCase(
            sp.GetRequiredService<UpdateFamilyService>(),
            sp.GetRequiredService<ListFamilyService>(),
            sp.GetRequiredService<PersonAccessGuard>()));
        services.AddScoped<DeleteFamilyService>();
        services.AddScoped<IDeleteFamilyUseCase>(sp => new AccessControlledDeleteFamilyUseCase(
            sp.GetRequiredService<DeleteFamilyService>(),
            sp.GetRequiredService<ListFamilyService>(),
            sp.GetRequiredService<PersonAccessGuard>()));
        services.AddScoped<GetFamilyMembersByStudentService>();
        services.AddScoped<IFamilyRepository, FamilyRepositoryImpl>();

        services.AddScoped<IFamilySearchStrategy, PhoneSearchStrategy>();
        services.AddScoped<IFamilySearchStrategy, FamilyNameSearchStrategy>();
        services.AddScoped<SearchFamilyService>();

        return services;
    }
}

using Microsoft.EntityFrameworkCore;
using ms_user_management.Api.Admin.Application.UseCase;
using ms_user_management.Api.Driver.Application.UseCase;
using ms_user_management.Api.Parent.Application.UseCase;
using ms_user_management.Api.Shared.Application.Mapper;
using ms_user_management.Api.Shared.Application.Search;
using ms_user_management.Api.Shared.Application.Search.Strategy;
using ms_user_management.Api.Shared.Domain.Port.Out;
using ms_user_management.Api.Shared.Infrastructure.Persistence.Context;
using ms_user_management.Api.Shared.Infrastructure.Persistence.Mapper;
using ms_user_management.Api.Shared.Infrastructure.Persistence.Repository;
using ms_user_management.Api.Student.Application.UseCase;

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

        services.AddDbContext<UserManagementContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection")
            ));

        services.AddScoped<IPersonRepository, PersonRepository>();
        services.AddScoped<IPersonSearchRepository, PersonSearchRepository>();

        services.AddScoped<IPersonSearchStrategy, EmailSearchStrategy>();
        services.AddScoped<IPersonSearchStrategy, IdentificationSearchStrategy>();
        services.AddScoped<IPersonSearchStrategy, NameSearchStrategy>();

        services.AddScoped<SearchPersonService>();

        services.AddScoped<CreateStudentService>();
        services.AddScoped<UpdateStudentService>();
        services.AddScoped<DeleteStudentService>();
        services.AddScoped<ListStudentService>();
        services.AddScoped<GetStudentService>();

        services.AddScoped<CreateDriverService>();
        services.AddScoped<UpdateDriverService>();
        services.AddScoped<DeleteDriverService>();
        services.AddScoped<ListDriverService>();
        services.AddScoped<GetDriverService>();

        services.AddScoped<CreateParentService>();
        services.AddScoped<UpdateParentService>();
        services.AddScoped<DeleteParentService>();
        services.AddScoped<ListParentService>();
        services.AddScoped<GetParentService>();

        services.AddScoped<CreateAdminService>();
        services.AddScoped<UpdateAdminService>();
        services.AddScoped<DeleteAdminService>();
        services.AddScoped<ListAdminService>();
        services.AddScoped<GetAdminService>();

        return services;
    }
}

using Microsoft.EntityFrameworkCore;
using ms_user_management.Api.Shared.Application.Mapper;
using ms_user_management.Api.Shared.Application.Search;
using ms_user_management.Api.Shared.Application.Search.Strategy;
using ms_user_management.Api.Shared.Domain.Port.Out;
using ms_user_management.Api.Shared.Infrastructure.Persistence.Context;
using ms_user_management.Api.Shared.Infrastructure.Persistence.Mapper;
using ms_user_management.Api.Shared.Infrastructure.Persistence.Repository;
using ms_user_management.Api.Student.Application.UseCase;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<PersonProfile>();
    cfg.AddProfile<PersonPersistenceProfile>();
});

builder.Services.AddDbContext<UserManagementContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    ));

builder.Services.AddControllers();

builder.Services.AddScoped<IPersonRepository, PersonRepository>();
builder.Services.AddScoped<IPersonSearchRepository, PersonSearchRepository>();

builder.Services.AddScoped<IPersonSearchStrategy, EmailSearchStrategy>();
builder.Services.AddScoped<IPersonSearchStrategy, IdentificationSearchStrategy>();
builder.Services.AddScoped<IPersonSearchStrategy, NameSearchStrategy>();

builder.Services.AddScoped<CreateStudentService>();
builder.Services.AddScoped<UpdateStudentService>();
builder.Services.AddScoped<DeleteStudentService>();
builder.Services.AddScoped<ListStudentService>();
builder.Services.AddScoped<GetStudentService>();

builder.Services.AddScoped<SearchPersonService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapOpenApi();
app.MapScalarApiReference();

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
using System.Data.Common;
using AutoMapper;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.SqlServer;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging.Abstractions;
using ms_user_management.Api.Family.Domain.Model;
using ms_user_management.Api.Family.Infrastructure.Persistence;
using ms_user_management.Api.Family.Infrastructure.Repository;
using ms_user_management.Api.Shared.Domain.Model;
using ms_user_management.Api.Shared.Infrastructure.Persistence.Context;
using ms_user_management.Api.Shared.Infrastructure.Persistence.Entity;
using ms_user_management.Api.Shared.Infrastructure.Persistence.Repository;
using NUnit.Framework;

namespace ms_user_management.Tests.Shared;

[TestFixture]
public class RecordDeletionPersistenceTests
{
    private SqliteConnection _connection = null!;
    private DeletionTestContext _context = null!;
    private DeletionFailureInterceptor _failure = null!;
    private PersonRepository _people = null!;
    private FamilyRepositoryImpl _families = null!;
    private Guid _personId;
    private Guid _otherPersonId;
    private Guid _profileId;
    private Guid _otherProfileId;
    private Guid _familyId;
    private Guid _otherFamilyId;

    [SetUp]
    public async Task SetUp()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        await _connection.OpenAsync();
        _failure = new DeletionFailureInterceptor();
        var options = new DbContextOptionsBuilder<UserManagementContext>()
            .UseSqlite(_connection)
            .ReplaceService<IExecutionStrategyFactory, RetryingStrategyFactory>()
            .AddInterceptors(_failure)
            .Options;
        _context = new DeletionTestContext(options);
        await _context.Database.EnsureCreatedAsync();

        _personId = Guid.NewGuid();
        _otherPersonId = Guid.NewGuid();
        _profileId = Guid.NewGuid();
        _otherProfileId = Guid.NewGuid();
        _familyId = Guid.NewGuid();
        _otherFamilyId = Guid.NewGuid();

        _context.Person.AddRange(Person(_personId), Person(_otherPersonId));
        _context.Families.AddRange(
            new FamilyEntity { Id = _familyId, Name = "Target", Observations = "", Status = Status.Active },
            new FamilyEntity { Id = _otherFamilyId, Name = "Other", Observations = "", Status = Status.Active });
        _context.FamilyMembers.AddRange(
            Member(_familyId, _profileId),
            Member(_familyId, _otherProfileId),
            Member(_otherFamilyId, _otherProfileId));
        await _context.SaveChangesAsync();
        await _context.Database.ExecuteSqlInterpolatedAsync(
            $"INSERT INTO \"Profile\" (\"Id\", \"PersonId\", \"Status\", \"RoleId\") VALUES ({_profileId}, {_personId}, {"Active"}, {(int)RoleId.Parent})");
        await _context.Database.ExecuteSqlInterpolatedAsync(
            $"INSERT INTO \"Profile\" (\"Id\", \"PersonId\", \"Status\", \"RoleId\") VALUES ({_otherProfileId}, {_otherPersonId}, {"Active"}, {(int)RoleId.Student})");
        _context.ChangeTracker.Clear();

        var mapper = new MapperConfiguration(_ => { }, NullLoggerFactory.Instance).CreateMapper();
        _people = new PersonRepository(_context, mapper);
        _families = new FamilyRepositoryImpl(_context);
    }

    [TearDown]
    public async Task TearDown()
    {
        await _context.DisposeAsync();
        await _connection.DisposeAsync();
    }

    [Test]
    public async Task Unwrapped_transaction_reproduces_the_original_retry_strategy_error()
    {
        await using var tx = await _context.Database.BeginTransactionAsync();

        var error = Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _context.Person.Where(p => p.Id == _personId)
                .ExecuteUpdateAsync(setters => setters.SetProperty(p => p.Status, Status.Inactive)));

        Assert.That(error!.Message, Does.Contain("does not support user-initiated transactions"));
        await tx.RollbackAsync();
    }

    [Test]
    public async Task Person_delete_deactivates_person_and_profiles_without_deleting_unrelated_members()
    {
        await _people.DeleteAsync(_personId);

        Assert.That(await _context.Person.CountAsync(), Is.EqualTo(2));
        Assert.That(await PersonStatus(_personId), Is.EqualTo(Status.Inactive));
        Assert.That(await PersonStatus(_otherPersonId), Is.EqualTo(Status.Active));
        Assert.That(await ProfileStatus(), Is.EqualTo("Inactive"));
        Assert.That(await _context.ProfileRefEntities.Where(p => p.Id == _otherProfileId)
            .Select(p => p.Status).SingleAsync(), Is.EqualTo("Active"));
        Assert.That(await _context.FamilyMembers.AnyAsync(m => m.ProfileId == _profileId), Is.False);
        Assert.That(await _context.FamilyMembers.CountAsync(), Is.EqualTo(2));
        Assert.That(await _context.Families.CountAsync(), Is.EqualTo(2));
    }

    [Test]
    public async Task Person_delete_replays_all_updates_after_transient_failure()
    {
        _failure.Arm("DELETE FROM \"FamilyMember\"", new TimeoutException("Transient test failure"), 1);

        await _people.DeleteAsync(_personId);

        Assert.That(_failure.Attempts, Is.EqualTo(2));
        Assert.That(await PersonStatus(_personId), Is.EqualTo(Status.Inactive));
        Assert.That(await ProfileStatus(), Is.EqualTo("Inactive"));
        Assert.That(await _context.FamilyMembers.CountAsync(), Is.EqualTo(2));
    }

    [Test]
    public async Task Person_delete_rolls_back_all_changes_on_permanent_failure()
    {
        _failure.Arm("DELETE FROM \"FamilyMember\"", new InvalidOperationException("Permanent test failure"), 1);

        Assert.ThrowsAsync<InvalidOperationException>(() => _people.DeleteAsync(_personId));

        Assert.That(_failure.Attempts, Is.EqualTo(1));
        await AssertPersonUnchanged();
    }

    [Test]
    public async Task Person_delete_rolls_back_when_transient_retries_are_exhausted()
    {
        _failure.Arm("DELETE FROM \"FamilyMember\"", new TimeoutException("Persistent test timeout"), 3);

        Assert.ThrowsAsync<RetryLimitExceededException>(() => _people.DeleteAsync(_personId));

        Assert.That(_failure.Attempts, Is.EqualTo(3));
        await AssertPersonUnchanged();
    }

    [Test]
    public async Task Person_delete_rejects_missing_person_without_modifying_other_records()
    {
        var error = Assert.ThrowsAsync<Exception>(() => _people.DeleteAsync(Guid.NewGuid()));

        Assert.That(error!.Message, Is.EqualTo("Person not found"));
        await AssertPersonUnchanged();
    }

    [Test]
    public async Task Family_delete_removes_only_target_family_and_members()
    {
        await _families.DeleteAsync(_familyId);

        Assert.That(await _context.Families.AnyAsync(f => f.Id == _familyId), Is.False);
        Assert.That(await _context.Families.AnyAsync(f => f.Id == _otherFamilyId), Is.True);
        Assert.That(await _context.FamilyMembers.CountAsync(), Is.EqualTo(1));
        Assert.That(await _context.FamilyMembers.AllAsync(m => m.FamilyId == _otherFamilyId), Is.True);
        Assert.That(await PersonStatus(_personId), Is.EqualTo(Status.Active));
        Assert.That(await ProfileStatus(), Is.EqualTo("Active"));
    }

    [Test]
    public async Task Family_delete_retries_whole_transaction_after_transient_failure()
    {
        _failure.Arm("DELETE FROM \"Family\"", new TimeoutException("Transient test failure"), 1);

        await _families.DeleteAsync(_familyId);

        Assert.That(_failure.Attempts, Is.EqualTo(2));
        Assert.That(await _context.Families.CountAsync(), Is.EqualTo(1));
        Assert.That(await _context.FamilyMembers.CountAsync(), Is.EqualTo(1));
    }

    [Test]
    public async Task Family_delete_rolls_back_members_when_family_delete_fails()
    {
        _failure.Arm("DELETE FROM \"Family\"", new InvalidOperationException("Permanent test failure"), 1);

        Assert.ThrowsAsync<InvalidOperationException>(() => _families.DeleteAsync(_familyId));

        Assert.That(await _context.Families.CountAsync(), Is.EqualTo(2));
        Assert.That(await _context.FamilyMembers.CountAsync(), Is.EqualTo(3));
    }

    [Test]
    public async Task Family_delete_rolls_back_when_transient_retries_are_exhausted()
    {
        _failure.Arm("DELETE FROM \"Family\"", new TimeoutException("Persistent test timeout"), 3);

        Assert.ThrowsAsync<RetryLimitExceededException>(() => _families.DeleteAsync(_familyId));

        Assert.That(_failure.Attempts, Is.EqualTo(3));
        Assert.That(await _context.Families.CountAsync(), Is.EqualTo(2));
        Assert.That(await _context.FamilyMembers.CountAsync(), Is.EqualTo(3));
    }

    [Test]
    public async Task Family_delete_honors_cancellation_without_changing_records()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        Assert.CatchAsync<OperationCanceledException>(() => _families.DeleteAsync(_familyId, cancellation.Token));

        Assert.That(await _context.Families.CountAsync(), Is.EqualTo(2));
        Assert.That(await _context.FamilyMembers.CountAsync(), Is.EqualTo(3));
    }

    [Test]
    public async Task Family_delete_rejects_missing_family()
    {
        Assert.ThrowsAsync<InvalidOperationException>(() => _families.DeleteAsync(Guid.NewGuid()));

        Assert.That(await _context.Families.CountAsync(), Is.EqualTo(2));
        Assert.That(await _context.FamilyMembers.CountAsync(), Is.EqualTo(3));
    }

    private async Task AssertPersonUnchanged()
    {
        Assert.That(await PersonStatus(_personId), Is.EqualTo(Status.Active));
        Assert.That(await ProfileStatus(), Is.EqualTo("Active"));
        Assert.That(await _context.FamilyMembers.CountAsync(), Is.EqualTo(3));
    }

    private Task<Status> PersonStatus(Guid id) =>
        _context.Person.Where(p => p.Id == id).Select(p => p.Status).SingleAsync();

    private Task<string> ProfileStatus() =>
        _context.ProfileRefEntities.Where(p => p.Id == _profileId).Select(p => p.Status).SingleAsync();

    private static PersonEntity Person(Guid id) => new()
    {
        Id = id, Name = "Test", LastName = "Person", IdentificationType = IdentificationType.CC,
        IdentificationNumber = id.ToString(), Email = "test@example.com", Phone = 3001234567,
        ResidenceAddress = "Test address", DateBirth = new DateOnly(2000, 1, 1), Status = Status.Active
    };

    private static FamilyMemberEntity Member(Guid familyId, Guid profileId) => new()
    {
        Id = Guid.NewGuid(), FamilyId = familyId, ProfileId = profileId,
        RelationType = RelationType.Parent, Status = Status.Active
    };

    private sealed class DeletionTestContext(DbContextOptions<UserManagementContext> options)
        : UserManagementContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Correct the existing SQL Server column-name mismatch only in the SQLite fixture.
            modelBuilder.Entity<FamilyMemberEntity>().ToTable(t =>
                t.HasCheckConstraint("CK_FamilyMember_RelationType", "\"RelationshipType\" IN ('Parent', 'Student')"));
        }
    }

    private sealed class RetryingStrategyFactory(ExecutionStrategyDependencies dependencies)
        : IExecutionStrategyFactory
    {
        public IExecutionStrategy Create() =>
            new SqlServerRetryingExecutionStrategy(dependencies, 2, TimeSpan.Zero, null);
    }

    private sealed class DeletionFailureInterceptor : DbCommandInterceptor
    {
        private string? _commandPrefix;
        private Exception? _error;
        private int _failuresRemaining;
        public int Attempts { get; private set; }

        public void Arm(string commandPrefix, Exception error, int failures)
        {
            _commandPrefix = commandPrefix;
            _error = error;
            _failuresRemaining = failures;
        }

        public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(
            DbCommand command, CommandEventData eventData, InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            if (_commandPrefix is not null && command.CommandText.StartsWith(_commandPrefix, StringComparison.Ordinal))
            {
                Attempts++;
                if (_failuresRemaining-- > 0) throw _error!;
            }
            return ValueTask.FromResult(result);
        }
    }
}

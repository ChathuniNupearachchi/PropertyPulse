using Microsoft.EntityFrameworkCore;
using Npgsql;
using PropertyPulse.Domain.Entities;
using PropertyPulse.Domain.Exceptions;
using PropertyPulse.Infrastructure.Data;

namespace PropertyPulse.Infrastructure.Repositories;

public class UserRepository(AppDbContext dbContext) : IUserRepository
{
    public Task<User?> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default) =>
        dbContext.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);

    public Task<User?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public Task<List<User>> ListAsync(CancellationToken cancellationToken = default) =>
        dbContext.Users.AsNoTracking().OrderBy(u => u.FullName).ToListAsync(cancellationToken);

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        dbContext.Users.Add(user);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            // Covers two registrations racing past the duplicate check.
            throw new EmailAlreadyInUseException();
        }
    }
}

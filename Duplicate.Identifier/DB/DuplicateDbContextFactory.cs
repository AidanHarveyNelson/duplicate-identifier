using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Duplicate.Identifier.Db;

/// <summary>
/// DuplicateDbContext factory.
/// </summary>
public class DuplicateDbContextFactory : IDesignTimeDbContextFactory<DuplicateDbContext>
{
    /// <inheritdoc/>
    public DuplicateDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<DuplicateDbContext>();
        optionsBuilder.UseSqlite("Data Source=duplicate.db")
                      .EnableSensitiveDataLogging(false);

        return new DuplicateDbContext(optionsBuilder.Options);
    }
}

using EBL.FIG.Process.Identity.Infra.Data.Context;
using Microsoft.Extensions.Logging;

namespace EBL.FIG.Process.Identity.Infra.Data.Seeders;

public class DatabaseSeeder
{
    private readonly IdentityDbContext _context;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(IdentityDbContext context, ILogger<DatabaseSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken ct = default)
    {

    }
}

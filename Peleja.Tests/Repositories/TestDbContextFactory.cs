namespace Peleja.Tests.Repositories;

using Microsoft.EntityFrameworkCore;
using Peleja.Infra.Context;

public static class TestDbContextFactory
{
    public static PelejaContext Create(string? dbName = null)
    {
        dbName ??= Guid.NewGuid().ToString();

        var options = new DbContextOptionsBuilder<PelejaContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        var context = new PelejaContext(options);
        context.Database.EnsureCreated();
        return context;
    }
}

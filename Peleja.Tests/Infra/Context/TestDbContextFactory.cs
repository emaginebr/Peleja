namespace Peleja.Tests.Infra.Context;

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Peleja.Infra.Context;
using Peleja.Infra.Mappings;
using Peleja.Domain.Mappings;

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

    public static IMapper CreateMapper()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<PageMapperProfile>();
            cfg.AddProfile<CommentMapperProfile>();
            cfg.AddProfile<CommentLikeMapperProfile>();
            cfg.AddProfile<CommentResultProfile>();
            cfg.AddProfile<CommentLikeResultProfile>();
        });
        return config.CreateMapper();
    }
}

using Artisaback.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Artisaback.Data.DbContext;

public class MongoDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    public MongoDbContext(DbContextOptions<MongoDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
}
﻿using IntegrationServices.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IntegrationServices.EF;

public class EFIntegrationDbContext : DbContext, IEFIntegrationDbContext
{
    public EFIntegrationDbContext(DbContextOptions<EFIntegrationDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("app");

        builder.Entity<IntegrationEventLogEntry>(ConfigureEvent);
    }

    private void ConfigureEvent(EntityTypeBuilder<IntegrationEventLogEntry> builder)
    {
        builder.Property(x => x.EventType).IsRequired();
        builder.Property(x => x.Content).IsRequired();
        builder.Ignore(x => x.IntegrationEvent);
    }

    public DbSet<IntegrationEventLogEntry> IntegrationEvents { get; set; }
}

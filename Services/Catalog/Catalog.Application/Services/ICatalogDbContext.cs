﻿using Catalog.Domian.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Catalog.Application.Services;

public interface ICatalogDbContext
{
    DatabaseFacade Database { get; }

    DbSet<CatalogItem> CatalogItems { get; }
    DbSet<CatalogBrand> CatalogBrands { get; }
    DbSet<CatalogType> CatalogTypes { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

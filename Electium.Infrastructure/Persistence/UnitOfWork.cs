using System;
using System.Collections.Generic;
using System.Text;
using Electium.Application.Interfaces;

namespace Electium.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly ElectiumDbContext _context;

    public UnitOfWork(ElectiumDbContext context)
    {
        _context = context;
    }

    public Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
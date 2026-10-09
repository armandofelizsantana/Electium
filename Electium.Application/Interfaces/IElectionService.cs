using System;
using System.Collections.Generic;
using System.Text;
using Electium.Application.DTOs;
using Electium.Domain.Entities;

namespace Electium.Application.Interfaces;

public interface IElectionService
    : IGenericService<
        Election,
        ElectionDto,
        CreateElectionDto,
        UpdateElectionDto>
{
    Task<bool> ActivateAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<bool> CompleteAsync(
        int id,
        CancellationToken cancellationToken = default);
}
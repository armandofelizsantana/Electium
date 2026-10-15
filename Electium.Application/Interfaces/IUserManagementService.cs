using System;
using System.Collections.Generic;
using System.Text;
using Electium.Application.DTOs;

namespace Electium.Application.Interfaces;

public interface IUserManagementService
{
    Task<IReadOnlyList<SystemUserDto>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<SystemUserDto?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<SystemUserDto> CreateAsync(
        CreateSystemUserDto dto,
        CancellationToken cancellationToken = default);

    Task<SystemUserDto?> UpdateAsync(
        UpdateSystemUserDto dto,
        CancellationToken cancellationToken = default);

    Task<bool> SetActiveAsync(
        int id,
        bool isActive,
        CancellationToken cancellationToken = default);
}
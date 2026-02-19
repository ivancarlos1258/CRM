using CRM.Application.DTOs;

namespace CRM.Application.Services;

public interface IZipCodeService
{
    Task<ZipCodeInfoDto?> GetAddressByZipCodeAsync(string zipCode, CancellationToken cancellationToken = default);
}

using CRM.Application.Common;
using CRM.Application.DTOs;

namespace CRM.Application.Queries.ZipCode;

public record GetZipCodeInfoQuery(string ZipCode) : IQuery<Result<ZipCodeInfoDto>>;

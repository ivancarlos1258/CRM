using CRM.Application.Common;
using CRM.Application.DTOs;
using CRM.Application.Services;

namespace CRM.Application.Queries.ZipCode;

public class GetZipCodeInfoHandler : IQueryHandler<GetZipCodeInfoQuery, Result<ZipCodeInfoDto>>
{
    private readonly IZipCodeService _zipCodeService;

    public GetZipCodeInfoHandler(IZipCodeService zipCodeService)
    {
        _zipCodeService = zipCodeService;
    }

    public async Task<Result<ZipCodeInfoDto>> Handle(GetZipCodeInfoQuery request, CancellationToken cancellationToken)
    {
        var zipCodeInfo = await _zipCodeService.GetAddressByZipCodeAsync(request.ZipCode, cancellationToken);

        if (zipCodeInfo == null || zipCodeInfo.Erro)
            return Result<ZipCodeInfoDto>.Failure("CEP não encontrado");

        return Result<ZipCodeInfoDto>.Success(zipCodeInfo);
    }
}

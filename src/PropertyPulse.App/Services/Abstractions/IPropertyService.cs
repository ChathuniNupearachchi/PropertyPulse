using PropertyPulse.Shared;
using PropertyPulse.Shared.Properties;

namespace PropertyPulse.App.Services;

public interface IPropertyService
{
	Task<ApiResult<PagedResult<PropertyDto>>> ListAsync(
		PropertyFilter filter, int page, int pageSize, CancellationToken cancellationToken = default);

	Task<ApiResult<PropertyDto>> GetAsync(Guid id, CancellationToken cancellationToken = default);

	Task<ApiResult<PropertyDto>> CreateAsync(
		PropertyFormRequest request, IReadOnlyList<PendingPhoto> photos, CancellationToken cancellationToken = default);

	Task<ApiResult<PropertyDto>> UpdateAsync(
		Guid id,
		PropertyFormRequest request,
		IReadOnlyList<PendingPhoto> newPhotos,
		IReadOnlyList<Guid> removedImageIds,
		CancellationToken cancellationToken = default);

	Task<ApiResult<Unit>> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

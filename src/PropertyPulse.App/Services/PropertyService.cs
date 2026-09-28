using System.Globalization;
using PropertyPulse.Shared;
using PropertyPulse.Shared.Properties;

namespace PropertyPulse.App.Services;

public class PropertyService(IApiClient apiClient) : IPropertyService
{
	public Task<ApiResult<PagedResult<PropertyDto>>> ListAsync(
		PropertyFilter filter, int page, int pageSize, CancellationToken cancellationToken = default) =>
		apiClient.GetAsync<PagedResult<PropertyDto>>($"api/properties?{filter.ToQueryString(page, pageSize)}", cancellationToken);

	public Task<ApiResult<PropertyDto>> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
		apiClient.GetAsync<PropertyDto>($"api/properties/{id}", cancellationToken);

	public Task<ApiResult<PropertyDto>> CreateAsync(
		PropertyFormRequest request, IReadOnlyList<PendingPhoto> photos, CancellationToken cancellationToken = default) =>
		apiClient.SendMultipartAsync<PropertyDto>(HttpMethod.Post, "api/properties", BuildFields(request), photos, cancellationToken);

	public async Task<ApiResult<PropertyDto>> UpdateAsync(
		Guid id,
		PropertyFormRequest request,
		IReadOnlyList<PendingPhoto> newPhotos,
		IReadOnlyList<Guid> removedImageIds,
		CancellationToken cancellationToken = default)
	{
		foreach (var imageId in removedImageIds)
		{
			var removeResult = await apiClient.DeleteAsync($"api/properties/{id}/images/{imageId}", cancellationToken);
			if (!removeResult.IsSuccess)
			{
				return ApiResult<PropertyDto>.Failure(removeResult.Error, removeResult.Message);
			}
		}

		return await apiClient.SendMultipartAsync<PropertyDto>(
			HttpMethod.Put, $"api/properties/{id}", BuildFields(request), newPhotos, cancellationToken);
	}

	public async Task<ApiResult<Unit>> DeleteAsync(Guid id, CancellationToken cancellationToken = default) =>
		await apiClient.DeleteAsync($"api/properties/{id}", cancellationToken);

	private static Dictionary<string, string> BuildFields(PropertyFormRequest request)
	{
		var fields = new Dictionary<string, string>
		{
			[nameof(request.Title)] = request.Title,
			[nameof(request.Description)] = request.Description,
			[nameof(request.PropertyType)] = request.PropertyType,
			[nameof(request.Status)] = request.Status,
			[nameof(request.PriceLkr)] = request.PriceLkr.ToString(CultureInfo.InvariantCulture),
			[nameof(request.Address)] = request.Address,
			[nameof(request.City)] = request.City,
			[nameof(request.Bedrooms)] = request.Bedrooms.ToString(CultureInfo.InvariantCulture),
			[nameof(request.Bathrooms)] = request.Bathrooms.ToString(CultureInfo.InvariantCulture)
		};

		AddIfSet(fields, nameof(request.Latitude), request.Latitude);
		AddIfSet(fields, nameof(request.Longitude), request.Longitude);
		AddIfSet(fields, nameof(request.FloorAreaSqFt), request.FloorAreaSqFt);
		AddIfSet(fields, nameof(request.LandSizePerches), request.LandSizePerches);

		return fields;
	}

	private static void AddIfSet(Dictionary<string, string> fields, string name, double? value)
	{
		if (value is not null)
		{
			fields[name] = value.Value.ToString(CultureInfo.InvariantCulture);
		}
	}
}

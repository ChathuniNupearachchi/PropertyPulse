using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using PropertyPulse.Api.Extensions;
using PropertyPulse.Api.Mappings;
using PropertyPulse.Api.Validation;
using PropertyPulse.Domain.Entities;
using PropertyPulse.Domain.Enums;
using PropertyPulse.Infrastructure.Repositories;
using PropertyPulse.Infrastructure.Storage;
using PropertyPulse.Shared;
using PropertyPulse.Shared.Properties;

namespace PropertyPulse.Api.Controllers;

[ApiController]
[Route("api/properties")]
[Authorize]
public class PropertiesController(
    IPropertyRepository propertyRepository,
    IPropertyImageStorage imageStorage,
    ProblemDetailsFactory problemDetailsFactory) : ControllerBase
{
    private const int DefaultPageSize = 20;

    [HttpGet]
    public async Task<ActionResult<PagedResult<PropertyDto>>> List(
        [FromQuery] PropertyType? type,
        [FromQuery] PropertyStatus? status,
        [FromQuery] string? city,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = DefaultPageSize,
        CancellationToken cancellationToken = default)
    {
        var query = new PropertyQuery(type, status, city, minPrice, maxPrice, search, Math.Max(page, 1), Math.Clamp(pageSize, 1, 100));
        var (items, totalCount) = await propertyRepository.QueryAsync(query, cancellationToken);

        var baseUrl = GetBaseUrl();
        return new PagedResult<PropertyDto>(items.Select(p => p.ToDto(baseUrl)).ToList(), query.Page, query.PageSize, totalCount);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PropertyDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var property = await propertyRepository.GetByIdAsync(id, cancellationToken);
        return property is null ? NotFound() : property.ToDto(GetBaseUrl());
    }

    /// <summary>Creates a property, owned by the calling Agent.</summary>
    [HttpPost]
    [Authorize(Roles = nameof(UserRole.Agent))]
    public async Task<IActionResult> Create(
        [FromForm] PropertyFormRequest request, List<IFormFile> photos, CancellationToken cancellationToken)
    {
        var imageErrors = PropertyImageValidator.Validate(photos, existingCount: 0);
        if (imageErrors.Count > 0)
        {
            return ValidationProblemFrom(imageErrors);
        }

        var property = request.ToEntity();
        property.AgentId = User.GetUserId()!.Value;

        await propertyRepository.AddAsync(property, cancellationToken);
        await AddPhotosAsync(property, photos, cancellationToken);

        var saved = await propertyRepository.GetByIdAsync(property.Id, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, saved!.ToDto(GetBaseUrl()));
    }

    /// <summary>Updates a property's fields and appends any submitted photos. Owning Agent or any Manager.</summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id, [FromForm] PropertyFormRequest request, List<IFormFile> photos, CancellationToken cancellationToken)
    {
        var property = await propertyRepository.GetByIdAsync(id, cancellationToken);
        if (property is null)
        {
            return NotFound();
        }

        if (!CanManage(property))
        {
            return Forbid();
        }

        var imageErrors = PropertyImageValidator.Validate(photos, property.Images.Count);
        if (imageErrors.Count > 0)
        {
            return ValidationProblemFrom(imageErrors);
        }

        request.ApplyTo(property);
        await propertyRepository.UpdateAsync(property, cancellationToken);
        await AddPhotosAsync(property, photos, cancellationToken);

        var saved = await propertyRepository.GetByIdAsync(property.Id, cancellationToken);
        return Ok(saved!.ToDto(GetBaseUrl()));
    }

    /// <summary>Deletes a property, its photo records, and its photo files. Owning Agent or any Manager.</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var property = await propertyRepository.GetByIdAsync(id, cancellationToken);
        if (property is null)
        {
            return NotFound();
        }

        if (!CanManage(property))
        {
            return Forbid();
        }

        await propertyRepository.DeleteAsync(property, cancellationToken);
        imageStorage.DeleteAll(property.Id);

        return NoContent();
    }

    /// <summary>Adds photos to an existing property. Owning Agent or any Manager.</summary>
    [HttpPost("{id:guid}/images")]
    public async Task<IActionResult> AddImages(Guid id, List<IFormFile> photos, CancellationToken cancellationToken)
    {
        var property = await propertyRepository.GetByIdAsync(id, cancellationToken);
        if (property is null)
        {
            return NotFound();
        }

        if (!CanManage(property))
        {
            return Forbid();
        }

        var imageErrors = PropertyImageValidator.Validate(photos, property.Images.Count);
        if (imageErrors.Count > 0)
        {
            return ValidationProblemFrom(imageErrors);
        }

        await AddPhotosAsync(property, photos, cancellationToken);

        var saved = await propertyRepository.GetByIdAsync(property.Id, cancellationToken);
        return Ok(saved!.ToDto(GetBaseUrl()));
    }

    /// <summary>Removes one photo from a property. Owning Agent or any Manager.</summary>
    [HttpDelete("{id:guid}/images/{imageId:guid}")]
    public async Task<IActionResult> RemoveImage(Guid id, Guid imageId, CancellationToken cancellationToken)
    {
        var property = await propertyRepository.GetByIdAsync(id, cancellationToken);
        if (property is null)
        {
            return NotFound();
        }

        if (!CanManage(property))
        {
            return Forbid();
        }

        var image = property.Images.FirstOrDefault(i => i.Id == imageId);
        if (image is null)
        {
            return NotFound();
        }

        property.Images.Remove(image);
        await propertyRepository.UpdateAsync(property, cancellationToken);
        imageStorage.Delete(property.Id, image.FilePath);

        return NoContent();
    }

    private bool CanManage(Property property) =>
        User.IsInRole(nameof(UserRole.Manager)) || User.GetUserId() == property.AgentId;

    private async Task AddPhotosAsync(Property property, List<IFormFile> photos, CancellationToken cancellationToken)
    {
        if (photos.Count == 0)
        {
            return;
        }

        var existingCount = property.Images.Count;
        var newImages = new List<PropertyImage>();

        for (var i = 0; i < photos.Count; i++)
        {
            var extension = PropertyImageValidator.GetExtension(photos[i].ContentType);
            await using var stream = photos[i].OpenReadStream();
            var fileName = await imageStorage.SaveAsync(property.Id, stream, extension, cancellationToken);

            newImages.Add(new PropertyImage
            {
                PropertyId = property.Id,
                FilePath = fileName,
                SortOrder = existingCount + i,
                IsPrimary = existingCount == 0 && i == 0
            });
        }

        await propertyRepository.AddImagesAsync(newImages, cancellationToken);
    }

    private BadRequestObjectResult ValidationProblemFrom(IEnumerable<(string Field, string Message)> errors)
    {
        foreach (var (field, message) in errors)
        {
            ModelState.AddModelError(field, message);
        }

        return BadRequest(problemDetailsFactory.CreateValidationProblemDetails(HttpContext, ModelState));
    }

    private string GetBaseUrl() => $"{Request.Scheme}://{Request.Host}";
}

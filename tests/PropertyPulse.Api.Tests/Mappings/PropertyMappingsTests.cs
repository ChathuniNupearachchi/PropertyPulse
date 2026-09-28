using PropertyPulse.Api.Mappings;
using PropertyPulse.Domain.Entities;
using PropertyPulse.Domain.Enums;

namespace PropertyPulse.Api.Tests.Mappings;

public class PropertyMappingsTests
{
    [Fact]
    public void ToDto_MapsScalarFieldsAndAgentName()
    {
        var agent = new User { FullName = "Kasun Fernando", Email = "agent1@propertypulse.lk", Role = UserRole.Agent };
        var property = new Property
        {
            Title = "Test House",
            Description = "A test description.",
            PropertyType = PropertyType.House,
            Status = PropertyStatus.Available,
            PriceLkr = 25_000_000m,
            Address = "Test Address",
            City = "Colombo",
            Bedrooms = 3,
            Bathrooms = 2,
            Agent = agent,
            AgentId = agent.Id
        };

        var dto = property.ToDto("https://api.example.com");

        Assert.Equal(property.Id, dto.Id);
        Assert.Equal("Test House", dto.Title);
        Assert.Equal("House", dto.PropertyType);
        Assert.Equal("Available", dto.Status);
        Assert.Equal(25_000_000m, dto.PriceLkr);
        Assert.Equal(agent.Id, dto.AgentId);
        Assert.Equal("Kasun Fernando", dto.AgentName);
    }

    [Fact]
    public void ToDto_BuildsImageUrlsFromTheBaseUrlAndStoredFileName_OrderedBySortOrder()
    {
        var property = new Property { Title = "Test" };
        property.Images.Add(new PropertyImage { PropertyId = property.Id, FilePath = "second.jpg", SortOrder = 1 });
        property.Images.Add(new PropertyImage { PropertyId = property.Id, FilePath = "first.jpg", SortOrder = 0 });

        var dto = property.ToDto("https://api.example.com");

        Assert.Equal(2, dto.Images.Count);
        Assert.Equal($"https://api.example.com/images/properties/{property.Id}/first.jpg", dto.Images[0].Url);
        Assert.Equal($"https://api.example.com/images/properties/{property.Id}/second.jpg", dto.Images[1].Url);
    }

    [Fact]
    public void ToDto_WithNoAgentLoaded_ReturnsEmptyAgentName()
    {
        var property = new Property { Title = "Test", AgentId = Guid.NewGuid() };

        var dto = property.ToDto("https://api.example.com");

        Assert.Equal(string.Empty, dto.AgentName);
    }
}

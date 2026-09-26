using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PropertyPulse.Domain.Entities;
using PropertyPulse.Domain.Enums;
using PropertyPulse.Infrastructure.Data;

namespace PropertyPulse.Api.Seeding;

/// <summary>
/// Inserts sample data for local development. Every seeded user shares <see cref="Password"/>.
/// </summary>
public class DevelopmentDataSeeder(AppDbContext dbContext, IPasswordHasher<User> passwordHasher)
{
    public const string Password = "Password123!";

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (await dbContext.Users.AnyAsync(cancellationToken))
        {
            return;
        }

        var manager = CreateUser("Nimali Perera", "manager@propertypulse.lk", UserRole.Manager);
        var kasun = CreateUser("Kasun Fernando", "agent1@propertypulse.lk", UserRole.Agent);
        var dilini = CreateUser("Dilini Jayawardena", "agent2@propertypulse.lk", UserRole.Agent);

        var colomboApartment = new Property
        {
            Title = "Luxury 3-bedroom apartment in Colombo 07",
            Description = "Top-floor apartment with city views, a pool, and secure parking.",
            PropertyType = PropertyType.Apartment,
            Status = PropertyStatus.Available,
            PriceLkr = 65_000_000m,
            Address = "Independence Avenue, Colombo 07",
            City = "Colombo",
            Latitude = 6.9147,
            Longitude = 79.8613,
            Bedrooms = 3,
            Bathrooms = 2,
            FloorAreaSqFt = 1650,
            Agent = kasun,
            Images =
            [
                new PropertyImage { FilePath = "seed/colombo-apartment-1.jpg", SortOrder = 0, IsPrimary = true },
                new PropertyImage { FilePath = "seed/colombo-apartment-2.jpg", SortOrder = 1 }
            ]
        };

        var kandyHouse = new Property
        {
            Title = "Hillside family house on Peradeniya Road",
            Description = "Four-year-old house with a garden and a view of the Hantana range.",
            PropertyType = PropertyType.House,
            Status = PropertyStatus.Reserved,
            PriceLkr = 38_500_000m,
            Address = "Peradeniya Road, Kandy",
            City = "Kandy",
            Latitude = 7.2664,
            Longitude = 80.5970,
            Bedrooms = 4,
            Bathrooms = 3,
            FloorAreaSqFt = 2100,
            LandSizePerches = 15,
            Agent = kasun,
            Images = [new PropertyImage { FilePath = "seed/kandy-house-1.jpg", SortOrder = 0, IsPrimary = true }]
        };

        var galleVilla = new Property
        {
            Title = "Colonial-style villa near Galle Fort",
            Description = "Restored villa with a courtyard, a five-minute walk from the fort.",
            PropertyType = PropertyType.House,
            Status = PropertyStatus.Available,
            PriceLkr = 92_000_000m,
            Address = "Lighthouse Street, Galle",
            City = "Galle",
            Latitude = 6.0263,
            Longitude = 80.2170,
            Bedrooms = 5,
            Bathrooms = 4,
            FloorAreaSqFt = 3400,
            LandSizePerches = 25,
            Agent = dilini,
            Images = [new PropertyImage { FilePath = "seed/galle-villa-1.jpg", SortOrder = 0, IsPrimary = true }]
        };

        var galleLand = new Property
        {
            Title = "Coastal land block in Unawatuna",
            Description = "Flat, cleared block with road access, 300 metres from the beach.",
            PropertyType = PropertyType.Land,
            Status = PropertyStatus.Available,
            PriceLkr = 14_750_000m,
            Address = "Matara Road, Unawatuna",
            City = "Galle",
            Latitude = 6.0108,
            Longitude = 80.2494,
            LandSizePerches = 20,
            Agent = dilini
        };

        var negomboHouse = new Property
        {
            Title = "Modern house near Negombo beach",
            Description = "Two-storey house with a tiled roof terrace, sold fully furnished.",
            PropertyType = PropertyType.House,
            Status = PropertyStatus.Sold,
            PriceLkr = 27_800_000m,
            Address = "Lewis Place, Negombo",
            City = "Negombo",
            Latitude = 7.2008,
            Longitude = 79.8737,
            Bedrooms = 3,
            Bathrooms = 2,
            FloorAreaSqFt = 1800,
            LandSizePerches = 10,
            Agent = kasun,
            Images = [new PropertyImage { FilePath = "seed/negombo-house-1.jpg", SortOrder = 0, IsPrimary = true }]
        };

        var nuwaraEliyaCottage = new Property
        {
            Title = "Cottage with tea estate views",
            Description = "Stone cottage with a fireplace and a private garden.",
            PropertyType = PropertyType.House,
            Status = PropertyStatus.Available,
            PriceLkr = 21_500_000m,
            Address = "Lake Road, Nuwara Eliya",
            City = "Nuwara Eliya",
            Latitude = 6.9497,
            Longitude = 80.7891,
            Bedrooms = 3,
            Bathrooms = 2,
            FloorAreaSqFt = 1500,
            LandSizePerches = 12,
            Agent = dilini
        };

        var colomboShop = new Property
        {
            Title = "Ground-floor commercial space on Galle Road",
            Description = "High-footfall frontage, suitable for retail or a showroom.",
            PropertyType = PropertyType.Commercial,
            Status = PropertyStatus.Available,
            PriceLkr = 145_000_000m,
            Address = "Galle Road, Colombo 03",
            City = "Colombo",
            Latitude = 6.9022,
            Longitude = 79.8531,
            FloorAreaSqFt = 3200,
            Agent = kasun
        };

        var kandyApartment = new Property
        {
            Title = "Two-bedroom apartment in Kandy city",
            Description = "Third-floor apartment close to the Temple of the Tooth and the lake.",
            PropertyType = PropertyType.Apartment,
            Status = PropertyStatus.Available,
            PriceLkr = 16_900_000m,
            Address = "Dalada Veediya, Kandy",
            City = "Kandy",
            Latitude = 7.2936,
            Longitude = 80.6413,
            Bedrooms = 2,
            Bathrooms = 1,
            FloorAreaSqFt = 950,
            Agent = dilini
        };

        var sanjaya = new Lead
        {
            FullName = "Sanjaya Wickramasinghe",
            Phone = "+94 77 123 4567",
            Email = "sanjaya.w@example.com",
            Stage = LeadStage.Visiting,
            BudgetLkr = 70_000_000m,
            Agent = kasun,
            Property = colomboApartment,
            Notes =
            [
                new LeadNote { Author = kasun, Text = "Prefers a higher floor with a sea view." },
                new LeadNote { Author = kasun, Text = "Wants to see it again with his spouse at the weekend." }
            ]
        };

        var priyanka = new Lead
        {
            FullName = "Priyanka Rajapaksa",
            Phone = "+94 71 234 5678",
            Email = "priyanka.r@example.com",
            Stage = LeadStage.Negotiating,
            BudgetLkr = 40_000_000m,
            Agent = kasun,
            Property = kandyHouse,
            Notes =
            [
                new LeadNote { Author = kasun, Text = "Offer of LKR 37,000,000 sent, waiting for a reply." },
                new LeadNote { Author = manager, Text = "Approved to go down to LKR 36,500,000 if she signs this month." }
            ]
        };

        var ruwan = new Lead
        {
            FullName = "Ruwan Silva",
            Phone = "+94 76 345 6789",
            Stage = LeadStage.Contacted,
            BudgetLkr = 100_000_000m,
            Agent = dilini,
            Property = galleVilla,
            Notes = [new LeadNote { Author = dilini, Text = "Looking for a beachside property. Brochure sent by WhatsApp." }]
        };

        var tharushi = new Lead
        {
            FullName = "Tharushi de Silva",
            Phone = "+94 70 456 7890",
            Email = "tharushi.ds@example.com",
            Stage = LeadStage.New,
            BudgetLkr = 15_000_000m,
            Agent = dilini,
            Property = galleLand
        };

        var fazil = new Lead
        {
            FullName = "Mohamed Fazil",
            Phone = "+94 75 567 8901",
            Stage = LeadStage.Won,
            BudgetLkr = 28_000_000m,
            Agent = kasun,
            Property = negomboHouse,
            Notes = [new LeadNote { Author = kasun, Text = "Documents signed and deposit received." }]
        };

        var anjali = new Lead
        {
            FullName = "Anjali Kumar",
            Phone = "+94 72 678 9012",
            Stage = LeadStage.Lost,
            BudgetLkr = 20_000_000m,
            Agent = dilini,
            Property = nuwaraEliyaCottage,
            Notes = [new LeadNote { Author = dilini, Text = "Budget is too low for Nuwara Eliya." }]
        };

        var chamara = new Lead
        {
            FullName = "Chamara Bandara",
            Phone = "+94 78 789 0123",
            Stage = LeadStage.New,
            BudgetLkr = 150_000_000m,
            Agent = kasun
        };

        var today = DateTime.UtcNow.Date;

        var visits = new[]
        {
            new SiteVisit
            {
                Lead = sanjaya,
                Property = colomboApartment,
                Agent = kasun,
                ScheduledAt = today.AddDays(2).AddHours(5)
            },
            new SiteVisit
            {
                Lead = priyanka,
                Property = kandyHouse,
                Agent = kasun,
                ScheduledAt = today.AddDays(-3).AddHours(6),
                CompletedAt = today.AddDays(-3).AddHours(7),
                Notes = "Liked the garden, concerned about road noise."
            },
            new SiteVisit
            {
                Lead = ruwan,
                Property = galleVilla,
                Agent = dilini,
                ScheduledAt = today.AddDays(5).AddHours(4)
            },
            new SiteVisit
            {
                Lead = fazil,
                Property = negomboHouse,
                Agent = kasun,
                ScheduledAt = today.AddDays(-20).AddHours(5),
                CompletedAt = today.AddDays(-20).AddHours(6),
                Notes = "Decided on the spot, asked for the furniture to be included."
            }
        };

        dbContext.Users.AddRange(manager, kasun, dilini);
        dbContext.Properties.AddRange(
            colomboApartment, kandyHouse, galleVilla, galleLand, negomboHouse, nuwaraEliyaCottage, colomboShop, kandyApartment);
        dbContext.Leads.AddRange(sanjaya, priyanka, ruwan, tharushi, fazil, anjali, chamara);
        dbContext.SiteVisits.AddRange(visits);

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private User CreateUser(string fullName, string email, UserRole role)
    {
        var user = new User { FullName = fullName, Email = email, Role = role };
        user.PasswordHash = passwordHasher.HashPassword(user, Password);
        return user;
    }
}

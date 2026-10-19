using ARROW_ARO_RICE.Models;
using Microsoft.EntityFrameworkCore;

namespace ARROW_ARO_RICE.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(ApplicationDbContext db)
    {
        await db.Database.EnsureCreatedAsync();

        if (await db.Users.AnyAsync())
        {
            return; // DB has been seeded
        }

        // 1. Seed Users
        string adminHash = BCrypt.Net.BCrypt.HashPassword("Admin@2026#Rice");
        string salesHash = BCrypt.Net.BCrypt.HashPassword("Sales@2026#Rice");
        string inventoryHash = BCrypt.Net.BCrypt.HashPassword("Inventory@2026#Rice");
        string accountsHash = BCrypt.Net.BCrypt.HashPassword("Accounts@2026#Rice");

        var adminUser = new User
        {
            Username = "admin",
            Email = "admin@arrowarorice.com",
            PasswordHash = adminHash,
            FullName = "Rajesh Sharma",
            Role = UserRole.Admin,
            Department = "Executive Management",
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddMonths(-6)
        };

        var salesUser = new User
        {
            Username = "sales",
            Email = "sales@arrowarorice.com",
            PasswordHash = salesHash,
            FullName = "Tariq Mansoor",
            Role = UserRole.Sales,
            Department = "International Export Sales",
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddMonths(-6)
        };

        var inventoryUser = new User
        {
            Username = "inventory",
            Email = "inventory@arrowarorice.com",
            PasswordHash = inventoryHash,
            FullName = "Vikram Singh",
            Role = UserRole.Inventory,
            Department = "Logistics & Warehousing",
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddMonths(-6)
        };

        var accountsUser = new User
        {
            Username = "accounts",
            Email = "accounts@arrowarorice.com",
            PasswordHash = accountsHash,
            FullName = "Pooja Verma",
            Role = UserRole.Accounts,
            Department = "Export Finance & Documentation",
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddMonths(-6)
        };

        db.Users.AddRange(adminUser, salesUser, inventoryUser, accountsUser);
        await db.SaveChangesAsync();

        // 2. Seed Warehouses
        var whKarnal = new Warehouse
        {
            Name = "Central Milling Complex & Silos",
            Code = "WH-KRN-01",
            Location = "GT Karnal Road, Taraori Industrial Area",
            City = "Karnal",
            State = "Haryana",
            CapacityMT = 45000,
            ContactPerson = "Baldev Singh",
            Phone = "+91 98120 44550",
            IsActive = true
        };

        var whMundra = new Warehouse
        {
            Name = "Mundra Port CFS Warehouse",
            Code = "WH-MND-02",
            Location = "Near APSEZ CFS Zone, Port Road",
            City = "Mundra",
            State = "Gujarat",
            CapacityMT = 25000,
            ContactPerson = "Kishore Patel",
            Phone = "+91 98250 88210",
            IsActive = true
        };

        var whNhavaSheva = new Warehouse
        {
            Name = "Nhava Sheva CFS Logistics Hub",
            Code = "WH-JNPT-03",
            Location = "Sector 11, Dronagiri Node, Uran",
            City = "Navi Mumbai",
            State = "Maharashtra",
            CapacityMT = 18000,
            ContactPerson = "Sanjay Sawant",
            Phone = "+91 98200 33190",
            IsActive = true
        };

        db.Warehouses.AddRange(whKarnal, whMundra, whNhavaSheva);
        await db.SaveChangesAsync();

        // 3. Seed Products (Premium Basmati & Non-Basmati Rice Varieties)
        var products = new List<Product>
        {
            new()
            {
                Code = "RICE-1121-STM",
                Name = "1121 XXL Steam Basmati Rice",
                Category = "Basmati",
                Variety = "1121 Basmati",
                ProcessingType = "Steam",
                AverageLengthMm = 8.35m,
                MoistureMaxPercent = 12.0m,
                BrokenMaxPercent = 1.0m,
                ChalkyMaxPercent = 1.5m,
                ForeignMatterMaxPercent = 0.05m,
                AdmixtureMaxPercent = 5.0m,
                ElongationRatio = "2.2x - 2.5x",
                AromaGrade = "Rich Natural Aroma",
                KettWhiteness = "33 - 35 Kett",
                PackagingOptions = "1kg, 5kg, 10kg, 20kg, 25kg, 50kg Non-Woven, BOPP, Jute & Poly Pouches",
                MinOrderQuantityMT = 25.0m,
                DefaultFobPricePerMT = 1150.0m,
                Currency = "USD",
                Description = "The crown jewel of Indian Basmati rice. Known worldwide for exceptional grain length (8.35mm+ raw), delightful aroma, and supreme cooking elongation without stickiness. Ideal for royal Biryanis and gourmet hospitality.",
                Highlights = "Average grain length 8.35mm+; 100% Sortex cleaned; Extra long grain elongation; Zero artificial polishing",
                ImageUrl = "/images/1121-steam.jpg",
                IsActive = true,
                IsFeatured = true
            },
            new()
            {
                Code = "RICE-1121-GDS",
                Name = "1121 Golden Sella Basmati Rice",
                Category = "Basmati",
                Variety = "1121 Basmati",
                ProcessingType = "Golden Sella (Parboiled)",
                AverageLengthMm = 8.35m,
                MoistureMaxPercent = 12.5m,
                BrokenMaxPercent = 1.0m,
                ChalkyMaxPercent = 0.5m,
                ForeignMatterMaxPercent = 0.02m,
                AdmixtureMaxPercent = 5.0m,
                ElongationRatio = "2.4x - 2.8x",
                AromaGrade = "Authentic Nutty Aroma",
                KettWhiteness = "26 - 28 Kett (Rich Golden Hue)",
                PackagingOptions = "5kg, 10kg, 20kg, 25kg, 50kg PP & Non-Woven Bags",
                MinOrderQuantityMT = 25.0m,
                DefaultFobPricePerMT = 1190.0m,
                Currency = "USD",
                Description = "Parboiled in husk with specialized warm water immersion and gentle steam pressure that locks all vitamins into the grain, giving it a golden tint and maximum resistance to breakage during heavy catering cooking.",
                Highlights = "High elongation; Hard grain resists breakage; Golden parboiled nutrient seal; Perfect for Persian, Mandi and Kabsa dishes",
                ImageUrl = "/images/1121-golden-sella.jpg",
                IsActive = true,
                IsFeatured = true
            },
            new()
            {
                Code = "RICE-1121-WTS",
                Name = "1121 White / Creamy Sella Basmati Rice",
                Category = "Basmati",
                Variety = "1121 Basmati",
                ProcessingType = "Creamy Sella",
                AverageLengthMm = 8.35m,
                MoistureMaxPercent = 12.5m,
                BrokenMaxPercent = 1.0m,
                ChalkyMaxPercent = 0.5m,
                ForeignMatterMaxPercent = 0.05m,
                AdmixtureMaxPercent = 5.0m,
                ElongationRatio = "2.3x - 2.6x",
                AromaGrade = "Mellow Delicate Aroma",
                KettWhiteness = "30 - 32 Kett",
                PackagingOptions = "10kg, 20kg, 25kg, 50kg Master Cartons & PP Bags",
                MinOrderQuantityMT = 25.0m,
                DefaultFobPricePerMT = 1170.0m,
                Currency = "USD",
                Description = "Creamy parboiled Basmati offering supreme pearl-white luster and firm texture after boiling. Most favored by top catering companies in Saudi Arabia, UAE, and the European Union.",
                Highlights = "Pearl white finish; Fluffy separate grains; Excellent holding time on warming trays",
                ImageUrl = "/images/1121-creamy-sella.jpg",
                IsActive = true,
                IsFeatured = true
            },
            new()
            {
                Code = "RICE-1509-STM",
                Name = "1509 Steam Basmati Rice",
                Category = "Basmati",
                Variety = "1509 Basmati",
                ProcessingType = "Steam",
                AverageLengthMm = 8.40m,
                MoistureMaxPercent = 12.0m,
                BrokenMaxPercent = 1.5m,
                ChalkyMaxPercent = 2.0m,
                ForeignMatterMaxPercent = 0.05m,
                AdmixtureMaxPercent = 5.0m,
                ElongationRatio = "2.1x - 2.4x",
                AromaGrade = "Pleasant Basmati Fragrance",
                KettWhiteness = "34 - 36 Kett",
                PackagingOptions = "25kg, 50kg PP Bags, BOPP Bags",
                MinOrderQuantityMT = 25.0m,
                DefaultFobPricePerMT = 980.0m,
                Currency = "USD",
                Description = "Remarkable cost-performance ratio among extra long grain Basmati varieties. Offers an impressive grain length comparable to 1121 with fast cooking time and light mouthfeel.",
                Highlights = "Ultra-long grain length 8.40mm+; Economical high-volume commercial choice; Pure white luster",
                ImageUrl = "/images/1509-steam.jpg",
                IsActive = true,
                IsFeatured = true
            },
            new()
            {
                Code = "RICE-TRD-RAW",
                Name = "Traditional Aged Raw Basmati Rice",
                Category = "Basmati",
                Variety = "Traditional Pure Basmati",
                ProcessingType = "Raw / Milled (Aged 2 Years)",
                AverageLengthMm = 7.40m,
                MoistureMaxPercent = 11.5m,
                BrokenMaxPercent = 1.0m,
                ChalkyMaxPercent = 2.0m,
                ForeignMatterMaxPercent = 0.02m,
                AdmixtureMaxPercent = 3.0m,
                ElongationRatio = "2.2x - 2.5x",
                AromaGrade = "Intense Vintage Aroma (Heritage APEDA certified)",
                KettWhiteness = "36 - 38 Kett",
                PackagingOptions = "1kg, 2kg, 5kg, 10kg Jute & Velvet Gift Bags, Retail Pouches",
                MinOrderQuantityMT = 20.0m,
                DefaultFobPricePerMT = 1450.0m,
                Currency = "USD",
                Description = "Harvested from the pristine Himalayan foothills of Punjab and Haryana, naturally aged for up to 24 months in temperature controlled silos to maximize essential aroma oils and cooking elongation.",
                Highlights = "Naturally aged 2 years; Unmatched nostalgic aroma; Heritage pedigree; Premium gift & retail packaging",
                ImageUrl = "/images/traditional-raw.jpg",
                IsActive = true,
                IsFeatured = true
            },
            new()
            {
                Code = "RICE-SUG-STM",
                Name = "Sugandha Steam Basmati Rice",
                Category = "Basmati",
                Variety = "Sugandha",
                ProcessingType = "Steam",
                AverageLengthMm = 7.80m,
                MoistureMaxPercent = 12.5m,
                BrokenMaxPercent = 2.0m,
                ChalkyMaxPercent = 2.0m,
                ForeignMatterMaxPercent = 0.05m,
                AdmixtureMaxPercent = 5.0m,
                ElongationRatio = "2.0x - 2.2x",
                AromaGrade = "Sweet Aromatic",
                KettWhiteness = "32 - 34 Kett",
                PackagingOptions = "25kg, 50kg PP Bags",
                MinOrderQuantityMT = 25.0m,
                DefaultFobPricePerMT = 860.0m,
                Currency = "USD",
                Description = "Popular semi-long grain aromatic rice variety that delivers the scent and presentation of Basmati at an attractive commercial price point. Very popular in Gulf and African wholesale markets.",
                Highlights = "Strong aroma; Attractive price point; High bulk volume availability",
                ImageUrl = "/images/sugandha-steam.jpg",
                IsActive = true,
                IsFeatured = false
            },
            new()
            {
                Code = "RICE-SHR-STM",
                Name = "Sharbati White Sella Basmati Rice",
                Category = "Basmati",
                Variety = "Sharbati",
                ProcessingType = "Creamy Sella",
                AverageLengthMm = 7.15m,
                MoistureMaxPercent = 12.5m,
                BrokenMaxPercent = 2.0m,
                ChalkyMaxPercent = 1.0m,
                ForeignMatterMaxPercent = 0.05m,
                AdmixtureMaxPercent = 5.0m,
                ElongationRatio = "2.0x",
                AromaGrade = "Mild Aroma",
                KettWhiteness = "30 - 32 Kett",
                PackagingOptions = "25kg, 50kg PP Bags",
                MinOrderQuantityMT = 25.0m,
                DefaultFobPricePerMT = 780.0m,
                Currency = "USD",
                Description = "A hybrid variety sister to Basmati, with high grain elongation upon cooking and soft texture. Highly preferred for food processing, institutional cafeterias, and mid-range dining.",
                Highlights = "Budget friendly; Fluffy grains; High yield per cooked kilogram",
                ImageUrl = "/images/sharbati-sella.jpg",
                IsActive = true,
                IsFeatured = false
            },
            new()
            {
                Code = "RICE-SNM-RAW",
                Name = "Sona Masoori Raw Milled Rice",
                Category = "Non-Basmati",
                Variety = "Sona Masoori",
                ProcessingType = "Raw / Silky Polish",
                AverageLengthMm = 5.20m,
                MoistureMaxPercent = 13.0m,
                BrokenMaxPercent = 3.0m,
                ChalkyMaxPercent = 3.0m,
                ForeignMatterMaxPercent = 0.10m,
                AdmixtureMaxPercent = 5.0m,
                ElongationRatio = "1.8x",
                AromaGrade = "Neutral Fresh Grain",
                KettWhiteness = "38 - 42 Kett",
                PackagingOptions = "10kg, 20kg, 25kg Non-Woven & BOPP Bags",
                MinOrderQuantityMT = 25.0m,
                DefaultFobPricePerMT = 690.0m,
                Currency = "USD",
                Description = "Lightweight, low-starch medium grain rice grown primarily in the fertile river basins of South India. Easy to digest and loved by South Asian diaspora across the US, UK, Canada, and Australia.",
                Highlights = "Low starch & easy digestion; Silky optical sortex polish; Premium household staple",
                ImageUrl = "/images/sona-masoori.jpg",
                IsActive = true,
                IsFeatured = true
            },
            new()
            {
                Code = "RICE-IR64-PB5",
                Name = "IR64 Parboiled Rice (5% Broken)",
                Category = "Non-Basmati",
                Variety = "IR64",
                ProcessingType = "Parboiled / Double Polished",
                AverageLengthMm = 6.10m,
                MoistureMaxPercent = 13.5m,
                BrokenMaxPercent = 5.0m,
                ChalkyMaxPercent = 3.0m,
                ForeignMatterMaxPercent = 0.15m,
                AdmixtureMaxPercent = 5.0m,
                ElongationRatio = "1.7x",
                AromaGrade = "Standard Grain",
                KettWhiteness = "30 - 32 Kett",
                PackagingOptions = "25kg, 50kg PP Sacks / Container Bulk Liner",
                MinOrderQuantityMT = 50.0m,
                DefaultFobPricePerMT = 510.0m,
                Currency = "USD",
                Description = "The undisputed heavyweight of international non-basmati bulk rice trade. Huge volume exports to West Africa, Southeast Asia, South Africa, and Middle Eastern import tenders.",
                Highlights = "High energy nutrition; Double polished sortex clean; Bulk vessel & container loading capability",
                ImageUrl = "/images/ir64-parboiled.jpg",
                IsActive = true,
                IsFeatured = true
            },
            new()
            {
                Code = "RICE-PR11-STM",
                Name = "PR11 / PR14 Steam Non-Basmati Rice",
                Category = "Non-Basmati",
                Variety = "PR11 / 14",
                ProcessingType = "Steam",
                AverageLengthMm = 6.80m,
                MoistureMaxPercent = 12.5m,
                BrokenMaxPercent = 3.0m,
                ChalkyMaxPercent = 2.0m,
                ForeignMatterMaxPercent = 0.10m,
                AdmixtureMaxPercent = 5.0m,
                ElongationRatio = "1.9x",
                AromaGrade = "Mild Grain",
                KettWhiteness = "34 - 36 Kett",
                PackagingOptions = "25kg, 50kg PP Bags",
                MinOrderQuantityMT = 25.0m,
                DefaultFobPricePerMT = 580.0m,
                Currency = "USD",
                Description = "Long grain non-basmati variety that offers appearance and cooking length nearing Basmati rice, making it a very popular export to Russia, Central Asia, and Africa.",
                Highlights = "Long slender grain 6.80mm; Non-sticky cooking; Competitive pricing",
                ImageUrl = "/images/pr11-steam.jpg",
                IsActive = true,
                IsFeatured = false
            },
            new()
            {
                Code = "RICE-BRK-100",
                Name = "100% Broken White Rice (Brewery / Starch)",
                Category = "Broken",
                Variety = "Broken Grain",
                ProcessingType = "Raw Milled Optical Cleaned",
                AverageLengthMm = 2.50m,
                MoistureMaxPercent = 14.0m,
                BrokenMaxPercent = 100.0m,
                ChalkyMaxPercent = 6.0m,
                ForeignMatterMaxPercent = 0.20m,
                AdmixtureMaxPercent = 5.0m,
                ElongationRatio = "N/A",
                AromaGrade = "Neutral",
                KettWhiteness = "35 - 38 Kett",
                PackagingOptions = "50kg PP Sacks / Jumbo 1-Ton Bags",
                MinOrderQuantityMT = 100.0m,
                DefaultFobPricePerMT = 380.0m,
                Currency = "USD",
                Description = "Clean, destoned, sortex-screened broken rice grains ideal for industrial food manufacturing, baby food porridge, beer breweries, rice noodles, and starch manufacturing plants.",
                Highlights = "Destoned & metal free; High starch content; Jumbo bag packaging available",
                ImageUrl = "/images/broken-rice.jpg",
                IsActive = true,
                IsFeatured = false
            }
        };

        db.Products.AddRange(products);
        await db.SaveChangesAsync();

        // 4. Seed Inventory Stock across warehouses
        var stocks = new List<InventoryStock>
        {
            new() { WarehouseId = whKarnal.Id, ProductId = products[0].Id, QuantityMT = 1200.0m, ReservedMT = 150.0m, BatchNumber = "KRN-1121S-2601", MoistureTestedPercent = 11.8m, PackagingType = "50kg PP Bags", ReorderLevelMT = 100m, LastUpdated = DateTime.UtcNow },
            new() { WarehouseId = whMundra.Id, ProductId = products[0].Id, QuantityMT = 450.0m, ReservedMT = 100.0m, BatchNumber = "MND-1121S-2602", MoistureTestedPercent = 12.0m, PackagingType = "50kg PP Bags", ReorderLevelMT = 50m, LastUpdated = DateTime.UtcNow },
            new() { WarehouseId = whKarnal.Id, ProductId = products[1].Id, QuantityMT = 850.0m, ReservedMT = 50.0m, BatchNumber = "KRN-1121G-2601", MoistureTestedPercent = 12.2m, PackagingType = "50kg PP Bags", ReorderLevelMT = 75m, LastUpdated = DateTime.UtcNow },
            new() { WarehouseId = whMundra.Id, ProductId = products[1].Id, QuantityMT = 320.0m, ReservedMT = 0m, BatchNumber = "MND-1121G-2603", MoistureTestedPercent = 12.1m, PackagingType = "50kg PP Bags", ReorderLevelMT = 50m, LastUpdated = DateTime.UtcNow },
            new() { WarehouseId = whKarnal.Id, ProductId = products[3].Id, QuantityMT = 600.0m, ReservedMT = 25.0m, BatchNumber = "KRN-1509S-2601", MoistureTestedPercent = 11.9m, PackagingType = "25kg PP Bags", ReorderLevelMT = 60m, LastUpdated = DateTime.UtcNow },
            new() { WarehouseId = whNhavaSheva.Id, ProductId = products[7].Id, QuantityMT = 950.0m, ReservedMT = 200.0m, BatchNumber = "JNPT-SNM-2601", MoistureTestedPercent = 12.8m, PackagingType = "20kg Non-Woven", ReorderLevelMT = 80m, LastUpdated = DateTime.UtcNow },
            new() { WarehouseId = whNhavaSheva.Id, ProductId = products[8].Id, QuantityMT = 2400.0m, ReservedMT = 500.0m, BatchNumber = "JNPT-IR64-2601", MoistureTestedPercent = 13.2m, PackagingType = "50kg PP Bags", ReorderLevelMT = 200m, LastUpdated = DateTime.UtcNow }
        };
        db.InventoryStocks.AddRange(stocks);
        await db.SaveChangesAsync();

        // 5. Seed International Customers (Importers & Commodity Traders)
        var customers = new List<Customer>
        {
            new()
            {
                CompanyName = "Al-Barakah General Trading LLC",
                ContactPerson = "Sheikh Tariq Al-Nuaimi",
                Email = "tariq@albarakahtrading.ae",
                Phone = "+971 4 398 2210",
                WhatsApp = "+971 50 123 4567",
                Country = "United Arab Emirates",
                City = "Dubai",
                Address = "Al Ras Wholesale Food Market, Deira",
                TaxOrVatNumber = "AE-TRN-1002938475",
                ImportLicenseNumber = "DXB-IMP-88192",
                PreferredDestinationPort = "Jebel Ali Port (AEJEA)",
                Status = "Active Buyer",
                CreditLimitUsd = 250000m,
                PaymentTerms = "30% Advance + 70% against BL Scan",
                Notes = "Major distributor of premium Basmati across the UAE and Oman. Orders 4-6 containers monthly.",
                CreatedAt = DateTime.UtcNow.AddMonths(-5)
            },
            new()
            {
                CompanyName = "Al-Khaleej Foodstuffs Est.",
                ContactPerson = "Fahad Al-Otaibi",
                Email = "fahad@alkhaleejfoods.com.sa",
                Phone = "+966 11 482 9901",
                WhatsApp = "+966 55 981 2234",
                Country = "Saudi Arabia",
                City = "Riyadh",
                Address = "Al-Sulay Food Logistics District",
                TaxOrVatNumber = "SA-VAT-3001827461",
                ImportLicenseNumber = "SFDA-IMP-55410",
                PreferredDestinationPort = "Jeddah Islamic Port (SAJED)",
                Status = "Active Buyer",
                CreditLimitUsd = 400000m,
                PaymentTerms = "100% Irrevocable LC at Sight",
                Notes = "Specializes in 1121 Golden Sella and 1121 Creamy Sella for Mandi and Kabsa restaurants.",
                CreatedAt = DateTime.UtcNow.AddMonths(-4)
            },
            new()
            {
                CompanyName = "EuroGrains GmbH & Co. KG",
                ContactPerson = "Dr. Markus Weber",
                Email = "m.weber@eurograins-hamburg.de",
                Phone = "+49 40 380 1190",
                WhatsApp = "+49 171 445 9912",
                Country = "Germany",
                City = "Hamburg",
                Address = "Speicherstadt Logistics Terminal, Brooktorkai 18",
                TaxOrVatNumber = "DE-814920381",
                ImportLicenseNumber = "EU-EORI-DE991827",
                PreferredDestinationPort = "Port of Hamburg (DEHAM)",
                Status = "Active Buyer",
                CreditLimitUsd = 300000m,
                PaymentTerms = "CAD (Cash Against Documents) via Deutsche Bank",
                Notes = "Requires EU pesticide MRL compliance lab certificate and DNA Basmati purity certificate > 95%.",
                CreatedAt = DateTime.UtcNow.AddMonths(-3)
            },
            new()
            {
                CompanyName = "Orient Foodstuffs Import Corp",
                ContactPerson = "David Henderson",
                Email = "d.henderson@orientfoodsus.com",
                Phone = "+1 713 550 8820",
                WhatsApp = "+1 713 550 8820",
                Country = "United States",
                City = "Houston",
                Address = "8820 Westpark Dr, Suite 400, Houston TX 77063",
                TaxOrVatNumber = "US-EIN-74-8891024",
                ImportLicenseNumber = "FDA-REG-19283746",
                PreferredDestinationPort = "Port of Houston (USHOU)",
                Status = "Active Buyer",
                CreditLimitUsd = 500000m,
                PaymentTerms = "30% Advance + 70% against BL",
                Notes = "Distributes Sona Masoori and 1121 Basmati to Indian supermarkets across North America.",
                CreatedAt = DateTime.UtcNow.AddMonths(-3)
            },
            new()
            {
                CompanyName = "Lion City Agro Commodities Pte Ltd",
                ContactPerson = "Tan Wei Ming",
                Email = "weiming@lioncityagro.sg",
                Phone = "+65 6789 2211",
                WhatsApp = "+65 9123 4567",
                Country = "Singapore",
                City = "Singapore",
                Address = "Jurong Port Road, Warehouse Complex 4",
                TaxOrVatNumber = "SG-UEN-201899124K",
                ImportLicenseNumber = "SFA-IMP-3381",
                PreferredDestinationPort = "Port of Singapore (SGSIN)",
                Status = "Active Buyer",
                CreditLimitUsd = 200000m,
                PaymentTerms = "100% LC at sight",
                Notes = "Imports IR64, Sona Masoori and PR11 for regional distribution across ASEAN.",
                CreatedAt = DateTime.UtcNow.AddMonths(-2)
            }
        };

        db.Customers.AddRange(customers);
        await db.SaveChangesAsync();

        // 6. Seed Inquiries
        var inquiries = new List<Inquiry>
        {
            new()
            {
                InquiryNumber = "INQ-2026-001",
                CustomerId = customers[0].Id,
                FullName = "Sheikh Tariq Al-Nuaimi",
                CompanyName = "Al-Barakah General Trading LLC",
                Email = "tariq@albarakahtrading.ae",
                Phone = "+971 4 398 2210",
                Country = "United Arab Emirates",
                RiceVariety = "1121 Steam Basmati Rice",
                ProductId = products[0].Id,
                QuantityMT = 50.0m,
                DestinationPort = "Jebel Ali Port (AEJEA)",
                IncoTerm = "CIF",
                PreferredPackaging = "50 Kg Non-Woven Bags with buyer private logo",
                Message = "Looking for urgent CIF Jebel Ali quotation for 2x20ft FCL (50 MT) 1121 Steam Basmati. Need SGS pre-shipment quality certificate. Please confirm earliest shipping vessel schedule.",
                Status = InquiryStatus.Converted,
                Priority = InquiryPriority.High,
                AssignedToUserId = salesUser.Id,
                FollowUpDate = DateTime.UtcNow.AddDays(-2),
                InternalNotes = "Quotation accepted and converted into Sales Order SO-2026-001.",
                CreatedAt = DateTime.UtcNow.AddDays(-20)
            },
            new()
            {
                InquiryNumber = "INQ-2026-002",
                CustomerId = customers[1].Id,
                FullName = "Fahad Al-Otaibi",
                CompanyName = "Al-Khaleej Foodstuffs Est.",
                Email = "fahad@alkhaleejfoods.com.sa",
                Phone = "+966 11 482 9901",
                Country = "Saudi Arabia",
                RiceVariety = "1121 Golden Sella Basmati Rice",
                ProductId = products[1].Id,
                QuantityMT = 100.0m,
                DestinationPort = "Jeddah Islamic Port (SAJED)",
                IncoTerm = "CIF",
                PreferredPackaging = "40 Kg PP Bags (Standard Saudi Packing)",
                Message = "Require CIF Jeddah quotation for 4 FCL containers (100 MT) 1121 Golden Sella. Payment by 100% Irrevocable LC from Al-Rajhi Bank. Please send crop certificate and moisture guarantee.",
                Status = InquiryStatus.QuotationSent,
                Priority = InquiryPriority.Urgent,
                AssignedToUserId = salesUser.Id,
                FollowUpDate = DateTime.UtcNow.AddDays(2),
                InternalNotes = "Quotation QT-2026-002 dispatched. Buyer reviewing commercial terms with bank.",
                CreatedAt = DateTime.UtcNow.AddDays(-10)
            },
            new()
            {
                InquiryNumber = "INQ-2026-003",
                CustomerId = customers[2].Id,
                FullName = "Dr. Markus Weber",
                CompanyName = "EuroGrains GmbH & Co. KG",
                Email = "m.weber@eurograins-hamburg.de",
                Phone = "+49 40 380 1190",
                Country = "Germany",
                RiceVariety = "Traditional Aged Raw Basmati Rice",
                ProductId = products[4].Id,
                QuantityMT = 25.0m,
                DestinationPort = "Port of Hamburg (DEHAM)",
                IncoTerm = "CIF",
                PreferredPackaging = "10 Kg Master Carton with Jute Retail Bags",
                Message = "Seeking 1x20' FCL Traditional Raw Basmati. Strictly zero tricyclazole residue, compliant with current European Commission limits. Require Eurofins laboratory analysis test report before container stuffing.",
                Status = InquiryStatus.SampleSent,
                Priority = InquiryPriority.High,
                AssignedToUserId = salesUser.Id,
                FollowUpDate = DateTime.UtcNow.AddDays(4),
                InternalNotes = "DHL air courier tracking sent for 2kg lab sample lot.",
                CreatedAt = DateTime.UtcNow.AddDays(-6)
            },
            new()
            {
                InquiryNumber = "INQ-2026-004",
                CustomerId = customers[3].Id,
                FullName = "David Henderson",
                CompanyName = "Orient Foodstuffs Import Corp",
                Email = "d.henderson@orientfoodsus.com",
                Phone = "+1 713 550 8820",
                Country = "United States",
                RiceVariety = "Sona Masoori Raw Milled Rice",
                ProductId = products[7].Id,
                QuantityMT = 50.0m,
                DestinationPort = "Port of Houston (USHOU)",
                IncoTerm = "CIF",
                PreferredPackaging = "20 Lb (9.07 Kg) BOPP Bags with barcode and nutrition panel",
                Message = "Quote CIF Houston for 2 containers Sona Masoori silky polished. Must be registered with US FDA prior notice and fumigated with aluminum phosphide.",
                Status = InquiryStatus.InNegotiation,
                Priority = InquiryPriority.Normal,
                AssignedToUserId = salesUser.Id,
                FollowUpDate = DateTime.UtcNow.AddDays(1),
                InternalNotes = "Buyer requested 2% discount on bulk 50 MT order. Discussed with management.",
                CreatedAt = DateTime.UtcNow.AddDays(-4)
            },
            new()
            {
                InquiryNumber = "INQ-2026-005",
                FullName = "Kareem El-Sayed",
                CompanyName = "Nile Delta Grains Cairo",
                Email = "kareem@niledeltagrains.com",
                Phone = "+20 2 2459 1100",
                Country = "Egypt",
                RiceVariety = "IR64 Parboiled Rice (5% Broken)",
                ProductId = products[8].Id,
                QuantityMT = 250.0m,
                DestinationPort = "Alexandria Port (EGALY)",
                IncoTerm = "CFR",
                PreferredPackaging = "50 Kg Polypropylene Sacks",
                Message = "Looking for immediate CFR Alexandria rate for 10 FCL containers (250 MT) of Indian IR64 Parboiled 5% broken. Send specification sheet and SGS inspection terms.",
                Status = InquiryStatus.New,
                Priority = InquiryPriority.Urgent,
                AssignedToUserId = salesUser.Id,
                FollowUpDate = DateTime.UtcNow.AddDays(1),
                InternalNotes = "New inquiry from website RFQ portal. Requires immediate price calculation.",
                CreatedAt = DateTime.UtcNow.AddHours(-5)
            }
        };

        db.Inquiries.AddRange(inquiries);
        await db.SaveChangesAsync();

        // 7. Seed Quotations
        var quotation1 = new Quotation
        {
            QuotationNumber = "QT-2026-001",
            CustomerId = customers[0].Id,
            InquiryId = inquiries[0].Id,
            CustomerName = customers[0].CompanyName,
            CustomerEmail = customers[0].Email,
            CustomerCountry = customers[0].Country,
            PortOfLoading = "Mundra Port, Gujarat, India",
            DestinationPort = "Jebel Ali Port (AEJEA), Dubai, UAE",
            IncoTerm = "CIF",
            Currency = "USD",
            SubTotal = 57500.0m,
            FreightCharges = 2400.0m,
            InsuranceCharges = 350.0m,
            OtherCharges = 250.0m,
            Discount = 500.0m,
            GrandTotal = 60000.0m,
            Status = QuotationStatus.Accepted,
            QuotationDate = DateTime.UtcNow.AddDays(-18),
            ValidUntil = DateTime.UtcNow.AddDays(12),
            PaymentTerms = "30% Advance T/T upon order confirmation, 70% against scanned copy of original Bill of Lading",
            InspectionAgency = "SGS India pre-shipment container inspection at Mundra Port (Certificate of Quality & Quantity)",
            TermsAndConditions = "1. Price includes export seaworthy 50kg non-woven packaging.\n2. Shipment within 21 days from receipt of advance remittance.\n3. Tolerance of +/- 5% in quantity and weight at seller's option.\n4. Subject to Force Majeure as per standard GAFTA rules.",
            BankDetails = "Beneficiary: ARROW ARO RICE EXPORTS PVT LTD\nBank: State Bank of India, Overseas Branch, Mumbai\nAccount No: 39182940192\nSWIFT Code: SBININBB104\nIFSC: SBIN0001040",
            CreatedByUserId = salesUser.Id,
            CreatedAt = DateTime.UtcNow.AddDays(-18),
            Items = new List<QuotationItem>
            {
                new()
                {
                    ProductId = products[0].Id,
                    ProductName = "1121 XXL Steam Basmati Rice",
                    SpecificationsSummary = "8.35mm AGL, 12% Moisture max, 1% Broken max, Sortex Cleaned",
                    QuantityMT = 50.0m,
                    UnitPricePerMT = 1150.0m,
                    TotalAmount = 57500.0m,
                    Packaging = "50 Kg Non-Woven Bags with Buyer Logo"
                }
            }
        };

        var quotation2 = new Quotation
        {
            QuotationNumber = "QT-2026-002",
            CustomerId = customers[1].Id,
            InquiryId = inquiries[1].Id,
            CustomerName = customers[1].CompanyName,
            CustomerEmail = customers[1].Email,
            CustomerCountry = customers[1].Country,
            PortOfLoading = "Mundra Port, Gujarat, India",
            DestinationPort = "Jeddah Islamic Port (SAJED), Saudi Arabia",
            IncoTerm = "CIF",
            Currency = "USD",
            SubTotal = 119000.0m,
            FreightCharges = 4800.0m,
            InsuranceCharges = 650.0m,
            OtherCharges = 400.0m,
            Discount = 850.0m,
            GrandTotal = 124000.0m,
            Status = QuotationStatus.SentToBuyer,
            QuotationDate = DateTime.UtcNow.AddDays(-8),
            ValidUntil = DateTime.UtcNow.AddDays(7),
            PaymentTerms = "100% Irrevocable Letter of Credit (LC) payable at sight from prime international bank",
            InspectionAgency = "Bureau Veritas container stuffing and pesticide residue test",
            TermsAndConditions = "1. Packing in standard 40 Kg PP bags with Arabic & English bilingual labeling conforming to SFDA standards.\n2. LC must allow transhipment and partial shipments.",
            BankDetails = "Beneficiary: ARROW ARO RICE EXPORTS PVT LTD\nBank: ICICI Bank Ltd, International Trade Branch, New Delhi\nSWIFT: ICICINBB001",
            CreatedByUserId = salesUser.Id,
            CreatedAt = DateTime.UtcNow.AddDays(-8),
            Items = new List<QuotationItem>
            {
                new()
                {
                    ProductId = products[1].Id,
                    ProductName = "1121 Golden Sella Basmati Rice",
                    SpecificationsSummary = "8.35mm AGL, 12.5% Moisture max, 1% Broken max, Rich Golden Parboiled",
                    QuantityMT = 100.0m,
                    UnitPricePerMT = 1190.0m,
                    TotalAmount = 119000.0m,
                    Packaging = "40 Kg PP Bags (SFDA Approved)"
                }
            }
        };

        db.Quotations.AddRange(quotation1, quotation2);
        await db.SaveChangesAsync();

        // 8. Seed Sales Orders
        var salesOrder1 = new SalesOrder
        {
            OrderNumber = "SO-2026-001",
            QuotationId = quotation1.Id,
            CustomerId = customers[0].Id,
            OrderDate = DateTime.UtcNow.AddDays(-15),
            ExpectedShipmentDate = DateTime.UtcNow.AddDays(5),
            PortOfLoading = "Mundra Port, Gujarat (INMUN1)",
            PortOfDestination = "Jebel Ali Port (AEJEA), Dubai, UAE",
            IncoTerm = "CIF",
            PaymentTerms = "30% Advance T/T, 70% against BL Scan",
            Currency = "USD",
            TotalQuantityMT = 50.0m,
            TotalAmount = 60000.0m,
            Status = SalesOrderStatus.Shipped,
            BuyerPoReference = "ABG-PO-2026-881",
            SpecialInstructions = "Ensure fumigation with Methyl Bromide for 48 hours prior to vessel departure. Original documents to be sent via DHL to Dubai head office.",
            CreatedAt = DateTime.UtcNow.AddDays(-15),
            Items = new List<SalesOrderItem>
            {
                new()
                {
                    ProductId = products[0].Id,
                    ProductName = "1121 XXL Steam Basmati Rice",
                    QuantityMT = 50.0m,
                    UnitPricePerMT = 1150.0m,
                    TotalAmount = 57500.0m,
                    Packaging = "50 Kg Non-Woven Bags"
                }
            }
        };

        db.SalesOrders.Add(salesOrder1);
        await db.SaveChangesAsync();

        // Link quotation back
        quotation1.ConvertedSalesOrder = salesOrder1;
        await db.SaveChangesAsync();

        // 9. Seed Export Shipment for Sales Order 1
        var shipment1 = new ExportShipment
        {
            ShipmentNumber = "SH-2026-001",
            SalesOrderId = salesOrder1.Id,
            BookingNumber = "MAEU-DXB-981042",
            ShippingLine = "Maersk Line (A.P. Moller - Maersk)",
            VesselName = "MAERSK BROOKLYN",
            VoyageNumber = "V.2604W",
            ContainerNumbers = "MSKU7821941 (25 MT), MSKU7821942 (25 MT)",
            SealNumbers = "ML-IN-98124, ML-IN-98125",
            BillOfLadingNumber = "BL-MND-DXB-98104",
            PortOfLoading = "Mundra Port (INMUN1)",
            PortOfDestination = "Jebel Ali Port, Dubai (AEJEA)",
            EstimatedDeparture = DateTime.UtcNow.AddDays(-5),
            ActualDeparture = DateTime.UtcNow.AddDays(-5),
            EstimatedArrival = DateTime.UtcNow.AddDays(4),
            CurrentMilestone = ShipmentMilestone.InTransit,
            StatusDescription = "Vessel Sailing in Arabian Sea - ETA Jebel Ali in 4 Days",
            TrackingNotes = "Customs cleared at Mundra Port CFS on schedule. Container loading and fumigation completed under SGS supervision.",
            CreatedAt = DateTime.UtcNow.AddDays(-10),
            Documents = new List<ShipmentDocument>
            {
                new()
                {
                    DocType = DocumentType.CommercialInvoice,
                    Title = "Export Commercial Invoice (Original)",
                    DocumentNumber = "INV-2026-001",
                    IssuedDate = DateTime.UtcNow.AddDays(-6),
                    FilePath = "/documents/INV-2026-001.pdf",
                    IsVerified = true,
                    IssuingAuthority = "ARROW ARO RICE EXPORTS PVT LTD",
                    Remarks = "Certified true copy for customs & bank clearance."
                },
                new()
                {
                    DocType = DocumentType.PackingList,
                    Title = "Detailed Container Packing List",
                    DocumentNumber = "PL-2026-001",
                    IssuedDate = DateTime.UtcNow.AddDays(-6),
                    FilePath = "/documents/PL-2026-001.pdf",
                    IsVerified = true,
                    IssuingAuthority = "ARROW ARO RICE Logistics Division",
                    Remarks = "1,000 bags of 50kg net weight each across 2x20' FCL containers."
                },
                new()
                {
                    DocType = DocumentType.CertificateOfOrigin,
                    Title = "Certificate of Origin (Non-Preferential)",
                    DocumentNumber = "COO-IN-2026-4410",
                    IssuedDate = DateTime.UtcNow.AddDays(-5),
                    FilePath = "/documents/COO-IN-2026-4410.pdf",
                    IsVerified = true,
                    IssuingAuthority = "Export Inspection Council & Chamber of Commerce India",
                    Remarks = "Attested and stamped."
                },
                new()
                {
                    DocType = DocumentType.PhytosanitaryCertificate,
                    Title = "Plant Quarantine Phytosanitary Certificate",
                    DocumentNumber = "PHYTO-IND-99182",
                    IssuedDate = DateTime.UtcNow.AddDays(-5),
                    FilePath = "/documents/PHYTO-IND-99182.pdf",
                    IsVerified = true,
                    IssuingAuthority = "Directorate of Plant Protection, Quarantine & Storage, Govt. of India",
                    Remarks = "Free from injurious pests and weed seeds."
                },
                new()
                {
                    DocType = DocumentType.SGSInspectionCertificate,
                    Title = "SGS Quality & Container Inspection Report",
                    DocumentNumber = "SGS-MND-88219",
                    IssuedDate = DateTime.UtcNow.AddDays(-5),
                    FilePath = "/documents/SGS-MND-88219.pdf",
                    IsVerified = true,
                    IssuingAuthority = "SGS India Pvt. Ltd.",
                    Remarks = "Average length verified at 8.38mm, moisture 11.9%, broken 0.8%."
                },
                new()
                {
                    DocType = DocumentType.BillOfLading,
                    Title = "Ocean Clean on Board Bill of Lading",
                    DocumentNumber = "BL-MND-DXB-98104",
                    IssuedDate = DateTime.UtcNow.AddDays(-5),
                    FilePath = "/documents/BL-MND-DXB-98104.pdf",
                    IsVerified = true,
                    IssuingAuthority = "Maersk Shipping Line",
                    Remarks = "Original 3/3 clean on board signed."
                }
            }
        };

        db.ExportShipments.Add(shipment1);
        await db.SaveChangesAsync();

        // 10. Seed Commercial Invoice & Advance Payment for Sales Order 1
        var invoice1 = new Invoice
        {
            InvoiceNumber = "INV-2026-001",
            SalesOrderId = salesOrder1.Id,
            CustomerId = customers[0].Id,
            InvoiceDate = DateTime.UtcNow.AddDays(-14),
            DueDate = DateTime.UtcNow.AddDays(16),
            Currency = "USD",
            SubTotal = 57500.0m,
            TaxOrDuty = 0m, // Zero rated export
            GrandTotal = 60000.0m, // Including freight and insurance
            PaidAmount = 18000.0m, // 30% advance received
            Status = "PartiallyPaid",
            BankSwiftDetails = "State Bank of India Overseas Branch | SWIFT: SBININBB104",
            Notes = "30% Advance ($18,000) received on confirmation. 70% Balance ($42,000) payable against Bill of Lading scan.",
            CreatedAt = DateTime.UtcNow.AddDays(-14),
            Payments = new List<PaymentReceipt>
            {
                new()
                {
                    ReceiptNumber = "RCT-2026-001",
                    CustomerId = customers[0].Id,
                    PaymentDate = DateTime.UtcNow.AddDays(-13),
                    Amount = 18000.0m,
                    Currency = "USD",
                    PaymentMethod = PaymentMethod.WireTransferTT,
                    TransactionReference = "SWIFT-ENBD-AE-992819",
                    BankName = "Emirates NBD Bank Dubai",
                    Notes = "30% Advance payment for SO-2026-001"
                }
            }
        };

        db.Invoices.Add(invoice1);
        await db.SaveChangesAsync();

        // 11. Seed Order Expenses for Order-wise Profitability Tracking
        var expenses1 = new List<OrderExpense>
        {
            new()
            {
                SalesOrderId = salesOrder1.Id,
                Category = ExpenseCategory.OceanFreight,
                Amount = 2200.0m,
                Currency = "USD",
                VendorOrPayee = "Maersk Line Agency India",
                InvoiceOrReceiptNumber = "MND-FRT-9810",
                ExpenseDate = DateTime.UtcNow.AddDays(-6),
                Remarks = "Ocean freight for 2x20' FCL Mundra to Jebel Ali"
            },
            new()
            {
                SalesOrderId = salesOrder1.Id,
                Category = ExpenseCategory.InlandTransport,
                Amount = 850.0m,
                Currency = "USD",
                VendorOrPayee = "Northern Express Cargo Trailers",
                InvoiceOrReceiptNumber = "NET-KRN-MND-441",
                ExpenseDate = DateTime.UtcNow.AddDays(-8),
                Remarks = "Trailer transport from Karnal processing plant to Mundra CFS"
            },
            new()
            {
                SalesOrderId = salesOrder1.Id,
                Category = ExpenseCategory.CustomsClearingCHA,
                Amount = 400.0m,
                Currency = "USD",
                VendorOrPayee = "Mundra Customs House Logistics Agents",
                InvoiceOrReceiptNumber = "CHA-MND-221",
                ExpenseDate = DateTime.UtcNow.AddDays(-6),
                Remarks = "Customs documentation, shipping bill filing, and port gate pass"
            },
            new()
            {
                SalesOrderId = salesOrder1.Id,
                Category = ExpenseCategory.SGSInspectionFee,
                Amount = 350.0m,
                Currency = "USD",
                VendorOrPayee = "SGS India Pvt Ltd",
                InvoiceOrReceiptNumber = "SGS-INV-4418",
                ExpenseDate = DateTime.UtcNow.AddDays(-5),
                Remarks = "Pre-shipment laboratory analysis and container stuffing supervision"
            },
            new()
            {
                SalesOrderId = salesOrder1.Id,
                Category = ExpenseCategory.FumigationFee,
                Amount = 180.0m,
                Currency = "USD",
                VendorOrPayee = "Gujarat Agro Fumigation Services",
                InvoiceOrReceiptNumber = "GAF-2026-109",
                ExpenseDate = DateTime.UtcNow.AddDays(-5),
                Remarks = "Methyl Bromide 48-hour container fumigation with government certificate"
            },
            new()
            {
                SalesOrderId = salesOrder1.Id,
                Category = ExpenseCategory.MarineInsurance,
                Amount = 250.0m,
                Currency = "USD",
                VendorOrPayee = "New India Assurance Co Ltd",
                InvoiceOrReceiptNumber = "POL-MAR-99182",
                ExpenseDate = DateTime.UtcNow.AddDays(-6),
                Remarks = "Institute Cargo Clauses (A) all risk marine coverage"
            }
        };

        db.OrderExpenses.AddRange(expenses1);
        await db.SaveChangesAsync();

        // 12. Seed Suppliers & Purchase Orders
        var suppliers = new List<Supplier>
        {
            new()
            {
                Name = "Haryana Agro Farmer Producer Cooperative",
                ContactPerson = "Harpreet Singh Mann",
                Phone = "+91 98120 77110",
                Email = "harpreet@haryanaagrocoop.org",
                Location = "Taraori Mandi Yard, Karnal",
                SupplierType = "Paddy Farmer Cooperative",
                GstOrTaxNumber = "06AAACH1234F1Z1",
                RatingStars = 5
            },
            new()
            {
                Name = "Gujarat Poly-Sack & Packaging Mills",
                ContactPerson = "Nilesh Shah",
                Phone = "+91 98250 44100",
                Email = "nilesh@gujaratpolysacks.com",
                Location = "GIDC Industrial Estate, Ahmedabad",
                SupplierType = "Packaging Manufacturer",
                GstOrTaxNumber = "24AABCG5512L1Z5",
                RatingStars = 5
            }
        };

        db.Suppliers.AddRange(suppliers);
        await db.SaveChangesAsync();

        var po1 = new PurchaseOrder
        {
            PoNumber = "PO-2026-001",
            SupplierId = suppliers[0].Id,
            WarehouseId = whKarnal.Id,
            ProductId = products[0].Id,
            QuantityMT = 100.0m,
            UnitRatePerMT = 850.0m,
            TotalAmount = 85000.0m,
            Currency = "USD",
            Status = "Received",
            OrderDate = DateTime.UtcNow.AddDays(-30),
            ActualReceivedDate = DateTime.UtcNow.AddDays(-22),
            BatchNumberGenerated = "KRN-1121S-2601",
            QualityCheckRemarks = "Grain elongation test passed. Moisture 11.8%."
        };

        db.PurchaseOrders.Add(po1);
        await db.SaveChangesAsync();

        // 13. Seed Activity Logs
        var activityLogs = new List<ActivityLog>
        {
            new()
            {
                UserId = salesUser.Id,
                UserName = "Tariq Mansoor",
                Action = "Inquiry Converted",
                Module = "Inquiry",
                Details = "Inquiry INQ-2026-001 converted to Quotation QT-2026-001 for Al-Barakah Trading Dubai.",
                Timestamp = DateTime.UtcNow.AddDays(-18)
            },
            new()
            {
                UserId = adminUser.Id,
                UserName = "Rajesh Sharma",
                Action = "Sales Order Approved",
                Module = "SalesOrder",
                Details = "Sales Order SO-2026-001 (50 MT 1121 Steam Basmati) approved for export execution.",
                Timestamp = DateTime.UtcNow.AddDays(-15)
            },
            new()
            {
                UserId = accountsUser.Id,
                UserName = "Pooja Verma",
                Action = "Payment Received",
                Module = "Finance",
                Details = "Received $18,000 (30% advance) for Invoice INV-2026-001 via Swift TT from Emirates NBD.",
                Timestamp = DateTime.UtcNow.AddDays(-13)
            },
            new()
            {
                UserId = inventoryUser.Id,
                UserName = "Vikram Singh",
                Action = "Shipment Milestone Updated",
                Module = "Shipment",
                Details = "Shipment SH-2026-001 loaded on vessel MAERSK BROOKLYN at Mundra Port. Status updated to In Transit.",
                Timestamp = DateTime.UtcNow.AddDays(-5)
            },
            new()
            {
                UserId = salesUser.Id,
                UserName = "Tariq Mansoor",
                Action = "Quotation Dispatched",
                Module = "Quotation",
                Details = "Quotation QT-2026-002 sent to Al-Khaleej Foodstuffs Riyadh for 100 MT 1121 Golden Sella.",
                Timestamp = DateTime.UtcNow.AddDays(-8)
            }
        };

        db.ActivityLogs.AddRange(activityLogs);
        await db.SaveChangesAsync();
    }
}

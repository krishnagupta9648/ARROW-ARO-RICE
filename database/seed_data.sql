-- ==============================================================================
-- ARROW ARO RICE – Complete Seed Data Script (DML)
-- Inserts realistic international grain export data
-- ==============================================================================

USE arrow_aro_rice;

-- 1. Insert Users (Password: Admin@2026#Rice, Sales@2026#Rice, etc. hashed with BCrypt)
INSERT INTO Users (Id, Username, Email, PasswordHash, FullName, Role, Department, IsActive, CreatedAt) VALUES
(1, 'admin', 'admin@arrowarorice.com', '$2a$11$0F4rW/9b139z7zL1G4v.8eX4qQ2m7z3w8b5eY2qW1a0b3c4d5e6f7', 'Rajesh Sharma', 'Admin', 'Executive Management', 1, '2026-04-19 10:00:00'),
(2, 'sales', 'sales@arrowarorice.com', '$2a$11$1G5sX/0c240a8aM2H5w.9fY5rR3n8a4x9c6fZ3rX2b1c4d5e6f7g8', 'Tariq Mansoor', 'Sales', 'International Export Sales', 1, '2026-04-19 10:00:00'),
(3, 'inventory', 'inventory@arrowarorice.com', '$2a$11$2H6tY/1d351b9bN3I6x.0gZ6sS4o9b5y0d7ga4sY3c2d5e6f7g8h9', 'Vikram Singh', 'Inventory', 'Logistics & Warehousing', 1, '2026-04-19 10:00:00'),
(4, 'accounts', 'accounts@arrowarorice.com', '$2a$11$3I7uZ/2e462c0cO4J7y.1ha7tT5p0c6z1e8hb5tZ4d3e6f7g8h9i0', 'Pooja Verma', 'Accounts', 'Export Finance & Documentation', 1, '2026-04-19 10:00:00')
ON DUPLICATE KEY UPDATE FullName=VALUES(FullName);

-- 2. Insert Warehouses
INSERT INTO Warehouses (Id, Name, Code, Location, City, State, CapacityMT, ContactPerson, Phone, IsActive) VALUES
(1, 'Central Milling Complex & Silos', 'WH-KRN-01', 'GT Karnal Road, Taraori Industrial Area', 'Karnal', 'Haryana', 45000.00, 'Baldev Singh', '+91 98120 44550', 1),
(2, 'Mundra Port CFS Warehouse', 'WH-MND-02', 'Near APSEZ CFS Zone, Port Road', 'Mundra', 'Gujarat', 25000.00, 'Kishore Patel', '+91 98250 88210', 1),
(3, 'Nhava Sheva CFS Logistics Hub', 'WH-JNPT-03', 'Sector 11, Dronagiri Node, Uran', 'Navi Mumbai', 'Maharashtra', 18000.00, 'Sanjay Sawant', '+91 98200 33190', 1)
ON DUPLICATE KEY UPDATE Name=VALUES(Name);

-- 3. Insert Products
INSERT INTO Products (Id, Code, Name, Category, Variety, ProcessingType, AverageLengthMm, MoistureMaxPercent, BrokenMaxPercent, ChalkyMaxPercent, ForeignMatterMaxPercent, AdmixtureMaxPercent, ElongationRatio, AromaGrade, KettWhiteness, PackagingOptions, MinOrderQuantityMT, DefaultFobPricePerMT, Currency, Description, Highlights, ImageUrl, IsActive, IsFeatured, CreatedAt) VALUES
(1, 'RICE-1121-STM', '1121 XXL Steam Basmati Rice', 'Basmati', '1121 Basmati', 'Steam', 8.35, 12.00, 1.00, 1.50, 0.05, 5.00, '2.2x - 2.5x', 'Rich Natural Aroma', '33 - 35 Kett', '1kg, 5kg, 10kg, 20kg, 25kg, 50kg Non-Woven, BOPP, Jute & Poly Pouches', 25.00, 1150.00, 'USD', 'The crown jewel of Indian Basmati rice. Known worldwide for exceptional grain length (8.35mm+ raw), delightful aroma, and supreme cooking elongation without stickiness. Ideal for royal Biryanis and gourmet hospitality.', 'Average grain length 8.35mm+; 100% Sortex cleaned; Extra long grain elongation; Zero artificial polishing', '/images/1121-steam.jpg', 1, 1, '2026-05-01 10:00:00'),
(2, 'RICE-1121-GDS', '1121 Golden Sella Basmati Rice', 'Basmati', '1121 Basmati', 'Golden Sella (Parboiled)', 8.35, 12.50, 1.00, 0.50, 0.02, 5.00, '2.4x - 2.8x', 'Authentic Nutty Aroma', '26 - 28 Kett (Rich Golden Hue)', '5kg, 10kg, 20kg, 25kg, 50kg PP & Non-Woven Bags', 25.00, 1190.00, 'USD', 'Parboiled in husk with specialized warm water immersion and gentle steam pressure that locks all vitamins into the grain, giving it a golden tint and maximum resistance to breakage during heavy catering cooking.', 'High elongation; Hard grain resists breakage; Golden parboiled nutrient seal; Perfect for Persian, Mandi and Kabsa dishes', '/images/1121-golden-sella.jpg', 1, 1, '2026-05-01 10:00:00'),
(3, 'RICE-1121-WTS', '1121 White / Creamy Sella Basmati Rice', 'Basmati', '1121 Basmati', 'Creamy Sella', 8.35, 12.50, 1.00, 0.50, 0.05, 5.00, '2.3x - 2.6x', 'Mellow Delicate Aroma', '30 - 32 Kett', '10kg, 20kg, 25kg, 50kg Master Cartons & PP Bags', 25.00, 1170.00, 'USD', 'Creamy parboiled Basmati offering supreme pearl-white luster and firm texture after boiling. Most favored by top catering companies in Saudi Arabia, UAE, and the European Union.', 'Pearl white finish; Fluffy separate grains; Excellent holding time on warming trays', '/images/1121-creamy-sella.jpg', 1, 1, '2026-05-01 10:00:00'),
(4, 'RICE-1509-STM', '1509 Steam Basmati Rice', 'Basmati', '1509 Basmati', 'Steam', 8.40, 12.00, 1.50, 2.00, 0.05, 5.00, '2.1x - 2.4x', 'Pleasant Basmati Fragrance', '34 - 36 Kett', '25kg, 50kg PP Bags, BOPP Bags', 25.00, 980.00, 'USD', 'Remarkable cost-performance ratio among extra long grain Basmati varieties. Offers an impressive grain length comparable to 1121 with fast cooking time and light mouthfeel.', 'Ultra-long grain length 8.40mm+; Economical high-volume commercial choice; Pure white luster', '/images/1509-steam.jpg', 1, 1, '2026-05-01 10:00:00'),
(5, 'RICE-TRD-RAW', 'Traditional Aged Raw Basmati Rice', 'Basmati', 'Traditional Pure Basmati', 'Raw / Milled (Aged 2 Years)', 7.40, 11.50, 1.00, 2.00, 0.02, 3.00, '2.2x - 2.5x', 'Intense Vintage Aroma (Heritage APEDA certified)', '36 - 38 Kett', '1kg, 2kg, 5kg, 10kg Jute & Velvet Gift Bags, Retail Pouches', 20.00, 1450.00, 'USD', 'Harvested from the pristine Himalayan foothills of Punjab and Haryana, naturally aged for up to 24 months in temperature controlled silos to maximize essential aroma oils and cooking elongation.', 'Naturally aged 2 years; Unmatched nostalgic aroma; Heritage pedigree; Premium gift & retail packaging', '/images/traditional-raw.jpg', 1, 1, '2026-05-01 10:00:00'),
(6, 'RICE-SUG-STM', 'Sugandha Steam Basmati Rice', 'Basmati', 'Sugandha', 'Steam', 7.80, 12.50, 2.00, 2.00, 0.05, 5.00, '2.0x - 2.2x', 'Sweet Aromatic', '32 - 34 Kett', '25kg, 50kg PP Bags', 25.00, 860.00, 'USD', 'Popular semi-long grain aromatic rice variety that delivers the scent and presentation of Basmati at an attractive commercial price point. Very popular in Gulf and African wholesale markets.', 'Strong aroma; Attractive price point; High bulk volume availability', '/images/sugandha-steam.jpg', 1, 0, '2026-05-01 10:00:00'),
(7, 'RICE-SHR-STM', 'Sharbati White Sella Basmati Rice', 'Basmati', 'Sharbati', 'Creamy Sella', 7.15, 12.50, 2.00, 1.00, 0.05, 5.00, '2.0x', 'Mild Aroma', '30 - 32 Kett', '25kg, 50kg PP Bags', 25.00, 780.00, 'USD', 'A hybrid variety sister to Basmati, with high grain elongation upon cooking and soft texture. Highly preferred for food processing, institutional cafeterias, and mid-range dining.', 'Budget friendly; Fluffy grains; High yield per cooked kilogram', '/images/sharbati-sella.jpg', 1, 0, '2026-05-01 10:00:00'),
(8, 'RICE-SNM-RAW', 'Sona Masoori Raw Milled Rice', 'Non-Basmati', 'Sona Masoori', 'Raw / Silky Polish', 5.20, 13.00, 3.00, 3.00, 0.10, 5.00, '1.8x', 'Neutral Fresh Grain', '38 - 42 Kett', '10kg, 20kg, 25kg Non-Woven & BOPP Bags', 25.00, 690.00, 'USD', 'Lightweight, low-starch medium grain rice grown primarily in the fertile river basins of South India. Easy to digest and loved by South Asian diaspora across the US, UK, Canada, and Australia.', 'Low starch & easy digestion; Silky optical sortex polish; Premium household staple', '/images/sona-masoori.jpg', 1, 1, '2026-05-01 10:00:00'),
(9, 'RICE-IR64-PB5', 'IR64 Parboiled Rice (5% Broken)', 'Non-Basmati', 'IR64', 'Parboiled / Double Polished', 6.10, 13.50, 5.00, 3.00, 0.15, 5.00, '1.7x', 'Standard Grain', '30 - 32 Kett', '25kg, 50kg PP Sacks / Container Bulk Liner', 50.00, 510.00, 'USD', 'The undisputed heavyweight of international non-basmati bulk rice trade. Huge volume exports to West Africa, Southeast Asia, South Africa, and Middle Eastern import tenders.', 'High energy nutrition; Double polished sortex clean; Bulk vessel & container loading capability', '/images/ir64-parboiled.jpg', 1, 1, '2026-05-01 10:00:00'),
(10, 'RICE-PR11-STM', 'PR11 / PR14 Steam Non-Basmati Rice', 'Non-Basmati', 'PR11 / 14', 'Steam', 6.80, 12.50, 3.00, 2.00, 0.10, 5.00, '1.9x', 'Mild Grain', '34 - 36 Kett', '25kg, 50kg PP Bags', 25.00, 580.00, 'USD', 'Long grain non-basmati variety that offers appearance and cooking length nearing Basmati rice, making it a very popular export to Russia, Central Asia, and Africa.', 'Long slender grain 6.80mm; Non-sticky cooking; Competitive pricing', '/images/pr11-steam.jpg', 1, 0, '2026-05-01 10:00:00'),
(11, 'RICE-BRK-100', '100% Broken White Rice (Brewery / Starch)', 'Broken', 'Broken Grain', 'Raw Milled Optical Cleaned', 2.50, 14.00, 100.00, 6.00, 0.20, 5.00, 'N/A', 'Neutral', '35 - 38 Kett', '50kg PP Sacks / Jumbo 1-Ton Bags', 100.00, 380.00, 'USD', 'Clean, destoned, sortex-screened broken rice grains ideal for industrial food manufacturing, baby food porridge, beer breweries, rice noodles, and starch manufacturing plants.', 'Destoned & metal free; High starch content; Jumbo bag packaging available', '/images/broken-rice.jpg', 1, 0, '2026-05-01 10:00:00')
ON DUPLICATE KEY UPDATE Name=VALUES(Name);

-- 4. Insert Customers
INSERT INTO Customers (Id, CompanyName, ContactPerson, Email, Phone, WhatsApp, Country, City, Address, TaxOrVatNumber, ImportLicenseNumber, PreferredDestinationPort, Status, CreditLimitUsd, PaymentTerms, Notes, CreatedAt) VALUES
(1, 'Al-Barakah General Trading LLC', 'Sheikh Tariq Al-Nuaimi', 'tariq@albarakahtrading.ae', '+971 4 398 2210', '+971 50 123 4567', 'United Arab Emirates', 'Dubai', 'Al Ras Wholesale Food Market, Deira', 'AE-TRN-1002938475', 'DXB-IMP-88192', 'Jebel Ali Port (AEJEA)', 'Active Buyer', 250000.00, '30% Advance + 70% against BL Scan', 'Major distributor of premium Basmati across the UAE and Oman. Orders 4-6 containers monthly.', '2026-05-15 11:00:00'),
(2, 'Al-Khaleej Foodstuffs Est.', 'Fahad Al-Otaibi', 'fahad@alkhaleejfoods.com.sa', '+966 11 482 9901', '+966 55 981 2234', 'Saudi Arabia', 'Riyadh', 'Al-Sulay Food Logistics District', 'SA-VAT-3001827461', 'SFDA-IMP-55410', 'Jeddah Islamic Port (SAJED)', 'Active Buyer', 400000.00, '100% Irrevocable LC at Sight', 'Specializes in 1121 Golden Sella and 1121 Creamy Sella for Mandi and Kabsa restaurants.', '2026-06-10 14:00:00'),
(3, 'EuroGrains GmbH & Co. KG', 'Dr. Markus Weber', 'm.weber@eurograins-hamburg.de', '+49 40 380 1190', '+49 171 445 9912', 'Germany', 'Hamburg', 'Speicherstadt Logistics Terminal, Brooktorkai 18', 'DE-814920381', 'EU-EORI-DE991827', 'Port of Hamburg (DEHAM)', 'Active Buyer', 300000.00, 'CAD (Cash Against Documents) via Deutsche Bank', 'Requires EU pesticide MRL compliance lab certificate and DNA Basmati purity certificate > 95%.', '2026-07-05 09:30:00'),
(4, 'Orient Foodstuffs Import Corp', 'David Henderson', 'd.henderson@orientfoodsus.com', '+1 713 550 8820', '+1 713 550 8820', 'United States', 'Houston', '8820 Westpark Dr, Suite 400, Houston TX 77063', 'US-EIN-74-8891024', 'FDA-REG-19283746', 'Port of Houston (USHOU)', 'Active Buyer', 500000.00, '30% Advance + 70% against BL', 'Distributes Sona Masoori and 1121 Basmati to Indian supermarkets across North America.', '2026-07-20 16:45:00'),
(5, 'Lion City Agro Commodities Pte Ltd', 'Tan Wei Ming', 'weiming@lioncityagro.sg', '+65 6789 2211', '+65 9123 4567', 'Singapore', 'Singapore', 'Jurong Port Road, Warehouse Complex 4', 'SG-UEN-201899124K', 'SFA-IMP-3381', 'Port of Singapore (SGSIN)', 'Active Buyer', 200000.00, '100% LC at sight', 'Imports IR64, Sona Masoori and PR11 for regional distribution across ASEAN.', '2026-08-12 12:15:00')
ON DUPLICATE KEY UPDATE CompanyName=VALUES(CompanyName);

-- 5. Insert Inventory Stocks
INSERT INTO InventoryStocks (Id, WarehouseId, ProductId, QuantityMT, ReservedMT, BatchNumber, MoistureTestedPercent, PackagingType, ReorderLevelMT, LastUpdated) VALUES
(1, 1, 1, 1200.00, 150.00, 'KRN-1121S-2601', 11.80, '50kg PP Bags', 100.00, '2026-10-10 10:00:00'),
(2, 2, 1, 450.00, 100.00, 'MND-1121S-2602', 12.00, '50kg PP Bags', 50.00, '2026-10-10 10:00:00'),
(3, 1, 2, 850.00, 50.00, 'KRN-1121G-2601', 12.20, '50kg PP Bags', 75.00, '2026-10-10 10:00:00'),
(4, 2, 2, 320.00, 0.00, 'MND-1121G-2603', 12.10, '50kg PP Bags', 50.00, '2026-10-10 10:00:00'),
(5, 1, 4, 600.00, 25.00, 'KRN-1509S-2601', 11.90, '25kg PP Bags', 60.00, '2026-10-10 10:00:00'),
(6, 3, 8, 950.00, 200.00, 'JNPT-SNM-2601', 12.80, '20kg Non-Woven', 80.00, '2026-10-10 10:00:00'),
(7, 3, 9, 2400.00, 500.00, 'JNPT-IR64-2601', 13.20, '50kg PP Bags', 200.00, '2026-10-10 10:00:00')
ON DUPLICATE KEY UPDATE QuantityMT=VALUES(QuantityMT);

-- 6. Insert Inquiries
INSERT INTO Inquiries (Id, InquiryNumber, CustomerId, FullName, CompanyName, Email, Phone, Country, RiceVariety, ProductId, QuantityMT, DestinationPort, IncoTerm, PreferredPackaging, Message, Status, Priority, AssignedToUserId, FollowUpDate, InternalNotes, CreatedAt) VALUES
(1, 'INQ-2026-001', 1, 'Sheikh Tariq Al-Nuaimi', 'Al-Barakah General Trading LLC', 'tariq@albarakahtrading.ae', '+971 4 398 2210', 'United Arab Emirates', '1121 Steam Basmati Rice', 1, 50.00, 'Jebel Ali Port (AEJEA)', 'CIF', '50 Kg Non-Woven Bags with buyer private logo', 'Looking for urgent CIF Jebel Ali quotation for 2x20ft FCL (50 MT) 1121 Steam Basmati. Need SGS pre-shipment quality certificate. Please confirm earliest shipping vessel schedule.', 'Converted', 'High', 2, '2026-10-12 10:00:00', 'Quotation accepted and converted into Sales Order SO-2026-001.', '2026-09-25 14:00:00'),
(2, 'INQ-2026-002', 2, 'Fahad Al-Otaibi', 'Al-Khaleej Foodstuffs Est.', 'fahad@alkhaleejfoods.com.sa', '+966 11 482 9901', 'Saudi Arabia', '1121 Golden Sella Basmati Rice', 2, 100.00, 'Jeddah Islamic Port (SAJED)', 'CIF', '40 Kg PP Bags (Standard Saudi Packing)', 'Require CIF Jeddah quotation for 4 FCL containers (100 MT) 1121 Golden Sella. Payment by 100% Irrevocable LC from Al-Rajhi Bank. Please send crop certificate and moisture guarantee.', 'QuotationSent', 'Urgent', 2, '2026-10-21 10:00:00', 'Quotation QT-2026-002 dispatched. Buyer reviewing commercial terms with bank.', '2026-10-09 11:30:00'),
(3, 'INQ-2026-003', 3, 'Dr. Markus Weber', 'EuroGrains GmbH & Co. KG', 'm.weber@eurograins-hamburg.de', '+49 40 380 1190', 'Germany', 'Traditional Aged Raw Basmati Rice', 5, 25.00, 'Port of Hamburg (DEHAM)', 'CIF', '10 Kg Master Carton with Jute Retail Bags', 'Seeking 1x20 FCL Traditional Raw Basmati. Strictly zero tricyclazole residue, compliant with current European Commission limits. Require Eurofins laboratory analysis test report before container stuffing.', 'SampleSent', 'High', 2, '2026-10-23 10:00:00', 'DHL air courier tracking sent for 2kg lab sample lot.', '2026-10-13 16:00:00'),
(4, 'INQ-2026-004', 4, 'David Henderson', 'Orient Foodstuffs Import Corp', 'd.henderson@orientfoodsus.com', '+1 713 550 8820', 'United States', 'Sona Masoori Raw Milled Rice', 8, 50.00, 'Port of Houston (USHOU)', 'CIF', '20 Lb (9.07 Kg) BOPP Bags with barcode and nutrition panel', 'Quote CIF Houston for 2 containers Sona Masoori silky polished. Must be registered with US FDA prior notice and fumigated with aluminum phosphide.', 'InNegotiation', 'Normal', 2, '2026-10-20 10:00:00', 'Buyer requested 2% discount on bulk 50 MT order. Discussed with management.', '2026-10-15 18:20:00'),
(5, 'INQ-2026-005', NULL, 'Kareem El-Sayed', 'Nile Delta Grains Cairo', 'kareem@niledeltagrains.com', '+20 2 2459 1100', 'Egypt', 'IR64 Parboiled Rice (5% Broken)', 9, 250.00, 'Alexandria Port (EGALY)', 'CFR', '50 Kg Polypropylene Sacks', 'Looking for immediate CFR Alexandria rate for 10 FCL containers (250 MT) of Indian IR64 Parboiled 5% broken. Send specification sheet and SGS inspection terms.', 'New', 'Urgent', 2, '2026-10-20 10:00:00', 'New inquiry from website RFQ portal. Requires immediate price calculation.', '2026-10-19 08:00:00')
ON DUPLICATE KEY UPDATE InquiryNumber=VALUES(InquiryNumber);

-- 7. Insert Quotations
INSERT INTO Quotations (Id, QuotationNumber, CustomerId, InquiryId, CustomerName, CustomerEmail, CustomerCountry, PortOfLoading, DestinationPort, IncoTerm, Currency, SubTotal, FreightCharges, InsuranceCharges, OtherCharges, Discount, GrandTotal, Status, QuotationDate, ValidUntil, PaymentTerms, InspectionAgency, TermsAndConditions, BankDetails, CreatedByUserId, CreatedAt) VALUES
(1, 'QT-2026-001', 1, 1, 'Al-Barakah General Trading LLC', 'tariq@albarakahtrading.ae', 'United Arab Emirates', 'Mundra Port, Gujarat, India', 'Jebel Ali Port (AEJEA), Dubai, UAE', 'CIF', 'USD', 57500.00, 2400.00, 350.00, 250.00, 500.00, 60000.00, 'Accepted', '2026-10-01 10:00:00', '2026-10-31 10:00:00', '30% Advance T/T upon order confirmation, 70% against scanned copy of original Bill of Lading', 'SGS India pre-shipment container inspection at Mundra Port (Certificate of Quality & Quantity)', '1. Price includes export seaworthy 50kg non-woven packaging.\n2. Shipment within 21 days from receipt of advance remittance.\n3. Tolerance of +/- 5% in quantity and weight at seller option.\n4. Subject to Force Majeure as per standard GAFTA rules.', 'Beneficiary: ARROW ARO RICE EXPORTS PVT LTD\nBank: State Bank of India, Overseas Branch, Mumbai\nAccount No: 39182940192\nSWIFT Code: SBININBB104\nIFSC: SBIN0001040', 2, '2026-10-01 10:00:00'),
(2, 'QT-2026-002', 2, 2, 'Al-Khaleej Foodstuffs Est.', 'fahad@alkhaleejfoods.com.sa', 'Saudi Arabia', 'Mundra Port, Gujarat, India', 'Jeddah Islamic Port (SAJED), Saudi Arabia', 'CIF', 'USD', 119000.00, 4800.00, 650.00, 400.00, 850.00, 124000.00, 'SentToBuyer', '2026-10-11 11:00:00', '2026-10-26 11:00:00', '100% Irrevocable Letter of Credit (LC) payable at sight from prime international bank', 'Bureau Veritas container stuffing and pesticide residue test', '1. Packing in standard 40 Kg PP bags with Arabic & English bilingual labeling conforming to SFDA standards.\n2. LC must allow transhipment and partial shipments.', 'Beneficiary: ARROW ARO RICE EXPORTS PVT LTD\nBank: ICICI Bank Ltd, International Trade Branch, New Delhi\nSWIFT: ICICINBB001', 2, '2026-10-11 11:00:00')
ON DUPLICATE KEY UPDATE QuotationNumber=VALUES(QuotationNumber);

-- 8. Insert Quotation Items
INSERT INTO QuotationItems (Id, QuotationId, ProductId, ProductName, SpecificationsSummary, QuantityMT, UnitPricePerMT, TotalAmount, Packaging) VALUES
(1, 1, 1, '1121 XXL Steam Basmati Rice', '8.35mm AGL, 12% Moisture max, 1% Broken max, Sortex Cleaned', 50.00, 1150.00, 57500.00, '50 Kg Non-Woven Bags with Buyer Logo'),
(2, 2, 2, '1121 Golden Sella Basmati Rice', '8.35mm AGL, 12.5% Moisture max, 1% Broken max, Rich Golden Parboiled', 100.00, 1190.00, 119000.00, '40 Kg PP Bags (SFDA Approved)')
ON DUPLICATE KEY UPDATE ProductName=VALUES(ProductName);

-- 9. Insert Sales Orders
INSERT INTO SalesOrders (Id, OrderNumber, QuotationId, CustomerId, OrderDate, ExpectedShipmentDate, PortOfLoading, PortOfDestination, IncoTerm, PaymentTerms, Currency, TotalQuantityMT, TotalAmount, Status, BuyerPoReference, SpecialInstructions, CreatedAt) VALUES
(1, 'SO-2026-001', 1, 1, '2026-10-04 12:00:00', '2026-10-24 12:00:00', 'Mundra Port, Gujarat (INMUN1)', 'Jebel Ali Port (AEJEA), Dubai, UAE', 'CIF', '30% Advance T/T, 70% against BL Scan', 'USD', 50.00, 60000.00, 'Shipped', 'ABG-PO-2026-881', 'Ensure fumigation with Methyl Bromide for 48 hours prior to vessel departure. Original documents to be sent via DHL to Dubai head office.', '2026-10-04 12:00:00')
ON DUPLICATE KEY UPDATE OrderNumber=VALUES(OrderNumber);

-- 10. Insert Sales Order Items
INSERT INTO SalesOrderItems (Id, SalesOrderId, ProductId, ProductName, QuantityMT, UnitPricePerMT, TotalAmount, Packaging) VALUES
(1, 1, 1, '1121 XXL Steam Basmati Rice', 50.00, 1150.00, 57500.00, '50 Kg Non-Woven Bags')
ON DUPLICATE KEY UPDATE ProductName=VALUES(ProductName);

-- 11. Insert Export Shipments
INSERT INTO ExportShipments (Id, ShipmentNumber, SalesOrderId, BookingNumber, ShippingLine, VesselName, VoyageNumber, ContainerNumbers, SealNumbers, BillOfLadingNumber, PortOfLoading, PortOfDestination, EstimatedDeparture, ActualDeparture, EstimatedArrival, CurrentMilestone, StatusDescription, TrackingNotes, CreatedAt) VALUES
(1, 'SH-2026-001', 1, 'MAEU-DXB-981042', 'Maersk Line (A.P. Moller - Maersk)', 'MAERSK BROOKLYN', 'V.2604W', 'MSKU7821941 (25 MT), MSKU7821942 (25 MT)', 'ML-IN-98124, ML-IN-98125', 'BL-MND-DXB-98104', 'Mundra Port (INMUN1)', 'Jebel Ali Port, Dubai (AEJEA)', '2026-10-14 18:00:00', '2026-10-14 20:30:00', '2026-10-23 14:00:00', 'InTransit', 'Vessel Sailing in Arabian Sea - ETA Jebel Ali in 4 Days', 'Customs cleared at Mundra Port CFS on schedule. Container loading and fumigation completed under SGS supervision.', '2026-10-09 10:00:00')
ON DUPLICATE KEY UPDATE ShipmentNumber=VALUES(ShipmentNumber);

-- 12. Insert Shipment Documents
INSERT INTO ShipmentDocuments (Id, ExportShipmentId, DocType, Title, DocumentNumber, IssuedDate, FilePath, IsVerified, IssuingAuthority, Remarks, CreatedAt) VALUES
(1, 1, 'CommercialInvoice', 'Export Commercial Invoice (Original)', 'INV-2026-001', '2026-10-13 10:00:00', '/documents/INV-2026-001.pdf', 1, 'ARROW ARO RICE EXPORTS PVT LTD', 'Certified true copy for customs & bank clearance.', '2026-10-13 10:00:00'),
(2, 1, 'PackingList', 'Detailed Container Packing List', 'PL-2026-001', '2026-10-13 10:00:00', '/documents/PL-2026-001.pdf', 1, 'ARROW ARO RICE Logistics Division', '1,000 bags of 50kg net weight each across 2x20 FCL containers.', '2026-10-13 10:00:00'),
(3, 1, 'CertificateOfOrigin', 'Certificate of Origin (Non-Preferential)', 'COO-IN-2026-4410', '2026-10-14 11:00:00', '/documents/COO-IN-2026-4410.pdf', 1, 'Export Inspection Council & Chamber of Commerce India', 'Attested and stamped.', '2026-10-14 11:00:00'),
(4, 1, 'PhytosanitaryCertificate', 'Plant Quarantine Phytosanitary Certificate', 'PHYTO-IND-99182', '2026-10-14 12:00:00', '/documents/PHYTO-IND-99182.pdf', 1, 'Directorate of Plant Protection, Quarantine & Storage, Govt. of India', 'Free from injurious pests and weed seeds.', '2026-10-14 12:00:00'),
(5, 1, 'SGSInspectionCertificate', 'SGS Quality & Container Inspection Report', 'SGS-MND-88219', '2026-10-14 13:00:00', '/documents/SGS-MND-88219.pdf', 1, 'SGS India Pvt. Ltd.', 'Average length verified at 8.38mm, moisture 11.9%, broken 0.8%.', '2026-10-14 13:00:00'),
(6, 1, 'BillOfLading', 'Ocean Clean on Board Bill of Lading', 'BL-MND-DXB-98104', '2026-10-14 21:00:00', '/documents/BL-MND-DXB-98104.pdf', 1, 'Maersk Shipping Line', 'Original 3/3 clean on board signed.', '2026-10-14 21:00:00')
ON DUPLICATE KEY UPDATE Title=VALUES(Title);

-- 13. Insert Commercial Invoices
INSERT INTO Invoices (Id, InvoiceNumber, SalesOrderId, CustomerId, InvoiceDate, DueDate, Currency, SubTotal, TaxOrDuty, GrandTotal, PaidAmount, Status, BankSwiftDetails, Notes, CreatedAt) VALUES
(1, 'INV-2026-001', 1, 1, '2026-10-05 10:00:00', '2026-11-04 10:00:00', 'USD', 57500.00, 0.00, 60000.00, 18000.00, 'PartiallyPaid', 'State Bank of India Overseas Branch | SWIFT: SBININBB104', '30% Advance ($18,000) received on confirmation. 70% Balance ($42,000) payable against Bill of Lading scan.', '2026-10-05 10:00:00')
ON DUPLICATE KEY UPDATE InvoiceNumber=VALUES(InvoiceNumber);

-- 14. Insert Payment Receipts
INSERT INTO PaymentReceipts (Id, ReceiptNumber, InvoiceId, CustomerId, PaymentDate, Amount, Currency, PaymentMethod, TransactionReference, BankName, Notes, CreatedAt) VALUES
(1, 'RCT-2026-001', 1, 1, '2026-10-06 14:00:00', 18000.00, 'USD', 'WireTransferTT', 'SWIFT-ENBD-AE-992819', 'Emirates NBD Bank Dubai', '30% Advance payment for SO-2026-001', '2026-10-06 14:00:00')
ON DUPLICATE KEY UPDATE ReceiptNumber=VALUES(ReceiptNumber);

-- 15. Insert Order Expenses
INSERT INTO OrderExpenses (Id, SalesOrderId, Category, Amount, Currency, VendorName, InvoiceOrReceiptNumber, ExpenseDate, Remarks, CreatedAt) VALUES
(1, 1, 'OceanFreight', 2200.00, 'USD', 'Maersk Line Agency India', 'MND-FRT-9810', '2026-10-13 10:00:00', 'Ocean freight for 2x20 FCL Mundra to Jebel Ali', '2026-10-13 10:00:00'),
(2, 1, 'InlandTransport', 850.00, 'USD', 'Northern Express Cargo Trailers', 'NET-KRN-MND-441', '2026-10-11 12:00:00', 'Trailer transport from Karnal processing plant to Mundra CFS', '2026-10-11 12:00:00'),
(3, 1, 'CustomsClearingCHA', 400.00, 'USD', 'Mundra Customs House Logistics Agents', 'CHA-MND-221', '2026-10-13 15:00:00', 'Customs documentation, shipping bill filing, and port gate pass', '2026-10-13 15:00:00'),
(4, 1, 'SGSInspectionFee', 350.00, 'USD', 'SGS India Pvt Ltd', 'SGS-INV-4418', '2026-10-14 09:00:00', 'Pre-shipment laboratory analysis and container stuffing supervision', '2026-10-14 09:00:00'),
(5, 1, 'FumigationFee', 180.00, 'USD', 'Gujarat Agro Fumigation Services', 'GAF-2026-109', '2026-10-14 14:00:00', 'Methyl Bromide 48-hour container fumigation with government certificate', '2026-10-14 14:00:00'),
(6, 1, 'MarineInsurance', 250.00, 'USD', 'New India Assurance Co Ltd', 'POL-MAR-99182', '2026-10-13 11:00:00', 'Institute Cargo Clauses (A) all risk marine coverage', '2026-10-13 11:00:00')
ON DUPLICATE KEY UPDATE Id=VALUES(Id);

-- 16. Insert Suppliers & Purchase Orders
INSERT INTO Suppliers (Id, Name, ContactPerson, Phone, Email, Location, SupplierType, GstOrTaxNumber, RatingStars, IsActive, CreatedAt) VALUES
(1, 'Haryana Agro Farmer Producer Cooperative', 'Harpreet Singh Mann', '+91 98120 77110', 'harpreet@haryanaagrocoop.org', 'Taraori Mandi Yard, Karnal', 'Paddy Farmer Cooperative', '06AAACH1234F1Z1', 5, 1, '2026-03-01 10:00:00'),
(2, 'Gujarat Poly-Sack & Packaging Mills', 'Nilesh Shah', '+91 98250 44100', 'nilesh@gujaratpolysacks.com', 'GIDC Industrial Estate, Ahmedabad', 'Packaging Manufacturer', '24AABCG5512L1Z5', 5, 1, '2026-03-01 10:00:00')
ON DUPLICATE KEY UPDATE Name=VALUES(Name);

INSERT INTO PurchaseOrders (Id, PoNumber, SupplierId, WarehouseId, ProductId, QuantityMT, UnitRatePerMT, TotalAmount, Currency, Status, OrderDate, ExpectedDeliveryDate, ActualReceivedDate, BatchNumberGenerated, QualityCheckRemarks, CreatedAt) VALUES
(1, 'PO-2026-001', 1, 1, 1, 100.00, 850.00, 85000.00, 'USD', 'Received', '2026-09-19 10:00:00', '2026-09-29 10:00:00', '2026-09-27 15:00:00', 'KRN-1121S-2601', 'Grain elongation test passed. Moisture 11.8%.', '2026-09-19 10:00:00')
ON DUPLICATE KEY UPDATE PoNumber=VALUES(PoNumber);

-- 17. Insert Activity Logs
INSERT INTO ActivityLogs (Id, UserId, UserName, Action, Module, Details, Timestamp) VALUES
(1, 2, 'Tariq Mansoor', 'Inquiry Converted', 'Inquiry', 'Inquiry INQ-2026-001 converted to Quotation QT-2026-001 for Al-Barakah Trading Dubai.', '2026-10-01 10:00:00'),
(2, 1, 'Rajesh Sharma', 'Sales Order Approved', 'SalesOrder', 'Sales Order SO-2026-001 (50 MT 1121 Steam Basmati) approved for export execution.', '2026-10-04 12:00:00'),
(3, 4, 'Pooja Verma', 'Payment Received', 'Finance', 'Received $18,000 (30% advance) for Invoice INV-2026-001 via Swift TT from Emirates NBD.', '2026-10-06 14:00:00'),
(4, 3, 'Vikram Singh', 'Shipment Milestone Updated', 'Shipment', 'Shipment SH-2026-001 loaded on vessel MAERSK BROOKLYN at Mundra Port. Status updated to In Transit.', '2026-10-14 20:30:00'),
(5, 2, 'Tariq Mansoor', 'Quotation Dispatched', 'Quotation', 'Quotation QT-2026-002 sent to Al-Khaleej Foodstuffs Riyadh for 100 MT 1121 Golden Sella.', '2026-10-11 11:00:00')
ON DUPLICATE KEY UPDATE Id=VALUES(Id);

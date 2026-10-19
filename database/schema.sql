-- ==============================================================================
-- ARROW ARO RICE – Complete Relational Database Schema
-- Compatible with MySQL 8.0+, MariaDB, PostgreSQL, MS SQL Server & SQLite
-- ==============================================================================

CREATE DATABASE IF NOT EXISTS arrow_aro_rice CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE arrow_aro_rice;

-- 1. Users Table (Role-Based Access Control)
CREATE TABLE IF NOT EXISTS Users (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Username VARCHAR(100) NOT NULL UNIQUE,
    Email VARCHAR(150) NOT NULL UNIQUE,
    PasswordHash VARCHAR(255) NOT NULL,
    FullName VARCHAR(120) NOT NULL,
    Role VARCHAR(50) NOT NULL DEFAULT 'Sales',
    Department VARCHAR(50) NULL,
    IsActive TINYINT(1) NOT NULL DEFAULT 1,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    LastLoginAt DATETIME NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 2. Customers Table (International Importers / Buyers)
CREATE TABLE IF NOT EXISTS Customers (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    CompanyName VARCHAR(150) NOT NULL,
    ContactPerson VARCHAR(120) NOT NULL,
    Email VARCHAR(150) NOT NULL,
    Phone VARCHAR(50) NOT NULL,
    WhatsApp VARCHAR(50) NULL,
    Country VARCHAR(100) NOT NULL,
    City VARCHAR(100) NULL,
    Address VARCHAR(300) NULL,
    TaxOrVatNumber VARCHAR(100) NULL,
    ImportLicenseNumber VARCHAR(100) NULL,
    PreferredDestinationPort VARCHAR(100) NULL,
    Status VARCHAR(50) NOT NULL DEFAULT 'Active Buyer',
    CreditLimitUsd DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    PaymentTerms VARCHAR(100) NOT NULL DEFAULT '30% Advance + 70% against BL Scan',
    Notes TEXT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 3. Products Table (Rice Varieties & Laboratory Technical Specifications)
CREATE TABLE IF NOT EXISTS Products (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Code VARCHAR(50) NOT NULL UNIQUE,
    Name VARCHAR(150) NOT NULL,
    Category VARCHAR(100) NOT NULL DEFAULT 'Basmati',
    Variety VARCHAR(100) NOT NULL,
    ProcessingType VARCHAR(100) NOT NULL,
    AverageLengthMm DECIMAL(18,2) NOT NULL DEFAULT 8.35,
    MoistureMaxPercent DECIMAL(18,2) NOT NULL DEFAULT 12.50,
    BrokenMaxPercent DECIMAL(18,2) NOT NULL DEFAULT 1.00,
    ChalkyMaxPercent DECIMAL(18,2) NOT NULL DEFAULT 2.00,
    ForeignMatterMaxPercent DECIMAL(18,2) NOT NULL DEFAULT 0.05,
    AdmixtureMaxPercent DECIMAL(18,2) NOT NULL DEFAULT 5.00,
    ElongationRatio VARCHAR(100) NOT NULL DEFAULT '2.2x - 2.5x',
    AromaGrade VARCHAR(100) NOT NULL DEFAULT 'Rich Natural Aroma',
    KettWhiteness VARCHAR(100) NOT NULL DEFAULT '33 - 35 Kett',
    PackagingOptions VARCHAR(255) NOT NULL,
    MinOrderQuantityMT DECIMAL(18,2) NOT NULL DEFAULT 25.00,
    DefaultFobPricePerMT DECIMAL(18,2) NOT NULL,
    Currency VARCHAR(10) NOT NULL DEFAULT 'USD',
    Description TEXT NOT NULL,
    Highlights TEXT NULL,
    ImageUrl VARCHAR(255) NOT NULL DEFAULT '/images/rice-placeholder.jpg',
    IsActive TINYINT(1) NOT NULL DEFAULT 1,
    IsFeatured TINYINT(1) NOT NULL DEFAULT 0,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 4. Inquiries Table (Buyer RFQs & Lead Pipeline)
CREATE TABLE IF NOT EXISTS Inquiries (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    InquiryNumber VARCHAR(50) NOT NULL UNIQUE,
    CustomerId INT NULL,
    FullName VARCHAR(120) NOT NULL,
    CompanyName VARCHAR(150) NULL,
    Email VARCHAR(150) NOT NULL,
    Phone VARCHAR(50) NOT NULL,
    Country VARCHAR(100) NOT NULL,
    RiceVariety VARCHAR(150) NOT NULL,
    ProductId INT NULL,
    QuantityMT DECIMAL(18,2) NOT NULL,
    DestinationPort VARCHAR(100) NOT NULL,
    IncoTerm VARCHAR(50) NOT NULL DEFAULT 'CIF',
    PreferredPackaging VARCHAR(150) NOT NULL DEFAULT '50 Kg PP Bags',
    Message TEXT NOT NULL,
    Status VARCHAR(50) NOT NULL DEFAULT 'New',
    Priority VARCHAR(50) NOT NULL DEFAULT 'Normal',
    AssignedToUserId INT NULL,
    FollowUpDate DATETIME NULL,
    InternalNotes TEXT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    LastContactedAt DATETIME NULL,
    CONSTRAINT FK_Inquiries_Customers FOREIGN KEY (CustomerId) REFERENCES Customers(Id) ON DELETE SET NULL,
    CONSTRAINT FK_Inquiries_Products FOREIGN KEY (ProductId) REFERENCES Products(Id) ON DELETE SET NULL,
    CONSTRAINT FK_Inquiries_Users FOREIGN KEY (AssignedToUserId) REFERENCES Users(Id) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 5. Warehouses Table (Silos & Port CFS Complexes)
CREATE TABLE IF NOT EXISTS Warehouses (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Name VARCHAR(100) NOT NULL,
    Code VARCHAR(50) NOT NULL UNIQUE,
    Location VARCHAR(200) NULL,
    City VARCHAR(100) NOT NULL,
    State VARCHAR(100) NOT NULL,
    CapacityMT DECIMAL(18,2) NOT NULL,
    ContactPerson VARCHAR(100) NULL,
    Phone VARCHAR(50) NULL,
    IsActive TINYINT(1) NOT NULL DEFAULT 1
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 6. InventoryStocks Table (Stock Ledger by Batch & Moisture)
CREATE TABLE IF NOT EXISTS InventoryStocks (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    WarehouseId INT NOT NULL,
    ProductId INT NOT NULL,
    QuantityMT DECIMAL(18,2) NOT NULL,
    ReservedMT DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    BatchNumber VARCHAR(100) NOT NULL,
    MoistureTestedPercent DECIMAL(18,2) NOT NULL,
    PackagingType VARCHAR(100) NOT NULL,
    ReorderLevelMT DECIMAL(18,2) NOT NULL DEFAULT 50.00,
    LastUpdated DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    CONSTRAINT FK_InventoryStocks_Warehouses FOREIGN KEY (WarehouseId) REFERENCES Warehouses(Id) ON DELETE CASCADE,
    CONSTRAINT FK_InventoryStocks_Products FOREIGN KEY (ProductId) REFERENCES Products(Id) ON DELETE CASCADE,
    CONSTRAINT UQ_Warehouse_Product_Batch UNIQUE (WarehouseId, ProductId, BatchNumber)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 7. StockTransactions Table (Inward & Outward Movements)
CREATE TABLE IF NOT EXISTS StockTransactions (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    ProductId INT NOT NULL,
    WarehouseId INT NOT NULL,
    TransactionType VARCHAR(50) NOT NULL,
    QuantityMT DECIMAL(18,2) NOT NULL,
    BatchNumber VARCHAR(100) NOT NULL,
    ReferenceDocument VARCHAR(100) NULL,
    Notes TEXT NULL,
    CreatedByUserId INT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT FK_StockTx_Products FOREIGN KEY (ProductId) REFERENCES Products(Id) ON DELETE CASCADE,
    CONSTRAINT FK_StockTx_Warehouses FOREIGN KEY (WarehouseId) REFERENCES Warehouses(Id) ON DELETE CASCADE,
    CONSTRAINT FK_StockTx_Users FOREIGN KEY (CreatedByUserId) REFERENCES Users(Id) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 8. Suppliers Table (Farmers, Mandis, Packaging Vendors)
CREATE TABLE IF NOT EXISTS Suppliers (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Name VARCHAR(150) NOT NULL,
    ContactPerson VARCHAR(100) NULL,
    Phone VARCHAR(50) NOT NULL,
    Email VARCHAR(150) NULL,
    Location VARCHAR(200) NULL,
    SupplierType VARCHAR(100) NOT NULL,
    GstOrTaxNumber VARCHAR(100) NULL,
    RatingStars INT NOT NULL DEFAULT 5,
    IsActive TINYINT(1) NOT NULL DEFAULT 1,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 9. PurchaseOrders Table (Raw Paddy & Material Procurement)
CREATE TABLE IF NOT EXISTS PurchaseOrders (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    PoNumber VARCHAR(50) NOT NULL UNIQUE,
    SupplierId INT NOT NULL,
    WarehouseId INT NOT NULL,
    ProductId INT NOT NULL,
    QuantityMT DECIMAL(18,2) NOT NULL,
    UnitRatePerMT DECIMAL(18,2) NOT NULL,
    TotalAmount DECIMAL(18,2) NOT NULL,
    Currency VARCHAR(10) NOT NULL DEFAULT 'USD',
    Status VARCHAR(50) NOT NULL DEFAULT 'Ordered',
    OrderDate DATETIME NOT NULL,
    ExpectedDeliveryDate DATETIME NULL,
    ActualReceivedDate DATETIME NULL,
    BatchNumberGenerated VARCHAR(100) NULL,
    QualityCheckRemarks TEXT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT FK_PO_Suppliers FOREIGN KEY (SupplierId) REFERENCES Suppliers(Id) ON DELETE RESTRICT,
    CONSTRAINT FK_PO_Warehouses FOREIGN KEY (WarehouseId) REFERENCES Warehouses(Id) ON DELETE RESTRICT,
    CONSTRAINT FK_PO_Products FOREIGN KEY (ProductId) REFERENCES Products(Id) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 10. Quotations Table (Proforma Offers & Export Terms)
CREATE TABLE IF NOT EXISTS Quotations (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    QuotationNumber VARCHAR(50) NOT NULL UNIQUE,
    CustomerId INT NULL,
    InquiryId INT NULL,
    CustomerName VARCHAR(150) NOT NULL,
    CustomerEmail VARCHAR(150) NOT NULL,
    CustomerCountry VARCHAR(100) NULL,
    PortOfLoading VARCHAR(100) NOT NULL,
    DestinationPort VARCHAR(100) NOT NULL,
    IncoTerm VARCHAR(20) NOT NULL DEFAULT 'CIF',
    Currency VARCHAR(10) NOT NULL DEFAULT 'USD',
    SubTotal DECIMAL(18,2) NOT NULL,
    FreightCharges DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    InsuranceCharges DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    OtherCharges DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    Discount DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    GrandTotal DECIMAL(18,2) NOT NULL,
    Status VARCHAR(50) NOT NULL DEFAULT 'Draft',
    QuotationDate DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ValidUntil DATETIME NOT NULL,
    PaymentTerms VARCHAR(150) NOT NULL,
    InspectionAgency VARCHAR(150) NULL,
    TermsAndConditions TEXT NULL,
    BankDetails TEXT NULL,
    Notes TEXT NULL,
    RevisionNumber INT NOT NULL DEFAULT 1,
    CreatedByUserId INT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT FK_Quotations_Customers FOREIGN KEY (CustomerId) REFERENCES Customers(Id) ON DELETE SET NULL,
    CONSTRAINT FK_Quotations_Inquiries FOREIGN KEY (InquiryId) REFERENCES Inquiries(Id) ON DELETE SET NULL,
    CONSTRAINT FK_Quotations_Users FOREIGN KEY (CreatedByUserId) REFERENCES Users(Id) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 11. QuotationItems Table (Line Items per Quotation)
CREATE TABLE IF NOT EXISTS QuotationItems (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    QuotationId INT NOT NULL,
    ProductId INT NULL,
    ProductName VARCHAR(150) NOT NULL,
    SpecificationsSummary VARCHAR(250) NULL,
    QuantityMT DECIMAL(18,2) NOT NULL,
    UnitPricePerMT DECIMAL(18,2) NOT NULL,
    TotalAmount DECIMAL(18,2) NOT NULL,
    Packaging VARCHAR(100) NOT NULL,
    CONSTRAINT FK_QItems_Quotations FOREIGN KEY (QuotationId) REFERENCES Quotations(Id) ON DELETE CASCADE,
    CONSTRAINT FK_QItems_Products FOREIGN KEY (ProductId) REFERENCES Products(Id) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 12. SalesOrders Table (Confirmed Trade Contracts)
CREATE TABLE IF NOT EXISTS SalesOrders (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    OrderNumber VARCHAR(50) NOT NULL UNIQUE,
    QuotationId INT NULL UNIQUE,
    CustomerId INT NOT NULL,
    OrderDate DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ExpectedShipmentDate DATETIME NOT NULL,
    PortOfLoading VARCHAR(100) NOT NULL,
    PortOfDestination VARCHAR(100) NOT NULL,
    IncoTerm VARCHAR(20) NOT NULL DEFAULT 'CIF',
    PaymentTerms VARCHAR(150) NOT NULL,
    Currency VARCHAR(10) NOT NULL DEFAULT 'USD',
    TotalQuantityMT DECIMAL(18,2) NOT NULL,
    TotalAmount DECIMAL(18,2) NOT NULL,
    Status VARCHAR(50) NOT NULL DEFAULT 'Confirmed',
    BuyerPoReference VARCHAR(150) NULL,
    SpecialInstructions TEXT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT FK_SalesOrders_Quotations FOREIGN KEY (QuotationId) REFERENCES Quotations(Id) ON DELETE SET NULL,
    CONSTRAINT FK_SalesOrders_Customers FOREIGN KEY (CustomerId) REFERENCES Customers(Id) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 13. SalesOrderItems Table (Line Items per Sales Order)
CREATE TABLE IF NOT EXISTS SalesOrderItems (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    SalesOrderId INT NOT NULL,
    ProductId INT NULL,
    ProductName VARCHAR(150) NOT NULL,
    QuantityMT DECIMAL(18,2) NOT NULL,
    UnitPricePerMT DECIMAL(18,2) NOT NULL,
    TotalAmount DECIMAL(18,2) NOT NULL,
    Packaging VARCHAR(100) NOT NULL,
    CONSTRAINT FK_SOItems_SalesOrders FOREIGN KEY (SalesOrderId) REFERENCES SalesOrders(Id) ON DELETE CASCADE,
    CONSTRAINT FK_SOItems_Products FOREIGN KEY (ProductId) REFERENCES Products(Id) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 14. ExportShipments Table (Maritime Tracking & Voyage Milestones)
CREATE TABLE IF NOT EXISTS ExportShipments (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    ShipmentNumber VARCHAR(50) NOT NULL UNIQUE,
    SalesOrderId INT NOT NULL,
    BookingNumber VARCHAR(100) NOT NULL,
    ShippingLine VARCHAR(100) NOT NULL,
    VesselName VARCHAR(120) NOT NULL,
    VoyageNumber VARCHAR(50) NOT NULL,
    ContainerNumbers VARCHAR(200) NOT NULL,
    SealNumbers VARCHAR(200) NOT NULL,
    BillOfLadingNumber VARCHAR(100) NOT NULL,
    PortOfLoading VARCHAR(100) NOT NULL,
    PortOfDestination VARCHAR(100) NOT NULL,
    EstimatedDeparture DATETIME NULL,
    ActualDeparture DATETIME NULL,
    EstimatedArrival DATETIME NULL,
    ActualArrival DATETIME NULL,
    CurrentMilestone VARCHAR(50) NOT NULL DEFAULT 'OrderConfirmed',
    StatusDescription VARCHAR(100) NOT NULL,
    TrackingNotes TEXT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT FK_Shipments_SalesOrders FOREIGN KEY (SalesOrderId) REFERENCES SalesOrders(Id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 15. ShipmentDocuments Table (Statutory Certificates & Dossiers)
CREATE TABLE IF NOT EXISTS ShipmentDocuments (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    ExportShipmentId INT NOT NULL,
    DocType VARCHAR(50) NOT NULL,
    Title VARCHAR(150) NOT NULL,
    DocumentNumber VARCHAR(100) NOT NULL,
    IssuedDate DATETIME NOT NULL,
    FilePath VARCHAR(255) NOT NULL,
    IsVerified TINYINT(1) NOT NULL DEFAULT 1,
    IssuingAuthority VARCHAR(150) NOT NULL,
    Remarks TEXT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT FK_ShipDocs_Shipments FOREIGN KEY (ExportShipmentId) REFERENCES ExportShipments(Id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 16. Invoices Table (Commercial Export Invoices)
CREATE TABLE IF NOT EXISTS Invoices (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    InvoiceNumber VARCHAR(50) NOT NULL UNIQUE,
    SalesOrderId INT NOT NULL,
    CustomerId INT NOT NULL,
    InvoiceDate DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    DueDate DATETIME NOT NULL,
    Currency VARCHAR(10) NOT NULL DEFAULT 'USD',
    SubTotal DECIMAL(18,2) NOT NULL,
    TaxOrDuty DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    GrandTotal DECIMAL(18,2) NOT NULL,
    PaidAmount DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    Status VARCHAR(50) NOT NULL DEFAULT 'Unpaid',
    BankSwiftDetails VARCHAR(200) NULL,
    Notes TEXT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT FK_Invoices_SalesOrders FOREIGN KEY (SalesOrderId) REFERENCES SalesOrders(Id) ON DELETE CASCADE,
    CONSTRAINT FK_Invoices_Customers FOREIGN KEY (CustomerId) REFERENCES Customers(Id) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 17. PaymentReceipts Table (Swift Wire Transfers, LCs, CAD)
CREATE TABLE IF NOT EXISTS PaymentReceipts (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    ReceiptNumber VARCHAR(50) NOT NULL UNIQUE,
    InvoiceId INT NOT NULL,
    CustomerId INT NOT NULL,
    PaymentDate DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Amount DECIMAL(18,2) NOT NULL,
    Currency VARCHAR(10) NOT NULL DEFAULT 'USD',
    PaymentMethod VARCHAR(50) NOT NULL,
    TransactionReference VARCHAR(100) NOT NULL,
    BankName VARCHAR(150) NULL,
    Notes TEXT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT FK_Receipts_Invoices FOREIGN KEY (InvoiceId) REFERENCES Invoices(Id) ON DELETE CASCADE,
    CONSTRAINT FK_Receipts_Customers FOREIGN KEY (CustomerId) REFERENCES Customers(Id) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 18. OrderExpenses Table (Order-Wise Logistics, Freight, CHA)
CREATE TABLE IF NOT EXISTS OrderExpenses (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    SalesOrderId INT NOT NULL,
    Category VARCHAR(50) NOT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    Currency VARCHAR(10) NOT NULL DEFAULT 'USD',
    VendorOrPayee VARCHAR(150) NOT NULL,
    InvoiceOrReceiptNumber VARCHAR(100) NULL,
    ExpenseDate DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Remarks TEXT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT FK_Expenses_SalesOrders FOREIGN KEY (SalesOrderId) REFERENCES SalesOrders(Id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 19. ActivityLogs Table (System Audit Trail)
CREATE TABLE IF NOT EXISTS ActivityLogs (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    UserId INT NULL,
    UserName VARCHAR(120) NOT NULL DEFAULT 'System',
    Action VARCHAR(100) NOT NULL,
    Module VARCHAR(100) NOT NULL,
    Details TEXT NOT NULL,
    IpAddress VARCHAR(50) NULL,
    Timestamp DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT FK_ActivityLogs_Users FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Indexes for High Performance Search
CREATE INDEX IX_Inquiries_Status ON Inquiries(Status);
CREATE INDEX IX_Inquiries_Country ON Inquiries(Country);
CREATE INDEX IX_Customers_Country ON Customers(Country);
CREATE INDEX IX_Products_Category ON Products(Category);
CREATE INDEX IX_SalesOrders_Status ON SalesOrders(Status);
CREATE INDEX IX_ExportShipments_Milestone ON ExportShipments(CurrentMilestone);
CREATE INDEX IX_Invoices_Status ON Invoices(Status);

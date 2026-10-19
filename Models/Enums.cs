namespace ARROW_ARO_RICE.Models;

public enum UserRole
{
    Admin,
    Sales,
    Inventory,
    Accounts
}

public enum InquiryStatus
{
    New,
    Contacted,
    SampleRequested,
    SampleSent,
    QuotationSent,
    InNegotiation,
    Converted,
    Lost
}

public enum InquiryPriority
{
    Normal,
    High,
    Urgent
}

public enum QuotationStatus
{
    Draft,
    PendingApproval,
    Approved,
    SentToBuyer,
    Accepted,
    Rejected,
    Expired
}

public enum SalesOrderStatus
{
    Confirmed,
    Processing,
    Packed,
    Shipped,
    Delivered,
    Completed,
    Cancelled
}

public enum ShipmentMilestone
{
    OrderConfirmed,
    FactoryProcessing,
    PackingCompleted,
    CfsPortArrival,
    CustomsCleared,
    VesselDeparted,
    InTransit,
    ArrivedDestinationPort,
    CustomsClearedDest,
    Delivered
}

public enum DocumentType
{
    CommercialInvoice,
    PackingList,
    CertificateOfOrigin,
    PhytosanitaryCertificate,
    BillOfLading,
    SGSInspectionCertificate,
    FumigationCertificate,
    HealthCertificate,
    CertificateOfAnalysis,
    InsuranceCertificate
}

public enum StockTransactionType
{
    StockIn,
    StockOut,
    Transfer,
    Adjustment
}

public enum PaymentMethod
{
    WireTransferTT,
    LetterOfCredit,
    CashAgainstDocuments,
    AdvancePayment
}

public enum ExpenseCategory
{
    OceanFreight,
    InlandTransport,
    PackagingMaterial,
    CustomsClearingCHA,
    SGSInspectionFee,
    FumigationFee,
    PortCharges,
    MarineInsurance,
    BankCharges,
    Miscellaneous
}

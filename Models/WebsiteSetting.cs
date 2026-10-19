using System.ComponentModel.DataAnnotations;

namespace ARROW_ARO_RICE.Models;

public class WebsiteSetting
{
    public int Id { get; set; }

    [Required, MaxLength(150)]
    public string CompanyName { get; set; } = "ARROW ARO RICE EXPORTS PVT LTD";

    [MaxLength(250)]
    public string Tagline { get; set; } = "India's Premier Miller & Star Export House of Authentic Basmati & Non-Basmati Rice";

    [Required, MaxLength(100), EmailAddress]
    public string SupportEmail { get; set; } = "export@arrowarorice.com";

    [Required, MaxLength(50)]
    public string SupportPhone { get; set; } = "+91 98120 44550";

    [MaxLength(50)]
    public string WhatsAppNumber { get; set; } = "+91 98120 44550";

    [MaxLength(300)]
    public string HeadOfficeAddress { get; set; } = "GT Karnal Road, Taraori Industrial Area, Karnal, Haryana, India - 132116";

    [MaxLength(300)]
    public string KarnalMillingAddress { get; set; } = "Milestone 138, NH-44, Taraori Modern Milling Zone, Karnal (Haryana)";

    [MaxLength(300)]
    public string MundraPortAddress { get; set; } = "CFS Complex, APSEZ Port Road, Mundra, Kutch, Gujarat - 370421";

    [MaxLength(300)]
    public string JnptPortAddress { get; set; } = "Sector 11, Dronagiri Node, JNPT Logistics Hub, Uran, Navi Mumbai - 400702";

    public int AnnualMillingCapacityMT { get; set; } = 150000;

    [MaxLength(50)]
    public string YearsOfHeritage { get; set; } = "25+";

    [MaxLength(50)]
    public string CountriesExported { get; set; } = "45+";

    [MaxLength(300)]
    public string TopbarAnnouncement { get; set; } = "APEDA Registered Star Export House | ISO 22000 & HACCP Certified | Shipments to UAE, Saudi Arabia, USA & EU";

    public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;
}

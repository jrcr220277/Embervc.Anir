using System;
using System.Collections.Generic;
using System.Text;

namespace Anir.Shared.Contracts.Common;

public class ReportConfigDto
{
    public string CompanyName { get; set; } = string.Empty;
    public string? ShortName { get; set; }
    public byte[]? LogoBytes { get; set; }
    public string? HeaderText { get; set; }
    public string? FooterText { get; set; }
    public string PrimaryColor { get; set; } = "#4A6FA5";
}

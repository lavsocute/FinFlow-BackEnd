namespace FinFlow.Domain.Employees;

/// <summary>
/// Static registry of supported Vietnamese banks. Adding a new bank only requires
/// extending <see cref="All"/>; downstream code (validation, GraphQL bankCodes query,
/// CSV exporters) auto-picks up the new entry.
/// </summary>
public static class VietnamBanks
{
    public sealed record Bank(string Code, string Name, string FullName);

    public static readonly IReadOnlyList<Bank> All =
    [
        new("VCB",  "Vietcombank", "Ngân hàng TMCP Ngoại thương Việt Nam"),
        new("BIDV", "BIDV",        "Ngân hàng TMCP Đầu tư và Phát triển Việt Nam"),
        new("TCB",  "Techcombank", "Ngân hàng TMCP Kỹ thương Việt Nam"),
        new("MBB",  "MB Bank",     "Ngân hàng TMCP Quân đội"),
        new("ACB",  "ACB",         "Ngân hàng TMCP Á Châu"),
        new("VPB",  "VPBank",      "Ngân hàng TMCP Việt Nam Thịnh Vượng"),
        new("OCB",  "OCB",         "Ngân hàng TMCP Phương Đông"),
        new("SHB",  "SHB",         "Ngân hàng TMCP Sài Gòn - Hà Nội"),
        new("VIB",  "VIB",         "Ngân hàng TMCP Quốc tế Việt Nam"),
        new("TPB",  "TPBank",      "Ngân hàng TMCP Tiên Phong"),
        new("STB",  "Sacombank",   "Ngân hàng TMCP Sài Gòn Thương Tín"),
        new("AGB",  "Agribank",    "Ngân hàng Nông nghiệp và Phát triển Nông thôn Việt Nam"),
    ];

    public static bool IsValidCode(string? code) =>
        !string.IsNullOrWhiteSpace(code) && All.Any(b => b.Code == code);

    public static Bank? Find(string code) =>
        All.FirstOrDefault(b => b.Code == code);
}

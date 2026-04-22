using Anir.Shared.Contracts.Common;
using Anir.Shared.Enums;

namespace Anir.Shared.Contracts.User;

public class UpdateProfileRequest
{
    public string FullName { get; set; } = string.Empty;

    public FileResponse? ImageFile { get; set; }

    public ThemeMode ThemeMode { get; set; }
}
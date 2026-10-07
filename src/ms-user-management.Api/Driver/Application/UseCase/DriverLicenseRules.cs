using ms_user_management.Api.Shared.Domain.Model;

namespace ms_user_management.Api.Driver.Application.UseCase;

public static class DriverLicenseRules
{
    public const int MaxLicenseNumberLength = 20;

    public static string ValidateLicenseNumber(string licenseNumber)
    {
        if (string.IsNullOrWhiteSpace(licenseNumber))
            throw new ArgumentException("LicenseNumber is required.", nameof(licenseNumber));

        var trimmed = licenseNumber.Trim();

        if (trimmed.Length > MaxLicenseNumberLength)
            throw new ArgumentException(
                $"LicenseNumber must be {MaxLicenseNumberLength} characters or fewer.",
                nameof(licenseNumber));

        return trimmed;
    }

    public static DateOnly ValidateExpirationDate(DateOnly expirationDate)
    {
        if (expirationDate == default)
            throw new ArgumentException("LicenseExpirationDate is required.", nameof(expirationDate));

        return expirationDate;
    }

    public static Status ParseStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
            throw new ArgumentException("Status must be 'Active' or 'Inactive'.", nameof(status));

        if (!Enum.TryParse<Status>(status, true, out var parsed))
            throw new ArgumentException("Status must be 'Active' or 'Inactive'.", nameof(status));

        return parsed;
    }
}

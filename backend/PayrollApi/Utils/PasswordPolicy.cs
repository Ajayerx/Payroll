using System.Text.RegularExpressions;

namespace PayrollApi.Utils;

public static class PasswordPolicy
{
    public const int MinimumLength = 8;
    public const int MaximumLength = 128;

    public static (bool Valid, string Message) Validate(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return (false, "Password is required");

        if (password.Length < MinimumLength)
            return (false, $"Password must be at least {MinimumLength} characters long");

        if (password.Length > MaximumLength)
            return (false, $"Password must not exceed {MaximumLength} characters");

        if (!Regex.IsMatch(password, @"[A-Z]"))
            return (false, "Password must contain at least one uppercase letter");

        if (!Regex.IsMatch(password, @"[a-z]"))
            return (false, "Password must contain at least one lowercase letter");

        if (!Regex.IsMatch(password, @"[0-9]"))
            return (false, "Password must contain at least one digit");

        if (!Regex.IsMatch(password, @"[!@#$%^&*(),.?"":{}|<>]"))
            return (false, "Password must contain at least one special character");

        return (true, "Password is valid");
    }

    public static string GenerateTempPassword()
    {
        var chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789!@#$";
        var random = new Random();
        return new string(Enumerable.Range(0, 12).Select(_ => chars[random.Next(chars.Length)]).ToArray());
    }
}

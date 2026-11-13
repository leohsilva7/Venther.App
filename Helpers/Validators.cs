using System.Net.Mail;
using System.Text.RegularExpressions;

namespace Venther.App.Helpers;

public static partial class Validators
{
    private static readonly Regex PasswordRegex = new(@"^(?=.*[A-Z])(?=.*[^A-Za-z0-9]).{8,}$", RegexOptions.Compiled);

    public static bool IsValidEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        return MailAddress.TryCreate(email.Trim(), out _);
    }

    public static bool IsValidPassword(string? password)
    {
        if (string.IsNullOrEmpty(password))
            return false;

        return PasswordRegex.IsMatch(password);
    }
}

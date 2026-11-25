using System.Net.Mail;
using System.Text.RegularExpressions;

namespace Venther.App.Helpers;

public static partial class Validators
{
    // API: 8 a 100 caracteres, com maiúscula, minúscula, número e caractere especial.
    private static readonly Regex PasswordRegex = new(
        @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).{8,100}$",
        RegexOptions.Compiled);

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

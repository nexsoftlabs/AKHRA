using System.Text.RegularExpressions;

namespace MoviePlatform.Infrastructure.Identity;

public static partial class PhoneNumberNormalizer
{
    public static bool TryNormalize(string input, out string e164)
    {
        var digits = NonDigits().Replace(input, string.Empty);
        if (digits.Length < 10)
        {
            e164 = string.Empty;
            return false;
        }

        if (digits.Length == 10)
        {
            e164 = "+91" + digits;
            return true;
        }

        if (digits.StartsWith("91") && digits.Length == 12)
        {
            e164 = "+" + digits;
            return true;
        }

        if (input.TrimStart().StartsWith('+') && digits.Length >= 11)
        {
            e164 = "+" + digits;
            return true;
        }

        e164 = string.Empty;
        return false;
    }

    [GeneratedRegex("[^0-9]")]
    private static partial Regex NonDigits();
}

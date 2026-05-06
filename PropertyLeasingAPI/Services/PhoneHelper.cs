using System.Text.RegularExpressions;

namespace PropertyLeasingAPI.Services
{
    /// <summary>
    /// L13: single source of truth for phone-number canonicalisation.
    /// Stored phone numbers are always digit-only with an optional leading '+',
    /// so the Track lookup's digits-only comparison can never miss a match
    /// because of formatting differences.
    /// </summary>
    public static class PhoneHelper
    {
        /// <summary>
        /// Normalises a raw user-entered phone number to digits-only,
        /// preserving a leading '+' for international format.
        /// </summary>
        public static string Normalize(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return string.Empty;
            var trimmed = raw.Trim();
            var hasPlus = trimmed.StartsWith("+");
            var digits = Regex.Replace(trimmed, @"\D", string.Empty);
            return digits.Length == 0 ? string.Empty : (hasPlus ? "+" + digits : digits);
        }

        /// <summary>
        /// Compares two phone numbers ignoring any formatting differences.
        /// </summary>
        public static bool Match(string? stored, string? submitted)
        {
            var a = Regex.Replace(stored ?? string.Empty, @"\D", string.Empty);
            var b = Regex.Replace(submitted ?? string.Empty, @"\D", string.Empty);
            return a.Length > 0 && a == b;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Domain.ValueObjects
{
    public sealed partial class Email
    {
        private static readonly Regex Pattern = EmailRegex();
        public string Value { get; }
        private Email(string value) => Value = value.ToLowerInvariant();
        public static Email Create(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be empty.", nameof(email));
            if (!Pattern.IsMatch(email))
                throw new ArgumentException($"Invalid email format: {email}", nameof(email));
            return new Email(email);
        }

        public override string ToString()
        {
            return Value;
        }

        [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase)]
        private static partial Regex EmailRegex();
    }
}

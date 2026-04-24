using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Domain.ValueObjects
{
    public sealed partial class WalletAddress
    {
        private static readonly Regex Pattern = EthAddressRegex(); 
        public string Value { get; }

        private WalletAddress(string value) => Value = value.ToLowerInvariant();

        public static WalletAddress Create(string address)
        {
            if (string.IsNullOrWhiteSpace(address))
                throw new ArgumentException("Wallet address cannot be empty.", nameof(address));
            if (!Pattern.IsMatch(address))
                throw new ArgumentException($"Invalid Ethereum wallet address: {address}", nameof(address));
            return new WalletAddress(address);
        }

        public static bool TryCreate(string address, out WalletAddress? result)
        {
            result = null;
            if (string.IsNullOrWhiteSpace(address) || !Pattern.IsMatch(address))
                return false;

            result = new WalletAddress(address);
            return true;
        }

        public override string ToString()
        {
            return Value;
        }

        [GeneratedRegex("^0x[a-fA-F0-9]{40}$")]
        private static partial Regex EthAddressRegex();
    }
}

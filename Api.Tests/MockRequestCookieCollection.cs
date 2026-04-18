using Microsoft.AspNetCore.Http;
using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Api.Tests
{
    internal class MockRequestCookieCollection(IDictionary<string, string> cookies) : IRequestCookieCollection
    {
        private readonly IDictionary<string, string> _cookies = cookies;

        public string? this[string key]
            => _cookies.TryGetValue(key, out var value) ? value : null;

        public int Count => _cookies.Count;

        public ICollection<string> Keys => _cookies.Keys;

        public bool ContainsKey(string key)
        {
            return _cookies.ContainsKey(key);
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return _cookies.GetEnumerator();
        }

        public bool TryGetValue(string key, [NotNullWhen(true)] out string? value)
        {
            return _cookies.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


    }
}

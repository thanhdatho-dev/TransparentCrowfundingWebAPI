using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Configurations
{
    public class JWT
    {
        public string Issuer { get; set; } = null!;
        public string Audience { get; set; } = null!;
        public string SigningKey { get; set; } = null!;
    }
}

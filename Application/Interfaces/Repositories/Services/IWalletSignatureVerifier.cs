using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces.Repositories.Services
{
    public interface IWalletSignatureVerifier
    {
        bool VerifySignature(string message, string signature, string expectedAddress);
    }
}

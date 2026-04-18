using Application.Interfaces.Repositories.Services;
using Nethereum.Signer;
using Nethereum.Util;

namespace Infrastructure.Services
{
    public class WalletSignatureVerifier : IWalletSignatureVerifier
    {
        public bool VerifySignature(string message, string signature, string expectedAddress)
        {
            if (string.IsNullOrEmpty(message) || string.IsNullOrEmpty(signature) || string.IsNullOrEmpty(expectedAddress))
                return false;
            var signer = new EthereumMessageSigner();
            string? recoveredAddress;
            try
            {
                recoveredAddress = signer.EncodeUTF8AndEcRecover(message, signature);
            }
            catch
            {
                return false;
            }
            if (expectedAddress.IsTheSameAddress(recoveredAddress))
                return true;
            return false;
        }
    }
}

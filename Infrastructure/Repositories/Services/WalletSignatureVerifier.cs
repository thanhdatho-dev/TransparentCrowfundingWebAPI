using Application.Interfaces.Services;
using Nethereum.Signer;
using Nethereum.Util;

namespace Infrastructure.Repositories.Services
{
    public class WalletSignatureVerifier : IWalletSignatureVerifier
    {
        public bool VerifySignature(string message, string signature, string expectedAddress)
        {
            var signer = new EthereumMessageSigner();
            var recoveredAddress = signer.EncodeUTF8AndEcRecover(message, signature);
            if (expectedAddress.IsTheSameAddress(recoveredAddress))
                return true;
            return false;
        }
    }
}

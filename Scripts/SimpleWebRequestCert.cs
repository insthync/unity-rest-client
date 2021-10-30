using UnityEngine.Networking;

namespace UnityRestClient
{
    public class SimpleWebRequestCert : CertificateHandler
    {
        protected override bool ValidateCertificate(byte[] certificateData)
        {
            return true;
        }
    }
}

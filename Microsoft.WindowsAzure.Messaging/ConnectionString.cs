using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.Messaging
{
    public static class ConnectionString
    {
        public static string CreateUsingSharedAccessKey(Uri endPoint, string keyName, string accessSecret)
        {
            if (endPoint == null)
            {
                throw new ArgumentException("endPoint");
            }

            if (String.IsNullOrWhiteSpace(keyName))
            {
                throw new ArgumentException("keyName");
            }

            if (String.IsNullOrWhiteSpace(accessSecret))
            {
                throw new ArgumentException("accessSecret");
            }

            return String.Format("Endpoint={0};SharedAccessKeyName={1};SharedAccessKey={2}", endPoint.ToString(), keyName, accessSecret);
        }

        public static string CreateUsingSharedAccessKeyWithFullAccess(Uri endPoint, string fullAccessSecret)
        {
            if (String.IsNullOrWhiteSpace(fullAccessSecret))
            {
                throw new ArgumentException("fullAccessSecret");
            }

            return CreateUsingSharedAccessKey(endPoint, "DefaultFullSharedAccessSignature", fullAccessSecret);
        }

        public static string CreateUsingSharedAccessKeyWithListenAccess(Uri endPoint, string listenAccessSecret)
        {
            if (String.IsNullOrWhiteSpace(listenAccessSecret))
            {
                throw new ArgumentException("listenAccessSecret");
            }

            return CreateUsingSharedAccessKey(endPoint, "DefaultListenSharedAccessSignature", listenAccessSecret);
        }

        public static string CreateUsingSharedSecret(Uri endPoint, string issuer, string issuerSecret)
        {
            throw new NotImplementedException();
        }
    }
}

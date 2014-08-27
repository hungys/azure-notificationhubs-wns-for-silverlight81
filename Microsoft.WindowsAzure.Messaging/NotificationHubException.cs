using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.Messaging
{
    public class NotificationHubException : Exception
    {
        private static readonly long serialVersionUID = -2417498840698257022L;
        public int StatusCode { get; set; }

        public NotificationHubException(String error, int statusCode) : base(error)
        {
            StatusCode = statusCode;
        }
    }
}

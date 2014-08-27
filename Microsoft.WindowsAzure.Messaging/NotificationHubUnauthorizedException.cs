using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.Messaging
{
    public class NotificationHubUnauthorizedException : NotificationHubException
    {
        private static readonly long serialVersionUID = -5926583893712403416L;

        public NotificationHubUnauthorizedException() : base("Unauthorized", 401)
        {

        }
    }
}

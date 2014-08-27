using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.Messaging
{
    public class NotificationHubResourceNotFoundException : NotificationHubException
    {
        private static readonly long serialVersionUID = -1205615098165583127L;

        public NotificationHubResourceNotFoundException() : base("Resource not found", 404)
        {

        }
    }
}

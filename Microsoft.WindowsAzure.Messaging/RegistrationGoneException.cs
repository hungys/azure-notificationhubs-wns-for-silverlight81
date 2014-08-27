using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.Messaging
{
    public class RegistrationGoneException : Exception
    {
        private static readonly long serialVersionUID = -156200383034074631L;

        public RegistrationGoneException() : base("Registration is gone")
        {

        }
    }
}

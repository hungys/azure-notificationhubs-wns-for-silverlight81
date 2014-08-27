using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Data.Xml.Dom;

namespace Microsoft.WindowsAzure.Messaging
{
    public class Registration
    {
        private readonly String DEFAULT_REGISTRATION_NAME = "$Default";
	    private readonly String REGISTRATIONID_JSON_PROPERTY = "registrationid";
	    private readonly String REGISTRATION_NAME_JSON_PROPERTY = "registrationName";

        /// <summary>
        /// Gets or sets the registration name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the URI associated with the registration.
        /// </summary>
        public string ChannelUri { get; set; }

        /// <summary>
        /// Gets or sets the entity tag.
        /// </summary>
        public string ETag { get; set; }

        /// <summary>
        /// Gets the date and time when the registration expires.
        /// </summary>
        public DateTime? ExpiresAt { get; set; }

        /// <summary>
        /// Gets or sets the notification hub path.
        /// </summary>
        public string NotificationHubPath { get; set; }

        /// <summary>
        /// Gets or sets the registration ID.
        /// </summary>
        public string RegistrationId { get; set; }

        /// <summary>
        /// Gets or sets the collection of tags for the registration.
        /// </summary>
        public ISet<string> Tags { get; set; }

        /// <summary>
        /// Initializes a new instance of the Registration class.
        /// </summary>
        public Registration()
        {
            Name = DEFAULT_REGISTRATION_NAME;
            Tags = new HashSet<string>();
        }

        /// <summary>
        /// Initializes a new instance of the Registration class.
        /// </summary>
        /// <param name="channelUri"></param>
        public Registration(string channelUri)
        {
            Name = DEFAULT_REGISTRATION_NAME;
            ChannelUri = channelUri;
            Tags = new HashSet<string>();
        }

        /// <summary>
        /// Initializes a new instance of the Registration class.
        /// </summary>
        /// <param name="channelUri"></param>
        /// <param name="tags"></param>
        public Registration(string channelUri, IEnumerable<string> tags)
        {
            Name = DEFAULT_REGISTRATION_NAME;
            ChannelUri = channelUri;
            Tags = new HashSet<string>(tags);
        }

        public string ToXml()
        {
            string template = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                            "<entry xmlns=\"http://www.w3.org/2005/Atom\">" +
                            "    <content type=\"application/xml\">" +
                            "        <WindowsRegistrationDescription xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://schemas.microsoft.com/netservices/2010/10/servicebus/connect\">" +
                            "            <Tags>{0}</Tags>" +
                            "            <ChannelUri>{1}</ChannelUri>" +
                            "        </WindowsRegistrationDescription>" +
                            "    </content>" +
                            "</entry>";

            string tags = "";
            List<String> tagList = new List<String>(Tags);
            if (tagList != null && tagList.Count > 0)
            {
                tags = tagList[0];

                for (int i = 1; i < tagList.Count; i++)
                {
                    tags += "," + tagList[i];
                }
            }

            return String.Format(template, tags, ChannelUri);
        }

        public void loadXml(string xml, string notificationHubPath)
        {
            NotificationHubPath = notificationHubPath;
            RegistrationId = getNodeValueFromXml(xml, "RegistrationId");
            ChannelUri = getNodeValueFromXml(xml, "ChannelUri");
            ETag = getNodeValueFromXml(xml, "ETag");
            ExpiresAt = DateTime.Parse(getNodeValueFromXml(xml, "ExpirationTime"));

            string[] tags = getNodeValueFromXml(xml, "Tags").Split(',');
            foreach(var tag in tags)
            {
                Tags.Add(tag);
            }
        }

        private string getNodeValueFromXml(string xml, string key)
        {
            string head = String.Format("<{0}>", key);
            string tail = String.Format("</{0}>", key);

            if (xml.IndexOf(head) < 0)
            {
                return "";
            }

            int start = xml.IndexOf(head)+head.Length;
            int end = xml.IndexOf(tail) - 1;

            try
            {
                return xml.Substring(start, end - start + 1);
            }
            catch
            {
                return "";
            }
        }
    }
}

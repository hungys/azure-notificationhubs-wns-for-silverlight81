using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.Messaging
{
    public class NotificationHub
    {
        private readonly String NEW_REGISTRATION_LOCATION_HEADER = "Location";
        private readonly String XML_CONTENT_TYPE = "application/atom+xml";
	    private readonly String STORAGE_REGISTRATION_KEY = "Registrations";
	    private readonly String STORAGE_VERSION_KEY = "Version";
	    private readonly String STORAGE_VERSION_VALUE = "v1.0.0";
	    private readonly String STORAGE_CHANNEL_KEY = "Channel";
	    private bool mIsRefreshNeeded = false;
        private IsolatedStorageSettings isolatedStorageSettings;

        /// <summary>
        /// Gets or sets the connection string to the Windows Azure service.
        /// </summary>
        public string Connection { get; set; }

        /// <summary>
        /// Gets or sets the path associated with the notification hub.
        /// </summary>
        public string Path { get; set; }
         
        /// <summary>
        /// Initializes a new instance of the NotificationHub class.
        /// </summary>
        /// <param name="notificationHubPath"></param>
        /// <param name="connectionString"></param>
        public NotificationHub(string notificationHubPath, string connectionString)
        {
            setNotificationHubPath(notificationHubPath);
            setConnectionString(connectionString);

            isolatedStorageSettings = IsolatedStorageSettings.ApplicationSettings;
        }

        /// <summary>
        /// Creates a new registration (or updates an existing one) as specified
        /// in the registration parameter. Depending upon the type of the registration
        /// parameter (Registration or TemplateRegistration), the registration of a native
        /// or template notification is created or updated.
        /// </summary>
        /// <param name="registration"></param>
        /// <returns></returns>
        public async Task<Registration> RegisterAsync(Registration registration)
        {
            return await registerInternal(registration);
        }

        /// <summary>
        /// Asynchronously registers the device for native notifications.
        /// </summary>
        /// <param name="channelUri"></param>
        /// <param name="tags"></param>
        /// <returns></returns>
        public async Task<Registration> RegisterNativeAsync(string channelUri, IEnumerable<string> tags)
        {
            if (String.IsNullOrEmpty(channelUri))
            {
                throw new ArgumentException("channelUri");
            }

            Registration registration = new Registration(channelUri, tags);

            return await registerInternal(registration);
        }

        /// <summary>
        /// Asynchronously registers the device for native notifications.
        /// </summary>
        /// <param name="channelUri"></param>
        /// <returns></returns>
        public async Task<Registration> RegisterNativeAsync(string channelUri)
        {
            if (String.IsNullOrEmpty(channelUri))
            {
                throw new ArgumentException("channelUri");
            }

            Registration registration = new Registration(channelUri);

            return await registerInternal(registration);
        }

        /// <summary>
        /// Asynchronously deletes all registrations for this application or secondary tile.
        /// </summary>
        /// <param name="channelUri"></param>
        /// <returns></returns>
        public async Task UnregisterAllAsync(string channelUri)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Asynchronously deletes the registration identified by the registration object
        /// specified by the registration parameter. Note that the registration object must
        /// have a non-null RegistrationId property.
        /// </summary>
        /// <param name="registration"></param>
        /// <returns></returns>
        public async Task UnregisterAsync(Registration registration)
        {
            if (!String.IsNullOrWhiteSpace(registration.RegistrationId))
            {
                await deleteRegistrationInternal(registration.RegistrationId);
            }
        }

        /// <summary>
        /// Asynchronously unregisters the native registration on the application or
        /// secondary tiles. Note that if you have template registrations, they will
        /// not be deleted.
        /// </summary>
        /// <returns></returns>
        public async Task UnregisterNativeAsync()
        {
            await unregisterInternal();
        }

        /// <summary>
        /// Asynchronously unregisters the template with the specified name on the 
        /// application or secondary tiles. Note that if you have other templates 
        /// or a native registration, they will not be deleted.
        /// </summary>
        /// <param name="templateName"></param>
        /// <returns></returns>
        public async Task UnregisterTemplateAsync(string templateName)
        {
            throw new NotImplementedException();
        }

        private async Task<Registration> registerInternal(Registration registration)
        {
            String registrationId = retrieveRegistrationId();
            if (String.IsNullOrWhiteSpace(registrationId))
            {
                registrationId = await createRegistrationId();
            }

            registration.RegistrationId = registrationId;

            try
            {
                return await upsertRegistrationInternal(registration);
            }
            catch (RegistrationGoneException e)
            {
                // if we get an RegistrationGoneException (410) from service, we will recreate registration id and will try to do upsert one more time.
            }

            registrationId = await createRegistrationId();
            registration.RegistrationId = registrationId;
            return await upsertRegistrationInternal(registration);
        }

        private async Task unregisterInternal()
        {
            String registrationId = retrieveRegistrationId();

            if (!String.IsNullOrWhiteSpace(registrationId))
            {
                await deleteRegistrationInternal(registrationId);
            }
        }

        private async Task deleteRegistrationInternal(String registrationId)
        {
            Connection conn = new Connection(Connection);
            String resource = Path + "/Registrations/" + registrationId;
            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("If-Match", "*");

            try
            {
                await conn.executeRequest(resource, null, XML_CONTENT_TYPE, HttpMethod.Delete, headers);
            }
            finally
            {
                removeRegistrationId();
            }
        }

        private async Task<Registration> upsertRegistrationInternal(Registration registration)
        {
            Connection conn = new Connection(Connection);

            String resource = Path + "/Registrations/" + registration.RegistrationId;
            String content = registration.ToXml();

            String response = await conn.executeRequest(resource, content, XML_CONTENT_TYPE, HttpMethod.Put, null);

            Registration result = new Registration();
            result.loadXml(response, Path);

            storeRegistrationId(result.Name, result.RegistrationId, registration.ChannelUri);

            return result;
        }

        private void storeRegistrationId(String registrationName, String registrationId, String channelUri)
        {
            isolatedStorageSettings[Path + "-" + STORAGE_REGISTRATION_KEY] = String.Format("{0}:{1}", registrationName, registrationId);
            isolatedStorageSettings[Path + "-" + STORAGE_CHANNEL_KEY] = channelUri;
            isolatedStorageSettings[Path + "-" + STORAGE_VERSION_KEY] = STORAGE_VERSION_VALUE;
            isolatedStorageSettings.Save();
        }

        private void setNotificationHubPath(String notificationHubPath)
        {
            if (String.IsNullOrWhiteSpace(notificationHubPath))
            {
                throw new ArgumentException("notificationHubPath");
            }

            Path = notificationHubPath;
        }

        private void setConnectionString(String connectionString)
        {
            if (String.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException("connectionString");
            }

            try
            {
                ConnectionStringParser.Parse(connectionString);
            }
            catch (Exception e)
            {
                throw new ArgumentException("connectionString", e);
            }

            Connection = connectionString;
        }

        private async Task<String> createRegistrationId()
        {
            Connection conn = new Connection(Connection);

            String resource = Path + "/registrationIDs/";
            String response = await conn.executeRequest(resource, null, XML_CONTENT_TYPE, HttpMethod.Post, NEW_REGISTRATION_LOCATION_HEADER, null);

            Uri regIdUri = new Uri(response);
            String[] pathFragments = regIdUri.AbsolutePath.Split('/');
            String result = pathFragments[pathFragments.Length - 1];

            return result;
        }

        private string retrieveRegistrationId()
        {
            if (isolatedStorageSettings.Contains(Path + "-" + STORAGE_REGISTRATION_KEY))
            {
                return ((string)isolatedStorageSettings[Path + "-" + STORAGE_REGISTRATION_KEY]).Split(':')[1];
            }
            else
            {
                return null;
            }
        }

        private void removeRegistrationId()
        {
            if (isolatedStorageSettings.Contains(Path + "-" + STORAGE_REGISTRATION_KEY))
            {
                isolatedStorageSettings.Remove(Path + "-" + STORAGE_REGISTRATION_KEY);
                isolatedStorageSettings.Save();
            }
        }
    }
}

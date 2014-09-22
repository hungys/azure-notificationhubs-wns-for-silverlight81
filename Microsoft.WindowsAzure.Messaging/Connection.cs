using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.Messaging
{
    class Connection
    {
	    private readonly String SHARED_ACCESS_KEY_NAME = "SharedAccessKeyName";
	    private readonly String SHARED_ACCESS_KEY = "SharedAccessKey";
	    private readonly String AUTHORIZATION_HEADER = "Authorization";
        private readonly String UTC_TIME_ZONE = "UTC";
        private readonly String UTF8_ENCODING = "UTF-8";
        private readonly String ENDPOINT_KEY = "Endpoint";
        private readonly int EXPIRE_MINUTES = 5;
        private readonly String SDK_VERSION = "2014-08";
        private readonly String API_VERSION_KEY = "api-version";
        private readonly String API_VERSION = "2013-08";

        private Dictionary<String, String> mConnectionData;

        public Connection(String connectionString)
        {
            mConnectionData = ConnectionStringParser.Parse(connectionString);
        }

        public async Task<String> executeRequest(String resource, String content, String contentType, HttpMethod method, Dictionary<string, string> extraHeaders)
        {
            try
            {
                return await executeRequest(resource, content, contentType, method, null, extraHeaders);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<String> executeRequest(String resource, String content, String contentType, HttpMethod method, String targetHeaderName, Dictionary<string, string> extraHeaders)
        {
            Uri endpointURI = new Uri(mConnectionData[ENDPOINT_KEY]);
            String scheme = endpointURI.Scheme;

            // Replace the scheme with "https"
            String url = "https" + endpointURI.ToString().Substring(scheme.Length);
            if (!url.ToString().EndsWith("/"))
            {
                url += "/";
            }

            url += resource;

            url = AddApiVersionToUrl(url);

            HttpRequestMessage request = new HttpRequestMessage(method, new Uri(url));
            if (!String.IsNullOrEmpty(content))
            {
                request.Content = new StringContent(content, Encoding.UTF8, contentType);
            }

            if (extraHeaders != null)
            {
                foreach (var header in extraHeaders)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
            }

            try
            {
                return await executeRequest(request, targetHeaderName);
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        private String AddApiVersionToUrl(String url)
        {
            Uri uri = new Uri(url);

            if (String.IsNullOrEmpty(uri.Query))
            {
                url = url + "?";
            }
            else
            {
                url = url + "&";
            }

            url = url + API_VERSION_KEY + "=" + API_VERSION;

            return url;
        }

        private async Task<String> executeRequest(HttpRequestMessage request, String targetHeaderName)
        {
            addAuthorizationHeader(request);

            int status;
            String content;
            String headerValue = null;
            HttpClient client = null;
            bool noHeaderButExpected = false;

            try
            {
                client = new HttpClient();

                HttpResponseMessage response = await client.SendAsync(request);

                status = (int)response.StatusCode;
                content = await response.Content.ReadAsStringAsync();

                if (targetHeaderName != null)
                {
                    if (!response.Headers.Contains(targetHeaderName))
                    {
                        noHeaderButExpected = true;
                    }
                    else
                    {
                        headerValue = response.Headers.GetValues(targetHeaderName).FirstOrDefault();
                    }
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (client != null)
                {
                    client.Dispose();
                }
            }

            if (status >= 200 && status < 300)
            {
                if (noHeaderButExpected)
                {
                    throw new NotificationHubException("The '" + targetHeaderName + "' header does not present in collection", status);
                }
                return targetHeaderName == null ? content : headerValue;
            }
            else if (status == 404)
            {
                throw new NotificationHubResourceNotFoundException();
            }
            else if (status == 401)
            {
                throw new NotificationHubUnauthorizedException();
            }
            else if (status == 410)
            {
                throw new RegistrationGoneException();
            }
            else
            {
                throw new NotificationHubException(content, status);
            }
        }

        private void addAuthorizationHeader(HttpRequestMessage request)
        {
            String token = generateAuthToken(request.RequestUri.ToString());
            request.Headers.Add(AUTHORIZATION_HEADER, token);
        }

        private String generateAuthToken(String url)
        {
            String keyName = mConnectionData[SHARED_ACCESS_KEY_NAME];
            String key = mConnectionData[SHARED_ACCESS_KEY];

            url = WebUtility.UrlEncode(url);

            // Set expiration in seconds
            long expires = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
            expires += EXPIRE_MINUTES * 60 * 1000;
            expires = expires / 1000;

            String toSign = url + '\n' + expires.ToString();

            // sign
            byte[] bytesToSign = Encoding.UTF8.GetBytes(toSign);

            HMACSHA256 hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
            hmac.ComputeHash(bytesToSign);
            byte[] signedHash = hmac.ComputeHash(bytesToSign);
            String base64Signature = Convert.ToBase64String(signedHash);
            base64Signature = base64Signature.Trim();
            base64Signature = WebUtility.UrlEncode(base64Signature);

            // construct autorization string
            String token = "SharedAccessSignature sr=" + url + "&sig=" + base64Signature + "&se=" + expires + "&skn=" + keyName;

            return token;
        }
    }
}

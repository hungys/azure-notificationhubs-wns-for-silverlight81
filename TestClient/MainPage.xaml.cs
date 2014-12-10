using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using TestClient.Resources;
using Microsoft.WindowsAzure.Messaging;
using Windows.Data.Xml.Dom;
using Windows.Networking.PushNotifications;
using System.IO.IsolatedStorage;

namespace TestClient
{
    public partial class MainPage : PhoneApplicationPage
    {
        private const string NOTIFICATION_HUB_NAME = "YOUR_NOTIFICATION_HUBS_NAME";
        private const string CONNECTION_STRING = "YOUR_NOTIFICATION_HUBS_CONNECTION_STRING";

        public MainPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ShowPushChannel();
            Register();
        }

        private void ShowPushChannel()
        {
            IsolatedStorageSettings isolatedStorageSettings = IsolatedStorageSettings.ApplicationSettings;
            if (isolatedStorageSettings.Contains("whoscallwp-Registrations"))
            {
                txtRegistrationID.Text = (string)isolatedStorageSettings["whoscallwp-Registrations"];
                txtChannelUri.Text = (string)isolatedStorageSettings["whoscallwp-Channel"];
            }
            else
            {
                txtRegistrationID.Text = "Unregistered";
                txtChannelUri.Text = "Unregistered";
            }
        }

        private async void Register()
        {
            PushNotificationChannel channel = null;

            try
            {
                channel = await PushNotificationChannelManager.CreatePushNotificationChannelForApplicationAsync();
            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
                return;
            }

            List<string> tags = new List<string>();
            tags.Add("tester");

            NotificationHub hub = new NotificationHub(NOTIFICATION_HUB_NAME, CONNECTION_STRING);
            //await hub.RegisterNativeAsync(channel.Uri);
            await hub.RegisterNativeAsync(channel.Uri, tags);
            ShowPushChannel();
        }

        private async void btnUnregister_Click(object sender, RoutedEventArgs e)
        {
            NotificationHub hub = new NotificationHub(NOTIFICATION_HUB_NAME, CONNECTION_STRING);
            await hub.UnregisterNativeAsync();
            ShowPushChannel();
        }
    }
}
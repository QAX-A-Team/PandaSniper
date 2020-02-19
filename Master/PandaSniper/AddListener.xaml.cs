using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PandaSniper
{
    /// <summary>
    /// AddListener.xaml 的交互逻辑
    /// </summary>
    public partial class AddListener : Window
    {

        public ObservableCollection<ListenersListView> listeners;
        public delegate void TransfDelegate(ObservableCollection<ListenersListView> listeners);
        public UserProfile userProfile;

        public event TransfDelegate TransfEvent;

        public AddListener()
        {
            InitializeComponent();
        }

        private void AddListenerWindow_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void WindowTitle_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }
        private void WindowClose_MouseEnter(object sender, MouseEventArgs e)
        {

            this.WindowCloseIcon.Visibility = Visibility.Visible;
            this.WindowMinSizeIcon.Visibility = Visibility.Visible;
        }
        private void WindowClose_MouseLeave(object sender, MouseEventArgs e)
        {

            this.WindowCloseIcon.Visibility = Visibility.Hidden;
            this.WindowMinSizeIcon.Visibility = Visibility.Hidden;
        }
        private void WindowMinSize_MouseEnter(object sender, MouseEventArgs e)
        {

            this.WindowCloseIcon.Visibility = Visibility.Visible;
            this.WindowMinSizeIcon.Visibility = Visibility.Visible;
        }
        private void WindowMinSize_MouseLeave(object sender, MouseEventArgs e)
        {
            this.WindowCloseIcon.Visibility = Visibility.Hidden;
            this.WindowMinSizeIcon.Visibility = Visibility.Hidden;
        }
        private void WindowMaxSize_MouseEnter(object sender, MouseEventArgs e)
        {

            this.WindowCloseIcon.Visibility = Visibility.Visible;
            this.WindowMinSizeIcon.Visibility = Visibility.Visible;
        }
        private void WindowMaxSize_MouseLeave(object sender, MouseEventArgs e)
        {
            this.WindowCloseIcon.Visibility = Visibility.Hidden;
            this.WindowMinSizeIcon.Visibility = Visibility.Hidden;
        }
        private void WindowClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void WindowMinSize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }


        private void HttpHostsListBox_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void AddListenerSave_MouseEnter(object sender, MouseEventArgs e)
        {

        }

        private void AddListenerSave_MouseLeave(object sender, MouseEventArgs e)
        {

        }

        private void AddListenerSave_Click(object sender, RoutedEventArgs e)
        {
            string hosts = "";
            foreach (ListBoxItem listBoxItem in HttpHostsListBox.Items)
            {
                if(hosts == "")
                {
                    hosts = listBoxItem.DataContext.ToString();
                }
                else
                {
                    hosts = hosts + "," + listBoxItem.DataContext.ToString();
                }
                
            }
            ListenersListView listener = new ListenersListView
            {
                name = ListenerName.Text,
                payload = ListenerPayload.Text,
                hosts = hosts,
                stagerHost = HttpHostStager.Text,
                port = HttpPortC2.Text,
                bindto = HttpPortBind.Text,
                header = HttpHostHeader.Text,
                proxy = HttpProxy.Text,
                profile = ListenerProfile.Text
            };

            if ("" == listener.name)
            {
                MessageBox.Show("name is empty");
                return;
            }
            else if ("" == listener.port)
            {
                MessageBox.Show("port is empty");
                return;
            }

            bool isE = false;
            foreach (ListenersListView listenerFormat in this.listeners)
            {
                if (listenerFormat.name == listener.name)
                {
                    MessageBox.Show("name is exits");
                    return;
                }
                else if (listenerFormat.port == listener.port)
                {
                    MessageBox.Show("port is exits");
                    return;
                }
                else
                {
                    isE = true;
                }
            }
            if (this.listeners.Count == 0 || isE)
            {
                this.listeners.Add(listener);
                DataFormat MessageData;
                MessageData.type = "3";
                MessageData.token = userProfile.token;
                MessageData.data = new Dictionary<string, string> { { "port", listener.Port } };
                string sendMessage = JsonConvert.SerializeObject(MessageData);
                SslTcpClient sslTcpClient = new SslTcpClient(userProfile.host, int.Parse(userProfile.port), "localhost");
                sslTcpClient.StartSslTcp();
                SslStream sslStream = sslTcpClient.SendMessage(sendMessage);
                sslTcpClient.ReadMessage(sslStream);

                JObject rMJson = (JObject)JsonConvert.DeserializeObject(sslTcpClient.resultMessage);
                if (rMJson["code"].ToString() == "200")
                {
                    MessageBox.Show("监听成功");
                }
                else
                {
                    MessageBox.Show(rMJson["error"].ToString());
                    sslTcpClient.CloseSslTcp();
                    return;
                }
                sslTcpClient.CloseSslTcp();
            }

            this.TransfEvent(this.listeners);//触发事件
            this.Close();
        }

        private void AddListenerClose_MouseEnter(object sender, MouseEventArgs e)
        {

        }

        private void AddListenerClose_MouseLeave(object sender, MouseEventArgs e)
        {

        }

        private void AddListenerClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void AddHttpHost_MouseEnter(object sender, MouseEventArgs e)
        {

        }

        private void AddHttpHost_MouseLeave(object sender, MouseEventArgs e)
        {

        }

        private void AddHttpHost_Click(object sender, RoutedEventArgs e)
        {

        }

        private void RemoveHttpHost_MouseEnter(object sender, MouseEventArgs e)
        {

        }

        private void RemoveHttpHost_MouseLeave(object sender, MouseEventArgs e)
        {

        }

        private void RemoveHttpHost_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AddHttpProxy_MouseEnter(object sender, MouseEventArgs e)
        {

        }

        private void AddHttpProxy_MouseLeave(object sender, MouseEventArgs e)
        {

        }

        private void AddHttpProxy_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}

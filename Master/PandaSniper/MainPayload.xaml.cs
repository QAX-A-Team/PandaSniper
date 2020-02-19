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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PandaSniper
{
    /// <summary>
    /// MainPayload.xaml 的交互逻辑
    /// </summary>
    public partial class MainPayload : Page
    {
        public MainPayload()
        {
            InitializeComponent();
        }

        public Packages Attacks_Packages;
        public PayloadGeneragor Attacks_PayloadGeneragor;
        public LinuxExecutable Attacks_LinuxExecutable;
        public LinuxExecutableS Attacks_LinuxExecutableS;
        public WebDriveBy Attacks_WebDriveBy;
        public SpearPhish Attacks_SpearPhish;

        public ObservableCollection<ListenersListView> listeners = new ObservableCollection<ListenersListView>() { };

        public UserProfile userProfile;

        private void MainPayload_Loaded(object sender, RoutedEventArgs e)
        {
            this.AutoSizeWindow();
            this.Attacks_Packages = new Packages();
            this.Attacks_PayloadGeneragor = new PayloadGeneragor();
            this.Attacks_LinuxExecutable = new LinuxExecutable();
            this.Attacks_LinuxExecutableS = new LinuxExecutableS();
            this.Attacks_WebDriveBy = new WebDriveBy();
            this.Attacks_SpearPhish = new SpearPhish();
            AttacksChangePage.Content = new Frame()
            {
                Content = this.Attacks_Packages
            };
            MainPayloadListView.ItemsSource = this.listeners;
        }

        public void AutoSizeWindow()
        {
            //listview自动调节头宽度
            foreach (GridViewColumn item in MainPayloadGridView.Columns)
            {
                switch (item.Header)
                {
                    case "name":
                        item.Width = (this.MainPayloadListView.ActualWidth / 100) * 8;
                        break;
                    case "payload":
                        item.Width = (this.MainPayloadListView.ActualWidth / 100) * 12;
                        break;
                    case "hosts":
                        item.Width = (this.MainPayloadListView.ActualWidth / 100) * 15;
                        break;
                    case "port":
                        item.Width = (this.MainPayloadListView.ActualWidth / 100) * 5;
                        break;
                    case "bindto":
                        item.Width = (this.MainPayloadListView.ActualWidth / 100) * 10;
                        break;
                    case "header":
                        item.Width = (this.MainPayloadListView.ActualWidth / 100) * 20;
                        break;
                    case "proxy":
                        item.Width = (this.MainPayloadListView.ActualWidth / 100) * 20;
                        break;
                    case "profile":
                        item.Width = (this.MainPayloadListView.ActualWidth / 100) * 10;
                        break;
                    default:
                        break;
                }
            }
        }

        //监听器事件
        private void ListenerAdd_MouseEnter(object sender, MouseEventArgs e)
        {

        }

        private void ListenerAdd_MouseLeave(object sender, MouseEventArgs e)
        {

        }

        private void ListenerAdd_Click(object sender, RoutedEventArgs e)
        {
            AddListener addListener = new AddListener() { };
            addListener.listeners = this.listeners;
            addListener.userProfile = this.userProfile;
            addListener.TransfEvent += TransfListeners;
            addListener.Show();
        }

        void TransfListeners(ObservableCollection<ListenersListView> listeners)
        {

            this.listeners = listeners;

        }

        private void ListenerEdit_MouseEnter(object sender, MouseEventArgs e)
        {

        }

        private void ListenerEdit_MouseLeave(object sender, MouseEventArgs e)
        {

        }

        private void ListenerEdit_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ListenerRemove_MouseEnter(object sender, MouseEventArgs e)
        {

        }

        private void ListenerRemove_MouseLeave(object sender, MouseEventArgs e)
        {

        }

        private void ListenerRemove_Click(object sender, RoutedEventArgs e)
        {
            string port = ((ListenersListView)MainPayloadListView.SelectedItem).Port;
            DataFormat MessageData;
            MessageData.type = "4";
            MessageData.token = userProfile.token;
            MessageData.data = new Dictionary<string, string> { { "port", port } };
            string sendMessage = JsonConvert.SerializeObject(MessageData);
            SslTcpClient sslTcpClient = new SslTcpClient(userProfile.host, int.Parse(userProfile.port), "localhost");
            sslTcpClient.StartSslTcp();
            SslStream sslStream = sslTcpClient.SendMessage(sendMessage);
            sslTcpClient.ReadMessage(sslStream);

            JObject rMJson = (JObject)JsonConvert.DeserializeObject(sslTcpClient.resultMessage);
            if (rMJson["code"].ToString() == "200")
            {
                MessageBox.Show("删除监听成功");
                this.listeners.Remove((ListenersListView)MainPayloadListView.SelectedItem);
            }
            else
            {
                MessageBox.Show(rMJson["error"].ToString());
                sslTcpClient.CloseSslTcp();
                return;
            }
            sslTcpClient.CloseSslTcp();
        }

        private void ListenerRestart_MouseEnter(object sender, MouseEventArgs e)
        {

        }

        private void ListenerRestart_MouseLeave(object sender, MouseEventArgs e)
        {

        }

        private void ListenerRestart_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ListenerHelp_MouseEnter(object sender, MouseEventArgs e)
        {

        }

        private void ListenerHelp_MouseLeave(object sender, MouseEventArgs e)
        {

        }

        private void ListenerHelp_Click(object sender, RoutedEventArgs e)
        {

        }

        //攻击模块事件

        private TreeViewItem TreeViewItemIsSelected(TreeViewItem treeViewItem)
        {
            if (treeViewItem.IsSelected)
            {
                return treeViewItem;
            }
            if (treeViewItem.HasItems == true)
            {
                foreach (TreeViewItem Item_X in treeViewItem.Items)
                {
                    if (TreeViewItemIsSelected(Item_X).IsSelected)
                    {
                        return Item_X;
                    }
                }
            }
            return treeViewItem;
        }

        private void TI_Attacks_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            bool IsSelected_count = false;
            foreach(TreeViewItem Item in Attack_TreeView.Items)
            {
                if(TreeViewItemIsSelected(Item).IsSelected)
                {
                    IsSelected_count = true;
                }
            }
            if(IsSelected_count == false)
            {
                TVI_Packages.IsSelected = true;
            }
        }

        private void TVI_Packages_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if(TVI_Packages.IsSelected)
            {
                if(TVI_Packages.IsExpanded == false)
                {
                    TVI_Packages.IsExpanded = true;
                }
                else
                {
                    TVI_Packages.IsExpanded = false;
                }  
            }
            AttacksChangePage.Content = new Frame()
            {
                Content = this.Attacks_Packages
            };
        }

        private void TVI_PayloadGeneragor_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            AttacksChangePage.Content = new Frame()
            {
                Content = this.Attacks_PayloadGeneragor
            };
        }

        private void TVI_LinuxExecutable_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            AttacksChangePage.Content = new Frame()
            {
                Content = this.Attacks_LinuxExecutable
            };
        }

        private void TVI_LinuxExecutableS_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            AttacksChangePage.Content = new Frame()
            {
                Content = this.Attacks_LinuxExecutableS
            };
        }

        private void TVI_WebDriveBy_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            AttacksChangePage.Content = new Frame()
            {
                Content = this.Attacks_WebDriveBy
            };
        }

        private void TVI_SpearPhish_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            AttacksChangePage.Content = new Frame()
            {
                Content = this.Attacks_SpearPhish
            };
        }
    }
}

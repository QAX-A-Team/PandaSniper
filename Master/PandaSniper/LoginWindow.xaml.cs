using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PandaSniper
{
    /// <summary>
    /// LoginWindow.xaml 的交互逻辑
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        public JArray jAConfig;
        private void LoginWindow_Loaded(object sender, RoutedEventArgs e)
        {
            string filePath = System.AppDomain.CurrentDomain.BaseDirectory + "/.config";
            if (!File.Exists(filePath))
            {
                ConfigFormat config = new ConfigFormat();
                DataConfigFormat data = new DataConfigFormat();
                List<ConfigFormat> listConfig = new List<ConfigFormat>();
                JsonSerializer serializer = new JsonSerializer();
                data.host = "127.0.0.1";
                data.port = "8443";
                data.user = "bwb";
                data.password = "ssssss";
                config.id = "1";
                config.ip = "New Profile";
                config.data = data;
                listConfig.Add(config);
                using (StreamWriter sw = new StreamWriter(filePath))
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    serializer.Serialize(writer, listConfig);
                }
            }

            JsonSerializer serializer1 = new JsonSerializer();
            using (StreamReader sr = new StreamReader(filePath))
            using (JsonReader reader = new JsonTextReader(sr))
            {
               this.jAConfig = (JArray)serializer1.Deserialize(reader);               
            }

            //绑定listbox数据
            int JAConfigLength = this.jAConfig.Count();
            LoginListBox.ItemsSource = LoadListBoxData();
            ArrayList LoadListBoxData()
            {
                ArrayList itemsList = new ArrayList();

                foreach (var item in this.jAConfig)
                {
                    itemsList.Add(item["ip"]);
                    if(item["id"].ToString() == JAConfigLength.ToString())
                    {
                        LoginHost.Text = item["data"]["host"].ToString();
                        LoginPort.Text = item["data"]["port"].ToString();
                        LoginUser.Text = item["data"]["user"].ToString();
                        LoginPassword.Password = item["data"]["password"].ToString();
                    }
                }
                return itemsList;
            }

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

        private void LoginListBox_PreviewMouseUp(object sender, RoutedEventArgs e)
        {
            if (LoginListBox.SelectedItem != null)
            {
                foreach(var item in this.jAConfig)
                {
                    String ip = LoginListBox.SelectedItem.ToString();
                    if(ip == item["ip"].ToString())
                    {
                        LoginHost.Text = item["data"]["host"].ToString();
                        LoginPort.Text = item["data"]["port"].ToString();
                        LoginUser.Text = item["data"]["user"].ToString();
                        LoginPassword.Password = item["data"]["password"].ToString();
                    }
                }
                
            }
        }



        private void LoginConnect_MouseEnter(object sender, MouseEventArgs e)
        {

        }

        private void LoginConnect_MouseLeave(object sender, MouseEventArgs e)
        {

        }

        private void LoginConnect_Click(object sender, RoutedEventArgs e)
        {
            string serverCertificateName = "localhost";
            string machineName = LoginHost.Text;
            int machinePort = int.Parse(LoginPort.Text);
            string loginUser = LoginUser.Text;
            string loginPassword = LoginPassword.Password;
            string loginHash = Md5.EncryptString(loginPassword);
            SslTcpClient sslTcpClient = new SslTcpClient(machineName, machinePort, serverCertificateName);
            sslTcpClient.StartSslTcp();
            DataFormat MessageData;
            MessageData.type = "0";
            MessageData.token = "";
            MessageData.data = new Dictionary<string, string> { { "user", loginUser }, { "hash", loginHash } };
            string sendMessage = JsonConvert.SerializeObject(MessageData);
            //Console.WriteLine(sendMessage);
            sslTcpClient.ReadMessage(sslTcpClient.SendMessage(sendMessage));
            //Console.WriteLine(sslTcpClient.resultMessage);
            JObject rMJson = (JObject)JsonConvert.DeserializeObject(sslTcpClient.resultMessage);
            
            if (rMJson["code"].ToString() == "504")
            {
                MessageBox.Show("服务器不能连接，请检测是否启动Agent");
            }


            if (rMJson["code"].ToString() == "401")
            {
                MessageBox.Show(rMJson["error"].ToString());
            }

            if (rMJson["code"].ToString() == "500")
            {
                MessageBox.Show(rMJson["error"].ToString());
            }

            if (rMJson["code"].ToString() == "200")
            {
                string filePath = System.AppDomain.CurrentDomain.BaseDirectory + "/.config";
                JsonSerializer serializer = new JsonSerializer();

                ConfigFormat config = new ConfigFormat();
                DataConfigFormat data = new DataConfigFormat();
                List<ConfigFormat> listConfig = new List<ConfigFormat>();

                bool isExists = false;

                foreach (var item in this.jAConfig)
                {
                    data.host = item["data"]["host"].ToString();
                    data.port = item["data"]["port"].ToString();
                    data.user = item["data"]["user"].ToString();
                    data.password = item["data"]["password"].ToString();
                    config.id = item["id"].ToString();
                    config.ip = item["ip"].ToString();
                    config.data = data;
                    listConfig.Add(config);
                    if(config.ip == machineName)
                    {
                        isExists = true;
                    }
                }
                
                if(isExists == false)
                {
                    data.host = machineName;
                    data.port = machinePort.ToString();
                    data.user = loginUser;
                    data.password = loginPassword;
                    int id_count = jAConfig.Count + 1;
                    config.id = id_count.ToString();
                    config.ip = machineName;
                    config.data = data;
                    listConfig.Add(config);
                }
                

                using (StreamWriter sw = new StreamWriter(filePath))
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    serializer.Serialize(writer, listConfig);
                }

                UserProfile userProfile = new UserProfile()
                {
                    token = rMJson["result"].ToString(),
                    host = machineName,
                    port = machinePort.ToString(),
                    user = loginUser,
                    password = loginPassword,
                    sslTcpClient = sslTcpClient,  
                };

                MainWindow mainWindow = new MainWindow()
                {
                    userProfile=userProfile,
                };
                this.Close();
                mainWindow.ShowDialog();
            }
        }

        private void LoginClose_MouseEnter(object sender, MouseEventArgs e)
        {
            
        }

        private void LoginClose_MouseLeave(object sender, MouseEventArgs e)
        {

        }

        private void LoginClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}

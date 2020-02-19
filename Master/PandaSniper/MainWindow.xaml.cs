using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Security;
using System.Text;
using System.Threading;
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
using System.Windows.Threading;

namespace PandaSniper
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainPage Mainpage;
        public MainPayload Payloadpage;
        public MainSetting Settingpage;

        public UserProfile userProfile;
        
        public ObservableCollection<ListenersListView> listeners = new ObservableCollection<ListenersListView>() { };

        //public List<TargetListView> targetListViews = new List<TargetListView>();

        public MainWindow()
        {
            InitializeComponent();
        }

        public class GetImplantResultClass
        {
            public UserProfile userProfile;
            public AsyncObservableCollection<TargetListView> targetListViews;
            public EventsContent eventsContent;
            //public ListView BodySessionListView;


            public void GetImplantResult()
            {
                DataFormat MessageData;
                MessageData.type = "1";
                MessageData.token = userProfile.token;
                MessageData.data = null;
                string sendMessage = JsonConvert.SerializeObject(MessageData);
                bool isGo = true;
                Thread.CurrentThread.IsBackground = true;
                SslTcpClient sslTcpClient = userProfile.sslTcpClient;
                do
                {
                    SslStream sslStream = sslTcpClient.SendMessage(sendMessage);
                    sslTcpClient.ReadMessage(sslStream);
                    JObject rMJson = (JObject)JsonConvert.DeserializeObject(sslTcpClient.resultMessage);
                    if (rMJson["code"].ToString() == "200")
                    {
                        foreach (var item in rMJson["result"])
                        {
                            TimeSpan ts = DateTime.Now - Function.GetDateTime(item["time"].ToString());
                            string invalTs = ts.Seconds.ToString() + "s";
                            
                            if (ts.Minutes != 0)
                            {
                                invalTs = ts.Minutes.ToString() + "m " + invalTs;
                                if (ts.Hours != 0)
                                {
                                    invalTs = ts.Hours.ToString() + "h " + invalTs;
                                    if (ts.Days != 0)
                                    {
                                        invalTs = ts.Days.ToString() + "d " + invalTs;
                                    }
                                }
                            }
                            
                            TargetListView tLV = new TargetListView(
                                item["country"].ToString(),
                                item["ip"].ToString(),
                                item["innerip"].ToString(),
                                item["pid"].ToString(),
                                item["user"].ToString(),
                                item["osinfo"].ToString(),
                                item["cpuinfo"].ToString(),
                                invalTs
                             )
                            {
                                uid = item["uid"].ToString(),
                                time = item["time"].ToString()
                            };
                            //Console.WriteLine(Function.GetDateTime(item["time"].ToString()).ToString());
                            bool isE = false;
                            foreach(TargetListView tlv in targetListViews)
                            {
                                if(tlv.uid == tLV.uid)
                                {
                                    if (tlv.Country != tLV.Country)
                                    {
                                        targetListViews.ElementAt(targetListViews.IndexOf(tlv)).Country = tLV.Country;
                                    }

                                    if (tlv.ExternalIP != tLV.ExternalIP)
                                    {
                                        targetListViews.ElementAt(targetListViews.IndexOf(tlv)).ExternalIP = tLV.ExternalIP;
                                    }
                                    if (tlv.InternalIP != tLV.InternalIP)
                                    {
                                        targetListViews.ElementAt(targetListViews.IndexOf(tlv)).InternalIP = tLV.InternalIP;
                                    }
                                    if (tlv.Pid != tLV.Pid)
                                    {
                                        targetListViews.ElementAt(targetListViews.IndexOf(tlv)).Pid = tLV.Pid;
                                    }
                                    if (tlv.User != tLV.User)
                                    {
                                        targetListViews.ElementAt(targetListViews.IndexOf(tlv)).User = tLV.User;
                                    }
                                    if (tlv.Computer != tLV.Computer)
                                    {
                                        targetListViews.ElementAt(targetListViews.IndexOf(tlv)).Computer = tLV.Computer;
                                    }
                                    if (tlv.Arch != tLV.Arch)
                                    {
                                        targetListViews.ElementAt(targetListViews.IndexOf(tlv)).Arch = tLV.Arch;
                                    }
                                    //if (tlv.time != tLV.time)
                                    //{
                                    targetListViews.ElementAt(targetListViews.IndexOf(tlv)).Last = tLV.Last;
                                    //}
                                    isE = true;
                                }
                            } 
                            if(isE == false || targetListViews.Count == 0)
                            {  
                                targetListViews.Add(tLV);
                                string events = "[" + tLV.Country + "] " + Function.GetDateTime(tLV.time).ToString() + "  " + tLV.InternalIP + "(" + tLV.computer.Trim() + ")  Online"; 
                                if(eventsContent.Content == "")
                                {
                                    eventsContent.Content = events + "\n";
                                }
                                else
                                {
                                    eventsContent.Content = eventsContent.Content + events + "\n";
                                }
                                //this.BodySessionListView.Dispatcher.Invoke(new Action(() => { this.BodySessionListView.Items.Clear(); this.BodySessionListView.ItemsSource = this.targetListViews; }));
                            }
                        }
                        //Thread.Sleep(1000);
                    }
                } while (isGo);

            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Mainpage = new MainPage();
            this.Payloadpage = new MainPayload();
            this.Settingpage = new MainSetting();

            //
            this.Mainpage.userProfile = this.userProfile;
            this.Payloadpage.userProfile = this.userProfile;
            this.Payloadpage.listeners = this.listeners;

            ChangePage.Content = new Frame()
            {
                Content = this.Mainpage
            };
            //检测是否有listeners并拉去
            
            DataFormat MessageData;
            MessageData.type = "6";
            MessageData.token = userProfile.token;
            MessageData.data = null;
            string sendMessage = JsonConvert.SerializeObject(MessageData);
            userProfile.sslTcpClient.ReadMessage(userProfile.sslTcpClient.SendMessage(sendMessage));
            JObject rMJson = (JObject)JsonConvert.DeserializeObject(userProfile.sslTcpClient.resultMessage);
            if (rMJson["code"].ToString() == "200")
            {
                foreach (var item in rMJson["result"])
                {
                    if ((bool)item["status"])
                    {
                        ListenersListView LLV = new ListenersListView(
                        Function.GenerateRandomString(6),
                        "",
                        "",
                        "",
                        item["port"].ToString(),
                        "",
                        "",
                        "",
                        ""
                        )
                        {
                        };
                        this.listeners.Add(LLV);
                    }
                    
                    //Console.WriteLine(Function.GetDateTime(item["time"].ToString()).ToString()); 
                }
                //Thread.Sleep(1000);
            }
            else if(rMJson["code"].ToString() == "500")
            {
                MessageBox.Show(rMJson["error"].ToString());
            }
            else if (rMJson["code"].ToString() == "401")
            {
                MessageBox.Show(rMJson["error"].ToString());
            }

            //拉取implant
            AsyncObservableCollection<TargetListView> targetListViews = new AsyncObservableCollection<TargetListView>();
            this.Mainpage.BodySessionListView.ItemsSource = targetListViews;

            EventsContent eventsContent = new EventsContent() { };
            this.Mainpage.EventsTextBox.DataContext = eventsContent;

            GetImplantResultClass myThread = new GetImplantResultClass
            {
                userProfile = this.userProfile,
                targetListViews = targetListViews,
                eventsContent = eventsContent
                //BodySessionListView = this.Mainpage.BodySessionListView
            };
            
            
            Thread thread = new Thread(myThread.GetImplantResult);
            thread.Start();



            //Console.WriteLine(this.userProfile.sslTcpClient.resultMessage);
        }

        private void AutoSizeWindow()
        {
            this.Mainpage.AutoSizeWindow();
            this.Payloadpage.AutoSizeWindow();
        }


        private void WindowTitle_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        int i = 0;
        private void WindowTitle_MouseDown(object sender, MouseButtonEventArgs e)
        {

            i += 1;
            System.Windows.Threading.DispatcherTimer timer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = new TimeSpan(0, 0, 0, 0, 300)
            };
            timer.Tick += (s, e1) => { timer.IsEnabled = false; i = 0; };
            timer.IsEnabled = true;
            if (i % 2 == 0)
            {
                timer.IsEnabled = false;
                i = 0;
                if (IsWindowMaxSize())
                {
                    ToWindowNormal();
                }
                else
                {
                    ToWindowMaxSize();
                }
            }
        }

        private void WindowClose_MouseEnter(object sender, MouseEventArgs e)
        {

            this.WindowCloseIcon.Visibility = Visibility.Visible;
            this.WindowMinSizeIcon.Visibility = Visibility.Visible;
            this.WindowMaxSizeIcon.Visibility = Visibility.Visible;
        }
        private void WindowClose_MouseLeave(object sender, MouseEventArgs e)
        {

            this.WindowCloseIcon.Visibility = Visibility.Hidden;
            this.WindowMinSizeIcon.Visibility = Visibility.Hidden;
            this.WindowMaxSizeIcon.Visibility = Visibility.Hidden;
        }

        private void WindowMinSize_MouseEnter(object sender, MouseEventArgs e)
        {

            this.WindowCloseIcon.Visibility = Visibility.Visible;
            this.WindowMinSizeIcon.Visibility = Visibility.Visible;
            this.WindowMaxSizeIcon.Visibility = Visibility.Visible;
        }
        private void WindowMinSize_MouseLeave(object sender, MouseEventArgs e)
        {
            this.WindowCloseIcon.Visibility = Visibility.Hidden;
            this.WindowMinSizeIcon.Visibility = Visibility.Hidden;
            this.WindowMaxSizeIcon.Visibility = Visibility.Hidden;
        }

        private void WindowMaxSize_MouseEnter(object sender, MouseEventArgs e)
        {

            this.WindowCloseIcon.Visibility = Visibility.Visible;
            this.WindowMinSizeIcon.Visibility = Visibility.Visible;
            this.WindowMaxSizeIcon.Visibility = Visibility.Visible;
        }
        private void WindowMaxSize_MouseLeave(object sender, MouseEventArgs e)
        {
            this.WindowCloseIcon.Visibility = Visibility.Hidden;
            this.WindowMinSizeIcon.Visibility = Visibility.Hidden;
            this.WindowMaxSizeIcon.Visibility = Visibility.Hidden;
        }

        private void WindowClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void WindowMinSize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void WindowMaxSize_Click(object sender, RoutedEventArgs e)
        {
            if (IsWindowMaxSize())
            {
                ToWindowNormal();
            }
            else
            {
                ToWindowMaxSize();
            }
        }

        private void ToWindowNormal()
        {
            this.Height = 720;
            this.Width = 1080;
            this.Left = (SystemParameters.WorkArea.Width - 1080) / 2;
            this.Top = (SystemParameters.WorkArea.Height - 720) / 2;
            this.AutoSizeWindow();
        }

        private void ToWindowMaxSize()
        {
            this.Left = 0;
            this.Top = 0;
            this.Height = SystemParameters.WorkArea.Height;
            this.Width = SystemParameters.WorkArea.Width;
            AutoSizeWindow();
        }

        private Boolean IsWindowMaxSize()
        {
            if (this.Height == SystemParameters.WorkArea.Height && this.Width == SystemParameters.WorkArea.Width)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        //菜单栏相关事件
        private void MenuMainPage_MouseEnter(object sender, RoutedEventArgs e)
        {
            this.MenuMainPageIcon.Foreground = new SolidColorBrush(Colors.White);
        }

        private void MenuMainPage_MouseLeave(object sender, RoutedEventArgs e)
        {
            this.MenuMainPageIcon.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF686868"));
        }

        private void MenuMainPage_Click(object sender, RoutedEventArgs e)
        {
            if (this.Mainpage == null)
            {
                this.Mainpage = new MainPage();
            }
            ChangePage.Content = new Frame()
            {

                Content = this.Mainpage
            };
        }

        private void MenuMainPayload_MouseEnter(object sender, RoutedEventArgs e)
        {
            this.MenuMainPayloadIcon.Foreground = new SolidColorBrush(Colors.White);
        }

        private void MenuMainPayload_MouseLeave(object sender, RoutedEventArgs e)
        {
            this.MenuMainPayloadIcon.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF686868"));
        }

        private void MenuMainPayload_Click(object sender, RoutedEventArgs e)
        {
            if (this.Payloadpage == null)
            {
                this.Payloadpage = new MainPayload();
            }
            ChangePage.Content = new Frame()
            {

                Content = this.Payloadpage
            };
        }

        private void MenuMainSetting_MouseEnter(object sender, RoutedEventArgs e)
        {
            this.MenuMainSettingIcon.Foreground = new SolidColorBrush(Colors.White);
        }

        private void MenuMainSetting_MouseLeave(object sender, RoutedEventArgs e)
        {
            this.MenuMainSettingIcon.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF686868"));
        }

        private void MenuMainSetting_Click(object sender, RoutedEventArgs e)
        {
            if (this.Settingpage == null)
            {
                this.Settingpage = new MainSetting();
            }
            ChangePage.Content = new Frame()
            {

                Content = this.Settingpage
            };
        }

        


    }
}

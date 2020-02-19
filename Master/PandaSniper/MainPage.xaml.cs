using MaterialDesignThemes.Wpf;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Reflection;
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
    /// MainPage.xaml 的交互逻辑
    /// </summary>
    public partial class MainPage : Page
    {
        public UserProfile userProfile;
        public Dictionary<string, Thread> ThreadDictionary = new Dictionary<string, Thread>();
        public Dictionary<string, SslTcpClient> sslTcpClients = new Dictionary<string, SslTcpClient>();
        
        
        public MainPage()
        {
            InitializeComponent();
            //BodySessionListView.DoubleBufferedListView(true);
              
        }


        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            this.AutoSizeWindow();
        }

        public void AutoSizeWindow()
        {
            //listview自动调节头宽度
            foreach (GridViewColumn item in BodySessionGridView.Columns)
            {
                switch (item.Header)
                {
                    case "country":
                        item.Width = (this.ActualWidth / 100) * 10;
                        break;
                    case "external":
                        item.Width = (this.ActualWidth / 100) * 12;
                        break;
                    case "internal":
                        item.Width = (this.ActualWidth / 100) * 12;
                        break;
                    case "pid":
                        item.Width = (this.ActualWidth / 100) * 10;
                        break;
                    case "arch":
                        item.Width = (this.ActualWidth / 100) * 10;
                        break;
                    case "last":
                        item.Width = (this.ActualWidth / 100) * 17;
                        break;
                    case "user":
                        item.Width = (this.ActualWidth / 100) * 8;
                        break;
                    case "computer":
                        item.Width = (this.ActualWidth / 100) * 22;
                        break;
                    default:
                        break;
                }
            }
        }

        private void MenuItemInteract_Click(object sender, RoutedEventArgs e)
        {
            bool TabItemisExsits = false;
           TabItem tabItemSelected = new TabItem();
            TargetListView listViewItem = (TargetListView)this.BodySessionListView.SelectedItems[0];
            foreach(TabItem tabItem in BodyControlPanel.Items)
            {
                if(tabItem.Name == ("BeaconTabItem_" + listViewItem.uid))
                {
                    TabItemisExsits = true;
                    tabItemSelected = tabItem;
                }
            }
            if (listViewItem != null && TabItemisExsits == false)
            {
                TabItem BeaconTabItem = new TabItem() { };
                //header
                StackPanel BeaconHeaderStackPanel = new StackPanel() { };
                BeaconHeaderStackPanel.Orientation = Orientation.Horizontal;
                PackIcon packIcon = new PackIcon()
                {
                    Foreground = (Brush)new BrushConverter().ConvertFromString("#FFD4D7D6"),
                    Kind = PackIconKind.FormatAlignLeft,
                    Height = 11,
                    Width = 11,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 0, 5, 0),
                };
                TextBlock textBlock = new TextBlock() { Text = "Beacon("+listViewItem.InternalIP+"#"+ listViewItem.Pid +")" };
                BeaconHeaderStackPanel.Children.Add(packIcon);
                BeaconHeaderStackPanel.Children.Add(textBlock);
                
                //content
                Grid grid = new Grid();
                RowDefinition rowGrid1 = new RowDefinition
                {
                    Height = new GridLength(1, GridUnitType.Star)
                };
                RowDefinition rowGrid2 = new RowDefinition
                {
                    Height = new GridLength(25)
                };
                grid.RowDefinitions.Add(rowGrid1);
                grid.RowDefinitions.Add(rowGrid2);

                Grid grid1 = new Grid();
                Border border = new Border() 
                {
                    BorderThickness = new Thickness(0,0,0,1),
                    BorderBrush = (Brush)new BrushConverter().ConvertFromString("#FF897979")
                };
                grid1.Children.Add(border);

                ScrollViewer stackPanelScrollViewer = new ScrollViewer();
                stackPanelScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;

                StackPanel stackPanel = new StackPanel()
                {
                    Orientation = Orientation.Vertical,
                    Margin = new Thickness(6, 5, 6, 0),
                    Name = "StackPanel_" + listViewItem.uid,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top
                };
                stackPanelScrollViewer.Content = stackPanel;
                grid1.Children.Add(stackPanelScrollViewer);
                Grid.SetRow(grid1, 0);

                StackPanel stackPanel1 = new StackPanel()
                {
                    Orientation = Orientation.Horizontal
                };
                Grid.SetRow(stackPanel1, 1);
                PackIcon packIcon1 = new PackIcon()
                {
                    Kind = PackIconKind.KeyboardArrowRight,
                    Height = 25,
                    Width = 20  
                };
                stackPanel1.Children.Add(packIcon1);

                TextBox textBox = new TextBox()
                {
                    Name = "BeaconTextBox_" + listViewItem.uid,
                    Width = this.ActualWidth - 20,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Center  
                };
                textBox.KeyDown += new KeyEventHandler(BeaconTextBox_KeyDown);
                stackPanel1.Children.Add(textBox);

                grid.Children.Add(grid1);
                grid.Children.Add(stackPanel1);

                //add
                BeaconTabItem.Header = BeaconHeaderStackPanel;
                BeaconTabItem.Content = grid;
                BeaconTabItem.Name = "BeaconTabItem_" + listViewItem.uid;

                BodyControlPanel.Items.Add(BeaconTabItem);
                BodyControlPanel.SelectedItem = BeaconTabItem;
                string textBoxName = "BeaconTextBox_" + listViewItem.uid;
                if (this.sslTcpClients == null || !this.sslTcpClients.ContainsKey(textBoxName))
                {
                    SslTcpClient sslTcpClient = new SslTcpClient(this.userProfile.host, int.Parse(this.userProfile.port), "localhost");
                    sslTcpClient.StartSslTcp();
                    this.sslTcpClients.Add(textBoxName, sslTcpClient);
                }
                
            }
            else
            {
                BodyControlPanel.SelectedItem = tabItemSelected;
            }
        }

        private void TabItemClose_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if(BodyControlPanel.SelectedIndex != -1 && BodyControlPanel.SelectedIndex != 0)
            {
                string uid = ((TabItem)BodyControlPanel.SelectedItem).Name.Split('_')[1];
                List<string> threadKey = new List<string>();
                foreach(var item in this.ThreadDictionary) 
                {
                    if (item.Key.Split('_')[0] == uid) 
                    {
                        threadKey.Add(item.Key);
                    }
                }
                foreach (string list in threadKey)
                {
                    Thread thread = this.ThreadDictionary[list];
                    thread.Abort();
                    this.ThreadDictionary.Remove(list);
                    
                }
                BodyControlPanel.Items.Remove(BodyControlPanel.SelectedItem);
            }
        }


        public class GetCommandResultClass
        {
            public string uid;
            public string execid;
            public UserProfile userProfile;
            public StackPanel stackPanel;

            public void GetCommandResult()
            {
                DataFormat MessageData;
                MessageData.type = "5";
                MessageData.token = this.userProfile.token;
                MessageData.data = new Dictionary<string, string> { { "uid", this.uid}, { "execid", this.execid } };
                string sendMessage = JsonConvert.SerializeObject(MessageData);
                bool isGo = true;
                Thread.CurrentThread.IsBackground = true;
                SslTcpClient sslTcpClient = new SslTcpClient(userProfile.host,int.Parse(userProfile.port), "localhost");
                sslTcpClient.StartSslTcp();
                do
                {
                    SslStream sslStream = sslTcpClient.SendMessage(sendMessage);
                    sslTcpClient.ReadMessage(sslStream);
                    JObject rMJson = (JObject)JsonConvert.DeserializeObject(sslTcpClient.resultMessage);
                    if (rMJson["code"].ToString() == "200")
                    {
                        App.Current.Dispatcher.Invoke((Action)(() =>
                        {
                            TextBlock textBlock = new TextBlock()
                            {
                                Text = "[" + execid + "] Result: \n" + Function.GetChsFromHex(rMJson["result"].ToString())
                            };
                            //"StackPanel_" + uid
                            this.stackPanel.Children.Add(textBlock);
                        }));
                        isGo = false;
                        sslTcpClient.CloseSslTcp();
                    }
                    if (rMJson["code"].ToString() == "500" || rMJson["code"].ToString() == "401" || rMJson["code"].ToString() == "404") {
                        isGo = false;
                        MessageBox.Show(rMJson["error"].ToString());
                    }
                    Thread.Sleep(2000);
                } while (isGo);
                
            }
        }

        private void BeaconTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)//如果输入的是回车键
            {
                TextBox textBox = (TextBox)sender;
                string uid = textBox.Name.Split('_')[1];
                if(textBox.Text.Trim() != "")
                {
                    SslTcpClient sslTcpClient = this.sslTcpClients[textBox.Name];
                    DataFormat MessageData;
                    MessageData.type = "2";
                    MessageData.token = userProfile.token;
                    MessageData.data = new Dictionary<string, string> { { "uid", uid }, { "cmd", textBox.Text.Trim() } };
                    string sendMessage = JsonConvert.SerializeObject(MessageData);
                    sslTcpClient.ReadMessage(sslTcpClient.SendMessage(sendMessage));
                    JObject rMJson = (JObject)JsonConvert.DeserializeObject(sslTcpClient.resultMessage);
                    if (rMJson["code"].ToString() == "200")
                    {
                        string execid = rMJson["result"].ToString();
                        if (execid != "")
                        {

                            TextBlock textBlock = new TextBlock()
                            {
                                Text = "[" + execid + "] Command: " + textBox.Text.Trim()
                            };
                            Grid grid = (Grid)BodyControlPanel.SelectedContent;
                            Grid grid1 = (Grid)grid.Children[0];
                            ScrollViewer scrollViewerStackPanel = (ScrollViewer)grid1.Children[1];
                            StackPanel stackPanel = (StackPanel)scrollViewerStackPanel.Content;
                            stackPanel.Children.Add(textBlock);
                            textBox.Text = "";
                            GetCommandResultClass myThread = new GetCommandResultClass
                            {
                                uid = uid,
                                execid = execid,
                                userProfile = this.userProfile,
                                stackPanel = stackPanel
                            };
                            Thread thread = new Thread(myThread.GetCommandResult);
                            thread.Start();
                            this.ThreadDictionary.Add(uid+ "_"+ Function.GenerateRandomString(32),thread);   
                        }
                    }

                }
            }
        }
    }
}

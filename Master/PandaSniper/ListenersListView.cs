using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PandaSniper
{
    public class ListenersListView : INotifyPropertyChanged
    {
        public string name;
        public string payload;
        public string hosts;
        public string stagerHost;
        public string port;
        public string bindto;
        public string profile;
        public string header;
        public string proxy;
        public event PropertyChangedEventHandler PropertyChanged;

        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("Name"));
                }
            }
        }
        public string Payload
        {
            get
            {
                return payload;
            }
            set
            {
                payload = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("Payload"));
                }
            }
        }
        public string Hosts
        {
            get
            {
                return hosts;
            }
            set
            {
                hosts = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("Hosts"));
                }
            }
        }
        public string StagerHost
        {
            get
            {
                return stagerHost;
            }
            set
            {
                stagerHost = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("StagerHost"));
                }
            }
        }
        public string Port
        {
            get
            {
                return port;
            }
            set
            {
                port = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("Port"));
                }
            }
        }
        public string BindTo
        {
            get
            {
                return bindto;
            }
            set
            {
                bindto = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("BindTo"));
                }
            }
        }
        public string Profile
        {
            get
            {
                return profile;
            }
            set
            {
                profile = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("Profile"));
                }
            }
        }
        public string Header
        {
            get
            {
                return header;
            }
            set
            {
                header = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("Header"));
                }
            }
        }

        public string Proxy
        {
            get
            {
                return proxy;
            }
            set
            {
                proxy = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("Proxy"));
                }
            }
        }

        public ListenersListView() { }
        public ListenersListView(
            string name,
            string payload,
            string hosts,
            string stagerHost,
            string port,
            string bindto,
            string profile,
            string header,
            string proxy)
        {
            this.name = name;
            this.payload = payload;
            this.hosts = hosts;
            this.stagerHost = stagerHost;
            this.port = port;
            this.bindto = bindto;
            this.profile = profile;
            this.header = header;
            this.proxy = proxy;
        }
    }
}

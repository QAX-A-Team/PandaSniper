using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PandaSniper
{
    public class TargetListView : INotifyPropertyChanged
    {
        public string uid;
        public string time;
        public string country;
        public string externalIP;
        public string internalIP;
        public string pid;
        public string user;
        public string computer;
        public string arch;
        public string last;
        public event PropertyChangedEventHandler PropertyChanged;

        public string Country
        {
            get
            {
                return country;
            }
            set
            {
                country = value;
                if (this.PropertyChanged != null)  
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("Country"));
                }
            }
        }
        public string ExternalIP
        {
            get
            {
                return externalIP;
            }
            set
            {
                externalIP = value;
                if (this.PropertyChanged != null)  
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("ExternalIP"));
                }
            }
        }
        public string InternalIP
        {
            get
            {
                return internalIP;
            }
            set
            {
                internalIP = value;
                if (this.PropertyChanged != null) 
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("InternalIP"));
                }
            }
        }
        public string Pid
        {
            get
            {
                return pid;
            }
            set
            {
                pid = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("Pid"));
                }
            }
        }
        public string User
        {
            get
            {
                return user;
            }
            set
            {
                user = value;
                if (this.PropertyChanged != null) 
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("User"));
                }
            }
        }
        public string Computer
        {
            get
            {
                return computer;
            }
            set
            {
                computer = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("Computer"));
                }
            }
        }
        public string Arch
        {
            get
            {
                return arch;
            }
            set
            {
                arch = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("Arch"));
                }
            }
        }
        public string Last
        {
            get
            {
                return last;
            }
            set
            {
                last = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("Last"));
                }
            }
        }

        public TargetListView() { }
        public TargetListView(
            string country, 
            string externalIP, 
            string internalIP, 
            string pid, 
            string user, 
            string computer, 
            string arch, 
            string last)
        {
            this.country = country;
            this.externalIP = externalIP;
            this.internalIP = internalIP;
            this.pid = pid;
            this.user = user;
            this.computer = computer;
            this.arch = arch;
            this.last = last;
        }
    }

    public class AsyncObservableCollection<T> : ObservableCollection<T>
    {
        //获取当前线程的SynchronizationContext对象
        private SynchronizationContext _synchronizationContext = SynchronizationContext.Current;
        public AsyncObservableCollection() { }
        public AsyncObservableCollection(IEnumerable<T> list) : base(list) { }
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {

            if (SynchronizationContext.Current == _synchronizationContext)
            {
                //如果操作发生在同一个线程中，不需要进行跨线程执行         
                RaiseCollectionChanged(e);
            }
            else
            {
                //如果不是发生在同一个线程中
                //准确说来，这里是在一个非UI线程中，需要进行UI的更新所进行的操作         
                _synchronizationContext.Post(RaiseCollectionChanged, e);
            }
        }
        private void RaiseCollectionChanged(object param)
        {
            // 执行         
            base.OnCollectionChanged((NotifyCollectionChangedEventArgs)param);
        }
        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (SynchronizationContext.Current == _synchronizationContext)
            {
                // Execute the PropertyChanged event on the current thread             
                RaisePropertyChanged(e);
            }
            else
            {
                // Post the PropertyChanged event on the creator thread             
                _synchronizationContext.Post(RaisePropertyChanged, e);
            }
        }
        private void RaisePropertyChanged(object param)
        {
            // We are in the creator thread, call the base implementation directly         
            base.OnPropertyChanged((PropertyChangedEventArgs)param);
        }
    }

    public class EventsContent : INotifyPropertyChanged
    {
        public string content;
        public event PropertyChangedEventHandler PropertyChanged;
        public string Content
        {
            get
            {
                return content;
            }
            set
            {
                content = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("Content"));
                }
            }
        }
    }


}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PandaSniper
{
    public struct DataFormat
    {
        public string type;
        public string token;
        public Dictionary<string, string> data;
    }

    public struct ConfigFormat
    {
        public string id;
        public string ip;
        public DataConfigFormat data;
    }

    public struct DataConfigFormat
    {
        public string host;
        public string port;
        public string user;
        public string password;
    }

    public struct UserProfile
    {
        public string token;
        public string user;
        public string host;
        public string port;
        public string password;
        public SslTcpClient sslTcpClient;
    }
}

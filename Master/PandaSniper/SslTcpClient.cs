using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.IO;

namespace PandaSniper
{
    public class SslTcpClient
    {
        public string machineName = null;
        public int machinePort = 443;
        public string serverName = null;
        public TcpClient tcpClient = null;
        public SslStream sslStream = null;
        public string resultMessage = null;
        
        //private static Hashtable certificateErrors = new Hashtable();
        //构造函数
        public SslTcpClient(string machineName, int machinePort, string serverName)
        {
            this.machineName = machineName;
            this.machinePort = machinePort;
            this.serverName = serverName;
        }
        // The following method is invoked by the RemoteCertificateValidationDelegate.
        public static bool ValidateServerCertificate(
              object sender,
              X509Certificate certificate,
              X509Chain chain,
              SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
                return true;

            // Console.WriteLine("Certificate error: {0}", sslPolicyErrors);

            // Do not allow this client to communicate with unauthenticated servers.
            //忽略证书验证
            return true;
        }
        public void StartSslTcp()
        {
            // Create a TCP/IP client socket.
            // machineName is the host running the server application.
            try
            {
                this.tcpClient = new TcpClient(this.machineName, this.machinePort);
                //Console.WriteLine("Client connected.");
                // Create an SSL stream that will close the client's stream.
                 this.sslStream = new SslStream(
                    this.tcpClient.GetStream(),
                    false,
                    new RemoteCertificateValidationCallback(ValidateServerCertificate),
                    null
                    );
                this.sslStream.ReadTimeout = 5000;
                this.sslStream.WriteTimeout = 5000;
                // The server name must match the name on the server certificate.
                try
                {
                    this.sslStream.AuthenticateAsClient(serverName);
                }
                catch (AuthenticationException e)
                {
                    Console.WriteLine("Exception: {0}", e.Message);
                    if (e.InnerException != null)
                    {
                        Console.WriteLine("Inner exception: {0}", e.InnerException.Message);
                    }
                    Console.WriteLine("Authentication failed - closing the connection.");
                    this.tcpClient.Close();    
                }
            }
            catch (SocketException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        
        public SslStream SendMessage(string message)
        {
            // Encode a test message into a byte array.
            // Signal the end of the message using the "<EOF>".
            byte[] byte_message = Encoding.UTF8.GetBytes(message);
            // Send hello message to the server.
            if(this.sslStream == null)
            {
                this.resultMessage = "{\"code\":\"504\"}";
            }
            else
            {
                this.sslStream.Write(byte_message);
                this.sslStream.Flush();
                // Read message from the server.
                //this.ReadMessage(this.sslStream);
                //Console.WriteLine("Server says: {0}", serverMessage);
            }
            return this.sslStream;


        }
        //static StringBuilder readData = new StringBuilder();
        //static byte[] buffer = new byte[2048];

        public void ReadMessage(SslStream sslStream)
        {
            // Read the  message sent by the server.
            // The end of the message is signaled using the
            // "<EOF>" marker.
            StringBuilder messageData = new StringBuilder();
            if (sslStream == null)
            {
                this.resultMessage = "{\"code\":\"504\"}";
            }
            else
            {
                try
                {
                    int bytes;
                    do
                    {
                        byte[] buffer = new byte[2048];
                        bytes = sslStream.Read(buffer, 0, buffer.Length);
                        // Use Decoder class to convert from bytes to UTF8
                        // in case a character spans two buffers.
                        Decoder decoder = Encoding.UTF8.GetDecoder();
                        char[] chars = new char[decoder.GetCharCount(buffer, 0, bytes)];
                        decoder.GetChars(buffer, 0, bytes, chars, 0);
                        messageData.Append(chars); 
                        if(messageData.ToString().IndexOf("<EOF>") != -1)
                        {
                            break;
                        }
                        
                    } while (true);
                    string SmessageData = messageData.ToString();
                    this.resultMessage = SmessageData.Substring(0, SmessageData.Length - 5);
                }
                catch (SocketException ex)
                {
                    this.resultMessage = "{{\"code\":\"504\"},{\"error\":\"" + ex.Message + "\"}}";
                }
                
            }
   
        }

        public void CloseSslTcp()
        {
            // Close the client connection.
            this.sslStream.Close();
            this.tcpClient.Close();
        }
        /* 
        private static void DisplayUsage()
        {
            Console.WriteLine("To start the client specify:");
            Console.WriteLine("clientSync machineName [serverName]");
            Environment.Exit(1);
        }
        public static int Main(string[] args)
        {
            string serverCertificateName = null;
            string machineName = null;
            if (args == null || args.Length < 1)
            {
                DisplayUsage();
            }
            // User can specify the machine name and server name.
            // Server name must match the name on the server's certificate. 
            machineName = args[0];
            if (args.Length < 2)
            {
                serverCertificateName = machineName;
            }
            else
            {
                serverCertificateName = args[1];
            }
            SslTcpClient.RunClient(machineName, serverCertificateName);
            return 0;
        }
        */
    }
}

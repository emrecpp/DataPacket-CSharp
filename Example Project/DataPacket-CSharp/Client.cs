/*
Author: Emre Demircan
Date: 2021-02-09
Github: emrecpp
Version: 1.0.0
*/
using System;
using System.Net;
using System.Net.Sockets;

namespace DataPacket_CSharp
{
    public class Client
    {
        public Socket s = null;
        public bool isConnected
        {
            get
            {
                if (s != null)
                    return s.Connected;
                return false;
            }
            set
            {

            }
        }

        public string IP;
        public int PORT;

        private IPAddress ipAddress;
        private IPEndPoint remoteEP;
        public Client() { }
        public Client(string _IP="127.0.0.1", int _PORT=2000)
        {
            ipAddress = IPAddress.Parse(_IP);
            remoteEP = new IPEndPoint(ipAddress, _PORT);
            TryConnect();
        }

        public bool TryConnect()
        {
            try
            {
                s = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                s.Connect(remoteEP);
                return true;
            }
            catch(Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
                return false;
            }
        }
        public bool CheckIsConntected()
        {
            try
            {
                this.isConnected = s.Connected;
                //this.isConnected = !(s.Poll(1, SelectMode.SelectRead) && s.Available == 0);
            }
            catch (SocketException) { }
            return this.isConnected;
        }
    }
}

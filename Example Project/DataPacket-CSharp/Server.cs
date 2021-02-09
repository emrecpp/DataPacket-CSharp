/*
Author: Emre Demircan
Date: 2021-02-09
Github: emrecpp
Version: 1.0.0
*/
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace DataPacket_CSharp
{
    public class Server
    {
        public Socket s = null;
        public int PORT = 0;
        private const int MAX_CLIENT = 100;


        public bool EXIT = false;
        Func<Socket, int> HandlerClientFunc;
        public Server(int _PORT, Func<Socket, int> _HandlerClientFunc)
        {
            PORT = _PORT;
            HandlerClientFunc = _HandlerClientFunc;

            IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, _PORT);
            s = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);


            s.Bind(remoteEP);
            s.Listen(MAX_CLIENT);
            Thread thr = new Thread(Listen) { IsBackground = true };
            thr.Start();

        }
        ~Server()
        {
            EXIT = true;
        }
        private void Listen()
        {
            while (!EXIT)
            {
                Socket client = s.Accept();
                // Prefer asynchronous socket. This is demo.
                ThreadStart doIt;
                doIt = () => HandlerClientFunc(client);

                Thread THR_NEW_CLIENT = new Thread(doIt) { IsBackground = true };
                THR_NEW_CLIENT.Start();
            }
        }
    }
}

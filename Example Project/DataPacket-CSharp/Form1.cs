using System;
using System.Linq;
using System.Net.Sockets;
using System.Windows.Forms;

namespace DataPacket_CSharp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        public const int PORT = 3000;
        private static class opcodes
        {
            public static int HANDSHAKE = 100;
            public static int REGISTER = 101;
            public static int LOGIN = 102;
        }
        public int ClientHandler(Socket client)
        {
            Packet pktReceiver = new Packet();
            while (client.Connected)
            {
                if (!pktReceiver.Recv(client))
                    break;

                if (pktReceiver.GetOpcode() == 0xAABB)
                {
                    int NUMBER = pktReceiver.readInt();
                    string NAME = pktReceiver.readString();
                    MessageBox.Show(String.Format("Got Packet NAME: {0} Number: {1}", NAME, NUMBER));
                }
            }
            return 0;
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            /*Func<Socket, int> Func_ClientHandler = s =>
            {
                return ClientHandler(s);                
            };*/

            Server server = new Server(PORT, ClientHandler);

            Client client = new Client("127.0.0.1", PORT);

            Packet sendData = new Packet(0xAABB);
            sendData.writeInt(123);
            sendData.writeString("Emre Demircan");
            //sendData.Encrypt();

            if (!sendData.Send(client.s)) // Client -> Server
                MessageBox.Show("Send Failed");
        }
    }
}

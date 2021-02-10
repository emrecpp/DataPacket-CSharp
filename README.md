# Data Packet for C#
Store data as packet. Send, Recv, Encrypt it.

For Pyhon: https://github.com/emrecpp/PacketHandler

For C++: https://github.com/emrecpp/DataPacket-CPP


# Example Usage

```
private static class opcodes
{
    public static int HANDSHAKE = 100;
}
        
Packet pkt = new Packet(opcodes.HANDSHAKE, littleEndian: false);
pkt.writeInt(123456);
pkt.writeString("Emre Demircan");
  
int Number = pkt.readInt(); // 123456
string Name = pkt.readString(); // Emre Demircan



```
## Send with Socket
```
Client client = new Client("127.0.0.1", 2000);

Packet sendData = new Packet(0xAABB);
sendData.writeInt(123456);
sendData.writeString("Emre Demircan");
sendData.Encrypt(); // automatically will be decrypted when received packet.

if (!sendData.Send(client.s)) // Client -> Server
    MessageBox.Show("Send Failed");
    

```


## Recv with Socket
```
public int ClientHandler(Socket client)
{
    Packet pktReceiver = new Packet();
    while (client.Connected)
    {
        if (!pktReceiver.Recv(client))
            break;

        if (pktReceiver.GetOpcode() == 0xAABB)
        {
            int Number = pktReceiver.readInt(); //123456
            string Name = pktReceiver.readString(); // Emre Demircan
            
            Console.WriteLine("Opcode: "+ pktReceiver.GetOpcode());            
            Console.WriteLine("Number:" + Number);
            Console.WriteLine("Name:" + Name);            
        }
    }
    return 0;
}



Server server = new Server(2000, ClientHandler);

```

# Output:
```
Opcode: 43707 (0xAABB)
Number: 123456
Name: Emre Demircan


sendData.Print();

Little Endian


Normal/Decrypted Print:
00000000 AA BB 00 02 00 00 7B 00 00 00 0D 00 00 00 45 6D   ??..?{?.?.?.?Em
00000010 72 65 20 44 65 6D 69 72 63 61 6E                  re.Demircan

Encrypted Print:
00000000 AA BB 01 02 00 00 3F C0 BC B8 C1 B0 AC A8 E9 0D   ??..??????????
00000010 0E FD B4 D4 F1 F5 ED F2 DF D9 E2                  .??????????



Big Endian


Normal/Decrypted Print:
00000000 AA BB 00 02 00 00 00 00 00 7B 00 00 00 0D 45 6D   ??..?.?.{.?.Em
00000010 72 65 20 44 65 6D 69 72 63 61 6E                  re.Demircan 

Encrypted Print:
00000000 AA BB 01 02 00 00 C4 C0 BC 33 B4 B0 AC B5 E9 0D   ??..????3?????
00000010 0E FD B4 D4 F1 F5 ED F2 DF D9 E2                  .??????????
```




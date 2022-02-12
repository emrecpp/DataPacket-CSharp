/*	
Author: Emre Demircan	
Date: 2022-01-21	
Github: emrecpp	
Version: 1.0.2.1
*/

using System;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.IO;

namespace DepotDownloader
{
    public class Packet
    {
        public List<Byte> storage = new List<byte>();
        private bool PrintError = true;
        // First 2 bytes : Opcodes [ 0 - (256*256-1) ]
        //       4. byte : Flags
        //       5. byte : Count of Total Data types
        //       6. byte : empty for now
        //       7. byte : empty for now

        const int INDEX_OF_FLAG = 2;
        const int INDEX_OF_COUNT_ELEMENTS = 3;


        private bool isLittleEndian = false;
        private static class Flags
        {
            public static byte Encrypted    = 1;
            public static byte LittleEndian = 2;
        };

        private int _rpos = 6;
        private int _wpos = 6;
        public Packet(int opcode = 0, bool littleEndian = false, bool PrintError = true)
        {
            isLittleEndian = littleEndian;
            this.PrintError = PrintError;
            if (opcode > 256 * 256 - 1)
                throw new Exception("Opcode range: [ 0 - 65535]. Your opcode: " + opcode);

            byte[] buffer = new byte[2 + 4];// first 2 bytes : opcodes, the other 4 bytes: reserved


            buffer[0] = (byte)(opcode >> 8);
            buffer[1] = (byte)opcode;

            buffer[2] = 0;
            buffer[3] = 0;
            buffer[4] = 0;
            buffer[5] = 0;
            if (isLittleEndian)
                buffer[INDEX_OF_FLAG] |= Flags.LittleEndian;
            storage.AddRange(buffer);
            _wpos = 6;
        }

        private byte[] intToEndian(int data)
        {
            var bytes = BitConverter.GetBytes(IPAddress.NetworkToHostOrder(data));
            if (isLittleEndian)
                Array.Reverse(bytes);
            return bytes;
        }

        public void writeString(string data)
        {
            byte[] utf8bytes = Encoding.UTF8.GetBytes(data);

            var strLength = intToEndian(utf8bytes.Length);
            foreach (byte b in strLength)
                storage.Add(b);
            _wpos += strLength.Length;

            storage.AddRange(utf8bytes);
            _wpos += utf8bytes.Length;

            increaseItemCount();
        }

        public void writeInt(int data)
        {
            byte[] asBytes = intToEndian(data);

            foreach (byte b in asBytes)
                storage.Add(b);
            _wpos += asBytes.Length;
            increaseItemCount();
        }

        public void writeBytes(byte[] data)
        {
            foreach (byte b in data)
                storage.Add(b);
            _wpos += data.Length;
            increaseItemCount();
        }

        private void increaseItemCount()
        {
            if (this.storage.Count < INDEX_OF_COUNT_ELEMENTS)
                return;
            byte count = this.storage[INDEX_OF_COUNT_ELEMENTS];
            if (count + 1 < 255)
                this.storage[INDEX_OF_COUNT_ELEMENTS] += 1;
        }

        public string readString()
        {
            int rl = readLength();
            if (rl > this.storage.Count)
                return "";
            byte[] total = new byte[rl];
            for (int i = 0; i < rl; i++)
                total[i] = storage[_rpos + i];

            _rpos += Convert.ToInt32(rl);


            return Encoding.UTF8.GetString(total);
        }
        public List<byte> readBytes()
        {
            int rl = readLength();
            if (rl > this.storage.Count)
                return new List<byte>();
            var q = this.storage.GetRange(this._rpos, Convert.ToInt32(rl)).ToArray();

            _rpos += Convert.ToInt32(rl);
            return new List<byte>(q);
        }
        public int readInt()
        {
            if (this._rpos + 4 > storage.Count)
                return 0;

            var q = this.storage.GetRange(this._rpos, 4).ToArray();
            int ret = 0;
            if (!isLittleEndian)
                ret = BitConverter.ToInt32(q.Reverse().ToArray(), 0);
            else
                ret = BitConverter.ToInt32(q, 0);
            this._rpos += 4;
            return ret;
        }

        private int readLength()
        {
            return Convert.ToInt32(readInt());
        }

        public int size() { return this.storage.Count; }

        public string Print(int maxPerLine = 16, bool utf_8 = true, int Flag = 1 | 2 | 4)
        {
            try
            {
                string Total = "";
                string dumpstr = "";
                for (int addr = 0; addr < size(); addr += maxPerLine)
                {
                    string line = "";
                    int leftBytes = (addr + maxPerLine < size()) ? maxPerLine : size() - addr;
                    var d = storage.GetRange(addr, leftBytes);
                    if ((Flag & 1) == 1)
                        line = string.Format("{0:X8} ", addr);
                    else
                        line = "";

                    if ((Flag & 2) == 2)
                        dumpstr = String.Join(" ", from hstr in d
                                                   select String.Format("{0:X2}", hstr));

                    line += string.Join("", dumpstr.Take(8 * 3));
                    if (d.Count > 8)
                        line += dumpstr.Substring(8 * 3);
                    int pad = 2;
                    if (d.Count < maxPerLine)
                        pad += 3 * (maxPerLine - d.Count);
                    if (d.Count <= 8)
                        pad += 1;
                    line += new string(' ', pad);
                    if ((Flag & 4) == 4)
                    {
                        line += ' ';
                        if (utf_8)
                        {
                            string utf8str = "";
                            try
                            {
                                utf8str = Encoding.UTF8.GetString(storage.GetRange(addr, leftBytes).ToArray());
                            }
                            catch
                            {
                                utf8str = String.Join(" ", String.Format("{0:X2}", storage.GetRange(addr, leftBytes).ToArray()));
                            }
                            var listutf8str = utf8str.ToArray();
                            for (int i = 0; i < listutf8str.Length; i++)
                            {
                                if ((byte)listutf8str[i] <= 0x20)
                                    listutf8str[i] = '.';
                                i += 1;
                            }
                            line += string.Join("", listutf8str);

                        }
                        else
                        {
                            for (int bytes = 0; bytes < d.Count; bytes++)
                            {
                                if (d[bytes] > 0x20 && d[bytes] <= 0x7E)
                                    line += (char)d[bytes];
                                else
                                    line += '.';
                            }

                        }
                    }
                    Total += line + '\n';
                }
                Console.WriteLine("\n" + Total);
                return Total;
            }
            catch (Exception ex)
            {
                if (PrintError) Console.WriteLine("Packet Print: \n" + ex.Message + "\n\n" + ex.StackTrace, "Hata");
                return ex.Message;
            }
        }
        public void Encrypt()
        {
            for (int i = 2 + 4; i < size(); i++) // Skip opcode and reserved 4 bytes
            {
                byte data = storage[i];
                int encVal = 0x123 + i * 4;
                encVal ^= 0xFF;

                storage[i] = (byte)((data + encVal) & 0xFF);
            }
            storage[INDEX_OF_FLAG] |= Flags.Encrypted;
        }
        public void Decrypt()
        {
            for (int i = 2 + 4; i < size(); i++)// Skip opcode and reserved 4 bytes
            {
                byte data = storage[i];
                int encVal = 0x123 + i * 4;
                encVal ^= 0xFF;

                storage[i] = (byte)((data - encVal) & 0xFF);
            }
            storage[INDEX_OF_FLAG] &= (byte)~Flags.Encrypted;
        }
        private bool Send_WaitToReach(Socket s, byte[] bytes)
        {
            long sent = 0;
            while (sent < bytes.Length)
            {
                sent += s.Send(bytes.Skip((int)sent - 1).ToArray());
            }
            return true;
        }
        public bool Send(Socket s)
        {
            try
            {
                if (!s.Connected) return false;
                byte[] bytes = intToEndian(storage.Count);
                long sentBytes = 0;

                //SendWaitToReach(s, bytes);
                s.Send(bytes);


                while (sentBytes < bytes.Length)
                {
                    int lastSentBytes = s.Send(storage.GetRange((int)sentBytes, (int)(storage.Count - sentBytes)).ToArray());
                    if (lastSentBytes < 0)
                        return false;
                    sentBytes += lastSentBytes;
                }
                return true;
            }
            catch
            {
                return false;
            }

        }

        public void Clear()
        {
            this.storage.Clear();
            this._rpos = 6;
            //this.storage.AddRange(new byte[] { 0, 0, 0, 0, 0, 0 });
            //this._wpos = 6;
        }
        public int GetOpcode()
        {
            return ((this.storage.Count > 0) ? (BitConverter.ToInt16(this.storage.GetRange(0, 2).ToArray().Reverse().ToArray(), 0)) : 0) & 0xFFFF;
        }
        public int GetItemsCount()
        {
            return (this.storage.Count > INDEX_OF_COUNT_ELEMENTS) ? (this.storage[INDEX_OF_COUNT_ELEMENTS]) : 0;
        }
        private bool RecvSize(Socket s, ref byte[] buffer, int packetSize)
        {
            int totalBytesReceived = 0;
            while (totalBytesReceived < packetSize)
            {
                int receivedLength = s.Receive(buffer, totalBytesReceived, Convert.ToInt32(packetSize - totalBytesReceived), SocketFlags.Partial);
                if (receivedLength <= 0)
                    return false;
                totalBytesReceived += receivedLength;
            }
            return true;
        }
        public bool Recv(Socket s, bool clear = true)
        {
            try
            {
                if (clear) Clear();

                if (!s.Connected) return false;

                byte[] dataLength = new byte[4];
                if (!RecvSize(s, ref dataLength, 4))
                    return false;


                if (!isLittleEndian) Array.Reverse(dataLength);
                int packetSize = BitConverter.ToInt32(dataLength, 0);

                byte[] bytes = new byte[packetSize];


                if (!RecvSize(s, ref bytes, packetSize))
                    return false;

                foreach (byte b in bytes)
                    this.storage.Add(b);
                if ((bytes[INDEX_OF_FLAG] & Flags.Encrypted) == Flags.Encrypted)
                    Decrypt();

                isLittleEndian = (bytes[INDEX_OF_FLAG] & Flags.LittleEndian) == Flags.LittleEndian;

                _wpos += packetSize;
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}

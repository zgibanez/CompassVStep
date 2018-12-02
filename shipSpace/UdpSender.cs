using System;
using System.Collections.Generic;
using System.Text;

using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace shipSpace
{
    //This implementation is not entirely mine!
    public class UdpSender
    {
        private string IP;
        public int port;

        IPEndPoint remoteEndPoint;
        UdpClient client;

        public UdpSender(int port)
        {
            IP = "127.0.0.1";
            this.port = port;

            remoteEndPoint = new IPEndPoint(IPAddress.Parse(IP), port);
            client = new UdpClient();
        }

        public void sendString(string message)
        {
            try
            {
                byte[] data = Encoding.UTF8.GetBytes(message);
                client.Send(data, data.Length, remoteEndPoint);
                Console.WriteLine("Sending message: " + message);
            }
            catch (Exception err)
            {
                Console.WriteLine(err.ToString());
            }
        }
    }
}

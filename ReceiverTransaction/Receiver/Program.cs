using BussinessLayer;
using Domain;
using Receiver;
using System;

namespace ReceiverSocket
{
    class Program
    {
        static void Main(string[] args)
        {
            
            var socket = new SocketReceiver();
            socket.Connect(Common.ip, Common.port);
            Console.ReadLine();

        }
    }
}



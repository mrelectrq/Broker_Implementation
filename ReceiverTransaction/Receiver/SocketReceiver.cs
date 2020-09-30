using BussinessLayer;
using BussinessLayer.BussinessModels;
using BussinessLayer.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Receiver
{
    public class SocketReceiver
    {
        private Socket _socket;
        public SocketReceiver()
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }
        public void Connect(string IP, int Port)
        {
            _socket.BeginConnect(new IPEndPoint(IPAddress.Parse(IP), Port), ConnectedCallback, null);
            Console.WriteLine("Waiting for connection...");
        }

        private void ConnectedCallback(IAsyncResult ar)
        {
            if (_socket.Connected)
            {
                Console.WriteLine("Receiver is connected to Queue Server...");
                Subscribe();
                StartReceive();
            }
            else
            {
                Console.WriteLine("Error! Can't connect to Queue Server...");

            }
        }
        private void Subscribe()
        {
            TransactionProtocol transaction = new TransactionProtocol();
            transaction.Type_message = MessageType.give;

            var data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(transaction));
            Send(data);
        }


        private void Send(byte[] data)
        {
            try
            {
                _socket.Send(data);
            }
            catch(Exception e)
            {
                Console.WriteLine($"Erorr! Couldn't send data {e.Message}");
            }
        }

        private void StartReceive()
        {
            Settings settings = new Settings();
            settings.Socket = _socket;
            _socket.BeginReceive(settings.Buffer, 0, settings.Buffer.Length, SocketFlags.None, ReceiveCallBack, settings);
        }

        private void ReceiveCallBack(IAsyncResult ar)
        {
            Settings settings = ar.AsyncState as Settings;
            TransactionProtocol transaction = ar.AsyncState as TransactionProtocol;
            try
            {
                SocketError response;
                int buffsize = _socket.EndReceive(ar, out response);
                if (response == SocketError.Success)
                {
                    byte[] payloadbytes = new byte[buffsize];
                    Array.Copy(settings.Buffer, payloadbytes, payloadbytes.Length);

                    PayloadHandler.Handle(payloadbytes,settings);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Can't receive data from Queue Server {e.Message}");
            }
            finally
            {
                try
                {
                    settings.Socket.BeginReceive(settings.Buffer, 0, settings.Buffer.Length, SocketFlags.None, ReceiveCallBack, transaction);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"{e.Message}");
                    settings.Socket.Close();
                }
            }
        }
    }
}
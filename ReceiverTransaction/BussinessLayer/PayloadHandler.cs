using BussinessLayer.BussinessModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json.Serialization;

namespace BussinessLayer
{
    public class PayloadHandler
    {
        public static void Handle(byte[] transactionbytes,Settings settings)
        {

            var transactionstring = Encoding.UTF8.GetString(transactionbytes);
            var data = JsonConvert.DeserializeObject<TransactionProtocol>(transactionstring);
            var transaction = JsonConvert.DeserializeObject<TransactionData>(data.Transaction);
            if (string.IsNullOrWhiteSpace(data.Request_id) || string.IsNullOrWhiteSpace(data.Sender_id)
                || string.IsNullOrWhiteSpace(data.Transaction))
            {
                data.Type_message = Enums.MessageType.error;
                data.Transaction = null;
                data.Message = $"Error! Transaction from {data.Sender_id} to {data.Request_id} at {data.Timestamp} was unsuccessful.";
                var byte_message = ConvertToBytes(data);
                settings.Socket.Send(byte_message);
            }
            else
            {
                var to_send = new TransactionProtocol();
                to_send.Sender_id = data.Sender_id;
                to_send.Request_id = data.Request_id;
                to_send.Type_message = Enums.MessageType.response;
                to_send.Transaction = null;
                to_send.Message = $"Successful! Transaction from {data.Sender_id} to {data.Request_id} at {data.Timestamp} was successful.";
                to_send.Timestamp = DateTime.Now;
                var byte_message = ConvertToBytes(to_send );
                settings.Socket.Send(byte_message);
                Write(data, transaction);
            }
        }
        private static void Write(TransactionProtocol data, TransactionData transaction)
        {

            Console.WriteLine("\n\n" + data.Sender_id + " add to " + data.Request_id + " at " + data.Timestamp);

            Console.WriteLine("Owner: " + transaction.Owner_card_id + "\nReceiver: " + transaction.Recipient_card_id + "\nTransaction: " + transaction.transactionType
                + "\nCcy: " + transaction.Ccy + "\nSum: " + transaction.Transaction_summ +"\nComment: " +transaction.Aditional_comment);

        }
        private static byte[] ConvertToBytes(TransactionProtocol message)
        {
            var transact_format = JsonConvert.SerializeObject(message);
            var byte_array = UTF8Encoding.UTF8.GetBytes(transact_format);
            return byte_array;
        }

    }
}

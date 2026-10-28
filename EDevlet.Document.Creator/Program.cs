using EDevlet.Document.Common;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDevlet.Document.Creator
{
    internal class Program
    {
        private static readonly string createDocument = "create-document-queue";
        private static readonly string documentCreated = "document-created-queue";
        private static readonly string documentCreateExchane = "document-create-exchange";
        static IConnection connection;
        static IChannel channel;
        static async Task Main(string[] args)
        {
            connection = await getConnection();
            channel = await getChannel();
            await channel.ExchangeDeclareAsync(documentCreateExchane, ExchangeType.Direct);
            await channel.QueueDeclareAsync(createDocument,true, false, false);
            await channel.QueueBindAsync(createDocument, documentCreateExchane, createDocument);

            await channel.QueueDeclareAsync(documentCreated, true, false, false);
            await channel.QueueBindAsync(documentCreated, documentCreateExchane, documentCreated);

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (ch, ea) =>
            {
                var modelJson = Encoding.UTF8.GetString(ea.Body.ToArray());
                var model = JsonConvert.DeserializeObject<Common.CreateDocumentModel>(modelJson);
                Console.WriteLine($"Received message : {modelJson}");

                Task.Delay(3000).Wait();

                model.Url = " http://www.turkiye.gov.tr/docs/x.pdf";
                var modelJsonUpdated = JsonConvert.SerializeObject(model);

                writeToQueue(documentCreated, model);
            };
            await channel.BasicConsumeAsync(createDocument, true, consumer);

            Console.WriteLine($"{documentCreateExchane} listening");

            Console.ReadLine();
        }
        private static void writeToQueue(string QueueName, CreateDocumentModel model)
        {
            var messageArray = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(model));
            channel.BasicPublishAsync(documentCreateExchane, QueueName, false,new BasicProperties(), messageArray);
            Console.WriteLine("Message Published to Queue : " + QueueName);
        }
        private static async Task<IConnection> getConnection()
        {
            var connectionFactory = new ConnectionFactory()
            {
                Uri = new Uri("amqp://guest:guest@localhost:5672")
            };
            return await connectionFactory.CreateConnectionAsync();
        }
        private static async Task<IChannel> getChannel()
        {
            if(channel == null)
            {
                channel = await connection.CreateChannelAsync();
            }
            return channel;
        }
    }
}

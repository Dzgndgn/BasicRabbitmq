using EDevlet.Document.Common;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EDevlet.Document.Request
{
    public partial class Form1 : Form
    {
        IConnection connect;
        private readonly string createDocumentQueueName = "create-document-queue";
        private readonly string documentCreatedQueueName = "document-created-queue";
        private readonly string documentCreateExchaneName = "document-create-exchange";
        IChannel _channel;

        private async Task<IChannel> GetChannelAsync()
        {
            if (_channel == null)
            {
                _channel = await connect.CreateChannelAsync();
            }

            return _channel;
        }

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private async void btnCreateDocument_Click(object sender, EventArgs e)
        {
            var model = new Common.CreateDocumentModel()
            {
               UserId = 1,
               documentType = Common.DocumentType.Pdf
            };
            AddLog("Belge oluşturma butonuna basıldı");
            await writeToQueue(createDocumentQueueName, model);
            SplashScreen splashScreen = new SplashScreen();
            splashScreen.Show();
            await Task.Delay(5000);

            var consumerEvent = new AsyncEventingBasicConsumer(_channel);
            consumerEvent.ReceivedAsync += async (ch, ea) =>
            {

                var modelReceived = JsonConvert.DeserializeObject<CreateDocumentModel>(Encoding.UTF8.GetString(ea.Body.ToArray()));
                AddLog($"Received Data Url: {modelReceived.Url}");

                CloseSplashScreen(splashScreen);
              
            };
            await _channel.BasicConsumeAsync(documentCreatedQueueName, true, consumerEvent);

        }

        private void CloseSplashScreen(SplashScreen splashScreen)
        {
            if(splashScreen.InvokeRequired)
            {
                splashScreen.Invoke(new Action(() => CloseSplashScreen(splashScreen)));
                return;
            }
            splashScreen.Close();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private async void btnConnect_Click(object sender, EventArgs e)
        {
            if (connect == null || !connect.IsOpen)
            {
                connect = await getConnection();
            }
            _channel = await GetChannelAsync();
           
            btnCreateDocument.Enabled = true;
            await _channel.ExchangeDeclareAsync(documentCreateExchaneName, ExchangeType.Direct);
            
            await _channel.QueueDeclareAsync(createDocumentQueueName, true, false, false);
            await _channel.QueueBindAsync(createDocumentQueueName, documentCreateExchaneName, createDocumentQueueName);

            await _channel.QueueDeclareAsync(documentCreatedQueueName, true, false, false);
            await _channel.QueueBindAsync(documentCreatedQueueName, documentCreateExchaneName, documentCreatedQueueName);


            AddLog("Bağlantı butonuna basıldı");
        }
        private void AddLog(string logString)
        {
            if (txtLog.InvokeRequired)
            {
                txtLog.Invoke(new Action(() => AddLog(logString)));
                return;
            }
            logString = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {logString}";
            txtLog.AppendText(logString + Environment.NewLine);
            txtLog.SelectionStart = txtLog.Text.Length;
            txtLog.ScrollToCaret();
        }
        private async Task<IConnection>getConnection()
        {
            var factory = new ConnectionFactory()
            {
                Uri = new Uri(txtConnectionString.Text)
            };
            return await factory.CreateConnectionAsync();

        }
        private async Task writeToQueue(string QueueName,CreateDocumentModel model)
        {
            var messageArray = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(model));
            await _channel.BasicPublishAsync(documentCreateExchaneName, QueueName, mandatory: false,
            basicProperties: new BasicProperties(), messageArray);
            AddLog("Mesaj kuyruğa yazıldı");

        }
    }
}

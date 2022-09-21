using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TClient
{
    public partial class Form1 : Form
    {
        string login = "";
        string password = "";

        public class Query
        {
            [DataMember]
            public string Flag { get; set; }
            [DataMember]
            public string Text { get; set; }
            public Query(string flag, string text)
            {
                Flag = flag;
                Text = text;
            }
        }

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Form2 newForm = new Form2(login, password);
            newForm.Show();
        }

        private string Send_Request(string message)
        {
            const string ip = "127.0.0.1";
            const int port = 8080;

            var tcpEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);

            var tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            var data = Encoding.UTF8.GetBytes(message);

            tcpSocket.Connect(tcpEndPoint);
            tcpSocket.Send(data);

            var buffer = new byte[256];
            var size = 0;
            var answer = new StringBuilder();

            do
            {
                size = tcpSocket.Receive(buffer);
                answer.Append(Encoding.UTF8.GetString(buffer, 0, size));
            }
            while (tcpSocket.Available > 0);

            message = answer.ToString();

            tcpSocket.Shutdown(SocketShutdown.Both);
            tcpSocket.Close();
            return message;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Form3 newForm = new Form3();
            newForm.Show();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            List<Query>  query = new List<Query>() { new Query("select_string", "SELECT COUNT(*) FROM person " +
                "WHERE login = '" + textBox1.Text + "' AND password = '" + textBox2.Text + "'") };
            var Json = JsonConvert.SerializeObject(query);
            string disabled = Send_Request(Json);
            if (disabled == "0")
            {
                login = "";
                password = "";
                MessageBox.Show("Неверно введен логин или пароль, или вы не зарегистрированы");
            }
            else
            {
                login = textBox1.Text;
                password = textBox2.Text;
                MessageBox.Show("Авторизация прошла успешно");
            }
        }
    }
}

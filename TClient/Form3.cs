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
    public partial class Form3 : Form
    {
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
        public Form3()
        {
            InitializeComponent();

            List<Query> query = new List<Query>() { new Query("select", "SELECT name FROM types_animal") };
            var Json = JsonConvert.SerializeObject(query);
            string str = Send_Request(Json);
            List<string> userList = JsonConvert.DeserializeObject<List<string>>(str);
            comboBox1.Items.AddRange(userList.ToArray());

            query = new List<Query>() { new Query("select", "SELECT name FROM breeds") };
            Json = JsonConvert.SerializeObject(query);
            str = Send_Request(Json);
            userList = JsonConvert.DeserializeObject<List<string>>(str);
            comboBox2.Items.AddRange(userList.ToArray());
        }

        private void button1_Click(object sender, EventArgs e)
        {
            List<Query> query = new List<Query>() { new Query("insert", "INSERT INTO person (first_name, last_name, patronymic, birthdate, male, start_date, login, password) " +
                "VALUES ('" + textBox1.Text + "', '" + textBox2.Text + "', '" + textBox3.Text + "', '" + textBox4.Text + "', '" + textBox5.Text + "', '" + DateTime.Today.ToShortDateString() + "', '" + textBox10.Text + "', '" + textBox11.Text + "')") };
            var Json = JsonConvert.SerializeObject(query);
            Send_Request(Json);

            query = new List<Query>() { new Query("insert", "INSERT INTO animal " +
                "VALUES ('" + textBox6.Text + "', (SELECT id FROM types_animal WHERE types_animal.name = '" + comboBox1.Text + "'), (SELECT id FROM breeds WHERE breeds.name = '" + comboBox2.Text + "'), '" + textBox7.Text + "', '" + textBox8.Text + "', '" + textBox9.Text + "')") };
            Json = JsonConvert.SerializeObject(query);
            Send_Request(Json);

            query = new List<Query>() { new Query("insert", "INSERT INTO person_animal_map " + 
                "VALUES ((SELECT id FROM person WHERE login = '" + textBox10.Text + "'), " +
                "(SELECT MAX(id) FROM animal))") };
            Json = JsonConvert.SerializeObject(query);
            Send_Request(Json);
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
    }
}

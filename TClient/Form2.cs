using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace TClient
{
    public partial class Form2 : Form
    {
        string Login;
        string time_start;
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

        public Form2(string login, string password)
        {
            InitializeComponent();

            if (login == "")
                label6.Text = "Для выбора нужна авторизация";

            Login = login;

            comboBox1.SelectedIndexChanged += comboBox1_SelectedIndexChanged;

            List<Query> query = new List<Query>()  { new Query("select", "SELECT name FROM specialities") };
            var Json = JsonConvert.SerializeObject(query);
            string str = Send_Request(Json);
            List<string> userList = JsonConvert.DeserializeObject<List<string>>(str);
            comboBox1.Items.AddRange(userList.ToArray());

            query = new List<Query>() { new Query("select", "SELECT animal.name " +
                "FROM animal JOIN person_animal_map ON animal.id = animal_id " +
                "JOIN person ON person_id = person.id " +
                "WHERE login = '" + login + "'") };
            Json = JsonConvert.SerializeObject(query);
            str = Send_Request(Json);
            userList = JsonConvert.DeserializeObject<List<string>>(str);
            comboBox4.Items.AddRange(userList.ToArray());
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

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedState = comboBox1.SelectedItem.ToString();
            comboBox2.Text = "";
            comboBox3.Text = "";

            List<Query> query = new List<Query>() { new Query("select", "SELECT services_doctor.name " +
                "FROM services_doctor JOIN specialities_services_map ON services_doctor_id = services_doctor.id " +
                "JOIN specialities ON specialities_id = specialities.id " +
                "WHERE specialities.name = '" + selectedState + "'") };
            var Json = JsonConvert.SerializeObject(query);
            string str = Send_Request(Json);
            List<string> userList = JsonConvert.DeserializeObject<List<string>>(str);
            comboBox2.Items.Clear();
            comboBox2.Items.AddRange(userList.ToArray());

            query = new List<Query>() { new Query("select", "SELECT doctor.last_name + doctor.first_name + doctor.patronymic " +
                "FROM doctor JOIN specialities ON speciality_id = specialities.id " +
                "WHERE specialities.name = '" + selectedState + "'") };
            Json = JsonConvert.SerializeObject(query);
            str = Send_Request(Json);
            userList = JsonConvert.DeserializeObject<List<string>>(str);
            comboBox3.Items.Clear();
            comboBox3.Items.AddRange(userList.ToArray());
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            ToMake();
        }

        void MakeOrder(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            string[] subs = comboBox3.Text.Split();
            List<Query> query = new List<Query>() { new Query("insert", "INSERT INTO get_service " +
                "VALUES ((SELECT id FROM person WHERE person.login = '" + Login + "'), (SELECT id FROM animal WHERE animal.name = '" + comboBox4.Text + "'), " +
                "(SELECT id FROM doctor WHERE first_name = '" + subs[1] + "' AND last_name = '" + subs[0] + "' AND patronymic = '" + subs[2] + "'), " +
                "(SELECT id FROM services_doctor WHERE services_doctor.name = '" + comboBox2.Text + "'), " +
                "'" + dateTimePicker1.Value.ToString("dd.MM.yyyy") + " " + DateTime.Parse(btn.Text).ToString("HH:mm") + "', DEFAULT, DEFAULT)") };
            var Json = JsonConvert.SerializeObject(query);
            string check = Send_Request(Json);
            if (check == "0")
                MessageBox.Show("Не все поля заполнены");
            else
            {
                btn.Enabled = false;
                MessageBox.Show("Запись прошла успешно");
            }
        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            if (comboBox3.Text == "")
            {
                MessageBox.Show("Выберите доктора");
                return;
            }

            ToMake();
        }

        void ToMake ()
        {
            string[] subs = comboBox3.Text.Split();
            int x = 50;
            int y = 120;

            List<Query> query = new List<Query>() { new Query("select_string", "SELECT DATEDIFF (MINUTE, start_time, end_time) " +
                "FROM timetable JOIN doctor ON doctor.id =  doctor_id " +
                "WHERE first_name = '" + subs[1] + "' AND last_name = '" + subs[0] + "' AND patronymic = '" + subs[2] + "' AND dt = '" + dateTimePicker1.Value.ToString("yyyy-MM-dd") + "'") };
            var Json = JsonConvert.SerializeObject(query);
            int count = Convert.ToInt32(Send_Request(Json));

            query = new List<Query>() { new Query("select_string", "SELECT start_time " +
                "FROM timetable JOIN doctor ON doctor.id =  doctor_id " +
                "WHERE first_name = '" + subs[1] + "' AND last_name = '" + subs[0] + "' AND patronymic = '" + subs[2] + "' AND dt = '" + dateTimePicker1.Value.ToString("yyyy-MM-dd") + "'") };
            Json = JsonConvert.SerializeObject(query);
            time_start = Send_Request(Json);

            panel2.Controls.Clear();

            for (int i = 1; i <= count / 30; i++)
            {
                Button btn = new Button();
                btn.Location = new Point(x, y);
                btn.Size = new Size(50, 30);

                btn.Text = DateTime.Parse(time_start).ToString("HH:mm");

                query = new List<Query>() { new Query("select_string", "SELECT COUNT(*) FROM get_service JOIN doctor ON doctor.id =  doctor_id " +
                    "WHERE date_appointment = '" + dateTimePicker1.Value.ToString("dd.MM.yyyy") + " " + DateTime.Parse(time_start).ToString("HH:mm") + "' " +
                    "AND first_name = '" + subs[1] + "' AND last_name = '" + subs[0] + "' AND patronymic = '" + subs[2] + "' ") };
                Json = JsonConvert.SerializeObject(query);
                string disabled = Send_Request(Json);
                btn.Enabled = (disabled == "0");
                btn.Click += new EventHandler(MakeOrder);

                panel2.Controls.Add(btn);

                x += 100;
                if (x + 100 >= Width)
                {
                    x = 50;
                    y += 50;
                }

                DateTime time = DateTime.Parse(time_start).AddMinutes(30);
                time_start = time.ToString("HH:mm");
            }
        }
    }
}

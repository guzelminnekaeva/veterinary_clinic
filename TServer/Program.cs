using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace TServer
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

    class Program
    {
        public static string connectString = @"Data Source=DESKTOP-R0R5NG4\SQLEXPRESS;Initial Catalog=veterinary_clinic_bd;Integrated Security=True";
        public static SqlConnection myConnection;
        public DataSet dset;
        public SqlDataAdapter dbAdpt1;
        public DataView dv;

        static void Main(string[] args)
        {
            myConnection = new SqlConnection(connectString);
            myConnection.Open();
            SqlCommand command;
            SqlDataReader reader;

            const string ip = "127.0.0.1";
            const int port = 8080;

            var tcpEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);

            var tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            tcpSocket.Bind(tcpEndPoint);
            tcpSocket.Listen(5);      

            while (true)
            {
                var listener = tcpSocket.Accept();
                var buffer = new byte[256];
                var size = 0;
                var data = new StringBuilder();
                string Json = "";

                do
                {
                    size = listener.Receive(buffer);
                    data.Append(Encoding.UTF8.GetString(buffer, 0, size));
                }
                while (listener.Available > 0);

                List<Query> userList = JsonConvert.DeserializeObject<List<Query>>(data.ToString());

                command = new SqlCommand(userList[0].Text.ToString(), myConnection);

                try
                {
                    switch (userList[0].Flag)
                    {
                        case "select":
                            List<string> specialities = new List<string>();
                            using (reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    specialities.Add(Regex.Replace(reader.GetString(0), " {2,}", " "));
                                }
                            }
                            reader.Close();
                            Json = JsonConvert.SerializeObject(specialities);
                            break;
                        case "select_string":
                            using (reader = command.ExecuteReader())
                            {
                                reader.Read();
                                Json = String.Format("{0}", reader[0]);
                            }
                            reader.Close();
                            break;
                        case "insert":
                            command.ExecuteNonQuery();
                            Json = "true";
                            break;
                    }
                }
                catch
                {
                    Json = "0";
                }

                //command = new SqlCommand("DELETE T FROM ( " +
                //        "SELECT * , DupRank = ROW_NUMBER() OVER( " +
                //            "PARTITION BY first_name, last_name, patronymic, birthdate  ORDER BY(SELECT NULL)) " +
                //        "FROM person) AS T " +
                //    "WHERE DupRank > 1 ", myConnection);
                //command.ExecuteNonQuery();

                Console.WriteLine(data);

                listener.Send(Encoding.UTF8.GetBytes(Json));

                listener.Shutdown(SocketShutdown.Both);
                listener.Close();
            }
        }
    }
}

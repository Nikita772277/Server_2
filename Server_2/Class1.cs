using System.Net;
using System.Net.Sockets;
using System;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using System.Xml.Linq;


namespace server
{

    public class Client
    {
        public int num;
        public TcpClient tcpClient;
        public NetworkStream stream;
        public Server serv;
        Thread t;
        public Client(TcpClient cl, Server s, int i)
        {
            tcpClient = cl;
            stream = tcpClient.GetStream();
            serv = s;
            num = i;
            t = new Thread(ListenMessage);
            t.Start();

        }
        public async void ListenMessage()
        {
            Console.WriteLine($"Прослушивание пользователя № {num}");
            try
            {
                byte[] message = new byte[100];

                while (true)
                {
                    int Byte = stream.Read(message);
                    if (Byte > 0)
                    {
                        Console.WriteLine(Encoding.UTF8.GetString(message));
                        serv.SendMessage(message);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                byte[] mes = Encoding.UTF8.GetBytes($"23");
                stream.WriteAsync(mes);
            }
            finally
            {
                stream.Close();
                tcpClient.Close();
                serv.Disconnect(num);
            }
        }
        public async void ReturningAMessage()
        {
            
        }
    }
    public class Server
    {
        int user = 0;
        static TcpListener listener;
        public static List<Client> clients = new List<Client>();
        Thread ListenConnect;
        public Server(string ip, int port)
        {
            IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            listener = new TcpListener(ipPoint);
            listener.Start();
            ListenConnect = new Thread(Connect);
            ListenConnect.Start();
            Console.WriteLine($"Сервер запущен по адресу {ip} / {port}");
        }
        public void Connect()
        {
            while (true)
            {
                try
                {
                    Client client = new Client(listener.AcceptTcpClient(), this, user);
                    clients.Add(client);
                    Console.WriteLine(client.tcpClient.Client.RemoteEndPoint);
                    user = clients.Count; ;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

            }

        }
        public void SendMessage(byte[] message)
        {
            for (int j = 0; j < clients.Count; j++)
            {
                clients[j].stream.WriteAsync(message);
            }
        }
        public void Disconnect(int i)
        {
            clients.RemoveAt(i);
        }
        public static void Main(string[] args)
        {
            //Console.WriteLine("Введите ip ");
            //string ip = Console.ReadLine();
            string ip = "192.168.0.130";
            //Console.WriteLine("Введите порт ");
            //int port = Convert.ToInt32(Console.ReadLine());
            int port = 88;
            Server s = new Server(ip, port);
            Console.ReadLine();
        }
    }

}


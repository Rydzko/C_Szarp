using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.Windows.Forms;
using System.Security.Cryptography;

namespace Client
{
    public partial class Form1 : Form
    {
        Socket localClientSocket;
        Thread myth;


        string reg_or_log; //pomoże nam zlokalizować czy chcemy się zarejestrować czy zalogować !!!
        public Form1()
        {
            InitializeComponent();
            IPHostEntry host = Dns.GetHostEntry("localhost");
            IPAddress ipAddress = host.AddressList[0]; // localhost
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, 15000);

            // Create a TCP/IP  socket.    
            localClientSocket = new Socket(ipAddress.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);
            localClientSocket.Connect(remoteEP);

            richTextBox1.Text = "Socket connected to {0}" +
                localClientSocket.RemoteEndPoint.ToString() + Environment.NewLine;

            myth = new Thread(listen);
            myth.Start(this);

        }

        bool check = false; //zmienna, która pozwoli wysłać wiadomość jeśli będziemy ZALOGOWANI !!!

        private void button1_Click(object sender, EventArgs e)
        {
            if (check)
            {
                try
                {
                    byte[] bytes = new byte[1024];
                    // Encode the data string into a byte array.    
                    byte[] msg = Encoding.ASCII.GetBytes(textBox1.Text);
                    int bytesSent = localClientSocket.Send(msg);

                    textBox1.Text = " ";
                }
                catch 
                {
                    textBox1.Text = " ";
                    MessageBox.Show("Something went wrong.");
                }
            }
            else
            {
                textBox1.Text = "";
                MessageBox.Show("Zaloguj się aby wysłać wiadomość.");
            }
        }


        private void button2_Click(object sender, EventArgs e)
        {
            if(textBox2.Text != "" || textBox3.Text != "") //ochrona przed nie wpisaniem loginu lub hasła
            {
                byte[] bytes = new byte[1024];
                // Encode the data string into a byte array.
                string password = textBox3.Text;
                password = haszowanie(password);
                string info = reg_or_log + " " + textBox2.Text + " " + password;
                byte[] msg = Encoding.ASCII.GetBytes(info);
                int bytesSent = localClientSocket.Send(msg);

                textBox1.Text = " ";
            }
            else
            {
                textBox2.Text = "";
                textBox3.Text = "";
                MessageBox.Show("Nie podano loginu lub hasła. Spróbuj ponownie.");
            }
        }

        //będziemy odnosić się do zapisanych radioButtonów !!!
        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            button2.Text = "REGISTER"; //wpisujemy w button2 REGISTER 
            reg_or_log = "/register"; //dzięki niemu będziemy wiedzieli, że chcemy się zarejestować
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            button2.Text = "LOG IN"; //wpisujemy w button 2 LOG IN
            reg_or_log = "/login"; //dzięki niemu będziemy wiedzieli, że chcemy się zalogować
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (textBox2.Text != "" || textBox3.Text != "")
            {
                byte[] msg = Encoding.ASCII.GetBytes("<EOF>");
                int send = localClientSocket.Send(msg);
            }
            else
            {
                MessageBox.Show("Nie możesz wylogować się z nie istniejącego lub nie zalogowanego konta !!!");
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            byte[] msg = Encoding.ASCII.GetBytes("<WARNING>"); //wyświetlamy komunikat jeśli mamy wiadomość na skrzynce
            int send = localClientSocket.Send(msg);

            button4.Enabled = false;
        }

        public void listen(Object form)
        {
            RichTextBox brd = richTextBox1;

            while (true)
            {
                string data = null;
                byte[] bytes = null;

                bytes = new byte[1024];
                int bytesRec = ((Form1)form).localClientSocket.Receive(bytes);
                data += Encoding.ASCII.GetString(bytes, 0, bytesRec);

                if (data.IndexOf("<EOF>") > -1) //zamykamy nim naszego KLIENTA !!!
                {
                    brd.Text = "Close. See you later !!!" + Environment.NewLine;
                    MessageBox.Show("Klient zostanie rozłączony !!!");
                    Thread.Sleep(2000); //uśpienie threada
                    break;
                }

                if (data.IndexOf("<LOGGED_TRUE>") > -1) //jeśli logowanie przebiegło pomyślnie to możemy wysłać wiadomość
                {
                    MessageBox.Show("Logowanie przebiegło pomyślnie. Możesz kontynuować.");
                    data = "You have connected to the serwer.";

                    button2.Enabled = false;

                    check = true; //jeśli się zalogowaliśmy to możemy teraz wysłać wiadomość
                }

                if (data.IndexOf("<WARNING>") > -1) //wiadomość dla klienta o wiadomości na skrzynce !!!
                {
                    data = "Sprawdź swoją skrzynke pocztową !!!" + Environment.NewLine;
                    button4.Enabled = true;
                }

                if (data.IndexOf("<BAD_WORD>") > -1) //jeśli podaliśmy "obraźliwy" login
                {
                    textBox2.Text = "";
                    textBox3.Text = "";
                    MessageBox.Show("Twoja nazwa użytkownika zawiera przekleństwa. Spróbuj ponownie.");
                    data = "";
                }

                if (data.IndexOf("<REGISTER_SAVED>") > -1) //jeśli się zarejestrowaliśmy to możemy się ZALOGOWAĆ
                {
                    MessageBox.Show("Rejestracja przebiegła pomyślnie. Możesz przystąpić do logowania.");
                    data = "";
                }

                if (data.IndexOf("<AGAIN_LOGGED>") > -1) //sprawdzamy czy nie zalogowaliśmy się drugi raz na tego samego klienta
                {
                    textBox2.Text = "";
                    textBox3.Text = "";
                    MessageBox.Show("To konto jest już zalogowane.");
                    data = "";
                }

                if (data.IndexOf("<REGISTER_BUSY>") > -1) //ten login jest już na liście, więc musimy zarejestrować się pod inną nazwą !!!
                {
                    textBox2.Text = "";
                    textBox3.Text = "";
                    MessageBox.Show("Nazwa użytkownika zajęta. Spróbuj ponownie.");
                    data = "";
                }

                if (data.IndexOf("<NOT_BAD_LOGGED>") > -1)
                {
                    textBox2.Text = "";
                    textBox3.Text = "";
                    MessageBox.Show("Konto nie istnieje. Zarejestruj się i spróbuj ponownie.");
                    data = "";
                }

                if (data.IndexOf("<NOT_SEND_MSG>") > -1)
                {
                    MessageBox.Show("Nie można wysłać wiadomości, ponieważ użytkownik nie jest zalogowany.");
                    data = "";
                }

                if (data.IndexOf("<SERVER_COUT>") > -1)
                {
                    MessageBox.Show("Serwer zakończy swoją pracę. Do zobaczenia !!!");
                    Thread.Sleep(2000);
                    break;
                }
                brd.Text += data;
            }

            Close(); //zamykamy threada
        }

        //haszowanie naszego hasła !!!
        public string haszowanie(string hasz)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] data = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(hasz));

                var result = new StringBuilder();

                for (int i = 0; i < data.Length; i++)
                {
                    result.Append(data[i].ToString("x2"));
                }

                return result.ToString();
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {

        }
    }
}
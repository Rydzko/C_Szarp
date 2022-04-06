using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;
using System.Net;
using System.IO;
using System.Globalization;

namespace zad_dom_3
{
    //Form1 = this;
    //Form2 = new Form2;
    public partial class Form1 : Form
    {
        internal static Form2 form2;
        internal static Form1 form1;
        public static List<string> abc = new List<string>();
        public Form1()
        {
            //new JObject();
            InitializeComponent();
            form1 = this;
            timer1.Interval = 30000; //Pobiera lub ustawia interwał wyrażony w milisekundach, w którym ma zostać zgłoszone zdarzenie 
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            form2 = new Form2(this); //tworzymy Form2 i ładujemy go w Form1
            form2.Show();
        }

        private void AVERAGE_Click(object sender, EventArgs e)
        {
            string path = textBox2.Text;
            path = path.ToLower();
            check_valid(path); //
            textBox2.Text = "";
        }

        public bool add_waluta(string waluta) //funkcja sprawdzająca czy wpisaliśmy poprawnie walute i dodająca ją na liste
        {
            waluta = waluta.ToLower(); //bez znaczenia czy waluta jest wpisana z małej czy wielkiej litery, zostaje dodana na liste tylko raz(To.Lower() zmienia wielkie na małe litery)!!!
            //HttpWebResponse response;
            HttpStatusCode StatusNumber;
            try
            {
                string url = "http://api.nbp.pl/api/exchangerates/rates/a/" + waluta + "/?format=json";
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                var d = response.StatusCode;
                string content = new StreamReader(response.GetResponseStream()).ReadToEnd();
                dynamic data = JObject.Parse(content);//var - kompilacja dynamic - runtime
                var g = data.rates[0].effectiveDate;

                //walidacja waluty
                bool czy_waluta_jest_na_liscie = false;

                foreach (string element in abc)
                {
                    if(element == waluta)
                    {
                        czy_waluta_jest_na_liscie = true;
                        MessageBox.Show("Waluta znajduje się na liście.");
                    }
                }
                if(czy_waluta_jest_na_liscie == false)
                {
                    add_dane(waluta, (string)data.rates[0].mid, (string)data.rates[0].effectiveDate);
                    abc.Add(waluta);
                }
                return true;
            }
            catch (WebException ex) //catch wysyłający informacje do nbp o błędzie
            {
                HttpWebResponse res = (HttpWebResponse)ex.Response;
                int status = (int)res.StatusCode;
                StatusNumber = res.StatusCode;
                MessageBox.Show("Http status code for " + waluta + " : " + Convert.ToString(status) + "\n" + StatusNumber.ToString());
                return false;
            }
        }

        private void add_dane(string money, string average, string date) //funkcja zapisująca walute do pliku !!!
        {
            //abc.Add(money);
            string name = "waluty/" + money + ".txt";
            //DateTime sDate = DateTime.Now;
            using (StreamWriter saveScore = new StreamWriter(name, true)) //zapisywanie do pliku txt
            {
                String sDate = DateTime.Now.ToString();
                saveScore.WriteLine(money + " | " + date + " | " + average + " | " + sDate);
            }
        }

        public void show_abc() //zapisywanie różnych walut obok siebie w textBoxie1
        {
            textBox3.Text = "";
            foreach (string element in abc)
            {
                textBox3.Text += element + ", ";
            }
        }

        private void timer1_Tick(object sender, EventArgs e) //timer zapisujący co 30sek odczyt danej waluty i jej kursu ze strony api.nbp.pl
        {
            for(int i = 0; i < abc.Count(); i++)
            {
                string url = "http://api.nbp.pl/api/exchangerates/rates/a/" + abc[i] + "/?format=json";
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                var d = response.StatusCode;
                string content = new StreamReader(response.GetResponseStream()).ReadToEnd();
                dynamic data = JObject.Parse(content);//var - kompilacja dynamic - runtime
                //var g = data.rates[0].effectiveDate;
                string date = (string)data.rates[0].effectiveDate;
                string average = (string)data.rates[0].mid;
                add_dane(abc[i], average, date); //wywołanie funkcji dodającej pobrane dane do pliku txt
            }
        }

        private double read(string path) //funkcja odczytująca dany rodzaj waluty i licząca średnią z danego pliku txt(konkretnej waluty, przez nas podanej)
        {
            double average = 0;
            int amount = 0;
            //try
            //{
                using (StreamReader sr = new StreamReader("waluty/" + path + ".txt"))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        if (!(line == "")) //zabezpieczenie przed odzieleniem ENTEREM danego kursu waluty w pliku txt
                        {
                            string[] red = line.Split('|');
                            double result = double.Parse(red[2], CultureInfo.InvariantCulture); //CultureInfo.InvariantCulture zamienia kropki na przecinki przy kursie danej waluty
                            average += result;
                            amount++;
                        }
                    }
                }
                return (average / amount);
            //}
            //catch
            //{
                //MessageBox.Show("Error. Podanej waluty nie ma na liście.");
                //return -1;
            //}
        }

        private bool check_valid(string path) //funkcja 
        {
            HttpWebResponse response;
            HttpStatusCode StatusNumber;
            try //try sprawdzający na stronie nbp czy wpisaliśmy walute do textBoxa czy loaowy ciąg znaków
            {
                string url = "http://api.nbp.pl/api/exchangerates/rates/a/" + path + "/?format=json";
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                response = (HttpWebResponse)request.GetResponse();
                string content = new StreamReader(response.GetResponseStream()).ReadToEnd();
                dynamic data = JObject.Parse(content); //var - kompilacja dynamic - runtime

                bool flag = false; //zmienna bool, która dopilnuje aby średnia liczona była tylko z walut, które dodaliśmy w danym momencie, a nie ogólnie w pliku txt

                foreach (string element in abc) //sprawdzamy czy waluta jest już na liście
                {
                    if (element == path) //jeśli wpisana przez nas waluta znajduje się na utworzonej wcześniej liście
                    {
                        flag = true;
                        textBox1.Text = read(path).ToString(); //przekaz walute, oblicz średnią i wyświetl
                        break;
                    }
                }
                if (flag == false) //jeśli wpisana przez nas waluta nie znajduje się na utworzonej wcześniej liście
                {
                    MessageBox.Show("Error. Podanej waluty nie ma na utworzonej liście.");
                    textBox1.Text = "";
                }
                return true;
            }
            catch (WebException ex)
            {
                HttpWebResponse res = (HttpWebResponse)ex.Response;
                int status = (int)res.StatusCode;
                StatusNumber = res.StatusCode;
                MessageBox.Show("Http status code for " + path + " : " + Convert.ToString(status) + "\n" + StatusNumber.ToString());
                textBox1.Text = "";
                return false;
            }
        }
    }
}

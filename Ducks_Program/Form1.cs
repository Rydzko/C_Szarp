using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using WMPLib;

namespace zad_dom_2
{
    public partial class Form1 : Form
    {
        WindowsMediaPlayer player = new WindowsMediaPlayer();
        public Form1()
        {
            InitializeComponent();
            kaczka.Click += new System.EventHandler(this.duck); //w tym momencie stara ilość kaczek nie zostanie dodana do nowej ilości kaczek i po złapaniu kaczki zostanie ona dodana raz, a nie kilka dwa, trzy itd
            //(działo się tak, gdy EventHandler został wstawiony do START_Click)
            //player.SoundLocation = "kaczuchy.mp3";
            player.URL = "kaczuchy.mp3"; //dodanie muzyczki
        }

        Button kaczka = new Button(); //dynamiczne stworzenie kaczki
        Random rand = new Random(); //będzie liczyć randomowe położenia kaczki

        int licznik = 0; //bedzie dodawany jeśli trafie w kaczke
        int wynik = 0; //będzie dodawany po każdym pojawieniu się kaczki

        private void START_Click(object sender, EventArgs e) //operacje dla kaczki po kliknięciu buttona START
        {
            player.controls.play(); //play do muzyczki
            MessageBox.Show("Ustrzel kaczuche i zdobądź jak największy wynik. Powodzenia !!!");
            if (textBox1.Text != "")
            {
                label1.Visible = false; //Visible usówa obiekty, przyciski itd...
                textBox1.Visible = false;
                button1.Visible = false;

                trackBar1.Visible = true;
                button2.Visible = true;
                textBox2.Visible = true;
                label2.Visible = true;
                label3.Visible = true;
                label4.Visible = true;
                label5.Visible = true;
                kaczka.Visible = true; //SPRAWDZIĆ !!!

                //kaczka.Click += new System.EventHandler(this.duck);
                //kaczka.Text = "kaczucha";
                Image kaczucha = Image.FromFile("kaczka.png"); //dodanie nowego tła gdy będziemy łapać kaczuchy(domyślne tło jest już ustawione na początku)
                this.BackgroundImage = kaczucha;
                ImageLayout ducker = ImageLayout.Stretch;
                this.BackgroundImageLayout = ducker;
                Image duck = Image.FromFile("duck.png"); //dodanie obrazu na kaczki
                kaczka.BackgroundImage = duck;
                ImageLayout kaczunia = ImageLayout.Stretch;
                kaczka.BackgroundImageLayout = kaczunia;
                Size size = new Size(50, 50);
                kaczka.Size = size;

                //Point location = kaczka.Location;
                //location.X = rand.Next(500);
                //location.Y = rand.Next(100);
                //kaczka.Location = location;
                textBox2.Text = wynik.ToString() + "/" + licznik.ToString(); //zapisanie wyniku w Scorze
                kaczka.Location = new Point(rand.Next(0,500), rand.Next(0,100)); //nowa lokacja kaczki
                Controls.Add(kaczka); //musimy dodać kaczke gdy się przemieści w inne miejsce
                timer1.Enabled = true;
                timer1.Interval = 2000;
                //textBox2.Text = wynik.ToString() + "/" + "1";
                //kaczka.Click += new System.EventHandler(this.timer1_Tick);
            }
            else
            {
                MessageBox.Show("Nie podałeś username. Uzupełnij TextBox");
            }
        }

        private void duck(object sender, EventArgs e)
        {
            licznik++; //licznik liczący trafienia w kaczke
            wynik++; //wynik, który będzie dodawał się za każdym razem, gdy kaczucha się przemieści
            textBox2.Text = wynik.ToString() + "/" + licznik.ToString(); //zapisanie wyniku
            //Point location = kaczka.Location;
            //location.X = rand.Next(500);
            //location.Y = rand.Next(100);
            //kaczka.Location = location;
            kaczka.Location = new Point(rand.Next(0, 500), rand.Next(0, 100));
            Controls.Add(kaczka);
        }

        private void przemieszczenie()
        {
            //Point location = kaczka.Location;
            //location.X = rand.Next(500);
            //location.Y = rand.Next(100);
            //kaczka.Location = location;
            kaczka.Location = new Point(rand.Next(0, 500), rand.Next(0, 100));
            textBox2.Text = wynik.ToString() + "/" + licznik.ToString();
            Controls.Add(kaczka);
        }

        private void STOP_Click(object sender, EventArgs e)
        {
            using (StreamWriter saveScore = new StreamWriter("Wyniki.txt", true)) //zapisywanie do pliku
            {
                String sDate = DateTime.Now.ToString();
                saveScore.WriteLine("USERNAME: " + textBox1.Text + "   " + "DATE: " + sDate + "   " + "SCORE: " + textBox2.Text); //zapisanie wyniku i bieżącej daty
            }
            label1.Visible = true; //Visible usówa obiekty, przyciski itd...
            textBox1.Visible = true;
            button1.Visible = true;
            textBox1.Text = ""; //po naciśnięciu STOP wyzerowuje mi wpisaną nazwe użytkownika

            trackBar1.Visible = false;
            button2.Visible = false;
            textBox2.Visible = false;
            label2.Visible = false;
            label3.Visible = false;
            label4.Visible = false;
            label5.Visible = false;
            kaczka.Visible = false;
            timer1.Enabled = false;
            licznik = 0; //SPRAWDZIĆ !!!
            wynik = 0; //musimy wyzerowac Score bo wyjściu z gry

            Image kaczucha = Image.FromFile("kaczka_background.png"); //po naciśnięciu STOP nasz background zmieni się na ten sam, który był na początku gry
            this.BackgroundImage = kaczucha;
            ImageLayout ducker = ImageLayout.Stretch;
            this.BackgroundImageLayout = ducker;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            int time = 2000 / trackBar1.Value; //uzależniamy poziom trudności gry trackBarem1
            double time_min = time * 0.8; //minimalny czas
            double time_max = time * 1.2; //maksymalny czas
            timer1.Interval = rand.Next((int)time_min, (int)time_max); //kiedy trafimy w kaczke, przemieści się ona w wyznaczonym przedziale czasu: (time_min, time_max)
            licznik++;
            przemieszczenie(); //wywołujemy samą funkcje przemieszczenie, ponieważ EventHandler dla kaczki wariował i przemieszczał sie w dziwny sposób
            //timer1.Interval += rand.Next((int)(-timer1.Interval * 0.2), (int)(timer1.Interval * 0.2));
            //timer1.Interval = 2000;
            //licznik++;
            //timer1.Tick += new System.EventHandler(this.przemieszczenie); //po uruchomieniu funkcji przemieszczenie kaczka wariowała
        } 
    }
}

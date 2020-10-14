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

namespace WindowsFormsApplication3
{
    public partial class Form1 : Form
    {
        public static bool step1 = false;
        public static bool step2 = false;
        public static bool step3 = false;
        public static bool step4 = false;
        public static string pathToFile;
        public static char separator;
        public static string metryka;
        public static List<double> atrybuty = new List<double>();
        public static int ileAtrybutów;

        public Form1()
        {
            InitializeComponent();
        }

        //main
        public void button5_Click(object sender, EventArgs e)
        {
            int ileAtrybutow = howManyAtribs(pathToFile, separator);
            List<Double> unikalne = uniqueClass(pathToFile, separator);

            List<double> wynik = txtToDoubleArray(pathToFile, separator);
            List<Probka> ListofProbes = DoubleArrayToObject(wynik, ileAtrybutow);

            ListofProbes = Normalizacja(ListofProbes, ileAtrybutow);

            Probka testowa = new Probka();
            testowa.atrybuty = atrybuty;

            int number;
            double p = 0;
            int k = 0;

            if (metryka == "Metryka Minkowskiego")
                if (textBox4.Text != "")
                    if (Int32.TryParse(textBox4.Text, out number))
                        p = Convert.ToDouble(textBox4.Text);
                    else
                        MessageBox.Show("wartość p powinna być typu double");
                else
                    MessageBox.Show("Proszę podać wartość p");

            if (textBox3.Text != "")
                if (Int32.TryParse(textBox3.Text, out number))
                    k = Convert.ToInt32(textBox3.Text);
                else
                    MessageBox.Show("wartość k powinna być liczbą całkowitą");
            else
                MessageBox.Show("Proszę podać wartość k");

            double decyzja = wykonajKNN(testowa, ListofProbes, unikalne, metryka, p, k);
            
            if (decyzja == -1)
            {
                label19.Text = "Nie można ustalić klasy próbki";
                return;
            }
            else
            {
                label19.Text = "Klasa: " + decyzja;
                testowa.klasa = decyzja;
            }

            double dokladnosc = SprawdzDokladnosc(ListofProbes, unikalne, metryka, p, k);

            label22.Text = "Dokładność: " + dokladnosc.ToString("#0.##%");
        }

        public double SprawdzDokladnosc(List<Probka> ListofProbes, List<Double> unikalne, string metryka, double p, int k)
        {
            double suma = 0;
            for (int i = 0; i < ListofProbes.Count; i++)
            {
                List<Probka> ProbesWithoutSelected = new List<Probka>(ListofProbes);
                ProbesWithoutSelected.RemoveAt(i);
                double decyzja = wykonajKNN(ListofProbes[i], ProbesWithoutSelected, unikalne, metryka, p, k);
                if (ListofProbes[i].klasa == decyzja && decyzja != -1)
                    suma++;
            }
            return suma / ListofProbes.Count;
        }

        public double wykonajKNN(Probka testowa, List<Probka> ListofProbes, List<Double> unikalne, string metryka, double p, int k)
        {
            Dictionary<Double, List<Double>> drogi = wypelnij(unikalne);

            oblicz(drogi, ListofProbes, testowa, metryka, p);

            sortowanie(drogi);

            int minSumaCount = minSuma(drogi);


            if (minSumaCount < k)
                throw new System.ArgumentException("k jest większe niż ilość elementów sumy");

            Double[] key = new Double[unikalne.Count];
            Double[] values = new Double[unikalne.Count];

            setValues(values, key, drogi, k);

            Double minimum = min(values);

            if (minimum == -1)
                return -1;

            return key[Convert.ToInt32(minimum)];
        }

        public static List<Probka> Normalizacja(List<Probka> ListofProbes, int atrybutyCount)
        {
            for (int j = 0; j < atrybutyCount; j++)
            {
                List<double> tmp = new List<double>();

                for (int i = 0; i < ListofProbes.Count; i++)
                    tmp.Add(ListofProbes[i].atrybuty[j]);

                double minimum = min(tmp);
                double maximum = max(tmp);

                for (int i = 0; i < ListofProbes.Count; i++)
                    ListofProbes[i].atrybuty[j] = (tmp[i] - minimum) / (maximum - minimum);
            }

            return ListofProbes;
        }

       // metoda sortująca

        public static void sortowanie(Dictionary<Double, List<Double>> drogi)
        {
            foreach (KeyValuePair<Double, List<Double>> kvp in drogi)
            {
                kvp.Value.Sort();
            }
        }

        // metody operujące na txt
        public static List<Double> txtToDoubleArray(string path, char separator)
        {
            List<string> lines = File.ReadAllLines(path).ToList();
            List<double> wynik = new List<double>();
            double test;
            // sprawdza czy na komputerze double posiada , czy .
            if (Double.TryParse("2,2", out test))
                test = 0;
            else
                test = 1;
            foreach (string line in lines)
            {
                string tmp = "";
                for (int i = 0; i < line.Length; i++)
                {
                    if (line[i] != separator)
                        if (line[i] == '.')
                            if (test == 0)
                                tmp += ',';
                            else
                                tmp += '.';
                        else if (line[i] == ',')
                            if (test == 0)
                                tmp += '.';
                            else
                                tmp += ',';
                        else
                            tmp += line[i];
                    else
                        if (tmp != "")
                        {
                            wynik.Add(Convert.ToDouble(tmp));
                            tmp = "";
                        }
                }
                wynik.Add(Convert.ToDouble(tmp));
                tmp = "";
            }
            return wynik;
        }

        public static List<Probka> DoubleArrayToObject(List<double> wynik, int ileAtrybutow)
        {
            List<Probka> ListofProbes = new List<Probka>();
            for (int i = 0; i < wynik.Count; i += ileAtrybutow + 1)
            {
                Probka nowa = new Probka();
                for (int j = 0; j < ileAtrybutow; j++)
                    nowa.atrybuty.Add(wynik[i + j]);
                nowa.klasa = wynik[i + ileAtrybutow];
                ListofProbes.Add(nowa);
            }
            return ListofProbes;
        }

        public static List<Double> uniqueClass(string path, char separator)
        {
            List<string> lines = File.ReadAllLines(path).ToList();
            List<Double> myList = new List<Double>();
            HashSet<Double> myHashSet = new HashSet<Double>();
            foreach (string line in lines)
            {
                string tmp = "";
                for (int i = 0; i < line.Length; i++)
                    if (line[i] != separator)
                        if (line[i] == '.')
                            tmp += ',';
                        else
                            tmp += line[i];
                    else
                        tmp = "";
                if (myHashSet.Add(Convert.ToDouble(tmp)))
                {
                    myList.Add(Convert.ToDouble(tmp));
                }
            }
            return myList;
        }

        public static int howManyAtribs(string path, char separator)
        {
            List<string> lines = File.ReadAllLines(path).ToList();
            int licznik = 0;
            for (int i = 0; i < lines[0].Length; i++)
                if (lines[0][i] == separator)
                    licznik++;
            return licznik;
        }

        //Metody obsługujące Windows Forms

        public void Form1_Load(object sender, EventArgs e)
        {
            button3.Enabled = false;
            textBox1.Enabled = false;
            textBox3.Enabled = false;
            button5.Enabled = false;
            textBox4.Enabled = false;
        }

        OpenFileDialog ofd = new OpenFileDialog();


        public void button1_Click(object sender, EventArgs e)
        {
            ofd.Filter = "TXT|*.txt";
            if (ofd.ShowDialog() == DialogResult.OK)
            {

                label8.Text = ofd.FileName;
                label7.Text = ofd.SafeFileName;
                pathToFile = ofd.FileName;
                button3.Enabled = true;
                step1 = true;

                atrybuty.Clear();
                step4 = false;
                label12.Text = "aktualne atrybuty: brak";
            }
        }

        public void checkedListBox1_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            metryka = (sender as CheckedListBox).SelectedItem.ToString();
            if (metryka == "Metryka Minkowskiego")
                textBox4.Enabled = true;
            else
                textBox4.Enabled = false;
            step3 = true;
            if (e.NewValue == CheckState.Checked && checkedListBox1.CheckedItems.Count > 0)
            {
                checkedListBox1.ItemCheck -= checkedListBox1_ItemCheck;
                checkedListBox1.SetItemChecked(checkedListBox1.CheckedIndices[0], false);
                checkedListBox1.ItemCheck += checkedListBox1_ItemCheck;
            }
        }

        public void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            step2 = true;
            separator = '\t';
            textBox2.Text = "";
            textBox1.Enabled = true;
        }

        public void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            step2 = true;
            separator = ' ';
            textBox2.Text = "";
            textBox1.Enabled = true;
        }

        public void radioButton3_Click(object sender, EventArgs e)
        {
            if (textBox2.Text == "")
            {
                radioButton3.Checked = false;
                MessageBox.Show("Podaj wartość");
                step2 = false;
            }
            else if (textBox2.Text.Length > 1)
            {
                radioButton3.Checked = false;
                MessageBox.Show("Separator to 1 znak");
                textBox2.Text = "";
                step2 = false;
            }
            else
            {
                separator = textBox2.Text[0];
                step2 = true;
                textBox1.Enabled = true;
            }
        }

        public void button3_Click(object sender, EventArgs e)
        {
            ileAtrybutów = howManyAtribs(pathToFile, separator);
            if (ileAtrybutów == 0)
            {
                MessageBox.Show("Podany separator nie pasuje do wzorca w pliku");
                return;
            }
            string x = textBox1.Text;
            double temp = 0;
            if (double.TryParse(x, out temp))
            {
                if (ileAtrybutów > atrybuty.Count)
                {
                    atrybuty.Add(Convert.ToDouble(textBox1.Text));
                    label12.Text = "aktualne atrybuty: ";
                    for (int i = 0; i < atrybuty.Count; i++)
                        label12.Text += atrybuty[i] + " ";
                    textBox1.Text = "";
                }
                if (ileAtrybutów == atrybuty.Count)
                    MessageBox.Show("Podałeś wymaganą liczbę atrybutów");
                    step4 = true;
            }
            else
            {
                MessageBox.Show("zła wartość");
                textBox1.Text = "";
            }
        }

        public void button4_Click(object sender, EventArgs e)
        {
            atrybuty.Clear();
            label12.Text = "aktualne atrybuty: brak";
            step4 = false;
        }

        public void button2_Click_1(object sender, EventArgs e)
        {
            Console.WriteLine(step1);
            if (step1 == false)
                pictureBox1.BackColor = Color.Red;
            else
                pictureBox1.BackColor = Color.Green;
            if (step2 == false)
                pictureBox2.BackColor = Color.Red;
            else
                pictureBox2.BackColor = Color.Green;
            if (step3 == false)
                pictureBox3.BackColor = Color.Red;
            else
                pictureBox3.BackColor = Color.Green;
            if (step4 == false)
                pictureBox4.BackColor = Color.Red;
            else
                pictureBox4.BackColor = Color.Green;
            if (step1 == true && step2 == true && step3 == true && step4 == true)
            {
                MessageBox.Show("Gratulację wprowadziłeś poprawne dane");
                textBox3.Enabled = true;
                button5.Enabled = true;
            }
            else
            {
                MessageBox.Show("Walidacja nie powiodła się, proszę poprawić kroki zaznaczone na czerwono");
            }
        }

        // metody pomocnicze

        public static Dictionary<Double, List<Double>> wypelnij(List<Double> unikalne)
        {
            Dictionary<Double, List<Double>> drogi = new Dictionary<Double, List<Double>>();
            for (int i = 0; i < unikalne.Count; i++)
                drogi.Add(unikalne[i], new List<Double>());
            return drogi;
        }


        public static int minSuma(Dictionary<Double, List<Double>> drogi)
        {
            int min = drogi.First().Value.Count;
            foreach (KeyValuePair<Double, List<Double>> kvp in drogi)
                if (min > kvp.Value.Count)
                    min = kvp.Value.Count;
            return min;
        }

        public static void oblicz(Dictionary<Double, List<Double>> drogi, List<Probka> ListofProbes, Probka testowa, string metryka, double p)
        {
            double suma = 0;
            for (int i = 0; i < ListofProbes.Count; i++)
            {
                if (metryka == "Metryka euklidesowa")
                    suma = Metryka.MetrykaEuklides(ListofProbes[i].atrybuty, testowa.atrybuty);
                if (metryka == "Metryka Manhattan")
                    suma = Metryka.MetrykaManhattan(ListofProbes[i].atrybuty, testowa.atrybuty);
                if (metryka == "Metryka z logarytmem")
                    suma = Metryka.MetrykaLogarytm(ListofProbes[i].atrybuty, testowa.atrybuty);
                if (metryka == "Metryka Czebyszewa")
                    suma = Metryka.MetrykaCzebyszewa(ListofProbes[i].atrybuty, testowa.atrybuty);
                if (metryka == "Metryka Minkowskiego")
                    suma = Metryka.MetrykaMinkowskiego(ListofProbes[i].atrybuty, testowa.atrybuty, p);
                drogi[Convert.ToInt32(ListofProbes[i].klasa)].Add(suma);
            }
        }

        public static void setValues(Double[] values, Double[] key, Dictionary<Double, List<Double>> drogi, int k)
        {
            int t = 0;
            foreach (KeyValuePair<Double, List<Double>> kvp in drogi)
            {
                key[t] = kvp.Key;
                for (int i = 0; i < k; i++)
                {
                    values[t] += kvp.Value[i];
                }
                t++;
            }
        }

        public static double max(List<Double> atribs)
        {
            double max = atribs[0];
            for (int i = 1; i < atribs.Count; i++)
                if (max < atribs[i])
                    max = atribs[i];
            return max;
        }

        public static double min(List<Double> atribs)
        {
            double min = atribs[0];
            for (int i = 1; i < atribs.Count; i++)
                if (min > atribs[i])
                    min = atribs[i];
            return min;
        }

        public static Double min(Double[] wynikowa)
        {
            Double min = wynikowa[0];
            Double index = 0;
            int ile = 0;
            for (int i = 1; i < wynikowa.Length; i++)
                if (min > wynikowa[i])
                {
                    min = wynikowa[i];
                    index = i;
                }

            for (int i = 0; i < wynikowa.Length; i++)
                if (min == wynikowa[i])
                    ile++;

            if (ile != 1)
                return -1;

            return index;
        }

    }

    public class Probka 
    {
        public Double klasa;
        public List<Double> atrybuty = new List<Double>();
    }

    public class Metryka : Form1
    {
        public static Double MetrykaEuklides(List<Double> v1, List<Double> v2)
        {
            Double suma = 0;
            for (int i = 0; i < v1.Count; i++)
                suma += (v1[i] - v2[i]) * (v1[i] - v2[i]);
            return Math.Sqrt(suma);
        }

        public static Double MetrykaManhattan(List<Double> v1, List<Double> v2)
        {
            Double suma = 0;
            for (int i = 0; i < v1.Count; i++)
                suma += Math.Abs(v1[i] - v2[i]);
            return suma;
        }

        public static Double MetrykaCzebyszewa(List<Double> v1, List<Double> v2)
        {
            Double max = -1;
            for (int i = 0; i < v1.Count; i++)
            {
                if (max < Math.Abs(v1[i] - v2[i]))
                {
                    max = Math.Abs(v1[i] - v2[i]);
                }
            }
            return max;
        }

        public static Double MetrykaMinkowskiego(List<Double> v1, List<Double> v2, double p)
        {
            Double suma = 0;
            double podziel = 1 / p;
            for (int i = 0; i < v1.Count; i++)
                suma += Math.Pow(Math.Abs((v1[i] - v2[i])), p);
            return (double)Math.Pow(suma, podziel);
        }

        public static Double MetrykaLogarytm(List<Double> v1, List<Double> v2)
        {
            Double suma = 0;
            for (int i = 0; i < v1.Count; i++)
                suma += Math.Abs(Math.Log(v1[i], 10) - Math.Log(v2[i], 10));
            return suma;
        }
    }

}
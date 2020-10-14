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

namespace WindowsFormsApplication4
{
    public partial class Form1 : Form
    {

        //Zmienne do Sieci Neuronowej
        List<double[]> Options = new List<double[]>();
        List<double[]> Expected = new List<double[]>();
        public string path;
        public double bias;
        public double beta;
        public double mi;
        public int[] numerki;
        public bool CzyOdczyt;
        public bool CzyZapis;
        public double Epsilon;

        //Zmienne do obsługi Forms
        OpenFileDialog ofd = new OpenFileDialog();
        OpenFileDialog ofd2 = new OpenFileDialog();
        public static Random rand = new Random();
        public bool validated = false;
        public int counter = 0;
        public double max;
        public double max2 = 0;

        public Form1()
        {
            InitializeComponent();
            Options.Add(new double[] { 0, 0 });
            Options.Add(new double[] { 1, 1 });
            Options.Add(new double[] { 1, 0 });
            Options.Add(new double[] { 0, 1 });

            Expected.Add(new double[] { 0 });
            Expected.Add(new double[] { 1 });
            Expected.Add(new double[] { 1 });
            Expected.Add(new double[] { 0 });

            printOptionsAndExpected();
        }

        // metoda odpowiedzialna za inicjalizację sieci

        public static List<Warstwa> InicjalizujSiec(int[] numerki, double[] wejscia, double bias, double beta, bool CzyWagiZPliku, string path)
        {
            List<Warstwa> SiecNeuronowa = new List<Warstwa>();
            SiecNeuronowa = dodajWarstwy(SiecNeuronowa, numerki.Length);
            SiecNeuronowa = dodajNeurony(SiecNeuronowa, numerki, bias, beta);
            if (CzyWagiZPliku)
                SiecNeuronowa = pobierzWagi(SiecNeuronowa, numerki, path);
            else
                SiecNeuronowa = dodajWagi(SiecNeuronowa, numerki);
            SiecNeuronowa = dodajWartosciWejsciowe(SiecNeuronowa, wejscia);
            SiecNeuronowa = obliczWyjscia(SiecNeuronowa);
            return SiecNeuronowa;
        }

        // metody obsługujące inicjalizację sieci

        public static List<Warstwa> dodajWarstwy(List<Warstwa> SiecNeuronowa, int ile)
        {
            for (int i = 0; i < ile; i++)
                SiecNeuronowa.Add(new Warstwa());
            return SiecNeuronowa;
        }

        public static List<Warstwa> dodajNeurony(List<Warstwa> SiecNeuronowa, int[] numerki, double bias, double beta)
        {
            for (int i = 0; i < numerki.Length; i++)
                for (int j = 0; j < numerki[i]; j++)
                    SiecNeuronowa[i].Neurony.Add(new Neuron(bias, beta));
            return SiecNeuronowa;
        }

        public static List<Warstwa> dodajWartosciWejsciowe(List<Warstwa> SiecNeuronowa, double[] wejscia)
        {
            for (int i = 0; i < SiecNeuronowa[0].Neurony.Count; i++)
            {
                SiecNeuronowa[0].Neurony[i].wejscie.Clear();
                for (int j = 0; j < wejscia.Length; j++)
                    SiecNeuronowa[0].Neurony[i].wejscie.Add(wejscia[j]);
                SiecNeuronowa[0].Neurony[i].obliczwyjscie();
            }
            return SiecNeuronowa;
        }

        public static List<Warstwa> obliczWyjscia(List<Warstwa> SiecNeuronowa)
        {
            for (int i = 0; i < SiecNeuronowa[0].Neurony.Count; i++)
                SiecNeuronowa[0].Neurony[i].obliczwyjscie();
            for (int i = 1; i < SiecNeuronowa.Count; i++)
            {
                for (int k = 0; k < SiecNeuronowa[i].Neurony.Count; k++)
                {
                    SiecNeuronowa[i].Neurony[k].wejscie.Clear();
                    for (int j = 0; j < SiecNeuronowa[i - 1].Neurony.Count; j++)
                    {
                        SiecNeuronowa[i].Neurony[k].wejscie.Add(SiecNeuronowa[i - 1].Neurony[j].wyjscie);
                        SiecNeuronowa[i].Neurony[k].obliczwyjscie();
                    }
                }
            }
            return SiecNeuronowa;
        }

        // metody obsługujące wagi

        public static List<double> LosujWagi(int ile, int max, int min)
        {
            List<double> nowy = new List<double>();
            for (int i = 0; i < ile; i++)
                nowy.Add(rand.NextDouble() * (max - min) + min);
            return nowy;
        }

        public static List<Warstwa> dodajWagi(List<Warstwa> SiecNeuronowa, int[] numerki)
        {
            for (int i = 0; i < SiecNeuronowa.Count; i++)
                for (int j = 0; j < numerki[i]; j++)
                    if (i == 0)
                        SiecNeuronowa[i].Neurony[j].wagi = LosujWagi(numerki[i] + 1, 1, -1);
                    else
                        SiecNeuronowa[i].Neurony[j].wagi = LosujWagi(numerki[i - 1] + 1, 1, -1);
            return SiecNeuronowa;
        }

        public static List<Warstwa> pobierzWagi(List<Warstwa> SiecNeuronowa, int[] numerki, string sciezka)
        {
            string[] lines = File.ReadAllLines(sciezka);
            int[] WzorSiecizPliku = lines[0].Split('/').Select(n => Convert.ToInt32(n)).ToArray();

            if (numerki.Length != WzorSiecizPliku.Length)
                throw new Exception("Ilość warstw się nie zgadza");

            for (int i = 0; i < numerki.Length; i++)
                if (numerki[i] != WzorSiecizPliku[i])
                    throw new Exception("Ilość neuronów się nie zgadza");

            int k = 1;
            for (int i = 0; i < SiecNeuronowa.Count; i++)
            {
                for (int j = 0; j < SiecNeuronowa[i].Neurony.Count; j++)
                {
                    SiecNeuronowa[i].Neurony[j].wagi = Array.ConvertAll(lines[k].Split('/'), Double.Parse).ToList();
                    k++;
                }
            }

            return SiecNeuronowa;
        }

        public static void zapiszWagi(List<Warstwa> SiecNeuronowa, int[] numerki, string path)
        {
            string lines = "";

            lines += String.Join("/", numerki) + "\r\n";
            for (int i = 0; i < SiecNeuronowa.Count; i++)
                for (int j = 0; j < SiecNeuronowa[i].Neurony.Count; j++)
                    lines += String.Join("/", SiecNeuronowa[i].Neurony[j].wagi.ToArray()) + "\r\n";

            System.IO.StreamWriter file = new System.IO.StreamWriter(path);
            file.WriteLine(lines);
            file.Close();
        }

        //Metoda odpowiedzialna za propagację wsteczną

        public static void propagacjaWsteczna(List<Warstwa> SiecNeuronowa, double[] d, double mi)
        {
            List<double> blad = obliczBlad(SiecNeuronowa, d);
            List<double> KorektaWag = new List<double>();

            for (int k = SiecNeuronowa.Count - 1; k >= 0; k--)
            {
                List<double> KorektaWyj = korektaWyjsc(SiecNeuronowa, blad, KorektaWag, mi, k);
                List<double> KorektaS = korektaS(SiecNeuronowa, KorektaWyj, k);
                zastosujZmiany(KorektaS, KorektaWyj, KorektaWag, SiecNeuronowa, k);
            }
        }


        //metody obsługujące propagację wsteczną

        public static List<double> obliczBlad(List<Warstwa> SiecNeuronowa, double[] d)
        {
            List<double> blad = new List<double>();
            for (int i = 0; i < d.Length; i++)
                blad.Add(d[i] - SiecNeuronowa[SiecNeuronowa.Count - 1].Neurony[i].wyjscie);
            return blad;
        }

        public static List<double> korektaWyjsc(List<Warstwa> SiecNeuronowa, List<double> blad, List<double> KorektaWag, double mi, int k)
        {
            List<double> KorektaWyj = new List<double>();
            if (k == SiecNeuronowa.Count - 1)
                for (int i = 0; i < SiecNeuronowa[k].Neurony.Count; i++)
                    KorektaWyj.Add(mi * blad[i]);
            else
                for (int i = 0; i < KorektaWag.Count; i++)
                    KorektaWyj.Add(KorektaWag[i]);
            return KorektaWyj;
        }

        public static List<double> korektaS(List<Warstwa> SiecNeuronowa, List<double> KorektaWyj, int k)
        {
            List<double> KorektaS = new List<double>();
            for (int i = 0; i < SiecNeuronowa[k].Neurony.Count; i++)
                KorektaS.Add(KorektaWyj[i] * SiecNeuronowa[k].Neurony[i].beta * SiecNeuronowa[k].Neurony[i].wyjscie * (1 - SiecNeuronowa[k].Neurony[i].wyjscie));
            return KorektaS;
        }

        public static List<double> korektaWejsc(List<Warstwa> SiecNeuronowa, List<double> KorektaS, int k, int j)
        {
            List<double> KorektaWejsc = new List<double>();
            KorektaWejsc.Add(SiecNeuronowa[k].Neurony[j].bias * KorektaS[j]);
            for (int i = 0; i < SiecNeuronowa[k].Neurony[j].wejscie.Count; i++)
                KorektaWejsc.Add(SiecNeuronowa[k].Neurony[j].wejscie[i] * KorektaS[j]);
            return KorektaWejsc;
        }

        public static void zastosujZmiany(List<double> KorektaS, List<double> KorektaWyj, List<double> KorektaWag, List<Warstwa> SiecNeuronowa, int k)
        {
            for (int j = 0; j < KorektaS.Count; j++)
            {
                List<double> KorektaWejsc = korektaWejsc(SiecNeuronowa, KorektaS, k, j);
                for (int i = 0; i < SiecNeuronowa[k].Neurony[j].wagi.Count; i++)
                {
                    if (i != 0)
                        KorektaWag.Add(SiecNeuronowa[k].Neurony[j].wagi[i] * KorektaS[j]);
                    SiecNeuronowa[k].Neurony[j].wagi[i] += KorektaWejsc[i];
                }
            }
        }

        // Metoda ucząca sieć

        public static List<Warstwa> UczenieSieci(List<Warstwa> SiecNeuronowa, List<double[]> Options, List<double[]> ExpectedValues, double mi, int ileEpok)
        {
            for (int i = 0; i < ileEpok; i++)
            {
                List<double[]> tmp = new List<double[]>(Options);
                List<double[]> tmp2 = new List<double[]>(ExpectedValues);
                for (int j = 0; j < Options.Count; j++)
                {
                    int index = rand.Next(tmp.Count);
                    SiecNeuronowa = dodajWartosciWejsciowe(SiecNeuronowa, tmp[index]);
                    SiecNeuronowa = obliczWyjscia(SiecNeuronowa);
                    propagacjaWsteczna(SiecNeuronowa, tmp2[index], mi);
                    tmp.RemoveAt(index);
                    tmp2.RemoveAt(index);
                }
            }
            return SiecNeuronowa;
        }


        //MAIN

        public void MainStart()
        {
            List<double[]> Options = this.Options;
            List<double[]> ExpectedValues = this.Expected;
            saveMAX(ExpectedValues);
            ExpectedValues = changeFrom0To1(ExpectedValues);
            int[] numerki = this.numerki;
            int ileEpok = 300;
            int ileUczen = 0;
            bool CzyNauczono = true;

            List<Warstwa> SiecNeuronowa = InicjalizujSiec(numerki, Options[0], bias, beta, CzyOdczyt, path);

            while (CzyNauczono)
            {
                SiecNeuronowa = UczenieSieci(SiecNeuronowa, Options, ExpectedValues, mi, ileEpok);
                int counter = 0;
                for (int i = 0; i < Options.Count; i++)
                {
                    SiecNeuronowa = dodajWartosciWejsciowe(SiecNeuronowa, Options[i]);
                    SiecNeuronowa = obliczWyjscia(SiecNeuronowa);
                    for (int j = 0; j < SiecNeuronowa[SiecNeuronowa.Count - 1].Neurony.Count; j++)
                    {
                        if ((ExpectedValues[i][j] * 10) - (SiecNeuronowa[SiecNeuronowa.Count - 1].Neurony[j].wyjscie * 10) < Epsilon)
                            counter++;
                    }
                }
                ileUczen += 300;
                if (counter == Expected.Count*Expected[0].Length || ileUczen > 35000)
                        CzyNauczono = false;
            }


            label23.Text = "Ile epok trwało uczenie: "+ileUczen;

            label24.Text = "Czy uczenie powiodło się: TAK";
            printOutput(SiecNeuronowa, ExpectedValues);
            if (CzyZapis)
                zapiszWagi(SiecNeuronowa, numerki, path);
            if (ileUczen > 35000)
                repeat();
        }
        public double rep = 0;

        public void repeat()
        {
            if (rep < 5)
            {
                rep++;
                MainStart();
            }
            else
                return;
        }

        // Metody obsługujące Froms

        public void printOutput(List<Warstwa> SiecNeuronowa, List<double[]> ExpectedValues)
        {
            clearOutput();
            for (int i = 0; i < Options.Count; i++)
            {
                SiecNeuronowa = dodajWartosciWejsciowe(SiecNeuronowa, Options[i]);
                SiecNeuronowa = obliczWyjscia(SiecNeuronowa);
                for (int j = 0; j < SiecNeuronowa[1].Neurony.Count; j++)
                {
                    textBox12.Text += (SiecNeuronowa[1].Neurony[j].wyjscie) + "\r\n";
                    textBox9.Text += (ExpectedValues[i][j]) + "\r\n";
                    textBox13.Text += Math.Abs((ExpectedValues[i][j] - SiecNeuronowa[1].Neurony[j].wyjscie)) + "\r\n";
                }

            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            counter = 0;
            CzyOdczyt = checkBox1.Checked;
            CzyZapis = checkBox2.Checked;
            if (checkBox1.Checked == true || checkBox2.Checked == true)
                if (label11.Text == "Ścieżka: brak")
                {
                    counter++;
                    MessageBox.Show("Podaj Ścieżkę do pliku!");
                }
            if (textBox4.Text != "")
                numerki = textBox4.Text.Split('/').Select(n => Convert.ToInt32(n)).ToArray();
            else
            {
                MessageBox.Show("Podaj definicję sieci!");
                counter++;
            }

            bias = inputValidate(textBox2.Text, "Wpisz wartość w polu bias!");
            beta = inputValidate(textBox6.Text, "Wpisz wartość w polu beta!");
            mi = inputValidate(textBox3.Text, "Wpisz wartość w polu współczynnika uczenia!");
            Epsilon = inputValidate(textBox15.Text, "Wpisz wartość w polu Epsilon!");

            if (Options.Count != numericUpDown1.Value)
            {
                counter++;
                MessageBox.Show("Ilość opcji nie zgadza się z deklaraowaną wartością!");
            }

            if (Expected[0].Length != numerki[numerki.Length-1])
            {
                counter++;
                MessageBox.Show("definicja sieci nie zgadza się z ilością wprowadzonych danych!");
            }

            if (counter == 0)
            {
                label20.BackColor = Color.Green;
                MessageBox.Show("Gratulację walidacja zakończona sukcesem!");
                textBox1.Text = "";
                clearOutput();
                printNetwork(numerki.Max());
                if (CzyOdczyt == true)
                    label25.Text = "Czy odczytano wagi z pliku: TAK";
                else
                    label25.Text = "Czy odczytano wagi z pliku: NIE";
                if (CzyZapis == true)
                    label22.Text = "Czy zapisano wagi do pliku: TAK";
                else
                    label22.Text = "Czy zapisano wagi do pliku: NIE";
                MainStart();
            }
            else
                label20.BackColor = Color.Red;

        }

        public void printNetwork(int max)
        {
            textBox1.Text += "\tL0\tL1\tL2\tL3\tL4\tL5\tL6\tL7\tL8   ...";
            textBox1.Text += "\r\n";
            for (int i = 0; i < max; i++)
            {
                if (i < 9)
                    textBox1.Text += (i + "\t");
                else if (i < 12)
                    textBox1.Text += (".\t");
                else
                    textBox1.Text += ("\t");
                for (int j = 0; j < numerki.Length; j++)
                    if (numerki[j] > i)
                        textBox1.Text += ("N\t");
                    else
                        textBox1.Text += ("\t");
                textBox1.Text += "\r\n";
            }
        }

        public void clearOutput()
        {
            textBox12.Text = "";
            textBox9.Text = "";
            textBox13.Text = "";
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if(Options.Count < numericUpDown1.Value){
                double[] doubles = Array.ConvertAll(textBox5.Text.Split('/'), double.Parse);
                Options.Add(doubles);
                doubles = Array.ConvertAll(textBox7.Text.Split('/'), double.Parse);
                Expected.Add(doubles);
                printOptionsAndExpected();
            }
            else
            {
                MessageBox.Show("Nie możesz dodać następnego, zwiększ ilość opcji!");
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            textBox10.Text = "";
            textBox11.Text = "";
            Options.Clear();
            Expected.Clear();
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            textBox10.Text = "";
            textBox11.Text = "";
            Options.Clear();
            Expected.Clear();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ofd.Filter = "TXT|*.txt";
            if (ofd.ShowDialog() == DialogResult.OK)
            {

                label11.Text = ofd.FileName;
                path = ofd.FileName;
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            ofd2.Filter = "TXT|*.txt";
            string pathToExpectedAndOptions = "";
            if (ofd2.ShowDialog() == DialogResult.OK)
            {
                pathToExpectedAndOptions = ofd2.FileName;
                string[] lines = File.ReadAllLines(pathToExpectedAndOptions);
                int[] Wzorzec = lines[0].Split('/').Select(n => Convert.ToInt32(n)).ToArray();
                int[] WzorzecSieci = textBox4.Text.Split('/').Select(n => Convert.ToInt32(n)).ToArray();

                if (WzorzecSieci[0] + WzorzecSieci[WzorzecSieci.Length - 1] == Wzorzec.Length)
                {
                    if (numericUpDown1.Value == lines.Length)
                    {
                        Options.Clear();
                        Expected.Clear();
                        for (int i = 0; i < lines.Length; i++)
                        {
                            double[] tmp = Array.ConvertAll(lines[i].Split('/'), Double.Parse);
                            Options.Add(tmp.Take(WzorzecSieci[0]).ToArray());
                            Expected.Add(tmp.Reverse().Take(WzorzecSieci[WzorzecSieci.Length - 1]).Reverse().ToArray());
                        }
                    }
                    else
                        MessageBox.Show("ilość wartości w pliku nie zgadza się z deklarowaną ilością opcji");
                }
                else
                    MessageBox.Show("Definicja sieci nie zgadza się z zawartością pliku");
            }
            printOptionsAndExpected();
        }

        public void printOptionsAndExpected()
        {
            textBox10.Text = "";
            textBox11.Text = "";
            for (int i = 0; i < Options.Count; i++)
            {
                for (int j = 0; j < Options[i].Length; j++)
                    if (j == Options[i].Length-1)
                        textBox10.Text += Options[i][j];
                    else
                        textBox10.Text += Options[i][j] + "/";
                for (int j = 0; j < Expected[i].Length; j++)
                    if (j == Expected[i].Length - 1)
                        textBox11.Text += Expected[i][j];
                    else
                        textBox11.Text += Expected[i][j] + "/";
                textBox10.Text += "\r\n";
                textBox11.Text += "\r\n";
            }
        }

        public double inputValidate(string input, string error)
        {
            double number;
            double val = 0;
            if (input != "")
            {
                if (Double.TryParse(input, out number))
                    val = Convert.ToDouble(input);
            }
            else
            {
                MessageBox.Show(error);
                counter++;
            }
            if (val == 0)
            {
                MessageBox.Show("wartość nie może równać się 0");
                counter++;
            }
            return val;
        }

        public void saveMAX(List<double[]> ExpectedValues)
        {
            if (max2 == 0)
                for (int i = 0; i < ExpectedValues.Count; i++)
                    for (int j = 0; j < ExpectedValues[i].Length; j++)
                        if (max2 < ExpectedValues[i][j])
                            max2 = ExpectedValues[i][j];
        }

        public List<double[]> changeFrom0To1(List<double[]> ExpectedValues)
        {
            max = 0;

            for (int i = 0; i < ExpectedValues.Count; i++)
                for (int j = 0; j < ExpectedValues[i].Length; j++)
                    if (max < ExpectedValues[i][j])
                        max = ExpectedValues[i][j];

            for (int i = 0; i < ExpectedValues.Count; i++)
                for (int j = 0; j < ExpectedValues[i].Length; j++)
                    ExpectedValues[i][j] = ExpectedValues[i][j] / max;

            return ExpectedValues;
        }

    }


    public class Neuron
    {
        public List<double> wagi = new List<double>();
        public List<double> wejscie = new List<double>();
        public double beta;
        public double bias;
        public double wyjscie;

        public Neuron(double bias, double beta)
        {
            this.bias = bias;
            this.beta = beta;
        }

        public void obliczwyjscie()
        {
            double s = 0;

            s += bias * wagi[0];
            for (int i = 0; i < wejscie.Count; i++)
                s += wejscie[i] * wagi[i + 1];

            s = 1 / (1 + Math.Pow(Math.E, -beta * s));

            this.wyjscie = s;
        }

    }

    public class Warstwa
    {
        public List<Neuron> Neurony = new List<Neuron>();
    }

}

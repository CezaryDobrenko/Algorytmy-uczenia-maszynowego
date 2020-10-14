using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApplication5
{
    public partial class Form1 : Form
    {

        public static Random rand = new Random();
        public static int counter = 1;

        public static List<Entity> createEntities(int howManyEntities)
        {
            List<Entity> Entities = new List<Entity>();
            for (int i = 0; i < howManyEntities; i++)
                Entities.Add(new Entity());
            return Entities;
        }

        public static List<int> generateRandomBits(int howManyBits)
        {
            List<int> output = new List<int>();
            for (int i = 0; i < howManyBits; i++)
                output.Add(rand.Next(0, 2));
            return output;
        }

        public static List<Entity> addRadomStrings(List<Entity> Entities, int[] howManyBitsPerParam)
        {
            for (int i = 0; i < Entities.Count; i++)
            {
                for (int j = 0; j < howManyBitsPerParam.Length; j++)
                {
                    List<int> stringOfRandomBits = generateRandomBits(howManyBitsPerParam[j]);
                    Entities[i].addString(stringOfRandomBits);
                }
            }
            return Entities;
        }

        public static List<Param> createParams(int howManyParams)
        {
            List<Param> Params = new List<Param>();
            for (int i = 0; i < howManyParams; i++)
                Params.Add(new Param());
            return Params;
        }

        public static List<Param> addKeysAndValuesToAssocTable(List<Param> Params, int[] howManyBitsPerParam, double max, double min)
        {
            for (int i = 0; i < howManyBitsPerParam.Length; i++)
            {
                int howManyOptions = Convert.ToInt32(Math.Pow(2, howManyBitsPerParam[i]));
                double outp = (max - min) / (howManyOptions - 1);
                for (int j = 0; j < howManyOptions; j++)
                {
                    string keyInBits = adjustOutputStringToCorrectFormat(Convert.ToString(j, 2), howManyBitsPerParam[i]);
                    Params[i].addToTable(keyInBits, min + j * outp);
                }

            }
            return Params;
        }

        public static List<Entity> calculateAdaptationFunction(List<Entity> Entities, List<Param> Params)
        {
            for (int i = 0; i < Entities.Count; i++)
            {
                List<double> xArg = new List<double>();
                for (int j = 0; j < Entities[i].binaryStrings.Count; j++)
                {
                    //Console.WriteLine("x"+j+" : "+string.Join("", Entities[i].binaryStrings[j].ToArray())+ " -> " + Params[j].paramsTable[string.Join("", Entities[i].binaryStrings[j].ToArray())]);
                    xArg.Add(Params[j].paramsTable[string.Join("", Entities[i].binaryStrings[j].ToArray())]);
                }
                double calculatePattern = Math.Sin(xArg[0] * 0.05) + Math.Sin(xArg[1] * 0.05) + 0.4 * Math.Sin(xArg[0] * 0.15) * Math.Sin(xArg[1] * 0.15);
                Entities[i].adaptationValue = calculatePattern;
            }
            return Entities;
        }

        public List<Entity> GenericAlgorithm(List<Entity> Entities, List<Param> Params, int TournamentSize, int HowManyTimes)
        {
            for (int i = 0; i < HowManyTimes; i++)
            {
                List<Entity> newEntities = new List<Entity>();

                //Operator turnieju punkt 3.1
                for (int j = 0; j < Entities.Count - 1; j++)
                    newEntities.Add(Tournament(Entities, TournamentSize));

                //Operator mutacji jednopunktowej 3.2
                newEntities = Mutation(newEntities);

                //Operator Hot Deck punkt 3.3
                newEntities.Add(hotDeck(Entities));

                //Oblicz nowe wartości dla funkcji przystosowania punkt 3.4
                newEntities = calculateAdaptationFunction(newEntities, Params);

                //Wypisz wartości max i średnią na populację osobników punkt 3.5
                getMaxAndAverageFromEntities(newEntities, Params);

                //Zamiana starej puli osobników na nową punkt 3.6
                Entities = newEntities;

            }

            return Entities;

        }

        public void getMaxAndAverageFromEntities(List<Entity> newEntities, List<Param> Params)
        {
            double max = hotDeck(newEntities).adaptationValue;
            double sum = 0;

            for (int i = 0; i < newEntities.Count; i++)
                sum += newEntities[i].adaptationValue;

            textBox1.Text += "generacja: " + counter;
            textBox1.Text += "\tMAX: " + max;
            textBox1.Text += "\tAVG: " + sum / newEntities.Count;
            textBox1.Text += "\r\n";
            counter++;
        }

        public static Entity Tournament(List<Entity> Entities, int TournamentSize)
        {
            List<Entity> Population = new List<Entity>(Entities);
            List<Entity> TournamentSquad = new List<Entity>();

            if (TournamentSize > Entities.Count)
                throw new Exception("Ilość osobników w turnieju nie może być większa niż w populacji!");

            for (int i = 0; i < TournamentSize; i++)
            {
                int YouWereTheChosenOne = rand.Next(Population.Count);
                TournamentSquad.Add(Population[YouWereTheChosenOne]);
                Population.RemoveAt(YouWereTheChosenOne);
            }

            return hotDeck(TournamentSquad);
        }

        public static List<Entity> Mutation(List<Entity> newEntities)
        {
            for (int i = 0; i < newEntities.Count; i++)
            {
                int j = rand.Next(newEntities[i].binaryStrings.Count);
                int r = rand.Next(newEntities[i].binaryStrings[j].Count);
                newEntities[i].binaryStrings[j][r] = 1 - newEntities[i].binaryStrings[j][r];
            }
            return newEntities;
        }

        public static Entity hotDeck(List<Entity> Entities)
        {
            Entity Winner = Entities[0];
            for (int i = 1; i < Entities.Count; i++)
                if (Winner.adaptationValue < Entities[i].adaptationValue)
                    Winner = Entities[i];
            return klonujObiekt(Winner);
        }

        public static Entity klonujObiekt(Entity obiekt)
        {
            Entity klon = new Entity();
            klon.adaptationValue = obiekt.adaptationValue;
            List<List<int>> cloneList = new List<List<int>>();
            for (int i = 0; i < obiekt.binaryStrings.Count; i++)
            {
                List<int> klonowanie = new List<int>();
                for (int j = 0; j < obiekt.binaryStrings[i].Count; j++)
                {
                    klonowanie.Add(obiekt.binaryStrings[i][j]);
                }
                cloneList.Add(klonowanie);
            }
            klon.binaryStrings = cloneList;
            return klon;
        }

        public static string adjustOutputStringToCorrectFormat(string bitString, int len)
        {
            if (bitString.Length != len)
            {
                string tmp = bitString;
                int length = bitString.Length;
                bitString = "";
                for (int i = 0; i < len - length; i++)
                    bitString += "0";
                bitString += tmp;
            }
            return bitString;
        }

        public void printEntitiesWP(List<Entity> Entities, List<Param> Params)
        {
            textBox8.Text = "";
            for (int i = 0; i < Entities.Count; i++)
            {
                textBox8.Text += "Osobnik " + i + "\r\n";
                textBox8.Text += "Ciągi binarne:\r\n";
                for (int j = 0; j < Entities[i].binaryStrings.Count; j++)
                {
                    for (int k = 0; k < Entities[i].binaryStrings[j].Count; k++)
                       textBox8.Text += Entities[i].binaryStrings[j][k] + ",";
                   textBox8.Text += " -> " + Params[j].paramsTable[string.Join("", Entities[i].binaryStrings[j].ToArray())] + "\r\n";
                }
                textBox8.Text += "Wartość funkcji przystosowania: \r\n" + Entities[i].adaptationValue + "\r\n\r\n";
            }
        }

        public void printParams(List<Param> Params, int[] howManyBitsPerParam)
        {
            textBox7.Text = "";
            for (int i = 0; i < Params.Count; i++)
            {
                textBox7.Text += "Parametr " + i + "\r\n";
                for (int j = 0; j < Params[i].paramsTable.Count; j++)
                {
                    string keyInBits = adjustOutputStringToCorrectFormat(Convert.ToString(j, 2), howManyBitsPerParam[i]);
                    textBox7.Text += keyInBits + " -> " + Params[i].paramsTable[keyInBits] + "\r\n";
                }
            }
        }

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
        public bool checkIfIsOnlyNumberAndNotZero(string Napis, string ErrorMessage, bool AllowZero)
        {
            int number;
            if (Napis.Length == 0)
            {
                MessageBox.Show(ErrorMessage + " i nie może być puste");
                return false;
            }
            if (!int.TryParse(Napis, out number))
            {
                MessageBox.Show(ErrorMessage);
                return false;
            }
            if (number == 0 && AllowZero == false)
            {
                MessageBox.Show(ErrorMessage + ", i nie może być 0");
                return false;
            }
            return true;
        }

        public bool checkIfArrayContainsZero(int[] array)
        {
            for(int i = 0; i < array.Length; i++)
                if (array[i] == 0)
                    return true;
            return false;
        }

        public bool checkIfIsValid()
        {
            try
            {
                int[] ia = textBox9.Text.Split('/').Select(n => Convert.ToInt32(n)).ToArray();
                
            }
            catch
            {
                MessageBox.Show("Pole: Podaj ilość bitów przypadającą na parametr może zawierać tylko liczby całkowite odzielone separatorem i nie może być puste");
                return false;
            }

            if (checkIfArrayContainsZero(textBox9.Text.Split('/').Select(n => Convert.ToInt32(n)).ToArray()))
            {
                MessageBox.Show("Wartości w tablicy nie mogą zawierać 0");
                return false;
            }

            
            if (checkIfIsOnlyNumberAndNotZero(textBox2.Text, "Pole: Podaj ilość osobników w populacji musi zawierać wyłącznie cyfry",false))
                if (checkIfIsOnlyNumberAndNotZero(textBox5.Text, "Pole: MIN musi zawierać wyłącznie cyfry", true))
                    if (checkIfIsOnlyNumberAndNotZero(textBox4.Text, "Pole: MAX musi zawierać wyłącznie cyfry", true))
                        if (checkIfIsOnlyNumberAndNotZero(textBox6.Text, "Pole: Podaj rozmiar turnieju musi zawierać wyłącznie cyfry", false))
                            if (checkIfIsOnlyNumberAndNotZero(textBox3.Text, "Pole: Podaj ilość generacji (pokoleń) musi zawierać wyłącznie cyfry", false))
                                if (textBox5.Text != textBox4.Text)
                                    return true;
                                else
                                    MessageBox.Show("Wartości Min i Max nie mogą być takie same");   
            return false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            counter = 0;
            bool isValid = checkIfIsValid();
            if (isValid)
            {
                textBox1.Text = "";
                int[] howManyBitsPerParam = Array.ConvertAll(textBox9.Text.Split('/'), int.Parse);
                int howManyEntities = Convert.ToInt32(textBox2.Text);
                int min = Convert.ToInt32(textBox5.Text);
                int max = Convert.ToInt32(textBox4.Text);
                int TournamentSize = Convert.ToInt32(textBox6.Text);
                int HowManyTimes = Convert.ToInt32(textBox3.Text);

                List<Entity> Entities = createEntities(howManyEntities);
                Entities = addRadomStrings(Entities, howManyBitsPerParam);

                List<Param> Params = createParams(howManyBitsPerParam.Length);
                Params = addKeysAndValuesToAssocTable(Params, howManyBitsPerParam, max, min);

                Entities = calculateAdaptationFunction(Entities, Params);
                Entities = GenericAlgorithm(Entities, Params, TournamentSize, HowManyTimes);

                printEntitiesWP(Entities, Params);
                printParams(Params, howManyBitsPerParam);
            }
            else
            {
                MessageBox.Show("Walidacja się nie powiodła");
            }
        }
    }

    public class Entity
    {
        public List<List<int>> binaryStrings = new List<List<int>>();
        public double adaptationValue;

        public void addAdaptationValue(double adaptationValue)
        {
            this.adaptationValue = adaptationValue;
        }

        public void addString(List<int> listOfBits)
        {
            this.binaryStrings.Add(listOfBits);
        }
    }

    public class Param
    {
        public Dictionary<string, double> paramsTable = new Dictionary<string, double>();

        public void addToTable(string key, double value)
        {
            paramsTable.Add(key, value);
        }

    }
}

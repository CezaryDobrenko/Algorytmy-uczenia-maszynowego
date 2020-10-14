using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApplication6
{

    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        List<double[]> Options = new List<double[]>();
        List<double[]> Expected = new List<double[]>();
        public static int maxLimit = 0;
        public static Random rand = new Random();

        public static Network createLayers(Network Entity, int howManyLayers)
        {
            for (int i = 0; i < howManyLayers; i++)
                Entity.neuralNetwork.Add(new Layer());
            return Entity;
        }

        public static Network createNeurons(Network Entity, int[] networkSchema, double bias, double beta)
        {
            for (int i = 0; i < Entity.neuralNetwork.Count; i++)
                for (int j = 0; j < networkSchema[i]; j++)
                    Entity.neuralNetwork[i].neurons.Add(new Neuron(bias, beta));
            return Entity;
        }


        public static Network createRandomBinaryStrings(Network Entity, int[] howManyWeights, int[] howManyBitsPerParam)
        {
            for (int i = 0; i < Entity.neuralNetwork.Count; i++)
                for (int j = 0; j < Entity.neuralNetwork[i].neurons.Count; j++)
                    Entity.neuralNetwork[i].neurons[j].binaryStrings = generateRandomBinaryString(howManyWeights[i], howManyBitsPerParam[i]);
            return Entity;
        }

        public static Network addEntriesValues(Network Entity, List<double[]> Options, int queue)
        {
            for (int i = 0; i < Entity.neuralNetwork[0].neurons.Count; i++)
                Entity.neuralNetwork[0].neurons[i].entries = Options[queue].ToList();
            return Entity;
        }

        public static Network calculateNetworkOutput(Network Entity)
        {
            for (int i = 0; i < Entity.neuralNetwork[0].neurons.Count; i++)
                Entity.neuralNetwork[0].neurons[i].calculateOutput();


            for (int i = 1; i < Entity.neuralNetwork.Count; i++)
            {
                List<double> newEntries = new List<double>();

                for (int j = 0; j < Entity.neuralNetwork[i - 1].neurons.Count; j++)
                    newEntries.Add(Entity.neuralNetwork[i - 1].neurons[j].output);

                for (int j = 0; j < Entity.neuralNetwork[i].neurons.Count; j++)
                {
                    Entity.neuralNetwork[i].neurons[j].entries = newEntries;
                    Entity.neuralNetwork[i].neurons[j].calculateOutput();
                }
            }

            return Entity;
        }

        public static Network randomStringToRealValue(Network Entity, List<Param> parameters)
        {
            for (int i = 0; i < Entity.neuralNetwork.Count; i++)
            {
                for (int j = 0; j < Entity.neuralNetwork[i].neurons.Count; j++)
                {
                    Entity.neuralNetwork[i].neurons[j].weights.Clear();
                    for (int k = 0; k < Entity.neuralNetwork[i].neurons[j].binaryStrings.Count; k++)
                        Entity.neuralNetwork[i].neurons[j].weights.Add(parameters[i].paramsTable[string.Join("", Entity.neuralNetwork[i].neurons[j].binaryStrings[k].ToArray())]);
                }
            }
            return Entity;
        }

        public static List<List<int>> generateRandomBinaryString(int weights, int bits)
        {
            List<List<int>> randomBinaryStrings = new List<List<int>>();
            for (int i = 0; i < weights; i++)
            {
                List<int> generatingBinarystrings = createRandomBinaryString(bits);
                randomBinaryStrings.Add(generatingBinarystrings);
            }
            return randomBinaryStrings;
        }

        public static List<int> createRandomBinaryString(int howManyBits)
        {
            List<int> generatedBirnaryString = new List<int>();
            for (int i = 0; i < howManyBits; i++)
                generatedBirnaryString.Add(rand.Next(0, 2));
            return generatedBirnaryString;
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

        public static int[] generateWeight(int[] networkSchema)
        {
            int[] weights = new int[networkSchema.Length];
            weights[0] = networkSchema[0] + 1;
            for (int i = 1; i < networkSchema.Length; i++)
            {
                weights[i] = networkSchema[i - 1] + 1;
            }
            return weights;
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

        public static Network calculateAdaptationValue(Network Entity, List<double[]> Options, List<double[]> Expected)
        {
            List<double> outputValues = new List<double>();
            double AdaptationValue = 0;
            int k = 0;

            for (int i = 0; i < Options.Count; i++)
            {
                Entity = addEntriesValues(Entity, Options, i);
                Entity = calculateNetworkOutput(Entity);
                for (int j = 0; j < Entity.neuralNetwork[Entity.neuralNetwork.Count - 1].neurons.Count; j++)
                    outputValues.Add(Entity.neuralNetwork[Entity.neuralNetwork.Count - 1].neurons[j].output);
            }

            for (int i = 0; i < Expected.Count; i++)
                for (int j = 0; j < Expected[i].Length; j++)
                {
                    AdaptationValue += (Expected[i][j] - outputValues[k]) * (Expected[i][j] - outputValues[k]);
                    k++;
                }

            Entity.adaptationValue = AdaptationValue;
            return Entity;
        }

        public static List<Network> createPopulation(int howManyPopulation, int[] networkSchema, double bias, double beta, int[] howManyWeightsPerLayer, int[] howManyBitsPerLayer, List<Param> parameters, List<double[]> Options, List<double[]> Expected)
        {
            List<Network> Population = new List<Network>();
            for (int i = 0; i < howManyPopulation; i++)
            {
                Network Entity = new Network();
                Entity = createLayers(Entity, networkSchema.Length);
                Entity = createNeurons(Entity, networkSchema, bias, beta);
                Entity = createRandomBinaryStrings(Entity, howManyWeightsPerLayer, howManyBitsPerLayer);
                Entity = randomStringToRealValue(Entity, parameters);
                Entity = addEntriesValues(Entity, Options, 0);
                Entity = calculateNetworkOutput(Entity);
                Entity = calculateAdaptationValue(Entity, Options, Expected);
                Population.Add(Entity);
            }
            return Population;
        }

        public static Network hotDeck(List<Network> Population)
        {
            Network Winner = Population[0];
            for (int i = 1; i < Population.Count; i++)
            {
                if (Winner.adaptationValue > Population[i].adaptationValue)
                    Winner = Population[i];
            }
            return cloneEntity(Winner);
        }

        public static List<double> copyList(List<double> listToCopy)
        {
            List<double> copiedList = new List<double>();
            for (int i = 0; i < listToCopy.Count; i++)
                copiedList.Add(listToCopy[i]);
            return copiedList;
        }

        public static List<List<int>> copyBinaryStrings(List<List<int>> binaryStrings)
        {
            List<List<int>> copiedBinaryStrings = new List<List<int>>();
            for (int i = 0; i < binaryStrings.Count; i++)
            {
                List<int> binaryString = new List<int>();
                for (int j = 0; j < binaryStrings[i].Count; j++)
                    binaryString.Add(binaryStrings[i][j]);
                copiedBinaryStrings.Add(binaryString);
            }
            return copiedBinaryStrings;
        }

        public static Network cloneEntity(Network Entity)
        {
            Network Clone = new Network();
            Clone.adaptationValue = Entity.adaptationValue;
            for (int i = 0; i < Entity.neuralNetwork.Count; i++)
            {
                Layer newLayer = new Layer();
                for (int j = 0; j < Entity.neuralNetwork[i].neurons.Count; j++)
                {
                    Neuron newNeuron = new Neuron(Entity.neuralNetwork[i].neurons[j].bias, Entity.neuralNetwork[i].neurons[j].beta);
                    newNeuron.output = Entity.neuralNetwork[i].neurons[j].output;
                    newNeuron.s = Entity.neuralNetwork[i].neurons[j].s;
                    newNeuron.weights = copyList(Entity.neuralNetwork[i].neurons[j].weights);
                    newNeuron.entries = copyList(Entity.neuralNetwork[i].neurons[j].entries);
                    newNeuron.binaryStrings = copyBinaryStrings(Entity.neuralNetwork[i].neurons[j].binaryStrings);
                    newLayer.neurons.Add(newNeuron);
                }
                Clone.neuralNetwork.Add(newLayer);
            }
            return Clone;
        }

        public static Network mutateEntity(Network Entity)
        {
            int randLayer = rand.Next(Entity.neuralNetwork.Count);
            int randNeuron = rand.Next(Entity.neuralNetwork[randLayer].neurons.Count);
            int randX = rand.Next(Entity.neuralNetwork[randLayer].neurons[randNeuron].binaryStrings.Count);
            int randY = rand.Next(Entity.neuralNetwork[randLayer].neurons[randNeuron].binaryStrings[0].Count);
            Entity.neuralNetwork[randLayer].neurons[randNeuron].binaryStrings[randX][randY] = 1 - Entity.neuralNetwork[randLayer].neurons[randNeuron].binaryStrings[randX][randY];
            return Entity;
        }

        public List<Network> geneticAlgorithm(List<Network> Population, int tournamentSize, List<Param> parameters, List<double[]> Options, List<double[]> Expected, double Epsilon)
        {
            maxLimit = 0;
            while (hotDeck(Population).adaptationValue > Epsilon && maxLimit < 130)
            {
                maxLimit++;
                List<Network> newPopulation = new List<Network>();

                for (int i = 0; i < Population.Count - 1; i++)
                    newPopulation.Add(tournamentEntity(Population, tournamentSize));

                for (int i = 0; i < newPopulation.Count; i++)
                    newPopulation[i] = mutateEntity(newPopulation[i]);

                newPopulation.Add(hotDeck(Population));

                for (int i = 0; i < newPopulation.Count; i++)
                    newPopulation[i] = calculateAdaptationValue(calculateNetworkOutput(randomStringToRealValue(newPopulation[i], parameters)), Options, Expected);

                textBox15.Text += "MIN: " + hotDeck(newPopulation).adaptationValue.ToString("F99").TrimEnd('0') + "\r\n";

                Population = newPopulation;
            }
            return Population;
        }

        public static Network tournamentEntity(List<Network> Population, int tournamentSize)
        {
            List<Network> ClonePopulation = new List<Network>(Population);
            List<Network> tournamentMembers = new List<Network>();
            for (int i = 0; i < tournamentSize; i++)
            {
                int youWereTheChosenOneAnakin = rand.Next(ClonePopulation.Count);
                tournamentMembers.Add(ClonePopulation[youWereTheChosenOneAnakin]);
                ClonePopulation.RemoveAt(youWereTheChosenOneAnakin);
            }
            return hotDeck(tournamentMembers);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Options.Add(new double[] { 0, 0 });
            Options.Add(new double[] { 0, 1 });
            Options.Add(new double[] { 1, 0 });
            Options.Add(new double[] { 1, 1 });

            Expected.Add(new double[] { 0, 1 });
            Expected.Add(new double[] { 1, 0 });
            Expected.Add(new double[] { 1, 0 });
            Expected.Add(new double[] { 0, 1 });

            printOptionsAndExpected();
        }

        //Main
        private void button1_Click(object sender, EventArgs e)
        {

            int[] networkSchema = Array.ConvertAll(textBox9.Text.Split('/'), int.Parse);
            int[] howManyBitsPerLayer = Array.ConvertAll(textBox4.Text.Split('/'), int.Parse);
            int[] howManyWeightsPerLayer = generateWeight(networkSchema);
            double bias = Convert.ToInt32(textBox7.Text);
            double beta = Convert.ToInt32(textBox8.Text);
            int howManyPopulation = Convert.ToInt32(textBox1.Text);
            double Epsilon = Convert.ToDouble(textBox6.Text);
            double max = Convert.ToInt32(textBox5.Text);
            double min = Convert.ToInt32(textBox2.Text);
            int tournamentSize = Convert.ToInt32(textBox3.Text);

            clearWindow();

            // Param setup
            List<Param> parameters = createParams(networkSchema.Length);
            parameters = addKeysAndValuesToAssocTable(parameters, howManyBitsPerLayer, max, min);

            // Population setup
            List<Network> Population = createPopulation(howManyPopulation, networkSchema, bias, beta, howManyWeightsPerLayer, howManyBitsPerLayer, parameters, Options, Expected);

            Population = geneticAlgorithm(Population, tournamentSize, parameters, Options, Expected, Epsilon);

            printEntities(Population);
            printParameters(parameters, howManyBitsPerLayer);
            printNetwork(networkSchema);
            printExpected(Expected);
            printOutput(hotDeck(Population), Options, Expected);
        }

        public void printParameters(List<Param> parameters, int[] weightPerLayer)
        {
            for (int i = 0; i < parameters.Count; i++)
            {
                textBox17.Text += "Param\r\n" + i;
                for (int j = 0; j < parameters[i].paramsTable.Count; j++)
                {
                    string keyInBits = adjustOutputStringToCorrectFormat(Convert.ToString(j, 2), weightPerLayer[i]);
                    textBox17.Text += keyInBits + " -> " + parameters[i].paramsTable[keyInBits] + "\r\n";
                }
                textBox17.Text += "\r\n";
            }

            if (maxLimit == 130)
                pictureBox1.BackColor = Color.Red;
            else
                pictureBox1.BackColor = Color.Green;
        }

        public void clearWindow()
        {
            textBox14.Text = "";
            textBox15.Text = "";
            textBox16.Text = "";
            textBox17.Text = "";
            textBox18.Text = "";
            textBox20.Text = "";
            textBox19.Text = "";
        }

        public void printEntities(List<Network> Entitiy)
        {
            for (int i = 0; i < Entitiy.Count; i++)
            {
                textBox16.Text += "\t\tOsobnik " + (i + 1) + "\r\n";
                textBox16.Text += "AdaptationValue: " + Entitiy[i].adaptationValue + "\r\n";
                for (int j = 0; j < Entitiy[i].neuralNetwork.Count; j++)
                {
                    for (int k = 0; k < Entitiy[i].neuralNetwork[j].neurons.Count; k++)
                    {
                        textBox16.Text += "\r\n\t    Warstwa: " + j + " Neuron: " + k + "\r\n";
                        for (int l = 0; l < Entitiy[i].neuralNetwork[j].neurons[k].binaryStrings.Count; l++)
                        {
                            textBox16.Text += string.Join("", Entitiy[i].neuralNetwork[j].neurons[k].binaryStrings[l].ToArray()) + "\r\n";
                        }
                        textBox16.Text += " wagi:" + string.Join("/", Entitiy[i].neuralNetwork[j].neurons[k].weights.ToArray()) + "\r\n";
                        textBox16.Text += " wejscia:" + string.Join("/", Entitiy[i].neuralNetwork[j].neurons[k].entries.ToArray()) + "\r\n";
                        textBox16.Text += " bias:" + Entitiy[i].neuralNetwork[j].neurons[k].bias + "\r\n";
                        textBox16.Text += " beta:" + Entitiy[i].neuralNetwork[j].neurons[k].beta + "\r\n";
                        textBox16.Text += " s:" + Entitiy[i].neuralNetwork[j].neurons[k].s + "\r\n";
                        textBox16.Text += " output:" + Entitiy[i].neuralNetwork[j].neurons[k].output.ToString("F5").TrimEnd('0') + "\r\n";
                    }
                }
                textBox16.Text += "\r\n";
            }
        }

        public void printExpected(List<double[]> Expected)
        {
            for (int i = 0; i < Expected.Count; i++)
                for(int j = 0; j < Expected[i].Length; j++)
                    textBox18.Text += Expected[i][j] +"\r\n";
        }

        public void printOutput(Network Entity, List<double[]> Options, List<double[]> Expected)
        {
            List<double> outputs = new List<double>();
            for (int i = 0; i < Options.Count; i++)
            {
                Entity = addEntriesValues(Entity, Options, i);
                Entity = calculateNetworkOutput(Entity);
                for (int j = 0; j < Entity.neuralNetwork[Entity.neuralNetwork.Count - 1].neurons.Count; j++)
                {
                    textBox20.Text += Entity.neuralNetwork[Entity.neuralNetwork.Count - 1].neurons[j].output.ToString("F20").TrimEnd('0') + "\r\n";
                    outputs.Add(Entity.neuralNetwork[Entity.neuralNetwork.Count - 1].neurons[j].output);
                }
            }
            int k = 0;
            for (int i = 0; i < Expected.Count; i++)
                for (int j = 0; j < Expected[i].Length; j++)
                {
                    textBox19.Text += (Expected[i][j] - outputs[k]).ToString("F20").TrimEnd('0') + "\r\n";
                    k++;
                }
        }

        public void printNetwork(int[] numerki)
        {
            textBox14.Text += "\tL0\tL1\tL2\tL3\tL4\tL5\tL6\tL7\tL8   ...";
            textBox14.Text += "\r\n";
            for (int i = 0; i < numerki.Max(); i++)
            {
                if (i < 9)
                    textBox14.Text += (i + "\t");
                else if (i < 12)
                    textBox14.Text += (".\t");
                else
                    textBox14.Text += ("\t");
                for (int j = 0; j < numerki.Length; j++)
                    if (numerki[j] > i)
                        textBox14.Text += ("N\t");
                    else
                        textBox14.Text += ("\t");
                textBox14.Text += "\r\n";
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (Options.Count < numericUpDown1.Value)
            {
                double[] doubles = Array.ConvertAll(textBox13.Text.Split('/'), double.Parse);
                Options.Add(doubles);

                doubles = Array.ConvertAll(textBox12.Text.Split('/'), double.Parse);
                Expected.Add(doubles);

                printOptionsAndExpected();
            }
            else
            {
                MessageBox.Show("Nie możesz dodać następnego, zwiększ ilość opcji!");
            }
        }

        public void printOptionsAndExpected()
        {
            textBox10.Text = "";
            textBox11.Text = "";
            for (int i = 0; i < Options.Count; i++)
            {
                for (int j = 0; j < Options[i].Length; j++)
                    if (j == Options[i].Length - 1)
                        textBox11.Text += Options[i][j];
                    else
                        textBox11.Text += Options[i][j] + "/";
                for (int j = 0; j < Expected[i].Length; j++)
                    if (j == Expected[i].Length - 1)
                        textBox10.Text += Expected[i][j];
                    else
                        textBox10.Text += Expected[i][j] + "/";
                textBox10.Text += "\r\n";
                textBox11.Text += "\r\n";
            }
        }

        private void button3_Click(object sender, EventArgs e)
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

        public class Network
        {
            public List<Layer> neuralNetwork = new List<Layer>();
            public double adaptationValue;

            public void setAdaptationValue(double adaptationValue)
            {
                this.adaptationValue = adaptationValue;
            }

        }

        public class Layer
        {
            public List<Neuron> neurons = new List<Neuron>();

        }

        public class Neuron
        {
            public List<List<int>> binaryStrings = new List<List<int>>();
            public List<double> weights = new List<double>();
            public List<double> entries = new List<double>();
            public double bias;
            public double beta;
            public double s;
            public double output;

            public Neuron(double bias, double beta)
            {
                this.bias = bias;
                this.beta = beta;
            }

            public void calculateOutput()
            {
                this.s = this.bias * this.weights[0];
                for (int i = 1; i < this.weights.Count; i++)
                    this.s += this.entries[i - 1] * this.weights[i];
                this.output = 1 / (1 + Math.Pow(Math.E, -this.beta * this.s));
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
}

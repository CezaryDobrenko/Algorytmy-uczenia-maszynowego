using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApplication2
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }


        // Funkcje pomocnicze
        //min() znajduje wartość najmniejszą z listy i zwraca tablice w której [0] to wartość a [1] to index
        static int[] min(List<int> lista)
        {
            int[] nowa = new int[2];
            int minimum = lista[0];
            int index = 0;
            for (int i = 1; i < lista.Count; i++)
            {
                if (minimum > lista[i])
                {
                    minimum = lista[i];
                    index = i;
                }
            }
            nowa[0] = minimum;
            nowa[1] = index;
            return nowa;
        }
        //max() znajduje wartość największą z listy i zwraca tablice w której [0] to wartość a [1] to index
        static int[] max(List<int> lista)
        {
            int[] nowa = new int[2];
            int maximum = lista[0];
            int index = 0;
            for (int i = 1; i < lista.Count; i++)
            {
                if (maximum < lista[i])
                {
                    maximum = lista[i];
                    index = i;
                }
            }
            nowa[0] = maximum;
            nowa[1] = index;
            return nowa;
        }

        // Zamienia string z argumentu na tablice intów
        public int[] stringToIntArr(string Skids)
        {
            int[] array = Array.ConvertAll(Skids.Split(','), int.Parse);
            return array;
        }
        // Ustala rezultat gry i dodaje wartość do inputów
        public void finalscore(int score, string rola)
        {
            if (score == 1)
            {
                if (rola == "p")
                {
                    textBox2.Text = "Protagonista wygrywa!";
                    textBox7.Text = "Antagonista przegrywa!";
                } 
                else
                {
                    textBox2.Text = "Antagonista wygrywa!";
                    textBox7.Text = "Protagonista przegrywa!";
                }
            }
            if (score == -1)
            {
                if (rola == "p")
                {
                    textBox2.Text = "Antagonista wygrywa!";
                    textBox7.Text = "Protagonista przegrywa!";
                }
                else
                {
                    textBox2.Text = "Protagonista wygrywa!";
                    textBox7.Text = "Antagonista przegrywa!";
                }
            }
        }

        // koniec funkcji pomocniczych

        //Algorytm minmax
        static int minMax(Node rodzic)
        {
            int wartosc = 0;
            List<int> lista = new List<int>();

            if (rodzic.dzieci.Count == 0)
                return rodzic.wartosc;

            for (int i = 0; i < rodzic.dzieci.Count; i++)
            {
                int val = minMax(rodzic.dzieci[i]);
                lista.Add(val);
                if (rodzic.dzieci.Count - 1 == i)
                {
                    if (rodzic.rola == "p")
                    {
                        wartosc = max(lista)[0];
                        rodzic.dzieci[max(lista)[1]].color = "red";
                    }

                    if (rodzic.rola == "a")
                    {
                        wartosc = min(lista)[0];
                        rodzic.dzieci[min(lista)[1]].color = "red";
                    }
                }
            }
            return wartosc;
        }

        // generowanie grafu przy pierwszym odpaleniu na predefiniowanych parametrach
        public void Form1_Load(object sender, EventArgs e)
        {
            string Role = "p";
            int Value = 0;
            int[] KidsValues = { 4, 5, 6 };
            int LoseCondition = 21;
            Node root = new Node(Value, Role);
            Tree drzewo = new Tree(Role, Value, LoseCondition, KidsValues);
            TreeNode treeNode = new TreeNode("" + root.wartosc);
            treeNode.ForeColor = Color.Red;
            treeNode.Expand();
            drzewo.generujDrzewo(root, KidsValues, Value, Role, LoseCondition, Role);
            int score = minMax(root);
            drzewo.generujTreeView(root, treeNode);
            treeView1.Nodes.Add(treeNode);
            finalscore(score, Role);
        }

        // generowanie grafu na podstawie parametrów z forms
        private void button1_Click(object sender, EventArgs e)
        {
            treeView1.Nodes.Clear();

            string Role = textBox3.Text;
            int Value = int.Parse(textBox4.Text);
            int LoseCondition = int.Parse(textBox6.Text);
            int[] KidsValues = stringToIntArr(textBox5.Text);

            Node root = new Node(Value, Role);
            Node rootOptymalne = new Node(Value, Role);

            Tree drzewo = new Tree(Role, Value, LoseCondition, KidsValues);

            drzewo.generujDrzewo(root, KidsValues, Value, Role, LoseCondition, Role);

            TreeNode treeNode = new TreeNode("" + root.wartosc);
            treeNode.ForeColor = Color.Red;
            treeNode.Expand();

            int score = minMax(root);

            if (textBox8.Text == "1")
                drzewo.generujTreeView(root, treeNode);
            else if (textBox8.Text == "2"){
                drzewo.generujDrzewo(rootOptymalne, KidsValues, Value, Role, LoseCondition, Role);
                drzewo.mainpath(root, rootOptymalne);
                drzewo.generujTreeView(rootOptymalne, treeNode);
            }
            else
                drzewo.generujTreeView(root, treeNode);

            treeView1.Nodes.Add(treeNode);
            finalscore(score, Role);
        }

    }

    public class Tree : Form
    {
        public string Role;
        public int Value;
        public int LoseCondition;
        public int[] KidsValues;

        public Tree(string Role, int Value, int LoseCondition, int[] KidsValues)
        {
            this.Role = Role;
            this.Value = Value;
            this.LoseCondition = LoseCondition;
            this.KidsValues = KidsValues;
        }

        public void generujDrzewo(Node rodzic, int[] KidsVals, int value, string role, int lose, string rola)
        {
            string newRola = nowaRola(role);

            for (int i = 0; i < KidsVals.Length; i++)
            {
                int wartosc = rodzic.wartosc + KidsVals[i];
                if (rodzic.wartosc > lose)
                {
                    if (rola == "p")
                    {
                        if (rodzic.rola == "p")
                            rodzic.wartosc = 1;
                        if (rodzic.rola == "a")
                            rodzic.wartosc = -1;
                    }
                    if (rola == "a")
                    {
                        if (rodzic.rola == "a")
                            rodzic.wartosc = 1;
                        if (rodzic.rola == "p")
                            rodzic.wartosc = -1;
                    }
                    return;
                }
                Node nowy = new Node(wartosc, newRola);
                rodzic.dodajdziecko(nowy);
            }

            for (int i = 0; i < KidsVals.Length; i++)
                generujDrzewo(rodzic.dzieci[i], KidsVals, value, newRola, lose, rola);
        }

        public string nowaRola(string role)
        {
            string newRola;
            if (role == "p")
                newRola = "a";
            else
                newRola = "p";
            return newRola;
        }

        public void generujTreeView(Node root, TreeNode rootTreeView)
        {
            for (int i = 0; i < root.dzieci.Count; i++)
            {
                TreeNode nowy = new TreeNode("" + root.dzieci[i].wartosc);
                if (root.dzieci[i].color == "red")
                    nowy.ForeColor = Color.Red;
                nowy.Expand();
                rootTreeView.Nodes.Add(nowy);
                generujTreeView(root.dzieci[i], nowy);
            }

        }

        public void mainpath(Node root, Node newRoot)
        {
            for (int i = 0; i < root.dzieci.Count; i++)
            {
                if (root.dzieci[i].color == "red")
                {
                    newRoot.dzieci[i].color = "red";
                    mainpath(root.dzieci[i], newRoot.dzieci[i]);
                }
            }
        }
    }

    public class Node : Form
    {
        public int wartosc;
        public string rola;
        public string color;
        public List<Node> dzieci = new List<Node>();

        public Node(int wartosc, string rola)
        {
            this.wartosc = wartosc;
            this.rola = rola;
            this.color = "black";
        }

        public void dodajdziecko(Node nowy)
        {
            dzieci.Add(nowy);
        }

    }
}

using System;
using ASD.Graphs;
using System.Collections.Generic;
using System.Linq;

namespace ASD
{

    public class IsGraphicTestCase : TestCase
    {
        private bool expected_result;
        bool result;
        int[] deg_list;

        public IsGraphicTestCase(double timeLimit, Exception expectedException, string description, int[] deg_list, bool expected_result)
            : base(timeLimit, expectedException, description)
        {
            this.expected_result = expected_result;
            this.deg_list = deg_list;
        }

        protected override void PerformTestCase(object prototypeObject)
        {
            result = ((Lab03)prototypeObject).IsGraphic(deg_list);
        }

        protected override (Result resultCode, string message) VerifyTestCase(object settings)
        {
            if (expected_result != result)
                return (Result.WrongResult, $"Wrong result: {result} (expected: {expected_result})");

            return (Result.Success, $"OK, sequence {(result?"is":"isn't")} graphic");
        }
    }
    public class ConstructGraph : TestCase
    {
        int[] sequence;
        private Graph result_graph;
        private bool isGraphic;

        public ConstructGraph(double timeLimit, Exception expectedException, string description, int[] sequence, bool isGraphic)
            : base(timeLimit, expectedException, description)
        {
            this.sequence = sequence;
            this.isGraphic = isGraphic;
        }

        protected override void PerformTestCase(object prototypeObject)
        {
            result_graph = ((Lab03)prototypeObject).ConstructGraph(sequence);
        }

        protected override (Result resultCode, string message) VerifyTestCase(object settings)
        {
            if (result_graph == null)
                return !isGraphic ? (Result.Success, "OK, sequence isn't graphic") : (Result.WrongResult, $"null returned") ;
            if (result_graph.Directed)
                return (Result.WrongResult, $"Returned graph is directed");
            if (!TestConstructedGraph())
               return (Result.WrongResult, $"Wrong result graph");

            return (Result.Success, "OK");
        }

        private bool TestConstructedGraph()
        {
            //sprawdzam czy zwrocony graf ma tyle wierzcholkow ile powinien
            if (result_graph.VerticesCount != sequence.Length)
                return false;

            //wyciagam wszystkie stopnie wierzcholkow do listy, zeby porownac z zadanym ciagiem
            List<int> result_list = new List<int>();
            for (int i = 0; i < result_graph.VerticesCount; ++i)
                result_list.Add(result_graph.InDegree(i));
            List<int> list = new List<int>(sequence);

            list.Sort();
            result_list.Sort();
            if (!result_list.SequenceEqual(list))
                return false;
            return true;
        }
    }
    public class MinimumSpanningTreeTestCase : TestCase
    {
        private Graph input_graph;
        private Graph graph;
        private double expected_result;
        double result;
        private Graph result_graph;

        public MinimumSpanningTreeTestCase(double timeLimit, Exception expectedException, string description, Graph graph, double expected_result)
            : base(timeLimit, expectedException, description)
        {
            this.graph = graph;
            this.expected_result = expected_result;
            input_graph = graph.Clone(); //zapamietuje graf wejsciowy, zeby potem sprawzic czy algorytm go nie zmienia
        }

        protected override void PerformTestCase(object prototypeObject)
        {
            result_graph = ((Lab03)prototypeObject).MinimumSpanningTree(graph, out result);
        }

        protected override (Result resultCode, string message) VerifyTestCase(object settings)
        {
            if (!input_graph.IsEqual(graph))
                return (Result.WrongResult, $"Changed input graph");
            if (result_graph == null)
                return (Result.WrongResult, $"null returned");
            if (result_graph.Directed)
                return (Result.WrongResult, $"returned graph is directed");
            if (expected_result != result)
                return (Result.WrongResult, $"Incorrect weight, result: {result} (expected: {expected_result})");
            if (!TestSpanningTree())
                return (Result.WrongResult, $"Result is not a spanning tree");

            return (Result.Success, "OK");
        }

        private bool TestSpanningTree()
        {
            //sprawdzam liczbe wierzcholkow
            if (graph.VerticesCount != result_graph.VerticesCount)
                return false;

            double sum = 0;
            //sprawdzam czy wagi nie zostaly zmienione
            //oraz czy suma wag w zwroconym drzewie rowna sie zadeklarowanej
            for (int v = 0; v < result_graph.VerticesCount; ++v)
                foreach (Edge e in result_graph.OutEdges(v))
                {
                    if (e.Weight != graph.GetEdgeWeight(e.From, e.To))
                        return false;
                    if (e.From < e.To)
                        sum += e.Weight;
                }

            if (sum != result)
                return false;

            //sprawdzam czy graf i las rozpinajacy maja tyle samo skladowych
            int cc1, cc2;
            graph.GeneralSearchAll<EdgesStack>(null, null, null, out cc1);
            result_graph.GeneralSearchAll<EdgesStack>(null, null, null, out cc2);
            if (cc1 != cc2)
                return false;

            //sprawdzam czy to co zwrocil algorytm rzeczywiscie jest drzewem/lasem
            //jezeli liczba skladowych i wierzcholkow jest poprawna to moge sprawdzic tylko liczbe krawedzi
            if(result_graph.EdgesCount != result_graph.VerticesCount - cc2)
                return false;

            return true;
        }
    }

    class Lab03TestModule : TestModule
    {

        public override void PrepareTestSets()
        {
            var rgg = new RandomGraphGenerator();
            int n1 = 12;
            int n2 = 9;
            int n2t = 7;
            int[][] sequences = new int[n1][];
            bool[] is_graphic = new bool[n1];
            string[] descriptions1 = new string[n1];

            int n3 = 13;
            Graph[] graphs = new Graph[n3];
            string[] descriptions3 = new string[n3];
            double[] expected_weight = new double[n3];
            double[] limits = Enumerable.Repeat(1.0,n3).ToArray();
            limits[7] = 2;     // cykl
            limits[10] = 2;    // niech bedzie z zapasem
            limits[12] = 300;  // duzy rzadki graf

            //Testy ciagu grafowego
            sequences[0] = new int[] { 0 };
            is_graphic[0] = true;
            descriptions1[0] = "{ 0 }"; 

            sequences[1] = new int[] { 1, 1, 1, 1 };
            is_graphic[1] = true;
            descriptions1[1] = "{ 1, 1, 1, 1 }";

            sequences[2] = new int[] { 5, 5, 5, 5, 5, 5 };
            is_graphic[2] = true;
            descriptions1[2] = "{ 5, 5, 5, 5, 5, 5 }";

            sequences[3] = new int[] { 1, 3, 5, 2, 3, 3, 3, 1, 3, 5, 2, 3, 3, 3 };
            is_graphic[3] = true;
            descriptions1[3] = "{ 1, 3, 5, 2, 3, 3, 3, 1, 3, 5, 2, 3, 3, 3 }";

            sequences[4] = new int[] { 5, 5, 5, 0, 5, 5, 5 };
            is_graphic[4] = true;
            descriptions1[4] = "{ 5, 5, 5, 0, 5, 5, 5 }";

            sequences[5] = new int[] { 4, 4, 3, 3, 2 };
            is_graphic[5] = true;
            descriptions1[5] = "{ 4, 4, 3, 3, 2 }";

            sequences[6] = new int[] { 2, 3, 2, 3, 2 };
            is_graphic[6] = true;
            descriptions1[6] = "{ 2, 3, 2, 3, 2 }";

            sequences[7] = new int[] { 1, 1, 1, 1, 3 };
            is_graphic[7] = false;
            descriptions1[7] = "{ 1, 1, 1, 1, 3 }";

            sequences[8] = new int[] { 1, 1, -1, 1 };
            is_graphic[8] = false;
            descriptions1[8] = "{ 1, 1, -1, 1 }";

            sequences[9] = new int[] { 1, 1, 1, 1, 6 };
            is_graphic[9] = false;
            descriptions1[9] = "{ 1, 1, 1, 1, 6 }";

            sequences[10] = new int[] { 4, 4, 3, 2, 1 };
            is_graphic[10] = false;
            descriptions1[10] = "{ 4, 4, 3, 2, 1 }";

            sequences[11] = new int[] { 1, 3, 1, 2, 4 };
            is_graphic[11] = false;
            descriptions1[11] = "{ 1, 3, 1, 2, 4 }";



            //Testy drzewa rozpinajacego
            //przyklady z zajec

            //graf o jednym wierzcholku izolowanym
            graphs[0] = new AdjacencyMatrixGraph(false, 1);
            descriptions3[0] = "Graf o jednym wierzcholku izolowanym";
            expected_weight[0] = 0;

            //graf bez krawedzi
            graphs[1] = new AdjacencyListsGraph<SimpleAdjacencyList>(false, 10);
            descriptions3[1] = "Graf bez krawedzi";
            expected_weight[1] = 0;

            //pewien maly graf
            graphs[2] = new AdjacencyMatrixGraph(false, 5);
            graphs[2].AddEdge(0, 1, 1);
            graphs[2].AddEdge(0, 3, -1);
            graphs[2].AddEdge(1, 2, -3);
            graphs[2].AddEdge(2, 0, 2);
            graphs[2].AddEdge(2, 3, 2);
            graphs[2].AddEdge(3, 4, 10);
            descriptions3[2] = "Pewien maly graf";
            expected_weight[2] = 7;

            //sciezka P6
            graphs[3] = new AdjacencyMatrixGraph(false, 6);
            graphs[3].AddEdge(0, 1, 1);
            graphs[3].AddEdge(1, 2, 2);
            graphs[3].AddEdge(2, 3, 3);
            graphs[3].AddEdge(3, 4, 4);
            graphs[3].AddEdge(4, 5, 5);
            descriptions3[3] = "Sciezka P6";
            expected_weight[3] = 15;

            //klika K5
            rgg.SetSeed(12345);
            graphs[4] = rgg.UndirectedGraph(typeof(AdjacencyMatrixGraph), 5, 1.0);
            descriptions3[4] = "Klika K5";
            expected_weight[4] = 4;

            //cykl C5
            graphs[5] = new AdjacencyMatrixGraph(false, 5);
            graphs[5].AddEdge(0, 1, 1.5);
            graphs[5].AddEdge(1, 2, -1.5);
            graphs[5].AddEdge(2, 3, 3.2);
            graphs[5].AddEdge(3, 4, 4.8);
            graphs[5].AddEdge(4, 0, 2.3);
            descriptions3[5] = "Cykl C5";
            expected_weight[5] = 5.5;

            //cykl
            int g6vc = 500;
            graphs[6] = new AdjacencyListsGraph<SimpleAdjacencyList>(false, g6vc);
            for (int i = 1; i < g6vc; ++i)
                graphs[6].AddEdge(i - 1, i);
            graphs[6].AddEdge(g6vc - 1, 0);
            descriptions3[6] = "Cykl";
            expected_weight[6] = 499;

            //graf niespojny
            graphs[7] = new AdjacencyMatrixGraph(false, 12);
            graphs[7].AddEdge(0, 3, 3);
            graphs[7].AddEdge(0, 7, 2);
            graphs[7].AddEdge(3, 8, 3);
            graphs[7].AddEdge(7, 11, 2);
            graphs[7].AddEdge(11, 8, 4);
            graphs[7].AddEdge(7, 8, 5);
            graphs[7].AddEdge(1, 2, 1);
            graphs[7].AddEdge(2, 10, 5);
            graphs[7].AddEdge(1, 10, 3);
            graphs[7].AddEdge(2, 9, 4);
            graphs[7].AddEdge(4, 5, 1);
            graphs[7].AddEdge(5, 6, 2);
            graphs[7].AddEdge(4, 6, 3);
            descriptions3[7] = "Graf niespojny";
            expected_weight[7] = 21;

            //grafy losowe 

            int g8vc = 200;
            rgg.SetSeed(123451);
            graphs[8] = rgg.UndirectedGraph(typeof(AdjacencyMatrixGraph), g8vc, 0.1, -100, -1);
            descriptions3[8] = "Graf losowy o wagach ujemnych";
            expected_weight[8] = -18599;

            int g9vc = 200;
            rgg.SetSeed(123452);
            graphs[9] = rgg.UndirectedGraph(typeof(AdjacencyListsGraph<SimpleAdjacencyList>), g9vc, 0.05, 1, 30);
            descriptions3[9] = "Graf losowy o wagach dodatnich";
            expected_weight[9] = 867;

            int g10vc = 300;
            rgg.SetSeed(123453);
            graphs[10] = rgg.UndirectedGraph(typeof(AdjacencyListsGraph<HashTableAdjacencyList>), g10vc, 0.02, -10, 40);
            descriptions3[10] = "Graf losowy o wagach calkowitych";
            expected_weight[10] = 238;

            //graf skierowany
            graphs[11] = new AdjacencyMatrixGraph(true, 5);
            graphs[11].AddEdge(0, 1, 1);
            graphs[11].AddEdge(0, 3, -1);
            graphs[11].AddEdge(1, 2, -3);
            graphs[11].AddEdge(2, 0, 2);
            graphs[11].AddEdge(2, 3, 2);
            graphs[11].AddEdge(3, 4, 10);
            descriptions3[11] = "Pewien maly graf skierowany";
            expected_weight[11] = 0;

            // graf dodatkowy
            int g12vc = 20000;
            graphs[12] = new AdjacencyListsGraph<SimpleAdjacencyList>(false, g12vc);
            for (int i = 0; i < g12vc-1; i+=4)
                {
                graphs[12].AddEdge(i,i+1);
                graphs[12].AddEdge(i+1,i+2);
                }
            descriptions3[12] = "duzy rzadki graf";
            expected_weight[12] = 10000;


            TestSets["LabIsGraphic"] = new TestSet(new Lab03(), "Część 1 - czy ciąg jest grafowy (0.5 pkt)");
            TestSets["LabConstructGraph"] = new TestSet(new Lab03(), "Część 2 - skonstruowanie grafu (1.5 pkt)");
            TestSets["LabMinimumSpanningTree"] = new TestSet(new Lab03(), "Część 3 - drzewo rozpinające, algorytm Kruskala (2 pkt)");

            for (int k = 0; k < n1; ++k)
            {
                TestSets["LabIsGraphic"].TestCases.Add(new IsGraphicTestCase(1, null, descriptions1[k], sequences[k], is_graphic[k]));
                if (k < n2)
                    TestSets["LabConstructGraph"].TestCases.Add(new ConstructGraph(1, null, descriptions1[k], sequences[k], k<n2t));
            }
            for (int k = 0; k < n3; ++k)
            {
                TestSets["LabMinimumSpanningTree"].TestCases.Add(new MinimumSpanningTreeTestCase(limits[k], graphs[k].Directed ? new ArgumentException("") : null, descriptions3[k], graphs[k], expected_weight[k]));
            }



        }

        public override double ScoreResult()
        {
            return 1;
        }

    }

    class Lab03Main
    {

            public static void Main()
            {
                Lab03TestModule tests = new Lab03TestModule();
                tests.PrepareTestSets();
                foreach (var ts in tests.TestSets)
                    ts.Value.PerformTests(false);
            }
        }
}

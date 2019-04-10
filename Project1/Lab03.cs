using System;
using System.Collections;
using System.Collections.Generic;
using ASD.Graphs;
using System.Diagnostics;

namespace ASD
{
    public class Lab03 : MarshalByRefObject
    {
        
        // Część 1
        //  Sprawdzenie czy podany ciąg stopni jest grafowy
        //  0.5 pkt

        public bool IsGraphic(int[] sequence)
        {
            List<int> l = new List<int>();
            if (sequence.Length == 1 && sequence[0] != 0) return false;

            int suma = 0;
            foreach (int a in sequence)
            {
                if (a < 0) return false;
                l.Add(a);
                suma += a;
            }
            if (suma % 2 == 1 ) return false;
            l.Sort();
       
            int count = l.Count;
            while(count>1 && l[count-1]>0)
            {
                if (count <= l[count - 1]) return false;
                for (int i=0; i<l[count - 1];i++)
                {   
                    
                    l[count-2-i]--;
                    if (l[count - 2 - i] < 0) return false;
                }
                l.RemoveAt(count-1);
                
                count--;
                l.Sort();
         
            }
            
            return true;
        }

        //Część 2
        // Konstruowanie grafu na podstawie podanego ciągu grafowego
        // 1.5 pkt
       
        public Graph ConstructGraph(int[] sequence)
        {
            Graph g = new AdjacencyListsGraph<HashTableAdjacencyList>(false, sequence.Length);
            if (sequence.Length == 1)
            {
                if (sequence[0] == 0) return g;
                else return null;
            }
            List<(int, int)> t = new List<(int, int)>();
            int sum = 0;
            for (int i = 0; i < sequence.Length; i++)
            {
                t.Add((sequence[i], i));
                sum += sequence[i];
            }
            if (sum % 2 == 1) return null;
            t.Sort();
            int count = t.Count;
            while (count > 1 && t[count - 1].Item1 > 0)
            {
                for (int i = 0; i < t[count - 1].Item1; i++)
                {
                    if (t[count - 2 - i].Item1 == 0) return null;
                    g.AddEdge(t[count - 1].Item2, t[count - 2 - i].Item2);
                    t[count - 2 - i] = (t[count - 2 - i].Item1 - 1, t[count - 2 - i].Item2);
                }

                t.RemoveAt(count - 1);
                count--;

                t.Sort();
            }
     

            return g;
        }

    //Część 3
    // Wyznaczanie minimalnego drzewa (bądź lasu) rozpinającego algorytmem Kruskala
    // 2 pkt
    public Graph MinimumSpanningTree(Graph graph, out double min_weight)
        {
            
            min_weight = 0;
            if (graph.Directed) throw new System.ArgumentException();
            int n = graph.VerticesCount;
            UnionFind u = new UnionFind(n);

            EdgesMinPriorityQueue edges = new EdgesMinPriorityQueue();
            Graph tree = graph.IsolatedVerticesGraph();

            for(int v=0; v<n; ++v)
            {
                foreach(Edge e in graph.OutEdges(v))
                {
                    if(e.To<e.From)
                    {
                        edges.Put(e);
                    }
                }
            }
            int count = edges.Count;
            for(; count>0;)
            {
                Edge e = edges.Get();
                count--;
                if (u.Find(e.From) != u.Find(e.To))
                {
                    u.Union(e.From, e.To);
                    tree.Add(e);
                    min_weight += e.Weight;
                }
            }
       
            return tree;
        }
      

    }
}

using SBC;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SimulatedBeeColony
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                int pathStart = 0, pathEnd = 0;
                EdgeList graph = LoadGraph("D:\\dump\\graph2.txt", ref pathStart, ref pathEnd);
                Console.WriteLine("Loaded graph:");
                Console.WriteLine(graph.ToString());

                int totalNumberBees = 10;
                
                int numberInactive = Convert.ToInt32(totalNumberBees * .85); ;
                int numberScout = Convert.ToInt32(totalNumberBees * .15); ;

                int maxNumberVisits = 5;
                int maxNumberCycles = 10;

                Hive hive = new Hive(numberInactive, numberScout, maxNumberVisits, maxNumberCycles, graph, pathStart, pathEnd);
                Console.WriteLine("\nInitial random hive");
                Console.WriteLine(hive);

                hive.Solve();

                Console.WriteLine("\nFinal hive");
                Console.WriteLine(hive);
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Fatal: " + ex.Message);
                Console.ReadLine();
            }
        }

        private static EdgeList LoadGraph(string filePath, ref int pathStart, ref int pathEnd)
        {
            Random rand = new Random();
            using (StreamReader sr = new StreamReader(filePath))
            {
                string line = sr.ReadLine();
                string[] parameters = line.Split(' ');
                int vCount = Int32.Parse(parameters[0]);
                pathStart = Int32.Parse(parameters[1]);
                pathEnd = Int32.Parse(parameters[2]);
                EdgeList result = new EdgeList(vCount);
                while ((line = sr.ReadLine()) != null)
                {
                    parameters = line.Split('-');
                    int from = Int32.Parse(parameters[0]);
                    int to = Int32.Parse(parameters[1]);
                    int weight = rand.Next(1, 10);
                    result.AddEdge(new Edge(from, to, weight));
                }
                return result;
            }
        }

    }
}

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
                EdgeList graph = LoadGraph("D:\\dump\\graph4bee.txt", ref pathStart, ref pathEnd);
                Console.WriteLine("Loaded graph:");
                Console.WriteLine(graph.ToString());

                int totalNumberBees = 30;
                
                int numberWorkers = Convert.ToInt32(totalNumberBees * .85); ;
                int numberScout = Convert.ToInt32(totalNumberBees * .15); ;

                int maxNumberVisits = 3;
                int maxNumberCycles = 20;
                int reportFreq = 2;

                Hive hive = new Hive(numberWorkers, numberScout, maxNumberVisits, 
                    graph, pathStart, pathEnd, maxNumberCycles, reportFreq);
                
                hive.Solve();

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
            Random rand = new Random(0);
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

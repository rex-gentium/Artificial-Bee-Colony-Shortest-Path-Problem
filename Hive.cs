using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SBC
{
    class Hive
    {
        static Random random = null; // multipurpose

        private EdgeList graph;
        private int pathStart, pathEnd;

        public int maxCycles; // за одну итерацию все агенты совершают по одному действию

        public double probPersuasion = 0.95;    // вероятность бездействующей пчелы откликнуться на танец разведчика
        public double probMistake = 0.01;       // вероятность пчелы-работника принять ошибочное решение

        public List<Bee> scouts, onlookers, employed;
        public int BeesCount { get => scouts.Count + onlookers.Count + employed.Count; }
        public Dictionary<int[], int> scoutedPaths;
        public Dictionary<int[], int> solutions;

        private int[] bestPath;
        private int bestDistance;

        public override string ToString()
        {
            string s = onlookers.Count.ToString() + " onlookers, ";
            s += employed.Count.ToString() + " employed, ";
            s += scouts.Count.ToString() + " scouts, ";
            s += BeesCount.ToString() + " total\n";
            s += "Best path found: ";
            if (bestPath != null)
            {
                for (int i = 0; i < this.bestPath.Length - 1; ++i)
                    s += bestPath[i] + "-";
                s += this.bestPath[this.bestPath.Length - 1] + "\n";
                s += "Path distance: ";
                s += bestDistance.ToString() + "\n";
            }
            else s += "none";            
            return s;
        }

        public Hive(int onlookerCount, int scoutCount, int maxNumberVisits,
          int maxNumberCycles, EdgeList graph, int pathStart, int pathEnd)
        {
            random = new Random();
            Bee.MaxUnluckyItersCount = maxNumberVisits;
            this.maxCycles = maxNumberCycles;
            this.graph = graph;
            this.pathStart = pathStart;
            this.pathEnd = pathEnd;
            this.scoutedPaths = new Dictionary<int[], int>();
            this.solutions = new Dictionary<int[], int>();

            ProduceInitialPopulation(onlookerCount, scoutCount);
        }

        private void ProduceInitialPopulation(int onlookerCount, int scoutCount)
        {
            employed = new List<Bee>(onlookerCount);
            onlookers = new List<Bee>(onlookerCount);
            for (int i = 0; i < onlookerCount; ++i)
                onlookers.Add(new Bee(Bee.Status.ONLOOKER));
            scouts = new List<Bee>(scoutCount);
            for (int i = 0; i < scoutCount; ++i)
                scouts.Add(new Bee(Bee.Status.SCOUT));
        }

        public void Solve()
        {
            int cycleCount = 0;
            while (cycleCount < maxCycles)
            {
                ScoutPhase();
                OnlookerPhase();
                EmployedPhase();
                KeepBestPath();
                ++cycleCount;
            }
        }

        private void KeepBestPath()
        {
            foreach (int[] path in solutions.Keys)
                if (bestPath == null || solutions[path] < bestDistance)
                {
                    bestDistance = solutions[path];
                    bestPath = path;
                }
        }

        private void EmployedPhase()
        {
            solutions.Clear();
            foreach (Bee bee in employed)
                ProcessEmployedBee(bee);

            employed.RemoveAll(bee => bee.CurrentStatus != Bee.Status.EMPLOYED);
        }

        private void OnlookerPhase()
        {
            int distanceSum = 0;
            foreach (int[] path in scoutedPaths.Keys)
                distanceSum += scoutedPaths[path];
            Dictionary<double, int[]> probToPath = new Dictionary<double, int[]>();
            double prevProb = 0.0;
            foreach (int[] path in scoutedPaths.Keys)
            {
                double prob = scoutedPaths[path] / (double)distanceSum;
                probToPath.Add(prevProb + prob, path);
                prevProb += prob;
            }
            double[] probRange = probToPath.Keys
                .ToList()
                .Concat(new List<double>{0.0})
                .ToArray();
            Array.Sort(probRange);

            foreach (Bee bee in onlookers)
                ProcessOnlookerBee(bee, probToPath, probRange);

            onlookers.RemoveAll(bee => bee.CurrentStatus != Bee.Status.ONLOOKER);
        }

        private void ScoutPhase()
        {
            scoutedPaths.Clear();
            foreach (Bee bee in scouts) {
                ProcessScoutBee(bee);
            }
            scouts.RemoveAll(bee => bee.CurrentStatus != Bee.Status.SCOUT);
        }

        private void ProcessOnlookerBee(Bee bee, Dictionary<double, int[]> probToPath, double[] probRange)
        {
            bool isPersuaded = random.NextDouble() < probPersuasion;
            if (isPersuaded)
            {
                int[] path = null;
                double probability = random.NextDouble();
                for (int i = 0; i < probRange.Length - 1; ++i)
                    if (probability >= probRange[i] && probability < probRange[i + 1])
                        path = probToPath[probRange[i + 1]];
                if (path == null)
                    path = probToPath.Values.First();
                bee.ChangePath(path, scoutedPaths[path]);
                bee.CurrentStatus = Bee.Status.EMPLOYED;
                employed.Add(bee);
            }
        }

        private void ProcessEmployedBee(Bee bee)
        {
            int[] neighborSolution = graph.ModifyRandomPath(bee.CurrentPath);
            int neighborDistance = graph.MeasureDistance(neighborSolution);

            bool isMistaken = random.NextDouble() < probMistake;
            bool foundNewSolution = neighborDistance < bee.CurrentPathDistance;

            if (foundNewSolution ^ isMistaken) // XOR 
                bee.ChangePath(neighborSolution, neighborDistance);
            else
                bee.StayIdle();            
            
            if (bee.IsUnluckyOverLimit())
            {   // пчела-неудачник прекращает попытки улучшить путь
                bee.CurrentStatus = Bee.Status.ONLOOKER;
                bee.UnluckyIterCount = 0;
                onlookers.Add(bee);
            }
            solutions.Add(bee.CurrentPath, bee.CurrentPathDistance);
        }

        private void ProcessScoutBee(Bee bee)
        {
            int[] randomSolution = graph.RandomPath(pathStart, pathEnd);
            int solutionDistance = graph.MeasureDistance(randomSolution);
            bee.ChangePath(randomSolution, solutionDistance);
            // пчела танцует к улью о том, какой путь нашла
            DoWaggleDance(bee);
        }

        private void DoWaggleDance(Bee bee)
        {
            scoutedPaths.Add(bee.CurrentPath, bee.CurrentPathDistance);
        }

    }

}

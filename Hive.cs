using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SBC
{
    class Hive
    {
        static Random random; // multipurpose

        EdgeList graph;
        int pathStart, pathEnd;

        int maxCycles; // за одну итерацию все агенты совершают по одному действию
        int reportEvery;

        public double PersuasionProbability { get; set; }    // вероятность бездействующей пчелы откликнуться на танец разведчика
        public double MistakeProbability { get; set; }       // вероятность пчелы-работника принять ошибочное решение

        int workerCount, scoutCount;

        List<Bee> scouts, onlookers, employed;
        public int BeesCount { get => scouts.Count + onlookers.Count + employed.Count; }
        Dictionary<int[], int> scoutedPaths;    // решения, найденные разведчиками

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

        public Hive(int workerCount, int scoutCount, int maxNumberVisits,
          EdgeList graph, int pathStart, int pathEnd, int maxCycles, int reportEvery)
        {
            random = new Random();
            Bee.MaxUnluckyItersCount = maxNumberVisits;
            this.maxCycles = maxCycles;
            this.reportEvery = reportEvery;
            this.graph = graph;
            this.pathStart = pathStart;
            this.pathEnd = pathEnd;
            this.scoutedPaths = new Dictionary<int[], int>();
            PersuasionProbability = 0.95;
            MistakeProbability = 0.01;
            this.workerCount = workerCount;
            this.scoutCount = scoutCount;
        }

        private void ProduceInitialPopulation()
        {   // создаёт начальную популяцию: разведчики + ожидающие в улье рабочие
            employed = new List<Bee>(workerCount);
            onlookers = new List<Bee>(workerCount);
            for (int i = 0; i < workerCount; ++i)
                onlookers.Add(new Bee(Bee.Status.ONLOOKER));
            scouts = new List<Bee>(scoutCount);
            for (int i = 0; i < scoutCount; ++i)
                scouts.Add(new Bee(Bee.Status.SCOUT));
        }

        public void Solve()
        {
            ProduceInitialPopulation();
            int cycleCount = 0;
            while (cycleCount < maxCycles)
            {
                ScoutPhase();
                OnlookerPhase();
                EmployedPhase();
                KeepBestPath();
                if (cycleCount++ % reportEvery == 0)
                {
                    Console.WriteLine("Iteration " + cycleCount.ToString());
                    Console.WriteLine(this);
                }                
            }
        }

        private void KeepBestPath()
        {   // если рабочим удалось найти лучшее решение, запоминает его
            foreach (Bee bee in employed)
                if (bestPath == null || bee.CurrentPathDistance < bestDistance)
                {
                    bestDistance = bee.CurrentPathDistance;
                    bestPath = bee.CurrentPath;
                }
        }

        private void EmployedPhase()
        {
            foreach (Bee bee in employed)
                ProcessEmployedBee(bee);

            employed.RemoveAll(bee => bee.CurrentStatus != Bee.Status.EMPLOYED);
        }

        private void OnlookerPhase()
        {
            Dictionary<double, int[]> rollingWheel = CreateScoutedPathsRollingWheel();
            
            foreach (Bee bee in onlookers)
                ProcessOnlookerBee(bee, rollingWheel);

            onlookers.RemoveAll(bee => bee.CurrentStatus != Bee.Status.ONLOOKER);
        }

        private Dictionary<double, int[]> CreateScoutedPathsRollingWheel()
        {   // строит рулетку решений разведчиков = проецирует каждое решение в отрезок внутри [0..1]
            // длины отрезков пропорциональны добротности значениям целевой функции
            int distanceSum = 0;
            foreach (int[] path in scoutedPaths.Keys)
                distanceSum += scoutedPaths[path];
            Dictionary<double, int[]> res = new Dictionary<double, int[]>();
            double prevProb = 0.0;
            foreach (int[] path in scoutedPaths.Keys)
            {
                double prob = 1.0 - scoutedPaths[path] / (double)distanceSum;
                res.Add(prevProb + prob, path);
                prevProb += prob;
            }
            return res;
        }

        private void ScoutPhase()
        {
            scoutedPaths.Clear();
            foreach (Bee bee in scouts) {
                ProcessScoutBee(bee);
            }
            //scouts.RemoveAll(bee => bee.CurrentStatus != Bee.Status.SCOUT);
        }

        private void ProcessOnlookerBee(Bee bee, Dictionary<double, int[]> rollingWheel)
        {
            bool isPersuaded = random.NextDouble() < PersuasionProbability;
            if (isPersuaded)
            {
                int[] path = GetPathFromWheel(random.NextDouble(), rollingWheel);
                bee.ChangePath(path, scoutedPaths[path]);
                bee.CurrentStatus = Bee.Status.EMPLOYED;
                employed.Add(bee);
            }
        }

        private int[] GetPathFromWheel(double randomDouble, Dictionary<double, int[]> rollingWheel)
        {   // вычисляет попадание точки в отрезок на рулетке и получает оттуда соответствующее решение
            int[] res = null;
            double[] wheelRange = new List<double> { 0.0 }
                .Concat(rollingWheel.Keys)
                .ToArray();
            Array.Sort(wheelRange);
            for (int i = 0; i < wheelRange.Length - 1; ++i)
                if (randomDouble >= wheelRange[i]
                    && randomDouble < wheelRange[i + 1])
                    res = rollingWheel[wheelRange[i + 1]];
            if (res == null)
                res = rollingWheel.Values.First();
            return res;
        }

        private void ProcessEmployedBee(Bee bee)
        {
            int[] neighborSolution = graph.ModifyRandomPath(bee.CurrentPath);
            int neighborDistance = graph.MeasureDistance(neighborSolution);

            bool isMistaken = random.NextDouble() < MistakeProbability;
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
        }

        private void ProcessScoutBee(Bee bee)
        {
            if (onlookers.Count == 0)
            {   // если в улье нет свободных рабочих, нет смысла искать решение
                return;
            }
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

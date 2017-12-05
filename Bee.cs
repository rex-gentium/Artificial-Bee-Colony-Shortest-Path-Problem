using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SBC
{
    public class Bee
    {
        public static int MaxUnluckyItersCount = 10;
        public enum Status { ONLOOKER, EMPLOYED, SCOUT }
        public Status CurrentStatus { set; get; }
        private int[] currentPath;
        private int currentPathDistance;
        public int[] CurrentPath
        {
            get
            {
                int[] res = new int[currentPath.Length];
                Array.Copy(currentPath, res, currentPath.Length);
                return res;
            }
        }
        public int CurrentPathDistance { get => currentPathDistance; }
        public int UnluckyIterCount { set; get; }

        public Bee(Status status)
        {
            this.CurrentStatus = status;
            this.currentPath = null;
            this.currentPathDistance = 0;
            UnluckyIterCount = 0;
        }

        public Bee(Status status, int[] currentPath, int currentPathDistance)
        {
            this.CurrentStatus = status;
            this.currentPath = new int[currentPath.Length];
            Array.Copy(currentPath, this.currentPath, currentPath.Length);
            this.currentPathDistance = currentPathDistance;
            UnluckyIterCount = 0;
        }

        public void ChangePath(int[] path, int pathDistance)
        {
            if (currentPath == null || currentPath.Length != path.Length)
                currentPath = new int[path.Length];
            Array.Copy(path, currentPath, path.Length);
            currentPathDistance = pathDistance;
            UnluckyIterCount = 0;
        }

        public bool IsUnluckyOverLimit()
        {
            return UnluckyIterCount > MaxUnluckyItersCount;
        }

        public void StayIdle()
        {
            ++UnluckyIterCount;
        }

        public override string ToString()
        {
            string s = "";
            s += "Status = " + CurrentStatus.ToString() + "\n";
            s += " Memory = ";
            for (int i = 0; i < currentPath.Length - 1; ++i)
                s += currentPath[i] + "-";
            s += currentPath[currentPath.Length - 1] + "\n";
            s += " Distance = " + currentPathDistance.ToString();
            //s += " Number visits = " + numberOfVisits;
            return s;
        }
    }
}

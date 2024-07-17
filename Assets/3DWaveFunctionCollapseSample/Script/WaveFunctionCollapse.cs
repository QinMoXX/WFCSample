using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _3DWaveFunctionCollapseSample.Script
{
    public class WaveFunctionCollapse
    {
        Vector3Int size;
        private List<int>[] wave;
        private int collapsedCount;
        private Prototype[] allPrototypes;

        private int[] dx = new int[6] { 1,-1,0,0,0,0};
        private int[] dy = new int[6] { 0,0,1,-1,0,0};
        private int[] dz = new int[6] { 0,0,0,0,1,-1};

        private int MX;
        private int MY;
        private int MZ;
        private int MXY;
        
        
        /// <summary>
        /// 初始化波函数
        /// </summary>
        public void Initialize(Vector3Int size,in List<Prototype> allPrototypes)
        {
            this.size = size;
            this.allPrototypes = this.allPrototypes.ToArray();
            collapsedCount = size.z * size.y * size.x;
            MX = size.x;
            MY = size.y;
            MZ = size.z;
            MXY = size.x * size.y;
            wave = new List<int>[collapsedCount];
            int[] prototypeIndexes = Enumerable.Range(0, allPrototypes.Count).ToArray();

            for (int i = 0; i < wave.Length; i++)
            {
                wave[i] = prototypeIndexes.ToList();
            }
        }

        public bool IsCollapsed()
        {
            return collapsedCount <= 0;
        }

        public bool Iterate()
        {
            int coord = GetEntropyCoord();
            CollapseTo(coord);
            if (!Propagate(coord))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 传递
        /// </summary>
        /// <param name="coord"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private bool Propagate(int coord)
        {
            Stack<int> stack = new Stack<int>();
            stack.Append(coord);
            while (stack.Count > 0)
            {
                int curCoord = stack.Pop();

                int z = curCoord / MXY;
                int xy = curCoord % MXY;
                int y = xy / MX;
                int x = xy % MX;
                for (int d = 0; d < 6; d++)
                {
                    int x2 = x + dx[d];
                    int y2 = y + dy[d];
                    int z2 = z + dz[d];
                    if (x2 <0 || x2 >= MX || y2 < 0 || y2>= MY || z2 < 0 || z2 >= MZ)
                    {
                        continue;
                    }
                    int otherCoord = x2 + y2 * MX + z2 * MXY;
                    var otherPossiblePrototypes = wave[otherCoord];
                    var possibleNeighbours = GetPossibleNeighbours(curCoord, d);
                    if (otherPossiblePrototypes.Count == 0)
                    {
                        return false;
                    }

                    for (int i = otherPossiblePrototypes.Count; i >=0 ; i--)
                    {
                        if (!possibleNeighbours.Contains(otherPossiblePrototypes[i]))
                        {
                            otherPossiblePrototypes.RemoveAt(i);
                            if (!stack.Contains(otherCoord))
                            {
                                stack.Append(otherCoord);
                            }
                        }
                    }
                }
            }

            return true;
        }

        private HashSet<int> GetPossibleNeighbours(int curCoord, int d)
        {
            var possiblePrototypes = wave[curCoord];
            HashSet<int> possibleNeighbours = new HashSet<int>();
            Prototype prototype = allPrototypes[curCoord];
            for (int i = 0; i < possiblePrototypes.Count; i++)
            {
                List<int> vaildPrototypes = new List<int>();
                if (d == 0)
                {
                    vaildPrototypes = prototype.posX;
                }
                else if (d == 1)
                {
                    vaildPrototypes = prototype.negX;
                }
                else if (d == 2)
                {
                    vaildPrototypes = prototype.posY;
                }
                else if (d == 3)
                {
                    vaildPrototypes = prototype.negY;
                }
                else if (d == 4)
                {
                    vaildPrototypes = prototype.posZ;
                }
                else if (d == 5)
                {
                    vaildPrototypes = prototype.negZ;
                }

                for (int j = 0; j < vaildPrototypes.Count; j++)
                {
                    possibleNeighbours.Add(vaildPrototypes[j]);
                }
            }

            return possibleNeighbours;
        }


        /// <summary>
        /// 坍缩唯一解
        /// </summary>
        /// <param name="coord">坍缩网格快</param>
        public void CollapseTo(int coord)
        {
            List<int> possiblePrototypes = wave[coord];
            int range = Random.Range(0, possiblePrototypes.Count);
            int prototypeIndex = possiblePrototypes[range];
            possiblePrototypes.Clear();
            possiblePrototypes.Add(prototypeIndex);
            collapsedCount--;
        }
        

        /// <summary>
        /// 获取最小熵下标
        /// </summary>
        /// <returns></returns>
        private int GetEntropyCoord()
        {
            int minEntropy = int.MaxValue;
            int minIndex = int.MinValue;
            for (int i = 0; i < wave.Length; i++)
            {
                if (wave[i].Count > 1 & wave[i].Count < minEntropy)
                {
                    minEntropy = wave[i].Count;
                    minIndex = i;
                }           
            }
            return minIndex;
        }

        
        
        public bool Run()
        {
            while (!IsCollapsed())
            {
                if (!Iterate())
                {
                    return false;
                }
            }

            return true;
        }
    }
}
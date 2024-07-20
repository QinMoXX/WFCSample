using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class WaveFunctionCollapse:MonoBehaviour
{
    [Header("runtime display")]
    public bool RunTime = true;
    [SerializeField]
    private Vector3Int size;
    private List<int>[] wave;
    private int collapsedCount;
    private  Prototype[] allPrototypes;

    private int[] dx = new int[6] { 1,-1,0,0,0,0};
    private int[] dy = new int[6] { 0,0,1,-1,0,0};
    private int[] dz = new int[6] { 0,0,0,0,1,-1};

    private int MX;
    private int MY;
    private int MZ;
    private int MXY;
    
    private int[] weightCache; //权重列表缓存
    
    public Vector3Int Size => size;

    public List<int>[] Wave => wave;

    public int CollapsedCount => collapsedCount;

    public Prototype[] AllPrototypes => allPrototypes;

    public int[] Dx => dx;

    public int[] Dy => dy;

    public int[] Dz => dz;

    public int Mx => MX;

    public int My => MY;

    public int Mz => MZ;

    public int Mxy => MXY;

    public void Start()
    {
        string jsonpPath = Application.dataPath + "/3DWaveFunctionCollapseSample/PrototypeConfig.json";
        var prototypes = ProtoPreprocess.LoadJson(jsonpPath);
        Initialize(size,prototypes);
        StartCoroutine(Run());
    }

    private void Update()
    {
        // if (Input.GetKeyDown(KeyCode.Space))
        // {
        //     if (!IsCollapsed())
        //     {
        //         Iterate();
        //         ShowInstance();
        //     }
        // }
    }

    private List<GameObject> showGameObjects = new List<GameObject>();
    private void ShowInstance()
    {
        if (showGameObjects.Count > 0)
        {
            for (int i = 0; i < showGameObjects.Count; i++)
            {
                Destroy(showGameObjects[i]);
            }
        }
        showGameObjects.Clear();
        for (int z = 0; z < size.z; z++)
        {
            for (int y = 0; y < size.y; y++)
            {
                for (int x = 0; x < size.x; x++)
                {
                    int index = MXY * z + MX * y + x;
                    if (wave[index].Count != 1 )
                    {
                        continue;
                    }
                    Prototype prototype = allPrototypes[wave[index][0]];
                    if (prototype != null)
                    {
                        GameObject ob = CreateInstance(prototype);
                        ob.transform.position = new Vector3(x + 0.5f, y, z + 0.5f);
                        showGameObjects.Add(ob);
                    }
                }
            }
        }
    }
    
    
    public GameObject CreateInstance(Prototype prototype)
    {
        GameObject prefab = Resources.Load<GameObject>("3DWaveFunctionCollapseSample/Blocks/" + prototype.model);
        GameObject instance = Instantiate(prefab);
        instance.transform.eulerAngles = new Vector3(0,prototype.rotation * 90,0);
        return instance;
    }

    /// <summary>
    /// 初始化波函数
    /// </summary>
    public void Initialize(Vector3Int size,in List<Prototype> allPrototypes)
    {
        this.size = size;
        this.allPrototypes = allPrototypes.ToArray();
        this.weightCache = new int[allPrototypes.Count];
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
        Debug.Log("CollapseTo :"+coord);
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
        stack.Push(coord);
        while (stack.Count > 0)
        {
            int curCoord = stack.Pop();
            // Debug.Log("Propagate :"+curCoord);
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
                if (otherPossiblePrototypes.Count <= 1)
                {
                    continue;
                }

                for (int i = otherPossiblePrototypes.Count -1; i >=0 ; i--)
                {
                    if (!possibleNeighbours.Contains(otherPossiblePrototypes[i]))
                    {
                        otherPossiblePrototypes.RemoveAt(i);
                        if (otherPossiblePrototypes.Count <= 1)
                        {
                            collapsedCount--;
                        }
                        if (!stack.Contains(otherCoord))
                        {
                            stack.Push(otherCoord);
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
        
        for (int i = 0; i < possiblePrototypes.Count; i++)
        {
            Prototype prototype = allPrototypes[possiblePrototypes[i]];
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
        for (int i = 0; i < weightCache.Length; i++)
        {
            weightCache[i] = 0;
        }
        float sumOfWeights = 0;
        for (int i = 0; i < possiblePrototypes.Count; i++)
        {
            int index = possiblePrototypes[i];
            weightCache[index] = allPrototypes[index].weight;
            sumOfWeights += weightCache[index];
        }
        int prototypeIndex = 0;
        float range = Random.Range(0f, sumOfWeights);
        for (int i = 0; i < weightCache.Length; i++)
        {
            range -= weightCache[i];
            if (range <= 0)
            {
                prototypeIndex = i;
                break;
            }
        }
        
        // int range = Random.Range(0, possiblePrototypes.Count);
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

    
    
    public IEnumerator Run()
    {
        while (!IsCollapsed())
        {
            if (!Iterate())
            {
                yield break;
            }

            if (RunTime)
            {
                yield return new WaitForSeconds(0.1f);
                ShowInstance();
            }
        }
        if (!RunTime)
        {
            ShowInstance();
        }

        yield return null;
    }
}

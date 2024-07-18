
using System.Collections.Generic;
using UnityEditor;
using Newtonsoft.Json;
using Unity.Mathematics;
using UnityEngine;

public static class ProtoPreprocess
{
    [MenuItem("Assets/ProtoPreprocess")]
    public static void Preprocess()
    {
        string jsonpPath = Application.dataPath + "/3DWaveFunctionCollapseSample/PrototypeConfig.json";
        var prototypes = LoadJson(jsonpPath);
        ClearData(prototypes);
        PrototypePreprocess(prototypes);
        SaveJson(jsonpPath, prototypes);
    }

    //对应面下标
    private static int[] matchingSurface = new int[6] { 1, 0, 3, 2, 5, 4 };

    public static void PrototypePreprocess(List<Prototype> prototypes)
    {
        for (int i = 0; i < prototypes.Count; i++)
        {
            var current = prototypes[i];
            for (int j = 0; j < prototypes.Count; j++)
            {
                var contrast = prototypes[j];
                for (int s = 0; s < 6; s++)
                {
                    if (MatchCheck(current.sockets[s], contrast.sockets[matchingSurface[s]], s,matchingSurface[s]))
                    {
                        if (s == 0)
                        {
                            current.posX.Add(j);
                        }else if (s == 1)
                        {
                            current.negX.Add(j);
                        }else if (s == 2)
                        {
                            current.posY.Add(j);
                        }else if (s == 3)
                        {
                            current.negY.Add(j);
                        }else if (s == 4)
                        {
                            current.posZ.Add(j);
                        }else if (s == 5)
                        {
                            current.negZ.Add(j);
                        }
                    }
                }
            }
        }
    }

    public static void ClearData(List<Prototype> prototypes)
    {
        for (int i = 0; i < prototypes.Count; i++)
        {
            prototypes[i].posX.Clear();
            prototypes[i].negX.Clear();
            prototypes[i].posY.Clear();
            prototypes[i].negY.Clear();
            prototypes[i].posZ.Clear();
            prototypes[i].negZ.Clear();
        }
    }

    public static bool MatchCheck(string socketA, string socketB, int socketAI, int socketBI)
    {
        //vertical
        if (socketAI == 2 | socketAI == 3)
        {
            if (string.Equals(socketA, socketB))
            {
                return true;
            }
        }
        //asymmetrical
        if (string.Equals(socketA,socketB+"f") | string.Equals(socketA + "f",socketB) )
        {
            return true;
        }
        //symmetrical
        if (socketA.EndsWith('s') & socketB.EndsWith('s'))
        {
            if (string.Equals(socketA,socketB))
            {
                return true;
            }
        }
        //empty
        if ((socketAI != 2 & socketAI != 3) & string.Equals(socketA,"-1") & string.Equals(socketB, "-1"))
        {
            return true;
        }
        
        return false;
    }

    public static List<Prototype> LoadJson(string path)
    {
        var json = System.IO.File.ReadAllText(path);
        List<Prototype> prototypes = JsonConvert.DeserializeObject<List<Prototype>>(json);
        return prototypes;
    }

    public static void SaveJson(string path, List<Prototype> prototypes)
    {
        var json = JsonConvert.SerializeObject(prototypes);
        System.IO.File.WriteAllText(path, json);
    }
}
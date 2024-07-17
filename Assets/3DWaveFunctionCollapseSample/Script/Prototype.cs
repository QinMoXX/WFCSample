
using System.Collections.Generic;

public class Prototype
{
    public string model;
    public int rotation;
    public string[] sockets;

    public List<int> posX;
    public List<int> negX;
    public List<int> posY;
    public List<int> negY;
    public List<int> posZ;
    public List<int> negZ;

    // public bool Judge(string socketA, string socketB)
    // {
    //     if (symmetrical)
    //     {
    //         return socketA == socketB;
    //     }
    //
    //     if (asymmetrical)
    //     {
    //         return socketA == socketB + "f" || socketA + "f" == socketB;
    //     }
    //
    //     if (vertical)
    //     {
    //         return socketA == socketB;
    //     }
    //
    //     return false;
    // }
}
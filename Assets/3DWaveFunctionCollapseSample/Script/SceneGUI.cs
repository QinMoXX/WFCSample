using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(WaveFunctionCollapse))]
public class SceneGUI:Editor
{
    private WaveFunctionCollapse m_WFC;
    private void OnSceneGUI()
    {
        if (m_WFC == null)
        {
            m_WFC = GameObject.FindObjectOfType<WaveFunctionCollapse>();
        }

        if (m_WFC == null | m_WFC.Wave == null)
        {
            return;
        }
        for (int z = 0; z < m_WFC.Size.z; z++)
        {
            for (int y = 0; y < m_WFC.Size.y; y++)
            {
                for (int x = 0; x < m_WFC.Size.x; x++)
                {
                    int index = m_WFC.Mxy * z + m_WFC.Mx * y + x;
                    string str = BreakLongString(m_WFC.Wave[index], 5);
                    //显示的坐标文字
                    Handles.Label( new Vector3(x + 0.5f,y,z+0.5f) , str);
                }
            }
        }
    }
    
    public string BreakLongString(IList<int> intArray, int lineLength)  
    {  
        StringBuilder sb = new StringBuilder();  
        int offset = 0;  
        for(int i=0;i<intArray.Count;i++)
        {
            sb.Append(intArray[i].ToString()+' ');
            if (++offset % lineLength == 0)
            {
                sb.Append('\n');
            }
        }  
        return sb.ToString();  
    }
}
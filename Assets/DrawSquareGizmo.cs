
using UnityEngine;


public class DrawSquareGizmo : MonoBehaviour
{
    // 这个方法在编辑器中绘制Gizmos
    private void OnDrawGizmos()
    {
        // 设置Gizmos的颜色
        Gizmos.color = Color.red;
        Vector3 pos = transform.position + Rotate(new Vector3(-0.5f, 0.5f, 0.5f), -transform.eulerAngles.y /180 * Mathf.PI) ;
        Gizmos.DrawWireCube(pos, new Vector3(1, 1, 1));
    }
    
    // 计算点 (x, y) 绕原点旋转 theta 角度后的新坐标
    public Vector3 Rotate(Vector3 point, float theta)
    {
        float cosTheta = Mathf.Cos(theta);
        float sinTheta = Mathf.Sin(theta);
        
        float xNew = point.x * cosTheta - point.z * sinTheta;
        float zNew = point.x * sinTheta + point.z * cosTheta;
        
        return new Vector3(xNew, point.y, zNew);
    }
}


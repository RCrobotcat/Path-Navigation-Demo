using NaviFunnel;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// 导航计算结果展示
public class NaviView : MonoBehaviour
{
    public Transform AreaIDRoot;
    public NaviVector[] vertexArr;

    /// <summary>
    /// 显示区域ID
    /// </summary>
    public void ShowAreaIDView(NaviVector position, int areaID)
    {
        var go = new GameObject { name = $"AreaID {areaID}" };
        go.transform.SetParent(AreaIDRoot);

        go.transform.position = position.ConvertToUnityVector();
        go.transform.localEulerAngles = new Vector3(90, 0, 0);
        var comp = go.AddComponent<TextMesh>();

        comp.text = areaID.ToString();
        comp.anchor = TextAnchor.MiddleCenter;
        comp.alignment = TextAlignment.Center;
        comp.color = Color.gray;
        comp.fontSize = 45;
        comp.fontStyle = FontStyle.Normal;

        go.transform.localScale = Vector3.one;
    }

    /// <summary>
    /// 显示区域路径
    /// </summary>
    public void ShowPathAreaView(List<NaviArea> pathAreaList)
    {
        string PathID = "Path Area ID:";
        for (int i = 0; i < pathAreaList.Count; i++)
        {
            PathID = $"{PathID} {pathAreaList[i].areaID}";
            NaviArea area = pathAreaList[i];
            int[] indexArr = area.indexArr;

            NaviVector v1, v2;
            for (int j = 0, k = indexArr.Length - 1; j < indexArr.Length; k = j++)
            {
                v1 = vertexArr[indexArr[k]];
                v2 = vertexArr[indexArr[j]];
                ShowDebugLine(v1, v2, Color.yellow, 5);
            }
        }
    }

    void ShowDebugLine(NaviVector v1, NaviVector v2, Color color, float showTime = float.MaxValue)
    {
        Debug.DrawLine(v1.ConvertToUnityVector(), v2.ConvertToUnityVector(), color, showTime);
    }
}
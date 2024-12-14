using PEUtils;
using NaviFunnel;
using UnityEngine;
using System.Collections.Generic;

// 多边形漏斗寻路算法基础架构
public class NavFunnelRoot : MonoBehaviour
{
    NaviConfig naviConfig;
    NaviMap naviMap;

    void Start()
    {
        PELog.InitSettings(LoggerType.Unity);
        PELog.LogGreen("Init PELog Done.");

        CalculateNaviConfig();

        NaviView naviView = GetComponent<NaviView>();
        if (naviView != null)
        {
            NaviMap.ShowAreaIDView += naviView.ShowAreaIDView;
        }

        naviMap = new NaviMap(naviConfig.indexArrList, naviConfig.vertexArr);
    }

    /// <summary>
    /// 读取场景配置数据
    /// </summary>
    void CalculateNaviConfig()
    {
        GameObject map = GameObject.FindGameObjectWithTag("FunnelMapRoot");
        Transform pointRoot = map.transform.Find("pointRoot");
        Transform indexRoot = map.transform.Find("indexRoot");

        naviConfig = new NaviConfig
        {
            indexArrList = new List<int[]>(),
            vertexArr = new NaviVector[pointRoot.childCount]
        };

        for (int i = 0; i < indexRoot.childCount; i++)
        {
            Transform trans = indexRoot.GetChild(i);
            string[] indexArrString = trans.name.Split('-');
            int[] indexArr = new int[indexArrString.Length];
            for (int j = 0; j < indexArrString.Length; j++)
            {
                indexArr[j] = int.Parse(indexArrString[j]);
            }
            naviConfig.indexArrList.Add(indexArr);
        }

        NaviVector[] vertexArr = new NaviVector[pointRoot.childCount];
        for (int i = 0; i < pointRoot.childCount; i++)
        {
            vertexArr[i] = new NaviVector(pointRoot.GetChild(i).position);
        }
        naviConfig.vertexArr = vertexArr;
    }

    public Color DrawGizmosColor = Color.black;
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying || naviConfig == null)
        {
            CalculateNaviConfig();
        }

        Gizmos.color = DrawGizmosColor;
        for (int i = 0; i < naviConfig.indexArrList.Count; i++)
        {
            int[] indexArr = naviConfig.indexArrList[i];
            Vector3 v1, v2; // 多边形上某一条边的起点和终点
            int count = indexArr.Length;
            for (int j = 0, k = count - 1; j < count; k = j++)
            {
                int index1 = indexArr[k];
                int index2 = indexArr[j];

                v1 = naviConfig.vertexArr[index1].ConvertToUnityVector();
                v2 = naviConfig.vertexArr[index2].ConvertToUnityVector();
                Gizmos.DrawLine(v1, v2);
            }
        }

        Gizmos.color = Color.white;
    }
}

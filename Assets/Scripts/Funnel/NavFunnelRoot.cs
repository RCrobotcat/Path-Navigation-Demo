using PEUtils;
using NaviFunnel;
using UnityEngine;
using System.Collections.Generic;

// 多边形漏斗寻路算法基础架构
public class NavFunnelRoot : MonoBehaviour
{
    public bool ClickingAvailable = true; // 是否可以点击设置起点和终点

    NaviConfig naviConfig;
    NaviMap naviMap;

    NaviVector startPos = NaviVector.Zero;
    NaviVector endPos = NaviVector.Zero;

    void Start()
    {
        PELog.InitSettings(LoggerType.Unity);
        PELog.LogGreen("Init PELog Done.");

        CalculateNaviConfig();

        NaviView naviView = GetComponent<NaviView>();
        if (naviView != null)
        {
            naviView.vertexArr = naviConfig.vertexArr;
            NaviMap.ShowAreaIDView += naviView.ShowAreaIDView;
            NaviMap.ShowPathAreaView += naviView.ShowPathAreaView;
            NaviMap.ShowInflectionPointView += naviView.ShowInflectionPointView;
        }

        naviMap = new NaviMap(naviConfig.indexArrList, naviConfig.vertexArr);
    }

    void Update()
    {
        // 鼠标左键设置起点
        if (Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, 1000))
            {
                Transform start = GameObject.FindGameObjectWithTag("Start").transform;
                if (ClickingAvailable) start.position = hit.point;
                startPos = new NaviVector(start.position);
            }
        }

        // 鼠标右键设置终点
        if (Input.GetMouseButtonDown(1))
        {
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, 1000))
            {
                Transform end = GameObject.FindGameObjectWithTag("End").transform;
                if (ClickingAvailable) end.position = hit.point;
                endPos = new NaviVector(end.position);

                if (startPos != endPos)
                {
                    naviMap.CalculateNaviPath(startPos, endPos);
                }
            }
        }
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

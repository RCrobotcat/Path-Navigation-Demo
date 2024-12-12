using PEUtils;
using UnityEngine;

// 多边形漏斗寻路算法基础架构
public class NavFunnelRoot : MonoBehaviour
{
    private void Start()
    {
        PELog.InitSettings(LoggerType.Unity);
        PELog.LogGreen("Init PELog Done.");
    }
}

using PEUtils;
using UnityEngine;

// �����©��Ѱ·�㷨�����ܹ�
public class NavFunnelRoot : MonoBehaviour
{
    private void Start()
    {
        PELog.InitSettings(LoggerType.Unity);
        PELog.LogGreen("Init PELog Done.");
    }
}

using UnityEngine;
using NaviPath;
using System.Collections.Generic;

public enum OperationEnum
{
    Prepare, // ׼���׶�: ���õ���
    SetPoint, // �������׶�
    Searching, // Ѱ·�н׶�
    ShowResult // ��ʾ����׶�
}

public class PathFindingRoot : MonoBehaviour
{
    public static PathFindingRoot Instance; // Singleton

    public int xCount;
    public int yCount;
    public GameObject blockObject;
    public Transform blockRoot;

    public int searchDelay = 5;

    [HideInInspector] public OperationEnum operation = OperationEnum.Prepare;

    BlockLogic m_startBlock = null;
    BlockLogic m_endBlock = null;
    BlockMap blockMap;

    void Start()
    {
        Instance = this;

        blockMap = new();
        blockMap.InitMap(xCount, yCount);
    }

    public async void OnClickBlockItem(BlockLogic block)
    {
        switch (operation)
        {
            case OperationEnum.Prepare:
                operation = OperationEnum.SetPoint;
                m_startBlock = block;
                m_endBlock = null;
                m_startBlock.OnViewChange(ViewState.StatingPoint);
                break;
            case OperationEnum.SetPoint:
                operation = OperationEnum.Searching;
                m_endBlock = block;
                Debug.Log($"starting point: {m_startBlock}, ending point: {m_endBlock}");
                m_endBlock.OnViewChange(ViewState.EndingPoint);

                // Ѱ·
                blockMap.UpdateMapData();
                BasePathFinder basePathFinder = new BasePathFinder();
                // List<BlockLogic> pathList = blockMap.CalculatePath(m_startBlock, m_endBlock, basePathFinder);

                float startTime = Time.realtimeSinceStartup;
                // �ȴ�(await)Ѱ·���
                List<BlockLogic> pathList = await blockMap.CalculatePath(m_startBlock, m_endBlock, basePathFinder);

                string idStr = "PathID: ";
                for (int i = 0; i < pathList.Count; i++)
                {
                    pathList[i].OnViewChange(ViewState.PathResult);
                    idStr += pathList[i].XIndex + "," + pathList[i].YIndex + " -> ";
                }
                Debug.Log($"Length: {m_endBlock.sumDistance}, Calculation Time: {Time.realtimeSinceStartup - startTime}");
                m_endBlock.OnViewChange(ViewState.EndingPoint); // �����յ�״̬Ϊ��ɫ
                operation = OperationEnum.ShowResult;
                break;
            case OperationEnum.Searching:
                break;
            case OperationEnum.ShowResult:
                break;
            default:
                break;
        }
    }

    public BlockView CreateBlockView()
    {
        GameObject go = Instantiate(blockObject);
        go.transform.SetParent(blockRoot);
        BlockView view = go.GetComponent<BlockView>();
        return view;
    }
}
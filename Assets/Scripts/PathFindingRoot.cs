using UnityEngine;
using NaviPath;
using System.Collections.Generic;

public enum OperationEnum
{
    Prepare, // 准备阶段: 设置地形
    SetPoint, // 设置起点阶段
    Searching, // 寻路中阶段
    ShowResult // 显示结果阶段
}

public enum PathFinderMode
{
    BFS,
    Dijkstra,
    AStar
}

public class PathFindingRoot : MonoBehaviour
{
    public static PathFindingRoot Instance; // Singleton

    public PathFinderMode pathFinderMode = PathFinderMode.BFS;

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

    void Update()
    {
        // 重置地图: 直接重新开始寻路
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (operation == OperationEnum.ShowResult)
            {
                blockMap.SaveWalkableState();
                blockMap.UnInitMap();
                operation = OperationEnum.Prepare;

                blockMap.LoadWalkableState();

                BlockLogic tempEnd = m_endBlock;
                OnClickBlockItem(m_startBlock);
                OnClickBlockItem(tempEnd);
            }
        }

        // 重置地图: 在原地形基础上设置地形以及重新设置起点终点
        if (Input.GetKeyDown(KeyCode.C))
        {
            if (operation == OperationEnum.ShowResult)
            {
                blockMap.SaveWalkableState();
                blockMap.UnInitMap();
                operation = OperationEnum.Prepare;

                blockMap.LoadWalkableState();
            }
        }

        // 完全重置地图
        if (Input.GetKeyDown(KeyCode.E))
        {
            blockMap.SaveWalkableState();
            blockMap.UnInitMap();
            operation = OperationEnum.Prepare;
        }
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
                // Debug.Log($"starting point: {m_startBlock}, ending point: {m_endBlock}");
                m_endBlock.OnViewChange(ViewState.EndingPoint);

                // 寻路
                blockMap.UpdateMapData();
                BasePathFinder basePathFinder = null;
                switch (pathFinderMode)
                {
                    case PathFinderMode.BFS:
                        basePathFinder = new BFSFinder();
                        break;
                    case PathFinderMode.Dijkstra:
                        basePathFinder = new DijkstraFinder();
                        break;
                    case PathFinderMode.AStar:
                        // basePathFinder = new AStarFinder();
                        basePathFinder = new BFSFinder();
                        break;
                    default:
                        break;
                }
                // List<BlockLogic> pathList = blockMap.CalculatePath(m_startBlock, m_endBlock, basePathFinder);

                float startTime = Time.realtimeSinceStartup;
                // 等待(await)寻路完成
                List<BlockLogic> pathList = await blockMap.CalculatePath(m_startBlock, m_endBlock, basePathFinder);

                string idStr = "PathID: ";
                for (int i = 0; i < pathList.Count; i++)
                {
                    pathList[i].OnViewChange(ViewState.PathResult);
                    idStr += pathList[i].XIndex + "," + pathList[i].YIndex + " -> ";
                }
                Debug.Log($"{pathFinderMode}\nLength: {m_endBlock.sumDistance}, Calculation Time: {Time.realtimeSinceStartup - startTime}");
                m_endBlock.OnViewChange(ViewState.EndingPoint); // 重置终点状态为红色
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

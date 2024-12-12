// 贪心算法寻路
namespace NaviPath
{
    public class GreedyPathFinder : BasePathFinder
    {
        protected override void DetectNeighborBlocks(BlockLogic detectBlock, BlockLogic neighborBlock)
        {
            if (!m_finishedList.Contains(neighborBlock) && !m_detectQueue.Contains(neighborBlock))
            {
                // 正在检测区块，显示为检测中
                neighborBlock.OnViewChange(ViewState.Checking);
                float neighborDistance = BlockLogic.GetLogicBlockDistance(detectBlock, neighborBlock);
                neighborBlock.sumDistance = detectBlock.sumDistance + neighborDistance;

                neighborBlock.priority = BlockLogic.GetLogicBlockDistance(neighborBlock, m_endBlock);
                neighborBlock.preBlock = detectBlock;
                m_detectQueue.Enqueue(neighborBlock);

                // 检测完后显示方向指向信息
                neighborBlock.OnViewChange(ViewState.DirctionDebug);
            }
        }
    }
}

using System;
using System.Collections.Generic;

// 单个逻辑区块
namespace NaviPath
{
    public class BlockLogic : IComparable<BlockLogic>
    {
        int m_xIndex;
        int m_yIndex;
        bool m_walkable;

        public int XIndex { get => m_xIndex; }
        public int YIndex { get => m_yIndex; }
        public bool Walkable { get => m_walkable; }

        #region View
        BlockView m_view;
        public Action<ViewState> OnViewChange;
        #endregion

        public List<BlockLogic> neighborsList; // 相邻区块列表
        public BlockLogic preBlock; // 记录当前区块的前一个区块
        public float sumDistance = float.PositiveInfinity; // 记录起点到当前点的总距离, 默认无穷大

        public float priority; // 优先级

        public void InitBlockLogic(int x, int y)
        {
            // 初始化区块
            m_xIndex = x;
            m_yIndex = y;
            m_walkable = true; // 默认可行走

            // 视图初始化
            m_view = PathFindingRoot.Instance.CreateBlockView();
            m_view.InitBlockView(this);

            OnViewChange.Invoke(ViewState.Walk);
        }

        public void UnInitBlockLogic()
        {
            m_walkable = true;
            sumDistance = float.PositiveInfinity;
            preBlock = null;

            m_view.UnInitBlockView();
        }

        public void SetWalkableState(bool state)
        {
            m_walkable = state;
            OnViewChange(m_walkable ? ViewState.Walk : ViewState.Block);
        }

        /// <summary>
        /// 获取逻辑区块之间的距离(欧几里得距离)
        /// </summary>
        public static float GetLogicBlockDistance(BlockLogic start, BlockLogic target)
        {
            var x = MathF.Abs(target.XIndex - start.XIndex);
            var y = MathF.Abs(target.YIndex - start.YIndex);
            var min = Math.Min(x, y);
            var max = Math.Max(x, y);
            return 1.4f * min + (max - min);
        }

        /// <summary>
        /// 获取逻辑区块之间的距离(曼哈顿距离)
        /// </summary>
        /// <returns></returns>
        public static float GetManhattanDistance(BlockLogic start, BlockLogic target)
        {
            var x = MathF.Abs(target.XIndex - start.XIndex);
            var y = MathF.Abs(target.YIndex - start.YIndex);
            return x + y;
        }

        public override string ToString()
        {
            return $"[{XIndex}, {YIndex}]";
        }

        // 优先级队列比较
        public int CompareTo(BlockLogic otherBlock)
        {
            if (priority < otherBlock.priority)
            {
                return -1;
            }
            else if (priority > otherBlock.priority)
            {
                return 1;
            }
            else return 0;
        }
    }
}

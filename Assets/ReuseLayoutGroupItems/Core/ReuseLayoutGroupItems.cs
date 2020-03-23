using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIExtension
{
    public enum LayoutOrient
    {
        Extend,
        Fixed,
    }

    public enum Aligment
    {
        Left,
        Right,
        Top,
        Bottom,
    }

    public enum AligmentTag
    {
        Start,
        End,
    }

    public enum ExtendOrient
    {
        Horizontal,
        Vertical,
    }

    public enum LayoutGroupType
    {
        Grid,
        Vertical,
        Horizontal,
    }

    public abstract class ReuseLayoutGroupItems : MonoBehaviour
    {
        protected ExtendOrient m_ExtendOrient;

        protected LayoutGroupType? m_LayoutGroupType;

        protected ReuseItem m_ItemPrefab;

        protected int m_ItemCount;

        protected Action<int, ReuseItem> m_ItemRefresh;

        protected ScrollRect m_ScrollRect;
        protected LayoutGroup m_LayoutGroup;

        protected float m_LayoutTop;
        protected float m_LayoutBottom;
        protected float m_LayoutLeft;
        protected float m_LayoutRight;
        protected float m_LayoutSpacingExtend;
        protected float m_LayoutSpacingFixed;

        private List<ReuseItem> m_Items;

        protected float m_InitContentExtendPos;
        protected float m_InitContentFixedPos;
        protected int m_NowExtendGroup;
        protected int m_NowFixedGroup;
        protected Vector2 m_InitItemPos;
        protected float m_itemExtendDeltaPos;
        protected float m_itemFixedDeltaPos;

        protected bool m_Init = false;
        protected bool m_RefreshLayout = false;

        protected float m_ItemExtendLength;
        protected float m_ItemFixedLength;

        protected int m_ItemExtendCount;

        protected List<float> m_ItemExtendLengths;
        protected float m_CheckRefreshStart = 0;
        protected float m_CheckRefreshEnd = 0;

        protected bool SameItemLength;

        protected float m_MinItemExtendLength = float.MaxValue;

        protected List<ReuseItem> Items { get => m_Items; set => m_Items = value; }

        public void AddItem()
        {
            if (!SameItemLength)
                return;

            RefreshItems(m_ItemCount + 1);
        }

        public void RemoveItem()
        {
            if (GetItemCount() <= 0)
                return;

            if (SameItemLength)
            {
                RefreshItems(m_ItemCount - 1);
            }
            else
            {
                m_ItemExtendLengths.RemoveAt(m_ItemExtendLengths.Count - 1);
                InteralRefreshItems();
            }
        }

        public void RemoveAllItems()
        {
            if (SameItemLength)
            {
                RefreshItems(0);
            }
            else
            {
                m_ItemExtendLengths.Clear();
                InteralRefreshItems();
            }
        }

        public void RefreshItems(int itemCount)
        {
            if (!SameItemLength)
                return;

            m_ItemCount = itemCount;

            SetContentSize();
            //刷新后，当前已经滑动的位置大于contentsize，默认显示到最底部，否则当前位置不变
            int newExtendGroup = GetItemCount(LayoutOrient.Extend, itemCount) - GetItemShowCount(LayoutOrient.Extend);

            if (newExtendGroup < m_NowExtendGroup)
            {
                SetContentPosToEnd();
            }
            else
            {
                RefreshLayout();
            }
        }


        public void AddItem(float itemLength)
        {
            if (SameItemLength)
                return;

            m_ItemExtendLengths.Add(itemLength);
            InteralRefreshItems();
        }

        public void RefreshItems(List<float> itemExtendLengths)
        {
            if (SameItemLength)
                return;

            m_ItemExtendLengths = itemExtendLengths;

            SetContentSize();

            Reset();
            m_CheckRefreshEnd = GetItemLengthWithSpacing(LayoutOrient.Extend, 0);

            ResetContentPos();
        }

        public void ResetContentPos()
        {
            m_ScrollRect.content.anchoredPosition = new Vector2(0, 0);
            CheckAndRefreshLayout();
        }

        public void SetContentPosToEnd()
        {
            m_ScrollRect.content.anchoredPosition = GetEndPos();
            CheckAndRefreshLayout();
        }

        public List<ReuseItem> GetItems()
        {
            return Items;
        }

        public ReuseItem GetItem(int index)
        {
            return Items.Find((item) => item.Index == index);
        }

        public float GetExtendContentLength()
        {
            return GetContentLength(LayoutOrient.Extend);
        }

        protected void OnEnable()
        {
        }

        protected void Start()
        {
        }

        protected void OnDestroy()
        {
            if (!m_Init)
                return;

            CleatItems();
            m_Items.Clear();
        }

        public void InitItems(ReuseItem itemPrefab, int itemCount, float itemExtendLength, float itemFixedLength, Action<int, ReuseItem> itemRefresh)
        {
            SameItemLength = true;

            m_ItemPrefab = itemPrefab;
            m_ItemExtendLength = itemExtendLength;
            m_ItemFixedLength = itemFixedLength;
            m_ItemRefresh = itemRefresh;
            m_ItemCount = itemCount;

            m_Init = false;

            if (!InitCheck())
            {
                Debug.LogError("ReuseLayoutGroupItems Init Failed");
                return;
            }

            Init();

            CleatItems();
            GenerateItems();
        }

        public void InitItems(ReuseItem itemPrefab, List<float> itemExtendLengths, float itemFixedLength, Action<int, ReuseItem> itemRefresh)
        {
            SameItemLength = false;

            m_ItemPrefab = itemPrefab;
            m_ItemExtendLengths = itemExtendLengths;
            m_ItemFixedLength = itemFixedLength;
            m_ItemRefresh = itemRefresh;
            m_ItemCount = itemExtendLengths.Count;

            m_MinItemExtendLength = float.MaxValue;
            m_ItemExtendLengths.ForEach((length) => { if (length < m_MinItemExtendLength) { m_MinItemExtendLength = length; } });
            m_ItemExtendLength = m_MinItemExtendLength;

            m_Init = false;

            if (!InitCheck())
            {
                Debug.LogError("ReuseLayoutGroupItems Init Failed");
                return;
            }

            Init();

            CleatItems();
            GenerateItems();
        }

        protected void GenerateItems()
        {
            int generateCount = GetItemShowCount(LayoutOrient.Fixed) * GetItemMaxShowCount(LayoutOrient.Extend);//GetItemShowCount(LayoutOrient.Fixed) * GetItemShowCount(LayoutOrient.Extend);

            m_Items = new List<ReuseItem>(generateCount);

            for (int i = 0; i < generateCount; i++)
            {
                ReuseItem item = GameObject.Instantiate<ReuseItem>(m_ItemPrefab, m_ScrollRect.content);
                item.RefreshCallback = m_ItemRefresh;
                m_Items.Add(item);
            }

            m_Init = true;

            RefreshLayout();
        }

        private void CleatItems()
        {
            foreach (Transform child in m_ScrollRect.content)
            {
                if (child.GetComponent<ReuseItem>() != null)
                {
                    GameObject.DestroyImmediate(child.gameObject);
                }
            }
        }

        public bool InitCheck()
        {
            m_ScrollRect = GetComponent<ScrollRect>();

            if (m_ScrollRect == null)
                return false;

            if (m_ScrollRect.content == null)
                return false;

            LayoutGroup layoutGroup = m_ScrollRect.content.GetComponent<GridLayoutGroup>();
            if (layoutGroup)
            {
                m_LayoutGroupType = LayoutGroupType.Grid;
                m_LayoutGroup = layoutGroup;
            }

            layoutGroup = m_ScrollRect.content.GetComponent<VerticalLayoutGroup>();
            if (layoutGroup)
            {
                m_LayoutGroupType = LayoutGroupType.Vertical;
                m_LayoutGroup = layoutGroup;
            }

            layoutGroup = m_ScrollRect.content.GetComponent<HorizontalLayoutGroup>();
            if (layoutGroup)
            {
                m_LayoutGroupType = LayoutGroupType.Horizontal;
                m_LayoutGroup = layoutGroup;
            }

            if (m_LayoutGroupType == null)
                return false;

            return true;
        }

        private void Init()
        {
            m_NowFixedGroup = 0;
            m_NowExtendGroup = 0;

            InitLayout();

            m_CheckRefreshStart = 0;
            m_CheckRefreshEnd = GetNowItemExtendLengthWithSpacing();


            if (m_LayoutGroupType == LayoutGroupType.Grid)
            {
                SetFixedItemCount();
            }

            SetContentSize();
            SetInitContentPos();

            m_ScrollRect.onValueChanged.AddListener(OnScrollRectValueChanged);
        }

        protected virtual void InitLayout()
        {
            LayoutGroup layoutGroup = m_ScrollRect.content.GetComponent<LayoutGroup>();
            RectOffset padding = layoutGroup.padding;

            m_LayoutTop = padding.top;
            m_LayoutBottom = padding.bottom;
            m_LayoutLeft = padding.left;
            m_LayoutRight = padding.right;

            switch (m_LayoutGroupType)
            {
                case LayoutGroupType.Grid:
                    GridLayoutGroup gridLayoutGroup = layoutGroup as GridLayoutGroup;

                    m_LayoutSpacingExtend = gridLayoutGroup.spacing.y;
                    m_LayoutSpacingFixed = gridLayoutGroup.spacing.x;

                    m_ItemPrefab.Width = gridLayoutGroup.cellSize.x;
                    m_ItemPrefab.Height = gridLayoutGroup.cellSize.y;

                    break;

                case LayoutGroupType.Vertical:
                    VerticalLayoutGroup verticalLayoutGroup = layoutGroup as VerticalLayoutGroup;

                    m_LayoutSpacingExtend = verticalLayoutGroup.spacing;
                    m_LayoutSpacingFixed = 0;

                    m_ItemPrefab.Width = m_ItemFixedLength;
                    m_ItemPrefab.Height = m_ItemExtendLength;

                    break;

                case LayoutGroupType.Horizontal:
                    HorizontalLayoutGroup horizontalLayoutGroup = layoutGroup as HorizontalLayoutGroup;

                    m_LayoutSpacingExtend = horizontalLayoutGroup.spacing;
                    m_LayoutSpacingFixed = 0;

                    m_ItemPrefab.Width = m_ItemExtendLength;
                    m_ItemPrefab.Height = m_ItemFixedLength;

                    break;
            }

        }

        protected void OnScrollRectValueChanged(Vector2 offset)
        {
            //Debug.Log("OnScrollRectValueChanged");
            RefreshLayoutWithCheck();
        }

        protected void CheckAndRefreshLayout()
        {
            CheckRefreshLayout();
            RefreshLayout();
        }

        protected bool CheckRefreshLayout()
        {
            if (!m_Init)
            {
                return false;
            }

            if (GetItemCount() <= 0)
            {
                return false;
            }

            if (GetItemCount() <= m_Items.Count)
            {
                Reset();
                m_CheckRefreshEnd = GetItemLengthWithSpacing(LayoutOrient.Extend, 0);
                return false;
            }

            bool refresh = false;

            //Extend
            float extendScrollDelta = GetScrollDelta(LayoutOrient.Extend);

            //Debug.Log("extendScrollDelta:" + extendScrollDelta);

            int loop = 0;
            //一帧内移动跨越多个“Group”，需要多次判断
            while (true)
            {
                if (++loop >= 100)
                {
                    Debug.LogWarning("CheckRefreshLayout Loop:" + m_NowExtendGroup + "--" + GetItemCount() + "--" + extendScrollDelta + "--" + m_CheckRefreshStart + "--" + m_CheckRefreshEnd);
                    break;
                }

                if (m_NowExtendGroup < GetItemCount() && m_CheckRefreshEnd != m_CheckRefreshStart && extendScrollDelta >= m_CheckRefreshEnd)
                {
                    //Debug.Log("Arefresh1:" + m_NowExtendGroup + "--" + GetItemCount() + "--" + extendScrollDelta + "--" + m_CheckRefreshStart + "--" + m_CheckRefreshEnd);

                    m_itemExtendDeltaPos = GetItemDeltaSign(LayoutOrient.Extend) * m_CheckRefreshEnd;
                    m_CheckRefreshStart = m_CheckRefreshEnd;
                    m_NowExtendGroup++;
                    m_CheckRefreshEnd += GetNowItemExtendLengthWithSpacing();
                    refresh = true;
                    //Debug.Log("Brefresh1:" + m_NowExtendGroup + "--" + GetItemCount() + "--" + extendScrollDelta + "--" + m_CheckRefreshStart + "--" + m_CheckRefreshEnd);

                }
                else if (m_NowExtendGroup > 0 && extendScrollDelta <= m_CheckRefreshStart)
                {
                    m_CheckRefreshEnd = m_CheckRefreshStart;
                    m_NowExtendGroup--;
                    m_CheckRefreshStart -= GetNowItemExtendLengthWithSpacing();
                    m_itemExtendDeltaPos = GetItemDeltaSign(LayoutOrient.Extend) * m_CheckRefreshStart;
                    refresh = true;
                    //Debug.Log("refresh2");
                }
                else
                {
                    break;
                }
            }


            if (m_LayoutGroupType == LayoutGroupType.Grid)
            {
                //Fixed
                float fixedScrollDelta = GetScrollDelta(LayoutOrient.Fixed);

                int fixedGroup = Mathf.FloorToInt(fixedScrollDelta / GetItemLengthWithSpacing(LayoutOrient.Fixed));

                fixedGroup = Mathf.Clamp(fixedGroup, 0, GetItemCount(LayoutOrient.Fixed) - GetItemShowCount(LayoutOrient.Fixed));

                if (fixedGroup != m_NowFixedGroup || m_NowFixedGroup == 0)
                {
                    m_NowFixedGroup = fixedGroup;
                    m_itemFixedDeltaPos = GetItemDeltaPos(fixedGroup, LayoutOrient.Fixed);
                    refresh = true;
                }

            }

            return refresh;

        }

        protected float GetNowItemExtendLengthWithSpacing()
        {
            return GetItemLengthWithSpacing(LayoutOrient.Extend, m_NowExtendGroup);
        }

        protected float GetItemLengthWithSpacing(LayoutOrient layoutOrient, int index)
        {
            if (SameItemLength)
            {
                return GetItemLength(layoutOrient) + GetSpacingLength(LayoutOrient.Extend);
            }
            else
            {
                float itemLength = GetItemLength(layoutOrient, index);

                return itemLength == 0 ? 0 : itemLength + GetSpacingLength(LayoutOrient.Extend);
            }
        }

        protected int GetItemCount()
        {

            if (SameItemLength)
            {
                return m_ItemCount;
            }
            else
            {

                return m_ItemExtendLengths.Count;
            }
        }

        public void RefreshLayoutWithCheck()
        {
            if (CheckRefreshLayout())
            {
                RefreshLayout();
            }

        }

        public void RefreshLayout()
        {
            if (!m_Init)
            {
                return;
            }

            Vector2 deltaPos = GetItemDeltaPos();

            m_LayoutGroup.padding.top = (int)(m_LayoutTop - deltaPos.y);
            m_LayoutGroup.padding.left = (int)(m_LayoutLeft - deltaPos.x);

            m_LayoutGroup.enabled = false;
            m_LayoutGroup.enabled = true;

            int index = GetStartIndex();

            for (int i = 0; i < m_Items.Count; i++)
            {
                ReuseItem child = m_Items[i];

                //当滑动到固定方向中间或扩展方向中间，需要跳过之前、之后的item
                CheckIndex(ref index);

                float itemExtendLength = GetItemLength(LayoutOrient.Extend, index);

                child.Refresh(index, itemExtendLength, index < GetItemCount());
                index++;

            }

        }

        protected bool CheckAligment(LayoutOrient layoutOrient, AligmentTag aligmentTag)
        {
            Aligment aligment = GetAligment(layoutOrient, aligmentTag);

            return CheckLayoutGroupChildAlignment(aligment);


        }

        //item位置偏移符号，当向上对齐或向右对齐，在content中item需要向下或向左偏移，所以为负号
        protected int GetItemDeltaSign(LayoutOrient layoutOrient)
        {
            CheckAligment(layoutOrient, AligmentTag.Start);

            Aligment aligment = GetAligment(layoutOrient, AligmentTag.Start);
            Aligment? realAligment = null;

            if (CheckLayoutGroupChildAlignment(aligment))
            {
                realAligment = aligment;
            }

            aligment = GetAligment(layoutOrient, AligmentTag.End);
            if (CheckLayoutGroupChildAlignment(aligment))
            {
                realAligment = aligment;
            }

            switch (realAligment)
            {
                case Aligment.Top:
                case Aligment.Right:
                    return -1;
                case Aligment.Bottom:
                case Aligment.Left:
                    return 1;
                default:
                    Debug.LogError("aligment is error");
                    return 0;

            }
        }

        protected float GetItemDeltaPos(int group, LayoutOrient layoutOrient)
        {
            return GetItemDeltaSign(layoutOrient) * group * GetItemLengthWithSpacing(layoutOrient);
        }

        protected float GetScrollDelta(LayoutOrient layoutOrient)
        {
            float contentPos = GetContentPos(layoutOrient);

            float deltaSign = GetItemDeltaSign(layoutOrient);

            //deltaSign > 0 表示向下对齐或向左对齐
            //contentPos > 0 表示已滑动下边界或已滑动左边界
            //继续往边界滑动，就不触发偏移修正
            if ((contentPos > 0 && deltaSign > 0) || (contentPos < 0 && deltaSign < 0))
            {
                return 0;
            }

            return Math.Abs(GetContentPos(layoutOrient)) - Math.Abs(GetInitContentPos(layoutOrient));

        }

        protected int GetItemMaxShowCount(LayoutOrient layoutOrient)
        {
            if (layoutOrient == LayoutOrient.Fixed &&
                (m_LayoutGroupType == LayoutGroupType.Vertical ||
                 m_LayoutGroupType == LayoutGroupType.Horizontal))
            {
                return 1;
            }

            //int itemInShow = Mathf.CeilToInt((GetShowLength(layoutOrient)) / (GetItemLengthWithSpacing(layoutOrient) + .0f));
            int itemInShow = Mathf.FloorToInt((GetShowLength(layoutOrient)) / (GetMinItemLengthWithSpacing(layoutOrient) + .0f));

            //当显示区域扣除n个（最大可包含）item的总长度，剩余长度如果大于2个间隔，说明最多可显示n+2个
            float remainder = GetRemainder(layoutOrient, itemInShow);

            if (remainder > 2 * GetSpacingLength(layoutOrient))
            {
                return itemInShow += 2;
            }
            else
            {
                return itemInShow + 1;
            }
        }

        protected int GetItemShowCount(LayoutOrient layoutOrient)
        {
            return Math.Min(GetItemMaxShowCount(layoutOrient), GetItemCount(layoutOrient));
        }

        private float GetRemainder(LayoutOrient layoutOrient, int count)
        {
            return GetShowLength(layoutOrient) - (count * GetItemLength(layoutOrient) + (count - 1) * GetSpacingLength(layoutOrient));
        }

        protected int GetItemCount(LayoutOrient layoutOrient)
        {
            return GetItemCount(layoutOrient, GetItemCount());
        }

        //content中最多能包含几个item
        protected int GetItemCount(LayoutOrient layoutOrient, int itemCount)
        {
            switch (layoutOrient)
            {
                case LayoutOrient.Extend:
                    return Mathf.CeilToInt(itemCount / (GetItemCount(LayoutOrient.Fixed) + .0f));

                case LayoutOrient.Fixed:

                    if (m_LayoutGroupType == LayoutGroupType.Grid)
                    {
                        //因为item的总长度是由n个item长度+n-1个item间隔组成，所以content长度还要加个1个item间隔
                        return Mathf.FloorToInt((GetContentLengthExceptPadding(LayoutOrient.Fixed) + GetSpacingLength(LayoutOrient.Fixed)) / (GetItemLengthWithSpacing(LayoutOrient.Fixed) + .0f));

                    }
                    else
                    {
                        return 1;
                    }

                default:
                    return 0;
            }
        }

        protected float GetMinItemLengthWithSpacing(LayoutOrient layoutOrient)
        {
            return GetMinItemLength(layoutOrient) + GetSpacingLength(layoutOrient);
        }

        protected float GetItemLengthWithSpacing(LayoutOrient layoutOrient)
        {
            return GetItemLength(layoutOrient) + GetSpacingLength(layoutOrient);
        }

        protected float GetContentLengthExceptPadding(LayoutOrient layoutOrient)
        {
            return GetContentLength(layoutOrient) - GetPaddingLength(layoutOrient);
        }

        protected float GetSpacingLength(LayoutOrient layoutOrient)
        {
            switch (layoutOrient)
            {
                case LayoutOrient.Extend:
                    return m_LayoutSpacingExtend;

                case LayoutOrient.Fixed:
                    return m_LayoutSpacingFixed;

                default:
                    return 0;

            }
        }


        protected float GetExtendLength()
        {
            int itemExtendCount = GetItemCount(LayoutOrient.Extend);

            return GetPaddingLength(LayoutOrient.Extend) + GetItemExtendLength() + (itemExtendCount - 1) * GetSpacingLength(LayoutOrient.Extend);

        }

        protected float GetItemExtendLength()
        {
            if (SameItemLength)
            {

                return GetItemCount(LayoutOrient.Extend) * GetItemLength(LayoutOrient.Extend);
            }
            else
            {
                float allExtendLength = 0;

                foreach (float extendLength in m_ItemExtendLengths)
                {
                    allExtendLength += extendLength;
                }

                return allExtendLength;

            }
        }

        protected float GetInitContentPos(LayoutOrient layoutOrient)
        {

            switch (layoutOrient)
            {
                case LayoutOrient.Extend:
                    return m_InitContentExtendPos;

                case LayoutOrient.Fixed:
                    return m_InitContentFixedPos;

                default:
                    return 0;
            }
        }

        protected bool CheckLayoutGroupChildAlignment(Aligment? aligment)
        {
            if (aligment == null)
                return false;

            switch (aligment)
            {
                case Aligment.Left:
                    if (m_LayoutGroup.childAlignment == TextAnchor.UpperLeft ||
                        m_LayoutGroup.childAlignment == TextAnchor.MiddleLeft ||
                        m_LayoutGroup.childAlignment == TextAnchor.LowerLeft)
                    {

                        return true;
                    }

                    break;
                case Aligment.Right:
                    if (m_LayoutGroup.childAlignment == TextAnchor.UpperRight ||
                        m_LayoutGroup.childAlignment == TextAnchor.MiddleRight ||
                        m_LayoutGroup.childAlignment == TextAnchor.LowerRight)
                    {

                        return true;
                    }
                    break;
                case Aligment.Top:
                    if (m_LayoutGroup.childAlignment == TextAnchor.UpperLeft ||
                        m_LayoutGroup.childAlignment == TextAnchor.UpperCenter ||
                        m_LayoutGroup.childAlignment == TextAnchor.UpperRight)
                    {

                        return true;
                    }
                    break;
                case Aligment.Bottom:
                    if (m_LayoutGroup.childAlignment == TextAnchor.LowerLeft ||
                        m_LayoutGroup.childAlignment == TextAnchor.LowerCenter ||
                        m_LayoutGroup.childAlignment == TextAnchor.LowerRight)
                    {

                        return true;
                    }
                    break;
                default:
                    return false;

            }

            return false;
        }

        protected virtual void CheckIndex(ref int index)
        {
            if (GetItemCount() <= 0)
                return;

            switch (m_LayoutGroupType)
            {
                case LayoutGroupType.Vertical:
                    SkipUnshowIndex(ref index, LayoutOrient.Extend);

                    break;
                case LayoutGroupType.Horizontal:
                    SkipUnshowIndex(ref index, LayoutOrient.Extend);
                    break;

            }

        }

        protected virtual void SetContentSize()
        {
            //CheckResetItemInitialPos();
        }

        protected void SkipUnshowIndex(ref int index, LayoutOrient layoutOrient)
        {
            int nowGroup = layoutOrient == LayoutOrient.Extend ? m_NowExtendGroup : m_NowFixedGroup;
            int remainder = index % GetItemCount(layoutOrient);
            int loop = 0;

            while (remainder > nowGroup + GetItemShowCount(layoutOrient) ||
                   remainder < nowGroup)
            {
                index++;
                remainder = index % GetItemCount(layoutOrient);

                if (++loop > 100) { Debug.LogWarning("SkipUnshowIndex Loop:" + nowGroup + "--" + GetItemShowCount(layoutOrient) + "--" + remainder); break; }
            }

        }

        protected void InteralRefreshItems()
        {
            SetContentSize();

            if (GetItemCount() <= 0)
            {
                Reset();
                CheckAndRefreshLayout();
                return;
            }

            Reset();
            m_CheckRefreshEnd = GetItemLengthWithSpacing(LayoutOrient.Extend, 0);
            CheckAndRefreshLayout();

        }

        protected void Reset()
        {
            m_CheckRefreshStart = 0;
            m_CheckRefreshEnd = 0;
            m_itemExtendDeltaPos = 0;
            m_NowExtendGroup = 0;
        }

        protected void CheckResetItemInitialPos()
        {

            if (!m_Init)
                return;

            Aligment extendStartAligment = GetAligment(LayoutOrient.Extend, AligmentTag.Start);
            if (CheckLayoutGroupChildAlignment(extendStartAligment))
                return;

            //判断右下对齐方式的布局，尚未完善
            /*
            float extendDelta = GetExtendLength() - GetContentLength(LayoutOrient.Extend);

            switch (m_LayoutGroupType)
            {
                case LayoutGroupType.Grid:
                    if (extendDelta > 0.1f)
                    {
                        //ResetItemInitialPos(-extendDelta);
                    }
                    break;
                case LayoutGroupType.Vertical:
                case LayoutGroupType.Horizontal:
                    int deltaCount = GetItemCount(LayoutOrient.Extend) - GetItemShowCount(LayoutOrient.Extend);

                    if (deltaCount > 0)
                    {
                        //ResetItemInitialPos(-extendDelta);
                    }

                    break;
            }
            */
        }

        protected void ResetItemInitialPos(float extendDelta)
        {
            if (!m_Init)
                return;

            for (int i = 0; i < Items.Count; i++)
            {
                Items[i].InitialPosition = GetNewInitialPos(Items[i].InitialPosition, extendDelta);

            }
        }

        protected float GetMinItemLength(LayoutOrient layoutOrient)
        {
            if (SameItemLength)
                return GetItemLength(layoutOrient);
            else
                //待支持LayoutOrient.Fixed
                return m_MinItemExtendLength;


        }

        protected abstract void SetInitContentPos();

        protected abstract Vector2 GetItemDeltaPos();

        protected abstract void SetFixedItemCount();

        protected abstract float GetShowLength(LayoutOrient layoutOrient);

        protected abstract float GetContentPos(LayoutOrient layoutOrient);

        protected abstract float GetItemLength(LayoutOrient layoutOrient, int index = 0);

        protected abstract float GetContentLength(LayoutOrient layoutOrient);

        protected abstract float GetPaddingLength(LayoutOrient layoutOrient);

        protected abstract int GetStartIndex();

        protected abstract Aligment GetAligment(LayoutOrient layoutOrient, AligmentTag aligmentTag);

        protected abstract Vector2 GetNewInitialPos(Vector2 oriInitialPos, float extendDelta);

        protected abstract Vector2 GetEndPos();
    }

}

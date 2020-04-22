using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIExtension
{
    public struct OrientValue<T> {
        public T Extend;
        public T Fixed;
    }

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

        protected GameObject m_ItemGo;

        protected int m_ItemShowCount;

        protected Action<int, GameObject> m_ItemRefresh;

        protected ScrollRect m_ScrollRect;
        protected LayoutGroup m_LayoutGroup;

        protected float m_LayoutTop;
        protected float m_LayoutBottom;
        protected float m_LayoutLeft;
        protected float m_LayoutRight;
        protected OrientValue<float> m_LayoutSpacing = new OrientValue<float>();

        private List<ReuseItem> m_Items = new List<ReuseItem>();

        protected Vector2 m_InitItemPos;
        protected OrientValue<float> m_InitContentPos = new OrientValue<float>();
        protected OrientValue<int> m_NowGroup = new OrientValue<int>();
        protected OrientValue<float> m_itemDeltaPos = new OrientValue<float>();
        protected OrientValue<float> m_ItemDefaultSize = new OrientValue<float>();
        protected bool m_Init = false;
        protected bool m_RefreshLayout = false;

        //protected int m_ItemExtendCount;

        protected List<float> m_ItemExtendLengths;
        protected float m_CheckRefreshStart = 0;
        protected float m_CheckRefreshEnd = 0;

        protected OrientValue<float> m_MinItemLength = new OrientValue<float>() { Extend = float.MaxValue, Fixed = float.MaxValue };

        protected List<ReuseItem> Items { get => m_Items; set => m_Items = value; }

        public void AddItem()
        {
            RefreshItems(m_ItemShowCount + 1);
        }

        public void RemoveItem()
        {
            if (GetItemCount() <= 0)
                return;

            RefreshItems(m_ItemShowCount - 1);

        }

        public void RemoveAllItems()
        {
            RefreshItems(0);
        }

        public void RefreshItems(int itemCount)
        {
            m_ItemShowCount = itemCount;

            //当实际个数小于最大个数，且实际个数小于显示个数，动态生成
            int maxCount = GetItemMaxShowCount(LayoutOrient.Extend) * GetItemShowCount(LayoutOrient.Fixed);
            if (Items.Count < maxCount && Items.Count < m_ItemShowCount) {

                int generateCount = Math.Min(m_ItemShowCount - Items.Count, maxCount - Items.Count);
                GenerateItems(generateCount);
            }

            SetContentSize();

            //刷新后，当前已经滑动的位置大于contentsize，默认显示到最底部，否则当前位置不变
            int newExtendGroup = GetItemCount(LayoutOrient.Extend, itemCount) - GetItemShowCount(LayoutOrient.Extend);

            if (newExtendGroup < m_NowGroup.Extend)
            {
                SetContentPosToEnd();
            }
            else
            {
                RefreshLayout();
            }

            m_CheckRefreshEnd = m_CheckRefreshStart + GetNowItemExtendLengthWithSpacing();
        }

        public void ResetContentPos()
        {
            m_ScrollRect.content.anchoredPosition = new Vector2(0, 0);
            CheckAndRefreshLayout();
        }

        public void SetContentPosToEnd()
        {
            m_ScrollRect.content.anchoredPosition = GetEndPos();
            Debug.Log("endPos:" + GetEndPos());
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

        protected void OnDestroy()
        {
            if (!m_Init)
                return;

            CleatItems();
            m_Items.Clear();
        }

        public void InitItems(GameObject itemGo, Action<int, GameObject> itemRefresh)
        {
            m_ItemGo = itemGo;
            m_ItemRefresh = itemRefresh;

            SetMinItemSize(itemGo);

            m_ItemDefaultSize = new OrientValue<float>()
            {
                Extend = m_ItemGo.GetComponent<RectTransform>().rect.height,
                Fixed = m_ItemGo.GetComponent<RectTransform>().rect.width,
            };

            if (!InitCheck())
            {
                Debug.LogError("ReuseLayoutGroupItems Init Failed");
                return;
            }

            Init();

            CleatItems();

            m_Init = true;

        }

        protected void GenerateItems(int count)
        {
            for (int i = 0; i < count; i++) {

                GenerateItem();
            }  
        }

        protected void GenerateItem()
        {
            GameObject item = GameObject.Instantiate(m_ItemGo, m_ScrollRect.content);

            ReuseItem reuseItem = item.GetComponent<ReuseItem>();

            if (reuseItem == null)
                reuseItem = item.AddComponent<ReuseItem>();

            SetMinItemSize(item);
            reuseItem.RefreshCallback = m_ItemRefresh;
            m_Items.Add(reuseItem);

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
            m_NowGroup.Fixed = 0;
            m_NowGroup.Extend = 0;

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

                    m_LayoutSpacing.Extend = gridLayoutGroup.spacing.y;
                    m_LayoutSpacing.Fixed = gridLayoutGroup.spacing.x;

                    break;

                case LayoutGroupType.Vertical:
                    VerticalLayoutGroup verticalLayoutGroup = layoutGroup as VerticalLayoutGroup;

                    m_LayoutSpacing.Extend = verticalLayoutGroup.spacing;
                    m_LayoutSpacing.Fixed = 0;

                    break;

                case LayoutGroupType.Horizontal:
                    HorizontalLayoutGroup horizontalLayoutGroup = layoutGroup as HorizontalLayoutGroup;

                    m_LayoutSpacing.Extend = horizontalLayoutGroup.spacing;
                    m_LayoutSpacing.Fixed = 0;

                    break;
            }

        }

        protected void OnScrollRectValueChanged(Vector2 offset)
        {
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
                //Debug.LogWarning("CheckRefreshLayout:" + m_NowGroup.Extend + "--" + GetItemCount() + "--" + extendScrollDelta + "--" + m_CheckRefreshStart + "--" + m_CheckRefreshEnd);

                if (++loop >= 1000)
                {
                    Debug.LogWarning("CheckRefreshLayout Loop:" + m_NowGroup.Extend + "--" + GetItemCount() + "--" + extendScrollDelta + "--" + m_CheckRefreshStart + "--" + m_CheckRefreshEnd);
                    break;
                }

                if (m_NowGroup.Extend < GetItemCount() && m_CheckRefreshEnd != m_CheckRefreshStart && extendScrollDelta >= m_CheckRefreshEnd)
                {
                    //Debug.Log("Arefresh1:" + m_NowExtendGroup + "--" + GetItemCount() + "--" + extendScrollDelta + "--" + m_CheckRefreshStart + "--" + m_CheckRefreshEnd);

                    m_itemDeltaPos.Extend = GetItemDeltaSign(LayoutOrient.Extend) * m_CheckRefreshEnd;
                    m_CheckRefreshStart = m_CheckRefreshEnd;
                    m_NowGroup.Extend++;
                    m_CheckRefreshEnd += GetNowItemExtendLengthWithSpacing();
                    refresh = true;
                    //Debug.Log("Brefresh1:" + m_NowExtendGroup + "--" + GetItemCount() + "--" + extendScrollDelta + "--" + m_CheckRefreshStart + "--" + m_CheckRefreshEnd);;
                    //Debug.Log("re：" + m_itemDeltaPos.Extend);
                }
                else if (m_NowGroup.Extend > 0 && extendScrollDelta < m_CheckRefreshStart)
                {
                    m_CheckRefreshEnd = m_CheckRefreshStart;
                    m_NowGroup.Extend--;
                    m_CheckRefreshStart -= GetNowItemExtendLengthWithSpacing();
                    m_itemDeltaPos.Extend = GetItemDeltaSign(LayoutOrient.Extend) * m_CheckRefreshStart;
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

                if (fixedGroup != m_NowGroup.Fixed || m_NowGroup.Fixed == 0)
                {
                    m_NowGroup.Fixed = fixedGroup;
                    m_itemDeltaPos.Fixed = GetItemDeltaPos(fixedGroup, LayoutOrient.Fixed);
                    refresh = true;
                }

            }

            return refresh;

        }

        protected float GetNowItemExtendLengthWithSpacing()
        {
            return GetItemLengthWithSpacing(LayoutOrient.Extend, m_NowGroup.Extend);
        }

        protected float GetItemLengthWithSpacing(LayoutOrient layoutOrient, int index)
        {
            float itemLength = GetItemLength(layoutOrient, index);

            return itemLength == 0 ? 0 : itemLength + GetSpacingLength(LayoutOrient.Extend);
        }

        protected int GetItemCount()
        {
            return m_ItemShowCount;
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
            m_LayoutGroup.padding.left = (int)(m_LayoutLeft + deltaPos.x);

            m_LayoutGroup.enabled = false;
            m_LayoutGroup.enabled = true;

            int index = GetStartIndex();

            for (int i = 0; i < m_Items.Count; i++)
            {
                ReuseItem child = m_Items[i];

                //当滑动到固定方向中间或扩展方向中间，需要跳过之前、之后的item
                CheckIndex(ref index);
                //GetItemSize(index), 
                child.Refresh(index, index < GetItemCount());
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
                    return m_LayoutSpacing.Extend;

                case LayoutOrient.Fixed:
                    return m_LayoutSpacing.Fixed;

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
            return GetItemCount(LayoutOrient.Extend) * GetItemLength(LayoutOrient.Extend);
        }

        protected float GetInitContentPos(LayoutOrient layoutOrient)
        {

            switch (layoutOrient)
            {
                case LayoutOrient.Extend:
                    return m_InitContentPos.Extend;

                case LayoutOrient.Fixed:
                    return m_InitContentPos.Fixed;

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
            int nowGroup = layoutOrient == LayoutOrient.Extend ? m_NowGroup.Extend : m_NowGroup.Fixed;
            int remainder = index % GetItemCount(layoutOrient);
            int loop = 0;

            while (remainder >= nowGroup + GetItemShowCount(layoutOrient) ||
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
            m_itemDeltaPos.Extend = 0;
            m_NowGroup.Extend = 0;
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
            switch (layoutOrient) {

                case LayoutOrient.Extend:
                    return m_MinItemLength.Extend;

                case LayoutOrient.Fixed:
                    return m_MinItemLength.Fixed;

                default:
                    return 0;
            }

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

        protected abstract Vector2 GetItemSize(int itemIndex);

        protected abstract void SetMinItemSize(GameObject itemGo);
    }

}

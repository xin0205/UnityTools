using UnityEngine;
using UnityEngine.UI;

namespace UGUIExtension
{
    public class ReuseLayoutGroupItemsHorizontal : ReuseLayoutGroupItems
    {
        protected override void InitLayout()
        {
            base.InitLayout();

            switch (m_LayoutGroupType)
            {
                case LayoutGroupType.Grid:
                    GridLayoutGroup gridLayoutGroup = (GridLayoutGroup)m_LayoutGroup;

                    m_LayoutSpacing.Extend = gridLayoutGroup.spacing.x;
                    m_LayoutSpacing.Fixed = gridLayoutGroup.spacing.y;

                    break;

                case LayoutGroupType.Vertical:
                    break;

                case LayoutGroupType.Horizontal:
                    break;
            }
        }

        protected override void SetInitContentPos()
        {
            if (CheckLayoutGroupChildAlignment(GetAligment(LayoutOrient.Extend, AligmentTag.Start)))
            {
                m_InitContentPos.Extend = -m_LayoutLeft;
            }

            if (CheckLayoutGroupChildAlignment(GetAligment(LayoutOrient.Extend, AligmentTag.End)))
            {
                m_InitContentPos.Extend = m_LayoutRight;
            }

            if (CheckLayoutGroupChildAlignment(GetAligment(LayoutOrient.Fixed, AligmentTag.Start)))
            {
                m_InitContentPos.Fixed = -m_LayoutTop;
            }

            if (CheckLayoutGroupChildAlignment(GetAligment(LayoutOrient.Fixed, AligmentTag.End)))
            {
                m_InitContentPos.Fixed = m_LayoutBottom;
            }
        }

        protected override void SetContentSize()
        {
            base.SetContentSize();
            m_ScrollRect.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, GetExtendLength());
            m_ScrollRect.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, GetContentLength(LayoutOrient.Fixed));
        }

        protected override Vector2 GetItemDeltaPos()
        {
            return new Vector2(m_itemDeltaPos.Extend, m_itemDeltaPos.Fixed);
        }

        protected override void SetFixedItemCount()
        {
            if (m_LayoutGroupType != LayoutGroupType.Grid)
            {
                return;
            }

            GridLayoutGroup gridLayoutGroup = m_LayoutGroup as GridLayoutGroup;
            gridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedRowCount;
            gridLayoutGroup.constraintCount = GetItemShowCount(LayoutOrient.Fixed);
        }

        protected override float GetShowLength(LayoutOrient layoutOrient)
        {
            switch (layoutOrient)
            {
                case LayoutOrient.Extend:
                    return m_ScrollRect.GetComponent<RectTransform>().rect.width;

                case LayoutOrient.Fixed:
                    return m_ScrollRect.GetComponent<RectTransform>().rect.height;
                default:
                    return 0;
            }
        }

        protected override float GetContentPos(LayoutOrient layoutOrient)
        {
            switch (layoutOrient)
            {
                case LayoutOrient.Extend:
                    return m_ScrollRect.content.anchoredPosition.x;

                case LayoutOrient.Fixed:
                    return m_ScrollRect.content.anchoredPosition.y;

                default:
                    return 0;

            }
        }

        protected override float GetItemLength(LayoutOrient layoutOrient, int index)
        {
            ReuseItem reuseItem = Items.Find((item) => item.Index == index);

            if (reuseItem == null)
            {

                if (Items.Count > 0)
                {
                    reuseItem = Items[0];
                }
                else
                {
                    switch (layoutOrient)
                    {
                        case LayoutOrient.Extend:
                            return m_ItemDefaultSize.Extend;

                        case LayoutOrient.Fixed:
                            return m_ItemDefaultSize.Fixed;

                        default:
                            return 0;
                    }
                }
            }

            switch (layoutOrient)
            {
                case LayoutOrient.Extend:
                    return reuseItem.GetComponent<RectTransform>().rect.width;

                case LayoutOrient.Fixed:
                    return reuseItem.GetComponent<RectTransform>().rect.height;

                default:
                    return 0;
            }
        }

        protected override float GetContentLength(LayoutOrient layoutOrient)
        {
            switch (layoutOrient)
            {
                case LayoutOrient.Extend:
                    return m_ScrollRect.content.rect.width;

                case LayoutOrient.Fixed:
                    return m_ScrollRect.content.rect.height;

                default:
                    return 0;

            }
        }

        protected override float GetPaddingLength(LayoutOrient layoutOrient)
        {
            switch (layoutOrient)
            {
                case LayoutOrient.Extend:
                    return m_LayoutLeft + m_LayoutRight;

                case LayoutOrient.Fixed:
                    return m_LayoutTop + m_LayoutBottom;

                default:
                    return 0;

            }
        }

        protected override int GetStartIndex()
        {
            switch (m_LayoutGroupType)
            {
                case LayoutGroupType.Grid:
                    GridLayoutGroup gridLayoutGroup = (GridLayoutGroup)m_LayoutGroup;
                    switch (gridLayoutGroup.startAxis)
                    {
                        case GridLayoutGroup.Axis.Vertical:
                            return m_NowGroup.Extend * GetItemCount(LayoutOrient.Fixed) + m_NowGroup.Fixed;

                        case GridLayoutGroup.Axis.Horizontal:
                            return m_NowGroup.Fixed * GetItemCount(LayoutOrient.Extend) + m_NowGroup.Extend;


                        default:
                            return 0;
                    }

                case LayoutGroupType.Horizontal:
                    return m_NowGroup.Fixed * GetItemCount(LayoutOrient.Extend) + m_NowGroup.Extend;
                case LayoutGroupType.Vertical:
                    return 0;

            }
            return 0;
        }

        protected override void CheckIndex(ref int index)
        {
            base.CheckIndex(ref index);

            if (GetItemCount() <= 0)
                return;

            switch (m_LayoutGroupType)
            {
                case LayoutGroupType.Grid:
                    GridLayoutGroup gridLayoutGroup = (GridLayoutGroup)m_LayoutGroup;
                    switch (gridLayoutGroup.startAxis)
                    {
                        case GridLayoutGroup.Axis.Vertical:
                            SkipUnshowIndex(ref index, LayoutOrient.Fixed);
                            break;

                        case GridLayoutGroup.Axis.Horizontal:
                            SkipUnshowIndex(ref index, LayoutOrient.Extend);
                            break;

                        default:
                            break;
                    }
                    break;

            }
        }

        protected override Aligment GetAligment(LayoutOrient layoutOrient, AligmentTag aligmentTag)
        {
            switch (layoutOrient)
            {
                case LayoutOrient.Fixed:
                    if (aligmentTag == AligmentTag.Start)
                        return Aligment.Top;
                    if (aligmentTag == AligmentTag.End)
                        return Aligment.Bottom;
                    break;

                case LayoutOrient.Extend:
                    if (aligmentTag == AligmentTag.Start)
                        return Aligment.Left;
                    if (aligmentTag == AligmentTag.End)
                        return Aligment.Right;
                    break;

                default:
                    return Aligment.Left;

            }

            return Aligment.Left;
        }

        protected override Vector2 GetNewInitialPos(Vector2 oriInitialPos, float extendDelta)
        {
            Vector3 pos = oriInitialPos;
            pos.x += extendDelta;
            return pos;
        }

        protected override Vector2 GetEndPos()
        {

            float contentLength = GetContentLength(LayoutOrient.Extend);
            float showLength = GetShowLength(LayoutOrient.Extend);

            if (contentLength < showLength)
            {
                return new Vector2(0, 0);
            }
            else
            {
                bool isStartAligment = CheckAligment(LayoutOrient.Extend, AligmentTag.Start);

                float endPos = isStartAligment ? -contentLength + showLength : contentLength - showLength;

                return new Vector2(endPos, 0);
            }
        }

        protected override Vector2 GetItemSize(int itemIndex)
        {
            return new Vector2(GetItemLength(LayoutOrient.Extend, itemIndex), GetItemLength(LayoutOrient.Fixed, itemIndex));
        }

        protected override void SetMinItemSize(GameObject itemGo)
        {
            m_MinItemLength.Extend = itemGo.GetComponent<RectTransform>().rect.width; 
            m_MinItemLength.Fixed = itemGo.GetComponent<RectTransform>().rect.height;
        }
    }

}

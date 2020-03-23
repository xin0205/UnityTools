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

                    m_LayoutSpacingExtend = gridLayoutGroup.spacing.x;
                    m_LayoutSpacingFixed = gridLayoutGroup.spacing.y;

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
                m_InitContentExtendPos = -m_LayoutLeft;
            }

            if (CheckLayoutGroupChildAlignment(GetAligment(LayoutOrient.Extend, AligmentTag.End)))
            {
                m_InitContentExtendPos = m_LayoutRight;
            }

            if (CheckLayoutGroupChildAlignment(GetAligment(LayoutOrient.Fixed, AligmentTag.Start)))
            {
                m_InitContentFixedPos = -m_LayoutTop;
            }

            if (CheckLayoutGroupChildAlignment(GetAligment(LayoutOrient.Fixed, AligmentTag.End)))
            {
                m_InitContentFixedPos = m_LayoutBottom;
            }
        }

        protected override void SetContentSize()
        {
            m_ScrollRect.content.sizeDelta = new Vector2(GetExtendLength(), GetContentLength(LayoutOrient.Fixed));
        }

        protected override Vector2 GetItemDeltaPos()
        {
            return new Vector2(m_itemExtendDeltaPos, m_itemFixedDeltaPos);
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
                    return m_ScrollRect.GetComponent<RectTransform>().sizeDelta.x;

                case LayoutOrient.Fixed:
                    return m_ScrollRect.GetComponent<RectTransform>().sizeDelta.y;
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
            if (SameItemLength)
            {
                switch (layoutOrient)
                {
                    case LayoutOrient.Extend:
                        return m_ItemPrefab.Width;

                    case LayoutOrient.Fixed:
                        return m_ItemPrefab.Height;

                    default:
                        return 0;
                }
            }
            else
            {

                return (index < 0 || index >= GetItemCount()) ? 0 : m_ItemExtendLengths[index];
            }
        }

        protected override float GetContentLength(LayoutOrient layoutOrient)
        {
            switch (layoutOrient)
            {
                case LayoutOrient.Extend:
                    return m_ScrollRect.content.sizeDelta.x;

                case LayoutOrient.Fixed:
                    return m_ScrollRect.content.sizeDelta.y;

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
                            return m_NowExtendGroup * GetItemCount(LayoutOrient.Fixed) + m_NowFixedGroup;

                        case GridLayoutGroup.Axis.Horizontal:
                            return m_NowFixedGroup * GetItemCount(LayoutOrient.Extend) + m_NowExtendGroup;


                        default:
                            return 0;
                    }

                case LayoutGroupType.Horizontal:
                    return m_NowFixedGroup * GetItemCount(LayoutOrient.Extend) + m_NowExtendGroup;
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
    }

}

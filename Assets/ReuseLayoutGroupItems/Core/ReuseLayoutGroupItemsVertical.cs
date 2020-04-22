using System;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIExtension
{
    public class ReuseLayoutGroupItemsVertical : ReuseLayoutGroupItems
    {
        protected override void InitLayout()
        {
            base.InitLayout();

            switch (m_LayoutGroupType)
            {
                case LayoutGroupType.Grid:
                    GridLayoutGroup gridLayoutGroup = m_LayoutGroup as GridLayoutGroup;

                    m_LayoutSpacing.Extend = gridLayoutGroup.spacing.y;
                    m_LayoutSpacing.Fixed = gridLayoutGroup.spacing.x;

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
                m_InitContentPos.Extend = -m_LayoutTop;
            }

            if (CheckLayoutGroupChildAlignment(GetAligment(LayoutOrient.Extend, AligmentTag.End)))
            {
                m_InitContentPos.Extend = m_LayoutBottom;
            }

            if (CheckLayoutGroupChildAlignment(GetAligment(LayoutOrient.Fixed, AligmentTag.Start)))
            {
                m_InitContentPos.Fixed = -m_LayoutLeft;
            }

            if (CheckLayoutGroupChildAlignment(GetAligment(LayoutOrient.Fixed, AligmentTag.End)))
            {
                m_InitContentPos.Fixed = m_LayoutRight;
            }
        }

        protected override void SetContentSize()
        {
            base.SetContentSize();
            m_ScrollRect.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, GetExtendLength());
            m_ScrollRect.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, GetContentLength(LayoutOrient.Fixed));
        }

        protected override Vector2 GetItemDeltaPos()
        {
            return new Vector2(m_itemDeltaPos.Fixed, m_itemDeltaPos.Extend);
        }

        protected override void SetFixedItemCount()
        {
            if (m_LayoutGroupType != LayoutGroupType.Grid)
            {
                return;
            }

            GridLayoutGroup gridLayoutGroup = m_LayoutGroup as GridLayoutGroup;
            gridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayoutGroup.constraintCount = GetItemShowCount(LayoutOrient.Fixed);
        }

        protected override float GetShowLength(LayoutOrient layoutOrient)
        {
            switch (layoutOrient)
            {
                case LayoutOrient.Extend:
                    return m_ScrollRect.GetComponent<RectTransform>().rect.height;

                case LayoutOrient.Fixed:
                    return m_ScrollRect.GetComponent<RectTransform>().rect.width;

                default:
                    return 0;
            }
        }

        protected override float GetContentPos(LayoutOrient layoutOrient)
        {
            switch (layoutOrient)
            {
                case LayoutOrient.Extend:
                    return m_ScrollRect.content.anchoredPosition.y;

                case LayoutOrient.Fixed:
                    return m_ScrollRect.content.anchoredPosition.x;

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
                    return reuseItem.GetComponent<RectTransform>().rect.height;

                case LayoutOrient.Fixed:
                    return reuseItem.GetComponent<RectTransform>().rect.width;

                default:
                    return 0;
            }

        }

        protected override float GetContentLength(LayoutOrient layoutOrient)
        {
            switch (layoutOrient)
            {
                case LayoutOrient.Extend:
                    return m_ScrollRect.content.rect.height;

                case LayoutOrient.Fixed:
                    return m_ScrollRect.content.rect.width;

                default:
                    return 0;

            }
        }

        protected override float GetPaddingLength(LayoutOrient layoutOrient)
        {
            switch (layoutOrient)
            {
                case LayoutOrient.Extend:
                    return m_LayoutTop + m_LayoutBottom;

                case LayoutOrient.Fixed:
                    return m_LayoutLeft + m_LayoutRight;

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
                            return m_NowGroup.Fixed * GetItemCount(LayoutOrient.Extend) + m_NowGroup.Extend;

                        case GridLayoutGroup.Axis.Horizontal:
                            return m_NowGroup.Extend * GetItemCount(LayoutOrient.Fixed) + m_NowGroup.Fixed;

                        default:
                            return 0;
                    }
                case LayoutGroupType.Vertical:
                    return m_NowGroup.Fixed * GetItemCount(LayoutOrient.Extend) + m_NowGroup.Extend;

                case LayoutGroupType.Horizontal:
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
                            SkipUnshowIndex(ref index, LayoutOrient.Extend);
                            break;

                        case GridLayoutGroup.Axis.Horizontal:
                            SkipUnshowIndex(ref index, LayoutOrient.Fixed);
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
                        return Aligment.Left;
                    if (aligmentTag == AligmentTag.End)
                        return Aligment.Right;
                    break;

                case LayoutOrient.Extend:
                    if (aligmentTag == AligmentTag.Start)
                        return Aligment.Top;
                    if (aligmentTag == AligmentTag.End)
                        return Aligment.Bottom;
                    break;

                default:
                    return Aligment.Left;

            }

            return Aligment.Left;

        }

        protected override Vector2 GetNewInitialPos(Vector2 oriInitialPos, float extendDelta)
        {
            Vector3 pos = oriInitialPos;
            pos.y += extendDelta;
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

                float endPos = isStartAligment ? contentLength - showLength : -contentLength + showLength;

                return new Vector2(0, endPos);
            }
        }

        protected override Vector2 GetItemSize(int itemIndex)
        {
            return new Vector2(GetItemLength(LayoutOrient.Fixed, itemIndex), GetItemLength(LayoutOrient.Extend, itemIndex));
        }

        protected override void SetMinItemSize(GameObject itemGo)
        {
            m_MinItemLength.Extend = Math.Min(m_MinItemLength.Extend, itemGo.GetComponent<RectTransform>().rect.height);
            m_MinItemLength.Fixed = Math.Min(m_MinItemLength.Fixed, itemGo.GetComponent<RectTransform>().rect.width);

        }

        protected override void SetCellSize() {
            ((GridLayoutGroup)m_LayoutGroup).cellSize = new Vector2(m_ItemDefaultSize.Fixed, m_ItemDefaultSize.Extend);

        }
    }

}

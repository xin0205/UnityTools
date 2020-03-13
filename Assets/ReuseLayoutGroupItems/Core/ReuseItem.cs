using System;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIExtension
{
    public class ReuseItem : MonoBehaviour
    {
        private float m_width;
        private float m_height;

        private int m_Index;

        private Action<int, ReuseItem> m_RefreshCallback;

        private Vector2 m_InitialPosition;

        private RectTransform m_RectTransform;

        public float Width { get => m_width; set => m_width = value; }
        public float Height { get => m_height; set => m_height = value; }
        public Action<int, ReuseItem> RefreshCallback { get => m_RefreshCallback; set => m_RefreshCallback = value; }
        public Vector2 InitialPosition { get => m_InitialPosition; set => m_InitialPosition = value; }
        public int Index { get => m_Index; set => m_Index = value; }

        public void Awake()
        {
            m_RectTransform = GetComponent<RectTransform>();
        }

        public void Refresh(int index, Vector2 pos, bool callback = true)
        {
            Index = index;

            gameObject.SetActive(callback);

            m_RectTransform.anchoredPosition = m_InitialPosition + pos;

            if (callback && m_RefreshCallback != null)
            {
                m_RefreshCallback(index, this);

            }
        }

    }

}



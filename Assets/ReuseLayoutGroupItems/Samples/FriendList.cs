using UGUIExtension;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Lin {
    public class FriendList : MonoBehaviour
    {
        [SerializeField]
        private ReuseLayoutGroupItems m_ReuseScrollRect;

        [SerializeField]
        private ReuseItem m_ReuseItem;

        [SerializeField]
        private InputField m_InputField;

        // Start is called before the first frame update
        void Start()
        {
            m_ReuseScrollRect.InitItems(m_ReuseItem, 100, 90, 0, (index, scrollRectItem) =>
            {
                scrollRectItem.GetComponent<FriendItem>().text.text = "Friend " + index;
            });
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void AddItem()
        {
            m_ReuseScrollRect.AddItem();
        }

        public void RemoveItem()
        {
            m_ReuseScrollRect.RemoveItem();
        }

        public void RemoveAllItems()
        {
            m_ReuseScrollRect.RemoveAllItems();
        }

        public void RefreshItem() {

            int itemCount = int.Parse(m_InputField.text);

            m_ReuseScrollRect.RefreshItems(itemCount);

            m_ReuseScrollRect.ResetContentPos();
        }

    }

}


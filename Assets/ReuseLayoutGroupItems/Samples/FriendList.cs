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
        private GameObject m_ReuseGo;

        [SerializeField]
        private InputField m_InputField;

        // Start is called before the first frame update
        void Start()
        {
            //m_ReuseScrollRect.InitItems(m_ReuseItem, 100, 90, 0, (index, scrollRectItem) =>
            //{
            //    scrollRectItem.GetComponent<FriendItem>().text.text = "Friend " + index;
            //});


            //List<float> itemLengths = new List<float>() {
            //    50,  150, 300,  100, 110,
            //    70,  120, 70,  290,  130,
            //    100, 50,  260, 90,  120,
            //    270,  60,  90,  350,  190,

            //};

            //m_ReuseScrollRect.InitItems(m_ReuseItem, itemLengths, 0, (index, scrollRectItem) =>
            //{
            //    scrollRectItem.GetComponent<FriendItem>().text.text = "Friend " + index;
            //});

            //m_ReuseScrollRect.InitItems(m_ReuseItem, 100, 90, 0, (index, scrollRectItem) =>
            //{
            //    scrollRectItem.GetComponent<FriendItem>().text.text = "Friend " + index;
            //});

            //m_ReuseScrollRect.InitItems(m_ReuseGo, 100, 90, 0, (index, scrollRectItem) =>
            //{
            //    scrollRectItem.GetComponent<FriendItem>().text.text = "Friend " + index;
            //});

            m_ReuseScrollRect.InitItems(m_ReuseGo, (index, scrollRectItem) =>
            {
                scrollRectItem.GetComponent<FriendItem>().text.text = "Friend " + index;
            });

            m_ReuseScrollRect.RefreshItems(100);
        }

        // Update is called once per frame
        void Update()
        {

        }

        

        public void RemoveItem()
        {
            m_ReuseScrollRect.RemoveItem();
        }

        public void RemoveAllItems()
        {
            m_ReuseScrollRect.RemoveAllItems();
        }

        public void AddItem()
        {
            m_ReuseScrollRect.AddItem();
            m_ReuseScrollRect.SetContentPosToEnd();
        }

        public void RefreshItem()
        {
            //Random.Range(1, 30)
            int itemCount = int.Parse(m_InputField.text);

            m_ReuseScrollRect.RefreshItems(itemCount);

            //m_ReuseScrollRect.ResetContentPos();
        }

        //public void AddItem()
        //{
        //    m_ReuseScrollRect.AddItem(Random.Range(50, 200));
        //    m_ReuseScrollRect.SetContentPosToEnd();
        //}

        //public void RefreshItem()
        //{
        //    m_ReuseScrollRect.RefreshItems(GetRandomList());
        //    m_ReuseScrollRect.SetContentPosToEnd();
        //}

        public static List<int> countList = new List<int>{ 10, 1, 20, 0, 4, 1, 10 };
        public static int listIndex = 0;
        public List<float> GetRandomList()
        {
            int count = countList[++listIndex >= countList.Count ? 0 : listIndex];// Random.Range(0, 15);

            List<float> itemLengths = new List<float>();

            for (int i = 0; i < count; i++) {

                itemLengths.Add(Random.Range(50, 200));
            }

            return itemLengths;

        }

    }

}


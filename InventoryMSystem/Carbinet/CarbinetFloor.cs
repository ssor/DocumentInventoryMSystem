using System.Collections.Generic;
using System.Diagnostics;

namespace InventoryMSystem
{
    // 柜子的层
    public class CarbinetFloor
    {
        #region Members

        public int maxDocNumber = -1;
        private int height = 104;

        public int Height
        {
            get { return height; }
            set { height = value; }
        }
        private int width = 300;

        public int Width
        {
            get { return width; }
            set { width = value; }
        }
        // 相对于柜子的位置
        private int left = 40;//放置档案的最左边位置
        private int top = 33;//层的顶置
        public int Left
        {
            get { return left; }
            set { left = value; }
        }


        public int Top
        {
            get { return top; }
            set { top = value; }
        }
        //保存添加的档案
        public List<DocumentFile> documentList = new List<DocumentFile>();
        public int floorNumber;
        public static int DOCUMENT_GAP = 2;//档案间的间隔
        System.Windows.Forms.Control.ControlCollection controls;
        List<DocumentChair> chairsList = new List<DocumentChair>();

        public Carbinet carbinet = null;

        #endregion

        public CarbinetFloor(Carbinet carbinet, int floorNumber, System.Windows.Forms.Control.ControlCollection controls)
        {
            this.floorNumber = floorNumber;
            this.controls = controls;
        }
        /// <summary>
        ///   获取指定索引前档案的宽度总和
        /// </summary>
        /// <param name="endIndex"></param>
        /// <returns></returns>
        public int getDocumentsTotalWidth(int endIndex)
        {
            int totalWidth = 0;
            if (endIndex == -1)
            {
                endIndex = this.documentList.Count;
            }
            for (int i = 0; i < endIndex; i++)
            {
                totalWidth += this.documentList[i].Width;
            }
            //if (this.documentList.Count > 0)
            //{
            //    foreach (DocumentFile df in this.documentList)
            //    {
            //        totalWidth += df.Width;
            //    }
            //}
            return totalWidth;
        }
        public void AddDoc(DocumentFile Doc)
        {
            //检查是否档案数量是否已经超出设置的最大数量或者超出了该层的宽度
            int totalDocumentWidth = this.getDocumentsTotalWidth(-1) + (this.documentList.Count - 1) * CarbinetFloor.DOCUMENT_GAP;
            if (totalDocumentWidth > this.width)
            {
                return;
            }
            if (this.maxDocNumber != -1 && this.documentList.Count >= this.maxDocNumber)
            {
                return;
            }

            //检查是否已经存在了
            bool bExist = false;
            foreach (DocumentFile df in this.documentList)
            {
                if (Doc.name == df.name)
                {
                    bExist = true;
                    break;
                }
            }
            if (bExist)//存在的将不再重复添加
            {
                return;
            }
            //int left = this.documentList.Count * (CarbinetFloor.DOCUMENT_GAP + DocumentFile.STATIC_WIDTH);

            DocumentChair dc = null;
            if (this.chairsList.Count <= this.documentList.Count)//坑的数量至少和档案数量相同，数量相同的时候，要添加新坑
            {
                //每增加一个档案需要为档案增加一个位置
                dc = new DocumentChair(Doc, this);
                //dc.index = this.documentList.Count - 1;
                dc.index = this.chairsList.Count;
                dc.floor = this.floorNumber;
                this.chairsList.Add(dc);
            }
            else
            {
                //坑的数量多于档案的数量，说明有坑是空的
                foreach (DocumentChair d in this.chairsList)
                {
                    Debug.WriteLine(string.Format("isEmpty -> floor = {0}  index = {1}  state = {2}", d.floor, d.index, d.getEmptyState()));
                    if (d.getEmptyState())
                    {
                        dc = d;
                        break;
                    }
                }
            }

            // Debug.WriteLine(string.Format("*** AddDoc *** {0}  {1}   ", Doc.name, Doc.floorNumber));
            //Debug.WriteLine(string.Format("*** chair -> {0}   Doc -> {1}", this.chairsList.Count, this.documentList.Count));
            //根据坑设置档案的位置
            //Doc.setDocPosition(left + this.left, this.top + 33);
            //Doc.setDocPosition(dc.Left+this.left, CarbinetFloor.FLOOR_HEIGHT - DocumentFile.DOCUMENT_HEIGHT);
            dc.setChairNotEmpty(Doc);//坑已被占用
            this.recaculateOtherDocumentPos(dc.index);

            Doc.setDocPosition(dc.Left + this.left, this.top + this.height - Doc.Height);
            Doc.setChair(dc);

            //档案添加到每层的列表中
            this.documentList.Add(Doc);


            //Debug.WriteLine(string.Format("left -> {0} top -> {1}", dc.Left, this.top));
            this.controls.Add(Doc.doc);
        }

        private void recaculateOtherDocumentPos(int startIndex)
        {
            for (int i = startIndex + 1; i < this.chairsList.Count; i++)
            {
                this.chairsList[i].recaculateDocPosition();
            }
        }
        public void RemoveDoc(DocumentFile doc, int i)
        {
#if TRACE
            Debug.WriteLine("RemoveDoc->");
#endif
            this.controls.Remove(doc.doc);
            this.documentList.Remove(doc);
            //this.chairsList[i].setEmptyState(true);
            //解除位置的占用
            doc.myChair.setChairEmpty();
            //this.chairsList.Remove(this.chairsList[i]);
#if TRACE
            Debug.WriteLine("RemoveDoc <-");
#endif
        }
    }
}

using System.Windows.Forms;

namespace InventoryMSystem
{
    public class DocumentFile
    {
        public string name;
        public int floorNumber;//柜子编号
        public Button doc;
        private int width = 16;

        public int Width
        {
            get { return this.doc.Width; }
            set { this.doc.Width = value; }
        }
        private int height = 92;

        public int Height
        {
            get { return this.doc.Height; }
            set { this.doc.Height = value; }
        }

        public int Left
        {
            get { return this.doc.Left; }
            set { this.doc.Left = value; }
        }

        public int Top
        {
            get { return this.doc.Top; }
            set { this.doc.Top = value; }
        }
        public DocumentChair myChair;



        public DocumentFile(string name, int floor)
        {
            this.doc = new Button();
            this.doc.Width = this.width;
            this.doc.Height = this.height;
            this.doc.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.doc.BackgroundImage = global::InventoryMSystem.Properties.Resources.DocFile;
            this.doc.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.doc.FlatAppearance.BorderColor = System.Drawing.Color.DarkRed;
            this.doc.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.doc.Name = name;
            this.doc.UseVisualStyleBackColor = false;
            this.name = name;
            this.floorNumber = floor;
        }
        public void setDocPosition(int left, int top)
        {
            this.doc.Left = left;
            this.doc.Top = top;
        }
        public void setChair(DocumentChair dc)
        {
            this.myChair = dc;
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace InventoryMSystem
{
    public partial class frmCarbinetShow : Form
    {
        Carbinet carbinet;
        Timer refreshTimer = new System.Windows.Forms.Timer();
        int refreshTimeSpan = 5000;
        TDJ_RFIDHelper tdjHelper;
        public frmCarbinetShow()
        {
            InitializeComponent();
            //carbinet = new Carbinet(this.Controls);
            //carbinet = new Carbinet(this.groupBox1.Controls);
            carbinet = new Carbinet(this.pictureBox1.Controls);
            carbinet.Left = 100;
            carbinet.Width = 300;
            carbinet.Height = 578;
            carbinet.LeftPading = 30;
            carbinet.ConfigFloor(1, 106, -1);
            carbinet.ConfigFloor(2, 106, -1);
            //carbinet.ConfigFloor(2);

            this.refreshTimer.Interval = this.refreshTimeSpan;
            this.refreshTimer.Tick += new EventHandler(refreshTimer_Tick);


            this.tdjHelper = new TDJ_RFIDHelper();
        }
        List<TagInfo> oldTags = null;
        void refreshTimer_Tick(object sender, EventArgs e)
        {
            //获取rfid标签信息，并将其加入到柜子中
            //this.carbinet.AddDocFile("file1", 1);
            this.tdjHelper.tagDeleted(this.refreshTimeSpan);
            List<TagInfo> tags = this.tdjHelper.getTagList();
            //return;


            //比较之前的标签和新获取的标签的差异，如果之前存在而在新标签中不存在的标签，
            //则说明该标签已经被拿走了
            if (oldTags != null)
            {
                List<string> noExistTags = new List<string>();
                foreach (TagInfo ti in oldTags)
                {
                    string name = ti.epc;
                    bool bFind = false;
                    foreach (TagInfo tiIn in tags)
                    {
                        if (tiIn.epc == name && tiIn.antennaID == ti.antennaID)
                        {
                            bFind = true;
                            break;
                        }
                    }
                    if (!bFind)
                    {
                        noExistTags.Add(name);
                    }
                }
                foreach (string s in noExistTags)
                {
                    this.carbinet.RemoveDocFile(s);
                }
            }
            foreach (TagInfo ti in tags)
            {
                int antennaID = 0;
                try
                {
                    antennaID = int.Parse(ti.antennaID);
                }
                catch (System.Exception ex)
                {
                    Debug.WriteLine("frmCarbinetShow -> " + ex.Message);
                }
                //Debug.WriteLine(String.Format("refreshTimer_Tick   epc -> {0}  floor -> {1}", ti.epc, antennaID));
                //TODO 天线和档案柜的层的对应关系需要配置
                if (ti.epc == "3005FB63AC1F3841EC880472")
                {
                    this.carbinet.AddDocFile(ti.epc, antennaID, 16, 80);
                }
                else
                {
                    this.carbinet.AddDocFile(ti.epc, antennaID, 16, 90);
                }
                //this.carbinet.AddDocFile(ti.epc, antennaID);
            }
            this.oldTags = tags;
        }

        public void AddDocument()
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.tdjHelper.StartRunning();
            this.refreshTimer.Start();

        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.refreshTimer.Stop();
            this.tdjHelper.StopRunning();
        }
    }

}

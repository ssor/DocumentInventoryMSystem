using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;

namespace InventoryMSystem
{
    public partial class frmDocumentManage : Form, IRefreshDGV
    {
        //RFIDHelper _RFIDHelper = new RFIDHelper();
        //SerialPort comport = new SerialPort();
        InvokeDic _UpdateList = new InvokeDic();
        rfidOperateUnitGetTagEPC operateUnit = new rfidOperateUnitGetTagEPC();
        rfidOperateUnitWirteEPC operateUnitWriteEpc = new rfidOperateUnitWirteEPC();

        string tagUII = string.Empty;
        documentManageCtl ctl = new documentManageCtl();
        string serial = "0001";

        ctlProductName ctlProductName = new ctlProductName();
        public frmDocumentManage()
        {
            InitializeComponent();



            this.operateUnit.registeCallback(new deleRfidOperateCallback(UpdateStatus));
            this.operateUnitWriteEpc.registeCallback(new deleRfidOperateCallback(showReturn));
            this.operateUnit.openSerialPort();

            this.dateTimePicker1.Value = DateTime.Now;
            this.FormClosing += new FormClosingEventHandler(frmProductManage_FormClosing);
            this.Shown += new EventHandler(frmProductManage_Shown);
            //comport.DataReceived += new SerialDataReceivedEventHandler(port_DataReceived);
            //使得Helper类可以向串口中写入数据
            //_RFIDHelper.evtWriteToSerialPort += new deleVoid_Byte_Func(RFIDHelper_evtWriteToSerialPort);
            // 处理当前操作的状态
            //_RFIDHelper.evtCardState += new deleVoid_RFIDEventType_Object_Func(_RFIDHelper_evtCardState);

            this.cmbDepartment.SelectedIndexChanged += new EventHandler(cmb_SelectedIndexChanged);
            this.cmbCabinet.SelectedIndexChanged += new EventHandler(cmb_SelectedIndexChanged);
            this.cmbFloor.SelectedIndexChanged += new EventHandler(cmb_SelectedIndexChanged);
            this.cmbRoom.SelectedIndexChanged += new EventHandler(cmb_SelectedIndexChanged);
            this.cmbCabinet.SelectedIndexChanged += new EventHandler(cmb_SelectedIndexChanged);
            this.cmbProject.SelectedIndexChanged += new EventHandler(cmbProject_SelectedIndexChanged);


            this.dataGridView1.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellContentClick);
            this.txtTag.TextChanged += new EventHandler(txtPID_TextChanged);
        }

#region 跨线程变量

        string DEPARTMENT_ID;
        string PROJECT_ID ;
        string CREATE_DATE;
        string YEAR ;
        string ROOM_ID ;
        string CABINET_ID ;
        string FLOOR ;
        string COMMENT;
        string epc;
        string tag;
#endregion

        private void showReturn(object o)
        {
            operateMessage msg = (operateMessage)o;
            if (msg.status == "fail")
            {
                MessageBox.Show("出现错误：" + msg.message);
                return;
            }
            else
                if (msg.status == "success")
                {
                    this.getCodeSource();
                    if (this.ctl.AddProductItem(this.epc, DEPARTMENT_ID, YEAR, PROJECT_ID, this.serial
                             , CREATE_DATE, ROOM_ID, CABINET_ID, FLOOR, this.tag, COMMENT))
                    {
                        //if (this.checkBox1.Checked)
                        {
                            MessageBox.Show("成功添加档案信息！");
                        }
                        if (this.dataGridView1.InvokeRequired)
                        {
                            this.dataGridView1.Invoke(new deleControlInvokeVoid_Void(this.refreshDGV));
                        }
                        //this.refreshDGV();
                    }
                }
        }
        void cmbProject_SelectedIndexChanged(object sender, EventArgs e)
        {
            //流水号需要重新查询
            // 重新编码
            cmb_SelectedIndexChanged(null, null);
        }

        void cmb_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 重新编码
            if ((this.cmbDepartment.Text == null) || (this.cmbProject.Text == null) ||
                (this.cmbRoom.Text == null) || (this.cmbCabinet.Text == null) || (this.cmbFloor.Text == null))
            {
                return;
            }

            //string DEPARTMENT_ID = (this.cmbDepartment.Text == null) ? "00" : this.cmbDepartment.Text;
            //string PROJECT_ID = (this.cmbProject.Text == null) ? "00" : this.cmbProject.Text;
            //string CREATE_DATE = (this.dateTimePicker1.Value.ToString("yyyy-MM-dd HH:mm:ss"));
            //string YEAR = this.dateTimePicker1.Value.Year.ToString();
            //string ROOM_ID = (this.cmbRoom.Text == null) ? "00" : this.cmbRoom.Text;
            //string CABINET_ID = (this.cmbCabinet.Text == null) ? "00" : this.cmbCabinet.Text;
            //string FLOOR = (this.cmbFloor.Text == null) ? "00" : this.cmbFloor.Text;
            //string COMMENT = (this.txtComment.Text == null) ? "00" : this.txtComment.Text;

            this.getCodeSource();

            string pid = "35" + DEPARTMENT_ID + YEAR + PROJECT_ID + this.serial + ROOM_ID + CABINET_ID + FLOOR + "0000";
            this.txtPID.Text = pid;
            //this.txtTag.Text = pid;

        }

        void frmProductManage_Shown(object sender, EventArgs e)
        {
            this.btnAdd.Enabled = false;


            this.cmbDepartment.Items.Add("部门一");
            this.cmbDepartment.Items.Add("部门二");
            this.cmbDepartment.Items.Add("部门三");
            this.cmbDepartment.SelectedIndex = 0;

            this.cmbProject.Items.Add("项目一");
            this.cmbProject.Items.Add("项目二");
            this.cmbProject.Items.Add("项目三");
            this.cmbProject.SelectedIndex = 0;

            this.cmbRoom.Items.Add("档案室一");
            this.cmbRoom.Items.Add("档案室二");
            this.cmbRoom.Items.Add("档案室三");
            this.cmbRoom.SelectedIndex = 0;

            this.cmbCabinet.Items.Add("档案柜一");
            this.cmbCabinet.Items.Add("档案柜二");
            this.cmbCabinet.Items.Add("档案柜三");
            this.cmbCabinet.SelectedIndex = 0;

            this.cmbFloor.Items.Add("一层");
            this.cmbFloor.Items.Add("二层");
            this.cmbFloor.Items.Add("三层");
            this.cmbFloor.SelectedIndex = 0;


            /*
                         DataTable dt = ctlProductName.GetProductName();
            if (dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    this.cmbProject.Items.Add(dr[0].ToString());
                }
            }
             */
            this.refreshDGV();
        }

        void frmProductManage_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.closeSerialPort();
        }

        private void btnQuit_Click(object sender, EventArgs e)
        {
            this.closeSerialPort();
            /* 
            
            bool bOk = false;
            if (null != comport)
            {
                if (comport.IsOpen)
                {
                    bOk = _UpdateList.ChekcAllItem();
                    // 如果没有全部完成，则要将消息处理让出，使Invoke有机会完成
                    while (!bOk)
                    {
                        Application.DoEvents();
                        bOk = _UpdateList.ChekcAllItem();
                    }
                    comport.Close();
                }
            }
            */
            this.Close();
        }

        private void btnGetPID_Click(object sender, EventArgs e)
        {
            //DateTime dt = DateTime.Now;
            //this.txtPID.Text = dt.ToString("yyyyMMddHHmmssff");
            this.operateUnit.OperateStart();
            //_RFIDHelper.StartCallback();
            //_RFIDHelper.SendCommand(RFIDHelper.RFIDCommand_RMU_GetStatus, RFIDEventType.RMU_CardIsReady);

        }
        void UpdateStatus(string value)
        {
            if (this.statusLabel.InvokeRequired)
            {
                this.statusLabel.Invoke(new deleUpdateContorl(UpdateStatusLable), value);
            }
            else
            {
                UpdateStatusLable(value);
            }
        }
        void UpdateStatus(object o)
        {
            operateMessage msg = (operateMessage)o;
            if (msg.status == "fail")
            {
                MessageBox.Show("出现错误：" + msg.message);
                return;
            }
            string value = msg.message;
            if (this.statusLabel.InvokeRequired)
            {
                this.statusLabel.Invoke(new deleUpdateContorl(UpdateStatusLable), value);
            }
            else
            {
                UpdateStatusLable(value);
            }
        }
        void UpdateStatusLable(string value)
        {
            this.txtPID.Text = value;
            //this.statusLabel.Text = value;
        }
        void UpdateEPCtxtBox(string value)
        {
            if (!_UpdateList.CheckItem("UpdateTipLable"))
            {
                return;
            }
            _UpdateList.SetItem("UpdateTipLable", false);

            this.txtPID.Text = value;

            _UpdateList.SetItem("UpdateTipLable", true);
        }

        private void btnSerialPortConf_Click(object sender, EventArgs e)
        {
            this.closeSerialPort();
            frmSerialPortConfig frm = new frmSerialPortConfig();
            frm.ShowDialog();
        }

        private bool checkInfoComplete()
        {
            if ((this.cmbDepartment.Text == null) || (this.cmbProject.Text == null) ||
                (this.cmbRoom.Text == null) || (this.cmbCabinet.Text == null) || (this.cmbFloor.Text == null))
            {
                MessageBox.Show("信息填写不完整！");
                return false;
            }
            if (this.txtTag.Text == null || this.txtTag.Text.Length <= 0)
            {
                MessageBox.Show("请填写一个档案编号！");
                return false;

            }
            return true;
        }
        private void getCodeSource()
        {
            //DEPARTMENT_ID = (this.cmbDepartment.Text == null) ? "" : this.cmbDepartment.Text;
            DEPARTMENT_ID = "0" + (this.cmbDepartment.SelectedIndex + 1).ToString();
            //PROJECT_ID = (this.cmbProject.Text == null) ? "" : this.cmbProject.Text;
            PROJECT_ID = "0" + (this.cmbProject.SelectedIndex + 1).ToString();
            CREATE_DATE = (this.dateTimePicker1.Value.ToString("yyyy-MM-dd HH:mm:ss"));
            YEAR = this.dateTimePicker1.Value.Year.ToString();
            //ROOM_ID = (this.cmbRoom.Text == null) ? "" : this.cmbRoom.Text;
            ROOM_ID = "0" + (this.cmbRoom.SelectedIndex + 1).ToString();
            //CABINET_ID = (this.cmbCabinet.Text == null) ? "" : this.cmbCabinet.Text;
            CABINET_ID = "0" + (this.cmbCabinet.SelectedIndex + 1).ToString();
            //FLOOR = (this.cmbFloor.Text == null) ? "" : this.cmbFloor.Text;
            FLOOR = "0" + (this.cmbFloor.SelectedIndex + 1).ToString();
            COMMENT = (this.txtComment.Text == null) ? "" : this.txtComment.Text;
            this.epc = this.txtPID.Text;
            this.tag = this.txtTag.Text;
        }
        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (!this.checkInfoComplete())
            {
                return;
            }


            if (this.ctl.CheckExists(this.txtPID.Text))
            {
                    MessageBox.Show("已存在此编号档案信息！");
                    return;
            }
            else
            {
                this.operateUnitWriteEpc.setEPC(this.txtPID.Text);
                this.operateUnitWriteEpc.OperateStart(true);
                /*
                if (this.ctl.AddProductItem(this.txtPID.Text, DEPARTMENT_ID, YEAR, PROJECT_ID, this.serial
                                            , CREATE_DATE, ROOM_ID, CABINET_ID, FLOOR, this.txtTag.Text, COMMENT))
                {
                    if (this.checkBox1.Checked)
                    {
                        MessageBox.Show("成功添加档案信息！");
                    }
                    this.refreshDGV();
                }
                 */
            }
            // frmProductManageAdd frmAdd = new frmProductManageAdd(this);
            //frmAdd.ShowDialog();
        }
        private void closeSerialPort()
        {
            operateUnit.closeSerialPort();
            bool bOk = _UpdateList.ChekcAllItem();
            // 如果没有全部完成，则要将消息处理让出，使Invoke有机会完成
            while (!bOk)
            {
                Application.DoEvents();
                bOk = _UpdateList.ChekcAllItem();
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }
        public void refreshDGV()
        {

            this.dataGridView1.DataSource = ctl.GetDocumentInfoTable();

            this.dataGridView1.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
            int headerW = this.dataGridView1.RowHeadersWidth;
            int columnsW = 0;
            DataGridViewColumnCollection columns = this.dataGridView1.Columns;
            if (columns.Count > 0)
            {
                for (int i = 0; i < columns.Count; i++)
                {
                    columnsW += columns[i].Width;
                }
                if (columnsW + headerW < this.dataGridView1.Width)
                {
                    int leftTotalWidht = this.dataGridView1.Width - columnsW - headerW;
                    int eachColumnAddedWidth = leftTotalWidht / (columns.Count);
                    for (int i = 0; i < columns.Count; i++)
                    {
                        columns[i].Width += eachColumnAddedWidth;
                    }
                }
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!this.checkInfoComplete())
            {
                return;
            }
            //string DEPARTMENT_ID = (this.cmbDepartment.Text == null) ? "" : this.cmbDepartment.Text;
            //string PROJECT_ID = (this.cmbProject.Text == null) ? "" : this.cmbProject.Text;
            //string CREATE_DATE = (this.dateTimePicker1.Value.ToString("yyyy-MM-dd HH:mm:ss"));
            //string YEAR = this.dateTimePicker1.Value.Year.ToString();
            //string ROOM_ID = (this.cmbRoom.Text == null) ? "" : this.cmbRoom.Text;
            //string CABINET_ID = (this.cmbCabinet.Text == null) ? "" : this.cmbCabinet.Text;
            //string FLOOR = (this.cmbFloor.Text == null) ? "" : this.cmbFloor.Text;
            //string COMMENT = (this.txtComment.Text == null) ? "" : this.txtComment.Text;
            this.getCodeSource();

            if (ctl.UpdateProductItem(this.txtPID.Text, DEPARTMENT_ID, YEAR, PROJECT_ID, this.serial
                                        , CREATE_DATE, ROOM_ID, CABINET_ID, FLOOR, this.txtTag.Text, COMMENT))
            {
                MessageBox.Show("更新档案信息成功！");
                this.refreshDGV();
            }


        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (ctl.DeleteProductItem(this.txtPID.Text))
            {
                MessageBox.Show("删除产品信息成功！");
                this.refreshDGV();
            }
            else
            {
                MessageBox.Show("删除产品信息出错！");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.refreshDGV();
        }

        void SetLabelContent()
        {
            DataTable tb = (DataTable)dataGridView1.DataSource;
            if (tb != null && tb.Rows.Count > 0)
            {
                txtPID.Text = tb.Rows[0][0].ToString();
                this.cmbProject.Text = tb.Rows[0][1].ToString();
                try
                {
                    this.dateTimePicker1.Value = DateTime.Parse(tb.Rows[0][2].ToString());
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                this.cmbRoom.Text = tb.Rows[0][3].ToString();
                this.txtComment.Text = tb.Rows[0][4].ToString();
            }
            else
            {
                txtPID.Text = null;
                cmbProject.Text = null;
                this.dateTimePicker1.Value = DateTime.Now;
                cmbRoom.Text = null;
                txtComment.Text = null;
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            DataTable tb = (DataTable)dataGridView1.DataSource;
            if (e.RowIndex >= 0)
            {
                dataGridView1.Rows[e.RowIndex].Selected = true;
                txtPID.Text = tb.Rows[e.RowIndex][0].ToString();
                this.cmbDepartment.Text =tb.Rows[e.RowIndex][1].ToString();
                this.cmbProject.Text =tb.Rows[e.RowIndex][2].ToString();
                //this.cmbName.Text = tb.Rows[e.RowIndex][1].ToString();
                //this.cmbProject.SelectedIndex = this.cmbProject.Items.IndexOf(tb.Rows[e.RowIndex][1].ToString());
                try
                {
                    this.dateTimePicker1.Value = DateTime.Parse(tb.Rows[e.RowIndex][3].ToString());
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                this.cmbRoom.Text = tb.Rows[e.RowIndex][4].ToString();
                this.cmbCabinet.Text = tb.Rows[e.RowIndex][5].ToString();
                this.cmbFloor.Text = tb.Rows[e.RowIndex][6].ToString();
                this.txtTag.Text = tb.Rows[e.RowIndex][7].ToString();
                DocumentInfo info  = this.ctl.GetSpecifiedDocumentInfo(this.txtPID.Text);
                this.txtComment.Text = info.COMMENT;
                this.serial = info.RPOJECT_SERIAL;
            }
        }

        private void txtPID_TextChanged(object sender, EventArgs e)
        {
            if (this.txtPID.Text == null || this.txtTag.Text == null || this.txtPID.Text.Length <= 0 || this.txtTag.Text.Length <= 0)
            {
                this.btnAdd.Enabled = false;
            }
            else
            {
                this.btnAdd.Enabled = true;
            }
        }
    }
}

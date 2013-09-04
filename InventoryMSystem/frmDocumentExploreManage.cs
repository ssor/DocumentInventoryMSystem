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
    public partial class frmDocumentExploreManage : Form
    {
        //RFIDHelper _RFIDHelper = new RFIDHelper();
        //SerialPort comport = new SerialPort();
        InvokeDic _UpdateList = new InvokeDic();
        rfidOperateUnitGetTagEPC operateUnit = new rfidOperateUnitGetTagEPC();
        string tagUII = string.Empty;
        documentManageCtl ctl = new documentManageCtl();
        string serial = "0001";

        ctlProductName ctlProductName = new ctlProductName();
        public frmDocumentExploreManage()
        {
            InitializeComponent();



            this.operateUnit.registeCallback(new deleRfidOperateCallback(UpdateStatus));
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
            this.cmbProject.SelectedIndexChanged += new EventHandler(cmbProject_SelectedIndexChanged);


            this.dataGridView1.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellContentClick);
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
            if ((this.cmbDepartment.Text == null) || (this.cmbProject.Text == null) 
                )
            {
                return;
            }

            string DEPARTMENT_ID = (this.cmbDepartment.Text == null) ? "00" : this.cmbDepartment.Text;
            string PROJECT_ID = (this.cmbProject.Text == null) ? "00" : this.cmbProject.Text;
            string CREATE_DATE = (this.dateTimePicker1.Value.ToString("yyyy-MM-dd HH:mm:ss"));
            string YEAR = this.dateTimePicker1.Value.Year.ToString();
            string COMMENT = (this.txtComment.Text == null) ? "00" : this.txtComment.Text;


        }

        void frmProductManage_Shown(object sender, EventArgs e)
        {
            this.btnAdd.Enabled = false;


            this.cmbDepartment.Items.Add("01");
            this.cmbDepartment.Items.Add("02");
            this.cmbDepartment.Items.Add("03");
            this.cmbDepartment.SelectedIndex = 0;

            this.cmbProject.Items.Add("01");
            this.cmbProject.Items.Add("02");
            this.cmbProject.Items.Add("03");
            this.cmbProject.SelectedIndex = 0;



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
            if ((this.cmbDepartment.Text == null) || (this.cmbProject.Text == null) 
                )
            {
                MessageBox.Show("信息填写不完整！");
                return false;
            }
            return true;
        }
        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (!this.checkInfoComplete())
            {
                return;
            }

            string DEPARTMENT_ID = (this.cmbDepartment.Text == null) ? "" : this.cmbDepartment.Text;
            string PROJECT_ID = (this.cmbProject.Text == null) ? "" : this.cmbProject.Text;
            string CREATE_DATE = (this.dateTimePicker1.Value.ToString("yyyy-MM-dd HH:mm:ss"));
            string YEAR = this.dateTimePicker1.Value.Year.ToString();
            string COMMENT = (this.txtComment.Text == null) ? "" : this.txtComment.Text;
            
            if (this.ctl.CheckExists(this.txtPID.Text))
            {
                    MessageBox.Show("已存在此编号档案信息！");
            }
            else
            {

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
            string DEPARTMENT_ID = (this.cmbDepartment.Text == null) ? "" : this.cmbDepartment.Text;
            string PROJECT_ID = (this.cmbProject.Text == null) ? "" : this.cmbProject.Text;
            string CREATE_DATE = (this.dateTimePicker1.Value.ToString("yyyy-MM-dd HH:mm:ss"));
            string YEAR = this.dateTimePicker1.Value.Year.ToString();
            string COMMENT = (this.txtComment.Text == null) ? "" : this.txtComment.Text;


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
                this.txtComment.Text = tb.Rows[0][4].ToString();
            }
            else
            {
                txtPID.Text = null;
                cmbProject.Text = null;
                this.dateTimePicker1.Value = DateTime.Now;
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
                this.txtTag.Text = tb.Rows[e.RowIndex][7].ToString();
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

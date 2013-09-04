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
    public partial class frmProductManageAdd : Form
    {
        //RFIDHelper _RFIDHelper = new RFIDHelper();
        //SerialPort comport = new SerialPort();
        InvokeDic _UpdateList = new InvokeDic();
        rfidOperateUnitGetTagEPC operateUnit = new rfidOperateUnitGetTagEPC();
        string tagUII = string.Empty;
        documentManageCtl ctl = new documentManageCtl();

        ctlProductName ctlProductName = new ctlProductName();
        IRefreshDGV refreshForm = null;
        public frmProductManageAdd(IRefreshDGV refreshForm)
        {
            InitializeComponent();
            DataTable dt = ctlProductName.GetProductName();
            if (dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    this.cmbName.Items.Add(dr[0].ToString());
                }
            }

            this.refreshForm = refreshForm;

            this.operateUnit.registeCallback(new deleRfidOperateCallback(UpdateStatus));
            this.operateUnit.openSerialPort();

            this.dateTimePicker1.Value = DateTime.Now;
            this.FormClosing += new FormClosingEventHandler(frmProductManage_FormClosing);
            this.Shown += new EventHandler(frmProductManage_Shown);

        }

        void frmProductManage_Shown(object sender, EventArgs e)
        {

        }

        void frmProductManage_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.closeSerialPort();
        }

        private void btnQuit_Click(object sender, EventArgs e)
        {
            this.closeSerialPort();

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

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (this.txtPID.Text == null || this.txtPID.Text == string.Empty)
            {
                MessageBox.Show("产品编号必须填写！");
                return;

            }
            if (this.cmbName.SelectedIndex == -1)
            {
                MessageBox.Show("请选择产品名称！");
                return;
            }
            /*
            if (ctl.AddProductItem(this.txtPID.Text, this.cmbName.Text,
                                    this.dateTimePicker1.Value.ToString("yyyy-MM-dd HH:mm:ss"), this.cmbFactory.Text,
                                    this.txtComment.Text))
            {
                MessageBox.Show("添加产品信息成功！");
                if (this.refreshForm != null)
                {
                    this.refreshForm.refreshDGV();
                }
                if (this.checkBox1.Checked)
                {
                    this.closeSerialPort();

                    this.Close();
                }
            }
            else
            {
                MessageBox.Show("添加产品信息失败！");
            }
             */
        }
        private void closeSerialPort()
        {
            bool bOk = _UpdateList.ChekcAllItem();
            // 如果没有全部完成，则要将消息处理让出，使Invoke有机会完成
            while (!bOk)
            {
                Application.DoEvents();
                bOk = _UpdateList.ChekcAllItem();
            }
            operateUnit.closeSerialPort();
        }

    }
}

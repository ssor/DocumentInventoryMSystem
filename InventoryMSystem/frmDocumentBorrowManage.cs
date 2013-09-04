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
    public partial class frmDocumentBorrowManage : Form
    {
        //RFIDHelper _RFIDHelper = new RFIDHelper();
        //SerialPort comport = new SerialPort();
        InvokeDic _UpdateList = new InvokeDic();
        rfidOperateUnitGetTagEPC operateUnit = new rfidOperateUnitGetTagEPC();
        string tagUII = string.Empty;
        documentManageCtl ctl = new documentManageCtl();
        ctlDocumentBorrowManage ctlBorrow = new ctlDocumentBorrowManage();
        bool borrowORreturn = true;//标识借阅 归还状态，默认为借阅状态
        public frmDocumentBorrowManage()
        {
            InitializeComponent();



            this.operateUnit.registeCallback(new deleRfidOperateCallback(UpdateStatus));
            this.operateUnit.openSerialPort();

            this.FormClosing += new FormClosingEventHandler(frmProductManage_FormClosing);
            this.Shown += new EventHandler(frmProductManage_Shown);
            //comport.DataReceived += new SerialDataReceivedEventHandler(port_DataReceived);
            //使得Helper类可以向串口中写入数据
            //_RFIDHelper.evtWriteToSerialPort += new deleVoid_Byte_Func(RFIDHelper_evtWriteToSerialPort);
            // 处理当前操作的状态
            //_RFIDHelper.evtCardState += new deleVoid_RFIDEventType_Object_Func(_RFIDHelper_evtCardState);

        }


        void frmProductManage_Shown(object sender, EventArgs e)
        {
            this.dateTimePicker1.Value = DateTime.Now;
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

        void UpdateStatus(object o)
        {
            operateMessage msg = (operateMessage)o;
            if (msg.status == "fail")
            {
                MessageBox.Show("出现错误：" + msg.message);
                return;
            }
            string value = msg.message;
            if (this.txtPID.InvokeRequired)
            {
                this.txtPID.Invoke(new deleUpdateContorl(UpdateEPCtxtBox), value);
            }
            else
            {
                UpdateEPCtxtBox(value);
            }
        }

        void UpdateEPCtxtBox(string value)
        {
            if (!_UpdateList.CheckItem("UpdateEPCtxtBox"))
            {
                return;
            }
            _UpdateList.SetItem("UpdateEPCtxtBox", false);

            this.txtPID.Text = value;

            _UpdateList.SetItem("UpdateEPCtxtBox", true);
        }

        private void btnSerialPortConf_Click(object sender, EventArgs e)
        {
            this.closeSerialPort();
            frmSerialPortConfig frm = new frmSerialPortConfig();
            frm.ShowDialog();
        }

        private bool checkInfoComplete()
        {
            return true;
        }
        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (!this.checkInfoComplete())
            {
                return;
            }

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



        private void txtPID_TextChanged(object sender, EventArgs e)
        {
            if (this.txtPID.Text == null || this.txtPID.Text.Length <= 0)
            {
                return;
            }
            else
            {

                DocumentInfo info = this.ctl.GetSpecifiedDocumentInfo(this.txtPID.Text);
                try
                {
                    this.dateTimePicker1.Value = DateTime.Parse(info.CREATE_DATE);
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                this.txtComment.Text = info.COMMENT;
                this.txtTag.Text = info.TAG_ID;
                this.txtPosition.Text = string.Format("{0}档案室{1}档案柜{2}层", info.ROOM_ID, info.CABINET_ID, info.FLOOR);


                DocumentBorrowInfo bInfo = this.ctlBorrow.GetDocumentBorrowInfo(this.txtPID.Text);
                this.txtBorrower.Text = bInfo.BORROWER_ID;
                if (this.txtBorrower.Text != null && this.txtBorrower.Text.Length > 0)
                {
                    this.button1.Text = "归还";
                    this.borrowORreturn = false;
                }
                else
                {
                    this.button1.Text = "借阅";
                    this.borrowORreturn = true;
                }
                this.txtBorrowComment.Text = bInfo.COMMENT;

            }
        }
        /*
        private void txtBorrower_TextChanged(object sender, EventArgs e)
        {
            if (this.txtBorrower.Text != null && this.txtBorrower.Text.Length > 0)
            {
                this.button1.Text = "归还";
            }
            else
            {
                this.button1.Text = "借阅";
            }
        }
*/
        private void button1_Click(object sender, EventArgs e)
        {
            //借阅
            if (this.txtBorrower.Text == null || this.txtBorrower.Text.Length <= 0)
            {
                MessageBox.Show("请填写借阅人！");
                return;
            }
            if (this.txtPID.Text == null || this.txtTag.Text == null || this.txtPID.Text.Length <= 0 || this.txtTag.Text.Length <= 0)
            {
                MessageBox.Show("档案信息不完整！");
                return;
            }
            if (this.borrowORreturn)
            {
                string comment = (this.txtBorrowComment.Text == null) ? "" : this.txtBorrowComment.Text;
                if (
                this.ctlBorrow.AddBorrowInfo(this.txtPID.Text, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), this.txtTag.Text, this.txtBorrower.Text, comment))
                {
                    MessageBox.Show("借阅成功！");
                }
            }
            else
            {
                if (this.ctlBorrow.DeleteBorrowInfo(this.txtPID.Text, this.txtBorrower.Text))
                {
                    MessageBox.Show("归还成功！");

                }
            }
        }
    }
}

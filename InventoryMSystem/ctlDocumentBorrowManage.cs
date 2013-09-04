using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Windows.Forms;

namespace InventoryMSystem
{
    public class DocumentBorrowInfo
    {
        public string DOCUMENT_ID;
        public string CREATE_DATE;
        public string TAG_ID;
        public string BORROWER_ID;
        public string COMMENT;
    }
    public class ctlDocumentBorrowManage
    {
        string sqlSelectGetBorrowInfo =
            @"select DOCUMENT_ID , CREATE_DATE , TAG_ID , BORROWER_ID , COMMENT from  T_DOCUMENT_BORROW_INFO  where DOCUMENT_ID=@DOCUMENT_ID";
        string sqlDeleteRemoveBorrowInfo =
            @"delete from T_DOCUMENT_BORROW_INFO where DOCUMENT_ID =@DOCUMENT_ID and BORROWER_ID =@BORROWER_ID; ";
        string sqlInsertAddBorrowInfo =
            @"insert into T_DOCUMENT_BORROW_INFO(DOCUMENT_ID , CREATE_DATE , TAG_ID , BORROWER_ID , COMMENT) values(@DOCUMENT_ID , @CREATE_DATE , @TAG_ID , @BORROWER_ID , @COMMENT ); ";
        public DocumentBorrowInfo GetDocumentBorrowInfo(string DOCUMENT_ID)
        {
            DocumentBorrowInfo info = new DocumentBorrowInfo();

            DataSet ds = null;
            try
            {
                ds = SQLiteHelper.ExecuteDataSet(
                          ConfigManager.GetDBConnectString(ConfigManager.GetCurrentDBType()),
                           sqlSelectGetBorrowInfo, new object[1] { DOCUMENT_ID });
                if (ds != null)
                {
                    if (ds.Tables.Count > 0)
                    {
                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            DataRow dr = ds.Tables[0].Rows[0];
                            info.DOCUMENT_ID = dr["DOCUMENT_ID"].ToString();
                            info.TAG_ID = dr["TAG_ID"].ToString();
                            info.COMMENT = dr["COMMENT"].ToString();
                            info.CREATE_DATE = dr["CREATE_DATE"].ToString();
                            info.BORROWER_ID = dr["BORROWER_ID"].ToString();
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("查询数据库时出现错误：" + ex.Message);
            }

            return info;
        }
        public bool AddBorrowInfo(
                            string DOCUMENT_ID, string CREATE_DATE, string TAG_ID, string BORROWER_ID, string COMMENT)
        {
            try
            {
                int result = int.Parse(SQLiteHelper.ExecuteNonQuery(ConfigManager.GetDBConnectString(ConfigManager.GetCurrentDBType()),
                                             sqlInsertAddBorrowInfo
                                             , new object[5]
                                                    {
                                                        DOCUMENT_ID
                                                        ,CREATE_DATE
                                                        ,TAG_ID
                                                        ,BORROWER_ID
                                                        ,COMMENT
                                                    }).ToString());
                if (result > 0)
                {
                    return true;
                }
            }
            catch (System.Exception ex)
            {

                MessageBox.Show("添加数据时出现错误：" + ex.Message);
            }
            return false;
        }
        public bool DeleteBorrowInfo(string DEPARTMENT_ID, string BORROWER_ID)
        {
            try
            {
                int result = int.Parse(SQLiteHelper.ExecuteNonQuery(ConfigManager.GetDBConnectString(ConfigManager.GetCurrentDBType()),
                                             sqlDeleteRemoveBorrowInfo
                                             , new object[2]
                                                    {
                                                        DEPARTMENT_ID,BORROWER_ID
                                                    }).ToString());
                if (result > 0)
                {
                    return true;
                }
            }
            catch (System.Exception ex)
            {

                MessageBox.Show("删除数据时出现错误：" + ex.Message);
            }
            return false;
        }


    }
}

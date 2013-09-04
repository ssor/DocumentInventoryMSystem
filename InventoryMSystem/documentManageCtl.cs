using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Data;

namespace InventoryMSystem
{
    public class DocumentInfo
    {
        public string DEPARTMENT_ID ;
        public string PROJECT_ID ;
        public string CREATE_DATE ;
        public string ROOM_ID ;
        public string CABINET_ID ;
        public string FLOOR ;
        public string COMMENT;
        public string DOCUMENT_ID;
        public string TAG_ID;
        public string YEAR;
        public string RPOJECT_SERIAL;
    }
    public class documentManageCtl
    {

        string SqlInsertItem =
                @"insert into T_DOCUMENT_INFO(
                    DOCUMENT_ID ,DEPARTMENT_ID ,YEAR ,PROJECT_ID,RPOJECT_SERIAL ,
                    CREATE_DATE ,ROOM_ID ,CABINET_ID ,FLOOR ,TAG_ID ,COMMENT
                     ) values
                     ( @DOCUMENT_ID ,@DEPARTMENT_ID ,@YEAR ,@PROJECT_ID,@RPOJECT_SERIAL ,
                    @CREATE_DATE ,@ROOM_ID ,@CABINET_ID ,@FLOOR ,@TAG_ID ,@COMMENT );";

        string sqlSelect =
            @"SELECT 
                DOCUMENT_ID AS RFID编码, 
                DEPARTMENT_ID AS 部门,
                PROJECT_ID AS 项目类型,
                CREATE_DATE AS 建档时间,
                ROOM_ID AS 档案室,
                CABINET_ID AS 档案柜,
                FLOOR AS 层数,
                TAG_ID AS 档案标签
                 FROM T_DOCUMENT_INFO;";
        string sqlUpdate =
            @"update T_DOCUMENT_INFO set DEPARTMENT_ID = @DEPARTMENT_ID,YEAR = @YEAR
                ,PROJECT_ID = @PROJECT_ID,RPOJECT_SERIAL = @RPOJECT_SERIAL,CREATE_DATE = @CREATE_DATE
                ,ROOM_ID = @ROOM_ID,CABINET_ID = @CABINET_ID,FLOOR = @FLOOR
                ,TAG_ID = @TAG_ID,COMMENT = @COMMENT
                where DOCUMENT_ID = @DOCUMENT_ID; ";
        string sqlDelete =
            @"delete from T_DOCUMENT_INFO where DOCUMENT_ID = @DOCUMENT_ID ;";
        string sqlSelectExist =
            @"select DOCUMENT_ID ,DEPARTMENT_ID ,YEAR ,PROJECT_ID,RPOJECT_SERIAL ,
CREATE_DATE ,ROOM_ID ,CABINET_ID ,FLOOR ,TAG_ID ,COMMENT from T_DOCUMENT_INFO where DOCUMENT_ID = @DOCUMENT_ID ; ";

        public DataTable GetDocumentInfoTable()
        {
            DataSet ds = null;
            try
            {
                ds = SQLiteHelper.ExecuteDataSet(
                          ConfigManager.GetDBConnectString(ConfigManager.GetCurrentDBType()),
                           sqlSelect, null);
                if (ds != null)
                {
                    if (ds.Tables.Count > 0)
                    {
                        return ds.Tables[0];
                    }
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("查询数据库时出现错误：" + ex.Message);
            }
            return null;
        }
        public bool UpdateProductItem(
                                    string DOCUMENT_ID, string DEPARTMENT_ID
                                    , string YEAR, string PROJECT_ID
                                    , string RPOJECT_SERIAL, string CREATE_DATE, string ROOM_ID, string CABINET_ID, string FLOOR, string TAG_ID, string COMMENT)
        {
            try
            {
                int result = int.Parse(SQLiteHelper.ExecuteNonQuery(ConfigManager.GetDBConnectString(ConfigManager.GetCurrentDBType()),
                                             sqlUpdate
                                           , new object[11]
                                                    {
                                                        DEPARTMENT_ID
                                                        ,YEAR
                                                        ,PROJECT_ID
                                                        ,RPOJECT_SERIAL
                                                        ,CREATE_DATE
                                                        ,ROOM_ID
                                                        ,CABINET_ID
                                                        ,FLOOR
                                                        ,TAG_ID
                                                        ,COMMENT
                                                        ,DOCUMENT_ID
                                                    }).ToString());
                if (result > 0)
                {
                    return true;
                }
            }
            catch (System.Exception ex)
            {

                MessageBox.Show("更新数据时出现错误：" + ex.Message);
            }
            return false;
        }
        public bool AddProductItem(
                                    string DOCUMENT_ID, string DEPARTMENT_ID
                                    , string YEAR, string PROJECT_ID
                                    , string RPOJECT_SERIAL, string CREATE_DATE, string ROOM_ID, string CABINET_ID, string FLOOR,string TAG_ID,string COMMENT)
        {
            try
            {
                int result = int.Parse(SQLiteHelper.ExecuteNonQuery(ConfigManager.GetDBConnectString(ConfigManager.GetCurrentDBType()),
                                             SqlInsertItem
                                             , new object[11]
                                                    {
                                                        DOCUMENT_ID
                                                        ,DEPARTMENT_ID
                                                        ,YEAR
                                                        ,PROJECT_ID
                                                        ,RPOJECT_SERIAL
                                                        ,CREATE_DATE
                                                        ,ROOM_ID
                                                        ,CABINET_ID
                                                        ,FLOOR
                                                        ,TAG_ID
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
        public bool DeleteProductItem(string DEPARTMENT_ID)
        {
            try
            {
                int result = int.Parse(SQLiteHelper.ExecuteNonQuery(ConfigManager.GetDBConnectString(ConfigManager.GetCurrentDBType()),
                                             sqlDelete
                                             , new object[1]
                                                    {
                                                        DEPARTMENT_ID
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
        public DocumentInfo GetSpecifiedDocumentInfo(string DOCUMENT_ID)
        {
            DocumentInfo info = new DocumentInfo();

            DataSet ds = null;
            try
            {
                ds = SQLiteHelper.ExecuteDataSet(
                          ConfigManager.GetDBConnectString(ConfigManager.GetCurrentDBType()),
                           sqlSelectExist, new object[1] { DOCUMENT_ID });
                if (ds != null)
                {
                    if (ds.Tables.Count > 0)
                    {
                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            DataRow dr = ds.Tables[0].Rows[0];
                            info.DOCUMENT_ID=dr["DOCUMENT_ID"].ToString();
                            info.TAG_ID = dr["TAG_ID"].ToString();
                            info.CABINET_ID = dr["CABINET_ID"].ToString();
                            info.COMMENT = dr["COMMENT"].ToString();
                            info.CREATE_DATE = dr["CREATE_DATE"].ToString();
                            info.DEPARTMENT_ID = dr["DEPARTMENT_ID"].ToString();
                            info.FLOOR = dr["FLOOR"].ToString();
                            info.PROJECT_ID = dr["PROJECT_ID"].ToString();
                            info.ROOM_ID = dr["ROOM_ID"].ToString();
                            info.RPOJECT_SERIAL = dr["RPOJECT_SERIAL"].ToString();
                            info.YEAR = dr["YEAR"].ToString();
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
        public bool CheckExists(string DOCUMENT_ID)
        {
            DataSet ds = null;
            try
            {
                ds = SQLiteHelper.ExecuteDataSet(
                          ConfigManager.GetDBConnectString(ConfigManager.GetCurrentDBType()),
                           sqlSelectExist, new object[1]{DOCUMENT_ID});
                if (ds != null)
                {
                    if (ds.Tables.Count > 0)
                    {
                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            return true;
                       }
                    }
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("查询数据库时出现错误：" + ex.Message);
            }
            return false;
        }
    }
}

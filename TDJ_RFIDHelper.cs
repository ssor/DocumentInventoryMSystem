using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Diagnostics;
using System.IO.Ports;
using System.Timers;


namespace InventoryMSystem
{
    public enum emuTagInfoFormat
    {
        standard, simple
    }
    /// <summary>
    /// 将标签信息解析后通过此类中转处理
    /// </summary>
    public class TagInfo
    {
        public int readCount = 0;
        public string antennaID = string.Empty;
        public string tagType = string.Empty;
        public string epc = string.Empty;
        public string getTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        //public int milliSecond = DateTime.Now.Millisecond;

        public long milliSecond = 0;

        public string toString()
        {
            string str = string.Empty;
            str = string.Format("ant -> {0} | count -> {1} | epc -> {2}",
                                this.antennaID, this.readCount, this.epc);

            return str;
        }
        public static TagInfo Parse(string tagInfo)
        {
            TagInfo ti = new TagInfo();
            emuTagInfoFormat format = emuTagInfoFormat.standard;
            if (tagInfo.Substring(0, 4) == "Disc")
            {
                format = emuTagInfoFormat.standard;
            }
            else
            {
                format = emuTagInfoFormat.simple;
            }
            /*
Disc:2000/02/28 20:01:51, Last:2000/02/28 20:07:42, Count:00019, Ant:02, Type:04, Tag:300833B2DDD906C000000000 
             
             */
            if (format == emuTagInfoFormat.standard)
            {
                string[] arrays = tagInfo.Split(',');
                if (arrays.Length < 6)//信息不全
                {
                    return null;
                }
                string temp = arrays[2];
                string strCount = arrays[2].Substring(temp.IndexOf(':') + 1);
                try
                {
                    ti.readCount = int.Parse(strCount);

                }
                catch (System.Exception ex)
                {

                }
                temp = arrays[3];
                ti.antennaID = temp.Substring(temp.IndexOf(':') + 1).Trim();
                if (ti.antennaID == "03" || ti.antennaID == "00" || ti.antennaID == "05" || ti.antennaID == "06" || ti.antennaID == "07")
                {
                    return null;
                }
                temp = arrays[4];
                ti.tagType = temp.Substring(temp.IndexOf(':') + 1).Trim();
                temp = arrays[5];
                ti.epc = temp.Substring(temp.IndexOf(':') + 1).Trim();
                DateTime dt = DateTime.Now;
                TimeSpan ts = dt - Program.timeBase;
                ti.getTime = dt.ToString("yyyy-MM-dd HH:mm:ss");
                //ti.milliSecond = dt.Millisecond;
                ti.milliSecond = (long)ts.TotalMilliseconds;

                //  Debug.WriteLine(ti.toString());
            }


            //过滤无效天线编号


            return ti;
        }
    }
    //板状读写器操作类
    //实时获取标签信息
    public class TDJ_RFIDHelper
    {
        #region Members
        //两个天线可能有误读，在此时间范围内更正
        int milliSecondDelay = 100;//缓冲时间，毫秒，

        //标签取走的缓冲时间，如果间隔该段时间没有该标签信息更新，则认为标签已经丢失
        int lostDelay = 10000;//单位毫秒
        int tagsInfoPrecision = 200;//更新标签表的时间精度
        int minReadCount = 2;//最少读取到的标签次数，少于此数目，则认为为误读
        //该表将存储读到的标签信息
        private DataTable tagsInfo = new DataTable("tagsInfo");
        private DataTable tempTable = new DataTable("tempTable");
        Timer refreshTimer = new Timer();//负责档案柜本身的逻辑
        private Timer _precisionTimer = new Timer();//负责缓存表里的时间精度
        private int _precision = 200;//缓存表的时间精度
        private int _tempSpan = 2000;//保存数据的时间跨度，缓存时间
        bool bRunning = false;

        #endregion


        public bool Running
        {
            get { return bRunning; }
            set { bRunning = value; }
        }
        public void StopRunning()
        {
            this.refreshTimer.Elapsed -= refreshTimer_Elapsed;
            this._precisionTimer.Elapsed -= _precisionTimer_Elapsed;
            this._precisionTimer.Stop();
            this.refreshTimer.Stop();
            StaticSerialPort.removeParser(this.Parse);
            this.tagsInfo.Rows.Clear();
        }
        public void StartRunning()
        {
            StaticSerialPort.AddParser(this.Parse);
            this.refreshTimer.Elapsed += new ElapsedEventHandler(refreshTimer_Elapsed);
            this._precisionTimer.Elapsed += new ElapsedEventHandler(_precisionTimer_Elapsed);
            this.refreshTimer.Start();
            this._precisionTimer.Start();
            StaticSerialPort.openStaticSerialPort();
        }

        public TDJ_RFIDHelper()
        {
            this.tagsInfo.CaseSensitive = true;

            tagsInfo.Columns.Add("epc", typeof(string));
            tagsInfo.Columns.Add("antennaID", typeof(string));
            tagsInfo.Columns.Add("tagType", typeof(string));
            tagsInfo.Columns.Add("readCount", typeof(string));
            tagsInfo.Columns.Add("milliSecond", typeof(long));//时间精度
            //tagsInfo.Columns.Add("milliSecond", typeof(string));
            tagsInfo.Columns.Add("getTime", typeof(string));
            tagsInfo.Columns.Add("state", typeof(string));

            tempTable.Columns.Add("epc", typeof(string));
            tempTable.Columns.Add("antennaID", typeof(string));
            tempTable.Columns.Add("tagType", typeof(string));
            tempTable.Columns.Add("readCount", typeof(string));
            tempTable.Columns.Add("milliSecond", typeof(long));//时间精度
            tempTable.Columns.Add("getTime", typeof(string));
            tempTable.Columns.Add("state", typeof(string));


            // StaticSerialPort.AddParser(this.Parse);
            //StaticSerialPort.getStaticSerialPort().Open();

            this.refreshTimer.Interval = this.tagsInfoPrecision;
            this._precisionTimer.Interval = this._precision;
            //this.refreshTimer.Elapsed += new ElapsedEventHandler(refreshTimer_Elapsed);
            //this.refreshTimer.Start();

        }
        public List<TagInfo> getTagList()
        {
            List<TagInfo> list = new List<TagInfo>();
            DataRowCollection rows = null;
            rows = this.tagsInfo.Rows;

            foreach (DataRow dr in rows)
            {
                TagInfo ti = new TagInfo();
                ti.epc = dr["epc"].ToString();
                ti.antennaID = dr["antennaID"].ToString();

                list.Add(ti);
            }
            return list;
        }
        //丢失或者拿走的标签列表
        //public List<TagInfo> lostTagList = new List<TagInfo>();


        //接收串口或者其它方式接收到的标签信息，
        public void ParseDataToTag()
        {
            int tagLength = 110;//每条数据的标准长度为110
            string temp1 = this.stringBuilder.ToString();
            //Debug.WriteLine(temp1);
            int start = temp1.IndexOf("Disc:");
            if (start<0)
            {
                return;
            }
            int tempStart = start;
            int lastDiscIndex = start;
            while (true)//找到最后一个Disc，并且其后有满格式的数据，即长度为110
            {
                int DiscIndex = temp1.IndexOf("Disc:", lastDiscIndex + 1);
                if (DiscIndex == -1)
                {
                    break;
                }
                else
                {
                    if (temp1.Length < DiscIndex + tagLength)
                    {
                        break;
                    }
                }
                lastDiscIndex = DiscIndex;
            }
            int tail = lastDiscIndex + 110;
            string temp = this.stringBuilder.ToString(start, tail + 2);
            this.stringBuilder.Remove(0, tail + 2);//正确数据之前的数据已经没用了
            //int end = temp1.IndexOf("\r\n");
            //if (start >= 0 && end >= 110 && start < end)
            //if (temp1.Length > start + 110)
            {

                for (int i = 0; i < temp.Length; i++)
                {
                    string tagInfo = string.Empty;
                    int startIndex = temp.IndexOf("Disc", i);
                    if (startIndex == -1)
                    {
                        return;
                    }
                    if (temp.Length - startIndex >= tagLength)
                    {
                        tagInfo = temp.Substring(startIndex, tagLength);
                    }
                    else
                    {
                        return;
                    }
                    TagInfo ti = TagInfo.Parse(tagInfo);
                    if (null == ti)
                    {
                        return;
                    }
                    //this.AddNewTag2Table(ti);
                    this.AddNewTag2TempTable(ti);
                    i = startIndex + tagLength;
                }
            }
        }
        public void ParseDataToTag(string tagInfo)
        {
            TagInfo ti = TagInfo.Parse(tagInfo);
            if (null == ti)
            {
                return;
            }
            this.AddNewTag2Table(ti);
        }
        public void outputTagTable(DataTable dt)
        {
            Debug.WriteLine("-----------------------***********************---------------------");
            DataRowCollection rows = dt.Rows;
            for (int i = 0; i < rows.Count; i++)
            {
                DataRow row = rows[i];
                Debug.WriteLine(string.Format("epc -> {0}  antennaID -> {1}  readCount = {2}  millisecond => {3} state => {4}"
                                , row["epc"].ToString(), row["antennaID"].ToString(), row["readCount"].ToString(), row["milliSecond"], row["state"]));
            }

            Debug.WriteLine("*******************************************************************");
        }
        void refreshTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            //this.AddNewTag2Table(new TagInfo());
            DataRow[] rows = null;
            rows = tempTable.Select("state = 'new'", "milliSecond asc");
            if (rows.Length > 0)
            {
                DataRow dr = rows[0];
                Debug.WriteLine(string.Format("Selected  old   millisecond => {0}  epc => {1}", dr["milliSecond"],dr["epc"]));
                dr["state"] = "deleted";
                outputTagTable(this.tempTable);
            }

        }

        //每隔一定时间扫描缓存数据，超过缓存时间的数据标记为deleted
        void _precisionTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            DataRow[] rows = null;
            TimeSpan ts = DateTime.Now - Program.timeBase;
            long span = (long)(ts.TotalMilliseconds - this._tempSpan);
            rows = tempTable.Select("milliSecond < " + span.ToString() + " and state = 'new'");
            if (rows.Length > 0)
            {
                DataRow dr = rows[0];
                Debug.WriteLine(string.Format("TempTable deleted  -> span -> {0}   millis -> {1} epc -> {2}", span.ToString(), dr["milliSecond"], dr["epc"]));
                dr["state"] = "deleted";
                //for (int i = 0; i < rows.Length; i++)
                //{
                //    DataRow dr = rows[i];
                //    Debug.WriteLine(string.Format("TempTable deleted  -> span -> {0}   millis -> {1} epc -> {2}", span.ToString(), dr["milliSecond"], dr["epc"]));
                //    dr["state"] = "deleted";
                //}
            }
        }

        public void AddNewTag2TempTable(TagInfo ti)
        {
            DataRow[] rows = null;
            rows = tempTable.Select("state = 'deleted'");
            if (rows.Length > 0)
            {
                DataRow dr = rows[0];
                Debug.WriteLine(string.Format("TempTable remove  ->    millis -> {0} epc -> {1}", dr["milliSecond"], dr["epc"]));
                this.tempTable.Rows.Remove(dr);
                outputTagTable(this.tempTable);
            }
            this.tempTable.Rows.Add(new object[] { ti.epc, ti.antennaID, ti.tagType, ti.readCount, ti.milliSecond, ti.getTime, "new" });

        }


        /// <summary>
        /// 将新解析完的标签尝试添加到列表中
        /// 首先要检查列表中是否已经有新标签的epc，如果已经有标签epc，查看天线编号是否一致，如果天线编号一致，则替换原有的
        /// 标签信息，如果天线编号不一致，则查看是否在缓冲时间段内，如果是则表明这可能是误读，要用读取次数多的标签信息代替
        /// 读取次数少的标签信息；如果不在缓冲时间段内，则认为标签已经改变了位置
        /// 因此，导致表内信息改变的情况有以下几种：
        /// 1 epc不存在，加到表中
        /// 2 epc存在，且天线编号一致，新的代替旧的
        /// 3 epc存在，缓冲时间段内，天线编号不一致，用读取次数多的代替少的
        /// 4 epc存在，非缓冲时间段内，天线编号不一致，新的代替旧的
        /// </summary>
        public void AddNewTag2Table(DataRow dr)
        {

        }
        public void AddNewTag2Table(TagInfo ti)
        {
            Debug.WriteLine("AddNewTag2Table  -> *******************************************************************");
            DataRow[] rows = null;
            TimeSpan ts = DateTime.Now - Program.timeBase;
            long span = (long)(ts.TotalMilliseconds - this.lostDelay);
            if (span > 0)
            {
                //rows = tagsInfo.Select("milliSecond < '" + span.ToString() + "'");
                rows = tagsInfo.Select("milliSecond < " + span.ToString() + "");
                if (rows.Length > 0)
                {
                    for (int i = 0; i < rows.Length; i++)
                    {
                        Debug.WriteLine(string.Format("span -> {0}   millis -> {1} epc -> {2}", span.ToString(), rows[i]["milliSecond"], rows[i]["epc"]));

                        this.tagsInfo.Rows.Remove(rows[i]);
                    }
                }
            }
            if (ti.readCount < this.minReadCount)
            {
                return;
            }
            rows = tagsInfo.Select("epc = '" + ti.epc + "'");
            if (rows.Length <= 0)//epc不存在，加到表中
            {
                this.tagsInfo.Rows.Add(new object[] { ti.epc, ti.antennaID, ti.tagType, ti.readCount, ti.milliSecond, ti.getTime });
            }
            else
            {
                if (ti.antennaID == rows[0]["antennaID"].ToString())//天线编号一致
                {
                    this.tagsInfo.Rows.Remove(rows[0]);
                    this.tagsInfo.Rows.Add(new object[] { ti.epc, ti.antennaID, ti.tagType, ti.readCount, ti.milliSecond, ti.getTime });
                }
                else//天线编号不一致
                {
                    if (ti.getTime != rows[0]["getTime"].ToString())//超出一秒，设定为超出缓冲时间
                    {
                        this.tagsInfo.Rows.Remove(rows[0]);
                        this.tagsInfo.Rows.Add(new object[] { ti.epc, ti.antennaID, ti.tagType, ti.readCount, ti.milliSecond, ti.getTime });
                    }
                    else
                    {
                        //没超出一秒，有可能超出缓冲时间，需要比较毫秒
                        long oldM = 0;
                        try
                        {
                            oldM = long.Parse(rows[0]["milliSecond"].ToString());
                        }
                        catch (System.Exception ex)
                        {
                            Debug.WriteLine(ex.Message);
                        }
                        if ((ti.milliSecond - oldM) > this.milliSecondDelay)//超出缓冲时间
                        {
                            this.tagsInfo.Rows.Remove(rows[0]);
                            this.tagsInfo.Rows.Add(new object[] { ti.epc, ti.antennaID, ti.tagType, ti.readCount, ti.milliSecond, ti.getTime });
                        }
                        else
                        {
                            //读取次数多的代替少的
                            int oldC = 0;
                            try
                            {
                                oldC = int.Parse(rows[0]["readCount"].ToString());
                            }
                            catch (System.Exception ex)
                            {
                                Debug.WriteLine(ex.Message);

                            }
                            if (ti.readCount > oldC)
                            {
                                this.tagsInfo.Rows.Remove(rows[0]);
                                this.tagsInfo.Rows.Add(new object[] { ti.epc, ti.antennaID, ti.tagType, ti.readCount, ti.milliSecond, ti.getTime });
                            }
                        }
                    }
                }
            }
            this.outputTagTable(this.tagsInfo);
        }

        List<byte> maxbuf = new List<byte>();
        StringBuilder stringBuilder = new StringBuilder();
        /// <summary>
        /// 接收数据源（串口）数据
        /// </summary>
        /// <param name="value"></param>
        public void Parse(byte[] value)
        {
            //Debug.WriteLine(string.Format("Parse -> {0}",BytesToHexString(value)));
            try
            {
                stringBuilder.Append(Encoding.ASCII.GetString(value));
                this.ParseDataToTag();
            }
            catch
            {

            }
        }

    }
}

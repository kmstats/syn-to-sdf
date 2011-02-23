using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Data.SqlServerCe;
using System.IO;

namespace com.echo.PDA
{
    public partial class MainForm : Form
    {
        bool canDo = false;

        public bool CanDo
        {
            get { return canDo; }
            set { canDo = value; }
        }

        bool sqlOK = false;
        public bool SqlOK
        {
            get { return sqlOK; }
            set { sqlOK = value; }
        }

        bool sdfOK = false;
        public bool SdfOK
        {
            get { return sdfOK; }
            set { sdfOK = value; }
        }


        yngbdbDataSet.tbl_CompenyDataTable tbl_CompenySDF = new yngbdbDataSet.tbl_CompenyDataTable();
        
        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Activated(object sender, EventArgs e)
        {
        }

        //检查SQL服务器连接情况
        private void ChkSql()
        {
            try
            {
                tbl_CompenyTableAdapter.Fill(sqlSvrDataSet.tbl_Compeny);
                label1.Text = "SQL服务器正常连接，地址：192.168.0.1 数据库：yngbdb1";
                sqlOK = true;
            }
            catch (SqlException)
            {
                label1.Text = "SQL服务器连接失败，请检查网络设置和服务器";
            }
        }

        private void ChkSDF()
        {
            try
            {
                
                tbl_CompenyTableAdapterSDF.Fill(tbl_CompenySDF);
                label2.Text = "SDF数据库正常连接";
                sdfOK = true;
            }
            catch (Exception)
            {
                label2.Text = "SDF数据库连接失败，请检查数据库文件";
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            ChkSql();
            ChkSDF();
            if (sqlOK && sdfOK)
            {
                canDo = true;
                button1.Enabled = true;
            }
            else
            {
                canDo = false;
                button1.Enabled = false;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            srcConnection.Open();
            destConnection.Open();

            // 复制数据
            CopyTable(srcConnection, destConnection, "SELECT * FROM tbl_Com_New", "tbl_Com_New");
            progressBar1.Value = 1 * 20;
            CopyTable(srcConnection, destConnection, "SELECT * FROM tbl_Compeny", "tbl_Compeny");
            progressBar1.Value = 2 * 20; 
            CopyTable(srcConnection, destConnection, "SELECT * FROM tbl_Gb_New", "tbl_Gb_New");
            progressBar1.Value = 3 * 20; 
            CopyTable(srcConnection, destConnection, "SELECT * FROM tbl_gbjm where 类型<>'删除' and 类型<>'退休'", "tbl_gbjm");
            progressBar1.Value = 4 * 20; 
            CopyTable(srcConnection, destConnection, "SELECT * FROM tbl_Relate_gbjm", "tbl_Relate_gbjm");
            progressBar1.Value = 5 * 20;

            srcConnection.Close();
            destConnection.Close();

            MessageBox.Show("数据成功同步");
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                File.Copy("./yngbdb.sdf", saveFileDialog1.FileName);
            }
        }


        public static void CopyTable(
            IDbConnection srcConnection,
            SqlCeConnection destConnection,
            string queryString,
            string destTableName)
        {
            IDbCommand srcCommand = srcConnection.CreateCommand();
            srcCommand.CommandText = queryString;

            SqlCeCommand destCommand = destConnection.CreateCommand();
            destCommand.CommandType = CommandType.Text;
            destCommand.CommandText = "delete from " + destTableName;
            destCommand.ExecuteNonQuery();
            
            destCommand.CommandType = CommandType.TableDirect; //基于表的访问，性能更好
            destCommand.CommandText = destTableName;
            try
            {
                IDataReader srcReader = srcCommand.ExecuteReader();

                SqlCeResultSet resultSet = destCommand.ExecuteResultSet(
                    ResultSetOptions.Sensitive |   //检测对数据源所做的更改
                    ResultSetOptions.Scrollable |  //可以向前或向后滚动
                    ResultSetOptions.Updatable); //允许更新数据

                object[] values;
                SqlCeUpdatableRecord record;
                while (srcReader.Read())
                {
                    // 从源数据库表读取记录
                    values = new object[srcReader.FieldCount];
                    srcReader.GetValues(values);

                    // 把记录写入到目标数据库表
                    record = resultSet.CreateRecord();
                    record.SetValues(values);
                    resultSet.Insert(record);
                }

                srcReader.Close();
                resultSet.Close();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
        }

        // 创建源 SQL Server 数据库连接对象
        SqlConnection srcConnection = new SqlConnection(com.echo.PDA.Properties.Settings.Default.SqlSvrConnectionString.ToString());

        // 创建目标 SQL Server Compact Edition 数据库连接对象
        SqlCeConnection destConnection = new SqlCeConnection(com.echo.PDA.Properties.Settings.Default.yngbdbConnectionString.ToString());


    }
}

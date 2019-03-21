using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DataTable = System.Data.DataTable;

namespace Excel2VCF
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        #region Event

        private void btnsel_Click(object sender, EventArgs e)
        {
            OpenFileDialog op = new OpenFileDialog();
            op.Title = "选择电话本";
            op.Multiselect = false;
            op.Filter = "Excel文件|*.xlsx|Excel文件|*.xls";
            if (DialogResult.OK == op.ShowDialog())
            {
                lblFilePath.Text = op.FileName;
                dataGridView1.DataSource = GetExcelTable();
                //dataGridView1.Enabled = false;
            }
        }
        private void btnsave_Click(object sender, EventArgs e)
        {
            SaveFileDialog sf = new SaveFileDialog();
            sf.Title = "保存为VCF文件";
            sf.Filter = "vcf文件|*.vcf";
            if (sf.ShowDialog() == DialogResult.OK)
            {
                SaveVCF(sf.FileName);
            }
        }
        #endregion

        #region Method

        /// <summary>
        /// 读取excel表
        /// </summary>
        /// <returns></returns>
        public DataTable GetExcelTable()
        {
            string strConn = "Provider=Microsoft.Ace.OleDb.12.0;" + "data source=" + lblFilePath.Text.Trim() + ";Extended Properties='Excel 12.0; HDR=Yes; IMEX=1'";
            OleDbConnection conn = new OleDbConnection(strConn);
            conn.Open();
            DataTable schemaTable = conn.GetOleDbSchemaTable(System.Data.OleDb.OleDbSchemaGuid.Tables, null);// 获取Sheet列表
            DataSet ds = new DataSet();
            OleDbDataAdapter adp;
            foreach (DataRow item in schemaTable.Rows)
            {
                adp = new OleDbDataAdapter($"Select * from [{item["TABLE_NAME"]}]", conn);
                adp.Fill(ds);
            }

            return ds.Tables[0];
        }

        void SaveVCF(string path)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                foreach (DataGridViewRow item in dataGridView1.Rows)
                {
                    if (item.Cells[0].Value == null)
                    {
                        break;
                    }
                    string name = item.Cells[0].Value.ToString();
                    string nums = item.Cells[1].Value.ToString();
                    sb.AppendLine("BEGIN:VCARD");
                    sb.AppendLine("VERSION:3.0");
                    sb.AppendLine($"N:{name.Substring(0, name.Length / 2)};{name.Substring(name.Length / 2)};;;");
                    sb.AppendLine($"FN:{name}");
                    sb.AppendLine($"TEL;TYPE=CELL:{nums}");
                    sb.AppendLine("END:VCARD");

                }
                File.WriteAllText(path, sb.ToString());
                MessageBox.Show(this, "转换保存成功！！", "提示", MessageBoxButtons.OK);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "转换保存失败！！", "提示", MessageBoxButtons.OK);
                WriteLog(ex);
            }
        }

        public static void WriteLog(Exception ex)
        {
            string logpath = Path.Combine(Application.StartupPath, "errlog.txt");
            StreamWriter sw = File.AppendText(logpath);
            sw.WriteLine();
            sw.WriteLine(string.Format("{0}Error 时间:{1}{0}", "---------------------------------", DateTime.Now));
            sw.WriteLine("导致错误信息 : " + ex.Message);
            sw.WriteLine(string.Format("导致错误的方法 :{0}.{1}", ex.TargetSite.DeclaringType.FullName, ex.TargetSite.Name));
            sw.WriteLine();
            sw.WriteLine(ex.StackTrace.ToString());
            //sw.WriteLine();
            //sw.WriteLine(ex.ToString());
            sw.Flush();
            sw.Close();
        }
        #endregion

    }
}

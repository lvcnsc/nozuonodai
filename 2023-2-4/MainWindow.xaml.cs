using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Data.SqlClient;
using System.IO;
using System.Timers;
using static System.Net.Mime.MediaTypeNames;

namespace _2023_2_4
{
    public partial class MainWindow : Window
    {
        SqlConnection conn;
        StreamWriter logWriter;
        Timer timer;

        public MainWindow()
        {
            InitializeComponent();

            //创建日志文件
            logWriter = new StreamWriter("log.txt", true, Encoding.UTF8);
            logWriter.AutoFlush = true;

            //创建计时器，每1分钟执行一次按钮点击事件中的代码
            /*timer = new Timer(60 * 1000);
            timer.Elapsed += Timer_Elapsed;
            timer.Start();*/

            Task.Run(() => { // 异步执行
                try
                {
                    conn = new SqlConnection("Data Source=192.168.1.212;Initial Catalog=wms;User ID=exe;Password=1234567;");
                    conn.Open();

                    this.Dispatcher.Invoke(() => {
                        if (conn.State == System.Data.ConnectionState.Open)
                        {
                            TextBlock1.Text = "数据库已连接";
                        }
                        else
                        {
                            TextBlock1.Text = "数据库未连接";
                        }
                    });

                    //记录日志
                    logWriter.WriteLine(DateTime.Now.ToString() + ": 数据库连接成功");
                }
                catch (Exception ex)
                {
                    this.Dispatcher.Invoke(() => {
                        TextBlock1.Text = $@"数据库连接失败!!!{ex}";
                    });

                    //记录日志
                    logWriter.WriteLine(DateTime.Now.ToString() + ": " + ex.Message);
                }
            });
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            this.Dispatcher.Invoke(() => {
                Button_Click(null, null);
            });
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
           /* try
            {
                SqlCommand cmd = new SqlCommand("insert into wms_inv_bat_fcl(inv_id,bat_code,fcl_qty) select d.inv_id,d.bat_code,d.fcl_qty from wms_move_detail d inner join wms_move m on d.master_id=m.id left join wms_inv_bat_fcl f on d.inv_id=f.inv_id and d.bat_code=f.bat_code where m.rsz_status_id=-1 and f.inv_id is null", conn);
                int result = cmd.ExecuteNonQuery();
                TextBlock2.Text = "无批号处理结果：" + result.ToString() + "行受影响。";
                TextBlock5.Text = "自动处理时间：" + DateTime.Now.ToString();
                //记录日志
                logWriter.WriteLine(DateTime.Now.ToString() + ": SQL语句执行成功，受影响行数：" + result);
            }
            catch (Exception ex)
            {
                TextBlock2.Text = "SQL语句执行失败：" + ex.Message;

                //记录日志
                logWriter.WriteLine(DateTime.Now.ToString() + ": SQL语句执行失败，错误信息：" + ex.Message);
            }*/

            try
            {
                /*string cuowupihao = TextBox1.Text;

                SqlCommand cmd = new SqlCommand
                    ("update a set a.fcl_qty=b.fcl_qty\r\nfrom wms_inv_bat_fcl a \r\njoin wms_move_detail b on a.inv_id=b.inv_id and a.bat_code=b.bat_code\r\njoin wms_move c on c.id=b.master_id\r\nwhere b.fcl_qty<>a.fcl_qty\r\nand c.erp_id=cuowupihao ", conn);*/

                string cuowupihao = TextBox1.Text;

                // 创建一个 SqlCommand 对象
                SqlCommand cmd = new SqlCommand(
                    "UPDATE a SET a.fcl_qty = b.fcl_qty " +
                    "FROM wms_inv_bat_fcl a " +
                    "JOIN wms_move_detail b ON a.inv_id = b.inv_id AND a.bat_code = b.bat_code " +
                    "JOIN wms_move c ON c.id = b.master_id " +
                    "WHERE b.fcl_qty <> a.fcl_qty " +
                    "AND c.erp_id = @cuowupihao", conn);

                // 添加 cuowupihao 参数
                cmd.Parameters.AddWithValue("@cuowupihao", cuowupihao);

                // 执行 SQL 命令
                cmd.ExecuteNonQuery();

                int result1 = cmd.ExecuteNonQuery();
                TextBlock6.Text = "单号：" + cuowupihao + result1.ToString() + "已处理,请在WMS点击复位后再刷新。";
                TextBox1.Text = "";

            }
            catch (Exception ex)
            {
                TextBlock6.Text = "SQL语句执行失败：" + ex.Message;

            }
        }

        //关闭窗口时关闭日志文件
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            timer.Stop();
            logWriter.Close();
        }
    }
}



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
            timer = new Timer(60 * 1000);
            timer.Elapsed += Timer_Elapsed;
            timer.Start();

            Task.Run(() => { // 异步执行
                try
                {
                    conn = new SqlConnection("Data Source=192.168.7.210;Initial Catalog=wms;User ID=sa;Password=123456;");
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
            try
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





/*在Button_Click下面增加一个if如果TextBox1有内容，就执行以下查询，
查询select* from wms_move where erp_id='TextBox1'，把字段名为“id”提取出来，填写到select fcl_qty,* from wms_move_detail where master_id='上一个查询提取的id'，再把该查询“inv_id”和“bat_code”提取出来，
填写到select * from wms_inv_bat_fcl where inv_id='上一个查询提取的inv_id' and bat_code='上一个查询提取的bat_code'，然后把fcl_qty字段名的内容和上一个查询做对比，如果相同则跳过，如果不同则将wms_inv_bat_fcl表的字段fcl_qty更改为
wms_move_detail表中fcl_qty字段的内容*/




/*select* from wms_move where erp_id='JHGJZT00056408'

select fcl_qty,* from wms_move_detail where master_id='2023020713400cac68528456b'

select * from wms_inv_bat_fcl where inv_id='4715' and bat_code='220601@CGX00000001~4715~0'


update wms_inv_bat_fcl  set fcl_qty=300 where id='68348' and bat_code='220601@CGX00000001~4715~0'*/
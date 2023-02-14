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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data.SqlClient;

namespace _2023_2_4
{
    public partial class MainWindow : Window
    {
        SqlConnection conn;

        public MainWindow() 
        {
            InitializeComponent();


                Task.Run(() => {         //异步执行

                    try      //异常捕获
                    {
                        conn = new SqlConnection("Data Source=192.168.7.210;Initial Catalog=wms;User ID=sa;Password=123456;"); //数据库连接
                        conn.Open();

                        this.Dispatcher.Invoke(() => //回到主线程运行，ui需要回到界面所在调度器运行
                        {
                            if (conn.State == System.Data.ConnectionState.Open)
                            {
                                TextBlock1.Text = "数据库已连接";
                            }
                            else
                            {

                                {
                                    TextBlock1.Text = "数据库未连接";
                                }

                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        this.Dispatcher.Invoke(() => {//回到主线程运行，ui需要回到界面所在调度器运行
                        TextBlock1.Text = $@"崩溃了！！{ex}";
                        });
                    }


                   
                });


            


                //增加了9700K，运行速度好快


        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SqlCommand cmd = new SqlCommand("insert into wms_inv_bat_fcl(inv_id,bat_code,fcl_qty) select d.inv_id,d.bat_code,d.fcl_qty from wms_move_detail d inner join wms_move m on d.master_id=m.id left join wms_inv_bat_fcl f on d.inv_id=f.inv_id and d.bat_code=f.bat_code where m.rsz_status_id=-1 and f.inv_id is null", conn);
            int result = cmd.ExecuteNonQuery();
            TextBlock2.Text = "运行结果：" + result.ToString() + "行受影响。";

            if (!string.IsNullOrEmpty(TextBox1.Text))
            {
                string erpId = TextBox1.Text;
                SqlCommand cmd1 = new SqlCommand($"select id from wms_move where erp_id='{erpId}'", conn);
                object idResult = cmd1.ExecuteScalar();
                if (idResult != null)
                {
                    int masterId = Convert.ToInt32(idResult);
                    SqlCommand cmd2 = new SqlCommand($"select inv_id, bat_code, fcl_qty from wms_move_detail where master_id='{masterId}'", conn);
                    SqlDataReader reader = cmd2.ExecuteReader();
                    List<string> invBatList = new List<string>();
                    while (reader.Read())
                    {
                        string invId = reader.GetString(0);
                        string batCode = reader.GetString(1);
                        int fclQty = reader.GetInt32(2);
                        SqlCommand cmd3 = new SqlCommand($"select fcl_qty from wms_inv_bat_fcl where inv_id='{invId}' and bat_code='{batCode}'", conn);
                        object fclQtyResult = cmd3.ExecuteScalar();
                        if (fclQtyResult == null)
                        {
                            SqlCommand cmd4 = new SqlCommand($"insert into wms_inv_bat_fcl(inv_id, bat_code, fcl_qty) values ('{invId}', '{batCode}', {fclQty})", conn);
                            cmd4.ExecuteNonQuery();
                            invBatList.Add(erpId);
                        }
                        else
                        {
                            int fclQtyInDb = Convert.ToInt32(fclQtyResult);
                            if (fclQty != fclQtyInDb)
                            {
                                SqlCommand cmd5 = new SqlCommand($"update wms_inv_bat_fcl set fcl_qty={fclQty} where inv_id='{invId}' and bat_code='{batCode}'", conn);
                                cmd5.ExecuteNonQuery();
                                invBatList.Add(erpId);
                            }
                        }
                    }
                    reader.Close();
                    TextBlock4.Text = $"运行结果：{invBatList.Count}行受影响。";
                    string fileName = $"{DateTime.Now:yyyyMMddHHmmssfff}.txt";
                    using (System.IO.StreamWriter sw = new System.IO.StreamWriter(fileName))
                    {
                        sw.WriteLine($"受更改的行{string.Join(",", invBatList)}");
                    }
                }
            }

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
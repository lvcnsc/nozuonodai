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
            conn = new SqlConnection("Data Source=192.168.7.210;Initial Catalog=wms;User ID=sa;Password=123456;");
            conn.Open();
            if (conn.State == System.Data.ConnectionState.Open)
            {
                TextBlock1.Text = "数据库已连接";
            }
            else
            {
                TextBlock1.Text = "数据库未连接";
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SqlCommand cmd = new SqlCommand("insert into wms_inv_bat_fcl(inv_id,bat_code,fcl_qty) select d.inv_id,d.bat_code,d.fcl_qty from wms_move_detail d inner join wms_move m on d.master_id=m.id left join wms_inv_bat_fcl f on d.inv_id=f.inv_id and d.bat_code=f.bat_code where m.rsz_status_id=-1 and f.inv_id is null", conn);
            int result = cmd.ExecuteNonQuery();
            TextBlock2.Text = "运行结果：" + result.ToString() + "行受影响。";
        }
    }
}
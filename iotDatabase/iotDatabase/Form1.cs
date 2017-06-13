using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Data.SqlClient;

namespace iotDatabase
{
    public partial class Form1 : Form
    {
        string cString = @"Data Source=UGUR-PC\SQLEXPRESS;Initial Catalog=TestDB;Integrated Security=True;Connect Timeout=15;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
        SqlConnection connection;
        SqlConnection connection1;
        SqlConnection connection2;
        SqlCommand cmd;
        SqlCommand cmd1;
        SqlCommand cmd2;
        StreamReader reader;
        StreamWriter writer;
        FileStream fs;
        SqlDataAdapter da;
        DataTable dt;
        string recive = null;

        TcpClient client1;
        TcpListener listener1 = new TcpListener(IPAddress.Any, 3000);
        bool stop = true;
        bool fullscreen = false;
        bool backgroundWorker1Busy = false;
        bool backgroundWorker2Busy = false;
        int serverTraffic = 0;
        long totalEntry = 0;
        int delayTime = 50;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (stop)
            {
                panel1.BackColor = Color.Green;
                timer2.Enabled = true;
                button1.Text = "Stop";
                label2.Text = "Availabe";
                stop = false;
            }
            else
            {
                panel1.BackColor = Color.Red;
                timer2.Enabled = false;
                button1.Text = "Start";
                label2.Text = "Closed";
                stop = true;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string address = @"D:\iotDatabase - Logs\" + DateTime.Now.ToShortDateString() + ".txt";
            try
            {
                if (!Directory.Exists(@"D:\iotDatabase - Logs"))
                {
                    Directory.CreateDirectory(@"D:\iotDatabase - Logs");
                }

                if (!File.Exists(address))
                    MessageBox.Show("Log file created at " + address);
                else
                    MessageBox.Show("Log file appended to " + address);

                fs = new FileStream(address, FileMode.Append, FileAccess.Write);
                writer = new StreamWriter(fs);
                writer.Write(textBox3.Text);
                writer.Close();
                fs.Close();
            }
            catch (Exception ex)
            {
                textBox3.Text += "Hata :" + DateTime.Now.ToLongTimeString() + "  " + DateTime.Now.ToShortDateString() + "  -  " + ex.Message + "\r\n";
            }
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex != -1)
            {
                try
                {
                    delayTime = 50;
                    chart1.Visible = false;
                    connection = new SqlConnection(cString);
                    connection.Open();
                    string sql = "SELECT ROW_NUMBER() OVER(ORDER BY date) AS #, * FROM iotDatabase WHERE name=@name";
                    cmd = new SqlCommand(sql, connection);
                    cmd.Parameters.AddWithValue("@name", listBox1.SelectedItem);
                    da = new SqlDataAdapter(cmd);
                    dt = new DataTable();
                    da.Fill(dt);
                    dataGridView1.DataSource = dt;
                    dataGridView1.Visible = true;
                    cmd.Dispose();
                    connection.Close();
                }
                catch (Exception ex)
                {
                    textBox3.Text += "Hata :" + DateTime.Now.ToLongTimeString() + "  " + DateTime.Now.ToShortDateString() + "  -  " + ex.Message + "\r\n";
                }
            }
            else
            {
                MessageBox.Show("Select a field!");
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex != -1)
            {
                delayTime = 50;
                chart1.Visible = true;
                dataGridView1.Visible = false;
                chart1.Series[0].Points.Clear();
                chart1.Series[1].Points.Clear();
                chart1.Series[2].Points.Clear();
                chart1.Series[3].Points.Clear();
                try
                {
                    connection = new SqlConnection(cString);
                    connection.Open();
                    string sql = "SELECT TOP 1000 field1, field2, field3, field4 FROM iotDatabase"
                        + " WHERE name=@name ORDER BY date";
                    cmd = new SqlCommand(sql, connection);
                    cmd.Parameters.AddWithValue("@name", listBox1.SelectedItem);

                    da = new SqlDataAdapter(cmd);
                    dt = new DataTable();
                    da.Fill(dt);
                    cmd.Dispose();
                    connection.Close();
                    chart1.DataSource = dt;
                    chart1.Update();
                }
                catch (Exception ex)
                {
                    textBox3.Text += "Hata :" + DateTime.Now.ToLongTimeString() + "  " + DateTime.Now.ToShortDateString() + "  -  " + ex.Message + "\r\n";
                }
            }
            else
            {
                MessageBox.Show("Select a field!");
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(textBox1.Text) || !string.IsNullOrEmpty(textBox2.Text))
            {
                if (textBox1.Text == "ugrprtkldl" && textBox2.Text == "1946814393865022")
                {
                    DialogResult dialogResult = MessageBox.Show("Are you sure you want to delete all the data PERMANENTLY?", "", MessageBoxButtons.YesNo);
                    if (dialogResult == DialogResult.Yes)
                    {
                        connection2 = new SqlConnection(cString);
                        connection2.Open();
                        string sql = @"TRUNCATE TABLE iotDatabase";
                        cmd2 = new SqlCommand(sql, connection2);
                        cmd2.ExecuteNonQuery();
                        cmd2.Dispose();
                        connection2.Close();

                        dataGridView1.Visible = false;
                        chart1.Visible = false;
                        chart1.Series[0].Points.Clear();
                        chart1.Series[1].Points.Clear();
                        chart1.Series[2].Points.Clear();
                        chart1.Series[3].Points.Clear();
                        textBox1.Clear();
                        textBox2.Clear();
                        textBox3.Clear();
                        label3.Text = "-";
                        totalEntry = 0;
                        if (Directory.Exists(@"D:\iotDatabase - Logs"))
                        {
                            Directory.Delete(@"D:\iotDatabase - Logs");
                        }

                        MessageBox.Show("Logs and Records successfully deleted.");
                    }
                }
                else
                {
                    MessageBox.Show("Username or password wrong!");
                }
            }
            else
            {
                MessageBox.Show("Username or password cannot be empty!");
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            label7.Text = serverTraffic.ToString();
            serverTraffic = 0;
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            try
            {
                if (!backgroundWorker1Busy)
                    backgroundWorker1.RunWorkerAsync();
                backgroundWorker1Busy = true;
                timer2.Enabled = false;
            }
            catch (Exception ex)
            {
                textBox3.Text += "Hata :" + DateTime.Now.ToLongTimeString() + "  " + DateTime.Now.ToShortDateString() + "  -  " + ex.Message + "\r\n";
            }
        }

        private void timer3_Tick(object sender, EventArgs e)
        {
            if (delayTime == 50)
            {
                try
                {
                    connection = new SqlConnection(cString);
                    connection.Open();
                    string sql = "SELECT DISTINCT name FROM iotDatabase ORDER BY name";
                    cmd = new SqlCommand(sql, connection);
                    SqlDataReader dr = cmd.ExecuteReader();
                    listBox1.Items.Clear();
                    while (dr.Read())
                    {
                        listBox1.Items.Add(dr.GetString(0));
                    }
                    dr.Close();
                    cmd.Dispose();
                    connection.Close();
                }
                catch (Exception ex)
                {
                    textBox3.Text += "Hata :" + DateTime.Now.ToLongTimeString() + "  " + DateTime.Now.ToShortDateString() + "  -  " + ex.Message + "\r\n";
                }
            }
            else
            {
                delayTime++;
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                listener1.Start();
                client1 = listener1.AcceptTcpClient();
                listener1.Stop();
                serverTraffic++;
                totalEntry++;
                label3.Invoke(new MethodInvoker(delegate()
                {
                    label3.Text = totalEntry.ToString();
                }));

                if (textBox3.Text.IndexOf(((IPEndPoint)client1.Client.RemoteEndPoint).Address.ToString()) == -1)
                {
                    textBox3.Invoke(new MethodInvoker(delegate()
                    {
                        textBox3.Text = "Bağlantı isteği -> " + ((IPEndPoint)client1.Client.RemoteEndPoint).Address.ToString() + "\r\n" + textBox3.Text;
                    }));
                }
                reader = new StreamReader(client1.GetStream());
                recive = reader.ReadLine();
                while (backgroundWorker2Busy) ;
                backgroundWorker2Busy = true;
                backgroundWorker2.RunWorkerAsync();
                reader.Close();
                client1.Close();
            }
            catch (Exception ex)
            {
                textBox3.Invoke(new MethodInvoker(delegate()
                {
                    textBox3.Text += "Hata :" + DateTime.Now.ToLongTimeString() + "  " + DateTime.Now.ToShortDateString() + "  -  " + ex.Message + "\r\n";
                }));
            }
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            backgroundWorker1Busy = false;
            if (!stop)
                timer2.Enabled = true;
        }

        private void backgroundWorker2_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                connection1 = new SqlConnection(cString);
                connection1.Open();
                string sql = @"INSERT INTO iotDatabase (name, date, field1, field2, field3, field4)"
                    + "VALUES (@name, @date, @field1, @field2, @field3, @field4)";
                cmd1 = new SqlCommand(sql, connection1);
                recive += ",null,null,null,null";
                string[] splittedRecieve = recive.Split(',');
                cmd1.Parameters.AddWithValue("@name", splittedRecieve[0]);
                cmd1.Parameters.AddWithValue("@date", DateTime.Now);
                cmd1.Parameters.AddWithValue("@field1", splittedRecieve[1]);
                cmd1.Parameters.AddWithValue("@field2", splittedRecieve[2]);
                cmd1.Parameters.AddWithValue("@field3", splittedRecieve[3]);
                cmd1.Parameters.AddWithValue("@field4", splittedRecieve[4]);
                cmd1.ExecuteNonQuery();
                cmd1.Dispose();
                connection1.Close();
            }

            catch (Exception ex)
            {
                textBox3.Invoke(new MethodInvoker(delegate()
                {
                    textBox3.Text += "Hata :" + DateTime.Now.ToLongTimeString() + "  " + DateTime.Now.ToShortDateString() + "  -  " + ex.Message + "\r\n";
                }));
            }
        }

        private void backgroundWorker2_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            backgroundWorker2Busy = false;
        }

        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (fullscreen ? fullscreen = false : fullscreen = true) { };

            if (fullscreen)
            {
                dataGridView1.Dock = DockStyle.Fill;
                this.WindowState = FormWindowState.Maximized;
                this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            }
            else
            {
                dataGridView1.Dock = DockStyle.Bottom;
                this.WindowState = FormWindowState.Normal;
                this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            }
        }

        private void chart1_DoubleClick(object sender, EventArgs e)
        {
            if (fullscreen ? fullscreen = false : fullscreen = true) { };

            if (fullscreen)
            {
                chart1.Dock = DockStyle.Fill;
                this.WindowState = FormWindowState.Maximized;
                this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            }
            else
            {
                chart1.Dock = DockStyle.Bottom;
                this.WindowState = FormWindowState.Normal;
                this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            }
        }

        private void listBox1_MouseEnter(object sender, EventArgs e)
        {
            delayTime = 0;
        }
    }
}

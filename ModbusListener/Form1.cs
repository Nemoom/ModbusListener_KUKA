using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;
using System.Reflection;
using ModbusTCP.Server;

namespace ModbusListener
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public ModbusTCP_Server myServer;

        void myServer_ModbusTCPDataWrittenTo(object sender, ModbusTCPDataEventArgs e)
        {
            string m_Data = "";
            switch (e.ModbusTCPDataType)
            {
                case ModbusTCP.Server.RegisterType.CoilDiscrete:
                     m_Data = "";
                    for (int i = 0; i < e.Data.A.Count; i++)
                    {
                        dataGridView3.Rows[e.StartAddress + i].Cells[1].Value = Convert.ToInt32(e.Data.A[i]);
                        if (i == 0)
                        {
                            m_Data = Convert.ToInt32(e.Data.A[i]).ToString();
                        }
                        else
                        {
                            m_Data = m_Data + " , " + Convert.ToInt32(e.Data.A[i]).ToString();
                        }
                    }
                    InvokeAppendText(textBox3, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff ") + e.StartAddress.ToString() + "[" + m_Data + "]");
                    InvokeAppendText(textBox3, Environment.NewLine + Environment.NewLine);
                    break;
                case ModbusTCP.Server.RegisterType.HoldRegister:
                     m_Data = "";
                    for (int i = 0; i < e.Data.B.Count; i++)
                    {
                        dataGridView1.Rows[e.StartAddress + i].Cells[1].Value = e.Data.B[i];
                        if (i == 0)
                        {
                            m_Data = e.Data.B[i].ToString();
                        }
                        else
                        {
                            m_Data = m_Data + " , " + e.Data.B[i].ToString();
                        }
                    }
                    comboBox2_SelectedIndexChanged(sender, e);
                    InvokeAppendText(textBox3, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff ") + e.StartAddress.ToString() + "[" + m_Data + "]");
                    InvokeAppendText(textBox3, Environment.NewLine + Environment.NewLine);
                    break;
                case ModbusTCP.Server.RegisterType.InputDiscrete:
                     m_Data = "";
                    for (int i = 0; i < e.Data.A.Count; i++)
                    {
                        dataGridView4.Rows[e.StartAddress + i].Cells[1].Value = Convert.ToInt32(e.Data.A[i]);
                        if (i == 0)
                        {
                            m_Data = Convert.ToInt32(e.Data.A[i]).ToString();
                        }
                        else
                        {
                            m_Data = m_Data + " , " + Convert.ToInt32(e.Data.A[i]).ToString();
                        }
                    }
                    InvokeAppendText(textBox3, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff ") + e.StartAddress.ToString() + "[" + m_Data + "]");
                    InvokeAppendText(textBox3, Environment.NewLine + Environment.NewLine);
                    break;
                case ModbusTCP.Server.RegisterType.InputRegister:
                     m_Data = "";
                    for (int i = 0; i < e.Data.B.Count; i++)
                    {
                        dataGridView2.Rows[e.StartAddress + i].Cells[1].Value = e.Data.B[i];
                        if (i == 0)
                        {
                            m_Data = e.Data.B[i].ToString();
                        }
                        else
                        {
                            m_Data = m_Data + " , " + e.Data.B[i].ToString();
                        }
                    }
                    comboBox3_SelectedIndexChanged(sender, e);
                    InvokeAppendText(textBox3, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff ") + e.StartAddress.ToString() + "[" + m_Data + "]");
                    InvokeAppendText(textBox3, Environment.NewLine + Environment.NewLine);
                    break;
                default:
                    break;
            }
        }

        void myServer_ModbusTCPRequestReceived(object sender, ModbusTCPRequestEventArgs e)
        {
            byte fc = e.FunctionCode;
            byte[] byteStartAddress = e.byte_StartAddress;
            byte[] byteNum = e.byte_Data;
            Int16 StartAddress = BitConverter.ToInt16(byteStartAddress, 0);
            Int16 NumOfPoint = BitConverter.ToInt16(byteNum, 0);

            //解析功能码
            switch (fc)
            {
                case (byte)1:
                    //读线圈[Read CoilDiscrete]
                    InvokeAppendText(textBox2, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "[Read CoilDiscrete] " + " 起始地址:" +
                                                   StartAddress.ToString() + " 数据内容：" + NumOfPoint.ToString());
                    InvokeAppendText(textBox2, Environment.NewLine + Environment.NewLine);
                    break;
                case (byte)2:
                    //读离散量输入[Read InputDiscrete]
                    InvokeAppendText(textBox2, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "[Read InputDiscrete] " + " 起始地址:" +
                                                   StartAddress.ToString() + " 数据内容：" + NumOfPoint.ToString());
                    InvokeAppendText(textBox2, Environment.NewLine + Environment.NewLine);
                    break;
                case (byte)3:
                    //读保持寄存器[Read HoldingRegister]
                    InvokeAppendText(textBox2, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "[Read HoldingRegister] " + " 起始地址:" +
                                                   StartAddress.ToString() + " 数据内容：" + NumOfPoint.ToString());
                    InvokeAppendText(textBox2, Environment.NewLine + Environment.NewLine);
                    break;
                case (byte)4:
                    //读输入寄存器[Read InputRegister]
                    InvokeAppendText(textBox2, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "[Read InputRegister] " + " 起始地址:" +
                                                   StartAddress.ToString() + " 数据内容：" + NumOfPoint.ToString());
                    InvokeAppendText(textBox2, Environment.NewLine + Environment.NewLine);
                    break;
                case (byte)5:
                    //写单个线圈[Write Single Coil]
                    InvokeAppendText(textBox4, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "[Write Single Coil] " + " 起始地址:" +
                                                   StartAddress.ToString() + " 数据内容：" + NumOfPoint.ToString());
                    InvokeAppendText(textBox4, Environment.NewLine + Environment.NewLine);
                    break;
                case (byte)6:
                    //写单个寄存器[Write Single Register]
                    InvokeAppendText(textBox4, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "[Write Single Register] " + " 起始地址:" +
                                                   StartAddress.ToString() + " 数据内容：" + NumOfPoint.ToString());
                    InvokeAppendText(textBox4, Environment.NewLine + Environment.NewLine);
                    break;
                case (byte)15:
                    //写多个线圈[Write Multiple Coils]
                    InvokeAppendText(textBox4, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " [Write Multiple Coils]" + " 起始地址:" +
                                                   StartAddress.ToString() + " 数据内容：" + NumOfPoint.ToString());
                    InvokeAppendText(textBox4, Environment.NewLine + Environment.NewLine);
                    break;
                case (byte)16:
                    //写多个寄存器[Write Multiple Registers]
                    InvokeAppendText(textBox4, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " [Write Multiple Registers]" + " 起始地址:" +
                                                   StartAddress.ToString() + " 数据内容：" + NumOfPoint.ToString());
                    InvokeAppendText(textBox4, Environment.NewLine + Environment.NewLine);
                    break;
                case (byte)17:
                    //[Report Slave ID]
                    break;
                case (byte)20:
                    //读文件记录
                    break;
                case (byte)21:
                    //写文件记录
                    break;
                case (byte)22:
                    //[Mask Write Register]
                    break;
                case (byte)23:
                    //[Read/Write Multiple Registers]
                    break;
                default:
                    break;
            }
        }       
       
        void myServer_ClientDisconnected(object sender, TCPEventArgs e)
        {
            InvokeDeleteItem(listBox1, e._handle.Client.RemoteEndPoint.ToString());
            //InvokeAppendText(textBox2, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " 客户端断开连接 客户端IP:" +
            //                       e._handle.Address + " 端口号：" + e._handle.Port.ToString());
            //InvokeAppendText(textBox2, Environment.NewLine + Environment.NewLine);
            //InvokeDeleteItem(listBox2, e._handle.Address + " ：" + e._handle.Port.ToString());
        }

        void myServer_ClientConnected(object sender, TCPEventArgs e)
        {
            try
            {

                InvokeAddItem(listBox1, e._handle.Client.RemoteEndPoint.ToString());

            }
            catch (Exception ex)
            {
                ex.ToString();
            }
            //InvokeAppendText(textBox2, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " 客户端接入 客户端IP:" +
            //                                   e._handle.Address + " 端口号：" + e._handle.Port.ToString());
            //InvokeAppendText(textBox2, Environment.NewLine + Environment.NewLine);

            //InvokeAddItem(listBox2,e._handle.Address + " ：" + e._handle.Port.ToString());
        }


        #region 委托：TextBox.AppendText
        protected delegate void AppendTextHandler(TextBox textBoxCtrl, string Txt);
        void InvokeAppendText(TextBox textBoxCtrl, string Txt)
        {
            textBoxCtrl.Invoke((AppendTextHandler)AppendText, textBoxCtrl, Txt);
        }
        void AppendText(TextBox textBoxCtrl, string Txt)
        {
            textBoxCtrl.AppendText(Txt);
        }

        protected delegate void ChangeComboBoxTextHandler(ComboBox comboBoxCtrl, string Txt);
        void InvokeChangeComboBoxText(ComboBox comboBoxCtrl, string Txt)
        {
            comboBoxCtrl.Invoke((ChangeComboBoxTextHandler)ChangeComboBoxText, comboBoxCtrl, Txt);
        }
        void ChangeComboBoxText(ComboBox comboBoxCtrl, string Txt)
        {
            comboBoxCtrl.Text = Txt;
        }

        protected delegate void ChangeComboBoxIndexHandler(ComboBox comboBoxCtrl, int m);
        void InvokeChangeComboBoxIndex(ComboBox comboBoxCtrl, int m)
        {
            comboBoxCtrl.Invoke((ChangeComboBoxIndexHandler)ChangeComboBoxIndex, comboBoxCtrl, m);
        }
        void ChangeComboBoxIndex(ComboBox comboBoxCtrl, int m)
        {
            comboBoxCtrl.SelectedIndex = m;
        }

        protected delegate void DeleteItemHandler(ListBox listBoxCtrl, string Txt);
        void InvokeDeleteItem(ListBox listBoxCtrl, string Txt)
        {
            listBoxCtrl.Invoke((DeleteItemHandler)DeleteItem, listBoxCtrl, Txt);
        }
        void DeleteItem(ListBox listBoxCtrl, string Txt)
        {
            for (int i = 0; i < listBoxCtrl.Items.Count; i++)
            {
                if (listBoxCtrl.Items[i].ToString() == Txt)
                {
                    listBoxCtrl.Items.RemoveAt(i);
                }
            }
        }

        protected delegate void AddItemHandler(ListBox listBoxCtrl, string Txt);
        void InvokeAddItem(ListBox listBoxCtrl, string Txt)
        {
            listBoxCtrl.Invoke((AddItemHandler)AddItem, listBoxCtrl, Txt);
        }
        void AddItem(ListBox listBoxCtrl, string Txt)
        {
            listBoxCtrl.Items.Add(Txt);
        }
        #endregion

        private void Form1_Load(object sender, EventArgs e)
        {
            Control.CheckForIllegalCrossThreadCalls = false;

            string strHostName = Dns.GetHostName();  //得到本机的主机名
            IPHostEntry ipEntry = Dns.GetHostByName(strHostName); //取得本机IP
            for (int i = 0; i < ipEntry.AddressList.Length; i++)
            {
                comboBox1.Items.Add(ipEntry.AddressList[i].ToString());
            }
            comboBox1.Items.Add("127.0.0.1");
            comboBox2.SelectedIndex = 1;
            comboBox3.SelectedIndex = 1;
            comboBox4.SelectedIndex = 0;
            comboBox5.SelectedIndex = 0;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (myServer!=null)
                {
                    myServer.Stop();
                    myServer.ClientConnected -= new EventHandler<TCPEventArgs>(myServer_ClientConnected);
                    myServer.ClientDisconnected -= new EventHandler<TCPEventArgs>(myServer_ClientDisconnected);
                    myServer.ModbusTCPRequestReceived -= new EventHandler<ModbusTCPRequestEventArgs>(myServer_ModbusTCPRequestReceived);
                    myServer.ModbusTCPDataWrittenTo -= new EventHandler<ModbusTCPDataEventArgs>(myServer_ModbusTCPDataWrittenTo);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                throw;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {

            if (button1.Text == "启动监听")
            {
                if (comboBox1.SelectedIndex == -1)
                {
                    myServer = new ModbusTCP_Server(new IPEndPoint(IPAddress.Any, 502));
                }
                else
                {
                    myServer = new ModbusTCP_Server(comboBox1.Text, 502);
                }
                myServer.ClientConnected += new EventHandler<TCPEventArgs>(myServer_ClientConnected);
                myServer.ClientDisconnected+=new EventHandler<TCPEventArgs>(myServer_ClientDisconnected);
                myServer.ModbusTCPRequestReceived += new EventHandler<ModbusTCPRequestEventArgs>(myServer_ModbusTCPRequestReceived);
                myServer.ModbusTCPDataWrittenTo += new EventHandler<ModbusTCPDataEventArgs>(myServer_ModbusTCPDataWrittenTo);
                myServer.Start();

                button1.Text = "停止监听";
              
                for (int i = 0; i < 26; i++)
                {
                    dataGridView1.Rows.Add();
                    dataGridView1.Rows[i].SetValues(i, 0);
                    dataGridView2.Rows.Add();
                    dataGridView2.Rows[i].SetValues(i, 0);
                    dataGridView3.Rows.Add();
                    dataGridView3.Rows[i].SetValues(i, 0);
                    dataGridView4.Rows.Add();
                    dataGridView4.Rows[i].SetValues(i, 0);
                }
               
            }
            else
            {
                myServer.Stop();
                button1.Text = "启动监听";
            }

        }

        string currentType = "";
        int currentIndex = 0;
        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            DisplayType DisplayType_Cur = new DisplayType();
            foreach (DisplayType item in Enum.GetValues(typeof(DisplayType)))
            {
                if (GetDescription.description(item) == comboBox2.Text)
                {
                    DisplayType_Cur = item;
                    break;
                }
            }
            if (myServer != null)
            {
                string[] mNewData = myServer.DataFormatting(ModbusTCP.Server.RegisterType.HoldRegister, DisplayType_Cur, 1, dataGridView1.Rows.Count);
                for (int i = 0; i < mNewData.Length; i++)
                {
                    if (mNewData[i] == "")
                    {
                        dataGridView1.Rows[i].Cells[1].Value = "--";
                    }
                    else
                    {
                        dataGridView1.Rows[i].Cells[1].Value = mNewData[i];
                    }
                }

            }
        }

        //切换显示格式
        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            DisplayType DisplayType_Cur = new DisplayType();
            foreach (DisplayType item in Enum.GetValues(typeof(DisplayType)))
            {
                if (GetDescription.description(item) == comboBox3.Text)
                {
                    DisplayType_Cur = item;
                    break;
                }
            }
            if (myServer != null)
            {
                string[] mNewData = myServer.DataFormatting(ModbusTCP.Server.RegisterType.InputRegister, DisplayType_Cur, 1, dataGridView2.Rows.Count);
                for (int i = 0; i < dataGridView2.Rows.Count; i++)
                {
                    if (mNewData[i] == "")
                    {
                        dataGridView2.Rows[i].Cells[1].Value = "--";
                    }
                    else
                    {
                        dataGridView2.Rows[i].Cells[1].Value = mNewData[i];
                    }
                }

            }
        }

        #region dataGridView具体单元格双击修改
        //保持寄存器（读写）
        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            DisplayType DisplayType_Cur = new DisplayType();
            foreach (DisplayType item in Enum.GetValues(typeof(DisplayType)))
            {
                if (GetDescription.description(item) == comboBox2.Text)
                {
                    DisplayType_Cur = item;
                    break;
                }
            }
            if (e.ColumnIndex == 1)
            {
                FormEditRegister m = new FormEditRegister(this, dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value, 1, e.RowIndex, DisplayType_Cur);
                m.ShowDialog(this);
            }

        }

        //输入寄存器（只读）
        private void dataGridView2_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 1)
            {
                DisplayType DisplayType_Cur = new DisplayType();
                foreach (DisplayType item in Enum.GetValues(typeof(DisplayType)))
                {
                    if (GetDescription.description(item) == comboBox3.Text)
                    {
                        DisplayType_Cur = item;
                        break;
                    }
                }
                FormEditRegister m = new FormEditRegister(this, dataGridView2.Rows[e.RowIndex].Cells[e.ColumnIndex].Value, 2, e.RowIndex, DisplayType_Cur);
                m.ShowDialog(this);
            }
        }

        //Coil Discrete
        private void dataGridView3_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 1)
            {
                FormEditRegister m = new FormEditRegister(this, dataGridView3.Rows[e.RowIndex].Cells[e.ColumnIndex].Value, 3, e.RowIndex);
                m.ShowDialog(this);
            }
        }

        //Input Discrete
        private void dataGridView4_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 1)
            {
                FormEditRegister m = new FormEditRegister(this, dataGridView4.Rows[e.RowIndex].Cells[e.ColumnIndex].Value, 4, e.RowIndex);
                m.ShowDialog(this);
            }
        } 
        #endregion

        public bool UpdateDataGridView(DataGridView m_DataGridView, RegisterType RegisterType_Edit, DisplayType DisplayType_Cur)
        {
            bool bool_UpdateResult = false;
            try
            {
                string[] mNewData = myServer.DataFormatting(RegisterType_Edit, DisplayType_Cur, 1, m_DataGridView.Rows.Count);

                for (int i = 0; i < m_DataGridView.Rows.Count; i++)
                {
                    if (mNewData[i] == "")
                    {
                        m_DataGridView.Rows[i].Cells[1].Value = "--";
                    }
                    else
                    {
                        m_DataGridView.Rows[i].Cells[1].Value = mNewData[i];
                    }
                }   
            }
            catch (Exception)
            {
                return false;
            }
            bool_UpdateResult = true;
            return bool_UpdateResult;
        }

    }

  
}

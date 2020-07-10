using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ModbusTCP.Server;

namespace ModbusListener
{
    public partial class FormEditRegister : Form
    {
        int m_Index;
        int m_ControlIndex;
        DisplayType m_Type;
        Form1 m_Form;
        CheckBox[] CheckBox_BinaryArray = new CheckBox[16];
        public FormEditRegister(Form1 mForm, object value, int ControlIndex, int RowIndex, DisplayType mType = new DisplayType())
        {
            InitializeComponent();
            InitializeArray();
            textBox1.Text = value.ToString();
            m_ControlIndex = ControlIndex;
            m_Index = RowIndex;
            m_Form = mForm;
            m_Type = mType;
            #region 根据功能码、StartAddress、数据类型，确定界面显示
            if (ControlIndex == 1 || ControlIndex == 2)
            {
                if (mType == new DisplayType())
                {
                    //ushort
                    panel_Value.Visible = true;
                    panel_Value.BringToFront();
                    panel_Value.Location = new Point(0, 0);
                    textBox1.Text = value.ToString();
                    panel_OnOff.Visible = false;
                    panel_Binary.Visible = false;
                    this.Text = "Edit Register";
                }
                else
                {
                    //Follow DisplayType
                    switch (m_Type)
                    {
                        case DisplayType.Unsigned:
                            panel_Value.Visible = true;
                            panel_Value.BringToFront();
                            panel_Value.Location = new Point(0, 0);
                            textBox1.Text = value.ToString();
                            panel_OnOff.Visible = false;
                            panel_Binary.Visible = false;
                            this.Text = "Edit Register";
                            break;
                        case DisplayType.Signed:
                            panel_Value.Visible = true;
                            panel_Value.BringToFront();
                            panel_Value.Location = new Point(0, 0);
                            textBox1.Text = value.ToString();
                            panel_OnOff.Visible = false;
                            panel_Binary.Visible = false;
                            this.Text = "Edit Register";
                            break;
                        case DisplayType.Hex:
                            panel_Value.Visible = true;
                            panel_Value.BringToFront();
                            panel_Value.Location = new Point(0, 0);
                            textBox1.Text = value.ToString();
                            panel_OnOff.Visible = false;
                            panel_Binary.Visible = false;
                            this.Text = "Edit Register";
                            break;
                        case DisplayType.Binary:
                            panel_Binary.Visible = true;
                            panel_Binary.BringToFront();
                            panel_Binary.Location = new Point(0, 0);
                            for (int i = 0; i < value.ToString().Length; i++)
                            {
                                if (value.ToString().Substring(i, 1) == "1")
                                {
                                    CheckBox_BinaryArray[i].Checked = true;
                                }
                                else
                                {
                                    CheckBox_BinaryArray[i].Checked = false;
                                }
                            }
                            panel_OnOff.Visible = false;
                            panel_Value.Visible = false;
                            this.Text = "Edit Register Binary";
                            break;
                        case DisplayType.LongABCD:
                            panel_Value.Visible = true;
                            panel_Value.BringToFront();
                            panel_Value.Location = new Point(0, 0);
                            textBox1.Text = value.ToString();
                            panel_OnOff.Visible = false;
                            panel_Binary.Visible = false;
                            this.Text = "Edit Long Integer";
                            break;
                        case DisplayType.LongCDAB:
                            panel_Value.Visible = true;
                            panel_Value.BringToFront();
                            panel_Value.Location = new Point(0, 0);
                            textBox1.Text = value.ToString();
                            panel_OnOff.Visible = false;
                            panel_Binary.Visible = false;
                            this.Text = "Edit Long Integer";
                            break;
                        case DisplayType.LongBADC:
                            panel_Value.Visible = true;
                            panel_Value.BringToFront();
                            panel_Value.Location = new Point(0, 0);
                            textBox1.Text = value.ToString();
                            panel_OnOff.Visible = false;
                            panel_Binary.Visible = false;
                            this.Text = "Edit Long Integer";
                            break;
                        case DisplayType.LongDCBA:
                            panel_Value.Visible = true;
                            panel_Value.BringToFront();
                            panel_Value.Location = new Point(0, 0);
                            textBox1.Text = value.ToString();
                            panel_OnOff.Visible = false;
                            panel_Binary.Visible = false;
                            this.Text = "Edit Long Integer";
                            break;
                        case DisplayType.FloatABCD:
                            panel_Value.Visible = true;
                            panel_Value.BringToFront();
                            panel_Value.Location = new Point(0, 0);
                            textBox1.Text = value.ToString();
                            panel_OnOff.Visible = false;
                            panel_Binary.Visible = false;
                            this.Text = "Edit Floating Point";
                            break;
                        case DisplayType.FloatCDAB:
                            panel_Value.Visible = true;
                            panel_Value.BringToFront();
                            panel_Value.Location = new Point(0, 0);
                            textBox1.Text = value.ToString();
                            panel_OnOff.Visible = false;
                            panel_Binary.Visible = false;
                            this.Text = "Edit Floating Point";
                            break;
                        case DisplayType.FloatBADC:
                            panel_Value.Visible = true;
                            panel_Value.BringToFront();
                            panel_Value.Location = new Point(0, 0);
                            textBox1.Text = value.ToString();
                            panel_OnOff.Visible = false;
                            panel_Binary.Visible = false;
                            this.Text = "Edit Floating Point";
                            break;
                        case DisplayType.FloatDCBA:
                            panel_Value.Visible = true;
                            panel_Value.BringToFront();
                            panel_Value.Location = new Point(0, 0);
                            textBox1.Text = value.ToString();
                            panel_OnOff.Visible = false;
                            panel_Binary.Visible = false;
                            this.Text = "Edit Floating Point";
                            break;
                        default:
                            break;
                    }
                }
            }
            else
            {
                //bool
                panel_OnOff.Visible = true;
                panel_OnOff.BringToFront();
                panel_OnOff.Location = new Point(0, 0);
                if (value.ToString().Trim() == "1")
                {
                    radioButton_1.Checked = true;
                    radioButton_0.Checked = false;
                }
                else
                {
                    radioButton_1.Checked = false;
                    radioButton_0.Checked = true;
                }
                panel_Value.Visible = false;
                panel_Binary.Visible = false;
                this.Text = "Edit Coil";
            } 
            #endregion           
        }

        /// <summary>
        /// 二进制显示时调用的16个bool量
        /// </summary>
        private void InitializeArray()
        {
            CheckBox_BinaryArray[0] = checkBox1;
            CheckBox_BinaryArray[1] = checkBox2;
            CheckBox_BinaryArray[2] = checkBox3;
            CheckBox_BinaryArray[3] = checkBox4;
            CheckBox_BinaryArray[4] = checkBox5;
            CheckBox_BinaryArray[5] = checkBox6;
            CheckBox_BinaryArray[6] = checkBox7;
            CheckBox_BinaryArray[7] = checkBox8;

            CheckBox_BinaryArray[8] = checkBox9;
            CheckBox_BinaryArray[9] = checkBox10;
            CheckBox_BinaryArray[10] = checkBox11;
            CheckBox_BinaryArray[11] = checkBox12;
            CheckBox_BinaryArray[12] = checkBox13;
            CheckBox_BinaryArray[13] = checkBox14;
            CheckBox_BinaryArray[14] = checkBox15;
            CheckBox_BinaryArray[15] = checkBox16;
        }

        private void FormEditRegister_Load(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// 确认修改值
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_OK_Click(object sender, EventArgs e)
        {
            string[] m_NewData = new string[1];
            switch (m_ControlIndex)
            {
                case 1:
                    switch (m_Type)
                    {
                        case DisplayType.Unsigned:
                        case DisplayType.Signed:
                        case DisplayType.Hex:
                        case DisplayType.LongABCD:
                        case DisplayType.LongCDAB:
                        case DisplayType.LongBADC:
                        case DisplayType.LongDCBA:
                        case DisplayType.FloatABCD:
                        case DisplayType.FloatCDAB:
                        case DisplayType.FloatBADC:
                        case DisplayType.FloatDCBA:
                            m_NewData[0] = textBox1.Text;
                            m_Form.myServer.SetData(RegisterType.HoldRegister, m_Index, m_NewData, m_Type);
                            m_Form.UpdateDataGridView(m_Form.dataGridView1, RegisterType.HoldRegister, m_Type);
                            //Form1.m_Slave.DataStore.HoldingRegisters[m_Index + 1] = Convert.ToUInt16(textBox1.Text);
                            //m_Form.dataGridView1.Rows[m_Index].Cells[1].Value = Convert.ToUInt16(textBox1.Text);
                            break;
                        case DisplayType.Binary:
                            string mCurValue = "";
                            for (int i = 0; i < CheckBox_BinaryArray.Length; i++)
                            {
                                if (CheckBox_BinaryArray[i].Checked)
                                {
                                    mCurValue = mCurValue + "1";
                                }
                                else
                                {
                                    mCurValue = mCurValue + "0";
                                }
                            }
                            m_NewData[0] = mCurValue;
                            m_Form.myServer.SetData(RegisterType.HoldRegister, m_Index, m_NewData, DisplayType.Binary);                           
                            m_Form.dataGridView1.Rows[m_Index].Cells[1].Value = mCurValue;
                            break;
                        default:
                            break;
                    }
                    break;
                case 2:
                    switch (m_Type)
                    {
                        case DisplayType.Unsigned:
                        case DisplayType.Signed:
                        case DisplayType.Hex:
                        case DisplayType.LongABCD:
                        case DisplayType.LongCDAB:
                        case DisplayType.LongBADC:
                        case DisplayType.LongDCBA:
                        case DisplayType.FloatABCD:
                        case DisplayType.FloatCDAB:
                        case DisplayType.FloatBADC:
                        case DisplayType.FloatDCBA:
                            m_NewData[0] = textBox1.Text;
                            m_Form.myServer.SetData(RegisterType.InputRegister, m_Index, m_NewData, m_Type);
                            m_Form.UpdateDataGridView(m_Form.dataGridView2, RegisterType.InputRegister, m_Type);
                            break;
                        case DisplayType.Binary:
                            string mCurValue = "";
                            for (int i = 0; i < CheckBox_BinaryArray.Length; i++)
                            {
                                if (CheckBox_BinaryArray[i].Checked)
                                {
                                    mCurValue = mCurValue + "1";
                                }
                                else
                                {
                                    mCurValue = mCurValue + "0";
                                }
                            }
                            m_NewData[0] = mCurValue;
                            m_Form.myServer.SetData(RegisterType.InputRegister, m_Index, m_NewData, DisplayType.Binary);  
                            m_Form.dataGridView1.Rows[m_Index].Cells[1].Value = mCurValue;
                            break;
                        default:
                            break;
                    }                  
                    break;
                case 3:
                    #region CoilDiscretes
                    bool[] m_NewBool = new bool[1];
                    if (radioButton_0.Checked && !radioButton_1.Checked)
                    {
                        m_NewBool[0]= Convert.ToBoolean(0);
                        m_Form.myServer.SetData(RegisterType.CoilDiscrete, m_Index, m_NewBool);
                        m_Form.dataGridView3.Rows[m_Index].Cells[1].Value = 0;
                    }
                    else
                    {
                        if (!radioButton_0.Checked && radioButton_1.Checked)
                        {
                            m_NewBool[0] = Convert.ToBoolean(1);
                            m_Form.myServer.SetData(RegisterType.CoilDiscrete, m_Index, m_NewBool);
                            m_Form.dataGridView3.Rows[m_Index].Cells[1].Value = 1;
                        }
                    }
                    #endregion
                    break;
                case 4:
                    #region InputDiscretes
                    bool[] m_NewBool1 = new bool[1];
                    if (radioButton_0.Checked && !radioButton_1.Checked)
                    {
                        m_NewBool1[0] = Convert.ToBoolean(0);
                        m_Form.myServer.SetData(RegisterType.InputDiscrete, m_Index, m_NewBool1);
                        m_Form.dataGridView4.Rows[m_Index].Cells[1].Value = 0;

                    }
                    else
                    {
                        if (!radioButton_0.Checked && radioButton_1.Checked)
                        {
                            m_NewBool1[0] = Convert.ToBoolean(1);
                            m_Form.myServer.SetData(RegisterType.InputDiscrete, m_Index, m_NewBool1);
                            m_Form.dataGridView4.Rows[m_Index].Cells[1].Value = 1;

                        }
                    }
                    #endregion
                    break;
                default:
                    break;
            }
        }
    }
}

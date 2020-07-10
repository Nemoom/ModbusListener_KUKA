using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Reflection;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Collections.ObjectModel;

namespace ModbusTCP.Server
{
    public class ModbusTCP_Server
    {
        private TcpListener tcpListener;
        private static Modbus.Device.ModbusTcpSlave m_Slave;
        /// <summary>
        /// 客户端会话列表
        /// </summary>
        private List<TcpClient> _clients;        

        #region 构造器
        /// <summary>
        /// 同步TCP服务器
        /// </summary>
        /// <param name="listenPort">监听的端口</param>
        public ModbusTCP_Server(int listenPort)
            : this(IPAddress.Any, listenPort)
        {
        }

        /// <summary>
        /// 同步TCP服务器
        /// </summary>
        /// <param name="localEP">监听的终结点</param>
        public ModbusTCP_Server(IPEndPoint localEP)
            : this(localEP.Address, localEP.Port)
        {
        }

        public ModbusTCP_Server(string ip, int listenPort = 502)
            : this(IPAddress.Parse(ip), listenPort)
        {
        }

        /// <summary>
        /// ModubusTCP服务器
        /// </summary>
        /// <param name="localIPAddress">监听的IP地址</param>
        /// <param name="listenPort">监听的端口</param>
        public ModbusTCP_Server(IPAddress localIPAddress, int listenPort)
        {
            this.Address = localIPAddress;
            this.Port = listenPort;

            _clients = new List<TcpClient>();
            tcpListener = new TcpListener(new IPEndPoint(this.Address, this.Port));
        }
        #endregion

        #region Properties
        /// <summary>
        /// 服务器是否正在运行
        /// </summary>
        public bool IsRunning { get; private set; }
        /// <summary>
        /// 监听的IP地址
        /// </summary>
        public IPAddress Address { get; private set; }
        /// <summary>
        /// 监听的端口
        /// </summary>
        public int Port { get; private set; }
        #endregion

        #region Method
        /// <summary>
        /// 启动服务器
        /// </summary>
        public void Start()
        {
            if (!IsRunning)
            {               
                tcpListener.Start();
               
                m_Slave = Modbus.Device.ModbusTcpSlave.CreateTcp(0, tcpListener);
                m_Slave.ModbusSlaveRequestReceived += new EventHandler<Modbus.Device.ModbusSlaveRequestEventArgs>(m_Slave_ModbusSlaveRequestReceived);
                
                m_Slave.DataStore = Modbus.Data.DataStoreFactory.CreateDefaultDataStore();
                m_Slave.DataStore.DataStoreWrittenTo += new EventHandler<Modbus.Data.DataStoreEventArgs>(DataStore_DataStoreWrittenTo);
                m_Slave.Listen();

                IsRunning = true;

                //启动线程监控接入Master的变化，触发ClientConnected&ClientDisconnected
                Thread thread = new Thread(Accept);
                thread.IsBackground = true;
                thread.Start();
            }
        }
        
        /// <summary>
        /// 开始进行监听客户端连接断开情况
        /// </summary>
        private void Accept()
        {
            while (IsRunning)
            {
                try
                {
                    ReadOnlyCollection<TcpClient> mCur_Masters = m_Slave.Masters;
                    //有客户端断开连接
                    for (int i = 0; i < _clients.Count; i++)
                    {
                        bool _clientsConnected = false;

                        foreach (TcpClient item in mCur_Masters)
                        {
                            if (item.Client.RemoteEndPoint == _clients[i].Client.RemoteEndPoint)
                            {
                                _clientsConnected = true;
                                break;
                            }
                        }

                        if (!_clientsConnected)
                        {
                            TCPEventArgs m = new TCPEventArgs(_clients[i]);
                            RaiseClientDisconnected(m);
                        }
                    }

                    //有新的客户端连接
                    for (int i = 0; i < mCur_Masters.Count; i++)
                    {
                        bool _newConnected = true;
                        for (int j = 0; j < _clients.Count; j++)
                        {
                            if (_clients[j].Client.RemoteEndPoint == mCur_Masters[i].Client.RemoteEndPoint)
                            {
                                _newConnected = false;
                                break;
                            }
                        }
                        if (_newConnected)
                        {
                            TCPEventArgs m = new TCPEventArgs(mCur_Masters[i]);
                            RaiseClientConnected(m);
                        }
                    }

                    //更新_clients
                    _clients = new List<TcpClient>();
                    for (int i = 0; i < mCur_Masters.Count; i++)
                    {
                        _clients.Add(mCur_Masters[i]);
                    }
                }
                catch (Exception)
                {

                }
            }
        }
      
        /// <summary>
        /// 停止服务器
        /// </summary>
        public void Stop()
        {
            if (IsRunning)
            {
                try
                {
                    IsRunning = false;

                    for (int i = 0; i < _clients.Count; i++)
                    {
                        TCPEventArgs m = new TCPEventArgs(_clients[i]);
                        RaiseClientDisconnected(m);
                    }
                    _clients.Clear();
                    m_Slave.ModbusSlaveRequestReceived -= new EventHandler<Modbus.Device.ModbusSlaveRequestEventArgs>(m_Slave_ModbusSlaveRequestReceived);
                    m_Slave.DataStore.DataStoreWrittenTo -= new EventHandler<Modbus.Data.DataStoreEventArgs>(DataStore_DataStoreWrittenTo);
                    m_Slave.Dispose();
                    tcpListener.Stop();
                }
                catch (Exception)
                {
                    
                }
            }
        }    

        /// <summary>
        /// 数据批量格式化
        /// </summary>
        /// <param name="RegisterType_Edit">Modbus功能码</param>
        /// <param name="CurType">预格式化类型</param>
        /// <param name="StartAddress">起始位（Start from 1）</param>
        /// <param name="DataLength">数据长度</param>
        /// <returns>格式化后的数据</returns>
        public string[] DataFormatting(RegisterType RegisterType_Edit, DisplayType CurType, int StartAddress, int DataLength)
        {
            string[] DatasFormatted = new string[DataLength];
            try
            {
                switch (RegisterType_Edit)
                {
                    case RegisterType.CoilDiscrete:
                        for (int i = 0; i < DatasFormatted.Length; i++)
                        {
                            DatasFormatted[i] = m_Slave.DataStore.CoilDiscretes[StartAddress + i].ToString();
                        }
                        break;
                    case RegisterType.InputDiscrete:
                        for (int i = 0; i < DatasFormatted.Length; i++)
                        {
                            DatasFormatted[i] = m_Slave.DataStore.InputDiscretes[StartAddress + i].ToString();
                        }
                        break;
                    case RegisterType.InputRegister:
                        #region InputRegister
                        switch (CurType)
                        {
                            case DisplayType.Unsigned:
                                for (int i = 0; i < DatasFormatted.Length; i++)
                                {
                                    DatasFormatted[i] = m_Slave.DataStore.InputRegisters[StartAddress + i].ToString();
                                }
                                break;
                            case DisplayType.Signed:
                                for (int i = 0; i < DatasFormatted.Length; i++)
                                {
                                    DatasFormatted[i] = Convert.ToInt16(Convert.ToString(m_Slave.DataStore.InputRegisters[StartAddress + i], 2).PadLeft(16, '0'), 2).ToString();
                                }
                                break;
                            case DisplayType.Hex:
                                for (int i = 0; i < DatasFormatted.Length; i++)
                                {
                                    DatasFormatted[i] = "0x" + Convert.ToString(m_Slave.DataStore.InputRegisters[StartAddress + i], 16).PadLeft(4, '0').ToUpper();
                                }
                                break;
                            case DisplayType.Binary:
                                for (int i = 0; i < DatasFormatted.Length; i++)
                                {
                                    DatasFormatted[i] = Convert.ToString(m_Slave.DataStore.InputRegisters[StartAddress + i], 2).PadLeft(16, '0');
                                }
                                break;
                            case DisplayType.LongABCD:
                                for (int i = 0; i < DatasFormatted.Length; i = i + 2)
                                {
                                    byte[] ba = BitConverter.GetBytes(m_Slave.DataStore.InputRegisters[StartAddress + i]);
                                    byte[] bb = BitConverter.GetBytes(m_Slave.DataStore.InputRegisters[StartAddress + i + 1]);
                                    byte[] b = new byte[4];
                                    b[0] = bb[0];
                                    b[1] = bb[1];
                                    b[2] = ba[0];
                                    b[3] = ba[1];
                                    Int32 m = BitConverter.ToInt32(b, 0);
                                    DatasFormatted[i] = m.ToString();
                                    DatasFormatted[i + 1] = "";
                                }
                                break;
                            case DisplayType.LongCDAB:
                                for (int i = 0; i < DatasFormatted.Length; i = i + 2)
                                {
                                    byte[] ba = BitConverter.GetBytes(m_Slave.DataStore.InputRegisters[StartAddress + i]);
                                    byte[] bb = BitConverter.GetBytes(m_Slave.DataStore.InputRegisters[StartAddress + i + 1]);
                                    byte[] b = new byte[4];
                                    b[0] = ba[0];
                                    b[1] = ba[1];
                                    b[2] = bb[0];
                                    b[3] = bb[1];
                                    Int32 m = BitConverter.ToInt32(b, 0);
                                    DatasFormatted[i] = m.ToString();
                                    DatasFormatted[i + 1] = "";
                                }
                                break;
                            case DisplayType.LongBADC:
                                for (int i = 0; i < DatasFormatted.Length; i = i + 2)
                                {
                                    byte[] ba = BitConverter.GetBytes(m_Slave.DataStore.InputRegisters[StartAddress + i]);
                                    byte[] bb = BitConverter.GetBytes(m_Slave.DataStore.InputRegisters[StartAddress + i + 1]);
                                    byte[] b = new byte[4];
                                    Array.Reverse(ba);
                                    Array.Reverse(bb);
                                    b[0] = bb[0];
                                    b[1] = bb[1];
                                    b[2] = ba[0];
                                    b[3] = ba[1];
                                    Int32 m = BitConverter.ToInt32(b, 0);
                                    DatasFormatted[i] = m.ToString();
                                    DatasFormatted[i + 1] = "";
                                }
                                break;
                            case DisplayType.LongDCBA:
                                for (int i = 0; i < DatasFormatted.Length; i = i + 2)
                                {
                                    byte[] ba = BitConverter.GetBytes(m_Slave.DataStore.InputRegisters[StartAddress + i]);
                                    byte[] bb = BitConverter.GetBytes(m_Slave.DataStore.InputRegisters[StartAddress + i + 1]);
                                    byte[] b = new byte[4];
                                    Array.Reverse(ba);
                                    Array.Reverse(bb);
                                    b[0] = ba[0];
                                    b[1] = ba[1];
                                    b[2] = bb[0];
                                    b[3] = bb[1];
                                    Int32 m = BitConverter.ToInt32(b, 0);
                                    DatasFormatted[i] = m.ToString();
                                    DatasFormatted[i + 1] = "";
                                }
                                break;
                            case DisplayType.FloatABCD:
                                for (int i = 0; i < DatasFormatted.Length; i = i + 2)
                                {
                                    byte[] ba = BitConverter.GetBytes(m_Slave.DataStore.InputRegisters[StartAddress + i]);
                                    byte[] bb = BitConverter.GetBytes(m_Slave.DataStore.InputRegisters[StartAddress + i + 1]);
                                    byte[] b = new byte[4];
                                    b[0] = bb[0];
                                    b[1] = bb[1];
                                    b[2] = ba[0];
                                    b[3] = ba[1];
                                    float m = BitConverter.ToSingle(b, 0);
                                    DatasFormatted[i] = m.ToString();
                                    DatasFormatted[i + 1] = "";
                                }
                                break;
                            case DisplayType.FloatCDAB:
                                for (int i = 0; i < DatasFormatted.Length; i = i + 2)
                                {
                                    byte[] ba = BitConverter.GetBytes(m_Slave.DataStore.InputRegisters[StartAddress + i]);
                                    byte[] bb = BitConverter.GetBytes(m_Slave.DataStore.InputRegisters[StartAddress + i + 1]);
                                    byte[] b = new byte[4];
                                    b[0] = ba[0];
                                    b[1] = ba[1];
                                    b[2] = bb[0];
                                    b[3] = bb[1];
                                    float m = BitConverter.ToSingle(b, 0);
                                    DatasFormatted[i] = m.ToString();
                                    DatasFormatted[i + 1] = "";
                                }
                                break;
                            case DisplayType.FloatBADC:
                                for (int i = 0; i < DatasFormatted.Length; i = i + 2)
                                {
                                    byte[] ba = BitConverter.GetBytes(m_Slave.DataStore.InputRegisters[StartAddress + i]);
                                    byte[] bb = BitConverter.GetBytes(m_Slave.DataStore.InputRegisters[StartAddress + i + 1]);
                                    byte[] b = new byte[4];
                                    Array.Reverse(ba);
                                    Array.Reverse(bb);
                                    b[0] = bb[0];
                                    b[1] = bb[1];
                                    b[2] = ba[0];
                                    b[3] = ba[1];
                                    float m = BitConverter.ToSingle(b, 0);
                                    DatasFormatted[i] = m.ToString();
                                    DatasFormatted[i + 1] = "";
                                }
                                break;
                            case DisplayType.FloatDCBA:
                                for (int i = 0; i < DatasFormatted.Length; i = i + 2)
                                {
                                    byte[] ba = BitConverter.GetBytes(m_Slave.DataStore.InputRegisters[StartAddress + i]);
                                    byte[] bb = BitConverter.GetBytes(m_Slave.DataStore.InputRegisters[StartAddress + i + 1]);
                                    byte[] b = new byte[4];
                                    Array.Reverse(ba);
                                    Array.Reverse(bb);
                                    b[0] = ba[0];
                                    b[1] = ba[1];
                                    b[2] = bb[0];
                                    b[3] = bb[1];
                                    float m = BitConverter.ToSingle(b, 0);
                                    DatasFormatted[i] = m.ToString();
                                    DatasFormatted[i + 1] = "";
                                }
                                break;
                        }
                        #endregion
                        break;
                    case RegisterType.HoldRegister:
                        #region HoldRegister
                        switch (CurType)
                        {
                            case DisplayType.Unsigned:
                                for (int i = 0; i < DatasFormatted.Length; i++)
                                {
                                    DatasFormatted[i] = m_Slave.DataStore.HoldingRegisters[StartAddress + i].ToString();
                                }
                                break;
                            case DisplayType.Signed:
                                for (int i = 0; i < DatasFormatted.Length; i++)
                                {
                                    DatasFormatted[i] = Convert.ToInt16(Convert.ToString(m_Slave.DataStore.HoldingRegisters[StartAddress + i], 2).PadLeft(16, '0'), 2).ToString();
                                }
                                break;
                            case DisplayType.Hex:
                                for (int i = 0; i < DatasFormatted.Length; i++)
                                {
                                    DatasFormatted[i] = "0x" + Convert.ToString(m_Slave.DataStore.HoldingRegisters[StartAddress + i], 16).PadLeft(4, '0').ToUpper();
                                }
                                break;
                            case DisplayType.Binary:
                                for (int i = 0; i < DatasFormatted.Length; i++)
                                {
                                    DatasFormatted[i] = Convert.ToString(m_Slave.DataStore.HoldingRegisters[StartAddress + i], 2).PadLeft(16, '0');
                                }
                                break;
                            case DisplayType.LongABCD:
                                for (int i = 0; i < DatasFormatted.Length; i = i + 2)
                                {
                                    byte[] ba = BitConverter.GetBytes(m_Slave.DataStore.HoldingRegisters[StartAddress + i]);
                                    byte[] bb = BitConverter.GetBytes(m_Slave.DataStore.HoldingRegisters[StartAddress + i + 1]);
                                    byte[] b = new byte[4];
                                    b[0] = bb[0];
                                    b[1] = bb[1];
                                    b[2] = ba[0];
                                    b[3] = ba[1];
                                    Int32 m = BitConverter.ToInt32(b, 0);
                                    DatasFormatted[i] = m.ToString();
                                    DatasFormatted[i + 1] = "";
                                }
                                break;
                            case DisplayType.LongCDAB:
                                for (int i = 0; i < DatasFormatted.Length; i = i + 2)
                                {
                                    byte[] ba = BitConverter.GetBytes(m_Slave.DataStore.HoldingRegisters[StartAddress + i]);
                                    byte[] bb = BitConverter.GetBytes(m_Slave.DataStore.HoldingRegisters[StartAddress + i + 1]);
                                    byte[] b = new byte[4];
                                    b[0] = ba[0];
                                    b[1] = ba[1];
                                    b[2] = bb[0];
                                    b[3] = bb[1];
                                    Int32 m = BitConverter.ToInt32(b, 0);
                                    DatasFormatted[i] = m.ToString();
                                    DatasFormatted[i + 1] = "";
                                }
                                break;
                            case DisplayType.LongBADC:
                                for (int i = 0; i < DatasFormatted.Length; i = i + 2)
                                {
                                    byte[] ba = BitConverter.GetBytes(m_Slave.DataStore.HoldingRegisters[StartAddress + i]);
                                    byte[] bb = BitConverter.GetBytes(m_Slave.DataStore.HoldingRegisters[StartAddress + i + 1]);
                                    byte[] b = new byte[4];
                                    Array.Reverse(ba);
                                    Array.Reverse(bb);
                                    b[0] = bb[0];
                                    b[1] = bb[1];
                                    b[2] = ba[0];
                                    b[3] = ba[1];
                                    Int32 m = BitConverter.ToInt32(b, 0);
                                    DatasFormatted[i] = m.ToString();
                                    DatasFormatted[i + 1] = "";
                                }
                                break;
                            case DisplayType.LongDCBA:
                                for (int i = 0; i < DatasFormatted.Length; i = i + 2)
                                {
                                    byte[] ba = BitConverter.GetBytes(m_Slave.DataStore.HoldingRegisters[StartAddress + i]);
                                    byte[] bb = BitConverter.GetBytes(m_Slave.DataStore.HoldingRegisters[StartAddress + i + 1]);
                                    byte[] b = new byte[4];
                                    Array.Reverse(ba);
                                    Array.Reverse(bb);
                                    b[0] = ba[0];
                                    b[1] = ba[1];
                                    b[2] = bb[0];
                                    b[3] = bb[1];
                                    Int32 m = BitConverter.ToInt32(b, 0);
                                    DatasFormatted[i] = m.ToString();
                                    DatasFormatted[i + 1] = "";
                                }
                                break;
                            case DisplayType.FloatABCD:
                                for (int i = 0; i < DatasFormatted.Length; i = i + 2)
                                {
                                    byte[] ba = BitConverter.GetBytes(m_Slave.DataStore.HoldingRegisters[StartAddress + i]);
                                    byte[] bb = BitConverter.GetBytes(m_Slave.DataStore.HoldingRegisters[StartAddress + i + 1]);
                                    byte[] b = new byte[4];
                                    b[0] = bb[0];
                                    b[1] = bb[1];
                                    b[2] = ba[0];
                                    b[3] = ba[1];
                                    float m = BitConverter.ToSingle(b, 0);
                                    DatasFormatted[i] = m.ToString();
                                    DatasFormatted[i + 1] = "";
                                }
                                break;
                            case DisplayType.FloatCDAB:
                                for (int i = 0; i < DatasFormatted.Length; i = i + 2)
                                {
                                    byte[] ba = BitConverter.GetBytes(m_Slave.DataStore.HoldingRegisters[StartAddress + i]);
                                    byte[] bb = BitConverter.GetBytes(m_Slave.DataStore.HoldingRegisters[StartAddress + i + 1]);
                                    byte[] b = new byte[4];
                                    b[0] = ba[0];
                                    b[1] = ba[1];
                                    b[2] = bb[0];
                                    b[3] = bb[1];
                                    float m = BitConverter.ToSingle(b, 0);
                                    DatasFormatted[i] = m.ToString();
                                    DatasFormatted[i + 1] = "";
                                }
                                break;
                            case DisplayType.FloatBADC:
                                for (int i = 0; i < DatasFormatted.Length; i = i + 2)
                                {
                                    byte[] ba = BitConverter.GetBytes(m_Slave.DataStore.HoldingRegisters[StartAddress + i]);
                                    byte[] bb = BitConverter.GetBytes(m_Slave.DataStore.HoldingRegisters[StartAddress + i + 1]);
                                    byte[] b = new byte[4];
                                    Array.Reverse(ba);
                                    Array.Reverse(bb);
                                    b[0] = bb[0];
                                    b[1] = bb[1];
                                    b[2] = ba[0];
                                    b[3] = ba[1];
                                    float m = BitConverter.ToSingle(b, 0);
                                    DatasFormatted[i] = m.ToString();
                                    DatasFormatted[i + 1] = "";
                                }
                                break;
                            case DisplayType.FloatDCBA:
                                for (int i = 0; i < DatasFormatted.Length; i = i + 2)
                                {
                                    byte[] ba = BitConverter.GetBytes(m_Slave.DataStore.HoldingRegisters[StartAddress + i]);
                                    byte[] bb = BitConverter.GetBytes(m_Slave.DataStore.HoldingRegisters[StartAddress + i + 1]);
                                    byte[] b = new byte[4];
                                    Array.Reverse(ba);
                                    Array.Reverse(bb);
                                    b[0] = ba[0];
                                    b[1] = ba[1];
                                    b[2] = bb[0];
                                    b[3] = bb[1];
                                    float m = BitConverter.ToSingle(b, 0);
                                    DatasFormatted[i] = m.ToString();
                                    DatasFormatted[i + 1] = "";
                                }
                                break;
                        }
                        #endregion
                        break;
                }
            }
            catch (Exception)
            {
                return null;
            }
            return DatasFormatted;
        }

        public bool SetData(RegisterType RegisterType_Edit, int StartAddress, bool[] Data_New)
        {
            bool bool_SetResult = false;
            try
            {
                switch (RegisterType_Edit)
                {
                    case RegisterType.CoilDiscrete:
                        for (int i = 0; i < Data_New.Length; i++)
                        {
                            m_Slave.DataStore.CoilDiscretes[StartAddress + 1 + i] = Data_New[i];
                        }
                        break;
                    case RegisterType.InputDiscrete:
                        for (int i = 0; i < Data_New.Length; i++)
                        {
                            m_Slave.DataStore.InputDiscretes[StartAddress + 1 + i] = Data_New[i];
                        }
                        break;

                    default:
                        return false;
                }
                bool_SetResult = true;
            }
            catch (Exception)
            {
                return false;
            }
            return bool_SetResult;
        }

        /// <summary>
        /// 主动修改寄存器数值
        /// </summary>
        /// <param name="RegisterType_Edit">修改寄存器类型</param>
        /// <param name="StartAddress">起始地址</param>
        /// <param name="Data_New">待写入数据</param>
        /// <param name="DisplayType_Cur">写入数据类型</param>
        /// <returns>设置是否成功</returns>
        public bool SetData(RegisterType RegisterType_Edit, int StartAddress, string[] Data_New, DisplayType DisplayType_Cur = DisplayType.Unsigned)
        {
            bool bool_SetResult = false;
            try
            {
                switch (RegisterType_Edit)
                {
                    case RegisterType.CoilDiscrete:
                        for (int i = 0; i < Data_New.Length; i++)
                        {
                            m_Slave.DataStore.CoilDiscretes[StartAddress + 1 + i] = Convert.ToBoolean(Data_New[i]);
                        }
                        break;
                    case RegisterType.InputDiscrete:
                        for (int i = 0; i < Data_New.Length; i++)
                        {
                            m_Slave.DataStore.InputDiscretes[StartAddress + 1 + i] = Convert.ToBoolean(Data_New[i]);
                        }
                        break;
                    case RegisterType.InputRegister:
                        ushort[] Data_ushort = new ushort[1];
                        switch (DisplayType_Cur)
                        {
                            case DisplayType.Unsigned:
                                Data_ushort = new ushort[Data_New.Length];
                                for (int i = 0; i < Data_New.Length; i++)
                                {
                                    Data_ushort[i] = Convert.ToUInt16(Data_New[i]);
                                }
                                break;
                            case DisplayType.Signed:
                                Data_ushort = new ushort[Data_New.Length];
                                for (int i = 0; i < Data_New.Length; i++)
                                {
                                    Int16 m_Data = Convert.ToInt16(Data_New[i]);
                                    Data_ushort[i] = Convert.ToUInt16(m_Data);
                                }
                                break;
                            case DisplayType.Hex:
                                Data_ushort = new ushort[Data_New.Length];
                                for (int i = 0; i < Data_New.Length; i++)
                                {
                                    if (Data_New[i].StartsWith("0x"))
                                    {
                                        Data_ushort[i] = Convert.ToUInt16(Data_New[i], 16);
                                    }
                                    else
                                    {
                                        Data_ushort[i] = Convert.ToUInt16("0x" + Data_New[i], 16);
                                    }
                                }
                                break;
                            case DisplayType.Binary:
                                Data_ushort = new ushort[Data_New.Length];
                                for (int i = 0; i < Data_New.Length; i++)
                                {
                                    Data_ushort[i] = Convert.ToUInt16(Data_New[i], 2);
                                }
                                break;
                            case DisplayType.LongABCD:
                                Data_ushort = new ushort[Data_New.Length * 2];
                                for (int i = 0; i < Data_New.Length; i++)
                                {
                                    Int32 m_Data = Convert.ToInt32(Data_New[i]);
                                    byte[] m_Bytes = BitConverter.GetBytes(m_Data);
                                    byte[] m_Bytes_AB = new Byte[2];
                                    byte[] m_Bytes_CD = new Byte[2];
                                    m_Bytes_AB[0] = m_Bytes[0];
                                    m_Bytes_AB[1] = m_Bytes[1];
                                    m_Bytes_CD[0] = m_Bytes[2];
                                    m_Bytes_CD[1] = m_Bytes[3];
                                    Data_ushort[2 * i] = BitConverter.ToUInt16(m_Bytes_CD, 0);
                                    Data_ushort[2 * i + 1] = BitConverter.ToUInt16(m_Bytes_AB, 0);
                                }
                                break;
                            case DisplayType.LongCDAB:
                                Data_ushort = new ushort[Data_New.Length * 2];
                                for (int i = 0; i < Data_New.Length; i++)
                                {
                                    Int32 m_Data = Convert.ToInt32(Data_New[i]);
                                    byte[] m_Bytes = BitConverter.GetBytes(m_Data);
                                    byte[] m_Bytes_AB = new Byte[2];
                                    byte[] m_Bytes_CD = new Byte[2];
                                    m_Bytes_AB[0] = m_Bytes[0];
                                    m_Bytes_AB[1] = m_Bytes[1];
                                    m_Bytes_CD[0] = m_Bytes[2];
                                    m_Bytes_CD[1] = m_Bytes[3];
                                    Data_ushort[2 * i] = BitConverter.ToUInt16(m_Bytes_AB, 0);
                                    Data_ushort[2 * i + 1] = BitConverter.ToUInt16(m_Bytes_CD, 0);
                                }
                                break;
                            case DisplayType.LongBADC:
                                Data_ushort = new ushort[Data_New.Length * 2];
                                for (int i = 0; i < Data_New.Length; i++)
                                {
                                    Int32 m_Data = Convert.ToInt32(Data_New[i]);
                                    byte[] m_Bytes = BitConverter.GetBytes(m_Data);
                                    byte[] m_Bytes_BA = new Byte[2];
                                    byte[] m_Bytes_DC = new Byte[2];
                                    m_Bytes_BA[0] = m_Bytes[1];
                                    m_Bytes_BA[1] = m_Bytes[0];
                                    m_Bytes_DC[0] = m_Bytes[3];
                                    m_Bytes_DC[1] = m_Bytes[2];
                                    Data_ushort[2 * i] = BitConverter.ToUInt16(m_Bytes_DC, 0);
                                    Data_ushort[2 * i + 1] = BitConverter.ToUInt16(m_Bytes_BA, 0);
                                }
                                break;
                            case DisplayType.LongDCBA:
                                Data_ushort = new ushort[Data_New.Length * 2];
                                for (int i = 0; i < Data_New.Length; i++)
                                {
                                    Int32 m_Data = Convert.ToInt32(Data_New[i]);
                                    byte[] m_Bytes = BitConverter.GetBytes(m_Data);
                                    byte[] m_Bytes_BA = new Byte[2];
                                    byte[] m_Bytes_DC = new Byte[2];
                                    m_Bytes_BA[0] = m_Bytes[1];
                                    m_Bytes_BA[1] = m_Bytes[0];
                                    m_Bytes_DC[0] = m_Bytes[3];
                                    m_Bytes_DC[1] = m_Bytes[2];
                                    Data_ushort[2 * i] = BitConverter.ToUInt16(m_Bytes_BA, 0);
                                    Data_ushort[2 * i + 1] = BitConverter.ToUInt16(m_Bytes_DC, 0);
                                }
                                break;
                            case DisplayType.FloatABCD:
                                Data_ushort = new ushort[Data_New.Length * 2];
                                for (int i = 0; i < Data_New.Length; i++)
                                {
                                    float m_Data = Convert.ToSingle(Data_New[i]);
                                    byte[] m_Bytes = BitConverter.GetBytes(m_Data);
                                    byte[] m_Bytes_AB = new Byte[2];
                                    byte[] m_Bytes_CD = new Byte[2];
                                    m_Bytes_AB[0] = m_Bytes[0];
                                    m_Bytes_AB[1] = m_Bytes[1];
                                    m_Bytes_CD[0] = m_Bytes[2];
                                    m_Bytes_CD[1] = m_Bytes[3];
                                    Data_ushort[2 * i] = BitConverter.ToUInt16(m_Bytes_CD, 0);
                                    Data_ushort[2 * i + 1] = BitConverter.ToUInt16(m_Bytes_AB, 0);
                                }
                                break;
                            case DisplayType.FloatCDAB:
                                Data_ushort = new ushort[Data_New.Length * 2];
                                for (int i = 0; i < Data_New.Length; i++)
                                {
                                    float m_Data = Convert.ToSingle(Data_New[i]);
                                    byte[] m_Bytes = BitConverter.GetBytes(m_Data);
                                    byte[] m_Bytes_AB = new Byte[2];
                                    byte[] m_Bytes_CD = new Byte[2];
                                    m_Bytes_AB[0] = m_Bytes[0];
                                    m_Bytes_AB[1] = m_Bytes[1];
                                    m_Bytes_CD[0] = m_Bytes[2];
                                    m_Bytes_CD[1] = m_Bytes[3];
                                    Data_ushort[2 * i] = BitConverter.ToUInt16(m_Bytes_AB, 0);
                                    Data_ushort[2 * i + 1] = BitConverter.ToUInt16(m_Bytes_CD, 0);
                                }
                                break;
                            case DisplayType.FloatBADC:
                                Data_ushort = new ushort[Data_New.Length * 2];
                                for (int i = 0; i < Data_New.Length; i++)
                                {
                                    float m_Data = Convert.ToSingle(Data_New[i]);
                                    byte[] m_Bytes = BitConverter.GetBytes(m_Data);
                                    byte[] m_Bytes_BA = new Byte[2];
                                    byte[] m_Bytes_DC = new Byte[2];
                                    m_Bytes_BA[0] = m_Bytes[1];
                                    m_Bytes_BA[1] = m_Bytes[0];
                                    m_Bytes_DC[0] = m_Bytes[3];
                                    m_Bytes_DC[1] = m_Bytes[2];
                                    Data_ushort[2 * i] = BitConverter.ToUInt16(m_Bytes_DC, 0);
                                    Data_ushort[2 * i + 1] = BitConverter.ToUInt16(m_Bytes_BA, 0);
                                }
                                break;
                            case DisplayType.FloatDCBA:
                                Data_ushort = new ushort[Data_New.Length * 2];
                                for (int i = 0; i < Data_New.Length; i++)
                                {
                                    float m_Data = Convert.ToSingle(Data_New[i]);
                                    byte[] m_Bytes = BitConverter.GetBytes(m_Data);
                                    byte[] m_Bytes_BA = new Byte[2];
                                    byte[] m_Bytes_DC = new Byte[2];
                                    m_Bytes_BA[0] = m_Bytes[1];
                                    m_Bytes_BA[1] = m_Bytes[0];
                                    m_Bytes_DC[0] = m_Bytes[3];
                                    m_Bytes_DC[1] = m_Bytes[2];
                                    Data_ushort[2 * i] = BitConverter.ToUInt16(m_Bytes_BA, 0);
                                    Data_ushort[2 * i + 1] = BitConverter.ToUInt16(m_Bytes_DC, 0);
                                }
                                break;
                            default:
                                break;
                        }
                        for (int i = 0; i < Data_ushort.Length; i++)
                        {
                            m_Slave.DataStore.InputRegisters[StartAddress + 1 + i] = Data_ushort[i];
                        }
                        break;
                    case RegisterType.HoldRegister:
                        Data_ushort = new ushort[1];
                        switch (DisplayType_Cur)
                        {
                            case DisplayType.Unsigned:
                                Data_ushort = new ushort[Data_New.Length];
                                for (int i = 0; i < Data_New.Length; i++)
                                {
                                    Data_ushort[i] = Convert.ToUInt16(Data_New[i]);
                                }
                                break;
                            case DisplayType.Signed:
                                Data_ushort = new ushort[Data_New.Length];
                                for (int i = 0; i < Data_New.Length; i++)
                                {
                                    Int16 m_Data = Convert.ToInt16(Data_New[i]);
                                    Data_ushort[i] = Convert.ToUInt16(m_Data);
                                }
                                break;
                            case DisplayType.Hex:
                                Data_ushort = new ushort[Data_New.Length];
                                for (int i = 0; i < Data_New.Length; i++)
                                {
                                    if (Data_New[i].StartsWith("0x"))
                                    {
                                        Data_ushort[i] = Convert.ToUInt16(Data_New[i], 16);
                                    }
                                    else
                                    {
                                        Data_ushort[i] = Convert.ToUInt16("0x" + Data_New[i], 16);
                                    }
                                }
                                break;
                            case DisplayType.Binary:
                                Data_ushort = new ushort[Data_New.Length];
                                for (int i = 0; i < Data_New.Length; i++)
                                {
                                    Data_ushort[i] = Convert.ToUInt16(Data_New[i], 2);
                                }
                                break;
                            case DisplayType.LongABCD:
                                Data_ushort = new ushort[Data_New.Length * 2];
                                for (int i = 0; i < Data_New.Length; i++)
                                {
                                    Int32 m_Data = Convert.ToInt32(Data_New[i]);
                                    byte[] m_Bytes = BitConverter.GetBytes(m_Data);
                                    byte[] m_Bytes_AB = new Byte[2];
                                    byte[] m_Bytes_CD = new Byte[2];
                                    m_Bytes_AB[0] = m_Bytes[0];
                                    m_Bytes_AB[1] = m_Bytes[1];
                                    m_Bytes_CD[0] = m_Bytes[2];
                                    m_Bytes_CD[1] = m_Bytes[3];
                                    Data_ushort[2 * i] = BitConverter.ToUInt16(m_Bytes_CD, 0);
                                    Data_ushort[2 * i + 1] = BitConverter.ToUInt16(m_Bytes_AB, 0);
                                }
                                break;
                            case DisplayType.LongCDAB:
                                Data_ushort = new ushort[Data_New.Length * 2];
                                for (int i = 0; i < Data_New.Length; i++)
                                {
                                    Int32 m_Data = Convert.ToInt32(Data_New[i]);
                                    byte[] m_Bytes = BitConverter.GetBytes(m_Data);
                                    byte[] m_Bytes_AB = new Byte[2];
                                    byte[] m_Bytes_CD = new Byte[2];
                                    m_Bytes_AB[0] = m_Bytes[0];
                                    m_Bytes_AB[1] = m_Bytes[1];
                                    m_Bytes_CD[0] = m_Bytes[2];
                                    m_Bytes_CD[1] = m_Bytes[3];
                                    Data_ushort[2 * i] = BitConverter.ToUInt16(m_Bytes_AB, 0);
                                    Data_ushort[2 * i + 1] = BitConverter.ToUInt16(m_Bytes_CD, 0);
                                }
                                break;
                            case DisplayType.LongBADC:
                                Data_ushort = new ushort[Data_New.Length * 2];
                                for (int i = 0; i < Data_New.Length; i++)
                                {
                                    Int32 m_Data = Convert.ToInt32(Data_New[i]);
                                    byte[] m_Bytes = BitConverter.GetBytes(m_Data);
                                    byte[] m_Bytes_BA = new Byte[2];
                                    byte[] m_Bytes_DC = new Byte[2];
                                    m_Bytes_BA[0] = m_Bytes[1];
                                    m_Bytes_BA[1] = m_Bytes[0];
                                    m_Bytes_DC[0] = m_Bytes[3];
                                    m_Bytes_DC[1] = m_Bytes[2];
                                    Data_ushort[2 * i] = BitConverter.ToUInt16(m_Bytes_DC, 0);
                                    Data_ushort[2 * i + 1] = BitConverter.ToUInt16(m_Bytes_BA, 0);
                                }
                                break;
                            case DisplayType.LongDCBA:
                                Data_ushort = new ushort[Data_New.Length * 2];
                                for (int i = 0; i < Data_New.Length; i++)
                                {
                                    Int32 m_Data = Convert.ToInt32(Data_New[i]);
                                    byte[] m_Bytes = BitConverter.GetBytes(m_Data);
                                    byte[] m_Bytes_BA = new Byte[2];
                                    byte[] m_Bytes_DC = new Byte[2];
                                    m_Bytes_BA[0] = m_Bytes[1];
                                    m_Bytes_BA[1] = m_Bytes[0];
                                    m_Bytes_DC[0] = m_Bytes[3];
                                    m_Bytes_DC[1] = m_Bytes[2];
                                    Data_ushort[2 * i] = BitConverter.ToUInt16(m_Bytes_BA, 0);
                                    Data_ushort[2 * i + 1] = BitConverter.ToUInt16(m_Bytes_DC, 0);
                                }
                                break;
                            case DisplayType.FloatABCD:
                                Data_ushort = new ushort[Data_New.Length * 2];
                                for (int i = 0; i < Data_New.Length; i++)
                                {
                                    float m_Data = Convert.ToSingle(Data_New[i]);
                                    byte[] m_Bytes = BitConverter.GetBytes(m_Data);
                                    byte[] m_Bytes_AB = new Byte[2];
                                    byte[] m_Bytes_CD = new Byte[2];
                                    m_Bytes_AB[0] = m_Bytes[0];
                                    m_Bytes_AB[1] = m_Bytes[1];
                                    m_Bytes_CD[0] = m_Bytes[2];
                                    m_Bytes_CD[1] = m_Bytes[3];
                                    Data_ushort[2 * i] = BitConverter.ToUInt16(m_Bytes_CD, 0);
                                    Data_ushort[2 * i + 1] = BitConverter.ToUInt16(m_Bytes_AB, 0);
                                }
                                break;
                            case DisplayType.FloatCDAB:
                                Data_ushort = new ushort[Data_New.Length * 2];
                                for (int i = 0; i < Data_New.Length; i++)
                                {
                                    float m_Data = Convert.ToSingle(Data_New[i]);
                                    byte[] m_Bytes = BitConverter.GetBytes(m_Data);
                                    byte[] m_Bytes_AB = new Byte[2];
                                    byte[] m_Bytes_CD = new Byte[2];
                                    m_Bytes_AB[0] = m_Bytes[0];
                                    m_Bytes_AB[1] = m_Bytes[1];
                                    m_Bytes_CD[0] = m_Bytes[2];
                                    m_Bytes_CD[1] = m_Bytes[3];
                                    Data_ushort[2 * i] = BitConverter.ToUInt16(m_Bytes_AB, 0);
                                    Data_ushort[2 * i + 1] = BitConverter.ToUInt16(m_Bytes_CD, 0);
                                }
                                break;
                            case DisplayType.FloatBADC:
                                Data_ushort = new ushort[Data_New.Length * 2];
                                for (int i = 0; i < Data_New.Length; i++)
                                {
                                    float m_Data = Convert.ToSingle(Data_New[i]);
                                    byte[] m_Bytes = BitConverter.GetBytes(m_Data);
                                    byte[] m_Bytes_BA = new Byte[2];
                                    byte[] m_Bytes_DC = new Byte[2];
                                    m_Bytes_BA[0] = m_Bytes[1];
                                    m_Bytes_BA[1] = m_Bytes[0];
                                    m_Bytes_DC[0] = m_Bytes[3];
                                    m_Bytes_DC[1] = m_Bytes[2];
                                    Data_ushort[2 * i] = BitConverter.ToUInt16(m_Bytes_DC, 0);
                                    Data_ushort[2 * i + 1] = BitConverter.ToUInt16(m_Bytes_BA, 0);
                                }
                                break;
                            case DisplayType.FloatDCBA:
                                Data_ushort = new ushort[Data_New.Length * 2];
                                for (int i = 0; i < Data_New.Length; i++)
                                {
                                    float m_Data = Convert.ToSingle(Data_New[i]);
                                    byte[] m_Bytes = BitConverter.GetBytes(m_Data);
                                    byte[] m_Bytes_BA = new Byte[2];
                                    byte[] m_Bytes_DC = new Byte[2];
                                    m_Bytes_BA[0] = m_Bytes[1];
                                    m_Bytes_BA[1] = m_Bytes[0];
                                    m_Bytes_DC[0] = m_Bytes[3];
                                    m_Bytes_DC[1] = m_Bytes[2];
                                    Data_ushort[2 * i] = BitConverter.ToUInt16(m_Bytes_BA, 0);
                                    Data_ushort[2 * i + 1] = BitConverter.ToUInt16(m_Bytes_DC, 0);
                                }
                                break;
                            default:
                                break;
                        }
                        for (int i = 0; i < Data_ushort.Length; i++)
                        {
                            m_Slave.DataStore.HoldingRegisters[StartAddress + 1 + i] = Data_ushort[i];
                        }
                        break;
                    default:
                        break;
                }
            }
            catch (Exception)
            {
                return false;
            }
            bool_SetResult = true;
            return bool_SetResult;
        }

        //客户端的响应请求
        void m_Slave_ModbusSlaveRequestReceived(object sender, Modbus.Device.ModbusSlaveRequestEventArgs e)
        {
            //Disaddemble packet from master
            byte fc = e.Message.FunctionCode;
            byte[] data = e.Message.MessageFrame;
            byte[] byteStartAddress = new byte[] { data[3], data[2] };
            byte[] byteNum = new byte[] { data[5], data[4] };

            ModbusTCPRequestEventArgs mm = new ModbusTCPRequestEventArgs(fc, byteStartAddress, byteNum);
            handle_ModbusTCPRequestReceived(sender, mm);
        }

        //客户端的写请求，对服务器数据区的修改
        void DataStore_DataStoreWrittenTo(object sender, Modbus.Data.DataStoreEventArgs e)
        {
            ModbusTCPDataEventArgs mm = new ModbusTCPDataEventArgs();
            mm.Data = e.Data;
            mm.StartAddress = e.StartAddress;
            switch (e.ModbusDataType)
            {
                case Modbus.Data.ModbusDataType.Coil:
                    mm.ModbusTCPDataType = RegisterType.CoilDiscrete;
                    break;
                case Modbus.Data.ModbusDataType.HoldingRegister:
                    mm.ModbusTCPDataType = RegisterType.HoldRegister;
                    break;
                case Modbus.Data.ModbusDataType.Input:
                    mm.ModbusTCPDataType = RegisterType.InputDiscrete;
                    break;
                case Modbus.Data.ModbusDataType.InputRegister:
                    mm.ModbusTCPDataType = RegisterType.InputRegister;
                    break;
            }
            handle_ModbusTCPDataWrittenTo(sender, mm);
        }
        #endregion

        #region Event
        public event EventHandler<ModbusTCPRequestEventArgs> ModbusTCPRequestReceived;
        void handle_ModbusTCPRequestReceived(object sender, ModbusTCPRequestEventArgs e)
        {
            if (ModbusTCPRequestReceived != null)
            {
                ModbusTCPRequestReceived(sender, e);
            }
        }
        //public event EventHandler<ModbusTCPRequestEventArgs> ModbusTCPWriteComplete;
        //void handle_ModbusTCPWriteComplete(object sender, ModbusTCPRequestEventArgs e)
        //{
        //    if (ModbusTCPWriteComplete != null)
        //    {
        //        ModbusTCPWriteComplete(sender, e);
        //    }
        //}

        public event EventHandler<ModbusTCPDataEventArgs> ModbusTCPDataWrittenTo;
        void handle_ModbusTCPDataWrittenTo(object sender, ModbusTCPDataEventArgs e)
        {
            if (ModbusTCPDataWrittenTo != null)
            {
                ModbusTCPDataWrittenTo(sender, e);
            }
        }

        /// <summary>
        /// 与客户端的连接已建立事件
        /// </summary>
        public event EventHandler<TCPEventArgs> ClientConnected;
        private void RaiseClientConnected(TCPEventArgs e)
        {
            if (ClientConnected != null)
            {
                ClientConnected(this, e);
            }
        }

        /// <summary>
        /// 与客户端的连接已断开事件
        /// </summary>
        public event EventHandler<TCPEventArgs> ClientDisconnected;
        private void RaiseClientDisconnected(TCPEventArgs e)
        {
            if (ClientDisconnected != null)
            {
                ClientDisconnected(this, e);
            }
        }
        #endregion
    }

    public enum RegisterType
    {
        CoilDiscrete = 1,
        InputDiscrete = 10001,
        InputRegister = 30001,
        HoldRegister = 40001,
    }

    public enum DisplayType
    {
        [Description("Unsigned")]
        Unsigned = 0,
        [Description("Signed")]
        Signed = 1,
        [Description("Hex")]
        Hex = 2,
        [Description("Binary")]
        Binary = 3,
        [Description("Long AB CD")]
        LongABCD = 11,
        [Description("Long CD AB")]
        LongCDAB = 12,
        [Description("Long BA DC")]
        LongBADC = 13,
        [Description("Long DC BA")]
        LongDCBA = 14,
        [Description("Float AB CD")]
        FloatABCD = 21,
        [Description("Float CD AB")]
        FloatCDAB = 22,
        [Description("Float BA DC")]
        FloatBADC = 23,
        [Description("Float DC BA")]
        FloatDCBA = 24,
    }

    /// <summary>
    /// 获取枚举类型的描述信息
    /// </summary>
    public static class GetDescription
    {
        /// <summary>
        /// 获取描述信息
        /// </summary>
        /// <param name="en"></param>
        /// <returns></returns>
        public static string description(this Enum en)
        {
            Type type = en.GetType();
            MemberInfo[] memInfo = type.GetMember(en.ToString());
            if (memInfo != null && memInfo.Length > 0)
            {
                object[] attrs = memInfo[0].GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), false);
                if (attrs != null && attrs.Length > 0)
                    return ((DescriptionAttribute)attrs[0]).Description;
            }
            return en.ToString();
        }
    }
}

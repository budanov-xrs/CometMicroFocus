using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using BR.AN.PviServices;

namespace CometMicroFocus;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private const string ServiceName = "service";
    private const string IpAddress = "192.168.12.10";
    private const short Port = 11159;

    private const short SourcePort = 26575;
    private const byte SourceStation = 1;

    private Service _service;
    private Cpu _cpu;
    private Variable _variable;

    private void btnConnectPLC_Click(object sender, EventArgs e)
    {
        if (_service == null)
        {
            _service = new Service(ServiceName);
            _service.Connected += service_Connected;
            _service.Error += service_Error;
        }

        _service.Connect();           
    }

    /// <summary> 
    /// Connect CPU object if service object connection successful 
    ///</summary> 
    private void service_Connected(object sender, PviEventArgs e)
    {
        grpVariables.IsEnabled = true;
        if (_cpu == null)
        {
            // Create CPU object and add the event handler 
            _cpu = new Cpu(_service, "cpu");
            _cpu.Connected += cpu_Connected;
            _cpu.DateTimeRead += cpu_DateTimeRead;
            // Set the connection properties for a TCP/IP connection 
            _cpu.Connection.DeviceType = DeviceType.TcpIp;
            _cpu.Connection.TcpIp.DestinationIpAddress = IpAddress;
            _cpu.Connection.TcpIp.DestinationPort = Port;
            //if the Destination Station is not specified, it looks like the system automatically determines it.
            //cpu.Connection.TcpIp.DestinationStation = Properties.Settings.Default.destination_station;
            _cpu.Connection.TcpIp.SourcePort = SourcePort;
            _cpu.Connection.TcpIp.SourceStation = SourceStation;
        }

        // Connect CPU
        _cpu.Connect();
    }

    /// <summary> 
    /// Handles service connection errors 
    ///</summary>
    private void service_Error(object sender, PviEventArgs e)
    {
        int errorCode = _service.ErrorCode;
        if (errorCode != 0)
        {
            MessageBox.Show(_service.GetErrorText(_service.ErrorCode));
        }
    }

    /// <summary> 
    /// Output text when connection to CPU successful and   
    /// enable Variable Connect button. Additionnaly reads the PLC date and time.
    /// </summary> 
    private void cpu_Connected(object sender, PviEventArgs e)
    {
        panelVariableRead.IsEnabled = true;
        _cpu.ReadDateTime();
    }

    /// <summary>
    /// Output the PLC date and time in the log
    /// </summary>
    private void cpu_DateTimeRead(object sender, CpuEventArgs e)
    {
    }


    /// <summary>
    /// Create and connect variable object
    /// </summary>
    private void btnConnectVariable_Click(object sender, EventArgs e)
    {
        ConnectVariable();
    }

    private void ConnectVariable()
    {
        // Create new (global) variable object -> global
        // variable "count" must be on the controller and should
        // cyclically count up
        _variable = new Variable(_cpu, tbVarNameRead.Text);
        // Activate and connect variable object
        _variable.Active = true;
        _variable.Connect();
        // Add event handler for value changes
        _variable.ValueChanged += variable_ValueChanged;
        _variable.ValueWritten += variable_ValueWritten;
        panelVariableRead.IsEnabled = false;
        panelVariableWrite.IsEnabled = true;
        btnDisconnectVariable.Focus();
    }

    /// <summary> 
    /// Output value changes in status field
    /// </summary> 
    private void variable_ValueChanged(object sender, VariableEventArgs e)
    {
        Variable tmpVariable = (Variable)sender;
        tbVarValue.Text = tmpVariable.Value.ToString(new CultureInfo("RU-ru"));
    }

    private void variable_Disconnected(object sender, PviEventArgs e)
    {
        _variable.Dispose();
        panelVariableRead.IsEnabled = true;
        tbVarValue.Text = "";
        panelVariableWrite.IsEnabled = false;
        tbVarNameRead.Focus();
    }

    private void variable_ValueWritten(object sender, PviEventArgs e)
    {
        tbVarNameRead.Focus();
    }

    private void btnDisconnectVariable_Click(object sender, EventArgs e)
    {
        _variable.Active = false;
        // Add event handler for value changes
        _variable.Disconnected += variable_Disconnected;
        _variable.Disconnect();
    }

    private void tbVarNameRead_KeyPress(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter) return;

        ConnectVariable();
        e.Handled = true;
    }

    private void btnVariableWrite_Click(object sender, EventArgs e)
    {
        VariableWrite();
    }

    private void VariableWrite()
    {
        _variable.Value = tbVarValue.Text;
        _variable.WriteValue();
    }

    private void tbVarValue_KeyPress(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter) return;

        VariableWrite();
        e.Handled = true;
    }
}
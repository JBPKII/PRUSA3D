using System;
using System.Windows.Forms;

namespace SLT_Printer
{
    public partial class FrmConexion : Form
    {
        public System.IO.Ports.SerialPort SerialPortConex;

        public FrmConexion(ref System.IO.Ports.SerialPort serialPort)
        {
            SerialPortConex = serialPort;

            InitializeComponent();
        }

        private void FrmConexion_Load(object sender, EventArgs e)
        {
            //carga los distintos puertos COM
            this.CBPuertos.Items.AddRange(System.IO.Ports.SerialPort.GetPortNames());

            if (this.CBPuertos.Items.Count > 0)
            {
                if (this.CBPuertos.Items.Contains(SerialPortConex.PortName))
                {
                    this.CBPuertos.SelectedItem = SerialPortConex.PortName;
                }
                else
                {
                    this.CBPuertos.SelectedIndex = 0;
                }
            }

            this.CBBaudios.SelectedItem = SerialPortConex.BaudRate.ToString();
        }

        private void CmBTest_Click(object sender, EventArgs e)
        {
            try
            {
                SerialPortConex.Close();
            }
            catch (System.Exception sysEx)
            {
                sysEx.Data.Clear();
            }
            finally
            {
                SerialPortConex.PortName = CBPuertos.Text;
                SerialPortConex.BaudRate = Convert.ToInt32(CBBaudios.Text);
                try
                {
                    SerialPortConex.Open();
                }
                catch (System.Exception sysEx1)
                {
                    sysEx1.Data.Clear();
                }
                finally
                {
                    if (SerialPortConex.IsOpen)
                    {
                        CmBAplicar.Enabled = true;

                        if (System.Windows.Forms.MessageBox.Show("Conexión correcta.\n¿Quiere aplicar la configuración?",
                            "Conexión:",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
                        {
                            CmBAplicar_Click(sender, e);
                        }
                    }
                    else
                    {
                        CmBAplicar.Enabled = false;
                        System.Windows.Forms.MessageBox.Show("Conexión fallida.", "Conexión:", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    }
                }
            }
        }

        private void CmBAplicar_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void CBPuertos_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}

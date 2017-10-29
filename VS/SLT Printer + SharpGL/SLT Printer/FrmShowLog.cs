using System;
using System.Windows.Forms;

namespace SLT_Printer
{
    public partial class FrmShowLog : Form
    {
        public FrmShowLog(string tittle, string initialText = "")
        {
            InitializeComponent();

            this.Text = tittle;
            this.TxtLog.Text = initialText;
        }

        private void CmBClearLog_Click(object sender, EventArgs e)
        {
            TxtLog.Text = "";
        }

        private void ToClipBoard_Click(object sender, EventArgs e)
        {
            if (TxtLog.Text != null && TxtLog.Text != "")
            {
                Clipboard.Clear();
                Clipboard.SetText(TxtLog.Text);
            }
        }

        public void AddLogLine(string logLine)
        {
            if (TxtLog != null && !TxtLog.IsDisposed)
            {
                TxtLog.AppendText(logLine.EndsWith(Environment.NewLine) ? logLine : (logLine + Environment.NewLine));
            }
        }

        delegate void SetLogCallback(string Warn);
        public void OnLog(string Log)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.TxtLog.InvokeRequired)
            {
                SetLogCallback d = new SetLogCallback(OnLog);
                this.Invoke(d, new object[] { Log });
            }
            else
            {
                this.AddLogLine("Log: " + Log);
            }
        }
    }
}

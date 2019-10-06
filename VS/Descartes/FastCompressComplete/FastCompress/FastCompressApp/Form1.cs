using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using BK.Util;

namespace FastCompressApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            richTextBox2.ZoomFactor = 1.2f;
            int noOfProcessor = Environment.ProcessorCount;
            processorDetail = "Number of Processor in this Machine = " + noOfProcessor.ToString() + "\n\n";

            if (noOfProcessor <= 1)
            {
                isMultiProcessor = false;
                richTextBox1.ForeColor = Color.Green;
                richTextBox1.Text = processorDetail + "Yours is NOT a Multi Core System.  You may not see any performance difference by selecting Multi Core for Performance.";
            }
            else
            {
                isMultiProcessor = true;
                richTextBox1.ForeColor = Color.Green;
                richTextBox1.Text = processorDetail + "Yours is a Multi Core System.  You may choose one of the choices presented at the left to see the visible performance improvement.";
            }

            radioButton3.Checked = true;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            richTextBox2.ForeColor = Color.DarkBlue;
            richTextBox2.BackColor = Color.Tan;

            richTextBox2.ZoomFactor = 1.2f;
            richTextBox2.Text = " Fast compression demo Application for Code Project \n\n -- Author: Bharath K A";
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked == true && isMultiProcessor == false)
            {
                richTextBox1.ForeColor = Color.IndianRed;
                richTextBox1.Text = processorDetail+ "This is a Single processor system.  Choosing this option will NOT improve the performance";
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileDlg = new OpenFileDialog();

            if (DialogResult.OK == fileDlg.ShowDialog(this))
            {
                textBox1.Text = fileDlg.FileName;
                textBox2.Text = fileDlg.FileName + ".cmprss";
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            richTextBox2.ForeColor = Color.Black;
            richTextBox2.BackColor = Color.White;
            richTextBox2.Text = "Processing compression request.  Please wait...";
            richTextBox2.Update();

            if (radioButton1.Checked == true)
            {
                FastCompress.doNotUseTPL = false;
                richTextBox1.ForeColor = Color.DarkBlue;
                richTextBox1.BackColor = Color.White;
                richTextBox1.Text = processorDetail + "User selected Multi Core.  Selected Multi Core based Fast Compression";
            }
            else if (radioButton2.Checked == true)
            {
                FastCompress.doNotUseTPL = true;
                richTextBox1.ForeColor = Color.DarkBlue;
                richTextBox1.BackColor = Color.White;
                richTextBox1.Text = processorDetail + "User selected not to use TPL based Compression";
            }
            else if (radioButton3.Checked == true)
            {
                if (isMultiProcessor)
                {
                    FastCompress.doNotUseTPL = false;
                    richTextBox1.ForeColor = Color.DarkBlue;
                    richTextBox1.BackColor = Color.White;
                    richTextBox1.Text = processorDetail + "Auto detected Multi Core.  Selected Multi Core based Fast Compression";  
                }
                else
                {
                    FastCompress.doNotUseTPL = true;
                    richTextBox1.ForeColor = Color.DarkBlue;
                    richTextBox1.BackColor = Color.White;
                    richTextBox1.Text = processorDetail + "Auto detected Single Core.  Selected Single Core based Compression";
                }
            }

            if (string.IsNullOrWhiteSpace(textBox1.Text) || string.IsNullOrWhiteSpace(textBox2.Text))
            {
                richTextBox2.ForeColor = Color.IndianRed;
                richTextBox2.BackColor = Color.WhiteSmoke;
                richTextBox2.Text = "Please select the File Name to Compress.  Click Browse button above to select a file";
                return;

            }

            try
            {
                int timeTaken = FastCompress.CompressFast(textBox2.Text, textBox1.Text, true);

                richTextBox2.ForeColor = Color.DarkGreen;
                richTextBox2.BackColor = Color.White;
                richTextBox2.Text = " File Compression Succesful. \n\n Time taken = " + timeTaken.ToString() + " milli seconds";
            }
            catch (FastCompressException ex)
            {
                richTextBox2.ForeColor = Color.IndianRed;
                richTextBox2.BackColor = Color.White;
                richTextBox2.Text = " Exception: \n " + ex.Message;
            }

        }

        private void button5_Click(object sender, EventArgs e)
        {
            richTextBox2.ForeColor = Color.Black;
            richTextBox2.BackColor = Color.White;
            richTextBox2.Text = "Processing De-compression request.  Please wait...";
            richTextBox2.Update();

            if (radioButton1.Checked == true)
            {
                FastCompress.doNotUseTPL = false;
                richTextBox1.ForeColor = Color.DarkBlue;
                richTextBox1.BackColor = Color.White;
                richTextBox1.Text = processorDetail + "User selected Multi Core.  Selected Multi Core based Fast Compression";
            }
            else if (radioButton2.Checked == true)
            {
                FastCompress.doNotUseTPL = true;
                richTextBox1.ForeColor = Color.DarkBlue;
                richTextBox1.BackColor = Color.White;
                richTextBox1.Text = processorDetail + "User selected using non-TPL based Compression ";
            }
            else if (radioButton3.Checked == true)
            {
                if (isMultiProcessor)
                {
                    FastCompress.doNotUseTPL = false;
                    richTextBox1.ForeColor = Color.DarkBlue;
                    richTextBox1.BackColor = Color.White;
                    richTextBox1.Text = processorDetail + "Auto detected Multi Core.  Selected Multi Core based Fast Compression";
                }
                else
                {
                    FastCompress.doNotUseTPL = true;
                    richTextBox1.ForeColor = Color.DarkBlue;
                    richTextBox1.BackColor = Color.White;
                    richTextBox1.Text = processorDetail + "Auto detected Single Core.  Selected Single Core based Compression";
                }
            }

            if (string.IsNullOrWhiteSpace(textBox1.Text) || string.IsNullOrWhiteSpace(textBox2.Text))
            {
                richTextBox2.ForeColor = Color.IndianRed;
                richTextBox2.BackColor = Color.WhiteSmoke;
                richTextBox2.Text = "Please select the File Name to Compress.  Click Browse button above to select a file";
                return;

            }

            try
            {
                int timeTaken = FastCompress.UncompressFast(textBox2.Text, textBox1.Text, true);

                richTextBox2.ForeColor = Color.DarkGreen;
                richTextBox2.BackColor = Color.White;
                richTextBox2.Text = " File De-Compression Succesful. \n\n Time taken = " + timeTaken.ToString() + " milli seconds";
            }
            catch (FastCompressException ex)
            {
                richTextBox2.ForeColor = Color.IndianRed;
                richTextBox2.BackColor = Color.White;
                richTextBox2.Text = " Exception: \n " + ex.Message;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;
using System.IO;

namespace FFMpeg_mp3
{
    public partial class Form1 : Form
    {
        Thread mp3t0;
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //讀取檔案
            OpenFileDialog openfile = new OpenFileDialog();
            openfile.Multiselect = true;
            openfile.Filter = "*|*";
            if (openfile.ShowDialog() == DialogResult.OK)
            {
                foreach (string file in openfile.FileNames)
                {
                    listBox1.AllowDrop = false;
                    listBox1.Items.Add(file);
                    listBox1.AllowDrop = true;
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            mp3t0 = new Thread(() =>
            {
                progressBar1.Value = 0;
                double count = 0;
                foreach (string list in listBox1.Items)
                {
                    textBox2.Text = list;
                    textBox1.Text = (count + 1) + " / " + listBox1.Items.Count;
                    Directory.SetCurrentDirectory(Path.GetDirectoryName(list));
                    FileInfo batchItemAttribute = new FileInfo(list);
                    batchItemAttribute.Attributes = FileAttributes.Normal;
                    var run = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase + @".\bin\ffmpeg\", "ffmpeg.exe"),
                            Arguments = "-i \"" + list + "\" -codec:a libmp3lame -threads 4 -q:a 6 -y buff.mp3",
                            UseShellExecute = false,
                            CreateNoWindow = true,
                            RedirectStandardError = true
                        }
                    };
                    run.Start();
                    StreamReader sr = run.StandardError;
                    while (!sr.EndOfStream)
                    {
                        string srstring = sr.ReadLine();
                        if (sr.EndOfStream)
                        {
                            File.Copy("buff.mp3", Path.GetFileNameWithoutExtension(list) + ".mp3", true);
                            File.Delete("buff.mp3");
                        }
                    }
                    count++;
                    double prodata = Math.Round(count / listBox1.Items.Count * 100,2);
                    progressBar1.Value = (int)prodata;
                    label4.Text = prodata.ToString() + " %";
                }   
            });
            mp3t0.Start();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Form.CheckForIllegalCrossThreadCalls = false;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            while (true)
            {
                if (mp3t0 != null && mp3t0.IsAlive) mp3t0.Abort();
                if (!mp3t0.IsAlive)
                {
                    foreach (Process p in Process.GetProcessesByName("ffmpeg")) p.Kill();
                    File.Delete("buff.mp3");
                    MessageBox.Show("以強制停止", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
                }
            }
        }

    }
}

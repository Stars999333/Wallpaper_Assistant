using System;
using System.Drawing;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading;
using Wallpaper_Assistant.Properties;
using System.Collections.Generic;

namespace Wallpaper_Assistant
{
    public class TextScroller : Form
    {

        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
        private Label label1, label2, label3;
        private PictureBox pictureBox1;
        private System.Windows.Forms.Timer timer;
        private string[] texts;
        private int currentIndex;
        private int txtinterval = 2000;

        private bool fatal = false;
        private int tryCount = 10;
        private int timeout = 10000;
        private int interval = 2000;

        public TextScroller()
        {
            //string[] textonline = { "1", "2", "3", "4", "5" };
            string[] textonline = null;
            FetchWebPageTextsFromConfig(textonline);
            //this.texts = textonline;
            currentIndex = 0;

            // 设置窗口样式
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(1000, 10);
            this.Size = new Size(250, 350); // 根据需要调整大小
            this.TransparencyKey = Color.Red;
            this.BackColor = Color.Red;

            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));

            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new Point(15, 5);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new Size(190, 20);  // 设置目标大小
            this.pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;  // 设置为拉伸模式
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            this.Controls.Add(this.pictureBox1);


            // 创建标签用于显示文本
            label1 = new Label();
            label1.AutoSize = false;
            label1.ForeColor = Color.White;
            label1.Font = new Font(label1.Font.FontFamily, 10, FontStyle.Bold);
            label1.Location = new Point(10, 35);
            label1.Size = new Size(230, 90);
            label1.BorderStyle = BorderStyle.FixedSingle; // 添加白色边框
            label1.Padding = new Padding(5);
            this.Controls.Add(label1);

            label2 = new Label();
            label2.AutoSize = false;
            label2.ForeColor = Color.White;
            label2.Font = new Font(label2.Font.FontFamily, 10, FontStyle.Bold);
            label2.Location = new Point(10, 140);
            label2.Size = new Size(230, 90);
            label2.BorderStyle = BorderStyle.FixedSingle; // 添加白色边框
            label2.Padding = new Padding(5);
            this.Controls.Add(label2);

            label3 = new Label();
            label3.AutoSize = false;
            label3.ForeColor = Color.White;
            label3.Font = new Font(label3.Font.FontFamily, 10, FontStyle.Bold);
            label3.Location = new Point(10, 255);
            label3.Size = new Size(230, 90);
            label3.BorderStyle = BorderStyle.FixedSingle; // 添加白色边框
            label3.Padding = new Padding(5);
            this.Controls.Add(label3);

            // 创建关闭按钮
            Button closeButton = new Button();
            closeButton.Text = "X";
            closeButton.ForeColor = Color.White;
            closeButton.BackColor = Color.Red;
            closeButton.FlatStyle = FlatStyle.Flat;
            closeButton.Location = new Point(220, 5);
            closeButton.Size = new Size(20, 20);
            closeButton.Click += CloseButton_Click;
            this.Controls.Add(closeButton);

            // 创建定时器用于轮播文本
            timer = new System.Windows.Forms.Timer();
            timer.Interval = txtinterval;
            timer.Tick += Timer_Tick;

            timer.Start();
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            label1.Text = texts[currentIndex];
            label2.Text = texts[(currentIndex + 1) % texts.Length];
            label3.Text = texts[(currentIndex + 2) % texts.Length];
            currentIndex = (currentIndex + 1) % texts.Length;
        }
        private async void FetchWebPageTextsFromConfig(string[] textonline)
        {
            // 读取配置文件并获取网页文本
            string configPath = "C:\\textscroller_assistant_config.txt";
            StreamReader sr = null;
            try
            {
                sr = new StreamReader(configPath);
            }
            catch (Exception)
            {
                fatal = true;
                MessageBox.Show("读取配置文件失败\n请检查C:\\textscroller_assistant_config.txt", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Application.Exit();
                return;
            }

            List<string> texts = new List<string>();
            int success = 0;
            while (true)
            {
                string url = sr.ReadLine();
                if (url == null)
                    break;
                if (Regex.Match(url, "^\\s*$").Success)
                    continue;
                success = 1;
                for (int c = 0; c < tryCount; ++c)
                {
                    if (c > 0)
                    {
                        // 这里可以添加进度报告或其他处理逻辑，类似于Form1中的bgWorker.ReportProgress(c);
                    }
                    try
                    {
                        string text = await GetWebPageText(url);
                        if (!string.IsNullOrEmpty(text))
                        {
                            texts.Add(text);
                            success = 2;
                            break;
                        }
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
                if (success == 2)
                    break;
            }
            if (success == 0)
            {
                fatal = true;
                MessageBox.Show("获取网页文本失败", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Application.Exit();
                return;
            }
            else if (success == 1)
            {
                fatal = true;
                MessageBox.Show("联络服务器失败", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Application.Exit();
                return;
            }

            textonline = texts.ToArray();
            this.texts = textonline;
        }

        private async Task<string> GetWebPageText(string url)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Timeout = timeout;
                using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
                {
                    using (Stream stream = response.GetResponseStream())
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            return await reader.ReadToEndAsync();
                        }
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}

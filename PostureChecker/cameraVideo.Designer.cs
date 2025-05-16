using System;
using System.Drawing;
using System.Drawing.Imaging;   
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
//using NetMQ;
//using NetMQ.Sockets;


namespace PostureChecker
{
    partial class cameraVideo
    {
//        private PictureBox pictureBox;
//        private SubscriberSocket subscriber;

//        public Form2()
//        {
//            pictureBox = new PictureBox
//            {
//                Dock = DockStyle.Fill,
//                SizeMode = PictureBoxSizeMode.StretchImage
//            };
//            this.Controls.Add(pictureBox);

//            var address = "tcp://localhost:5555";
//            using (var context = NetMQContext.Create())
//            {
//                subscriber = context.CreateSubscriberSocket();
//                subscriber.Connect(address);
//                subscriber.Subscribe("FRAME"); // Subscribe to all messages


//            }

//        }
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            button2 = new Button();
            menuStrip1 = new MenuStrip();
            主菜单ToolStripMenuItem = new ToolStripMenuItem();
            查询数据ToolStripMenuItem = new ToolStripMenuItem();
            menuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // button2
            // 
            button2.Location = new Point(272, 251);
            button2.Name = "button2";
            button2.Size = new Size(8, 8);
            button2.TabIndex = 1;
            button2.Text = "button2";
            button2.UseVisualStyleBackColor = true;
            // 
            // menuStrip1
            // 
            menuStrip1.ImageScalingSize = new Size(24, 24);
            menuStrip1.Items.AddRange(new ToolStripItem[] { 主菜单ToolStripMenuItem, 查询数据ToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(1083, 32);
            menuStrip1.TabIndex = 2;
            menuStrip1.Text = "menuStrip1";
            // 
            // 主菜单ToolStripMenuItem
            // 
            主菜单ToolStripMenuItem.Name = "主菜单ToolStripMenuItem";
            主菜单ToolStripMenuItem.Size = new Size(80, 28);
            主菜单ToolStripMenuItem.Text = "主菜单";
            主菜单ToolStripMenuItem.Click += 主菜单ToolStripMenuItem_Click;
            // 
            // 查询数据ToolStripMenuItem
            // 
            查询数据ToolStripMenuItem.Name = "查询数据ToolStripMenuItem";
            查询数据ToolStripMenuItem.Size = new Size(98, 28);
            查询数据ToolStripMenuItem.Text = "查询数据";
            查询数据ToolStripMenuItem.Click += 查询数据ToolStripMenuItem_Click;
            // 
            // cameraVideo
            // 
            AutoScaleDimensions = new SizeF(11F, 24F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.ButtonHighlight;
            ClientSize = new Size(1083, 633);
            Controls.Add(button2);
            Controls.Add(menuStrip1);
            ForeColor = Color.Black;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MainMenuStrip = menuStrip1;
            Name = "cameraVideo";
            Text = "Form2";
            Load += Form2_Load;
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Button button2;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem 主菜单ToolStripMenuItem;
        private ToolStripMenuItem 查询数据ToolStripMenuItem;
    }
}
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
    partial class Form2
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
            label1 = new Label();
            button1 = new Button();
            button3 = new Button();
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
            // label1
            // 
            label1.BackColor = SystemColors.ButtonFace;
            label1.BorderStyle = BorderStyle.Fixed3D;
            label1.Location = new Point(-2, -1);
            label1.Name = "label1";
            label1.Size = new Size(1086, 45);
            label1.TabIndex = 4;
            label1.Click += label1_Click;
            // 
            // button1
            // 
            button1.Location = new Point(32, 76);
            button1.Name = "button1";
            button1.Size = new Size(112, 34);
            button1.TabIndex = 5;
            button1.Text = "返回\r\n";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // button3
            // 
            button3.Location = new Point(32, 146);
            button3.Name = "button3";
            button3.Size = new Size(112, 34);
            button3.TabIndex = 6;
            button3.Text = "主菜单";
            button3.UseVisualStyleBackColor = true;
            button3.Click += button3_Click;
            // 
            // Form2
            // 
            AutoScaleDimensions = new SizeF(11F, 24F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.ButtonHighlight;
            ClientSize = new Size(1083, 633);
            Controls.Add(button3);
            Controls.Add(button1);
            Controls.Add(label1);
            Controls.Add(button2);
            ForeColor = Color.Black;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Name = "Form2";
            Text = "Form2";
            Load += Form2_Load;
            ResumeLayout(false);
        }

        #endregion
        private Button button2;
        private Label label1;
        private Button button1;
        private Button button3;
    }
}
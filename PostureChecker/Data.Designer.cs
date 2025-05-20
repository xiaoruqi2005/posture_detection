namespace PostureChecker
{
    partial class Data
    {
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
            button1 = new Button();
            button2 = new Button();
            dataGridView1 = new DataGridView();
            Column1 = new DataGridViewTextBoxColumn();
            ShoulderTiltAngle = new DataGridViewTextBoxColumn();
            Column3 = new DataGridViewTextBoxColumn();
            Column4 = new DataGridViewTextBoxColumn();
            Column5 = new DataGridViewTextBoxColumn();
            Column6 = new DataGridViewTextBoxColumn();
            Column7 = new DataGridViewTextBoxColumn();
            HeadTiltState = new DataGridViewTextBoxColumn();
            Head_Yaw_orientation = new DataGridViewTextBoxColumn();
            Head_Pitch_orientation = new DataGridViewTextBoxColumn();
            OverallPostureStatus = new DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            SuspendLayout();
            // 
            // button1
            // 
            button1.Location = new Point(1967, 115);
            button1.Name = "button1";
            button1.Size = new Size(112, 34);
            button1.TabIndex = 0;
            button1.Text = "查询";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // button2
            // 
            button2.Location = new Point(1967, 43);
            button2.Name = "button2";
            button2.Size = new Size(112, 34);
            button2.TabIndex = 1;
            button2.Text = "主菜单\r\n";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;
            // 
            // dataGridView1
            // 
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.AllowUserToOrderColumns = true;
            dataGridView1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.ColumnHeader;
            dataGridView1.BorderStyle = BorderStyle.Fixed3D;
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.Columns.AddRange(new DataGridViewColumn[] { Column1, ShoulderTiltAngle, Column3, Column4, Column5, Column6, Column7, HeadTiltState, Head_Yaw_orientation, Head_Pitch_orientation, OverallPostureStatus });
            dataGridView1.Location = new Point(23, 3);
            dataGridView1.MultiSelect = false;
            dataGridView1.Name = "dataGridView1";
            dataGridView1.RowHeadersVisible = false;
            dataGridView1.RowHeadersWidth = 62;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.Size = new Size(1938, 837);
            dataGridView1.TabIndex = 2;
            dataGridView1.CellContentClick += dataGridView1_CellContentClick;
            // 
            // Column1
            // 
            Column1.FillWeight = 62.5F;
            Column1.HeaderText = "Time";
            Column1.MinimumWidth = 10;
            Column1.Name = "Column1";
            Column1.Width = 88;
            // 
            // ShoulderTiltAngle
            // 
            ShoulderTiltAngle.HeaderText = "ShoulderTiltAngle";
            ShoulderTiltAngle.MinimumWidth = 8;
            ShoulderTiltAngle.Name = "ShoulderTiltAngle";
            ShoulderTiltAngle.Width = 201;
            // 
            // Column3
            // 
            Column3.FillWeight = 103.75F;
            Column3.HeaderText = "ShoulderState";
            Column3.MinimumWidth = 8;
            Column3.Name = "Column3";
            Column3.Width = 167;
            // 
            // Column4
            // 
            Column4.FillWeight = 103.75F;
            Column4.HeaderText = "EyeTiltAngle";
            Column4.MinimumWidth = 8;
            Column4.Name = "Column4";
            Column4.Width = 154;
            // 
            // Column5
            // 
            Column5.FillWeight = 103.75F;
            Column5.HeaderText = "EyeState";
            Column5.MinimumWidth = 8;
            Column5.Name = "Column5";
            Column5.Width = 120;
            // 
            // Column6
            // 
            Column6.FillWeight = 103.75F;
            Column6.HeaderText = "HunchbackState";
            Column6.MinimumWidth = 8;
            Column6.Name = "Column6";
            Column6.Width = 187;
            // 
            // Column7
            // 
            Column7.FillWeight = 103.75F;
            Column7.HeaderText = "headTiltAngle";
            Column7.MinimumWidth = 8;
            Column7.Name = "Column7";
            Column7.Width = 167;
            // 
            // HeadTiltState
            // 
            HeadTiltState.FillWeight = 103.75F;
            HeadTiltState.HeaderText = "HeadTiltState";
            HeadTiltState.MinimumWidth = 8;
            HeadTiltState.Name = "HeadTiltState";
            HeadTiltState.Width = 163;
            // 
            // Head_Yaw_orientation
            // 
            Head_Yaw_orientation.FillWeight = 103.75F;
            Head_Yaw_orientation.HeaderText = "Head_Yaw_orientation";
            Head_Yaw_orientation.MinimumWidth = 8;
            Head_Yaw_orientation.Name = "Head_Yaw_orientation";
            Head_Yaw_orientation.Width = 238;
            // 
            // Head_Pitch_orientation
            // 
            Head_Pitch_orientation.FillWeight = 103.75F;
            Head_Pitch_orientation.HeaderText = "Head_Pitch_orientation";
            Head_Pitch_orientation.MinimumWidth = 8;
            Head_Pitch_orientation.Name = "Head_Pitch_orientation";
            Head_Pitch_orientation.Width = 246;
            // 
            // OverallPostureStatus
            // 
            OverallPostureStatus.FillWeight = 103.75F;
            OverallPostureStatus.HeaderText = "OverallPostureStatus";
            OverallPostureStatus.MinimumWidth = 8;
            OverallPostureStatus.Name = "OverallPostureStatus";
            OverallPostureStatus.Width = 225;
            // 
            // Data
            // 
            AutoScaleDimensions = new SizeF(11F, 24F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(2089, 852);
            Controls.Add(dataGridView1);
            Controls.Add(button2);
            Controls.Add(button1);
            Name = "Data";
            Text = "Form3";
            Load += Data_Load;
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Button button1;
        private Button button2;
        private DataGridView dataGridView1;
        private DataGridViewTextBoxColumn Column1;
        private DataGridViewTextBoxColumn ShoulderTiltAngle;
        private DataGridViewTextBoxColumn Column3;
        private DataGridViewTextBoxColumn Column4;
        private DataGridViewTextBoxColumn Column5;
        private DataGridViewTextBoxColumn Column6;
        private DataGridViewTextBoxColumn Column7;
        private DataGridViewTextBoxColumn HeadTiltState;
        private DataGridViewTextBoxColumn Head_Yaw_orientation;
        private DataGridViewTextBoxColumn Head_Pitch_orientation;
        private DataGridViewTextBoxColumn OverallPostureStatus;
    }
}
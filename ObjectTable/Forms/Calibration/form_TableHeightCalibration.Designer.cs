namespace ObjectTable.Forms.Calibration
{
    partial class form_TableHeightCalibration
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
            this.p_img = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.b_reset = new System.Windows.Forms.Button();
            this.l_distancetolerance = new System.Windows.Forms.Label();
            this.b_calibrate = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.l_tabledistance = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.l_calibpoints = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.b_cancel = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.b_depth = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.label5 = new System.Windows.Forms.Label();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.tssl = new System.Windows.Forms.ToolStripStatusLabel();
            this.txt_top = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.txt_r = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.txt_bottom = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.txt_left = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // p_img
            // 
            this.p_img.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.p_img.Location = new System.Drawing.Point(12, 33);
            this.p_img.Name = "p_img";
            this.p_img.Size = new System.Drawing.Size(640, 480);
            this.p_img.TabIndex = 0;
            this.p_img.MouseClick += new System.Windows.Forms.MouseEventHandler(this.p_img_MouseClick);
            this.p_img.MouseMove += new System.Windows.Forms.MouseEventHandler(this.p_img_MouseMove);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(280, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Click several times (5 is a good Idea) on the Table surface";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.b_reset);
            this.groupBox1.Controls.Add(this.l_distancetolerance);
            this.groupBox1.Controls.Add(this.b_calibrate);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.l_tabledistance);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.l_calibpoints);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Location = new System.Drawing.Point(658, 33);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(234, 108);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Calibration Values";
            // 
            // b_reset
            // 
            this.b_reset.Location = new System.Drawing.Point(6, 81);
            this.b_reset.Name = "b_reset";
            this.b_reset.Size = new System.Drawing.Size(71, 21);
            this.b_reset.TabIndex = 6;
            this.b_reset.Text = "Reset";
            this.b_reset.UseVisualStyleBackColor = true;
            this.b_reset.Click += new System.EventHandler(this.b_reset_Click);
            // 
            // l_distancetolerance
            // 
            this.l_distancetolerance.AutoSize = true;
            this.l_distancetolerance.Location = new System.Drawing.Point(130, 51);
            this.l_distancetolerance.Name = "l_distancetolerance";
            this.l_distancetolerance.Size = new System.Drawing.Size(13, 13);
            this.l_distancetolerance.TabIndex = 5;
            this.l_distancetolerance.Text = "0";
            // 
            // b_calibrate
            // 
            this.b_calibrate.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.b_calibrate.Location = new System.Drawing.Point(83, 81);
            this.b_calibrate.Name = "b_calibrate";
            this.b_calibrate.Size = new System.Drawing.Size(99, 21);
            this.b_calibrate.TabIndex = 3;
            this.b_calibrate.Text = "1. Calibrate!";
            this.b_calibrate.UseVisualStyleBackColor = true;
            this.b_calibrate.Click += new System.EventHandler(this.b_calibrate_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(6, 51);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(103, 13);
            this.label6.TabIndex = 4;
            this.label6.Text = "Distance Tolerance:";
            // 
            // l_tabledistance
            // 
            this.l_tabledistance.AutoSize = true;
            this.l_tabledistance.Location = new System.Drawing.Point(130, 38);
            this.l_tabledistance.Name = "l_tabledistance";
            this.l_tabledistance.Size = new System.Drawing.Size(13, 13);
            this.l_tabledistance.TabIndex = 3;
            this.l_tabledistance.Text = "0";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 38);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(82, 13);
            this.label4.TabIndex = 2;
            this.label4.Text = "Table Distance:";
            // 
            // l_calibpoints
            // 
            this.l_calibpoints.AutoSize = true;
            this.l_calibpoints.Location = new System.Drawing.Point(130, 25);
            this.l_calibpoints.Name = "l_calibpoints";
            this.l_calibpoints.Size = new System.Drawing.Size(13, 13);
            this.l_calibpoints.TabIndex = 1;
            this.l_calibpoints.Text = "0";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 25);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(118, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "N° of Calibration Points:";
            // 
            // b_cancel
            // 
            this.b_cancel.Location = new System.Drawing.Point(817, 489);
            this.b_cancel.Name = "b_cancel";
            this.b_cancel.Size = new System.Drawing.Size(75, 23);
            this.b_cancel.TabIndex = 4;
            this.b_cancel.Text = "Cancel";
            this.b_cancel.UseVisualStyleBackColor = true;
            this.b_cancel.Click += new System.EventHandler(this.b_cancel_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.b_depth);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Location = new System.Drawing.Point(658, 147);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(234, 100);
            this.groupBox2.TabIndex = 5;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "DepthCorrectionMap";
            // 
            // b_depth
            // 
            this.b_depth.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.b_depth.Location = new System.Drawing.Point(9, 51);
            this.b_depth.Name = "b_depth";
            this.b_depth.Size = new System.Drawing.Size(219, 23);
            this.b_depth.TabIndex = 1;
            this.b_depth.Text = "2. Create DepthCorrectionMap";
            this.b_depth.UseVisualStyleBackColor = true;
            this.b_depth.Click += new System.EventHandler(this.b_depth_Click);
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(6, 16);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(222, 32);
            this.label3.TabIndex = 0;
            this.label3.Text = "Once there are enough calibration values, it is useful to create a depth correcti" +
    "on map";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.label12);
            this.groupBox3.Controls.Add(this.txt_left);
            this.groupBox3.Controls.Add(this.label13);
            this.groupBox3.Controls.Add(this.label10);
            this.groupBox3.Controls.Add(this.txt_bottom);
            this.groupBox3.Controls.Add(this.label11);
            this.groupBox3.Controls.Add(this.label8);
            this.groupBox3.Controls.Add(this.txt_r);
            this.groupBox3.Controls.Add(this.label9);
            this.groupBox3.Controls.Add(this.label7);
            this.groupBox3.Controls.Add(this.txt_top);
            this.groupBox3.Controls.Add(this.label5);
            this.groupBox3.Location = new System.Drawing.Point(658, 253);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(234, 139);
            this.groupBox3.TabIndex = 6;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Border Cut-Off";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 26);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(25, 13);
            this.label5.TabIndex = 0;
            this.label5.Text = "top:";
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tssl});
            this.statusStrip1.Location = new System.Drawing.Point(0, 531);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(904, 22);
            this.statusStrip1.TabIndex = 7;
            this.statusStrip1.Text = "statusStrip";
            // 
            // tssl
            // 
            this.tssl.Name = "tssl";
            this.tssl.Size = new System.Drawing.Size(12, 17);
            this.tssl.Text = "-";
            // 
            // txt_top
            // 
            this.txt_top.Location = new System.Drawing.Point(54, 23);
            this.txt_top.Name = "txt_top";
            this.txt_top.Size = new System.Drawing.Size(51, 20);
            this.txt_top.TabIndex = 1;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(111, 26);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(18, 13);
            this.label7.TabIndex = 2;
            this.label7.Text = "px";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(111, 52);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(18, 13);
            this.label8.TabIndex = 5;
            this.label8.Text = "px";
            // 
            // txt_r
            // 
            this.txt_r.Location = new System.Drawing.Point(54, 49);
            this.txt_r.Name = "txt_r";
            this.txt_r.Size = new System.Drawing.Size(51, 20);
            this.txt_r.TabIndex = 4;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(6, 52);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(30, 13);
            this.label9.TabIndex = 3;
            this.label9.Text = "right:";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(111, 78);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(18, 13);
            this.label10.TabIndex = 8;
            this.label10.Text = "px";
            // 
            // txt_bottom
            // 
            this.txt_bottom.Location = new System.Drawing.Point(54, 75);
            this.txt_bottom.Name = "txt_bottom";
            this.txt_bottom.Size = new System.Drawing.Size(51, 20);
            this.txt_bottom.TabIndex = 7;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(6, 78);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(42, 13);
            this.label11.TabIndex = 6;
            this.label11.Text = "bottom:";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(111, 104);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(18, 13);
            this.label12.TabIndex = 11;
            this.label12.Text = "px";
            // 
            // txt_left
            // 
            this.txt_left.Location = new System.Drawing.Point(54, 101);
            this.txt_left.Name = "txt_left";
            this.txt_left.Size = new System.Drawing.Size(51, 20);
            this.txt_left.TabIndex = 10;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(6, 104);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(24, 13);
            this.label13.TabIndex = 9;
            this.label13.Text = "left:";
            // 
            // form_TableHeightCalibration
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(904, 553);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.b_cancel);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.p_img);
            this.Name = "form_TableHeightCalibration";
            this.Text = "Table Height Calibration";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.form_TableHeightCalibration_FormClosed);
            this.Load += new System.EventHandler(this.form_TableHeightCalibration_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel p_img;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button b_reset;
        private System.Windows.Forms.Label l_distancetolerance;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label l_tabledistance;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label l_calibpoints;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button b_calibrate;
        private System.Windows.Forms.Button b_cancel;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button b_depth;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel tssl;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.TextBox txt_left;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox txt_bottom;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox txt_r;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox txt_top;
    }
}
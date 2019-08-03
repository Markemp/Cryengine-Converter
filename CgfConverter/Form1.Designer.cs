namespace CgfConverter
{
    partial class Form1
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.inputButton = new System.Windows.Forms.Button();
            this.input_file_path = new System.Windows.Forms.TextBox();
            this.output_path = new System.Windows.Forms.TextBox();
            this.convert_to_dae = new System.Windows.Forms.RadioButton();
            this.convert_to_obj = new System.Windows.Forms.RadioButton();
            this.convertButton = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.outputButton = new System.Windows.Forms.Button();
            this.cgf = new System.Windows.Forms.RadioButton();
            this.cga = new System.Windows.Forms.RadioButton();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.skin = new System.Windows.Forms.RadioButton();
            this.chr = new System.Windows.Forms.RadioButton();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // inputButton
            // 
            this.inputButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.inputButton.Location = new System.Drawing.Point(274, 24);
            this.inputButton.Name = "inputButton";
            this.inputButton.Size = new System.Drawing.Size(75, 20);
            this.inputButton.TabIndex = 0;
            this.inputButton.Text = "Input path";
            this.inputButton.UseVisualStyleBackColor = true;
            this.inputButton.Click += new System.EventHandler(this.InputButton_Click);
            // 
            // input_file_path
            // 
            this.input_file_path.Location = new System.Drawing.Point(13, 24);
            this.input_file_path.Name = "input_file_path";
            this.input_file_path.Size = new System.Drawing.Size(255, 20);
            this.input_file_path.TabIndex = 1;
            // 
            // output_path
            // 
            this.output_path.Location = new System.Drawing.Point(13, 62);
            this.output_path.Name = "output_path";
            this.output_path.Size = new System.Drawing.Size(255, 20);
            this.output_path.TabIndex = 2;
            // 
            // convert_to_dae
            // 
            this.convert_to_dae.AutoSize = true;
            this.convert_to_dae.Checked = true;
            this.convert_to_dae.Location = new System.Drawing.Point(69, 148);
            this.convert_to_dae.Name = "convert_to_dae";
            this.convert_to_dae.Size = new System.Drawing.Size(90, 17);
            this.convert_to_dae.TabIndex = 4;
            this.convert_to_dae.TabStop = true;
            this.convert_to_dae.Text = "Collada (.dae)";
            this.convert_to_dae.UseVisualStyleBackColor = true;
            // 
            // convert_to_obj
            // 
            this.convert_to_obj.AutoSize = true;
            this.convert_to_obj.Location = new System.Drawing.Point(183, 148);
            this.convert_to_obj.Name = "convert_to_obj";
            this.convert_to_obj.Size = new System.Drawing.Size(101, 17);
            this.convert_to_obj.TabIndex = 5;
            this.convert_to_obj.Text = "Wavefront (.obj)";
            this.convert_to_obj.UseVisualStyleBackColor = true;
            // 
            // convertButton
            // 
            this.convertButton.Location = new System.Drawing.Point(136, 186);
            this.convertButton.Name = "convertButton";
            this.convertButton.Size = new System.Drawing.Size(75, 23);
            this.convertButton.TabIndex = 6;
            this.convertButton.Text = "Convert It!";
            this.convertButton.UseVisualStyleBackColor = true;
            this.convertButton.Click += new System.EventHandler(this.ConvertButton_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // outputButton
            // 
            this.outputButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.outputButton.Location = new System.Drawing.Point(274, 62);
            this.outputButton.Name = "outputButton";
            this.outputButton.Size = new System.Drawing.Size(75, 20);
            this.outputButton.TabIndex = 7;
            this.outputButton.Text = "Output path";
            this.outputButton.UseVisualStyleBackColor = true;
            this.outputButton.Click += new System.EventHandler(this.outputButton_Click);
            // 
            // cgf
            // 
            this.cgf.AutoSize = true;
            this.cgf.Checked = true;
            this.cgf.Location = new System.Drawing.Point(3, 3);
            this.cgf.Name = "cgf";
            this.cgf.Size = new System.Drawing.Size(43, 17);
            this.cgf.TabIndex = 8;
            this.cgf.TabStop = true;
            this.cgf.Text = ".cgf";
            this.cgf.UseVisualStyleBackColor = true;
            // 
            // cga
            // 
            this.cga.AutoSize = true;
            this.cga.Location = new System.Drawing.Point(52, 3);
            this.cga.Name = "cga";
            this.cga.Size = new System.Drawing.Size(46, 17);
            this.cga.TabIndex = 9;
            this.cga.Text = ".cga";
            this.cga.UseVisualStyleBackColor = true;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.cgf);
            this.flowLayoutPanel1.Controls.Add(this.cga);
            this.flowLayoutPanel1.Controls.Add(this.chr);
            this.flowLayoutPanel1.Controls.Add(this.skin);
            this.flowLayoutPanel1.Location = new System.Drawing.Point(69, 97);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(215, 25);
            this.flowLayoutPanel1.TabIndex = 10;
            // 
            // skin
            // 
            this.skin.AutoSize = true;
            this.skin.Location = new System.Drawing.Point(153, 3);
            this.skin.Name = "skin";
            this.skin.Size = new System.Drawing.Size(47, 17);
            this.skin.TabIndex = 10;
            this.skin.TabStop = true;
            this.skin.Text = ".skin";
            this.skin.UseVisualStyleBackColor = true;
            // 
            // chr
            // 
            this.chr.AutoSize = true;
            this.chr.Location = new System.Drawing.Point(104, 3);
            this.chr.Name = "chr";
            this.chr.Size = new System.Drawing.Size(43, 17);
            this.chr.TabIndex = 11;
            this.chr.TabStop = true;
            this.chr.Text = ".chr";
            this.chr.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.ClientSize = new System.Drawing.Size(361, 228);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Controls.Add(this.outputButton);
            this.Controls.Add(this.convertButton);
            this.Controls.Add(this.convert_to_obj);
            this.Controls.Add(this.convert_to_dae);
            this.Controls.Add(this.output_path);
            this.Controls.Add(this.input_file_path);
            this.Controls.Add(this.inputButton);
            this.DoubleBuffered = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.Text = "Cryengine-Converter";
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button inputButton;
        private System.Windows.Forms.TextBox input_file_path;
        private System.Windows.Forms.TextBox output_path;
        private System.Windows.Forms.RadioButton convert_to_dae;
        private System.Windows.Forms.RadioButton convert_to_obj;
        private System.Windows.Forms.Button convertButton;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.Button outputButton;
        private System.Windows.Forms.RadioButton cgf;
        private System.Windows.Forms.RadioButton cga;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.RadioButton chr;
        private System.Windows.Forms.RadioButton skin;
    }
}
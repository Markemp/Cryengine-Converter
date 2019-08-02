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
            this.inputButton = new System.Windows.Forms.Button();
            this.input_file_path = new System.Windows.Forms.TextBox();
            this.output_path = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.convert_to_dae = new System.Windows.Forms.RadioButton();
            this.convert_to_obj = new System.Windows.Forms.RadioButton();
            this.convertButton = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.SuspendLayout();
            // 
            // inputButton
            // 
            this.inputButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.inputButton.Location = new System.Drawing.Point(274, 24);
            this.inputButton.Name = "inputButton";
            this.inputButton.Size = new System.Drawing.Size(75, 20);
            this.inputButton.TabIndex = 0;
            this.inputButton.Text = "Input File";
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
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(274, 65);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(63, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Output path";
            // 
            // convert_to_dae
            // 
            this.convert_to_dae.AutoSize = true;
            this.convert_to_dae.Checked = true;
            this.convert_to_dae.Location = new System.Drawing.Point(69, 99);
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
            this.convert_to_obj.Location = new System.Drawing.Point(183, 99);
            this.convert_to_obj.Name = "convert_to_obj";
            this.convert_to_obj.Size = new System.Drawing.Size(101, 17);
            this.convert_to_obj.TabIndex = 5;
            this.convert_to_obj.Text = "Wavefront (.obj)";
            this.convert_to_obj.UseVisualStyleBackColor = true;
            // 
            // convertButton
            // 
            this.convertButton.Location = new System.Drawing.Point(136, 137);
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
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(361, 186);
            this.Controls.Add(this.convertButton);
            this.Controls.Add(this.convert_to_obj);
            this.Controls.Add(this.convert_to_dae);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.output_path);
            this.Controls.Add(this.input_file_path);
            this.Controls.Add(this.inputButton);
            this.Name = "Form1";
            this.Text = "Cryengine-Converter";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button inputButton;
        private System.Windows.Forms.TextBox input_file_path;
        private System.Windows.Forms.TextBox output_path;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.RadioButton convert_to_dae;
        private System.Windows.Forms.RadioButton convert_to_obj;
        private System.Windows.Forms.Button convertButton;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
    }
}
namespace FarpostParse
{
    partial class Form1
    {
        /// <summary>
        /// Требуется переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Обязательный метод для поддержки конструктора - не изменяйте
        /// содержимое данного метода при помощи редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.path = new System.Windows.Forms.TextBox();
            this.Open = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.nameCB = new System.Windows.Forms.ComboBox();
            this.Start = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.path);
            this.groupBox1.Controls.Add(this.Open);
            this.groupBox1.Location = new System.Drawing.Point(50, 21);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(363, 69);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Выбор папки для сохранения";
            // 
            // path
            // 
            this.path.Location = new System.Drawing.Point(17, 26);
            this.path.Name = "path";
            this.path.Size = new System.Drawing.Size(235, 20);
            this.path.TabIndex = 1;
            // 
            // Open
            // 
            this.Open.Location = new System.Drawing.Point(267, 24);
            this.Open.Name = "Open";
            this.Open.Size = new System.Drawing.Size(75, 23);
            this.Open.TabIndex = 0;
            this.Open.Text = "Открыть";
            this.Open.UseVisualStyleBackColor = true;
            this.Open.Click += new System.EventHandler(this.Open_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(50, 117);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(115, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Название магазина :";
            // 
            // nameCB
            // 
            this.nameCB.FormattingEnabled = true;
            this.nameCB.Location = new System.Drawing.Point(182, 114);
            this.nameCB.Name = "nameCB";
            this.nameCB.Size = new System.Drawing.Size(210, 21);
            this.nameCB.TabIndex = 2;
            this.nameCB.SelectedIndexChanged += new System.EventHandler(this.nameCB_SelectedIndexChanged);
            // 
            // Start
            // 
            this.Start.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.Start.Location = new System.Drawing.Point(172, 158);
            this.Start.Name = "Start";
            this.Start.Size = new System.Drawing.Size(142, 37);
            this.Start.TabIndex = 3;
            this.Start.Text = "Начать парсинг";
            this.Start.UseVisualStyleBackColor = true;
            this.Start.Click += new System.EventHandler(this.Start_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(50, 170);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(0, 13);
            this.label2.TabIndex = 4;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(455, 207);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.Start);
            this.Controls.Add(this.nameCB);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.groupBox1);
            this.Name = "Form1";
            this.Text = "Парсер Farpost";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox path;
        private System.Windows.Forms.Button Open;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox nameCB;
        private System.Windows.Forms.Button Start;
        private System.Windows.Forms.Label label2;
    }
}


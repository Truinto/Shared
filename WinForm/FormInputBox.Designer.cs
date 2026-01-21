namespace Shared
{
    partial class FormInputBox
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
            this.Button1 = new Button();
            this.Button2 = new Button();
            this.LabelTime = new Label();
            this.Label1 = new Label();
            this.TextBox1 = new TextBox();
            SuspendLayout();
            // 
            // Button1
            // 
            this.Button1.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            this.Button1.AutoSize = true;
            this.Button1.Location = new Point(206, 130);
            this.Button1.Name = "Button1";
            this.Button1.Size = new Size(75, 25);
            this.Button1.TabIndex = 1;
            this.Button1.Text = "OK";
            this.Button1.UseVisualStyleBackColor = true;
            this.Button1.Click += Button1_Click;
            // 
            // Button2
            // 
            this.Button2.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            this.Button2.AutoSize = true;
            this.Button2.Location = new Point(287, 130);
            this.Button2.Name = "Button2";
            this.Button2.Size = new Size(75, 25);
            this.Button2.TabIndex = 2;
            this.Button2.Text = "Cancel";
            this.Button2.UseVisualStyleBackColor = true;
            this.Button2.Click += Button2_Click;
            // 
            // LabelTime
            // 
            this.LabelTime.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            this.LabelTime.BackColor = Color.Transparent;
            this.LabelTime.Location = new Point(12, 131);
            this.LabelTime.Name = "LabelTime";
            this.LabelTime.Size = new Size(36, 23);
            this.LabelTime.TabIndex = 3;
            this.LabelTime.Text = "0:00";
            this.LabelTime.TextAlign = ContentAlignment.MiddleCenter;
            this.LabelTime.Visible = false;
            // 
            // Label1
            // 
            this.Label1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            this.Label1.AutoSize = true;
            this.Label1.Location = new Point(65, 9);
            this.Label1.MaximumSize = new Size(297, 0);
            this.Label1.MinimumSize = new Size(297, 70);
            this.Label1.Name = "Label1";
            this.Label1.Size = new Size(297, 70);
            this.Label1.TabIndex = 4;
            this.Label1.Text = "Text here";
            this.Label1.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // TextBox1
            // 
            this.TextBox1.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            this.TextBox1.BorderStyle = BorderStyle.FixedSingle;
            this.TextBox1.Location = new Point(12, 90);
            this.TextBox1.Name = "TextBox1";
            this.TextBox1.Size = new Size(350, 23);
            this.TextBox1.TabIndex = 0;
            // 
            // FormInputBox
            // 
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.BackColor = SystemColors.ControlLightLight;
            this.ClientSize = new Size(374, 166);
            this.Controls.Add(this.TextBox1);
            this.Controls.Add(this.Button2);
            this.Controls.Add(this.Label1);
            this.Controls.Add(this.LabelTime);
            this.Controls.Add(this.Button1);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormInputBox";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = FormStartPosition.CenterParent;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Button Button1;
        private Button Button2;
        private Label LabelTime;
        private Label Label1;
        private TextBox TextBox1;
    }
}

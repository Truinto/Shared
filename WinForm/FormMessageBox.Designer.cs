namespace Shared
{
    partial class FormMessageBox
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
            this.Button3 = new Button();
            this.Button4 = new Button();
            this.LabelTime = new Label();
            this.TableLayoutPanel1 = new TableLayoutPanel();
            this.Label1 = new Label();
            this.TableLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // Button1
            // 
            this.Button1.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            this.Button1.AutoSize = true;
            this.Button1.Location = new Point(246, 3);
            this.Button1.Name = "Button1";
            this.Button1.Size = new Size(75, 25);
            this.Button1.TabIndex = 3;
            this.Button1.Text = "Button1";
            this.Button1.UseVisualStyleBackColor = true;
            this.Button1.Click += Button1_Click;
            // 
            // Button2
            // 
            this.Button2.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            this.Button2.AutoSize = true;
            this.Button2.Location = new Point(165, 3);
            this.Button2.Name = "Button2";
            this.Button2.Size = new Size(75, 25);
            this.Button2.TabIndex = 2;
            this.Button2.Text = "Button2";
            this.Button2.UseVisualStyleBackColor = true;
            this.Button2.Visible = false;
            this.Button2.Click += Button2_Click;
            // 
            // Button3
            // 
            this.Button3.AutoSize = true;
            this.Button3.Location = new Point(84, 3);
            this.Button3.Name = "Button3";
            this.Button3.Size = new Size(75, 25);
            this.Button3.TabIndex = 1;
            this.Button3.Text = "Button3";
            this.Button3.UseVisualStyleBackColor = true;
            this.Button3.Visible = false;
            this.Button3.Click += Button3_Click;
            // 
            // Button4
            // 
            this.Button4.AutoSize = true;
            this.Button4.Location = new Point(3, 3);
            this.Button4.Name = "Button4";
            this.Button4.Size = new Size(75, 25);
            this.Button4.TabIndex = 0;
            this.Button4.Text = "Button4";
            this.Button4.UseVisualStyleBackColor = true;
            this.Button4.Visible = false;
            this.Button4.Click += Button4_Click;
            // 
            // LabelTime
            // 
            this.LabelTime.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            this.LabelTime.BackColor = Color.Transparent;
            this.LabelTime.Location = new Point(12, 131);
            this.LabelTime.Name = "LabelTime";
            this.LabelTime.Size = new Size(36, 23);
            this.LabelTime.TabIndex = 5;
            this.LabelTime.Text = "0:00";
            this.LabelTime.TextAlign = ContentAlignment.MiddleCenter;
            this.LabelTime.Visible = false;
            // 
            // TableLayoutPanel1
            // 
            this.TableLayoutPanel1.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            this.TableLayoutPanel1.AutoSize = true;
            this.TableLayoutPanel1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.TableLayoutPanel1.BackColor = Color.Transparent;
            this.TableLayoutPanel1.ColumnCount = 4;
            this.TableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            this.TableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            this.TableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            this.TableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            this.TableLayoutPanel1.Controls.Add(this.Button2, 2, 0);
            this.TableLayoutPanel1.Controls.Add(this.Button4, 0, 0);
            this.TableLayoutPanel1.Controls.Add(this.Button3, 1, 0);
            this.TableLayoutPanel1.Controls.Add(this.Button1, 3, 0);
            this.TableLayoutPanel1.GrowStyle = TableLayoutPanelGrowStyle.AddColumns;
            this.TableLayoutPanel1.Location = new Point(44, 127);
            this.TableLayoutPanel1.Name = "TableLayoutPanel1";
            this.TableLayoutPanel1.RowCount = 1;
            this.TableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            this.TableLayoutPanel1.Size = new Size(324, 31);
            this.TableLayoutPanel1.TabIndex = 6;
            // 
            // Label1
            // 
            this.Label1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            this.Label1.AutoSize = true;
            this.Label1.Location = new Point(68, 9);
            this.Label1.MaximumSize = new Size(297, 0);
            this.Label1.MinimumSize = new Size(297, 100);
            this.Label1.Name = "Label1";
            this.Label1.Size = new Size(297, 100);
            this.Label1.TabIndex = 7;
            this.Label1.Text = "Text here";
            this.Label1.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // FormMessageBox
            // 
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.BackColor = SystemColors.ControlLightLight;
            this.ClientSize = new Size(374, 166);
            this.Controls.Add(this.Label1);
            this.Controls.Add(this.TableLayoutPanel1);
            this.Controls.Add(this.LabelTime);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormMessageBox";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.TableLayoutPanel1.ResumeLayout(false);
            this.TableLayoutPanel1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Button Button1;
        private Button Button2;
        private Button Button3;
        private Button Button4;
        private Label LabelTime;
        private TableLayoutPanel TableLayoutPanel1;
        private Label Label1;
    }
}

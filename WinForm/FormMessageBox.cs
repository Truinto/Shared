using Shared.CollectionNS;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Shared
{
    public partial class FormMessageBox : Form
    {
        public TimeSpan Timeout;
        public DialogResult[] ButtonResults;
        private System.Windows.Forms.Timer? timer1;
        private TimeSpan current_timeout;
        private Button defaultButton;

        public FormMessageBox(string text, string caption, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton, string[] buttonLabels, DialogResult[] buttonResults, TimeSpan timeout = default)
        {
            InitializeComponent();

            Label1.Text = text;
            base.Text = caption;
            this.ButtonResults = buttonResults;
            this.Timeout = timeout;

            Icon = icon switch
            {
                MessageBoxIcon.Warning => SystemIcons.Warning,
                MessageBoxIcon.Error => SystemIcons.Error,
                MessageBoxIcon.Question => SystemIcons.Question,
                _ => SystemIcons.Information,
            };

            for (int i = 0; i < buttonLabels.Length; i++)
            {
                switch (i)
                {
                    case 0:
                        Button1.Text = buttonLabels[i];
                        Button1.Visible = true;
                        break;
                    case 1:
                        Button2.Text = buttonLabels[i];
                        Button2.Visible = true;
                        break;
                    case 2:
                        Button3.Text = buttonLabels[i];
                        Button3.Visible = true;
                        break;
                    case 3:
                        Button4.Text = buttonLabels[i];
                        Button4.Visible = true;
                        break;
                }
            }

            switch (buttonResults.GetIndex(DialogResult.Cancel))
            {
                case 0:
                    CancelButton = Button1;
                    break;
                case 1:
                    CancelButton = Button2;
                    break;
                case 2:
                    CancelButton = Button3;
                    break;
                case 3:
                    CancelButton = Button4;
                    break;
            }

            this.defaultButton = defaultButton switch
            {
                MessageBoxDefaultButton.Button2 => Button2,
                MessageBoxDefaultButton.Button3 => Button3,
                MessageBoxDefaultButton.Button4 => Button4,
                _ => Button1,
            };

            RecalculateSize();
        }

        public void RecalculateSize()
        {
            this.Width = Math.Max(390, TableLayoutPanel1.Width + 80); // stretch window width to fit buttons
            this.Label1.MinimumSize = new(this.Width - 93, 100); // stretch text to window width
            this.Label1.MaximumSize = new(this.Width - 93, 0);
            this.Height = Math.Max(205, this.Label1.Height + 105); // stretch window height to fix text
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            defaultButton.Select();

            if (Timeout != default)
            {
                current_timeout = Timeout;
                LabelTime.Text = Timeout.ToString("mm\\:ss");
                LabelTime.Visible = true;
                if (timer1 == null)
                {
                    timer1 = new();
                    timer1.Interval = 1000;
                    timer1.Tick += Timer_Tick;
                }
                timer1.Start();
            }
            else
            {
                LabelTime.Visible = false;
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            timer1?.Stop();
            base.OnFormClosed(e);
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            current_timeout = current_timeout.Subtract(TimeSpan.FromSeconds(1));
            if (current_timeout <= TimeSpan.Zero)
                defaultButton.PerformClick();
            else
                LabelTime.Text = current_timeout.ToString("mm\\:ss");
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (Icon != null)
            {
                e.Graphics.DrawIconUnstretched(Icon, new Rectangle(20, this.Height / 2 - 60, Icon.Width, Icon.Height)); //43
            }
        }

        private Brush BackgroundBrush = new SolidBrush(SystemColors.Control);
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);

            e.Graphics.FillRectangle(BackgroundBrush, 0, this.TableLayoutPanel1.Top - 10, this.Width, 50);
        }

        public new string Text
        {
            get => Label1.Text;
            set => Label1.Text = value;
        }

        public string Caption
        {
            get => base.Text;
            set => base.Text = value;
        }

        public string ButtonText1
        {
            get => Button1.Text;
            set
            {
                Button1.Text = value;
                Button1.Visible = value is not (null or "");
            }
        }

        public string ButtonText2
        {
            get => Button2.Text;
            set
            {
                Button2.Text = value;
                Button2.Visible = value is not (null or "");
            }
        }

        public string ButtonText3
        {
            get => Button3.Text;
            set
            {
                Button3.Text = value;
                Button3.Visible = value is not (null or "");
            }
        }

        public string ButtonText4
        {
            get => Button4.Text;
            set
            {
                Button4.Text = value;
                Button4.Visible = value is not (null or "");
            }
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            DialogResult = ButtonResults.AtIndex(0);
            Close();
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            DialogResult = ButtonResults.AtIndex(1);
            Close();
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            DialogResult = ButtonResults.AtIndex(2);
            Close();
        }

        private void Button4_Click(object sender, EventArgs e)
        {
            DialogResult = ButtonResults.AtIndex(3);
            Close();
        }

        public static DialogResult Show(string text, string caption, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton, string[] buttonLabels, DialogResult[] buttonResults, TimeSpan timeout = default)
        {
            return new FormMessageBox(text, caption, icon, defaultButton, buttonLabels, buttonResults, timeout).ShowDialog();
        }
    }
}

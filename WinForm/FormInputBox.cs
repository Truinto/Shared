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
    public partial class FormInputBox : Form
    {
        public TimeSpan Timeout;
        private System.Windows.Forms.Timer? _Timer1;
        private TimeSpan _CurrentTimeout;

        public FormInputBox(string text, string caption, MessageBoxIcon icon, TimeSpan timeout = default)
        {
            InitializeComponent();

            AcceptButton = Button1;
            CancelButton = Button2;

            Label1.Text = text;
            base.Text = caption;
            this.Timeout = timeout;

            Icon = icon switch
            {
                MessageBoxIcon.Warning => SystemIcons.Warning,
                MessageBoxIcon.Error => SystemIcons.Error,
                MessageBoxIcon.Question => SystemIcons.Question,
                _ => SystemIcons.Information,
            };

            RecalculateSize();
        }

        public void RecalculateSize()
        {
            this.Width = 390;
            this.Label1.MinimumSize = new(this.Width - 93, 70); // stretch text to window width
            this.Label1.MaximumSize = new(this.Width - 93, 0);
            this.Height = Math.Max(205, this.Label1.Height + 135); // stretch window height to fix text
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            if (Timeout != default)
            {
                _CurrentTimeout = Timeout;
                LabelTime.Text = Timeout.ToString("mm\\:ss");
                LabelTime.Visible = true;
                if (_Timer1 == null)
                {
                    _Timer1 = new();
                    _Timer1.Interval = 1000;
                    _Timer1.Tick += Timer_Tick;
                }
                _Timer1.Start();
            }
            else
            {
                LabelTime.Visible = false;
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _Timer1?.Stop();
            base.OnFormClosed(e);
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            _CurrentTimeout = _CurrentTimeout.Subtract(TimeSpan.FromSeconds(1));
            if (_CurrentTimeout <= TimeSpan.Zero)
                Button1.PerformClick();
            else
                LabelTime.Text = _CurrentTimeout.ToString("mm\\:ss");
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (Icon != null)
            {
                e.Graphics.DrawIconUnstretched(Icon, new Rectangle(20, this.Height / 2 - 75, Icon.Width, Icon.Height)); //43
            }
        }

        private Brush _BackgroundBrush = new SolidBrush(SystemColors.Control);
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);

            e.Graphics.FillRectangle(_BackgroundBrush, 0, Button1.Top - 12, this.Width, 50);
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

        public string InputBoxText
        {
            get => TextBox1.Text;
            set => TextBox1.Text = value;
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        public static DialogResult Show(string text, string caption, MessageBoxIcon icon, TimeSpan timeout = default)
        {
            return new FormInputBox(text, caption, icon, timeout).ShowDialog();
        }
    }
}

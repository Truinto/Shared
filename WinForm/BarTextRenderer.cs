using BrightIdeasSoftware;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public class BarTextRenderer : BarRenderer
    {
        public BarTextRenderer()
        {
            this.MinimumValue = 0;
            this.MaximumValue = 1;
            this.MaximumWidth = int.MaxValue;
        }

        public override void Render(Graphics g, Rectangle r)
        {
            // draw only background if percent is zero or lower (use 1E-44f to render 0%)
            float percent;
            if (this.Aspect is not IConvertible value || (percent = value.ToSingle(NumberFormatInfo.InvariantInfo)) <= 0f)
            {
                DrawBackground(g, r);
                return;
            }

            // render bar and text
            base.Render(g, r);
            g.DrawString($"{percent * 100f:0}%", this.Font, this.TextBrush, r, _format ??= new StringFormat()
            {
                LineAlignment = StringAlignment.Center,
                Trimming = StringTrimming.None,
                Alignment = StringAlignment.Center,
                FormatFlags = StringFormatFlags.NoWrap,
            });
        }

        private static StringFormat? _format;
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    [ToolboxItem(false)]
    internal class ULongUpDown : NumericUpDown
    {
        public ULongUpDown()
        {
            this.DecimalPlaces = 0;
            this.Minimum = ulong.MinValue;
            this.Maximum = ulong.MaxValue;
        }

        public new ulong Value
        {
            get => Convert.ToUInt64(base.Value);
            set => base.Value = new decimal(value);
        }
    }

    [ToolboxItem(false)]
    public class FloatCellEditor : NumericUpDown
    {
        public FloatCellEditor()
        {
            this.DecimalPlaces = 2;
            this.Minimum = -9999999;
            this.Maximum = 9999999;
        }

        public new float Value
        {
            get => Convert.ToSingle(base.Value);
            set => base.Value = new decimal(value);
        }
    }
}

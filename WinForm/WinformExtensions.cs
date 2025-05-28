using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Shared
{
    public static class WinformExtensions
    {

        public const string EllipsisChars = "\u2026";

        public static string StringTruncateLeft(this string text, Font font, int width)
        {
            // text fits in full
            var measure_length = TextRenderer.MeasureText(text, font).Width;
            if (measure_length <= width)
                return text;

            // shorten target length by ellipsis length
            width = width + 1 - TextRenderer.MeasureText(EllipsisChars, font).Width;

            // measure half segment_length and check if text fits, then repeat until segment_length is 1 character
            int fit_length = 0;
            int length = 0;
            int segment_length = text.Length;
            while (segment_length > 1)
            {
                segment_length -= segment_length / 2;

                int test_length = text.Length - length - segment_length;
                measure_length = TextRenderer.MeasureText(text.AsSpan(test_length), font).Width;
                if (measure_length <= width)
                {
                    length += segment_length;
                    fit_length = test_length;
                }
            }

            return fit_length == 0 ? EllipsisChars : $"{EllipsisChars}{text[fit_length..]}";
        }
    }
}

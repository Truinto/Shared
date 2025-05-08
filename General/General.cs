using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace Shared.GeneralNS
{
    public static class General
    {
        public static readonly BindingFlags BindingAll = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;
        public static readonly BindingFlags BindingInstance = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        public static readonly BindingFlags BindingStatic = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;

        public static void PrintToDebug(this object obj)
        {
            if (obj is null)
                return;

            var type = obj.GetType();

            Debug.WriteLine($"Print of {type.FullName}");
            foreach (var fi in type.GetFields(BindingInstance))
            {
                object? value = fi.GetValue(obj);
                Debug.WriteLine($"  field={fi.Name} f-type={fi.FieldType} o-type={value?.GetType()} tostring={value}");
            }

            foreach (var pi in type.GetProperties(BindingInstance))
            {
                try
                {
                    object? value = pi.GetValue(obj);
                    Debug.WriteLine($"  property={pi.Name} f-type={pi.PropertyType} o-type={value?.GetType()} tostring={value}");
                } catch (Exception)
                {
                    Debug.WriteLine($"  property={pi.Name} f-type={pi.PropertyType}");
                }
            }
        }

        public static bool TryToSingle(this object obj, out float value)
        {
            try
            {
                value = Convert.ToSingle(obj, CultureInfo.InvariantCulture);
                return true;
            } catch (Exception)
            {
                value = float.NaN;
                return false;
            }
        }

        public static bool TryToDouble(this object obj, out double value)
        {
            try
            {
                value = Convert.ToDouble(obj, CultureInfo.InvariantCulture);
                return true;
            } catch (Exception)
            {
                value = double.NaN;
                return false;
            }
        }

        /// <summary>
        /// Opens a path in the default explorer.<br/>
        /// Directory path start the explorer in that directory. File path is run with default app.<br/>
        /// If <paramref name="location"/> is true, the explorer will instead navigate to that path and select it.<br/>
        /// Not tested in linux/macOS.
        /// </summary>
        public static void OpenExplorer(string? path, bool location = false)
        {
            if (path is null or "")
                return;

            if (Path.Exists(path))
            {
                if (OperatingSystem.IsWindows())
                    Process.Start("explorer.exe", location ? ["/select,", "file://" + path] : ["file://" + path]);
                else if (OperatingSystem.IsLinux())
                    Process.Start("xdg-open", ["file://" + (location ? Path.GetDirectoryName(path) : path)]);
                else if (OperatingSystem.IsMacOS())
                    Process.Start("open", location ? ["-R", path] : [path]);
            }
        }
    }
}

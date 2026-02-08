using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace SlimeSim.Gui
{
    public static class WpfUtil
    {
        public static void DispatchRender(Dispatcher dispatcher, Action action)
        {
            dispatcher.BeginInvoke(
            DispatcherPriority.Background,
            new Action(() => action()));
        }

        public static string GetTagAsString(object element)
        {
            if (element == null)
                return null;

            if (element is FrameworkElement)
            {
                var el = (FrameworkElement)element;
                if (el.Tag is string)
                    return el.Tag as string;
                else
                    return null;
            }
            else
                return null;
        }

        public static int GetTagAsInt(object element)
        {
            var str = GetTagAsString(element);
            if (int.TryParse(str, out var val))
            {
                return val;
            }

            return 0;
        }

        public static double GetTagAsDouble(object element)
        {
            var str = GetTagAsString(element);
            if (double.TryParse(str, out var val))
            {
                return val;
            }

            return 0;
        }

        public static T GetTagAsObject<T>(object element) where T : class
        {
            if (element is FrameworkElement)
            {
                var el = (FrameworkElement)element;
                if (el.Tag is T)
                    return el.Tag as T;
                else
                    return null;
            }
            else
                return null;
        }

        public static void SelectByDoubleTag(System.Windows.Controls.ComboBox combo, double value)
        {
            for (int i = 0; i < combo.Items.Count; i++)
                if (GetTagAsDouble(combo.Items[i]) == value)
                    combo.SelectedIndex = i;
        }

        public static void SelectByIntTag(System.Windows.Controls.ComboBox combo, int value)
        {
            for (int i = 0; i < combo.Items.Count; i++)
                if (GetTagAsInt(combo.Items[i]) == value)
                    combo.SelectedIndex = i;
        }
    }
}

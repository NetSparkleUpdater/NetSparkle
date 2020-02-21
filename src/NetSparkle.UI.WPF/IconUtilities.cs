using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NetSparkle.UI.WPF
{
    public class IconUtilities
    {
        /// <summary>
        /// Convert System.Drawing.Icon to System.Windows.Media.ImageSource.
        ///  From: https://stackoverflow.com/a/6580799/3938401
        /// </summary>
        /// <param name="icon"></param>
        /// <returns></returns>
        public static ImageSource ToImageSource(Icon icon)
        {
            if (icon == null)
            {
                return null;
            }

            ImageSource imageSource = Imaging.CreateBitmapSourceFromHIcon(
                icon.Handle,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            return imageSource;
        }
    }
}

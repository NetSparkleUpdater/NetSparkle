using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NetSparkleUpdater.UI.WPF
{
    /// <summary>
    /// Helper class for processing <see cref="Icon"/> objects and 
    /// converting them to <see cref="ImageSource"/> objects
    /// </summary>
    public class IconUtilities
    {
        /// <summary>
        /// Convert System.Drawing.Icon to System.Windows.Media.ImageSource.
        ///  From: https://stackoverflow.com/a/6580799/3938401
        /// </summary>
        /// <param name="icon">The <see cref="Icon"/> that should be converted to an <see cref="ImageSource"/></param>
        /// <returns>An <see cref="ImageSource"/> representation of the given icon if <paramref name="icon"/> is non-null</returns>
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

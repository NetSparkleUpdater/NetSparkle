using System;
using System.Collections.Generic;
using System.Text;
using Avalonia.Media;

namespace NetSparkleUpdater.UI.Avalonia.Interfaces
{
    /// <summary>
    /// Interface for objects that have the ability to change background color
    /// </summary>
    public interface IBackgroundColorChanger
    {
        /// <summary>
        /// Change background color to a specific color
        /// </summary>
        /// <param name="color">color to change background to</param>
        void ChangeBackgroundColor(Brush color);
    }
}

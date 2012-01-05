using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using System.Drawing;
using System.Windows.Forms;

namespace AppLimit.NetSparkle
{
    /// <summary>
    /// Diagnostic for net Sparkle
    /// </summary>
    internal class NetSparkleDiagnostic
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private bool _isDiagnosticWindowShown;
        private NetSparkleMainWindows _diagnosticWindow;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="isDiagnosticWindowShown"><c>true</c> if a diagnostic window should be shown</param>
        public NetSparkleDiagnostic(bool isDiagnosticWindowShown)
        {
            _isDiagnosticWindowShown = isDiagnosticWindowShown;
            if (_isDiagnosticWindowShown)
            {
                _diagnosticWindow = new NetSparkleMainWindows();
            }
        }

        /// <summary>
        /// Reports a message
        /// </summary>
        /// <param name="message">the message</param>
        public void Report(string message)
        {
            if (_isDiagnosticWindowShown && _diagnosticWindow != null)
            {
                _diagnosticWindow.Report(message);
            }
            logger.Info(message);
        }

        /// <summary>
        /// Shows the diagnostic form if needed
        /// </summary>
        /// <param name="isShown"><c>true</c> if we want to show the diagnostic window</param>
        public void ShowDiagnosticWindowIfNeeded(bool isShown)
        {
            if (_isDiagnosticWindowShown && _diagnosticWindow != null)
            {
                if (_diagnosticWindow.InvokeRequired)
                {
                    _diagnosticWindow.Invoke(new Action(() => ShowDiagnosticWindowIfNeeded(isShown)));
                }
                else
                {
                    // check the diagnotic value
                    if (isShown || _isDiagnosticWindowShown)
                    {
                        Point newLocation = new Point();

                        newLocation.X = Screen.PrimaryScreen.Bounds.Width - _diagnosticWindow.Width;
                        newLocation.Y = 0;

                        _diagnosticWindow.Location = newLocation;
                        _diagnosticWindow.Show();
                    }
                }
            }
        }
    }
}

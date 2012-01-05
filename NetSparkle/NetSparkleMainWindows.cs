using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using NLog;

namespace AppLimit.NetSparkle
{
    /// <summary>
    /// Form to show the main window
    /// </summary>
    public partial class NetSparkleMainWindows : Form, IDisposable
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Constructor
        /// </summary>
        public NetSparkleMainWindows()
        {
            // init ui
            InitializeComponent();
        }

        /// <summary>
        /// Reports a message
        /// </summary>
        /// <param name="message">the message</param>
        public void Report(String message)
        {
            if (lstActions.InvokeRequired)
                lstActions.Invoke(new Action<String>(Report), message);
            else
            {
                // build the message 
                DateTime c = DateTime.Now;
                String msg = "[" + c.ToLongTimeString() + "." + c.Millisecond + "] " + message;

                // report the message into ui
                lstActions.Items.Add(msg);
            }
        }
        #region IDisposable Members

        void IDisposable.Dispose()
        {
            // close the base
            base.Dispose();
        }
        #endregion
    }
}

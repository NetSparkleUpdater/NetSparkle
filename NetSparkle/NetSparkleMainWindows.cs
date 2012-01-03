using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace AppLimit.NetSparkle
{
    /// <summary>
    /// Form to show the main window
    /// </summary>
    public partial class NetSparkleMainWindows : Form, IDisposable
    {
        private StreamWriter sw = null;

        /// <summary>
        /// Constructor
        /// </summary>
        public NetSparkleMainWindows()
        {
            // init ui
            InitializeComponent();

            // init logfile
            sw = File.CreateText(Path.Combine(Environment.ExpandEnvironmentVariables("%temp%"), "NetSparkle.log"));
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

                // report to file
                ReportToFile(msg);

                // report the message into ui
                lstActions.Items.Add(msg);
            }
        }

        /// <summary>
        /// Reports a message to a file
        /// </summary>
        /// <param name="msg">the message</param>
        private void ReportToFile(String msg)
        {
            try
            {
                // write 
                sw.WriteLine(msg);

                // flush
                sw.Flush();
            } catch(Exception)
            {

            }
        }

        #region IDisposable Members

        void IDisposable.Dispose()
        {
            // flush again
            sw.Flush();

            // close the stream
            sw.Dispose();

            // close the base
            base.Dispose();
        }

        #endregion
    }
}

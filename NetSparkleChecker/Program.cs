using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;

using AppLimit.NetSparkle;

namespace NetSparkleChecker
{
    static class Program
    {        
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // init app
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // get the commandline args
            String[] args = Environment.GetCommandLineArgs();
            if (args.Length != 3)
            {
                MessageBox.Show("The NetSparkle Update Checker requires the following 2 commandline parameters:\n\n1: path to the application executable\n2: url of the appcast",
                                "NetSparkle Update Checker - Syntax",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            else            
            {
                // check parameter
                if (!File.Exists(args[1]))
                {
                    MessageBox.Show("Couldn't find the application executable (" + Path.GetFullPath(args[1]) + ")",
                                    "NetSparkle Update Checker - Missing File",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                try
                {                    
                    // show the form
                    NetSparkleCheckerWaitUI frmWait = new NetSparkleCheckerWaitUI();
                    Application.Run(frmWait);

                    // check for update
                    if (frmWait.SparkleRequestedUpdate)
                    {
                        frmWait.ShowUpdateUI();
                    }
                    else
                    {
                        SoftwareUpToDate ok = new SoftwareUpToDate();
                        ok.ShowDialog();
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message, "Unhandled exception");
                }
            }            
        }                   
    }
}

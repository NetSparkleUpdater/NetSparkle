using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using NetSparkle;

namespace NetSparkleChecker
{
    public partial class NetSparkleCheckerWaitUI : Form
    {
        private Sparkle _sparkle;
        private NetSparkleAppCastItem LatesVersion = null;

        public Boolean SparkleRequestedUpdate = false;
        
        public NetSparkleCheckerWaitUI(Icon icon)
        {
            // init ui
            InitializeComponent();

            // get cmdline args
            String[] args = Environment.GetCommandLineArgs();

            // init sparkle
            _sparkle = new Sparkle(args[2], icon, args[1]);
            
            // set labels
            lblRefFileName.Text = args[1];
            lblRefUrl.Text = args[2];

            imgAppIcon.Image = icon.ToBitmap();
            Icon = icon;            

            bckWorker.RunWorkerAsync();
        }

        public void ShowUpdateUI()
        {
            _sparkle.ShowUpdateNeededUI(LatesVersion, false);
        }

        private void bckWorker_DoWork(object sender, DoWorkEventArgs e)
        {            
            // get the config
            NetSparkleConfiguration config = _sparkle.GetApplicationConfig();

            // check for updats
            NetSparkleAppCastItem latestVersion = null;
            Boolean bUpdateRequired = Sparkle.UpdateStatus.UpdateAvailable == _sparkle.GetUpdateStatus(config, out latestVersion);
                                
            // save the result
            SparkleRequestedUpdate = bUpdateRequired;
            LatesVersion = latestVersion;
        }

        private void bckWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // close the form
            Close();
        }        
    }
}

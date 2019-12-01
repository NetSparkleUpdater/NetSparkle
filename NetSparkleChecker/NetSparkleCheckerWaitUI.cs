using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

using NetSparkle;
using NetSparkle.Enums;

namespace NetSparkleChecker
{
    public partial class NetSparkleCheckerWaitUI : Form
    {
        private readonly Sparkle _sparkle;
        private List<AppCastItem> _updates;

        public bool SparkleRequestedUpdate = false;
        
        public NetSparkleCheckerWaitUI(Icon icon)
        {
            // init ui
            InitializeComponent();

            // get cmdline args
            string[] args = Environment.GetCommandLineArgs();

            // init sparkle
            _sparkle = new Sparkle(args[2], icon, SecurityMode.UseIfPossible, null, args[1]);
            
            // set labels
            lblRefFileName.Text = args[1];
            lblRefUrl.Text = args[2];

            imgAppIcon.Image = icon.ToBitmap();
            Icon = icon;            

            bckWorker.RunWorkerAsync();
        }

        public void ShowUpdateUI()
        {
            _sparkle.ShowUpdateNeededUI(_updates);
        }

        private async void bckWorker_DoWork(object sender, DoWorkEventArgs e)
        {            
            // get the config
            Configuration config = _sparkle.GetApplicationConfig();

            // check for updates
            UpdateInfo updateInfo = await _sparkle.GetUpdateStatus(config);
            bool bUpdateRequired = UpdateStatus.UpdateAvailable == updateInfo.Status;
                                
            // save the result
            SparkleRequestedUpdate = bUpdateRequired;
            _updates = updateInfo.Updates;
        }

        private void bckWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // close the form
            Close();
        }        
    }
}

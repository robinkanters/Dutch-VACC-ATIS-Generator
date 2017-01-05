using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Windows.Forms;
using DutchVACCATISGenerator.Forms;

namespace DutchVACCATISGenerator.Workers
{
    public interface IVersionWorker
    {
        /// <summary>
        /// Method called when version background workers is started. Pulls latest version number from my site.
        /// </summary>
        /// <param name="sender">Object sender</param>
        /// <param name="e">Event arguments</param>
        void VersionBackgroundWorker_DoWork(object sender, DoWorkEventArgs e);

        /// <summary>
        /// Method called when version background worker has completed its task. Compares executable version with pulled latest version and gives a message if a newer version is available.
        /// </summary>
        /// <param name="sender">Object sender</param>
        /// <param name="e">Event arguments</param>
        void VersionBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e);
    }

    public class VersionWorker : IVersionWorker
    {
        private string LatestVersion { get; set; }

        /// <summary>
        /// Method called when version background workers is started. Pulls latest version number from my site.
        /// </summary>
        /// <param name="sender">Object sender</param>
        /// <param name="e">Event arguments</param>
        public void VersionBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                //Get latest version.
#if DEBUG
                var request = WebRequest.Create("http://daanbroekhuizen.com/Dutch VACC/Dutch VACC ATIS Generator/Version/version2.php");
#else
                var request = WebRequest.Create("http://daanbroekhuizen.com/Dutch VACC/Dutch VACC ATIS Generator/Version/version.php");
#endif
                var response = request.GetResponse();
                var reader = new StreamReader(response.GetResponseStream());

                //Trim latest version string.
                LatestVersion = reader.ReadToEnd().Trim();
            }
            catch (Exception)
            {
                //Do nothing... to bad we couldn't get the latest version...
            }
        }

        /// <summary>
        /// Method called when version background worker has completed its task. Compares executable version with pulled latest version and gives a message if a newer version is available.
        /// </summary>
        /// <param name="sender">Object sender</param>
        /// <param name="e">Event arguments</param>
        public void VersionBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //If a latest version has been pulled.
            if (LatestVersion == null || LatestVersion.Equals(string.Empty))
                return;

            //If a newer version is available.
            if (LatestVersion.Equals(FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion.Trim()))
                return;

            while (LatestVersion.Contains("."))
            {
                LatestVersion = LatestVersion.Remove(LatestVersion.IndexOf(".", StringComparison.Ordinal), 1);
            }

            var applicationVersion = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion.Trim();

            while (applicationVersion.Contains("."))
            {
                applicationVersion = applicationVersion.Remove(applicationVersion.IndexOf(".", StringComparison.Ordinal), 1);
            }

            if (Convert.ToInt32(LatestVersion) <= Convert.ToInt32(applicationVersion))
                return;

            if (MessageBox.Show("Newer version is available.\nDownload latest version?", "Message", MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk) != DialogResult.Yes)
                return;

            Form autoUpdater = new AutoUpdater();
            autoUpdater.ShowDialog();
        }
    }
}

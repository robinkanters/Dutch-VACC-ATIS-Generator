using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Reflection;
using System.Windows.Forms;

namespace DutchVACCATISGenerator.Forms
{
    /// <summary>
    /// AutoUpdater class.
    /// </summary>
    public partial class AutoUpdater : Form
    {
        private string _fileName;
        private readonly string _executablePath;

        /// <summary>
        /// Constructor of AutoUpdater.
        /// </summary>
        public AutoUpdater()
        {
            InitializeComponent();

            //Set path of executable.
            _executablePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\";

            //Start downloading latest version.
            DownloadLatestVersion();
        }

        /// <summary>
        /// Download latest version of Dutch VACC ATIS Generator.
        /// </summary>
        private void DownloadLatestVersion()
        {
            try
            {
                //Create web client.
                var webClient = new WebClient();

                //Set web client's completed event.
                webClient.DownloadFileCompleted += webClient_DownloadCompleted;

                //Set web client's progress changed event.
                webClient.DownloadProgressChanged += webClient_ProgressChanged;

                //Download the zip file.
                webClient.DownloadFileAsync(new Uri("http://daanbroekhuizen.com/Dutch VACC/Dutch VACC ATIS Generator/" + GetZipName()), _executablePath + _fileName);
            }
            catch(Exception)
            {
                Close();
            }
        }

        /// <summary>
        /// Get the name of the zip file to download (version).
        /// </summary>
        /// <returns>Name of the latest zip file.</returns>
        private string GetZipName()
        {
            //Request zip file name.
            var request = WebRequest.Create("http://daanbroekhuizen.com/Dutch VACC/Dutch VACC ATIS Generator/Version/filename.php");
            var response = request.GetResponse();

            //Read zip file name.
            var reader = new StreamReader(response.GetResponseStream());
            _fileName = reader.ReadToEnd();
            
            //Return and trim file name;
            return _fileName = _fileName.Trim();
        }

        /// <summary>
        /// Web client progress changed event.
        /// </summary>
        /// <param name="sender">Object sender</param>
        /// <param name="e">Event arguments</param>
        private void webClient_ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            //Update progress bar.
            progressBar.Value = e.ProgressPercentage;
        }

        /// <summary>
        /// Called when web client is finished downloading.
        /// </summary>
        /// <param name="sender">Object sender</param>
        /// <param name="e">Event arguments</param>
        private void webClient_DownloadCompleted(object sender, AsyncCompletedEventArgs e)
        {
            try
            {
                //Extract zip.
                ZipFile.ExtractToDirectory(_executablePath + _fileName, _executablePath + @"\temp");

                //Set temp folder to be hidden.
                var directoryInfo = Directory.CreateDirectory(_executablePath + @"\temp");
                directoryInfo.Attributes = FileAttributes.Directory | FileAttributes.Hidden | FileAttributes.ReadOnly; 

                //Delete zip.
                File.Delete(_executablePath + _fileName);

                //Start setup.
                Process.Start(_executablePath + @"\temp\" + "Dutch VACC ATIS Generator - Setup.exe");

                //Exit program to run setup.
                Application.Exit();
            }
            catch(Exception)
            {
                Close();
            }
        }
    }
}


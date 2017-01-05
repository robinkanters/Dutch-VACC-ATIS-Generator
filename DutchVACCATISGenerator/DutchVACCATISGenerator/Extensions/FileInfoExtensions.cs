using System.IO;

namespace DutchVACCATISGenerator.Extensions
{
    public static class FileInfoExtensions
    {
        /// <summary>
        /// Check if file is in use by another process.
        /// </summary>
        /// <param name="file">File to check.</param>
        /// <returns>Boolean indicating if file is in use by another process.</returns>
        public static bool IsFileLocked(this FileInfo file)
        {
            FileStream stream = null;

            //Try to open file using the FileStream.
            try
            {
                stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException)
            {
                //File locked.
                return true;
            }
            //Close FileStream.
            finally
            {
                stream?.Close();
            }

            //File not locked.
            return false;
        }
    }
}

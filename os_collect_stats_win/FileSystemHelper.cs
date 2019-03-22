using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using ICSharpCode.SharpZipLib.Zip;
using System.IO;

namespace os_collect_stats_win
{
    public class FileSystemHelper
    {
        public FileSystemHelper() { }

        public void CreateTarGzFromDirectory(string sourceDirectory, string targetTarGzFilepath, bool isRecursive)
        {
            using (FileStream fs = new FileStream(targetTarGzFilepath, FileMode.Create, FileAccess.Write, FileShare.None))
            using (Stream gzipStream = new GZipOutputStream(fs))
            using (TarArchive tarArchive = TarArchive.CreateOutputTarArchive(gzipStream))
            {
                AddDirectoryFilesToTar(tarArchive, sourceDirectory, isRecursive);
            }
        }

        protected void AddDirectoryFilesToTar(TarArchive tarArchive, string sourceDirectory, bool isRecursive)
        {
            // Recursively add sub-folders
            if (isRecursive)
            {
                string[] directories = Directory.GetDirectories(sourceDirectory);
                foreach (string directory in directories)
                    AddDirectoryFilesToTar(tarArchive, directory, isRecursive);
            }

            // Add files
            string[] filenames = Directory.GetFiles(sourceDirectory);
            foreach (string filename in filenames)
            {
                TarEntry tarEntry = TarEntry.CreateEntryFromFile(filename);
                tarArchive.WriteEntry(tarEntry, true);
            }
        }

        public void CreateZipFromDirectory(string sourceDirectory, string targetZipFilepath, bool isRecursive)
        {
            FastZip zip = new FastZip();
            zip.CreateEmptyDirectories = true;
            zip.CreateZip(targetZipFilepath, sourceDirectory, isRecursive, "");
        }
    }
}

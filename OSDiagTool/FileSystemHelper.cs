using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using ICSharpCode.SharpZipLib.Zip;
using System.IO;
using System;

namespace OSDiagTool
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

        // Use this function to copy all the contents of a path. Set the copySubDirs to True if you want to copy as well all subfolders contents
        public void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs, int daysToFetch = 3)
        {
            var dtNow = new DateTime();
            var dtSubLastWrite = new DateTime();
            dtNow = DateTime.Now;
            dtSubLastWrite = dtNow.AddDays(-daysToFetch);


            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                if (file.LastWriteTime > dtSubLastWrite) {
                    string temppath = Path.Combine(destDirName, file.Name);
                    file.CopyTo(temppath, false);
                }
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }
    }
}

        



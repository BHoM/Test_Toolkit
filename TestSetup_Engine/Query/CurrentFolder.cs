using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BH.Tests.Setup
{
    public static partial class Query
    {
        public static string CurrentRepoFolder()
        {
            string currentDirectory = System.Environment.CurrentDirectory;

            string localRunningPath = Directory.GetParent(currentDirectory).FullName;
            localRunningPath = Path.Combine(localRunningPath, "LocalRunningRepoFolder.txt");

            if(File.Exists(localRunningPath))
            {
                string path = File.ReadAllText(localRunningPath);
                if(Directory.Exists(path))
                    return path;
            }

            string endFolder = "";
            int indexAdd = 0;
            if (currentDirectory.Contains(".ci"))
                endFolder = ".ci";
            else if (currentDirectory.Contains("Build"))
                endFolder = "Build";
            else if (currentDirectory.Contains("_Tests_"))
                endFolder = "_Tests_";
            else if (currentDirectory.Contains("bin"))
            { 
                endFolder = "bin";
                indexAdd = 1;
            }

            string[] split = currentDirectory.Split(Path.DirectorySeparatorChar);

            string folder = "";

            int i = 0;
            while (split.Length > i + indexAdd && split[i + indexAdd] != endFolder)
            {
                folder = Path.Combine(folder, split[i]);
                i++;
            }

            return folder;
        }

        /***************************************************/

        public static string CurrentCiFolder()
        {
            return Path.Combine(CurrentRepoFolder(), ".ci");
        }

        /***************************************************/

        public static string CurrentDatasetsUTFolder()
        {
            return Path.Combine(CurrentCiFolder(), "Datasets");
        }

        /***************************************************/
    }
}

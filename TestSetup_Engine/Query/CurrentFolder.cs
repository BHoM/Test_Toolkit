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
            if (!currentDirectory.Contains(".ci"))
                return null;

            string[] split = currentDirectory.Split(Path.DirectorySeparatorChar);

            string folder = "";

            int i = 0;
            while (i < split.Length && split[i] != ".ci")
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

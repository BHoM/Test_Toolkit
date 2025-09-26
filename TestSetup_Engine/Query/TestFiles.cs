using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.Tests.Setup
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Returns .cs files to be tested. Prioritises files from input parameters. If non available, then all files of the specified format in the currently executing repo are extracted.")]
        public static List<string> TestFilesCs()
        {
            return TestFiles(".cs");
        }

        /***************************************************/

        [Description("Returns .csproj files to be tested. Prioritises files from input parameters. If non available, then all files of the specified format in the currently executing repo are extracted.")]
        public static List<string> TestFilesCsproj()
        {
            return TestFiles(".csproj");
        }

        /***************************************************/

        [Description("Returns files of a specific type to be tested. Prioritises files from input parameters. If non available, then all files of the specified format in the currently executing repo are extracted.")]
        public static List<string> TestFiles(string fileEnding)
        {
            if(!fileEnding.StartsWith('.'))
                fileEnding = "." + fileEnding;

            if (m_testFiles.TryGetValue(fileEnding, out List<string> files))
                return files;

            lock (m_fileLock)
            {
                if (m_testFiles.TryGetValue(fileEnding, out files))
                    return files;

                files = Setup.Query.InputParametersUpdatedFiles()?.Where(f => Path.GetExtension(f).Equals(fileEnding, StringComparison.OrdinalIgnoreCase)).ToList();

                if (files == null)
                {
                    files = Setup.Query.GetFiles(Setup.Query.CurrentRepoFolder(), $"*{fileEnding}", true).ToList();
                    m_testFiles[fileEnding] = files;
                }
                return files;
            }

        }

        /***************************************************/
        /**** Private Fields                            ****/
        /***************************************************/

        private static Dictionary<string, List<string>> m_testFiles = new Dictionary<string, List<string>>();
        private static object m_fileLock = new object();
    }
}

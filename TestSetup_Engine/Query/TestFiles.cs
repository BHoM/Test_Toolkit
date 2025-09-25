using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.Tests.Setup
{
    public static partial class Query
    {
        public static IEnumerable<string> TestFiles(string fileEnding)
        {
            if (m_testFiles.TryGetValue(fileEnding, out List<string> files))
                return files;

            lock (m_fileLock)
            {
                if (m_testFiles.TryGetValue(fileEnding, out files))
                    return files;

                files = Setup.Query.InputParametersUpdatedFiles()?.Where(f => Path.GetExtension(f).Equals(fileEnding, StringComparison.OrdinalIgnoreCase)).ToList();

                if (files == null)
                {
                    files = Setup.Query.GetFiles(Setup.Query.CurrentRepoFolder(), $"*.{fileEnding}", true).ToList();
                    m_testFiles[fileEnding] = files;
                }
                return files;
            }

        }

        private static Dictionary<string, List<string>> m_testFiles = new Dictionary<string, List<string>>();
        private static object m_fileLock = new object();
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace BH.Tests.Setup
{
    public static partial class Query
    {
        public static IEnumerable<string> GetFiles(string folder, string searchPattern = "*.*", bool recursive = false)
        {
            if (string.IsNullOrEmpty(folder) || !System.IO.Directory.Exists(folder))
            {
                throw new ArgumentException("The specified folder does not exist.", nameof(folder));
            }

            foreach (string file in System.IO.Directory.EnumerateFiles(folder, searchPattern, System.IO.SearchOption.TopDirectoryOnly))
                yield return file;

            if (recursive)
            {
                foreach (string subFolder in System.IO.Directory.EnumerateDirectories(folder))
                {
                    if (!m_FolderExcluded.Contains(System.IO.Path.GetFileName(subFolder)))
                    {
                        foreach (string file in GetFiles(subFolder, searchPattern, true))
                        {
                            yield return file;
                        }
                    }
                }
            }
        }

        private static HashSet<string> m_FolderExcluded = new HashSet<string>(new string[] { "bin", "obj", "Build", "_dependencies" });
    }
}

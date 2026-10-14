using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BH.Tests.Setup
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/


        public static string CurrentRepository()
        {
            if (m_currentRepo != null)
                return m_currentRepo;

            lock (m_currentRepoLock)
            {
                if (m_currentRepo != null)
                    return m_currentRepo;

                string currentRepository = InputParametersCurrentRepository();

                if (currentRepository != null)
                    m_currentRepo = currentRepository;
                else
                    m_currentRepo = CurrentRepositoryFromGitConfig();

            }

            return m_currentRepo;
        }

        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/


        public static string CurrentRepositoryFromGitConfig()
        {

            string repoPath = CurrentRepoFolder();
            string gitConfigPath = Path.Combine(repoPath, ".git", "config");

            if (!File.Exists(gitConfigPath))
            {
                Console.WriteLine("Git config not found.");
                return null;
            }

            string[] configContent = File.ReadAllLines(gitConfigPath);

            foreach (string line in configContent)
            {
                if (line.Contains("url") && line.Contains("github.com/"))
                {
                    // Found a GitHub URL
                    int urlIndex = line.IndexOf("github.com/");
                    string sub = line.Substring(urlIndex);
                    return sub.Replace("github.com/", "").Replace(".git", "").Trim();
                }
            }
            

            return null;

        }

        /***************************************************/
        /**** Private Fields                            ****/
        /***************************************************/

        private static object m_currentRepoLock = new object();
        private static string m_currentRepo = null;
    }
}

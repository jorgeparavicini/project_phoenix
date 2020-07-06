#if AIUNITY_CODE
using UnityEngine;
using AiUnity.Common.IO;
using AiUnity.Common.Patterns;
using System.IO;
using System.Linq;

namespace AiUnity.NLog.Core.Common
{
    public class NLogConfigFile : UnityFileInfo<NLogConfigFile>
    {
        //private NLogConfigFile()
        public NLogConfigFile()
        {
            string configFullFileName = PlayerPrefs.GetString("AiUnityNLogConfigFullFileName");

            if (string.IsNullOrEmpty(configFullFileName)) {

                if (Application.isEditor)
                {
                    string NLoggerFile = Directory.GetFiles(Application.dataPath, "NLogger.cs", SearchOption.AllDirectories).
                        Select(s => s.Replace('\\', '/')).FirstOrDefault(s => s.Contains(@"/NLog/Core/"));
                    string aiUnityPath = string.IsNullOrEmpty(NLoggerFile) ? Application.dataPath : NLoggerFile.Substring(0, NLoggerFile.IndexOf("/NLog/Core/"));
                    string configPath = aiUnityPath + @"/UserData/NLog/Resources";
                    Directory.CreateDirectory(configPath);

                    configFullFileName = configPath + "/NLog.xml";
                }
                else
                {
                    configFullFileName = Application.dataPath + "/NLog.xml";
                }
            }
            FileInfo = new FileInfo(configFullFileName);
        }

        public void SetConfigFileName(string configFullFileName)
        {
            PlayerPrefs.SetString("AiUnityNLogConfigFullFileName", configFullFileName);
            FileInfo = new FileInfo(configFullFileName);
        }

        public string GetConfigText()
        {
            TextAsset textAsset = Resources.Load<TextAsset>(NameWithoutExtension);

            if (textAsset != null) {
                return textAsset.text;
            }
            else if (FileInfo.Exists) {
                return File.ReadAllText(FileInfo.FullName);
            }
            else {
                return null;
            }
        }
    }
}
#endif
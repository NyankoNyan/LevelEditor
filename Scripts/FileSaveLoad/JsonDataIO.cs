using System.IO;

using UnityEngine;

namespace Level.IO
{
    public static class JsonDataIO
    {
        /// <summary>
        /// Loaded data from json file. Data must be serializable.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="folder">Absolute or relative folder</param>
        /// <param name="file">File name without extension</param>
        /// <returns></returns>
        public static T LoadData<T>(string folder, string file)
        {
            string fullPath = FileFullName(folder, file);
            string json = File.ReadAllText(fullPath, LevelFileConsts.ENCODING);
            return JsonUtility.FromJson<T>(json);
        }

        /// <summary>
        /// Save data to json file. Data must be serializable.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="folder">Absolute or relative folder</param>
        /// <param name="file">File name without extension</param>
        /// <param name="prettyPrint">When true, file will be human readable</param>
        public static void SaveData(object obj, string folder, string file, bool prettyPrint)
        {
            string json = JsonUtility.ToJson(obj, prettyPrint);
            string fullPath = FileFullName(folder, file);
            File.WriteAllText(fullPath, json, LevelFileConsts.ENCODING);
        }

        public static string FileFullName(string folder, string file)
        {
            return $"{folder}/{file}{LevelFileConsts.JSON_EXTENSION}";
        }
    }
}
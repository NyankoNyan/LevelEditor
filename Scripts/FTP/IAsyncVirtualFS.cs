using System.Collections;

namespace Level.IO
{
    [Serializable]
    public struct FileInfo
    {
        public string name;
        public bool is_dir;
        public bool isDir => is_dir;
    }

    public interface IAsyncVirtualFS
    {
        IEnumerator GetFilesList(string path, Action<FileInfo[]> callback = null, Action<string> errorCallback = null);
        IEnumerator AddDirectory(string path, Action callback = null, Action<string> errorCallback = null);
        IEnumerator AddFile(string path, string content, Action callback = null, Action<string> errorCallback = null);
        IEnumerator GetFile(string path, Action<string> callback = null, Action<string> errorCallback = null);
    }
}
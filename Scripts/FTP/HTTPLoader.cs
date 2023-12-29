using System.Collections;

using UnityEngine;
using UnityEngine.Networking;

namespace Level.IO
{
    public class HTTPLoader : IAsyncVirtualFS
    {
        [Serializable]
        private struct FileInfoRequest
        {
            public string msg;
            public FileInfo[] files;
        }

        private string _mainURLPart;

        public HTTPLoader(string mainURLPart)
        {
            _mainURLPart = mainURLPart;
        }

        public IEnumerator GetFilesList(string path, Action<FileInfo[]> callback = null, Action<string> errorCallback = null)
        {
            string url = _mainURLPart + path;
            using (UnityWebRequest request = UnityWebRequest.Get(url)) {
                yield return request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.Success) {
                    var filesInfo = JsonUtility.FromJson<FileInfoRequest>(request.downloadHandler.text);
                    callback?.Invoke(filesInfo.files);
                } else {
                    errorCallback?.Invoke(request.downloadHandler.text);
                }
            }
        }

        public IEnumerator AddDirectory(string path, Action callback = null, Action<string> errorCallback = null)
        {
            string url = _mainURLPart + path;
            using (UnityWebRequest request = UnityWebRequest.Post(url)) {
                yield return request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.Success) {
                    callback?.Invoke();
                } else {
                    errorCallback?.Invoke(request.downloadHandler.text);
                }
            }
        }

        public IEnumerator AddFile(string path, string content, Action callback = null, Action<string> errorCallback = null)
        {
            string url = _mainURLPart + path;
            using (UnityWebRequest request = UnityWebRequest.Post(url, content, "application/octet-stream")) {
                yield return request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.Success) {
                    callback?.Invoke();
                } else {
                    errorCallback?.Invoke(request.downloadHandler.text);
                }
            }
        }

        public IEnumerator GetFile(string path, Action<string> callback = null, Action<string> errorCallback = null)
        {
            string url = _mainURLPart + path;
            using (UnityWebRequest request = UnityWebRequest.Get(url)) {
                yield return request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.Success) {
                    callback?.Invoke(request.downloadHandler.text);
                } else {
                    errorCallback?.Invoke(request.downloadHandler.text);
                }
            }
        }
    }
}
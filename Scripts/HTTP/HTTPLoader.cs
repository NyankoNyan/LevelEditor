using System;
using System.Collections;
using System.Linq;
using System.Text.RegularExpressions;

using Level.API;

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
            using (UnityWebRequest request = UnityWebRequest.Post(url, "", "application/octet-stream")) {
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

    public class AsyncFSLevelSaver : MonoBehaviour, ILevelSave
    {
        [SerializeField] private string _defaultPath;
        [SerializeField] private bool _prettyPrint = true;

        /// <summary>
        /// Virtual file system provider
        /// </summary>
        private IAsyncVirtualFS _fs;

        private bool _initialized;
        private Status _status;

        private void Start()
        {
            if (!_initialized) {
                Init(_defaultPath);
            }
        }

        public void Init(string path)
        {
            if (Regex.IsMatch(path, @"^https?:://")) {
                _fs = new HTTPLoader(path);
            } else {
                throw new LevelAPIException($"Unknown file system provider on path{path}");
            }
            _initialized = true;
        }

        public IEnumerator SaveFullContent(LevelAPI level, string savePath = null)
        {
            string currentPath = NormalizeURL(level.LevelSettings.levelStoreFolder);

            yield return SaveBlockProtos(level, currentPath);
            yield return SaveGridSettings(level, currentPath);
            yield return SaveGridStates(level, currentPath);
        }

        private IEnumerator SaveData(object obj, string folder, string file)
        {
            string content = JsonUtility.ToJson(obj, _prettyPrint);
            string fullPath = ConcatPath(folder, file) + LevelFileNames.JSON_EXTENSION;
            bool error = false;
            yield return _fs.AddFile(
                fullPath,
                content,
                errorCallback: s => error = true);
            if (error) {
                throw new LevelAPIException($"Can't save file {file}");
            }
        }

        private IEnumerator SaveBlockProtos(LevelAPI level, string currentPath)
        {
            yield return SaveData(
                new ListWrapper<BlockProtoSerializable>(level.BlockProtoCollection.Select(x => (BlockProtoSerializable)x).ToArray()),
                currentPath,
                LevelFileNames.FILE_BLOCK_PROTO
            );
        }

        private IEnumerator SaveGridSettings(LevelAPI level, string currentPath)
        {
            yield return SaveData(
                new ListWrapper<GridSettingsSerializable>(level.GridSettingsCollection.Select(x => (GridSettingsSerializable)x).ToArray()),
                currentPath,
                LevelFileNames.FILE_GRID_SETTINGS
            );
        }

        private IEnumerator SaveGridStates(LevelAPI level, string currentPath)
        {
            yield return SaveGridStatesHeaders(level, currentPath);
            yield return SaveGridStatesBodies(level, currentPath);
        }

        private IEnumerator SaveGridStatesHeaders(LevelAPI level, string currentPath)
        {
            yield return SaveData(
                new ListWrapper<GridStateSerializable>(level.GridStatesCollection.Select(x => (GridStateSerializable)x).ToArray()),
                currentPath,
                LevelFileNames.FILE_GRID_STATE
            );
        }

        private IEnumerator SaveGridStatesBodies(LevelAPI level, string currentPath)
        {
            foreach (var gridState in level.GridStatesCollection) {
                yield return SaveGridStateBody(gridState, currentPath);
            }
        }

        private IEnumerator SaveGridStateBody(GridState gridState, string currentPath)
        {
            foreach (var dataLayer in gridState.DataLayers) {
                yield return SaveDataLayer(dataLayer, gridState, currentPath);
            }
        }

        private IEnumerator SaveDataLayer(DataLayer dataLayer, GridState gridState, string currentPath)
        {
            var routineSplitter = new RoutineSplitter(this);
            string dataLayerSavePath = ConcatPath(currentPath, LevelFileNames.GetDataLayerSubFolder(dataLayer.Settings, gridState));
            if (dataLayer is SimpleChunkLayer<BlockData> blockLayer) {
                foreach (Vector3Int chunkCoord in blockLayer.ExistedChunks) {
                    var chunkData = blockLayer.GetChunkData(chunkCoord);
                    routineSplitter.AddRoutine(SaveChunk(dataLayerSavePath, chunkCoord, chunkData));
                }
            } else if (dataLayer is SimpleChunkLayer<BigBlockData> bigBlockLayer) {
                foreach (Vector3Int chunkCoord in bigBlockLayer.ExistedChunks) {
                    var chunkData = bigBlockLayer.GetChunkData(chunkCoord);
                    routineSplitter.AddRoutine(SaveChunk(dataLayerSavePath, chunkCoord, chunkData));
                }
            } else {
                throw new NotImplementedException();
            }
            yield return routineSplitter.Start();
        }

        private IEnumerator SaveChunk<T>(string dataLayerSavePath, Vector3Int coord, DataLayerContent<T> currentData)
        {
            string filename = $"{coord.x}_{coord.y}_{coord.z}";
            if (currentData is DataLayerStaticContent<BlockData> bdContent) {
                var serializable = (BlockChunkConvertSerializable)bdContent;
                yield return SaveData(serializable, dataLayerSavePath, filename);
            } else if (currentData is DataLayerDynamicContent<BigBlockData> bbdContent) {
                var serializable = (BigBlockChunkContentSerializable)bbdContent;
                yield return SaveData(serializable, dataLayerSavePath, filename);
            } else {
                throw new NotImplementedException();
            }
        }

        private static string NormalizeURL(string url)
        {
            string result = url.Replace('\\', '/');
            return result;
        }

        private static string ConcatPath(params string[] parts)
        {
            if (parts.Length == 0) {
                return "";
            } else {
                string result = parts[0];
                for (int i = 1; i < parts.Length; i++) {
                    result += '/' + parts[i];
                }
                return result;
            }
        }

        public enum Status
        {
            NoTasks = 0,
            Working,
            Complete,
            Error
        }
    }
}
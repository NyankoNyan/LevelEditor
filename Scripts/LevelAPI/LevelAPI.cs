using System;
using System.Text.RegularExpressions;

using Level.IO;

using UnityEngine;

namespace Level.API
{
    public class LevelAPIException : Exception
    {
        public LevelAPIException(string msg) : base(msg)
        {
        }
    }

    public class LevelAPIFabric
    {
        public LevelAPI Create(LevelSettings gameSettings = default)
        {
            return new LevelAPI(gameSettings);
        }
    }

    public enum ChunkStorageStrategy
    {
        DontSave = 0,
        AllTogether = 1,
        DynamicSaveLoad = 2
    }

    public enum StorageMode
    {
        None = 0,
        Local = 1,
        RemoteFS = 2,
    }

    [Serializable]
    public struct LevelSettings
    {
        /// <summary>
        /// Способ хранения чанков. Пока толком не работает.
        /// Чанк долженбыть быть загружен или сохранён в рамках инициализации или общего сохранения уровня (всё вместе).
        /// Или же чанк загрузится, сохранится и выгрузится по мере передвижения по уровню.
        /// </summary>
        public ChunkStorageStrategy chunkStorageStrategy;

        /// <summary>
        /// Место хранения уровня.
        /// 1) Если пусто, значит уровень никуда не записывается и просто лежит в памяти.
        /// Память рано или поздно закончится, но если есть задача генерировать уровень, а потом
        /// его стирать, вполне нормальная стратегия.
        /// 2) Если начинается с file://, то уровень находится на локальном диске.
        /// Это может быть путь в любом поддерживаемом системой виде:
        /// file://D:\Temp\Levels, file:///home/user, file://%APPDATA%/Levels
        /// 3) Если начинается с https:// или http://, то уровень доступен по HTTP-запросу.
        /// По уиолчания используется мой FileServer. Если нужно что-то другое, делайте как в sqlalchemy,
        /// типа fileserver:https://service.local:2020. Карочи придумайте сами.
        /// </summary>
        public string levelStoreURI;

        public StorageMode storageMode;

        /// <summary>
        /// Имя уровня. Это техническое имя, и какбы желательно вводить его в каком-нибудь читаемом формате.
        /// </summary>
        public string name;
    }

    /// <summary>
    /// Предоставляет доступ к API уровня в целом и отдельным его компонентам.
    /// </summary>
    public class LevelAPI
    {
        private BlockProtoCollection _blockProtoCollection;

        private GridStatesCollection _gridStatesCollection;

        private GridSettingsCollection _gridSettingsCollection;

        private ChunkStorageFabric _chunkStorageFabric;

        private UserManager _userManager;
        private LevelSettings _settings;
        private ILevelLoader _levelLoader;
        private ILevelSave _levelSaver;

        private bool _changed;

        internal LevelAPI(LevelSettings settings)
        {
            _settings = settings;

            switch (_settings.chunkStorageStrategy) {
                case ChunkStorageStrategy.DontSave:
                    _chunkStorageFabric = new MockChunkStorageFabric();
                    break;

                case ChunkStorageStrategy.AllTogether:
                case ChunkStorageStrategy.DynamicSaveLoad:
                    _chunkStorageFabric =
                        new FileChunkStorageFabric(_settings.levelStoreURI + "\\" + LevelFileNames.DIR_CHUNKS);
                    _levelLoader = new FileLevelLoader(_settings.levelStoreURI, _chunkStorageFabric);
                    _levelSaver = new FileLevelSaver(_settings.levelStoreURI, true, _chunkStorageFabric);
                    break;

                default:
                    throw new Exception($"Unknown chunk storage strategy {_settings.chunkStorageStrategy}");
            }

            _blockProtoCollection = new BlockProtoCollection();
            _gridSettingsCollection = new GridSettingsCollection();
            _gridStatesCollection = new GridStatesCollection(this);
            _userManager = new();
        }

        public void Destroy()
        {
            _gridStatesCollection.Destroy();
            _gridSettingsCollection.Destroy();
            _blockProtoCollection.Destroy();
        }

        #region Public API

        /// <summary>
        /// Это настройки гридов. Сиречь сеток. Я хуй знает как это ещё объснить.
        /// </summary>
        public GridSettingsCollection GridSettingsCollection => _gridSettingsCollection;

        /// <summary>
        /// Прототипы блоков. Это ещё не блоки. То есть их ещё не существует.
        /// </summary>
        public BlockProtoCollection BlockProtoCollection => _blockProtoCollection;

        /// <summary>
        /// Гриды как они есть. Внутри чанки. И блоки. Блоки настоящие.
        /// </summary>
        public GridStatesCollection GridStatesCollection => _gridStatesCollection;

        /// <summary>
        /// Права пользователей. Просто обозначили пока, что они будут. Может и правда будут.
        /// </summary>
        public UserManager UserManager => _userManager;

        public Action settingsChanged;
        public LevelSettings LevelSettings => _settings;

        public bool Changed => _changed;

        public string Name {
            get => _settings.name;
            set => _settings.name = value;
        }

        public string LevelPathURL {
            get => _settings.levelStoreURI;
            set => SetLevelPathURL(value);
        }

        /// <summary>
        /// Специальный интерфейс, который предоставляет хралища для динамической подгрузки чанков.
        /// Не уверен, что он работает. Не уверен, что оно надо. Ладно. Может доделаю.
        /// </summary>
        internal ChunkStorageFabric ChunkStorageFabric => _chunkStorageFabric;

        /// <summary>
        /// Спасите уровень. Пусть пока будет здесь. По хорошему нужен отдельный интерфейс. Но хуй с ним пока.
        /// </summary>
        /// <param name="levelPath"></param>
        /// <exception cref="LevelAPIException"></exception>
        public void SaveLevel(string levelPath = null)
        {
            if (_settings.chunkStorageStrategy == ChunkStorageStrategy.DontSave) {
                throw new LevelAPIException($"Level saving not available");
            }

            _levelSaver.SaveFullContent(this, levelPath);
        }

        public void LoadLevel(string levelPath = null)
        {
            if (_settings.chunkStorageStrategy == ChunkStorageStrategy.DontSave) {
                throw new LevelAPIException($"Level loading not available");
            }

            _levelLoader.LoadFullContent(this, levelPath);
        }

        public bool ValidateLevelPathURL(string url)
        {
            return Regex.IsMatch(url, @"^file://.*$|^https?://$");
        }

        private void SetLevelPathURL(string url)
        {
            if (!string.IsNullOrWhiteSpace(url)) {
                if (ValidateLevelPathURL(url)) {
                    var match = Regex.Match(url, @"^file://(.*)$|^http(s?)://(.*)$");
                    if (match.Success) {
                        var groups = match.Groups;
                        if (!string.IsNullOrWhiteSpace(groups[1].Value)) {
                            //TODO create file interface
                            _settings.storageMode = StorageMode.Local;
                        } else {
                            //TODO create REST interface
                            _settings.storageMode = StorageMode.RemoteFS;
                        }

                        _settings.levelStoreURI = url;
                    } else {
                        throw new LevelAPIException($"Level url {url} is invalid (but why?)");
                    }
                } else {
                    throw new LevelAPIException($"Level url {url} is invalid");
                }
            } else {
                _settings.levelStoreURI = "";
                _settings.storageMode = StorageMode.None;
            }

            settingsChanged?.Invoke();
        }

        #endregion Public API
    }

    public static class LevelAPITools
    {
        public static void Clear(Transform target)
        {
            while (target.childCount > 0) {
                GameObject.DestroyImmediate(target.GetChild(0).gameObject);
            }
        }
    }
}
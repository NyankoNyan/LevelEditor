using System;
using System.IO;
using System.Linq;

using Level.API;

using LevelView;

using UnityEngine;
using UnityEngine.Assertions;

namespace Level.Samples
{
    /// <summary>
    /// Типа всё простенько. Попробуем повыполнять всякие методы и посмотреть
    /// состояние сцены после этого. Может это даст понимание, работает API
    /// или нет.
    /// </summary>
    public class TestLevelEditor : MonoBehaviour
    {
        [SerializeField]
        private string _levelFolder = @"Level\test_level";

        [SerializeField]
        private GameObject _dirtBlockPrefab;

        [SerializeField]
        private GameObject _stoneBlockPrefab;

        [SerializeField]
        private bool _testViewCreationFromModel = true;

        [SerializeField]
        private bool _testReactiveViewCreation = true;

        [SerializeField]
        private bool _testReactiveViewRemoving = true;

        [SerializeField]
        private bool _testSave = true;

        [SerializeField]
        private bool _testLoad = true;

        [SerializeField]
        private Vector3Int _dirtFrom = new Vector3Int(-50, 1, -50);

        [SerializeField]
        private Vector3Int _dirtTo = new Vector3Int(50, 1, 50);

        [SerializeField]
        private Vector3Int _stoneFrom = new Vector3Int(-50, 0, -50);

        [SerializeField]
        private Vector3Int _stoneTo = new Vector3Int(50, 0, 50);

        private LevelAPI _level;
        private LevelViewBuilder _levelViewBuilder;

        private void Start()
        {
            if (!LevelStorage.Instance) {
                throw new Exception($"Missing level storage on scene");
            }
            // Получаем объект уровня
            _level = LevelStorage.Instance.API;

            StartTest();
        }

        /// <summary>
        /// Простая проверочка, что заполнены все нужные поля.
        /// </summary>
        private void CheckSelf()
        {
            Assert.IsNotNull(_dirtBlockPrefab);
        }

        private void StartTest()
        {
            if (_testViewCreationFromModel) {
                CreateModelEnvironment();
            }
            if (_testLoad) {
                Load();
            }
            CreateViewEnvironment();
            if (_testReactiveViewCreation) {
                CreateAdditionalModelPart();
            }
            if (_testReactiveViewRemoving) {
                RemoveAdditionalModelPart();
            }
            if (_testSave) {
                PrepareFileStorage();
                Save();
            }
        }

        /// <summary>
        /// Создаёт необходимое для функционирования модели окружение.
        /// </summary>
        private void CreateModelEnvironment()
        {
            // Добавляем описание блоков
            var blockProto = _level.BlockProtoCollection.Add(new BlockProtoSettings() {
                name = "test_block_1",
                formFactor = "create_test",
                layerTag = "blocks",
                lockXZ = true,
                size = Vector3Int.one
            });
            // Описание слоя даннных
            DataLayerSettings dlSettings = new() {
                tag = "blocks",
                chunkSize = Vector3Int.one * 8,
                layerType = LayerType.BlockLayer
            };
            // Добавляем описание сетки
            var gridSettings = _level.GridSettingsCollection.Add(new GridSettingsCreateParams() {
                name = "Some default test simple block grid",
                cellSize = Vector3.one,
                chunkSize = Vector3Int.one * 8, // TODO Схуяли здесь ещё один размер чанка. Надо уже определиться уровень хранения.
                formFactor = "create_test", // TODO Собна мы на блок навесили и форм фактор и леер таг. А нах они оба нужны?
                layers = new() { dlSettings }
            });
            // Добавляем сетку (инстанцию)
            var grid = _level.GridStatesCollection.Add(gridSettings.Key);
            // Добавляем блоки пола
            BlockData blockFloor = new BlockData((ushort)blockProto.Key, 0);
            for (int x = _dirtFrom.x; x <= _dirtTo.x; x++) {
                for (int y = _dirtFrom.y; y <= _dirtTo.y; y++) {
                    for (int z = _dirtFrom.z; z <= _dirtTo.z; z++) {
                        grid.SetBlock<BlockData, Vector3Int>("blocks", new Vector3Int(x, y, z), blockFloor);
                    }
                }
            }
        }

        private void CreateAdditionalModelPart()
        {
            // Добавляем описание блоков
            var blockProto = _level.BlockProtoCollection.Add(new BlockProtoSettings() {
                name = "test_block_2",
                formFactor = "create_test",
                layerTag = "blocks",
                lockXZ = true,
                size = Vector3Int.one
            });
            // Описание слоя даннных
            DataLayerSettings dlSettings = new() {
                tag = "blocks",
                chunkSize = Vector3Int.one * 8,
                layerType = LayerType.BlockLayer
            };
            // Добавляем описание сетки
            var gridSettings = _level.GridSettingsCollection.Add(new GridSettingsCreateParams() {
                name = "Additional block grid",
                cellSize = Vector3.one,
                chunkSize = Vector3Int.one * 8, // TODO Схуяли здесь ещё один размер чанка. Надо уже определиться уровень хранения.
                formFactor = "create_test", // TODO Собна мы на блок навесили и форм фактор и леер таг. А нах они оба нужны?
                layers = new() { dlSettings }
            });
            // Добавляем сетку (инстанцию)
            var grid = _level.GridStatesCollection.Add(gridSettings.Key);
            // Добавляем блоки сверху пола
            // Добавляем блоки пола
            BlockData blockFloor = new BlockData((ushort)blockProto.Key, 0);
            for (int x = _stoneFrom.x; x <= _stoneTo.x; x++) {
                for (int y = _stoneFrom.y; y <= _stoneTo.y; y++) {
                    for (int z = _stoneFrom.z; z <= _stoneTo.z; z++) {
                        grid.SetBlock<BlockData, Vector3Int>("blocks", new Vector3Int(x, y, z), blockFloor);
                    }
                }
            }
        }

        private void RemoveAdditionalModelPart()
        {
            BlockData emptyBlock = default;
            var grid = _level.GridStatesCollection.Single(x => x.GridSettings.Name == "Additional block grid");
            for (int x = _stoneFrom.x; x <= _stoneTo.x; x++) {
                for (int y = _stoneFrom.y; y <= _stoneTo.y; y++) {
                    for (int z = _stoneFrom.z; z <= _stoneTo.z; z++) {
                        grid.SetBlock("blocks", new Vector3Int(x, y, z), emptyBlock);
                    }
                }
            }
        }

        private void CreateViewEnvironment()
        {
            // Мэппинг объектов на их отображение
            ObjectSetup[] objectSetups = {
                new ObjectSetup(){
                    id = "test_block_1_default",
                    refId = "test_block_1",
                    prefab = _dirtBlockPrefab
                },
                new ObjectSetup() {
                    id = "test_block_2_default",
                    refId = "test_block_2",
                    prefab = _stoneBlockPrefab
                }
            };

            ConstructFabric constructFabric = new();
            constructFabric.AddPrefabs(objectSetups);

            _levelViewBuilder = new(constructFabric);
            _levelViewBuilder.Build(_level, transform, false);
        }

        private void Save()
        {
            _level.SaveLevel(_levelFolder);
        }

        private void Load()
        {
            _level.LoadLevel(_levelFolder);
        }

        /// <summary>
        /// Подготавливает папку для хранения сейвов.
        /// </summary>
        private void PrepareFileStorage()
        {
            if (Directory.Exists(_levelFolder)) {
                ClearFolder(_levelFolder);
            } else {
                Directory.CreateDirectory(_levelFolder);
            }
        }

        private static void ClearFolder(string folder)
        {
            if (Directory.Exists(folder)) {
                //Check
                foreach (var dir in Directory.GetDirectories(folder)) {
                    try {
                        Directory.Delete(dir, true);
                    } catch (Exception e) {
                        throw new Exception($"Can't delete folder {dir}", e);
                    }
                }

                foreach (var file in Directory.GetFiles(folder)) {
                    try {
                        File.Delete(file);
                    } catch (Exception e) {
                        throw new Exception($"Can't delete file {file}", e);
                    }
                }
            } else {
                throw new Exception($"Folder {folder} isn't exists");
            }
        }
    }
}
using System;
using System.IO;

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
        [SerializeField] private string _levelFolder = @"Level\test_level";
        [SerializeField] private GameObject _floorBlockPrefab;

        private LevelAPI _level;

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
            Assert.IsNotNull(_floorBlockPrefab);
        }

        private void StartTest()
        {
            PrepareFileStorage();
            CreateModelEnvironment();
            CreateViewEnvironment();
            CreateAdditionalModelPart();
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
            for (int x = -50; x <= 50; x++) {
                for (int z = -50; z <= 50; z++) {
                    grid.AddBlock<BlockData, Vector3Int>("blocks", new Vector3Int(x, 0, z), blockFloor);
                }
            }
        }

        void CreateAdditionalModelPart(){
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
            BlockData blockFloor = new BlockData((ushort)blockProto.Key, 0);
            for (int x = -50; x <= 50; x++) {
                for (int z = -50; z <= 50; z++) {
                    grid.AddBlock<BlockData, Vector3Int>("blocks", new Vector3Int(x, 1, z), blockFloor);
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
                    prefab = _floorBlockPrefab
                }
            };

            ConstructFabric constructFabric = new();
            constructFabric.AddPrefabs(objectSetups);

            LevelViewBuilder levelViewBuilder = new(constructFabric);
            levelViewBuilder.Build(_level, transform, false);
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
                        Directory.Delete(dir);
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
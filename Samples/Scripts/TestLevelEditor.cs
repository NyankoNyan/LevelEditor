using System.IO;
using System.Numerics;
using Level.API;
using RuntimeEditTools;
using UnityEngine;

namespace Level.Samples
{

    /// <summary>
    /// Типа всё простенько. Попробуем повыполнять всякие методы и посмотреть 
    /// состояние сцены после этого. Может это даст понимание, работает API
    /// или нет.
    /// </summary>
    public class TestLevelEditor : MonoBehaviour
    {
        [SerializeField] string _levelFolder = @"Level\test_level";

        /// <summary>
        /// Простая проверочка, что заполнены все нужные поля.
        /// </summary>
        private void CheckSelf(){

        }

        /// <summary>
        /// Создаёт необходимое для функционирования модели окружение.
        /// </summary>
        private void CreateModelEnvironment()
        {
            // Добавляем уровень
            LevelAPIFabric levelAPIFabric = new();
            var level = levelAPIFabric.Create();
            // Добавляем описание блоков
            level.BlockProtoCollection.Add(new BlockProtoSettings(){
                name = "test_block_1",
                formFactor = "create_test",
                layerTag = "blocks",
                lockXZ = true,
                size = Vector3Int.one
            });
            // Описание слоя даннных
            DataLayerSettings dlSettings = new(){
                tag = "blocks",
                chunkSize = Vector3Int.one * 8,
                layerType = LayerType.BlockLayer
            };
            // Добавляем описание сетки
            var gridSettings = level.GridSettingsCollection.Add(new GridSettingsCreateParams(){
                name = "Some default test simple block grid",
                cellSize = Vector3.one,
                chunkSize = Vector3Int.one * 8, // TODO Схуяли здесь ещё один размер чанка. Надо уже определиться уровень хранения.
                formFactor = "create_test", // TODO Собна мы на блок навесили и форм фактор и леер таг. А нах они оба нужны?
                layers = new(){dlSettings}
            });
            // Добавляем сетку (инстанцию)
            var grid = level.GridStatesCollection.Add(gridSettings.Key);
            // Добавляем блоки 
        }

        /// <summary>
        /// Подготавливает папку для хранения сейвов.
        /// </summary>
        private void PrepareFileStorage(){
            if(Directory.Exists(_levelFolder)){
                ClearFolder(_levelFolder);
            }else{
                Directory.CreateDirectory(_levelFolder);
            }
        }

        private static void ClearFolder(string folder){
            if(Directory.Exists(folder)){
                //Check
                foreach(var dir in Directory.GetDirectories(folder)){
                    try{
                        Directory.Delete(dir);
                    }catch(Exception e){
                        throw new Exception($"Can't delete folder {dir}", e);
                    }
                }

                foreach(var file in Directory.GetFiles(folder)){
                    try{
                        File.Delete(file);
                    }catch(Exception e){
                        throw new Exception($"Can't delete file {file}", e);
                    }
                }
            }else{
                throw new Exception($"Folder {folder} isn't exists");
            }
        }
    }
}
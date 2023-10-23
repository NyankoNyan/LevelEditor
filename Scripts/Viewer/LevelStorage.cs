using Level.API;
using Level.IO;
using System;
using UnityEngine;

namespace LevelView
{
    public class LevelStorage : MonoBehaviour
    {
        public static LevelStorage Instance
        {
            get {
                if (!_instance) {
                    _instance = FindObjectOfType<LevelStorage>();
                    if (!_instance) {
                        throw new Exception( $"Missing {nameof( LevelStorage )} in scene" );
                    }
                }
                return _instance;
            }
        }
        private static LevelStorage _instance;

        public ILevelAPI API => _levelAPI;

        private LevelAPIFabric _levelAPIFabric;
        private ILevelAPI _levelAPI;


        private void Awake()
        {
            _levelAPIFabric = new LevelAPIFabric();
            _levelAPI = _levelAPIFabric.Create();
        }

        public void LoadAll(ILevelLoader levelLoader)
        {
            levelLoader.LoadFullContent( _levelAPI );
        }
    }
}

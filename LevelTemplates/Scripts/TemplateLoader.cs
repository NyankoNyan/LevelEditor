using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using Level;
using Level.API;

using LevelTemplates.JSONTypes;

using Newtonsoft.Json;

using UnityEngine;

namespace LevelTemplates
{
    namespace JSONTypes
    {
#pragma warning disable IDE1006 // Стили именования

        internal class Palette
        {
            public const string TYPE = "PALETTE";

            public string version;
            public string type;
            public string name;
            public string empty_block;
            public Dictionary<string, string> blocks;
        }

        internal class Size3D
        {
            public uint x, y, z;
        }

        internal class BlockComposition
        {
            public const string TYPE = "BLOCK_COMPOSITION";

            public string version;
            public string type;
            public string name;
            public Size3D size;
            public string[][] content;
        }

#pragma warning restore IDE1006 // Стили именования
    }

    public class TemplateLoader
    {
        private const string PALETTE_NAME = "palette.json";
        private readonly string _path;
        private Palette _palette;
        private readonly List<BlockComposition> _blockCompositions = new();

        public TemplateLoader(string path)
        {
            _path = path;
            LoadFiles(_path);
        }

        private void LoadFiles(string path)
        {
            if (!Directory.Exists(path)) {
                throw new System.Exception($"Path don't exist [{path}]");
            }

            bool error = false;

            string[] files = Directory.GetFiles(path);
            try {
                LoadPallete(files);
            } catch (Exception e) {
                Debug.LogError(e);
                error = true;
            }

            HashSet<string> duplicateNames = new();
            var blRe = new Regex(@"^bl_.*\.json$", RegexOptions.IgnoreCase);
            foreach (string blFile in files.Where(s => blRe.IsMatch( Path.GetFileName(s)))) {
                try {
                    var blockComposition = JsonConvert.DeserializeObject<BlockComposition>(File.ReadAllText(blFile));

                    if (blockComposition.type != BlockComposition.TYPE) {
                        throw new Exception($"Wrong file type {blFile}");
                    }
                    if (blockComposition.version != "0.1") {
                        throw new Exception($"Unknown file version {blFile}");
                    }
                    if (string.IsNullOrWhiteSpace(blockComposition.name)) {
                        throw new Exception($"Missing name in {blFile}");
                    }
                    if (duplicateNames.Contains(blockComposition.name)) {
                        throw new Exception($"Duplicate building {blFile}");
                    }

                    _blockCompositions.Add(blockComposition);
                    duplicateNames.Add(blockComposition.name);
                } catch (Exception e) {
                    error = true;
                    Debug.LogError(new Exception($"Can't deserialize file {blFile}", e));
                }
            }

            if (error) {
                throw new Exception($"Loading tamplates failed from {path}");
            }
        }

        private void LoadPallete(string[] files)
        {
            string paletteFile;
            //read pallete
            try {
                paletteFile = files.Single(x => Path.GetFileName(x).ToLower() == PALETTE_NAME);
            } catch {
                throw new System.Exception($"Missing {PALETTE_NAME}");
            }

            try {
                var palette = JsonConvert.DeserializeObject<JSONTypes.Palette>(File.ReadAllText(paletteFile));
                _palette = palette;
            } catch (Exception e) {
                throw new Exception($"Can't deserialize file {paletteFile}", e);
            }
            if (_palette.type != Palette.TYPE) {
                throw new Exception($"Wrong file type {paletteFile}");
            }
            if (_palette.version != "0.1") {
                throw new Exception($"Unknown file version {paletteFile}");
            }
        }

        public IEnumerable<string> GetBuildings()
        {
            return _blockCompositions.Select(c => c.name);
        }

        public void LoadToLevel(
            LevelAPI level,
            GridState targetGrid,
            string targetLayer,
            string buildName,
            DiscreteAngle angle = DiscreteAngle.U0,
            Vector3Int offset = default)
        {
            BlockComposition blComp;
            try {
                blComp = _blockCompositions.Single(c => c.name == buildName);
            } catch {
                throw new Exception($"Unknown building {buildName}");
            }

            Dictionary<string, BlockProto> blockMap = new();
            foreach (var (key, name) in _palette.blocks) {
                BlockProto blockProto = level.BlockProtoCollection.FindByName(name);
                if (blockProto is null) {
                    Debug.LogError($"Missing block proto {name}");
                    continue;
                }

                blockMap.Add(key, blockProto);
            }

            bool overgrow = false;
            for (int iy = 0; iy < blComp.content.Length; iy++) {
                if (iy >= blComp.size.y) {
                    overgrow = true;
                    break;
                }
                var layer = blComp.content[iy];

                for (int iz = 0; iz < layer.Length; iz++) {
                    if (iz >= blComp.size.z) {
                        overgrow = true;
                        break;
                    }

                    string[] line = layer[iz].Split(new char[] { ' ', '\n', '\t', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                    for (int ix = 0; ix < line.Length; ix++) {
                        if (ix >= blComp.size.x) {
                            overgrow = true;
                            break;
                        }
                        string block = line[ix];

                        string[] setup = block.Split('/');
                        string key = setup[0];

                        if(key == _palette.empty_block) {
                            continue;
                        }

                        BlockProto blockProto;
                        try {
                            blockProto = blockMap[key];
                        } catch {
                            Debug.LogError($"Unknown block key {key}");
                            continue;
                        }

                        DiscreteAngle blockAngle = angle.Mult(GetBlockAngle(buildName, setup));

                        var blockData = new BlockData((ushort)blockProto.Key, blockAngle.Compress());
                        targetGrid.SetBlock(targetLayer, DiscreteGrid.RotateOffset(offset, new Vector3Int(ix, iy, (int)blComp.size.z - iz), angle), blockData);
                    }
                }
            }

            if (overgrow) {
                Debug.LogError($"Content overgrow in {buildName}");
            }
        }

        private static DiscreteAngle GetBlockAngle(string buildName, string[] setup)
        {
            DiscreteAngle blockAngle = DiscreteAngle.U0;
            if (setup.Length > 1) {
                switch (setup[1]) {
                    case "n":
                        break;

                    case "s":
                        blockAngle = DiscreteAngle.U180;
                        break;

                    case "e":
                        blockAngle = DiscreteAngle.U90;
                        break;

                    case "w":
                        blockAngle = DiscreteAngle.U270;
                        break;

                    default:
                        Debug.LogError($"Unknown block angle in {buildName}");
                        break;
                }
            }

            return blockAngle;
        }
    }
}
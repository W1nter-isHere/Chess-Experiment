using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;
using UnityEngine;

namespace IO
{
    public static class IOUtilities
    {
        public static readonly string SavesPaths = Application.persistentDataPath + "/Board Saves/";
        [CanBeNull] public static string SaveToOpen = null;

        public static Data Load(string json)
        {
            if (String.IsNullOrEmpty(json)) return null;
            var datasFromjson = JsonConvert.DeserializeObject<Data>(json);
            return datasFromjson;
        }

        [CanBeNull]
        public static Data Load()
        {
            return Load(LoadStringFromFile(SaveToOpen));
        }

        private static string LoadStringFromFile(string json)
        {
            if (json == null || !json.Any()) return null;

            try
            {
                using (var reader = new StreamReader(SavesPaths + json))
                {
                    var output = reader.ReadToEnd();
                    return output;
                }
            }
            catch (FileNotFoundException e)
            {
                return null;
            }
        }

        public static void Save()
        {
            if (!File.Exists(SavesPaths)) Directory.CreateDirectory(SavesPaths);

            var latest = SavesPaths + "/latest.json";
            var time = SavesPaths + DateTime.Now.ToString("yyyy-MM-dd-hh-mm") + ".json";
            
            if (File.Exists(latest)) File.Delete(latest);
            if (File.Exists(time)) File.Delete(time);
            
            var writer = File.CreateText(latest);
            writer.Write(SerializePiecesOnBoard(GameManagerScript.PiecesOnBoard));
            writer.Close();

            var writer2 = File.CreateText(time);
            writer2.Write(SerializePiecesOnBoard(GameManagerScript.PiecesOnBoard));
            writer2.Close();
        }

        private static string SerializePiecesOnBoard(Dictionary<(int, int), ChessPieceScript> dictionaryIn)
        {
            var data = new List<ChessPieceData>();
            foreach (var kv in dictionaryIn)
            {
                data.Add(kv.Value.SaveData() as ChessPieceData);
            }
            return JsonConvert.SerializeObject(new Data(data, DeadPileManagerScript.DeadPieces), Formatting.Indented);
        }
    }

    [Serializable]
    public class ChessPieceData : DeadChessPieceData
    {
        public int Row { get; set; }
        public int Column { get; set; }
        public uint MovesAmount { get; set; } = 0;

        public ChessPieceData(int row, int column, string type, bool white, uint movesAmount) : base(white, type)
        {
            Row = row;
            Column = column;
            MovesAmount = movesAmount;
        }

        public static ChessPieceData FromReal(ChessPieceScript toBeConverted)
        {
            return new ChessPieceData(toBeConverted.Row, toBeConverted.Column, toBeConverted.Type, toBeConverted.White, toBeConverted.MovesAmount);
        }
    }

    [Serializable]
    public class DeadChessPieceData
    {
        public bool White { get; set; }
        public string Type { get; set; }

        public DeadChessPieceData(bool white, string type)
        {
            White = white;
            Type = type;
        }
    }

    [Serializable]
    public class Data
    {
        public List<ChessPieceData> AlivePieces { get; set; }
        public List<DeadChessPieceData> DeadPieces { get; set; }

        public Data(List<ChessPieceData> alivePieces, List<DeadChessPieceData> deadPieces)
        {
            AlivePieces = alivePieces;
            DeadPieces = deadPieces;
        }
    }
}
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

        public static List<ChessPieceData> Load(string json)
        {
            if (json == null || !json.Any()) return null;
            var datasFromjson = JsonConvert.DeserializeObject<List<ChessPieceData>>(json);
            return datasFromjson;
        }

        [CanBeNull]
        public static List<ChessPieceData> Load()
        {
            return Load(LoadStringFromFile(SaveToOpen));
        }

        public static string LoadStringFromFile(string json)
        {
            if (json == null || !json.Any()) return null;

            try
            {
                var reader = new StreamReader(SavesPaths + json);
                return reader.ReadToEnd();
            }
            catch (FileNotFoundException e)
            {
                return null;
            }
        }

        public static void Save()
        {
            if (!File.Exists(SavesPaths)) Directory.CreateDirectory(SavesPaths);
            var writer = File.CreateText(SavesPaths + PlayerPrefs.GetInt("LastSavedChess") + ".json");
            writer.Write(SerializePiecesOnBoard(GameManagerScript.PiecesOnBoard));
            writer.Close();
            PlayerPrefs.Save();
        }

        private static string SerializePiecesOnBoard(Dictionary<(int, int), ChessPieceScript> dictionaryIn)
        {
            var data = new List<ChessPieceData>();
            foreach (var kv in dictionaryIn)
            {
                data.Add(kv.Value.SaveData());
            }

            return JsonConvert.SerializeObject(data, Formatting.Indented);
        }
    }

    [Serializable]
    public class ChessPieceData
    {
        public int Row { get; set; }
        public int Column { get; set; }

        public string Type { get; set; }
        public bool White { get; set; }
        public bool Alive { get; set; }
        public uint MovesAmount { get; set; } = 0;

        public ChessPieceData(int row, int column, string type, bool white, uint movesAmount)
        {
            Row = row;
            Column = column;
            Type = type;
            White = white;
            MovesAmount = movesAmount;
        }

        public static ChessPieceData FromReal(ChessPieceScript toBeConverted)
        {
            return new ChessPieceData(toBeConverted.Row, toBeConverted.Column, toBeConverted.Type, toBeConverted.White,
                toBeConverted.MovesAmount);
        }
    }
}
﻿using System;
using System.IO;
using Newtonsoft.Json;

namespace CLI.IO
{
    public class JsonInterfacer<T> where T : new()
    {
        private const bool AutoAppendExtension = true;
        private const string Extension = ".json";

        private readonly string STORAGE_DIR = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config");
        private readonly string FILE_NAME;
        private string FILE_DIR => Path.Combine(STORAGE_DIR, FILE_NAME);

        [JsonIgnore]
        public T Data;

        public JsonInterfacer(string fileName, Action<string> onDirectoryInitiated = null)
        {
            // Create Storage Directory if it's missing
            if (!Directory.Exists(STORAGE_DIR))
            {
                Directory.CreateDirectory(STORAGE_DIR);
            }

            if (!fileName.ToLower().EndsWith(Extension) && AutoAppendExtension)
                fileName += Extension;

            FILE_NAME = fileName;

            if (!File.Exists(FILE_DIR))
            {
                File.Create(FILE_DIR).Close();
                Data = new T();
                Save();
                onDirectoryInitiated?.Invoke(FILE_DIR);
            }
            else
                Load();
        }

        private string FileContent
        {
            get
            {
                return File.ReadAllText(FILE_DIR);
            }
            set
            {
                File.WriteAllText(FILE_DIR, value);
            }
        }

        private T FileContentAsT
        {
            get
            {
                return JsonConvert.DeserializeObject<T>(FileContent);
            }
            set
            {
                FileContent = JsonConvert.SerializeObject(value, Formatting.Indented);
            }
        }

        public void Save()
        {
            FileContentAsT = Data;
        }

        public void Load()
        {
            try
            {
                Data = FileContentAsT;
            }
            catch
            {
                Console.WriteLine($"[JsonInterfcer<{typeof(T)}>] Failed to load file at directory:\n\t - {FILE_DIR}\n\t - Initializing as new {typeof(T)}");
            }
        }

        public static implicit operator T(JsonInterfacer<T> interfacer) => interfacer.Data;
    }
}

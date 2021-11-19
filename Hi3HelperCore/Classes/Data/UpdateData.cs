﻿using System;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using Hi3Helper.Preset;

using static Hi3Helper.Logger;
using static Hi3Helper.Data.ConverterTool;

namespace Hi3Helper.Data
{
    public class UpdateData
    {
        protected internal string LocalPath;

        protected internal RemoteURLProp RemoteURL;
        protected internal class RemoteURLProp
        {
            internal string Data { get; set; }
            internal string DataDictionary { get; set; }
            internal string Event { get; set; }
            internal string EventDictionary { get; set; }
            internal string Ai { get; set; }
            internal string AiDictionary { get; set; }
        }

        public UpdateData(PresetConfigClasses i)
        {
            string bundleURL = ConfigStore.GetMirrorAddressByIndex(i, ConfigStore.DataType.AssetBundle);
            string dictionaryURL = ConfigStore.GetMirrorAddressByIndex(i, ConfigStore.DataType.DictionaryAddress);
            RemoteURL = new RemoteURLProp()
            {
                DataDictionary = $"{dictionaryURL}data/editor_compressed/PackageVersion.txt",
                Data = $"{bundleURL}data/editor_compressed/",
                EventDictionary = $"{dictionaryURL}event/editor_compressed/PackageVersion.txt",
                Event = $"{bundleURL}event/editor_compressed/",
                AiDictionary = $"{dictionaryURL}ai/editor_compressed/PackageVersion.txt",
                Ai = $"{bundleURL}ai/editor_compressed/"
            };
        }

        /*
         * dataType
         * 0    = Data
         * 1    = Event
         * 2    = Ai
         */
        public void GetDataDict(PresetConfigClasses i, byte dataType)
        {
            HttpClientTool downloader = new HttpClientTool();
            string LocalDirPath = Path.Combine(Environment.GetEnvironmentVariable("userprofile"), $"AppData\\LocalLow\\miHoYo\\{Path.GetFileName(i.ConfigRegistryLocation)}\\{(dataType > 0 ? "Resources" : "Data")}");
            string RemotePath = dataType == 1 ? RemoteURL.Event : dataType == 2 ? RemoteURL.Ai : RemoteURL.Data;
            string LocalPath;
            MemoryStream memoryData = new MemoryStream();

            downloader.DownloadStream(
                dataType == 1 ? RemoteURL.EventDictionary : dataType == 2 ? RemoteURL.AiDictionary : RemoteURL.DataDictionary,
                memoryData,
                new System.Threading.CancellationToken(),
                -1,
                -1,
                $"Fetch to buffer: {Enum.GetName(typeof(ConfigStore.DataType), dataType)} list"
                );
            

#if NETCOREAPP
            Span<string> DictData = Encoding.UTF8.GetString(memoryData.ToArray()).Split('\n');
#else
            string[] DictData = Encoding.UTF8.GetString(memoryData.ToArray()).Split('\n');
#endif

            for (ushort a = (ushort)(dataType > 0 ? 0 : 1); a < DictData.Length - 1; a++)
            {
                ConfigStore.DataProp = JsonConvert.DeserializeObject<UpdateDataProperties>(DictData[a]);
                if (FilterRegion(ConfigStore.DataProp.N, i.UsedLanguage) > 0)
                {
                    LocalPath = Path.Combine(LocalDirPath, NormalizePath($"{ConfigStore.DataProp.N}_{ConfigStore.DataProp.CRC}.unity3d"));
                    if (!File.Exists(LocalPath))
                    {
                        ConfigStore.UpdateFiles.Add(new UpdateDataProperties()
                        {
                            N = ConfigStore.DataProp.N,
                            CS = ConfigStore.DataProp.CS,
                            CRC = ConfigStore.DataProp.CRC,
                            ECS = ConfigStore.DataProp.CS,
                            HumanizeSize = SummarizeSizeSimple(ConfigStore.DataProp.CS),
                            RemotePath = $"{RemotePath}{ConfigStore.DataProp.N}_{ConfigStore.DataProp.CRC}",
                            ActualPath = LocalPath,
                            ZoneName = i.ZoneName,
                            DataType = Enum.GetName(typeof(ConfigStore.DataType), dataType)
                        });
                    }
                    else if (File.Exists(LocalPath) && new FileInfo(LocalPath).Length != ConfigStore.DataProp.CS)
                    {
                        ConfigStore.UpdateFiles.Add(new UpdateDataProperties()
                        {
                            N = ConfigStore.DataProp.N,
                            CS = ConfigStore.DataProp.CS,
                            CRC = ConfigStore.DataProp.CRC,
                            ECS = ConfigStore.DataProp.CS - new FileInfo(LocalPath).Length,
                            HumanizeSize = SummarizeSizeSimple(ConfigStore.DataProp.CS),
                            RemotePath = $"{RemotePath}{ConfigStore.DataProp.N}_{ConfigStore.DataProp.CRC}",
                            ActualPath = LocalPath,
                            ZoneName = i.ZoneName,
                            DataType = Enum.GetName(typeof(ConfigStore.DataType), dataType),
                            DownloadStatus = $"Uncompleted {100 * new FileInfo(LocalPath).Length / ConfigStore.DataProp.CS}% ({SummarizeSizeSimple(new FileInfo(LocalPath).Length)})"
                        });
                    }
                }
            }
        }

        private static string NormalizePath(string i) => Path.Combine(Path.GetDirectoryName(i), Path.GetFileName(i));

        private static byte FilterRegion(string input, string regionName)
        {
            /* return value
             * 0 -> the file is a regional file but outside user region.
             * 1 -> the file is a regional file but inside user region and downloadable.
             * 2 -> the file is not a regional file and downloadable.
             */
            foreach (string word in ConfigStore.RegionalCheckName)
                if (input.Contains(word))
                    if (input.Contains($"{word}_{regionName}"))
                        return 1;
                    else
                        return 0;

            return 2;
        }
    }
}
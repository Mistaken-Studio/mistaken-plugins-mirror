using Exiled.API.Features;
using Gamer.API;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace Gamer.Archivizer
{
    public class PluginHandler : Plugin<Config>
    {
        public override string Author => "Gamer";
        public override string Name => "Archivizer";
        public override void OnEnabled()
        {
            Exiled.Events.Handlers.Server.RestartingRound += Server_RestartingRound;
            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            Exiled.Events.Handlers.Server.RestartingRound -= Server_RestartingRound;
            base.OnDisabled();
        }

        private DateTime lastDate = DateTime.Now;
        private void Server_RestartingRound()
        {
            if (lastDate.ToString("yyyy:MM:dd") == DateTime.Now.ToString("yyyy:MM:dd"))
                return;
            MEC.Timing.CallDelayed(60, () =>
            {
                string path = Path.Combine(Paths.AppData, "SCP Secret Laboratory/LocalAdminLogs", Server.Port.ToString());
                Dictionary<string, List<string>> filesToDates = new Dictionary<string, List<string>>();
                foreach (var file in Directory.GetFiles(path))
                {
                    if (file.EndsWith(".zip"))
                        continue;
                    string name = Path.GetFileName(file);
                    if (!name.StartsWith("LocalAdmin Log"))
                    {
                        Log.Error($"UnExpected File Name !! | {name} | {file}");
                        continue;
                    }
                    string date = name.Replace("LocalAdmin Log ", "").Split(' ')[0];
                    if (date == DateTime.Now.ToString("yyyy-MM-dd"))
                        continue;
                    if (!filesToDates.ContainsKey(date))
                        filesToDates.Add(date, new List<string>());
                    filesToDates[date].Add(file);
                }

                foreach (var item in filesToDates)
                {
                    string dirPath = Path.Combine(path, item.Key);
                    if(!Directory.Exists(dirPath))
                        Directory.CreateDirectory(dirPath);
                    foreach (var file in item.Value)
                    {
                        string name = Path.GetFileName(file);
                        try
                        {
                            File.Move(file, Path.Combine(dirPath, name));
                        }
                        catch { }
                    }
                    Compress(dirPath);
                }
            }); 
        }

        private static void Compress(string path)
        {
            if (File.Exists($"{path}.zip"))
                return;
            ZipFile.CreateFromDirectory(path, $"{path}.zip");
            Directory.Delete(path, true);
        }
    }
}

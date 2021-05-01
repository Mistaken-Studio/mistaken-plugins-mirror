using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace Gamer.Diagnostics.Analizer
{
    class Program
    {
        static void Main(string[] args)
        {
            string localFile;

            string path = AppContext.BaseDirectory + "/Data/";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            string pathAnalizezd = AppContext.BaseDirectory + "/Analized/";
            if (!Directory.Exists(pathAnalizezd))
                Directory.CreateDirectory(pathAnalizezd);

            Dictionary<string, (Data data, int num)> GlobalProccesedData = new Dictionary<string, (Data data, int num)>();
            if (File.Exists($"{pathAnalizezd}/global.analized.raw.log"))
            {
                try
                {
                    GlobalProccesedData = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, (Data data, int num)>>(File.ReadAllText($"{pathAnalizezd}/global.analized.raw.log"));
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine($"Loading OldValues global");
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                }
            }
            Console.WriteLine("Scanning Data");
            foreach (var dir in Directory.GetDirectories(path))
            {
                try
                {
                    string dirName = Path.GetFileNameWithoutExtension(dir);
                    Dictionary<string, (Data data, int num)> DirectoryProccesedData = new Dictionary<string, (Data data, int num)>();
                    if (File.Exists($"{pathAnalizezd}/{dirName}.analized.raw.log"))
                    {
                        try
                        {
                            DirectoryProccesedData = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, (Data data, int num)>>(File.ReadAllText($"{pathAnalizezd}/{dirName}.analized.raw.log"));
                        }
                        catch (System.Exception ex)
                        {
                            Console.WriteLine($"Loading OldValues {dirName}");
                            Console.WriteLine(ex.Message);
                            Console.WriteLine(ex.StackTrace);
                        }
                    }
                    if (!Directory.Exists(dir.Replace(path, pathAnalizezd)))
                        Directory.CreateDirectory(dir.Replace(path, pathAnalizezd));
                    Console.WriteLine($"Found {dirName}, analizing...");
                    foreach (var file in Directory.GetFiles(dir))
                    {
                        try
                        {
                            if (file.EndsWith(".log"))
                            {
                                string[] content = File.ReadAllLines(file);
                                string name = Path.GetFileNameWithoutExtension(file);
                                if (name.StartsWith("error"))
                                    continue;
                                Console.WriteLine($"Analizing {name}...");
                                string[] fileDate = name.Split('-');
                                Dictionary<string, Data> ProccesedData;
                                try
                                {
                                    ProccesedData = AnalizeContent(content, fileDate);
                                }
                                catch (System.Exception ex)
                                {
                                    Console.WriteLine(ex.Message);
                                    Console.WriteLine(ex.StackTrace);
                                    continue;
                                }

                                SumData(DirectoryProccesedData, ProccesedData);

                                var sortedProccesedData = ProccesedData.OrderByDescending(i => i.Value.Max).OrderBy(i => i.Key.Split('.').Last());
                                localFile = $"{pathAnalizezd}/{dirName}/{Path.GetFileNameWithoutExtension(file)}_{name}.analized.log";
                                File.WriteAllLines(localFile, sortedProccesedData.Select(i => $"[{i.Key}] Avg: {i.Value.Avg} | Called: {i.Value.Calls} | AvgCalled {i.Value.AvgCallsPerMinute}/minute | AvgTime: {i.Value.AvgCallsPerMinute * i.Value.Avg}ms/minute | Min: {i.Value.Min} | Max: {i.Value.Max}".Replace(",", ".")));
                                File.WriteAllText(localFile + ".raw.log", Newtonsoft.Json.JsonConvert.SerializeObject(ProccesedData));
                                Console.WriteLine($"Analized {name}");
                            }
                            else if (file.EndsWith(".zip"))
                            {
                                var archive = ZipFile.OpenRead(file);
                                foreach (var entry in archive.Entries)
                                {
                                    try
                                    {
                                        var stream = entry.Open();
                                        byte[] buffer = new byte[int.MaxValue / 2];
                                        string text = Encoding.UTF8.GetString(buffer, 0, stream.Read(buffer));
                                        stream.Close();

                                        string[] content = text.Split('\n');
                                        string name = Path.GetFileNameWithoutExtension(entry.Name);

                                        if (name.StartsWith("error"))
                                            continue;
                                        string[] fileDate = name.Split('-');
                                        Dictionary<string, Data> ProccesedData;
                                        bool analized = Path.GetFileName(name).EndsWith(".analized.raw.log") || Path.GetFileName(name).EndsWith(".analized.log");
                                        if (analized)
                                        {
                                            Console.WriteLine($"Copping Data from {Path.GetFileNameWithoutExtension(file)}.{name}...");
                                            try
                                            {
                                                ProccesedData = JsonConvert.DeserializeObject<Dictionary<string, Data>>(text);
                                            }
                                            catch (System.Exception ex)
                                            {
                                                Console.WriteLine(ex.Message);
                                                Console.WriteLine(ex.StackTrace);
                                                continue;
                                            }

                                            SumData(DirectoryProccesedData, ProccesedData);
                                        }
                                        else
                                        {
                                            Console.WriteLine($"Analizing {Path.GetFileNameWithoutExtension(file)}.{name}...");
                                            try
                                            {
                                                ProccesedData = AnalizeContent(content, fileDate);
                                            }
                                            catch (System.Exception ex)
                                            {
                                                Console.WriteLine(ex.Message);
                                                Console.WriteLine(ex.StackTrace);
                                                continue;
                                            }

                                            SumData(DirectoryProccesedData, ProccesedData);
                                        }


                                        var sortedProccesedData = ProccesedData.OrderByDescending(i => i.Value.Max).OrderBy(i => i.Key.Split('.').Last());
                                        localFile = $"{pathAnalizezd}/{dirName}/{Path.GetFileNameWithoutExtension(file)}_{name}.analized";
                                        File.WriteAllLines(localFile + ".log", sortedProccesedData.Select(i => $"[{i.Key}] Avg: {i.Value.Avg} | Called: {i.Value.Calls} | AvgCalled {i.Value.AvgCallsPerMinute}/minute | AvgTime: {i.Value.AvgCallsPerMinute * i.Value.Avg}ms/minute | Min: {i.Value.Min} | Max: {i.Value.Max}".Replace(",", ".")));
                                        File.WriteAllText(localFile + ".raw.log", Newtonsoft.Json.JsonConvert.SerializeObject(ProccesedData));
                                        if (analized)
                                            Console.WriteLine($"Copped Data from {Path.GetFileNameWithoutExtension(file)}.{name}");
                                        else
                                            Console.WriteLine($"Analized {Path.GetFileNameWithoutExtension(file)}.{name}");
                                    }
                                    catch (System.Exception ex)
                                    {
                                        Console.WriteLine(ex.Message);
                                        Console.WriteLine(ex.StackTrace);
                                    }
                                }
                                archive.Dispose();
                            }
                            else
                                continue;
                        }
                        catch (System.Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                            Console.WriteLine(ex.StackTrace);
                        }
                    }

                    SumData(GlobalProccesedData, DirectoryProccesedData);

                    var sortedDirectoryProccesedData = DirectoryProccesedData.OrderByDescending(i => i.Value.data.Max).OrderBy(i => i.Key.Split('.').Last());
                    localFile = $"{pathAnalizezd}/{dirName}.analized";
                    File.WriteAllLines(localFile + ".log", sortedDirectoryProccesedData.Select(i => $"[{i.Key}] Avg: {i.Value.data.Avg / i.Value.num} | Called: {i.Value.data.Calls} | AvgCallsPerHour: {i.Value.data.Calls / i.Value.num} | AvgCalled {i.Value.data.AvgCallsPerMinute / i.Value.num}/minute | AvgTime: {(i.Value.data.AvgCallsPerMinute / i.Value.num) * (i.Value.data.Avg / i.Value.num)}ms/minute | Min: {i.Value.data.Min} | Max: {i.Value.data.Max}".Replace(",", ".")));
                    File.WriteAllText(localFile + ".raw.log", Newtonsoft.Json.JsonConvert.SerializeObject(DirectoryProccesedData));
                    Console.WriteLine($"Analized {dirName}");
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                }
            }
            List<string> modules = new List<string>();
            List<string> handlers = new List<string>();
            Dictionary<string, (Data data, int num)> modulesData = new Dictionary<string, (Data data, int num)>();
            Dictionary<string, (Data data, int num)> handlersData = new Dictionary<string, (Data data, int num)>();
            foreach (var item in GlobalProccesedData)
            {
                string[] name = item.Key.Split('.');
                if (!modules.Contains(name[0]))
                    modules.Add(name[0]);
                if (!handlers.Contains(name[1]))
                    handlers.Add(name[1]);
            }
            var GlobalProccesedDataCategoryModule = new Dictionary<string, (Data data, int num)>(GlobalProccesedData.OrderBy(i => i.Value.data.Max));
            var GlobalProccesedDataCategoryHandler = new Dictionary<string, (Data data, int num)>(GlobalProccesedData.OrderBy(i => i.Value.data.Max));
            foreach (var item in modules)
            {
                modulesData.Add(item, (new Data() { Min = 99999999 }, 0));
                foreach (var data in GlobalProccesedData)
                {
                    if (data.Key.Split('.')[0] == item)
                    {
                        modulesData[item] = (
                            new Data()
                            {
                                Avg = modulesData[item].data.Avg + data.Value.data.Avg,
                                Calls = modulesData[item].data.Calls + data.Value.data.Calls,
                                AvgCallsPerMinute = modulesData[item].data.AvgCallsPerMinute + data.Value.data.AvgCallsPerMinute,
                                Min = modulesData[item].data.Min > data.Value.data.Min ? data.Value.data.Min : modulesData[item].data.Min,
                                Max = modulesData[item].data.Max > data.Value.data.Max ? modulesData[item].data.Max : data.Value.data.Max,
                            }, 
                            modulesData[item].num + data.Value.num
                        );
                    }
                }
                GlobalProccesedDataCategoryModule.Add(item + ".ALL", modulesData[item]);
            }
            foreach (var item in handlers)
            {
                handlersData.Add(item, (new Data() { Min = 99999999 }, 0));
                foreach (var data in GlobalProccesedData)
                {
                    if (data.Key.Split('.')[1] == item)
                    {
                        handlersData[item] = (
                            new Data()
                            {
                                Avg = handlersData[item].data.Avg + data.Value.data.Avg,
                                Calls = handlersData[item].data.Calls + data.Value.data.Calls,
                                AvgCallsPerMinute = handlersData[item].data.AvgCallsPerMinute + data.Value.data.AvgCallsPerMinute,
                                Min = handlersData[item].data.Min > data.Value.data.Min ? data.Value.data.Min : handlersData[item].data.Min,
                                Max = handlersData[item].data.Max > data.Value.data.Max ? handlersData[item].data.Max : data.Value.data.Max,
                            },
                            handlersData[item].num + data.Value.num
                        );
                    }
                }
                GlobalProccesedDataCategoryHandler.Add("GLOBAL." + item, handlersData[item]);
            }
            var sortedGlobalProccesedDataCategoryModule = GlobalProccesedDataCategoryModule.Reverse().OrderBy(i => i.Key.Split('.').First());
            var sortedGlobalProccesedDataCategoryHandler = GlobalProccesedDataCategoryHandler.Reverse().OrderBy(i => i.Key.Split('.').Last());
            var sortedGlobalProccesedData = GlobalProccesedData.OrderByDescending(i => i.Value.data.Max).OrderBy(i => i.Key.Split('.').Last());
            localFile = $"{pathAnalizezd}/global.analized";
            Func<KeyValuePair<string, (Data data, int num)>, string> Selector = (i) => $"[{i.Key}] Avg: {i.Value.data.Avg / i.Value.num} | Called: {i.Value.data.Calls} | AvgCallsPerHour: {i.Value.data.Calls / i.Value.num} | AvgCalled {i.Value.data.AvgCallsPerMinute / i.Value.num}/minute | AvgTime: {(i.Value.data.AvgCallsPerMinute / i.Value.num) * (i.Value.data.Avg / i.Value.num)}ms/minute | Min: {i.Value.data.Min} | Max: {i.Value.data.Max}".Replace(",", ".");
            File.WriteAllLines(localFile + ".log", sortedGlobalProccesedData.Select(Selector));
            File.WriteAllLines(localFile + ".ordered.max.log", sortedGlobalProccesedData.OrderByDescending(i => i.Value.data.Max).Select(Selector));
            File.WriteAllLines(localFile + ".ordered.calls.log", sortedGlobalProccesedData.OrderByDescending(i => i.Value.data.Calls).Select(Selector));
            File.WriteAllLines(localFile + ".ordered.avgcalls.log", sortedGlobalProccesedData.OrderByDescending(i => new Datas(i.Value.data, i.Value.num).AvgCallsPerHour).Select(Selector));
            File.WriteAllLines(localFile + ".ordered.avgTime.log", sortedGlobalProccesedData.OrderByDescending(i => new Datas(i.Value.data, i.Value.num).AvgTime).Select(Selector));

            File.WriteAllLines(localFile + ".category.module.log", sortedGlobalProccesedDataCategoryModule.Select(Selector));
            File.WriteAllLines(localFile + ".category.handler.log", sortedGlobalProccesedDataCategoryHandler.Select(Selector));

            File.WriteAllText(localFile + ".raw.log", Newtonsoft.Json.JsonConvert.SerializeObject(GlobalProccesedData));

            Console.WriteLine($"Scan completed");
        }

        private static Dictionary<string, Data> AnalizeContent(string[] lines, string[] fileDate)
        {
            Dictionary<string, List<(float Took, DateTime Time)>> times = new Dictionary<string, List<(float Took, DateTime Time)>>();
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;
                string[] data = line.Replace("[", "").Split(']');
                string[] date = data[0].Split(':');
                var time = new DateTime(int.Parse(fileDate[0]), int.Parse(fileDate[1]), int.Parse(fileDate[2].Split('_')[0]), int.Parse(date[0]), int.Parse(date[1]), int.Parse(date[2].Split('.')[0]), int.Parse(date[2].Split('.')[1]));
                string executor = string.Join(".", data[1].Trim().Replace(" ", "").Split(":"));
                data[2] = data[2].Replace(".", ",");
                float TimeTook = float.Parse(data[2]);
                if (!times.ContainsKey(executor))
                    times.Add(executor, new List<(float Took, DateTime Time)>());
                times[executor].Add((TimeTook, time));
            }
            Dictionary<string, Data> ProccesedData = new Dictionary<string, Data>();
            foreach (var time in times)
            {
                float min = float.MaxValue;
                float max = 0;
                float avg = 0;
                Dictionary<string, int> calls = new Dictionary<string, int>();
                foreach (var item in time.Value)
                {
                    avg += item.Took;
                    if (max < item.Took)
                        max = item.Took;
                    if (min > item.Took)
                        min = item.Took;
                    string stringTime = item.Time.ToString("yyyy-MM-dd HH-mm");
                    if (!calls.ContainsKey(stringTime))
                        calls.Add(stringTime, 0);
                    calls[stringTime]++;
                }
                float avgCalls = 0;
                foreach (var item in calls)
                    avgCalls += item.Value;
                avgCalls /= calls.Values.Count;
                avg /= time.Value.Count;
                var info = (avg, time.Value.Count, min, max, avgCalls);
                ProccesedData.Add(time.Key, new Data(info));
            }

            return ProccesedData;
        }
    
        private static void SumData(Dictionary<string, (Data data, int num)> input, Dictionary<string, Data> data)
        {
            foreach (var _item in data)
            {
                if (input.ContainsKey(_item.Key))
                {
                    var item = input[_item.Key];
                    input[_item.Key] =
                        (
                            new Data((
                                item.data.Avg + _item.Value.Avg,
                                item.data.Calls + _item.Value.Calls,
                                item.data.Min > _item.Value.Min ? _item.Value.Min : item.data.Min,
                                item.data.Max < _item.Value.Max ? _item.Value.Max : item.data.Max,
                                item.data.AvgCallsPerMinute + _item.Value.AvgCallsPerMinute
                            )),
                            item.num + 1
                        );
                }
                else
                    input.Add(_item.Key, (_item.Value, 1));
            }
        }

        private static void SumData(Dictionary<string, (Data data, int num)> input, Dictionary<string, (Data data, int num)> data)
        {
            foreach (var _item in data)
            {
                if (input.ContainsKey(_item.Key))
                {
                    var item = input[_item.Key];
                    input[_item.Key] =
                        (
                            new Data((
                                item.data.Avg + _item.Value.data.Avg,
                                item.data.Calls + _item.Value.data.Calls,
                                item.data.Min > _item.Value.data.Min ? _item.Value.data.Min : item.data.Min,
                                item.data.Max < _item.Value.data.Max ? _item.Value.data.Max : item.data.Max,
                                item.data.AvgCallsPerMinute + _item.Value.data.AvgCallsPerMinute
                            )),
                            item.num + item.num
                        );
                }
                else
                    input.Add(_item.Key, _item.Value);
            }
        }

        private static Dictionary<string, Datas> Convert(Dictionary<string, (Data data, int num)> input)
        {
            var tor = new Dictionary<string, Datas>(); 
            foreach (var item in input)
                tor.Add(item.Key, new Datas(item.Value.data, item.Value.num));
            return tor;
        }

        public class Data
        {
            public float Avg;
            public int Calls;
            public float Min;
            public float Max;
            public float AvgCallsPerMinute;

            public Data() { }
            public Data((float Avg, int Calls, float Min, float Max, float AvgCallsPerMinute) info)
            {
                Avg = info.Avg;
                Calls = info.Calls;
                Min = info.Min;
                Max = info.Max;
                AvgCallsPerMinute = info.AvgCallsPerMinute;
            }
        }

        public class Datas
        {
            public float Avg;
            public int Calls;
            public float AvgTime;
            public float Min;
            public float Max;
            public float AvgCallsPerMinute;
            public float AvgCallsPerHour;

            public Datas(Data data, int count)
            {
                Avg = data.Avg / count;
                Calls = data.Calls;
                Min = data.Min;
                Max = data.Max;
                AvgCallsPerMinute = data.AvgCallsPerMinute / count;
                AvgCallsPerHour = data.Calls / count;
                AvgTime = AvgCallsPerMinute * Avg;
            }
        }
    }
}

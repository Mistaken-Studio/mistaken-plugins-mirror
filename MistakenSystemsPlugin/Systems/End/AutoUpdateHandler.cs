using Exiled.API.Features;
using Gamer.Diagnostics;
using Gamer.Utilities;
using MistakenSocket.Client.SL;
using MistakenSocket.Shared;
using MistakenSocket.Shared.API;
using MistakenSocket.Shared.ClientToCentral;
using Octokit;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gamer.Mistaken.Systems.End
{
    internal class AutoUpdateHandler : Module
    {
        private static __Log Log;
        public AutoUpdateHandler(PluginHandler p) : base(p)
        {
            Log = base.Log;
            //this.Enabled = false;
        }
        public override string Name => "AutoUpdate";
        public override void OnEnable()
        {
            Exiled.Events.Handlers.Server.RestartingRound += this.Handle(() => Server_RestartingRound(), "RoundRestart");
            Exiled.Events.Handlers.Server.RoundStarted += this.Handle(() => Server_RoundStarted(), "RoundStart");
        }
        public override void OnDisable()
        {
            Exiled.Events.Handlers.Server.RestartingRound -= this.Handle(() => Server_RestartingRound(), "RoundRestart");
            Exiled.Events.Handlers.Server.RoundStarted -= this.Handle(() => Server_RoundStarted(), "RoundStart");
        }

        private void Server_RoundStarted()
        {
            if (RequestRestart)
            {
                ServerStatic.StopNextRound = ServerStatic.NextRoundAction.Restart;
                PlayerStats.StaticChangeLevel(true);
            }
        }

#pragma warning disable CS0649
        private struct Artifacts
        {
            public int total_count;
            public Artifact[] artifacts;
        }
        private struct Artifact
        {
            public int id;
            public string node_id;
            public string name;
            public ulong size_in_bytes;
            public string url;
            public string archive_download_url;
            public bool expired;
            public DateTime created_at;
            public DateTime expires_at;
            public DateTime updated_at;
        }
#pragma warning restore CS0649
        public static bool RequestRestart = false;

        private static readonly string VersionPath = Paths.Configs + "/PluginsVersion.txt";
        internal static async Task CheckUpdate()
        {
            if (!Gamer.Mistaken.Utilities.APILib.API.GetGithubKey(out string githubKey))
                return;
            var tokenAuth = new Credentials(githubKey);
            var github = new GitHubClient(new ProductHeaderValue("SL-Plugin"))
            {
                Credentials = tokenAuth
            };
            if (Base.PluginHandler.Config.IsExperimentalServer)
            {
                var url = $"https://api.github.com/repos/Mistaken-Studio/SL-Plugin/actions/artifacts";
                var result = await github.Connection.GetHtml(new Uri(url));
                var obj = result.Body.DeserializeJson<Artifacts>();
                Artifact artifact = obj.artifacts.FirstOrDefault();
                long max = 0;
                foreach (var item in obj.artifacts)
                {
                    if (item.expires_at.Ticks > max)
                    {
                        max = item.expires_at.Ticks;
                        artifact = item;
                    }
                }
                if (File.Exists(Paths.Plugins + "/Extracted/plugins.version.txt"))
                {
                    var fileVer = File.ReadAllText(Paths.Plugins + "/Extracted/plugins.version.txt");
                    if (fileVer == artifact.node_id)
                    {
                        Log.Debug("File Version is equal to node id");
                        return;
                    }
                    Log.Debug($"File Version missmatch | {fileVer} | {artifact.node_id}");
                }
                else
                    Log.Debug("File Version not found");
                var responseRaw = await github.Connection.GetRaw(new Uri(artifact.archive_download_url), new System.Collections.Generic.Dictionary<string, string>());
                File.WriteAllBytes(Paths.Plugins + "/Extracted/plugins.zip", responseRaw.Body);
                File.Delete(Paths.Plugins + "/Extracted/plugins.tar.gz");
                ZipFile.ExtractToDirectory(Paths.Plugins + "/Extracted/plugins.zip", Paths.Plugins + "/Extracted");
                UpdateLate();
                File.WriteAllText(Paths.Plugins + "/Extracted/plugins.version.txt", artifact.node_id);
                RequestRestart = true;
                ServerConsole.EnterCommand("rnr", out _);
            }
            else if(Base.PluginHandler.Config.IsPTBServer)
            {
                var release = (await github.Repository.Release.GetAll("Mistaken-Studio", "SL-Plugin")).OrderByDescending(i => i.PublishedAt.GetValueOrDefault().UtcDateTime.Ticks).First();
                
                if (!File.Exists(VersionPath))
                {
                    File.Create(VersionPath).Close();
                    File.WriteAllText(VersionPath, release.TagName);
                }
                else
                {
                    var version = File.ReadAllText(VersionPath);
                    if (version != release.TagName)
                    {
                        RoundLoggerSystem.RoundLogger.Log("AUTO UPDATE", "UPDATE", $"Updating from {version} to {release.TagName} ({(release.Prerelease ? "PTB" : " Normal")})");
                        Update(release, github);
                    }
                }
            }
            else
            {
                var release = await github.Repository.Release.GetLatest("Mistaken-Studio", "SL-Plugin");

                if (!File.Exists(VersionPath))
                {
                    File.Create(VersionPath).Close();
                    File.WriteAllText(VersionPath, release.TagName);
                }
                else
                {
                    var version = File.ReadAllText(VersionPath);
                    if (version != release.TagName)
                    {
                        RoundLoggerSystem.RoundLogger.Log("AUTO UPDATE", "UPDATE", $"Updating from {version} to {release.TagName}");
                        Update(release, github);
                    }
                }
            }
        }
        internal void Server_RestartingRound()
        {
            _ = CheckUpdate();
        }

        private static async void Update(Release release, GitHubClient github)
        {
            MapPlus.Broadcast("AUTO UPDATE", 10, $"Update of Mistaken.Plugins detected ({release.TagName}{(release.Prerelease ? " (PTB)" : "")})", Broadcast.BroadcastFlags.AdminChat);
            foreach (var item in release.Assets)
            {
                if (item.Name == "plugins.tar.gz")
                {
                    var responseRaw = await github.Connection.Get<byte[]>(new Uri(item.Url), new System.Collections.Generic.Dictionary<string, string>(), "application/octet-stream");
                    File.WriteAllBytes(Paths.Plugins + "/Extracted/plugins.tar.gz", responseRaw.Body);
                    UpdateLate();
                    if(!Base.PluginHandler.Config.IsPTBServer)
                        RequestServersRestart();
                    else
                    {
                        ServerStatic.StopNextRound = ServerStatic.NextRoundAction.Restart;
                    }
                    File.WriteAllText(VersionPath, release.TagName);
                }
            }
        }
        private static void RequestServersRestart()
        {
            SSL.Client.Send(MessageType.CMD_MULTI_MESSAGE, new MultiMessage
            {
                Messages = new Message[]
                {
                    new Message
                    {
                        MsgType = MessageType.CMD_REQUEST_EXECUTE,
                        MessageId = MessageIdentificator.Create(SSL.Client.MyType, ServerType.CENTRAL_SERVER),
                        Data = new RequestExecute
                        {
                            Type = ExecuteType.SL_RESTART_NEXT_ROUND,
                            Argument = ((MistakenSocket.Shared.API.ServerType)1).Serialize(false)
                        }.Serialize(false)
                    },
                    new Message
                    {
                        MsgType = MessageType.CMD_REQUEST_EXECUTE,
                        MessageId = MessageIdentificator.Create(SSL.Client.MyType, ServerType.CENTRAL_SERVER),
                        Data = new RequestExecute
                        {
                            Type = ExecuteType.SL_RESTART_NEXT_ROUND,
                            Argument = ((MistakenSocket.Shared.API.ServerType)2).Serialize(false)
                        }.Serialize(false)
                    },
                    new Message
                    {
                        MsgType = MessageType.CMD_REQUEST_EXECUTE,
                        MessageId = MessageIdentificator.Create(SSL.Client.MyType, ServerType.CENTRAL_SERVER),
                        Data = new RequestExecute
                        {
                            Type = ExecuteType.SL_RESTART_NEXT_ROUND,
                            Argument = ((MistakenSocket.Shared.API.ServerType)3).Serialize(false)
                        }.Serialize(false)
                    },
                    new Message
                    {
                        MsgType = MessageType.CMD_REQUEST_EXECUTE,
                        MessageId = MessageIdentificator.Create(SSL.Client.MyType, ServerType.CENTRAL_SERVER),
                        Data = new RequestExecute
                        {
                            Type = ExecuteType.SL_RESTART_NEXT_ROUND,
                            Argument = ((MistakenSocket.Shared.API.ServerType)4).Serialize(false)
                        }.Serialize(false)
                    }
                }
            });
        }

        private static void UpdateLate()
        {
            string sourceDirectory = Paths.Plugins + "/Extracted";
            using (var inputStream = File.OpenRead(sourceDirectory + "/plugins.tar.gz"))
            {
                using (var outputStream = File.Create(sourceDirectory + "/plugins.tar"))
                {
                    using (GZipStream decompresionStream = new GZipStream(inputStream, CompressionMode.Decompress))
                    {
                        decompresionStream.CopyTo(outputStream);
                    }
                }
            }
            if (Directory.Exists(sourceDirectory + "/Plugins"))
                Directory.Delete(sourceDirectory + "/Plugins", true);
            using (var stream = File.OpenRead(sourceDirectory + "/plugins.tar"))
                ExtractTar(stream, sourceDirectory);
            foreach (var item in Directory.GetFiles(sourceDirectory + "/Plugins"))
                File.Copy(item, Paths.Plugins + "/" + Path.GetFileName(item), true);
            foreach (var item in Directory.GetFiles(sourceDirectory + "/Plugins/dependencies"))
                File.Copy(item, Paths.Dependencies + "/" + Path.GetFileName(item), true);

        }

        public static void ExtractTar(Stream stream, string outputDir)
        {
            var buffer = new byte[100];
            while (true)
            {
                stream.Read(buffer, 0, 100);
                var name = Encoding.ASCII.GetString(buffer).Trim('\0');
                if (string.IsNullOrWhiteSpace(name))
                    break;
                stream.Seek(24, SeekOrigin.Current);
                stream.Read(buffer, 0, 12);
                var size = Convert.ToInt64(Encoding.UTF8.GetString(buffer, 0, 12).Trim('\0').Trim(), 8);

                stream.Seek(376L, SeekOrigin.Current);

                var output = Path.Combine(outputDir, name);
                if (!Directory.Exists(Path.GetDirectoryName(output)))
                    Directory.CreateDirectory(Path.GetDirectoryName(output));
                if (!name.Equals("./", StringComparison.InvariantCulture))
                {
                    try
                    {
                        using (var str = File.Open(output, System.IO.FileMode.OpenOrCreate, FileAccess.Write))
                        {
                            var buf = new byte[size];
                            stream.Read(buf, 0, buf.Length);
                            str.Write(buf, 0, buf.Length);
                        }
                    }
                    catch { }
                }

                var pos = stream.Position;

                var offset = 512 - (pos % 512);
                if (offset == 512)
                    offset = 0;

                stream.Seek(offset, SeekOrigin.Current);
            }
        }
    }
}

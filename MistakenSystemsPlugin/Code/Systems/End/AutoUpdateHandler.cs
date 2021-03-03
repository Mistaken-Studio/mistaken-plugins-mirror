using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Gamer.Utilities;
using Grenades;
using MEC;
using Mirror;
using MistakenSocket.Client.SL;
using MistakenSocket.Shared.SocketAdmin;
using Octokit;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using UnityEngine;
using Gamer.Diagnostics;
using MistakenSocket.Shared.CentralToSL;
using MistakenSocket.Shared.API;
using MistakenSocket.Shared.ClientToCentral;
using MistakenSocket.Shared;

namespace Gamer.Mistaken.Systems.End
{
    internal class AutoUpdateHandler : Module
    {
        public AutoUpdateHandler(PluginHandler p) : base(p)
        {
        }
        public override string Name => "AutoUpdate";
        public override void OnEnable()
        {
            Exiled.Events.Handlers.Server.RestartingRound += this.Handle(() => Server_RestartingRound(), "RoundRestart");
        }
        public override void OnDisable()
        {
            Exiled.Events.Handlers.Server.RestartingRound -= this.Handle(() => Server_RestartingRound(), "RoundRestart");
        }

        private readonly static string VersionPath = Paths.Configs + "/PluginsVersion.txt";
        private async void Server_RestartingRound()
        {
            if (!Gamer.Mistaken.Utilities.APILib.API.GetGithubKey(out string githubKey))
                return;
            var tokenAuth = new Credentials(githubKey);
            var github = new GitHubClient(new ProductHeaderValue("SL-Plugin"));
            github.Credentials = tokenAuth;
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

        private static async void Update(Release release, GitHubClient github)
        {
            MapPlus.Broadcast("AUTO UPDATE", 10, $"Update of Mistaken.Plugins detected ({release.TagName})", Broadcast.BroadcastFlags.AdminChat);
            foreach (var item in release.Assets)
            {
                if(item.Name == "plugins.tar.gz")
                {
                    var responseRaw = await github.Connection.Get<Byte[]>(new Uri(item.Url), new System.Collections.Generic.Dictionary<string, string>(), "application/octet-stream");
                    File.WriteAllBytes(Paths.Plugins + "/Extracted/plugins.tar.gz", responseRaw.Body);
                    UpdateLate(release);
                }
            }
        }
        
        private static void UpdateLate(Release release)
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
            if (Directory.Exists(sourceDirectory + "/plugins"))
                Directory.Delete(sourceDirectory + "/plugins", true);
            using (var stream = File.OpenRead(sourceDirectory + "/plugins.tar"))
                ExtractTar(stream, sourceDirectory);
            foreach (var item in Directory.GetFiles(sourceDirectory + "/plugins"))
                File.Copy(item, Paths.Plugins + "/" + item.Split('/').Last(), true);
            foreach (var item in Directory.GetFiles(sourceDirectory + "/plugins/dependencies"))
                File.Copy(item, Paths.Dependencies + "/" + item.Split('/').Last(), true);
            for (int i = 1; i <= 4; i++)
            {
                //Log.Error("Tu powinnien być autoupdate ale ....");
                SSL.Client.Send(MessageType.CMD_REQUEST_EXECUTE, new RequestExecute
                {
                    Type = ExecuteType.SL_RESTART_NEXT_ROUND,
                    Argument = ((MistakenSocket.Shared.API.ServerType)i).Serialize(false)
                });
            }
            File.WriteAllText(VersionPath, release.TagName);
        }

        public static void ExtractTar(Stream stream, string outputDir)
        {
            var buffer = new byte[100];
            while (true)
            {
                stream.Read(buffer, 0, 100);
                var name = Encoding.ASCII.GetString(buffer).Trim('\0');
                if (System.String.IsNullOrWhiteSpace(name))
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

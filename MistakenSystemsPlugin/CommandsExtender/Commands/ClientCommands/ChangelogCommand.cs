using CommandSystem;
using Exiled.API.Features;
using Gamer.Utilities;
using Octokit;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Gamer.Mistaken.CommandsExtender.Commands
{
    [CommandSystem.CommandHandler(typeof(CommandSystem.ClientCommandHandler))]
    internal class ChangelogCommand : IBetterCommand
    {
        public override string Description => "ChangeLog";

        public override string Command => "changelog";

        public override string[] Execute(ICommandSender sender, string[] args, out bool success)
        {
            _ = ExecuteAsync(sender.GetPlayer(), args.Contains("-all"), int.Parse(args.Where(i => i.StartsWith("-s")).Select(i => i.Replace("-s", "")).FirstOrDefault() ?? "0"), int.Parse(args.Where(i => i.StartsWith("-t")).Select(i => i.Replace("-t", "")).FirstOrDefault() ?? "0"));
            success = true;
            return new string[] { "Requested" };
        }

        private async Task ExecuteAsync(Player player, bool all, int skip = 0, int take = 0)
        {
            if (!Gamer.Mistaken.Utilities.APILib.API.GetGithubKey(out string key))
                return;
            string currentVersion = File.ReadAllText(Paths.Configs + "/PluginsVersion.txt");
            var tokenAuth = new Credentials(key);
            var github = new GitHubClient(new ProductHeaderValue("Mistaken.Plugins"))
            {
                Credentials = tokenAuth
            };
            var release = await github.Repository.Release.GetLatest("grzes0071", "Mistaken.Plugins");

            List<string> tor = NorthwoodLib.Pools.ListPool<string>.Shared.Rent();
            tor.Add($"Current Version: {currentVersion}");
            if (all)
            {
                var allReleases = await github.Repository.Release.GetAll("grzes0071", "Mistaken.Plugins");
                var prevRelease = allReleases.Skip(skip).First();
                foreach (var curRelease in allReleases.Skip(1 + skip).Take(take))
                {
                    var tag = prevRelease.TagName.Split('-')[0];
                    var commits = await github.Repository.Commit.GetAll("grzes0071", "Mistaken.Plugins", new CommitRequest
                    {
                        Since = curRelease.PublishedAt,
                        Until = prevRelease.PublishedAt
                    });
                    if (prevRelease.TagName.Split('-')[0] != curRelease.TagName.Split('-')[0])
                        tor.Add($"{tag}:");
                    foreach (var commit in commits)
                    {
                        tor.Add($"- {commit.Commit.Message}");
                    }
                    prevRelease = curRelease;
                }
            }
            else
            {
                var prevRelease = (await github.Repository.Release.GetAll("grzes0071", "Mistaken.Plugins")).First(i => i.Id != release.Id);
                var commits = await github.Repository.Commit.GetAll("grzes0071", "Mistaken.Plugins", new CommitRequest
                {
                    Since = prevRelease.PublishedAt,
                    Until = release.PublishedAt
                });
                tor.Add("Changes:");
                foreach (var commit in commits)
                {
                    tor.Add($"- {commit.Commit.Message}");
                }
            }

            player.SendConsoleMessage(string.Join("\n", tor), "green");
            NorthwoodLib.Pools.ListPool<string>.Shared.Return(tor);
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Newtonsoft.Json;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace bot_banner
{
    class Program
    {
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        public static void Main(string[] args)
        {
            #region Logger configuration
            var logConfig = new LoggingConfiguration();

            logConfig.AddRule(LogLevel.Debug, LogLevel.Fatal, new ConsoleTarget("logconsole"));
            logConfig.AddRule(LogLevel.Info, LogLevel.Fatal, new FileTarget("logfile")
            {
                FileName = "${basedir}/${longdate}.log"
            });

            LogManager.Configuration = logConfig;
            #endregion

            try
            {
                Run();
            }
            catch (Exception ex)
            {
                _log.Fatal(ex.Message);
                throw;
            }
        }

        public static void Run()
        {
            var config = JsonConvert.DeserializeObject<BotBannerConfig>(
                File.ReadAllText("config.json"));

            _log.Debug("Initializing Twitch API connection");
            var twitch = new TwitchClient(config.Twitch.ClientId, config.Twitch.AccessToken,
                config.Twitch.BroadcasterId);

            twitch.Validate();

            _log.Debug($"Initializing Twitch IRC connection to {config.ChatBot.Broadcaster}");
            var irc = new IrcClient("irc.chat.twitch.tv", 6667, config.ChatBot.BotName,
                config.ChatBot.OAuth, config.ChatBot.Broadcaster);

            var running = true;
            while (running)
            {
                var fuzzyMatches = GetFuzzyMatches();

                var potentialBots = new List<string>();

                _log.Info("Getting new followers");
                var followers = twitch.GetFollowersOfUserId(config.Twitch.BroadcasterId);

                foreach (var pattern in fuzzyMatches)
                {
                    _log.Info($"Checking if any new followers contain {pattern}");
                    var matches = followers.Where(x => x.Contains(pattern));

                    if (matches.Any())
                    {
                        _log.Info($"The following users match pattern {pattern}: {string.Join(", ", matches)}");
                        potentialBots.AddRange(matches);
                    }
                }

                if (potentialBots.Any())
                {
                    _log.Info("Getting banned users");
                    var banned = twitch.GetOwnBannedUsers();

                    var unbannedPotentialBots = potentialBots.Except(banned);

                    foreach (var bot in unbannedPotentialBots)
                    {
                        irc.SendPublicChatMessage($"/ban {bot}");
                    }
                }

                Thread.Sleep(TimeSpan.FromMinutes(5));
            }
        }

        private static IEnumerable<string> GetFuzzyMatches() => File.ReadAllText("fuzzyMatches.txt").Split();
    }
}

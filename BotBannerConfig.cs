using System.Collections.Generic;

namespace bot_banner
{
    public class BotBannerConfig
    {
        public BotBannerChatBotConfig ChatBot { get; set; }
        public BotBannerTwitchConfig Twitch { get; set; }
    }

    public class BotBannerChatBotConfig
    {
        public string BotName { get; set; }
        public string Broadcaster { get; set; }
        public string OAuth { get; set; }
    }

    public class BotBannerTwitchConfig
    {
        public string ClientId { get; set; }
        public string AccessToken { get; set; }
        public string BroadcasterId { get; set; }
    }
}
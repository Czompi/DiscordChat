using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DiscordChat.Configs
{
    public class Config
    {
        [JsonPropertyName("token")]
        public string Token { get; set; } = null;

        [JsonPropertyName("channel_id")]
        public ulong ChannelId { get; set; } = 0;

        [JsonPropertyName("chat_format")]
        public ChatFormat ChatFormat { get; set; } = new();

        [JsonPropertyName("message_sync_interval")]
        public int MessageSyncInterval { get; set; } = 1;
    }
}
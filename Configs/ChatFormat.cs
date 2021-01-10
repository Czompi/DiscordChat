using System.Text.Json.Serialization;

namespace DiscordChat.Configs
{
    public class ChatFormat
    {
        [JsonPropertyName("minecraft")]
        public string Minecraft { get; set; } = "§9<§r{username}§9>§r {message}";

        [JsonPropertyName("discord")]
        public string Discord { get; set; } = "**<{username}>** {message}";
    }
}
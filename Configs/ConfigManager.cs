using DiscordChat.Settings;
using Obsidian.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace DiscordChat.Configs
{
    public class ConfigManager
    {

        #region Properties
        public string Motd { get; private set; }
        //public GlobalConfig Config { get; private set; }
        public Config Config { get; set; } = null;
        #endregion

        public ConfigManager()
        {
            ReloadConfig();
        }

        #region MultiConfig

        #region LoadConfig
        public void LoadConfig()
        {
            #region Config.json
            LoadSingleConfig(EConfigs.Config, Globals.Files.Config, JsonSerializer.Serialize(Globals.Defaults.Config), out string config);
            Config = JsonSerializer.Deserialize<Config>(config);
            #endregion

        }
        #endregion

        #region ReloadConfig
        public void ReloadConfig()
        {
            SaveConfig();
            LoadConfig();
        }
        #endregion

        #region SaveConfig
        public void SaveConfig()
        {

            #region Config.json
            SaveSingleConfig(EConfigs.Config, Globals.Files.Config, JsonSerializer.Serialize(Globals.Defaults.Config, Globals.JsonSerializerOptions), Config);
            #endregion

        }
        #endregion

        #endregion

        #region SingleConfig

        #region LoadSingleConfig
        private void LoadSingleConfig(EConfigs type, string location, string defaultConfig, out string config)
        {
            config = null;
            try
            {
                config = Globals.FileReader.ReadAllText(location);
                Globals.Logger.Log($"§7[Config/{type}]{ChatColor.Reset} Config file {ChatColor.BrightGreen}{Globals.FileReader.GetFileName(location)}{ChatColor.Reset} loaded.");
            }
            catch (Exception ex)
            {
                Globals.Logger.LogWarning($"§7[Config/{type}]{ChatColor.Reset} Config file {ChatColor.Red}{Globals.FileReader.GetFileName(location)}{ChatColor.Reset} can't be loaded.");
#if DEBUG || SNAPSHOT
                Globals.Logger.LogDebug($"§7[Config/{type.ToString()}]{ChatColor.Reset} Error: {ChatColor.Red}{ex}");
#endif
            }
        }
        #endregion

        #region ReloadSingleConfig
        private void ReloadSingleConfig(EConfigs type, string location, string defaultConfig, out string config)
        {
            SaveSingleConfig(type, location, defaultConfig, null);
            LoadSingleConfig(type, location, defaultConfig, out config);
        }
        #endregion

        #region SaveSingleConfig
        private void SaveSingleConfig(EConfigs type, string location, string defaultConfig, object? config)
        {
            var configString = config != null ? (config is string ? (String)config : JsonSerializer.Serialize(config, Globals.JsonSerializerOptions)) : defaultConfig;
            try
            {
                if (!Globals.FileReader.FileExists(location))
                {
                    Globals.Logger.Log($"§7[Config/{type}]{ChatColor.Reset} Config file §e{Globals.FileReader.GetFileName(location)}{ChatColor.Reset} doesn't exists. Creating a new one.");
                    Globals.FileWriter.WriteAllText(location, Globals.RenderColoredChatMessage(configString));
                }
                else if (config != null)
                {
                    if (configString == defaultConfig)
                    {
                        Globals.Logger.LogWarning($"§7[Config/{type}]{ChatColor.Reset} Config file save §e{Globals.FileReader.GetFileName(location)}{ChatColor.Reset} skipped. No changes were made.");
                    }
                    else
                    {
                        Globals.Logger.Log($"§7[Config/{type}]{ChatColor.Reset} Config file {ChatColor.BrightGreen}{Globals.FileReader.GetFileName(location)}{ChatColor.Reset} saved!");
                        Globals.FileWriter.WriteAllText(location, Globals.RenderColoredChatMessage(configString));
                    }
                }
            }
            catch (Exception)
            {
                Globals.Logger.LogError($"§7[Config/{type}]{ChatColor.Reset} Config file {ChatColor.Red}{location.Replace(Globals.Files.WorkingDirectory, "")}{ChatColor.Reset} cannot be created.");
            }
        }
        #endregion

        #endregion

    }
}
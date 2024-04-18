using System;
using System.IO;
using System.Text.Json;

namespace FeatExtractor
{
    internal static class Program
    {
        static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("The path to the config file is required.");
                return 1;
            }

            if (args.Length == 1)
            {
                Console.WriteLine("The path to write the feat spells and boosts is required as the second parameter (including the Public/<modname>/Stats/Generated/Data path).");
                return 1;
            }

            string configPath = args[0];
            string targetPath = args[1];
            string? nexusApiKey = args.Length > 2 ? args[2] : null;

            JsonSerializerOptions options = new JsonSerializerOptions();
            options.AllowTrailingCommas = true;
            options.PropertyNameCaseInsensitive = true;
            var config = JsonSerializer.Deserialize<Config>(File.ReadAllText(configPath), options);
            if (config == null)
            {
                Console.WriteLine("Failed to read config file.");
                return 1;
            }
            config.Nexus.ApiKey = nexusApiKey;

            // Gather modules from the game.
            GameInfo gameInfo = new GameInfo();

            foreach (var gamePath in config.GameInstallPaths)
            {
                try
                {
                    gameInfo.GatherModules(gamePath);
                }
                catch (Exception) { }
            }
            // Gather modules from Nexus Mods

            // Merge modules together (to handle patches, and try to get them in loading order.
            gameInfo.Merge();

            // Write out all the files for each mod
            gameInfo.GenerateFiles(targetPath);

            // Write out summary file that describes each module, the spells to wire, and the descriptions to use.

            return 0;
        }
    }
}
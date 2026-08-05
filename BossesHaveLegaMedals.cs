using System.Reflection;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers.Server;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Models.Spt.Tables;
using Range = SemanticVersioning.Range;
using Version = SemanticVersioning.Version;

namespace BossesHaveLegaMedals;

public sealed class ModMetadata : IModMetadata
{
    public string ModGuid { get; init; } = "com.acidphantasm.bosseshavelegamedals";
    public string Name { get; init; } = "Bosses Have Lega Medals";
    public string Author { get; init; } = "acidphantasm";
    public List<string>? Contributors { get; init; }
    public Version Version { get; init; } = new("2.1.0");
    public Range SptVersion { get; init; } = new("~4.1.0");
    public bool HasPrepatcher { get; init; } = false;
    public List<string>? Incompatibilities { get; init; }
    public Dictionary<string, Range>? ModDependencies { get; init; }
    public string? Url { get; init; }
    public string License { get; init; } = "MIT";
}

[Injectable(TypePriority = OnLoadOrder.PostLoad + 1)]
public class BossesHaveLegaMedals(
    ISptLogger<BossesHaveLegaMedals> logger,
    BotTable botTable,
    ModHelper modHelper)
    : IOnLoad
{
    private ModConfig? _modConfig;

    public Task OnLoadAsync(CancellationToken cancellationToken)
    {
        var pathToMod = modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly());
        _modConfig = modHelper.GetJsonDataFromFile<ModConfig>(pathToMod, "config.json");

        EditBots();
        logger.Success("Bosses Have Lega Medals applied.");

        return Task.CompletedTask;
    }

    private void EditBots()
    {
        var bots = botTable.Types;

        foreach (var (key, botType) in bots)
        {
            if (botType?.BotInventory?.Items?.Pockets is null)
            {
                continue;
            }

            var botName = key.ToLowerInvariant();
            var isBoss = botName.Contains("boss")
                || (_modConfig!.IncludeFollowers && botName.Contains("follower"));

            if (!isBoss)
            {
                continue;
            }

            var bossPockets = botType.BotInventory.Items.Pockets;
            var totalBossPocketValues = bossPockets.Sum(kvp => kvp.Value);
            var config = _modConfig!;

            var guess = config.LegaMedalChance / 100 * totalBossPocketValues;
            var value = Math.Round((config.LegaMedalChance / 100) * (totalBossPocketValues + guess));
            bossPockets.TryAdd(ItemTpl.BARTER_LEGA_MEDAL, value);
        }
    }
}

public class ModConfig
{
    public double LegaMedalChance { get; set; }
    public bool IncludeFollowers { get; set; }
}

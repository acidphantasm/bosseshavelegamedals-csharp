using System.Reflection;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services;

namespace BossesHaveLegaMedals;

using SPTarkov.Common.Models.Logging;
using SPTarkov.Server.Core.Helpers.Server;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Tables;

[Injectable(TypePriority = OnLoadOrder.PostLoad + 1)]
public class BossesHaveLegaMedalsOnLoad(
    BotTable botTable,
    LegaMedalsModConfig config)
    : IOnLoad
{
    public Task OnLoadAsync(CancellationToken cancellationToken)
    {
        EditBots();
        
        return Task.CompletedTask;
    }
    
    private void EditBots()
    {
        var bots = botTable.Types;

        foreach (var (key, botType) in bots)
        {
            if (botType is null)
                continue;
            
            var botName = key.ToLowerInvariant();
            var isBoss = botName.Contains("boss") || config.IncludeFollowers && botName.Contains("follower");
            
            if (!isBoss) continue;
            var bossPockets = botType.BotInventory.Items.Pockets;
            var totalBossPocketValues = bossPockets.Sum( kvp => kvp.Value);

            double value = 0;
            double guess = 0;

            guess = config.LegaMedalChance / 100 * totalBossPocketValues;
            value = Math.Round((config.LegaMedalChance / 100) * (totalBossPocketValues + guess));
            bossPockets.TryAdd(ItemTpl.BARTER_LEGA_MEDAL, value);
        }
    }
}

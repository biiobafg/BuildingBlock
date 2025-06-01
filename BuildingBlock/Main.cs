using Rocket.API;
using Rocket.API.Collections;
using Rocket.Core.Commands;
using Rocket.Core.Logging;
using Rocket.Core.Plugins;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using System;
using UnityEngine;

namespace BuildingBlock
{
    public class Main : RocketPlugin
    {
        private JsonConfig loadeddConfig;
        protected override void Load()
        {
            BarricadeManager.onDeployBarricadeRequested+=BarricadeReq;

            Level.onLevelLoaded+=(_) =>
            {
                string mapName = Level.info?.name;
                if (string.IsNullOrEmpty(mapName))
                {
                    Rocket.Core.Logging.Logger.Log("Failed to get map name???");
                    return;
                }
                //Console.WriteLine(mapName);
                JsonConfig config = JsonConfig.Load(mapName, Directory);
                if (config==null)
                {
                    loadeddConfig=new JsonConfig();
                    loadeddConfig.AffectedBarricades.Add(366);
                    loadeddConfig.BlockRegions.Add(new(Vector3.zero, 10));
                    loadeddConfig.Write(mapName, Directory);
                }
                else
                {
                    loadeddConfig=config;
                }
            };
        }
        protected override void Unload()
        {
            BarricadeManager.onDeployBarricadeRequested-=BarricadeReq;
        }

        public override TranslationList DefaultTranslations => new()
        {
            {"CANTBUILDHERE", "Cant build here" }
        };

        private void BarricadeReq(Barricade barricade,
            ItemBarricadeAsset asset,
            Transform hit,
            ref Vector3 point,
            ref float angle_x,
            ref float angle_y,
            ref float angle_z,
            ref ulong owner,
            ref ulong group,
            ref bool shouldAllow)
        {
            ushort id = asset.id;
            if (!loadeddConfig.AffectedBarricades.Contains(id))
            {
                return;
            }

            foreach (BlockRegion region in loadeddConfig.BlockRegions)
            {
                if (Vector3.Distance(region.Position, point)<=region.Distance)
                {
                    shouldAllow=false;
                    UnturnedChat.Say((CSteamID)owner, Translate("CANTBUILDHERE"), color: Color.red);
                    return;
                }
            }
        }



        [RocketCommand("setblockzone", "", AllowedCaller: AllowedCaller.Player)]
        public void execute(IRocketPlayer caller, string[] args)
        {
            if (caller is not UnturnedPlayer player)
            {
                return;
            }
            if (args.Length<1)
            {
                UnturnedChat.Say(caller, "Use /setblockzone (distance)");
                return;
            }
            if (!float.TryParse(args[0], out float distance))
            {
                UnturnedChat.Say(caller, "Invalid distance");
                return;
            }

            Vector3 pos = player.Position;
            BlockRegion region = new(pos, distance);
            loadeddConfig.BlockRegions.Add(region);
            loadeddConfig.Write(Level.info.name, Directory);

            UnturnedChat.Say(caller, $"added with distance {distance}");
        }

    }
}

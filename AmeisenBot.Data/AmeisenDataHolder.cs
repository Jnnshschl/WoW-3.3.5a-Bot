﻿using AmeisenBotUtilities;
using System.Collections.Generic;

namespace AmeisenBotData
{
    public class AmeisenDataHolder
    {
        public List<WowObject> ActiveWoWObjects { get; set; }
        public List<NetworkBot> ActiveNetworkBots { get; set; }
        public double FollowDistance { get; set; }
        public bool IsAllowedToAssistParty { get; set; }
        public bool IsAllowedToAttack { get; set; }
        public bool IsAllowedToBuff { get; set; }
        public bool IsAllowedToFollowParty { get; set; }
        public bool IsAllowedToHeal { get; set; }
        public bool IsAllowedToTank { get; set; }
        public Me Me { get; set; }
        public Unit Target { get; set; }
        public Unit Pet { get; set; }
        public bool IsAllowedToReleaseSpirit { get; set; }
        public bool IsAllowedToRevive { get; set; }
        public Settings Settings { get; set; }
        public bool IsAllowedToDoRandomEmotes { get; set; }
        public bool IsAllowedToDoOwnStuff { get; set; }
        public bool IsConnectedToDB { get; set; }
        public bool IsConnectedToServer { get; set; }
        public Queue<Unit> LootableUnits { get; set; }
        public bool IsInWorld { get; set; }
        public bool IsHealer { get; set; }
        public List<Unit> Partymembers { get; set; }

        public AmeisenDataHolder()
        {
        }
    }
}
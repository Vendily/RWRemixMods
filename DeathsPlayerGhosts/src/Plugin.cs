using BepInEx;
using BepInEx.Logging;
using System.Security.Permissions;
using System;
using System.Reflection;
using MonoMod.RuntimeDetour;
using System.Runtime.CompilerServices;
using RWCustom;
using UnityEngine;
using System.Linq;

// Allows access to private members
#pragma warning disable CS0618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618

namespace DeathPlayerGhosts;

[BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
sealed class Plugin : BaseUnityPlugin
{
    public static new ManualLogSource Logger { get; private set; }
    private const string PLUGIN_GUID = "vendily.deathplayerghosts";
    private const string PLUGIN_NAME = "Deaths are Player Ghosts";
    private const string PLUGIN_VERSION = "1.0.0";
    internal class PlayerGhostData
    {
        public float deaths;
        public float deaths_quarter;
        public float deaths_remainder;
        public float timer;
        public PlayerGhostData(int deaths)
        {
            Plugin.Logger.LogDebug("Mode: " + Options.PlayerGhostMode.Value);
            Plugin.Logger.LogDebug("Cap: " + Options.PlayerGhostCap.Value);
            Plugin.Logger.LogDebug("Timer: " + Options.PlayerGhostTimer.Value);
            Plugin.Logger.LogDebug("Override : " + Options.PlayerGhostOverride.Value);
            Plugin.Logger.LogDebug("Deaths : " + deaths);
            this.deaths = (float)deaths;
            if (deaths == 0) { this.deaths = 1f; }
            if (Options.PlayerGhostCap.Value > 0 &&
                ((Options.PlayerGhostOverride.Value) || (this.deaths > Options.PlayerGhostCap.Value)))
            {
                
                this.deaths = (float)Options.PlayerGhostCap.Value;
            }
            if ((Options.PlayerGhostMode.Value != PlayerGhostMode.Static) &&
                    ((this.deaths % 4) != 0) && (this.deaths < 4))
            {
                Plugin.Logger.LogWarning("Not enough Ghosts to trigger the sequence to begin, increasing it");
                // at least 1 ghost must spawn for the end sequence to work
                this.deaths = this.deaths * 4;
            }
            
            this.deaths_quarter = Mathf.Floor((this.deaths / 4));
            this.deaths_remainder = this.deaths - this.deaths_quarter;
            this.timer = (float)Options.PlayerGhostTimer.Value;
        }
    }
    bool init;
    static readonly ConditionalWeakTable<VoidSea.PlayerGhosts, PlayerGhostData> cwt = new();
    static PlayerGhostData GetPGData(VoidSea.PlayerGhosts pg)
    {
        return cwt.GetValue(pg, _ => new PlayerGhostData(pg.voidSea.room.game.GetStorySession.saveState.deathPersistentSaveData.deaths));
    }

    public void OnEnable()
    {
        Logger = base.Logger;
        // Add hooks here
        On.RainWorld.OnModsInit += OnModsInit;
        try
        {
            _ = new Hook(typeof(VoidSea.PlayerGhosts).GetMethod("get_IdealGhostCount",BindingFlags.Instance | BindingFlags.Public), IdealGhostCount);
            Logger.LogDebug("Created IdealGhostCount Hook");
        }
        catch(Exception e)
        {
            Logger.LogError(e);
        }
    }

    private void OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
    {
        orig(self);

        if (init) return;

        init = true;
        MachineConnector.SetRegisteredOI(PLUGIN_GUID, new Options());
        Logger.LogInfo("Mod Initialized");
    }

    public enum PlayerGhostMode
    {
        TimerAndProximity,
        TimerOnly,
        ProximityOnly,
        Static,
        DisableMod
    }
    private int IdealGhostCount(Func<VoidSea.PlayerGhosts, int> orig, VoidSea.PlayerGhosts self)
    {
        PlayerGhostData pgdata = GetPGData(self);
        switch (Options.PlayerGhostMode.Value)
        {
            case PlayerGhostMode.Static:
                return (int)pgdata.deaths;
            case PlayerGhostMode.TimerAndProximity:
                return (int)Custom.LerpMap(self.voidSea.eggScenarioTimer, 0f, pgdata.timer, 0f, pgdata.deaths_quarter + pgdata.deaths_remainder * Mathf.Pow(self.voidSea.eggProximity, 0.3f));
            case PlayerGhostMode.TimerOnly:
                return (int)Custom.LerpMap(self.voidSea.eggScenarioTimer, 0f, pgdata.timer, 0f, pgdata.deaths);
            case PlayerGhostMode.ProximityOnly:
                return (int)Mathf.Lerp(0f, pgdata.deaths, self.voidSea.eggProximity);
        }
        return orig(self);
    }
}

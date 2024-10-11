using BepInEx;
using System.Security.Permissions;
using RWCustom;
using MoreSlugcats;

// Allows access to private members
#pragma warning disable CS0618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618

namespace BiteDemCentipedes;

[BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
sealed class Plugin : BaseUnityPlugin
{
    private const string PLUGIN_GUID = "vendily.bitedemcentipedes";
    private const string PLUGIN_NAME = "Bite Dem Centipedes";
    private const string PLUGIN_VERSION = "0.1.0";
    bool init;

    public void OnEnable()
    {
        // Add hooks here
        On.RainWorld.OnModsInit += OnModsInit;
        On.Player.GrabUpdate += Player_GrabUpdate;
    }

    private void OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
    {
        orig(self);
        if (init) return;
        init = true;
        // Initialize assets, your mod config, and anything that uses RainWorld here
        Logger.LogDebug("Hello world!");
    }

    private void Player_GrabUpdate(On.Player.orig_GrabUpdate orig, Player self, bool eu)
    {
        if (!ModManager.MSC || (self.SlugCatClass != MoreSlugcatsEnums.SlugcatStatsName.Spear && self.SlugCatClass != MoreSlugcatsEnums.SlugcatStatsName.Saint))
        {
            bool _not_moving_jumping_throwing = ((self.input[0].x == 0 && self.input[0].y == 0 && !self.input[0].jmp && !self.input[0].thrw) || (ModManager.MMF && self.input[0].x == 0 && self.input[0].y == 1 && !self.input[0].jmp && !self.input[0].thrw && (self.bodyMode != Player.BodyModeIndex.ClimbingOnBeam || self.animation == Player.AnimationIndex.BeamTip || self.animation == Player.AnimationIndex.StandOnBeam))) && (self.mainBodyChunk.submersion < 0.5f || self.isRivulet);
            int active_grasp_index = -1;
            if (_not_moving_jumping_throwing)
            {
                int i = 0;
                while (active_grasp_index < 0 && i < 2 && (!ModManager.MSC || self.SlugCatClass != MoreSlugcatsEnums.SlugcatStatsName.Spear))
                {
                    if (self.grasps[i] != null && self.grasps[i].grabbed is IPlayerEdible && (self.grasps[i].grabbed as IPlayerEdible).Edible)
                    {
                        active_grasp_index = i;
                    }
                    i++;
                }
                if (active_grasp_index > -1 && self.wantToPickUp < 1 && (self.input[0].pckp || self.eatCounter <= 15) && self.Consious && Custom.DistLess(self.mainBodyChunk.pos, self.mainBodyChunk.lastPos, 3.6f))
                {
                    if (self.graphicsModule != null)
                    {
                        (self.graphicsModule as PlayerGraphics).LookAtObject(self.grasps[active_grasp_index].grabbed);
                    }
                    if (self.grasps[active_grasp_index].grabbed is Centipede && (self.grasps[active_grasp_index].grabbed as Centipede).Small && !(self.grasps[active_grasp_index].grabbed as Centipede).dead)
                    {
                        if (self.eatCounter < 1)
                        {
                            self.eatCounter = 15;
                            self.BiteEdibleObject(eu);
                        }
                    }
                }
            }
        }
        orig(self, eu);
    }

}

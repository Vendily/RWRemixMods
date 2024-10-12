using BepInEx;
using System.Security.Permissions;
using RWCustom;
using MoreSlugcats;
using UnityEngine;

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
    private const string PLUGIN_VERSION = "1.1.0";
    bool init;

    public void OnEnable()
    {
        // Add hooks here
        On.RainWorld.OnModsInit += OnModsInit;
        On.Player.GrabUpdate += Player_GrabUpdate;
        On.Player.BiteEdibleObject += Player_BiteEdibleObject;
    }

    private void OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
    {
        orig(self);
        if (init) return;
        init = true;
    }

    private void Player_GrabUpdate(On.Player.orig_GrabUpdate orig, Player self, bool eu)
    {
        bool _not_moving_jumping_throwing = ((self.input[0].x == 0 && self.input[0].y == 0 && !self.input[0].jmp && !self.input[0].thrw) || (ModManager.MMF && self.input[0].x == 0 && self.input[0].y == 1 && !self.input[0].jmp && !self.input[0].thrw && (self.bodyMode != Player.BodyModeIndex.ClimbingOnBeam || self.animation == Player.AnimationIndex.BeamTip || self.animation == Player.AnimationIndex.StandOnBeam))) && (self.mainBodyChunk.submersion < 0.5f || self.isRivulet);
        int active_grasp_index = -1;
        if (_not_moving_jumping_throwing)
        {
            int i = 0;
            while (active_grasp_index < 0 && i < 2)
            {
                if (self.grasps[i] != null && (self.grasps[i].grabbed is Centipede) && (self.grasps[i].grabbed as Centipede).Small && !(self.grasps[i].grabbed as Centipede).dead)
                {
                    active_grasp_index = i;
                    break;
                }
                i++;
            }
            if (active_grasp_index > -1 && self.wantToPickUp < 1 && (self.input[0].pckp || self.eatCounter <= 15) && self.Consious && Custom.DistLess(self.mainBodyChunk.pos, self.mainBodyChunk.lastPos, 3.6f))
            {
                if (self.graphicsModule != null)
                {
                    (self.graphicsModule as PlayerGraphics).LookAtObject(self.grasps[active_grasp_index].grabbed);
                }
                if (ModManager.MSC && self.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Saint)
                {
                    bool can_saint_ascend = (self.KarmaCap >= 9 || (self.room.game.IsArenaSession && self.room.game.GetArenaGameSession.arenaSitting.gameTypeSetup.gameType != MoreSlugcatsEnums.GameTypeID.Challenge) || (self.room.game.session is ArenaGameSession && self.room.game.GetArenaGameSession.arenaSitting.gameTypeSetup.gameType == MoreSlugcatsEnums.GameTypeID.Challenge && self.room.game.GetArenaGameSession.arenaSitting.gameTypeSetup.challengeMeta.ascended));
                    if (can_saint_ascend && self.eatCounter < 1)
                    {
                        self.room.PlaySound(SoundID.Snail_Pop, self.mainBodyChunk, loop: false, 1f, 1.5f + UnityEngine.Random.value);
                        self.eatCounter = 30;
                        self.room.AddObject(new ShockWave(self.grasps[active_grasp_index].grabbed.firstChunk.pos, 25f, 0.8f, 4));
                        for (int m = 0; m < 5; m++)
                        {
                            self.room.AddObject(new Spark(self.grasps[active_grasp_index].grabbed.firstChunk.pos, Custom.RNV() * 3f, Color.yellow, null, 25, 90));
                        }
                        (self.grasps[active_grasp_index].grabbed as Creature).Die();
                    }
                } else if (ModManager.MSC && self.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Spear)
                {
                    /*
                    PlayerGraphics.TailSpeckles tailSpecks2 = (self.graphicsModule as PlayerGraphics).tailSpecks;
                    Logger.LogInfo(PLUGIN_NAME + ": Spear Progress =" + tailSpecks2.spearProg);
                    if (tailSpecks2.spearProg > 0.95f)
                    {
                        tailSpecks2.setSpearProgress(1f);
                    }
                    if (tailSpecks2.spearProg == 1f)
                    {
                        Logger.LogInfo(PLUGIN_NAME + ": Creating Spear");
                        self.room.PlaySound(MoreSlugcatsEnums.MSCSoundID.SM_Spear_Grab, 0f, 1f, 0.5f + UnityEngine.Random.value * 1.5f);
                        self.smSpearSoundReady = false;
                        Vector2 pos = (self.graphicsModule as PlayerGraphics).tail[(int)((float)(self.graphicsModule as PlayerGraphics).tail.Length / 2f)].pos;
                        for (int k = 0; k < 4; k++)
                        {
                            Vector2 vector = Custom.DirVec(pos, self.bodyChunks[1].pos);
                            self.room.AddObject(new WaterDrip(pos + Custom.RNV() * UnityEngine.Random.value * 1.5f, Custom.RNV() * 3f * UnityEngine.Random.value + vector * Mathf.Lerp(2f, 6f, UnityEngine.Random.value), waterColor: false));
                        }
                        for (int l = 0; l < 5; l++)
                        {
                            Vector2 vector2 = Custom.RNV();
                            self.room.AddObject(new Spark(pos + vector2 * UnityEngine.Random.value * 40f, vector2 * Mathf.Lerp(4f, 30f, UnityEngine.Random.value), Color.white, null, 4, 18));
                        }
                        int spearType = tailSpecks2.spearType;
                        tailSpecks2.setSpearProgress(0f);
                        AbstractSpear abstractSpear = new AbstractSpear(self.room.world, null, self.room.GetWorldCoordinate(self.mainBodyChunk.pos), self.room.game.GetNewID(), explosive: false);
                        self.room.abstractRoom.AddEntity(abstractSpear);
                        abstractSpear.pos = self.abstractCreature.pos;
                        abstractSpear.RealizeInRoom();
                        Vector2 pos2 = self.bodyChunks[0].pos;
                        Vector2 vector3 = Custom.DirVec(self.bodyChunks[1].pos, self.bodyChunks[0].pos);
                        if (Mathf.Abs(self.bodyChunks[0].pos.y - self.bodyChunks[1].pos.y) > Mathf.Abs(self.bodyChunks[0].pos.x - self.bodyChunks[1].pos.x) && self.bodyChunks[0].pos.y > self.bodyChunks[1].pos.y)
                        {
                            pos2 += Custom.DirVec(self.bodyChunks[1].pos, self.bodyChunks[0].pos) * 5f;
                            vector3 *= -1f;
                            vector3.x += 0.4f * (float)self.flipDirection;
                            vector3.Normalize();
                        }
                        abstractSpear.realizedObject.firstChunk.HardSetPosition(pos2);
                        abstractSpear.realizedObject.firstChunk.vel = Vector2.ClampMagnitude((vector3 * 2f + Custom.RNV() * UnityEngine.Random.value) / abstractSpear.realizedObject.firstChunk.mass, 6f);
                        if (abstractSpear.type == AbstractPhysicalObject.AbstractObjectType.Spear)
                        {
                            (abstractSpear.realizedObject as Spear).Spear_makeNeedle(spearType, active: true);
                            if ((self.graphicsModule as PlayerGraphics).useJollyColor)
                            {
                                (abstractSpear.realizedObject as Spear).jollyCustomColor = PlayerGraphics.JollyColor(self.playerState.playerNumber, 2);
                            }
                        }
                        (abstractSpear.realizedObject as Spear).thrownBy = self;
                        Weapon weapon = (abstractSpear.realizedObject as Weapon);
                        Creature creature = (self.grasps[active_grasp_index].grabbed as Creature);
                        SharedPhysics.CollisionResult result = new((creature as Centipede), (creature as Centipede).HeadChunk, null, true, (creature as Centipede).HeadChunk.pos);
                        (abstractSpear.realizedObject as Spear).HitSomething(result, eu);
                        weapon.forbiddenToPlayer = 40;
                        (result.obj as Creature).SetKillTag(self.abstractCreature);
                        weapon.room.socialEventRecognizer.WeaponAttack(weapon as Spear, weapon.thrownBy, result.obj as Creature, true);
                        if (self.FoodInStomach < self.MaxFoodInStomach)
                            self.AddFood(1);
                        creature.Die();
                        (abstractSpear.realizedObject as Spear).Spear_NeedleDisconnect();
                        (abstractSpear.realizedObject as Spear).LodgeInCreature(result, eu);
                        weapon.thrownBy = null;
                    }*/
                } else if (self.FoodInStomach >= self.MaxFoodInStomach && self.eatCounter < 1)
                {
                    self.eatCounter = 15;
                    self.BiteEdibleObject(eu);
                }
            }
        }
        orig(self, eu);
    }

    private void Player_BiteEdibleObject(On.Player.orig_BiteEdibleObject orig, Player self, bool eu)
    {
        int active_grasp_index = -1;
        for (int i = 0; i < 2; i++)
        {
            if (self.grasps[i] != null && (self.grasps[i].grabbed is Centipede) && (self.grasps[i].grabbed as Centipede).Small && !(self.grasps[i].grabbed as Centipede).dead)
            {
                active_grasp_index = i;
                break;
            }
        }
        if (self.FoodInStomach >= self.MaxFoodInStomach && active_grasp_index > -1 && (!ModManager.MSC || (self.SlugCatClass != MoreSlugcatsEnums.SlugcatStatsName.Spear && self.SlugCatClass != MoreSlugcatsEnums.SlugcatStatsName.Saint)))
        {
            (self.grasps[active_grasp_index].grabbed as Creature).SetKillTag(self.abstractCreature);
            if (self.graphicsModule != null)
            {
                (self.graphicsModule as PlayerGraphics).BiteFly(active_grasp_index);
            }
            (self.grasps[active_grasp_index].grabbed as IPlayerEdible).BitByPlayer(self.grasps[active_grasp_index], eu);
        } else
        {
            orig(self, eu);
        }
    }

}

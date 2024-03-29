﻿using OWOGame;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
//using Bhaptics.Tact;

namespace TactsuitAlyx
{
    public class TactsuitVR
    {
        public TactsuitVR()
        {
            FillFeedbackList();
        }

        public bool systemInitialized = false;
        //public HapticPlayer hapticPlayer;
        
        Dictionary<FeedbackType, Feedback> feedbackMap = new Dictionary<FeedbackType, Feedback>();
        public Dictionary<String, Sensation> FeedbackMap = new Dictionary<String, Sensation>();

        public enum FeedbackType
        {
            Startup,
            ThreeHeartBeats,

            //Attacks on Player's head
            DefaultHead,
            UnarmedHead,
            GunHead,

            //Unarmed Enemies
            UnarmedBloater,
            UnarmedHeadcrab,
            UnarmedHeadcrabArmored,
            UnarmedHeadcrabBlack, //Toxic
            UnarmedHeadcrabFast, //lightning dog
            UnarmedHeadcrabRunner, 
            UnarmedFastZombie,
            UnarmedPoisonZombie,
            UnarmedZombie,
            UnarmedZombieBlind,
            UnarmedZombine,
            UnarmedAntlion,
            UnarmedAntlionGuard,
            UnarmedManhack,

            GrabbedByBarnacle,

            //Grenade/Mine
            ConcussionGrenade,
            BugBaitGrenade,
            FragGrenade,
            SpyGrenade,
            HandGrenade,
            RollerGrenade,
            RollerMine,

            //Enemies with guns
            Combine,
            CombineS,

            CombineGantry,


            MetroPolice,
            Sniper,
            Strider,
            Turret,
            FoliageTurret,
            
            //On whole body
            EnvironmentExplosion,
            EnvironmentLaser,
            EnvironmentFire,
            EnvironmentSpark,
            EnvironmentPoison,
            EnvironmentRadiation,

            //Uses location
            DamageExplosion,
            DamageLaser,
            DamageFire,
            DamageSpark,

            //Player weapon shoot
            PlayerShootPistol,
            PlayerShootShotgun,
            PlayerShootSMG,

            PlayerShootDefault,

            PlayerGrenadeLaunch,

            PlayerShootPistolLeft,
            PlayerShootShotgunLeft,
            PlayerShootSMGLeft,

            PlayerShootDefaultLeft,

            PlayerGrenadeLaunchLeft,

            FallbackPistol,
            FallbackShotgun,
            FallbackSMG,

            FallbackPistolLeft,
            FallbackShotgunLeft,
            FallbackSMGLeft,

            KickbackPistol,
            KickbackShotgun,
            KickbackSMG,

            KickbackPistolLeft,
            KickbackShotgunLeft,
            KickbackSMGLeft,

            //Special stuff
            HeartBeat,
            HeartBeatFast,

            HealthPenUse,
            HealthStationUse,
            HealthStationUseLeftArm,
            HealthStationUseRightArm,

            BackpackStoreClip,
            BackpackStoreResin,
            BackpackRetrieveClip,
            BackpackRetrieveResin,
            ItemHolderStore,
            ItemHolderRemove,

            BackpackStoreClipLeft,
            BackpackStoreResinLeft,
            BackpackRetrieveClipLeft,
            BackpackRetrieveResinLeft,
            ItemHolderStoreLeft,
            ItemHolderRemoveLeft,

            GravityGloveLockOn,
            GravityGlovePull,
            GravityGloveCatch,

            GravityGloveLockOnLeft,
            GravityGlovePullLeft,
            GravityGloveCatchLeft,

            ClipInserted,
            ChamberedRound,

            ClipInsertedLeft,
            ChamberedRoundLeft,

            Cough,
            CoughHead,

            ShockOnHandLeft,
            ShockOnHandRight,

            DefaultDamage,

            NoFeedback
        }

        struct Feedback
        {
            public Feedback(FeedbackType _feedbackType, string _prefix, int _feedbackFileCount)
            {
                feedbackType = _feedbackType;
                prefix = _prefix;
                feedbackFileCount = _feedbackFileCount;
            }
            public FeedbackType feedbackType;
            public string prefix;
            public int feedbackFileCount;
        };

        public bool HeadCrabFeedback(FeedbackType feedback)
        {
            return feedback == FeedbackType.UnarmedHeadcrab
                   || feedback == FeedbackType.UnarmedHeadcrabArmored
                   || feedback == FeedbackType.UnarmedHeadcrabBlack
                   || feedback == FeedbackType.UnarmedHeadcrabFast
                   || feedback == FeedbackType.UnarmedHeadcrabRunner;
        }

        public bool EnvironmentFeedback(FeedbackType feedback)
        {
            return feedback == FeedbackType.DefaultDamage
                   || feedback == FeedbackType.EnvironmentExplosion
                   || feedback == FeedbackType.EnvironmentFire
                   || feedback == FeedbackType.EnvironmentLaser
                   || feedback == FeedbackType.EnvironmentSpark
                   || feedback == FeedbackType.EnvironmentPoison
                   || feedback == FeedbackType.EnvironmentRadiation;
        }

        public FeedbackType GetHeadFeedbackVersion(FeedbackType feedback)
        {
            if (feedback == FeedbackType.UnarmedHeadcrab
                || feedback == FeedbackType.UnarmedHeadcrabArmored
                || feedback == FeedbackType.UnarmedHeadcrabBlack
                || feedback == FeedbackType.UnarmedHeadcrabFast
                || feedback == FeedbackType.UnarmedHeadcrabRunner
                || feedback == FeedbackType.UnarmedAntlion
                || feedback == FeedbackType.UnarmedAntlionGuard
                || feedback == FeedbackType.UnarmedFastZombie
                || feedback == FeedbackType.UnarmedPoisonZombie
                || feedback == FeedbackType.UnarmedManhack
                || feedback == FeedbackType.UnarmedZombie
                || feedback == FeedbackType.UnarmedZombieBlind
                || feedback == FeedbackType.UnarmedZombine
            )
            {
                return FeedbackType.UnarmedHead;
            }
            else if(feedback == FeedbackType.Combine
                    || feedback == FeedbackType.CombineS
                    || feedback == FeedbackType.CombineGantry
                    || feedback == FeedbackType.MetroPolice
                    || feedback == FeedbackType.FoliageTurret
                    || feedback == FeedbackType.Turret
                    || feedback == FeedbackType.Sniper
                    || feedback == FeedbackType.Strider)
            {
                return FeedbackType.GunHead;
            }
            else
            {
                return FeedbackType.NoFeedback;
            }
        }
        
        public FeedbackType GetOtherHandFeedback(FeedbackType feedback)
        {
            switch (feedback)
            {
                case FeedbackType.PlayerShootPistol:
                    return FeedbackType.PlayerShootPistolLeft;
                case FeedbackType.PlayerShootShotgun:
                    return FeedbackType.PlayerShootShotgunLeft;
                case FeedbackType.PlayerShootSMG:
                    return FeedbackType.PlayerShootSMGLeft;
                case FeedbackType.PlayerShootPistolLeft:
                    return FeedbackType.PlayerShootPistol;
                case FeedbackType.PlayerShootShotgunLeft:
                    return FeedbackType.PlayerShootShotgun;
                case FeedbackType.PlayerShootSMGLeft:
                    return FeedbackType.PlayerShootSMG;
                case FeedbackType.PlayerShootDefault:
                    return FeedbackType.PlayerShootDefaultLeft;
                case FeedbackType.PlayerShootDefaultLeft:
                    return FeedbackType.PlayerShootDefault;
                default:
                    return FeedbackType.NoFeedback;
            }
        }

        public FeedbackType GetFallbackTypeOfWeaponFromPlayer(FeedbackType feedback, bool leftHanded)
        {
            switch (feedback)
            {
                case FeedbackType.PlayerShootPistol:
                    return leftHanded ? FeedbackType.FallbackPistolLeft : FeedbackType.FallbackPistol;
                case FeedbackType.PlayerShootShotgun:
                    return leftHanded ? FeedbackType.FallbackShotgunLeft : FeedbackType.FallbackShotgun;
                case FeedbackType.PlayerShootSMG:
                    return leftHanded ? FeedbackType.FallbackSMGLeft : FeedbackType.FallbackSMG;
                case FeedbackType.PlayerShootPistolLeft:
                    return leftHanded ? FeedbackType.FallbackPistolLeft : FeedbackType.FallbackPistol;
                case FeedbackType.PlayerShootShotgunLeft:
                    return leftHanded ? FeedbackType.FallbackShotgunLeft : FeedbackType.FallbackShotgun;
                case FeedbackType.PlayerShootSMGLeft:
                    return leftHanded ? FeedbackType.FallbackSMGLeft : FeedbackType.FallbackSMG;
                case FeedbackType.PlayerShootDefault:
                    return leftHanded ? FeedbackType.FallbackPistolLeft : FeedbackType.FallbackPistol;
                case FeedbackType.PlayerShootDefaultLeft:
                    return leftHanded ? FeedbackType.FallbackPistolLeft : FeedbackType.FallbackPistol;
                default:
                    return FeedbackType.NoFeedback;
            }

        }

        public FeedbackType GetKickbackOfWeaponFromPlayer(FeedbackType feedback, bool leftHanded)
        {
            switch (feedback)
            {
                case FeedbackType.PlayerShootPistol:
                    return leftHanded ? FeedbackType.KickbackPistolLeft : FeedbackType.KickbackPistol;
                case FeedbackType.PlayerShootShotgun:
                    return leftHanded ? FeedbackType.KickbackShotgunLeft : FeedbackType.KickbackShotgun;
                case FeedbackType.PlayerShootSMG:
                    return leftHanded ? FeedbackType.KickbackSMGLeft : FeedbackType.KickbackSMG;
                case FeedbackType.PlayerGrenadeLaunch:
                    return leftHanded ? FeedbackType.KickbackPistolLeft : FeedbackType.KickbackPistol;
                case FeedbackType.PlayerShootDefault:
                    return leftHanded ? FeedbackType.KickbackPistolLeft : FeedbackType.FallbackPistol;
                case FeedbackType.PlayerShootPistolLeft:
                    return leftHanded ? FeedbackType.KickbackPistolLeft : FeedbackType.KickbackPistol;
                case FeedbackType.PlayerShootShotgunLeft:
                    return leftHanded ? FeedbackType.KickbackShotgunLeft : FeedbackType.KickbackShotgun;
                case FeedbackType.PlayerShootSMGLeft:
                    return leftHanded ? FeedbackType.KickbackSMGLeft : FeedbackType.KickbackSMG;
                case FeedbackType.PlayerGrenadeLaunchLeft:
                    return leftHanded ? FeedbackType.KickbackPistolLeft : FeedbackType.KickbackPistol;
                case FeedbackType.PlayerShootDefaultLeft:
                    return leftHanded ? FeedbackType.KickbackPistolLeft : FeedbackType.FallbackPistol;
                default:
                    return leftHanded ? FeedbackType.KickbackPistolLeft : FeedbackType.FallbackPistol;
            }
        }

        public FeedbackType GetFeedbackTypeOfWeaponFromPlayer(string weapon, bool leftHanded)
        {
            switch (weapon)
            {
                case "hlvr_weapon_crowbar":
                case "hlvr_weapon_crowbar_physics":
                case "hlvr_weapon_energygun":
                    return leftHanded ? FeedbackType.PlayerShootPistolLeft : FeedbackType.PlayerShootPistol;
                case "hlvr_weapon_shotgun":
                    return leftHanded ? FeedbackType.PlayerShootShotgunLeft : FeedbackType.PlayerShootShotgun;
                case "hlvr_weapon_rapidfire":
                case "hlvr_weapon_rapidfire_ammo_capsule":
                case "hlvr_weapon_rapidfire_bullets_manager":
                case "hlvr_weapon_rapidfire_energy_ball":
                case "hlvr_weapon_rapidfire_extended_magazine":
                case "hlvr_weapon_rapidfire_tag_dart":
                case "hlvr_weapon_rapidfire_tag_marker":
                case "hlvr_weapon_rapidfire_upgrade_model":
                    return leftHanded ? FeedbackType.PlayerShootSMGLeft : FeedbackType.PlayerShootSMG;
                default:
                    return leftHanded ? FeedbackType.PlayerShootDefaultLeft : FeedbackType.PlayerShootDefault;
            }
        }

        public FeedbackType GetFeedbackTypeOfEnemyAttack(string enemy, string enemyName)
        {
            if(enemy == "npc_combine_s" && enemyName.Contains("gantry"))
            { 
                return FeedbackType.CombineGantry;
            }

            if (enemy.Contains("grenade") || enemy.Contains("mine"))
            {
                if(enemy.Contains("concussion"))
                { 
                    return FeedbackType.ConcussionGrenade;
                }
                else if (enemy.Contains("hand") || enemy.Contains("xen"))
                {
                    return FeedbackType.HandGrenade;
                }
                else if (enemy.Contains("bugbait"))
                {
                    return FeedbackType.BugBaitGrenade;
                }
                else if (enemy.Contains("frag"))
                {
                    return FeedbackType.FragGrenade;
                }
                else if (enemy.Contains("spy"))
                {
                    return FeedbackType.SpyGrenade;
                }
                else if (enemy.Contains("rollergrenade"))
                {
                    return FeedbackType.RollerGrenade;
                }
                else if (enemy.Contains("rollermine"))
                {
                    return FeedbackType.RollerMine;
                }
                else if (enemy.Contains("hand"))
                {
                    return FeedbackType.HandGrenade;
                }
                else
                {
                    return FeedbackType.FragGrenade;
                }
            }
            
            switch (enemy)
            {
                case "npc_headcrab":
                    return FeedbackType.UnarmedHeadcrab;
                case "npc_headcrab_armored":
                    return FeedbackType.UnarmedHeadcrabArmored;
                case "npc_headcrab_black":
                    return FeedbackType.UnarmedHeadcrabBlack;
                case "npc_headcrab_fast":
                    return FeedbackType.UnarmedHeadcrabFast;
                case "npc_headcrab_runner":
                    return FeedbackType.UnarmedHeadcrabRunner;
                case "npc_fastzombie":
                    return FeedbackType.UnarmedFastZombie;
                case "npc_poisonzombie":
                    return FeedbackType.UnarmedPoisonZombie;
                case "npc_zombie":
                    return FeedbackType.UnarmedZombie;
                case "npc_zombie_blind":
                    return FeedbackType.UnarmedZombieBlind;
                case "npc_zombine":
                    return FeedbackType.UnarmedZombine;
                case "npc_manhack":
                    return FeedbackType.UnarmedManhack;
                case "npc_antlion":
                    return FeedbackType.UnarmedAntlion;
                case "npc_antlionguard":
                case "npc_barnacle":
                case "npc_barnacle_tongue_tip":
                    return FeedbackType.UnarmedAntlionGuard;
                case "xen_foliage_bloater":
                    return FeedbackType.UnarmedBloater;
                case "env_explosion":
                    return FeedbackType.DamageExplosion;
                case "env_fire":
                    return FeedbackType.DamageFire;
                case "env_laser":
                    return FeedbackType.DamageLaser;
                case "env_physexplosion":
                    return FeedbackType.DamageExplosion;
                case "env_physimpact":
                    return FeedbackType.DamageExplosion;
                case "env_spark":
                    return FeedbackType.DamageSpark;
                case "npc_combine": return FeedbackType.Combine;
                case "npc_combine_s": return FeedbackType.CombineS;
                case "npc_metropolice": return FeedbackType.MetroPolice;
                case "npc_sniper": return FeedbackType.Sniper;
                case "npc_strider": return FeedbackType.Strider;
                case "npc_hunter": return FeedbackType.DamageExplosion;
                case "npc_hunter_invincible": return FeedbackType.DamageExplosion;
                case "npc_turret_ceiling": return FeedbackType.Turret;
                case "npc_turret_ceiling_pulse": return FeedbackType.Turret;
                case "npc_turret_citizen": return FeedbackType.Turret;
                case "npc_turret_floor": return FeedbackType.Turret;
                case "xen_foliage_turret": return FeedbackType.FoliageTurret;
                case "xen_foliage_turret_projectile": return FeedbackType.FoliageTurret;
            }

            if (enemy == "prop_physics" && enemyName.Contains("grenade"))
            {
                if (enemyName.Contains("concussion"))
                    return FeedbackType.ConcussionGrenade;
                else if (enemyName.Contains("hand") || enemyName.Contains("xen"))
                    return FeedbackType.HandGrenade;
                else if (enemyName.Contains("bugbait"))
                    return FeedbackType.BugBaitGrenade;
                else if (enemyName.Contains("frag"))
                    return FeedbackType.FragGrenade;
                else if (enemyName.Contains("spy"))
                    return FeedbackType.SpyGrenade;
                else if (enemyName.Contains("roller"))
                    return FeedbackType.RollerGrenade;
            }
            else if (enemy == "prop_physics" && enemyName.Contains("mine"))
            {
                return FeedbackType.RollerMine;
            }

            return FeedbackType.DefaultDamage;
        }

        void FillFeedbackList()
        {
            feedbackMap.Clear();
            feedbackMap.Add(FeedbackType.Startup, new Feedback(FeedbackType.DefaultHead, "Startup_", 0));
            feedbackMap.Add(FeedbackType.ThreeHeartBeats, new Feedback(FeedbackType.UnarmedHead, "ThreeHeartBeats_", 0));

            feedbackMap.Add(FeedbackType.DefaultHead, new Feedback(FeedbackType.DefaultHead, "DefaultHead_", 0));
            feedbackMap.Add(FeedbackType.UnarmedHead, new Feedback(FeedbackType.UnarmedHead, "UnarmedHead_", 0));
            feedbackMap.Add(FeedbackType.GunHead, new Feedback(FeedbackType.GunHead, "GunHead_", 0));

            feedbackMap.Add(FeedbackType.UnarmedBloater, new Feedback(FeedbackType.UnarmedBloater, "UnarmedBloater_", 0));
            feedbackMap.Add(FeedbackType.UnarmedHeadcrab, new Feedback(FeedbackType.UnarmedHeadcrab, "UnarmedHeadcrab_", 0));
            feedbackMap.Add(FeedbackType.UnarmedHeadcrabArmored, new Feedback(FeedbackType.UnarmedHeadcrabArmored, "UnarmedHeadcrabArmored_", 0));
            feedbackMap.Add(FeedbackType.UnarmedHeadcrabBlack, new Feedback(FeedbackType.UnarmedHeadcrabBlack, "UnarmedHeadcrabBlack_", 0));
            feedbackMap.Add(FeedbackType.UnarmedHeadcrabFast, new Feedback(FeedbackType.UnarmedHeadcrabFast, "UnarmedHeadcrabFast_", 0));
            feedbackMap.Add(FeedbackType.UnarmedHeadcrabRunner, new Feedback(FeedbackType.UnarmedHeadcrabRunner, "UnarmedHeadcrabRunner_", 0));
            feedbackMap.Add(FeedbackType.UnarmedFastZombie, new Feedback(FeedbackType.UnarmedFastZombie, "UnarmedFastZombie_", 0));
            feedbackMap.Add(FeedbackType.UnarmedPoisonZombie, new Feedback(FeedbackType.UnarmedPoisonZombie, "UnarmedPoisonZombie_", 0));
            feedbackMap.Add(FeedbackType.UnarmedZombie, new Feedback(FeedbackType.UnarmedZombie, "UnarmedZombie_", 0));
            feedbackMap.Add(FeedbackType.UnarmedZombieBlind, new Feedback(FeedbackType.UnarmedZombieBlind, "UnarmedZombieBlind_", 0));
            feedbackMap.Add(FeedbackType.UnarmedZombine, new Feedback(FeedbackType.UnarmedZombine, "UnarmedZombine_", 0));
            feedbackMap.Add(FeedbackType.UnarmedAntlion, new Feedback(FeedbackType.UnarmedAntlion, "UnarmedAntlion_", 0));
            feedbackMap.Add(FeedbackType.UnarmedAntlionGuard, new Feedback(FeedbackType.UnarmedAntlionGuard, "UnarmedAntlionGuard_", 0));
            feedbackMap.Add(FeedbackType.UnarmedManhack, new Feedback(FeedbackType.UnarmedManhack, "UnarmedManhack_", 0));

            feedbackMap.Add(FeedbackType.GrabbedByBarnacle, new Feedback(FeedbackType.GrabbedByBarnacle, "GrabbedByBarnacle_", 0));

            feedbackMap.Add(FeedbackType.ConcussionGrenade, new Feedback(FeedbackType.ConcussionGrenade, "ConcussionGrenade_", 0));
            feedbackMap.Add(FeedbackType.BugBaitGrenade, new Feedback(FeedbackType.BugBaitGrenade, "BugBaitGrenade_", 0));
            feedbackMap.Add(FeedbackType.FragGrenade, new Feedback(FeedbackType.FragGrenade, "FragGrenade_", 0));
            feedbackMap.Add(FeedbackType.SpyGrenade, new Feedback(FeedbackType.SpyGrenade, "SpyGrenade_", 0));
            feedbackMap.Add(FeedbackType.HandGrenade, new Feedback(FeedbackType.HandGrenade, "HandGrenade_", 0));
            feedbackMap.Add(FeedbackType.RollerGrenade, new Feedback(FeedbackType.RollerGrenade, "RollerGrenade_", 0));
            feedbackMap.Add(FeedbackType.RollerMine, new Feedback(FeedbackType.RollerMine, "RollerMine_", 0));

            feedbackMap.Add(FeedbackType.Combine, new Feedback(FeedbackType.Combine, "Combine_", 0));
            feedbackMap.Add(FeedbackType.CombineS, new Feedback(FeedbackType.CombineS, "CombineS_", 0));
            feedbackMap.Add(FeedbackType.CombineGantry, new Feedback(FeedbackType.CombineGantry, "CombineGantry_", 0));
            feedbackMap.Add(FeedbackType.MetroPolice, new Feedback(FeedbackType.MetroPolice, "MetroPolice_", 0));
            feedbackMap.Add(FeedbackType.Sniper, new Feedback(FeedbackType.Sniper, "Sniper_", 0));
            feedbackMap.Add(FeedbackType.Strider, new Feedback(FeedbackType.Strider, "Strider_", 0));
            feedbackMap.Add(FeedbackType.Turret, new Feedback(FeedbackType.Turret, "Turret_", 0));
            feedbackMap.Add(FeedbackType.FoliageTurret, new Feedback(FeedbackType.FoliageTurret, "FoliageTurret_", 0));

            feedbackMap.Add(FeedbackType.EnvironmentExplosion, new Feedback(FeedbackType.EnvironmentExplosion, "EnvironmentExplosion_", 0));
            feedbackMap.Add(FeedbackType.EnvironmentLaser, new Feedback(FeedbackType.EnvironmentLaser, "EnvironmentLaser_", 0));
            feedbackMap.Add(FeedbackType.EnvironmentFire, new Feedback(FeedbackType.EnvironmentFire, "EnvironmentFire_", 0));
            feedbackMap.Add(FeedbackType.EnvironmentSpark, new Feedback(FeedbackType.EnvironmentSpark, "EnvironmentSpark_", 0));
            feedbackMap.Add(FeedbackType.EnvironmentPoison, new Feedback(FeedbackType.EnvironmentPoison, "EnvironmentPoison_", 0));
            feedbackMap.Add(FeedbackType.EnvironmentRadiation, new Feedback(FeedbackType.EnvironmentRadiation, "EnvironmentRadiation_", 0));

            feedbackMap.Add(FeedbackType.DamageExplosion, new Feedback(FeedbackType.DamageExplosion, "DamageExplosion_", 0));
            feedbackMap.Add(FeedbackType.DamageLaser, new Feedback(FeedbackType.DamageLaser, "DamageLaser_", 0));
            feedbackMap.Add(FeedbackType.DamageFire, new Feedback(FeedbackType.DamageFire, "DamageFire_", 0));
            feedbackMap.Add(FeedbackType.DamageSpark, new Feedback(FeedbackType.DamageSpark, "DamageSpark_", 0));

            feedbackMap.Add(FeedbackType.PlayerShootPistol, new Feedback(FeedbackType.PlayerShootPistol, "PlayerShootPistol_", 0));
            feedbackMap.Add(FeedbackType.PlayerShootShotgun, new Feedback(FeedbackType.PlayerShootShotgun, "PlayerShootShotgun_", 0));
            feedbackMap.Add(FeedbackType.PlayerShootSMG, new Feedback(FeedbackType.PlayerShootSMG, "PlayerShootSMG_", 0));
            feedbackMap.Add(FeedbackType.PlayerShootDefault, new Feedback(FeedbackType.PlayerShootDefault, "PlayerShootDefault_", 0));
            feedbackMap.Add(FeedbackType.PlayerGrenadeLaunch, new Feedback(FeedbackType.PlayerGrenadeLaunch, "PlayerGrenadeLaunch_", 0));

            feedbackMap.Add(FeedbackType.PlayerShootPistolLeft, new Feedback(FeedbackType.PlayerShootPistolLeft, "PlayerShootPistolLeft_", 0));
            feedbackMap.Add(FeedbackType.PlayerShootShotgunLeft, new Feedback(FeedbackType.PlayerShootShotgunLeft, "PlayerShootShotgunLeft_", 0));
            feedbackMap.Add(FeedbackType.PlayerShootSMGLeft, new Feedback(FeedbackType.PlayerShootSMGLeft, "PlayerShootSMGLeft_", 0));
            feedbackMap.Add(FeedbackType.PlayerShootDefaultLeft, new Feedback(FeedbackType.PlayerShootDefaultLeft, "PlayerShootDefaultLeft_", 0));
            feedbackMap.Add(FeedbackType.PlayerGrenadeLaunchLeft, new Feedback(FeedbackType.PlayerGrenadeLaunchLeft, "PlayerGrenadeLaunchLeft_", 0));

            feedbackMap.Add(FeedbackType.FallbackPistol, new Feedback(FeedbackType.FallbackPistol, "FallbackPistol_", 0));
            feedbackMap.Add(FeedbackType.FallbackShotgun, new Feedback(FeedbackType.FallbackShotgun, "FallbackShotgun_", 0));
            feedbackMap.Add(FeedbackType.FallbackSMG, new Feedback(FeedbackType.FallbackSMG, "FallbackSMG_", 0));

            feedbackMap.Add(FeedbackType.FallbackPistolLeft, new Feedback(FeedbackType.FallbackPistolLeft, "FallbackPistolLeft_", 0));
            feedbackMap.Add(FeedbackType.FallbackShotgunLeft, new Feedback(FeedbackType.FallbackShotgunLeft, "FallbackShotgunLeft_", 0));
            feedbackMap.Add(FeedbackType.FallbackSMGLeft, new Feedback(FeedbackType.FallbackSMGLeft, "FallbackSMGLeft_", 0));

            feedbackMap.Add(FeedbackType.KickbackPistol, new Feedback(FeedbackType.KickbackPistol, "KickbackPistol_", 0));
            feedbackMap.Add(FeedbackType.KickbackShotgun, new Feedback(FeedbackType.KickbackShotgun, "KickbackShotgun_", 0));
            feedbackMap.Add(FeedbackType.KickbackSMG, new Feedback(FeedbackType.KickbackSMG, "KickbackSMG_", 0));

            feedbackMap.Add(FeedbackType.KickbackPistolLeft, new Feedback(FeedbackType.KickbackPistolLeft, "KickbackPistolLeft_", 0));
            feedbackMap.Add(FeedbackType.KickbackShotgunLeft, new Feedback(FeedbackType.KickbackShotgunLeft, "KickbackShotgunLeft_", 0));
            feedbackMap.Add(FeedbackType.KickbackSMGLeft, new Feedback(FeedbackType.KickbackSMGLeft, "KickbackSMGLeft_", 0));

            feedbackMap.Add(FeedbackType.HeartBeat, new Feedback(FeedbackType.HeartBeat, "HeartBeat_", 0));
            feedbackMap.Add(FeedbackType.HeartBeatFast, new Feedback(FeedbackType.HeartBeatFast, "HeartBeatFast_", 0));

            feedbackMap.Add(FeedbackType.HealthPenUse, new Feedback(FeedbackType.HealthPenUse, "HealthPenUse_", 0));
            feedbackMap.Add(FeedbackType.HealthStationUse, new Feedback(FeedbackType.HealthStationUse, "HealthStationUse_", 0));
            feedbackMap.Add(FeedbackType.HealthStationUseLeftArm, new Feedback(FeedbackType.HealthStationUseLeftArm, "HealthStationUseLeftArm_", 0));
            feedbackMap.Add(FeedbackType.HealthStationUseRightArm, new Feedback(FeedbackType.HealthStationUseRightArm, "HealthStationUseRightArm_", 0));

            feedbackMap.Add(FeedbackType.BackpackStoreClip, new Feedback(FeedbackType.BackpackStoreClip, "BackpackStoreClipRight_", 0));
            feedbackMap.Add(FeedbackType.BackpackStoreResin, new Feedback(FeedbackType.BackpackStoreResin, "BackpackStoreResinRight_", 0));
            feedbackMap.Add(FeedbackType.BackpackRetrieveClip, new Feedback(FeedbackType.BackpackRetrieveClip, "BackpackRetrieveClipRight_", 0));
            feedbackMap.Add(FeedbackType.BackpackRetrieveResin, new Feedback(FeedbackType.BackpackRetrieveResin, "BackpackRetrieveResinRight_", 0));
            feedbackMap.Add(FeedbackType.ItemHolderStore, new Feedback(FeedbackType.ItemHolderStore, "ItemHolderStore_", 0));
            feedbackMap.Add(FeedbackType.ItemHolderRemove, new Feedback(FeedbackType.ItemHolderRemove, "ItemHolderRemove_", 0));

            feedbackMap.Add(FeedbackType.BackpackStoreClipLeft, new Feedback(FeedbackType.BackpackStoreClipLeft, "BackpackStoreClipLeft_", 0));
            feedbackMap.Add(FeedbackType.BackpackStoreResinLeft, new Feedback(FeedbackType.BackpackStoreResinLeft, "BackpackStoreResinLeft_", 0));
            feedbackMap.Add(FeedbackType.BackpackRetrieveClipLeft, new Feedback(FeedbackType.BackpackRetrieveClipLeft, "BackpackRetrieveClipLeft_", 0));
            feedbackMap.Add(FeedbackType.BackpackRetrieveResinLeft, new Feedback(FeedbackType.BackpackRetrieveResinLeft, "BackpackRetrieveResinLeft_", 0));
            feedbackMap.Add(FeedbackType.ItemHolderStoreLeft, new Feedback(FeedbackType.ItemHolderStoreLeft, "ItemHolderStoreLeft_", 0));
            feedbackMap.Add(FeedbackType.ItemHolderRemoveLeft, new Feedback(FeedbackType.ItemHolderRemoveLeft, "ItemHolderRemoveLeft_", 0));

            feedbackMap.Add(FeedbackType.GravityGloveLockOn, new Feedback(FeedbackType.GravityGloveLockOn, "GravityGloveLockOn_", 0));
            feedbackMap.Add(FeedbackType.GravityGlovePull, new Feedback(FeedbackType.GravityGlovePull, "GravityGlovePull_", 0));
            feedbackMap.Add(FeedbackType.GravityGloveCatch, new Feedback(FeedbackType.GravityGloveCatch, "GravityGloveCatch_", 0));

            feedbackMap.Add(FeedbackType.GravityGloveLockOnLeft, new Feedback(FeedbackType.GravityGloveLockOnLeft, "GravityGloveLockOnLeft_", 0));
            feedbackMap.Add(FeedbackType.GravityGlovePullLeft, new Feedback(FeedbackType.GravityGlovePullLeft, "GravityGlovePullLeft_", 0));
            feedbackMap.Add(FeedbackType.GravityGloveCatchLeft, new Feedback(FeedbackType.GravityGloveCatchLeft, "GravityGloveCatchLeft_", 0));

            feedbackMap.Add(FeedbackType.ClipInserted, new Feedback(FeedbackType.ClipInserted, "ClipInserted_", 0));
            feedbackMap.Add(FeedbackType.ChamberedRound, new Feedback(FeedbackType.ChamberedRound, "ChamberedRound_", 0));
            feedbackMap.Add(FeedbackType.ClipInsertedLeft, new Feedback(FeedbackType.ClipInsertedLeft, "ClipInsertedLeft_", 0));
            feedbackMap.Add(FeedbackType.ChamberedRoundLeft, new Feedback(FeedbackType.ChamberedRoundLeft, "ChamberedRoundLeft_", 0));

            feedbackMap.Add(FeedbackType.Cough, new Feedback(FeedbackType.Cough, "Cough_", 0));
            feedbackMap.Add(FeedbackType.CoughHead, new Feedback(FeedbackType.CoughHead, "CoughHead_", 0));

            feedbackMap.Add(FeedbackType.ShockOnHandLeft, new Feedback(FeedbackType.ShockOnHandLeft, "ShockOnHandLeft_", 0));
            feedbackMap.Add(FeedbackType.ShockOnHandRight, new Feedback(FeedbackType.ShockOnHandRight, "ShockOnHandRight_", 0));


            feedbackMap.Add(FeedbackType.DefaultDamage, new Feedback(FeedbackType.DefaultDamage, "DefaultDamage_", 0));
        }


        void RegisterFeedbackFiles()
        {
            string configPath = Directory.GetCurrentDirectory() + "\\OWO";

            DirectoryInfo d = new DirectoryInfo(configPath);
            FileInfo[] Files = d.GetFiles("*.owo");
            
            for (int i = 0; i < Files.Length; i++)
            {
                string filename = Files[i].Name;
                string fullName = Files[i].FullName;
                string prefix = Path.GetFileNameWithoutExtension(filename);

                if (filename == "." || filename == "..")
                    continue;

                foreach (var element in feedbackMap)
                {
                    if (filename.StartsWith(element.Value.prefix))
                    {
                        //TactFileRegister(configPath, filename, element.Value);
                        string tactFileStr = File.ReadAllText(fullName);
                        try
                        {
                            Sensation test = Sensation.Parse(tactFileStr);
                            //bHaptics.RegisterFeedback(prefix, tactFileStr);
                            //LOG("Pattern registered: " + prefix);
                            Feedback f = feedbackMap[element.Value.feedbackType];
                            f.feedbackFileCount += 1;
                            feedbackMap[element.Value.feedbackType] = f;

                            FeedbackMap.Add(prefix, test);
                        }
                        catch (Exception e) { break; }

                        break;
                    }
                }
            }
        }

        private BakedSensation[] AllBakedSensations()
        {
            var result = new List<BakedSensation>();

            foreach (var sensation in FeedbackMap.Values)
            {
                /*
                BakedSensation baked = (BakedSensation) sensation;
                if (sensation != baked)
                {
                    continue;
                }
                */
                if (sensation is BakedSensation baked)
                    result.Add(baked);
                else continue;
            }

            return result.ToArray();
        }



        public void CreateSystem(string MyIP)
        {
            if (!systemInitialized)
            {
                RegisterFeedbackFiles();
                InitializeOWO(MyIP);
            }
        }

        public void DestroySystem()
        {
            OWO.Disconnect();
            systemInitialized = false;
        }

        private async void InitializeOWO(string MyIP)
        {
            var gameAuth = GameAuth.Create(AllBakedSensations()).WithId("68641817");

            OWO.Configure(gameAuth);

            //LOG("Initializing suit");
            if (MyIP != "")
            {
                systemInitialized = true;
                await OWO.Connect(MyIP);
                if (OWO.ConnectionState != ConnectionState.Connected) systemInitialized = false;
            }
            else
            {
                systemInitialized = true;
                await OWO.AutoConnect();
                if (OWO.ConnectionState != ConnectionState.Connected)
                {
                    systemInitialized = false;
                    //LOG("OWO suit connected.");
                }
            }

            if (systemInitialized) PlayBackFeedback("Startup_1");
        }

        public void PlayBackFeedback(string feedback)
        {
            if (FeedbackMap.ContainsKey(feedback))
            {
                if (FeedbackMap[feedback] is SensationWithMuscles)
                    OWO.Send(FeedbackMap[feedback]);
                else
                {
                    OWO.Send(FeedbackMap[feedback], Muscle.Pectoral_L, Muscle.Pectoral_R, Muscle.Dorsal_L, Muscle.Dorsal_R);
                }
            }
            //else LOG("Feedback not registered: " + feedback);
        }

        public void PlayBackHit(string pattern, float xzAngle, float yShift)
        {
            if (FeedbackMap.ContainsKey(pattern))
            {
                //if (!FeedbackMapWithoutMuscles.ContainsKey(pattern)) return;
                if (FeedbackMap[pattern] is SensationWithMuscles)
                {
                    OWO.Send(FeedbackMap[pattern]);
                    return;
                }
                Sensation sensation = FeedbackMap[pattern];
                Muscle myMuscle = Muscle.Pectoral_R;
                Muscle secondMuscle = Muscle.Pectoral_L;
                bool bigImpact = false;
                if (pattern.Contains("Explosion")) bigImpact = true;
                if (pattern.Contains("Unarmed")) bigImpact = true;
                if (pattern.Contains("Grenade")) bigImpact = true;
                if (pattern.Contains("Environment")) bigImpact = true;
                // two parameters can be given to the pattern to move it on the vest:
                // 1. An angle in degrees [0, 360] to turn the pattern to the left
                // 2. A shift [-0.5, 0.5] in y-direction (up and down) to move it up or down
                if ((xzAngle < 90f))
                {
                    if (bigImpact) { myMuscle = Muscle.Pectoral_L; secondMuscle = Muscle.Abdominal_L; }
                    else if (yShift >= 0f) myMuscle = Muscle.Pectoral_L;
                    else myMuscle = Muscle.Abdominal_L;
                }
                if ((xzAngle > 90f) && (xzAngle < 180f))
                {
                    if (bigImpact) { myMuscle = Muscle.Dorsal_L; secondMuscle = Muscle.Lumbar_L; }
                    else if (yShift >= 0f) myMuscle = Muscle.Dorsal_L;
                    else myMuscle = Muscle.Lumbar_L;
                }
                if ((xzAngle > 180f) && (xzAngle < 270f))
                {
                    if (bigImpact) { myMuscle = Muscle.Dorsal_R; secondMuscle = Muscle.Lumbar_R; }
                    else if (yShift >= 0f) myMuscle = Muscle.Dorsal_R;
                    else myMuscle = Muscle.Lumbar_R;
                }
                if ((xzAngle > 270f))
                {
                    if (bigImpact) { myMuscle = Muscle.Pectoral_R; secondMuscle = Muscle.Abdominal_R; }
                    else if (yShift >= 0f) myMuscle = Muscle.Pectoral_R;
                    else myMuscle = Muscle.Abdominal_R;
                }
                if (bigImpact) OWO.Send(sensation, myMuscle, secondMuscle);
                else OWO.Send(sensation, myMuscle);
            }
            else
            {
                //LOG("Feedback not registered: " + pattern);
                return;
            }

        }

        public void ArmHit(string pattern, bool isRightArm)
        {
            Sensation sensation = FeedbackMap[pattern];
            Muscle myMuscle = Muscle.Arm_L;
            if (isRightArm) myMuscle = Muscle.Arm_R;
            OWO.Send(sensation, myMuscle);
        }

        public void GunRecoil(bool isRightHand, float intensity = 1.0f, bool isTwoHanded = false, bool supportHand = true)
        {
            if (isTwoHanded)
            {
                PlayBackFeedback("Recoil_both");
                return;
            }
            if (isRightHand) PlayBackFeedback("Recoil_R");
            else PlayBackFeedback("Recoil_L");
        }

        public void WarningFeedback()
        {
            OWO.Send(FeedbackMap["ThreeHeartBeats"], Muscle.Pectoral_L);
        }

        public void ProvideHapticFeedback(float locationAngle, float locationHeight, FeedbackType effect, bool waitToPlay, FeedbackType secondEffect)
        {
            
            if (effect == FeedbackType.NoFeedback) return;
            if (!feedbackMap.ContainsKey(effect)) { return; }
            string myFeedback = feedbackMap[effect].prefix + "1";
            if (!FeedbackMap.ContainsKey(myFeedback)) { return; }
            if (!myFeedback.Contains("Spark"))
                if (locationAngle != 0.0f) PlayBackHit(myFeedback, locationAngle, locationHeight);
            else PlayBackFeedback(myFeedback);
        }

        public void StopHapticFeedback(FeedbackType effect)
        {
                if (feedbackMap.ContainsKey(effect))
                {
                    if (feedbackMap[effect].feedbackFileCount > 0)
                    {
                        for (int i = 1; i <= feedbackMap[effect].feedbackFileCount; i++)
                        {
                            string key = feedbackMap[effect].prefix + i.ToString();
                            OWO.Stop();
                            //hapticPlayer.TurnOff(key);
                        }
                    }
                }
        }

        public void PlayRandom()
        {
            ProvideHapticFeedback(0, 0, (FeedbackType)(RandomNumber.Between(0, 97)), false, FeedbackType.NoFeedback);
        }
    }
}

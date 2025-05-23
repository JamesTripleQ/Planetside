﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ItemAPI;
using Dungeonator;
using System.Reflection;
using Random = System.Random;
using FullSerializer;
using System.Collections;
using Gungeon;
using MonoMod.RuntimeDetour;
using MonoMod;


namespace Planetside
{
    public class AlchemicalVial : PlayerItem
    {

        public static void Init()
        {
            string itemName = "Projectile Transmutator";
            //string resourceName = "Planetside/Resources/precursor1.png";
            GameObject obj = new GameObject(itemName);
            AlchemicalVial activeitem = obj.AddComponent<AlchemicalVial>();
            var data = StaticSpriteDefinitions.Active_Item_Sheet_Data;
            ItemBuilder.AddSpriteToObjectAssetbundle(itemName, data.GetSpriteIdByName("precursor1"), data, obj);
            //ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);
            string shortDesc = "Completely Stabile";
            string longDesc = "Transmutes all of your projectiles to a pool of goop.\n\nReload on a full clip to change transmutation type.\n\nDespite what the label says, this is actually a bottle of lead paint. Don't drink it...";
            activeitem.SetupItem(shortDesc, longDesc, "psog");
            activeitem.SetCooldownType(ItemBuilder.CooldownType.Timed, 10f);
            activeitem.quality = PickupObject.ItemQuality.D;
            activeitem.AddToSubShop(ItemBuilder.ShopType.Goopton, 1f);
            AlchemicalVial.AlchemicalVialID = activeitem.PickupObjectId;
            ItemIDs.AddToList(activeitem.PickupObjectId);


            AlchemicalVial.spriteIDs.Add("nAn", data.GetSpriteIdByName("precursor1"));//SpriteBuilder.AddSpriteToCollection(AlchemicalVial.spritePaths[0], activeitem.sprite.Collection));
            AlchemicalVial.spriteIDs.Add("fire", data.GetSpriteIdByName("precursor2"));//SpriteBuilder.AddSpriteToCollection(AlchemicalVial.spritePaths[1], activeitem.sprite.Collection));
            AlchemicalVial.spriteIDs.Add("poison", data.GetSpriteIdByName("precursor3"));//SpriteBuilder.AddSpriteToCollection(AlchemicalVial.spritePaths[2], activeitem.sprite.Collection));

            AlchemicalVial.spriteIDs.Add("frail", data.GetSpriteIdByName("precursor4"));//SpriteBuilder.AddSpriteToCollection(AlchemicalVial.spritePaths[3], activeitem.sprite.Collection));
            AlchemicalVial.spriteIDs.Add("cheese", data.GetSpriteIdByName("precursor5"));//SpriteBuilder.AddSpriteToCollection(AlchemicalVial.spritePaths[4], activeitem.sprite.Collection));
            AlchemicalVial.spriteIDs.Add("tarnish", data.GetSpriteIdByName("precursor6"));//SpriteBuilder.AddSpriteToCollection(AlchemicalVial.spritePaths[5], activeitem.sprite.Collection));


            AlchemicalVial.ActiveIDS.Add("fire");
            AlchemicalVial.ActiveIDS.Add("poison");

            CurrentCount = 1;
            List<string> mandatoryConsoleIDs = new List<string>
            {
                "psog:projectile_transmutator",
            };
            List<string> optionalConsoleIDs = new List<string>
            {
                "elimentaler",
                "partially_eaten_cheese",
                "rat_boots"
            };
            CustomSynergies.Add("You Can Liquidate That???", mandatoryConsoleIDs, optionalConsoleIDs, true);
            List<string> optionalConsoleIDs2 = new List<string>
            {
                "psog:frailty_ammolet",
                "psog:frailty_rounds",
            };
            CustomSynergies.Add("Epic ____ Moments No.17", mandatoryConsoleIDs, optionalConsoleIDs2, true);
            List<string> optionalConsoleIDs3 = new List<string>
            {
                "plunger",
                "antibody",
                "monster_blood",
                "weird_egg",
                "psog:injector_rounds"
            };
            CustomSynergies.Add("You Killed Us All!", mandatoryConsoleIDs, optionalConsoleIDs3, true);
            List<string> optionalConsoleIDs4 = new List<string>
            {
                "psog:tarnished_rounds",
                "psog:tarnished_ammolet",
            };
            CustomSynergies.Add("Rust Bucket", mandatoryConsoleIDs, optionalConsoleIDs4, true);

            SynergyAPI.SynergyBuilder.AddItemToSynergy(activeitem, CustomSynergyType.GUON_UPGRADE_CLEAR);

            activeitem.CanSwitch = true;
        }

        public IEnumerator Cooldown()
        {
            CanSwitch = false;
            yield return new WaitForSeconds(0.25f);
            CanSwitch = true;
            yield break;
        }
        private bool CanSwitch;
        private static Dictionary<string, GoopDefinition> GoopKeys = new Dictionary<string, GoopDefinition>()
        {
            {"fire",  EasyGoopDefinitions.FireDef},
            {"poison",  EasyGoopDefinitions.PoisonDef},
            {"frail",  DebuffLibrary.FrailPuddle},
            {"cheese",  EasyGoopDefinitions.CheeseDef},
            {"nAn",  EasyGoopDefinitions.WaterGoop},
            {"tarnish",  DebuffLibrary.TarnishedGoop},

        };
        private Dictionary<string, Color> ColorKeys = new Dictionary<string, Color>()
        {
            {"fire", Color.red},
            {"poison", Color.green},
            {"frail", Color.magenta},
            {"cheese", Color.yellow},
            {"nAn", Color.white},
            {"tarnish", Color.green},
        };

        private Dictionary<string, string> SynergyKeys = new Dictionary<string, string>()
        {
            {"You Can Liquidate That???", "cheese"},
            {"Epic ____ Moments No.17", "frail"},
            {"Rust Bucket", "tarnish"},

        };

        public static int AlchemicalVialID;
        public override void Pickup(PlayerController player)
        {
            CanSwitch = true;
            base.Pickup(player);
            player.OnReloadPressed += reloadPressed;
        }
        public void reloadPressed(PlayerController player, Gun gun)
        {
            if (gun.ClipShotsRemaining == gun.ClipCapacity && CanSwitch != false)
            {
                AkSoundEngine.PostEvent("Play_ENM_wizardred_appear_01", player.gameObject);
                CurrentCount++;
                if (CurrentCount == ActiveIDS.Count+1) { CurrentCount = 1; }
                int yes;
                string c = AlchemicalVial.ActiveIDS[CurrentCount - 1] != null ? AlchemicalVial.ActiveIDS[CurrentCount - 1] : "nAn";
                AlchemicalVial.spriteIDs.TryGetValue(c, out yes);
                base.sprite.SetSprite(yes);
                player.BloopItemAboveHead(base.sprite);
                player.StartCoroutine(Cooldown());
            }
        }
        public override void Update()
        {
            base.Update();
            if (base.LastOwner)
            {
                foreach (var Entry in SynergyKeys)
                {
                    string synergy = Entry.Key;
                    string stringKey = Entry.Value;
                    if (base.LastOwner.PlayerHasActiveSynergy(synergy) && !ActiveIDS.Contains(stringKey))
                    { ActiveIDS.Add(stringKey); }
                    else if (!base.LastOwner.PlayerHasActiveSynergy(synergy) && ActiveIDS.Contains(stringKey))
                    { ActiveIDS.Remove(stringKey); }
                }
                if (CurrentCount == ActiveIDS.Count + 1) 
                {
                    AkSoundEngine.PostEvent("Play_ENM_wizardred_appear_01", base.LastOwner.gameObject);
                    CurrentCount = 1;
                    int yes;
                    AlchemicalVial.spriteIDs.TryGetValue(AlchemicalVial.ActiveIDS[CurrentCount-1], out yes);
                    base.sprite.SetSprite(yes);
                    base.LastOwner.BloopItemAboveHead(base.sprite);
                }              
            }
        }
        public override void OnPreDrop(PlayerController player)
        {
            CanSwitch = true;
            player.OnReloadPressed -= reloadPressed;
            base.OnPreDrop(player);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            CanSwitch = true;
            if (base.LastOwner != null)
            {
                base.LastOwner.OnReloadPressed -= reloadPressed;
            }
        }
        public override void DoEffect(PlayerController user)
        {
            base.DoEffect(user);
            GameObject gameObject2 = SpawnManager.SpawnVFX(StaticVFXStorage.JammedDeathVFX, user.transform.position, Quaternion.identity, false);
            if (gameObject2 && gameObject2.GetComponent<tk2dSprite>())
            {
                tk2dSprite component2 = gameObject2.GetComponent<tk2dSprite>();
                component2.scale *= 0.5f;
                component2.HeightOffGround = 5f;
                component2.UpdateZDepth();
            }
            Exploder.DoDistortionWave(user.sprite.WorldCenter, 2, 0.05f, 24, 0.5f);
            AkSoundEngine.PostEvent("Play_OBJ_bottle_cork_01", user.gameObject);
            for (int i = 0; i < StaticReferenceManager.AllProjectiles.Count; i++)
            {
                Projectile proj = StaticReferenceManager.AllProjectiles[i];
                PlayerController player = proj.Owner as PlayerController;
                bool isBem = proj.GetComponent<BasicBeamController>() != null;
                if (isBem != true && proj.Owner != null && proj.Owner == player && proj != null)
                {
                    GoopDefinition goop = null;
                    AlchemicalVial.GoopKeys.TryGetValue(AlchemicalVial.ActiveIDS[CurrentCount - 1], out goop);
                    DeadlyDeadlyGoopManager.GetGoopManagerForGoopType(goop != null ? goop: EasyGoopDefinitions.FireDef).TimedAddGoopCircle(proj.transform.PositionVector2(), user.PlayerHasActiveSynergy("You Killed Us All!") == true ? 4.5f : 2.25f, 0.5f, false);
                    GameObject PoofVFX = (GameObject)UnityEngine.Object.Instantiate(StaticVFXStorage.BlueSynergyPoofVFX, proj.transform.position, Quaternion.identity);
                    tk2dBaseSprite PoofVFXSprite = PoofVFX.GetComponent<tk2dBaseSprite>();
                    PoofVFXSprite.PlaceAtPositionByAnchor(proj.sprite.WorldCenter, tk2dBaseSprite.Anchor.MiddleCenter);
                    tk2dSpriteAnimator component2 = PoofVFXSprite.GetComponent<tk2dSpriteAnimator>();
                    if (component2 != null)
                    {
                        Color color = new Color();
                        ColorKeys.TryGetValue(AlchemicalVial.ActiveIDS[CurrentCount - 1] != null ? AlchemicalVial.ActiveIDS[CurrentCount - 1] : "nAn", out color);
                        PoofVFXSprite.scale *= 1.25f;
                        component2.playAutomatically = true;
                        component2.sprite.usesOverrideMaterial = true;
                        component2.sprite.renderer.material.shader = ShaderCache.Acquire("Brave/LitTk2dCustomFalloffTintableTiltedCutoutEmissive");
                        component2.sprite.renderer.material.EnableKeyword("BRIGHTNESS_CLAMP_ON");
                        component2.sprite.renderer.material.SetFloat("_EmissivePower", 1);
                        component2.sprite.renderer.material.SetFloat("_EmissiveColorPower", 0.1f);
                        component2.sprite.renderer.material.SetColor("_OverrideColor", color);
                        component2.sprite.renderer.material.SetColor("_EmissiveColor", color);
                    }
                    Exploder.Explode(proj.sprite.WorldCenter, new ExplosionData()
                    {
                        breakSecretWalls = false,
                        damage = 5 + (proj.baseData.damage * 0.333f),
                        debrisForce = 0,
                        damageRadius = user.PlayerHasActiveSynergy("You Killed Us All!") == true ? 5 : 3,
                        damageToPlayer = 0,
                        comprehensiveDelay = 0,
                        doForce = false,
                        doExplosionRing = false,
                        effect = null,
                        force = 0,
                        explosionDelay = 0,
                        doDamage = true,
                        doDestroyProjectiles = false,
                        doScreenShake = false,
                        ignoreList = new List<SpeculativeRigidbody>() { base.LastOwner.specRigidbody },
                        isFreezeExplosion = false,
                        pushRadius = 0,
                        playDefaultSFX = false,
                        
                    }, Vector2.zero);
                    proj.DieInAir();
                }
            }
        }
        private static Dictionary<string, int> spriteIDs = new Dictionary<string, int>();
        private static List<string> ActiveIDS = new List<string>();
        private static int CurrentCount;

        private static readonly string[] spritePaths = new string[]
        {
            "Planetside/Resources/precursor1.png",
            "Planetside/Resources/precursor2.png",
            "Planetside/Resources/precursor3.png",
            "Planetside/Resources/precursor4.png",
            "Planetside/Resources/precursor5.png",
            "Planetside/Resources/precursor6.png"
        };
    }
}




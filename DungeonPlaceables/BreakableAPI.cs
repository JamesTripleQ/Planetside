﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Dungeonator;
using ItemAPI;
using UnityEngine;
using GungeonAPI;
using MonoMod.RuntimeDetour;
using System.Reflection;
using Planetside;
using Gungeon;
using FullInspector;
using Brave.BulletScript;

namespace BreakAbleAPI
{
    public static class BreakableAPIToolbox
    {

        //public static Shader worldShader = ShaderCache.Acquire("Brave/LitTk2dCustomFalloffTiltedCutoutFastPixelShadow");


        /// <summary>
        /// Generates, and returns a TeleporterController. This is for generating a Teleporter that you can teleport to from the map. Of note, any of the gameObject stuff at the end of the method is nullable and should use the default teleporter VFX stuff if left as null
        /// </summary>
        /// <param name="name">The name of your Teleporter object. Keep it simple, no special characters.</param>
        /// <param name="idleSpritePaths">Your idle aniamtion sprite paths. Only insert one path if you don't want it to be animated.</param>
        /// <param name="activationSpritePaths">Your sprite paths for the aniamtion is plays when its activated.</param>
        /// <param name="activeIdleSpritePaths">Your sprite paths for the idle aniamtion it plays when its active.</param>
        /// <param name="MinimapIconPath">Your sprite path for the room icon.</param>
        /// <param name="idleAnimFPS">Your idle aniamtion FPS.</param>
        /// <param name="activateAnimFPS">Your active animtion FPS. Due to how Teleporters are coded, the "activate" and "active idle" animation are one aniamtion with the "active idle" just being looped. (CITATION NEEDED)</param>
        /// <param name="isActiveVFX">The VFX that plays on the Teleporter when you teleporter to one.</param>
        /// <param name="singleTimeActivateVFX">The VFX that plays on the Teleporter its initially activated.</param>
        /// <param name="teleproterArrivedVFX">The VFX that plays on the Teleporter when you teleport to it.</param>
        /// <param name="teleporterDepartVFX">The VFX that plays on the Teleporter when you teleport away from it.</param>

        public static TeleporterController GenerateTeleporterController(string name, string[] idleSpritePaths, string[] activationSpritePaths, string[] activeIdleSpritePaths, string MinimapIconPath, int idleAnimFPS = 5, int activateAnimFPS = 5, GameObject isActiveVFX = null, GameObject singleTimeActivateVFX = null, GameObject teleproterArrivedVFX = null, GameObject teleporterDepartVFX = null, tk2dSpriteAnimator portalVFX = null)
        {
            TeleporterController existingTeleporterController = ResourceManager.LoadAssetBundle("brave_resources_001").LoadAsset<GameObject>("Teleporter_Gungeon_01").GetComponentInChildren<TeleporterController>();

            GameObject gameObject = SpriteBuilder.SpriteFromResource(idleSpritePaths[0], null, false);
            FakePrefab.MarkAsFakePrefab(gameObject);
            gameObject.name = name;
            gameObject.layer = 20;

            tk2dSpriteCollectionData SpriteObjectSpriteCollection = SpriteBuilder.ConstructCollection(gameObject, (name + "_Collection"));
            int spriteID = SpriteBuilder.AddSpriteToCollection(idleSpritePaths[0], SpriteObjectSpriteCollection);
            tk2dSprite sprite = gameObject.GetOrAddComponent<tk2dSprite>();
            sprite.SetSprite(SpriteObjectSpriteCollection, spriteID);


            tk2dSpriteAnimator animator = gameObject.GetOrAddComponent<tk2dSpriteAnimator>();
            tk2dSpriteAnimation animation = gameObject.AddComponent<tk2dSpriteAnimation>();
            animation.clips = new tk2dSpriteAnimationClip[0];
            animator.Library = animation;

            TeleporterController teleporterController = gameObject.GetOrAddComponent<TeleporterController>();

            teleporterController.sprite = sprite;
            teleporterController.spriteAnimator = animator;


            GameObject roomIcon = SpriteBuilder.SpriteFromResource(MinimapIconPath, null, false);
            FakePrefab.MarkAsFakePrefab(roomIcon);
            roomIcon.name = name+"_RoomIcon";
            roomIcon.layer = 22;
            teleporterController.teleporterIcon = roomIcon;

            if (isActiveVFX == null)
            {
                GameObject clonedextantActiveVFX = FakePrefab.Clone(existingTeleporterController.extantActiveVFX);
                clonedextantActiveVFX.PostProcessFakePrefab();
                teleporterController.extantActiveVFX = clonedextantActiveVFX;
            }
            else
            { teleporterController.extantActiveVFX = isActiveVFX;}


            if (singleTimeActivateVFX != null)
            {
                teleporterController.onetimeActivateVFX = singleTimeActivateVFX;
            }

            if (teleproterArrivedVFX == null)
            {

                GameObject clonedteleportArrivalVFX = FakePrefab.Clone(existingTeleporterController.teleportArrivalVFX);
                clonedteleportArrivalVFX.PostProcessFakePrefab();
                teleporterController.teleportArrivalVFX = clonedteleportArrivalVFX;
            }
            else
            { teleporterController.teleportArrivalVFX = teleproterArrivedVFX; }


            if (teleporterDepartVFX == null)
            {

                GameObject clonedteleportDepartureVFX = FakePrefab.Clone(existingTeleporterController.teleportDepartureVFX);
                clonedteleportDepartureVFX.PostProcessFakePrefab();
                teleporterController.teleportDepartureVFX = clonedteleportDepartureVFX;
            }
            else
            { teleporterController.teleportDepartureVFX = teleporterDepartVFX; }


            if (portalVFX == null)
            {

                GameObject clonedportalVFX = FakePrefab.Clone(existingTeleporterController.portalVFX.gameObject);
                clonedportalVFX.PostProcessFakePrefab();
                teleporterController.portalVFX = clonedportalVFX.GetComponent<tk2dSpriteAnimator>();
            }
            else
            { teleporterController.portalVFX = portalVFX; }

            List<tk2dSpriteAnimationClip> clips = new List<tk2dSpriteAnimationClip>();


            if (idleSpritePaths.Length >= 1)
            {
                tk2dSpriteAnimationClip idleClip = new tk2dSpriteAnimationClip() { name = "idle", frames = new tk2dSpriteAnimationFrame[0], fps = idleAnimFPS };
                List<tk2dSpriteAnimationFrame> frames = new List<tk2dSpriteAnimationFrame>();
                for (int i = 0; i < idleSpritePaths.Length; i++)
                {
                    tk2dSpriteCollectionData collection = SpriteObjectSpriteCollection;
                    int frameSpriteId = SpriteBuilder.AddSpriteToCollection(idleSpritePaths[i], collection);
                    tk2dSpriteDefinition frameDef = collection.spriteDefinitions[frameSpriteId];
                    frames.Add(new tk2dSpriteAnimationFrame { spriteId = frameSpriteId, spriteCollection = collection });
                    frameDef.ConstructOffsetsFromAnchor(tk2dBaseSprite.Anchor.LowerLeft);
                }
                idleClip.frames = frames.ToArray();
                idleClip.wrapMode = tk2dSpriteAnimationClip.WrapMode.Loop;
                animator.Library.clips = animation.clips.Concat(new tk2dSpriteAnimationClip[] { idleClip }).ToArray();
                animator.playAutomatically = true;
                animator.DefaultClipId = animator.GetClipIdByName("idle");
                clips.Add(idleClip);

                tk2dSpriteAnimationClip[] array = clips.ToArray();
                animator.Library.clips = array;
                animator.playAutomatically = true;
                animator.DefaultClipId = animator.GetClipIdByName("idle");

            }

            if (activationSpritePaths.Length >= 1)
            {
                tk2dSpriteAnimationClip unsealClip = new tk2dSpriteAnimationClip() { name = "teleport_pad_activate", frames = new tk2dSpriteAnimationFrame[0], fps = activateAnimFPS };
                List<tk2dSpriteAnimationFrame> unsealFrames = new List<tk2dSpriteAnimationFrame>();

                for (int i = 0; i < activationSpritePaths.Length; i++)
                {
                    tk2dSpriteCollectionData collection = SpriteObjectSpriteCollection;
                    int frameSpriteId = SpriteBuilder.AddSpriteToCollection(activationSpritePaths[i], collection);
                    tk2dSpriteDefinition frameDef = collection.spriteDefinitions[frameSpriteId];
                    unsealFrames.Add(new tk2dSpriteAnimationFrame { spriteId = frameSpriteId, spriteCollection = collection });
                    frameDef.ConstructOffsetsFromAnchor(tk2dBaseSprite.Anchor.LowerLeft);
                }

                for (int i = 0; i < activeIdleSpritePaths.Length; i++)
                {
                    tk2dSpriteCollectionData collection = SpriteObjectSpriteCollection;
                    int frameSpriteId = SpriteBuilder.AddSpriteToCollection(activeIdleSpritePaths[i], collection);
                    tk2dSpriteDefinition frameDef = collection.spriteDefinitions[frameSpriteId];
                    unsealFrames.Add(new tk2dSpriteAnimationFrame { spriteId = frameSpriteId, spriteCollection = collection });
                    frameDef.ConstructOffsetsFromAnchor(tk2dBaseSprite.Anchor.LowerLeft);
                }
                unsealClip.wrapMode = tk2dSpriteAnimationClip.WrapMode.LoopSection;
                unsealClip.loopStart = activationSpritePaths.Length;
                unsealClip.frames = unsealFrames.ToArray();
                animator.Library.clips = animation.clips.Concat(new tk2dSpriteAnimationClip[] { unsealClip }).ToArray();
                clips.Add(unsealClip);
            }

            animator.Library.clips = clips.ToArray();
            animator.playAutomatically = true;
            animator.DefaultClipId = animator.GetClipIdByName("idle");

            return teleporterController;
        }

        private static void PostProcessFakePrefab(this GameObject self)
        {
            FakePrefab.MarkAsFakePrefab(self);
            UnityEngine.Object.DontDestroyOnLoad(self);
        }

        /// <summary>
        /// Generates, and returns a shadow for your breakables. This is a very simple shadow object, so make the sprite completely black for best results!
        /// </summary>
        /// <param name="ShadowSpritePath">The sprite path to yoru shadow sprite.</param>
        /// <param name="name">The object name.</param>
        /// <param name="parent">The parent objects Transform that your shadow will be parented to.</param>
        /// <param name="Offset">The offset of the shadow object.</param>
        /// <param name="customCollection">Leave this as null. Currently for an unfinished idea.</param>

        public static GameObject GenerateShadow(string ShadowSpritePath, string name, Transform parent, Vector3 Offset, tk2dSpriteCollectionData customCollection = null)
        {
            GameObject shadowObject = SpriteBuilder.SpriteFromResource(ShadowSpritePath, null, false);

            tk2dSpriteCollectionData ShadowSpriteCollection = customCollection ?? SpriteBuilder.ConstructCollection(shadowObject, (name + "_Collection"));
            shadowObject.name = name;
            int newSpriteId2 = SpriteBuilder.AddSpriteToCollection(ShadowSpritePath, ShadowSpriteCollection);
            tk2dSprite orAddComponent3 = shadowObject.GetOrAddComponent<tk2dSprite>();
            orAddComponent3.SetSprite(ShadowSpriteCollection, newSpriteId2);

            orAddComponent3.HeightOffGround = parent.gameObject.GetComponent<tk2dSprite>() != null ? parent.gameObject.GetComponent<tk2dSprite>().HeightOffGround - 0.1f : 0;
            shadowObject.transform.position = parent.gameObject.transform.position + Offset;
            shadowObject.transform.parent = parent;

            DepthLookupManager.ProcessRenderer(shadowObject.GetComponent<Renderer>(), DepthLookupManager.GungeonSortingLayer.BACKGROUND);
            orAddComponent3.usesOverrideMaterial = true;
            orAddComponent3.renderer.material.shader = Shader.Find("Brave/Internal/SimpleAlphaFadeUnlit");
            orAddComponent3.renderer.material.SetFloat("_Fade", 0.66f);

            return shadowObject;
        }


        /// <summary>
        /// Unfinished, do not use!
        /// </summary>
        private static DungeonDoorSubsidiaryBlocker GenerateDungeonDoorSubsidiaryBlocker(string name, string[] idleSpritePaths, string[] sealSpritePaths, string[] unsealSpritePaths, bool isNorthSouthDoor, string[] playerNearSealedDoorAnimPaths = null, int idleAnimFPS = 5, int sealAnimFPS = 5, int unsealAnimFPS = 5, int playerNearSealedDoorAnimFPS = 5, string[] chainIdleSpritePaths = null, string[] chainSealSpritePaths = null, string[] chainUnsealSpritePaths = null, string[] chainPlayerNearChainPaths = null )
        {

            GameObject gameObject = SpriteBuilder.SpriteFromResource(idleSpritePaths[0], null, false);
            FakePrefab.MarkAsFakePrefab(gameObject);
            gameObject.name = name;

            tk2dSpriteCollectionData SpriteObjectSpriteCollection = SpriteBuilder.ConstructCollection(gameObject, (name + "_Collection"));
            int spriteID = SpriteBuilder.AddSpriteToCollection(idleSpritePaths[0], SpriteObjectSpriteCollection);
            tk2dSprite sprite = gameObject.GetOrAddComponent<tk2dSprite>();
            sprite.SetSprite(SpriteObjectSpriteCollection, spriteID);


            tk2dSpriteAnimator animator = gameObject.GetOrAddComponent<tk2dSpriteAnimator>();
            tk2dSpriteAnimation animation = gameObject.AddComponent<tk2dSpriteAnimation>();
            DungeonDoorSubsidiaryBlocker dungeonDoorSubsidiaryBlocker = gameObject.GetOrAddComponent<DungeonDoorSubsidiaryBlocker>();

            animation.clips = new tk2dSpriteAnimationClip[0];
            animator.Library = animation;

            SpeculativeRigidbody speculativeRigidbody = sprite.SetUpEmptySpeculativeRigidbody(new IntVector2(0,0), new IntVector2(isNorthSouthDoor == true ? 32 : 16, isNorthSouthDoor == true ? 16 : 32));
            speculativeRigidbody.PixelColliders.Add(new PixelCollider
            {
                ColliderGenerationMode = PixelCollider.PixelColliderGeneration.Manual,
                CollisionLayer = CollisionLayer.HighObstacle,
                IsTrigger = false,
                BagleUseFirstFrameOnly = false,
                SpecifyBagelFrame = string.Empty,
                BagelColliderNumber = 0,
                ManualOffsetX = 0,
                ManualOffsetY = 0,
                ManualWidth = 12,
                ManualHeight = 12,
                ManualDiameter = 0,
                ManualLeftX = 0,
                ManualLeftY = 0,
                ManualRightX = 0,
                ManualRightY = 0,
            });

            List<tk2dSpriteAnimationClip> clips = new List<tk2dSpriteAnimationClip>();

       
            if (idleSpritePaths.Length >= 1)
            {
                tk2dSpriteAnimationClip idleClip = new tk2dSpriteAnimationClip() { name = "idle", frames = new tk2dSpriteAnimationFrame[0], fps = idleAnimFPS };
                List<tk2dSpriteAnimationFrame> frames = new List<tk2dSpriteAnimationFrame>();
                for (int i = 0; i < idleSpritePaths.Length; i++)
                {
                    tk2dSpriteCollectionData collection = SpriteObjectSpriteCollection;
                    int frameSpriteId = SpriteBuilder.AddSpriteToCollection(idleSpritePaths[i], collection);
                    tk2dSpriteDefinition frameDef = collection.spriteDefinitions[frameSpriteId];
                    frames.Add(new tk2dSpriteAnimationFrame { spriteId = frameSpriteId, spriteCollection = collection });
                    frameDef.ConstructOffsetsFromAnchor(tk2dBaseSprite.Anchor.LowerLeft);
                }
                idleClip.frames = frames.ToArray();
                idleClip.wrapMode = tk2dSpriteAnimationClip.WrapMode.Loop;
                animator.Library.clips = animation.clips.Concat(new tk2dSpriteAnimationClip[] { idleClip }).ToArray();
                animator.playAutomatically = true;
                animator.DefaultClipId = animator.GetClipIdByName("idle");
                clips.Add(idleClip);
                
                tk2dSpriteAnimationClip[] array = clips.ToArray();
                animator.Library.clips = array;
                animator.playAutomatically = true;
                animator.DefaultClipId = animator.GetClipIdByName("idle");
                
            }
            
            if (sealSpritePaths.Length >= 1)
            {
                tk2dSpriteAnimationClip sealClip = new tk2dSpriteAnimationClip() { name = "seal", frames = new tk2dSpriteAnimationFrame[0], fps = sealAnimFPS };
                List<tk2dSpriteAnimationFrame> sealFrames = new List<tk2dSpriteAnimationFrame>();
                for (int i = 0; i < sealSpritePaths.Length; i++)
                {
                    tk2dSpriteCollectionData collection = SpriteObjectSpriteCollection;
                    int frameSpriteId = SpriteBuilder.AddSpriteToCollection(sealSpritePaths[i], collection);
                    tk2dSpriteDefinition frameDef = collection.spriteDefinitions[frameSpriteId];
                    sealFrames.Add(new tk2dSpriteAnimationFrame { spriteId = frameSpriteId, spriteCollection = collection });
                    frameDef.ConstructOffsetsFromAnchor(tk2dBaseSprite.Anchor.LowerLeft);
                }
                sealClip.frames = sealFrames.ToArray();
                sealClip.wrapMode = tk2dSpriteAnimationClip.WrapMode.Once;
                animator.Library.clips = animation.clips.Concat(new tk2dSpriteAnimationClip[] { sealClip }).ToArray();
                clips.Add(sealClip);
                dungeonDoorSubsidiaryBlocker.sealAnimationName = "seal";
            }

            if (unsealSpritePaths.Length >= 1)
            {
                tk2dSpriteAnimationClip unsealClip = new tk2dSpriteAnimationClip() { name = "unseal", frames = new tk2dSpriteAnimationFrame[0], fps = unsealAnimFPS };
                List<tk2dSpriteAnimationFrame> unsealFrames = new List<tk2dSpriteAnimationFrame>();
                for (int i = 0; i < unsealSpritePaths.Length; i++)
                {
                    tk2dSpriteCollectionData collection = SpriteObjectSpriteCollection;
                    int frameSpriteId = SpriteBuilder.AddSpriteToCollection(unsealSpritePaths[i], collection);
                    tk2dSpriteDefinition frameDef = collection.spriteDefinitions[frameSpriteId];
                    unsealFrames.Add(new tk2dSpriteAnimationFrame { spriteId = frameSpriteId, spriteCollection = collection });
                    frameDef.ConstructOffsetsFromAnchor(tk2dBaseSprite.Anchor.LowerLeft);
                }
                unsealClip.frames = unsealFrames.ToArray();
                unsealClip.wrapMode = tk2dSpriteAnimationClip.WrapMode.Once;
                animator.Library.clips = animation.clips.Concat(new tk2dSpriteAnimationClip[] { unsealClip }).ToArray();
                clips.Add(unsealClip);
                dungeonDoorSubsidiaryBlocker.unsealAnimationName = "unseal";
            }

            if (playerNearSealedDoorAnimPaths.Length >= 1 && playerNearSealedDoorAnimPaths != null)
            {
                tk2dSpriteAnimationClip playernearblockerClip = new tk2dSpriteAnimationClip() { name = "playernearblocker", frames = new tk2dSpriteAnimationFrame[0], fps = playerNearSealedDoorAnimFPS };
                List<tk2dSpriteAnimationFrame> playernearblockerFrames = new List<tk2dSpriteAnimationFrame>();
                for (int i = 0; i < playerNearSealedDoorAnimPaths.Length; i++)
                {
                    tk2dSpriteCollectionData collection = SpriteObjectSpriteCollection;
                    int frameSpriteId = SpriteBuilder.AddSpriteToCollection(playerNearSealedDoorAnimPaths[i], collection);
                    tk2dSpriteDefinition frameDef = collection.spriteDefinitions[frameSpriteId];
                    playernearblockerFrames.Add(new tk2dSpriteAnimationFrame { spriteId = frameSpriteId, spriteCollection = collection });
                    frameDef.ConstructOffsetsFromAnchor(tk2dBaseSprite.Anchor.LowerLeft);
                }
                playernearblockerClip.frames = playernearblockerFrames.ToArray();
                playernearblockerClip.wrapMode = tk2dSpriteAnimationClip.WrapMode.Loop;
                animator.Library.clips = animation.clips.Concat(new tk2dSpriteAnimationClip[] { playernearblockerClip }).ToArray();
                clips.Add(playernearblockerClip);
                dungeonDoorSubsidiaryBlocker.playerNearSealedAnimationName = "playernearblocker";
            }
            dungeonDoorSubsidiaryBlocker.northSouth = isNorthSouthDoor;
            dungeonDoorSubsidiaryBlocker.unsealDistanceMaximum = -1;

            if (chainIdleSpritePaths != null && chainPlayerNearChainPaths != null && chainSealSpritePaths != null && chainUnsealSpritePaths != null)
            {
                tk2dSpriteAnimator ChainObjectAnimator = GenerateChainObject("chain_" + name, gameObject, SpriteObjectSpriteCollection, dungeonDoorSubsidiaryBlocker, chainIdleSpritePaths, chainSealSpritePaths, chainUnsealSpritePaths, chainPlayerNearChainPaths);
                dungeonDoorSubsidiaryBlocker.chainAnimator = ChainObjectAnimator;
            }

            return dungeonDoorSubsidiaryBlocker;
        }

        private static tk2dSpriteAnimator GenerateChainObject(string name, GameObject parent, tk2dSpriteCollectionData parentCollection,DungeonDoorSubsidiaryBlocker dungeonDoorSubsidiaryBlocker ,string[] idleSpritePaths, string[] sealSpritePaths, string[] unsealSpritePaths, string[] playerNearSealedDoorAnimPaths = null, int idleAnimFPS = 5, int sealAnimFPS = 5, int unsealAnimFPS = 5, int playerNearSealedDoorAnimFPS = 5)
        {
            GameObject gameObject = SpriteBuilder.SpriteFromResource(idleSpritePaths[0], null, false);
            FakePrefab.MarkAsFakePrefab(gameObject);
            gameObject.name = name + "Chain";
       

            tk2dSpriteCollectionData SpriteObjectSpriteCollection = parentCollection;
            int spriteID = SpriteBuilder.AddSpriteToCollection(idleSpritePaths[0], SpriteObjectSpriteCollection);
            tk2dSprite sprite = gameObject.GetOrAddComponent<tk2dSprite>();
            sprite.SetSprite(SpriteObjectSpriteCollection, spriteID);


            tk2dSpriteAnimator animator = gameObject.GetOrAddComponent<tk2dSpriteAnimator>();
            tk2dSpriteAnimation animation = gameObject.AddComponent<tk2dSpriteAnimation>();

            animation.clips = new tk2dSpriteAnimationClip[0];
            animator.Library = animation;

        
            List<tk2dSpriteAnimationClip> clips = new List<tk2dSpriteAnimationClip>();


            if (idleSpritePaths.Length >= 1)
            {
                tk2dSpriteAnimationClip idleClip = new tk2dSpriteAnimationClip() { name = "idle", frames = new tk2dSpriteAnimationFrame[0], fps = idleAnimFPS };
                List<tk2dSpriteAnimationFrame> frames = new List<tk2dSpriteAnimationFrame>();
                for (int i = 0; i < idleSpritePaths.Length; i++)
                {
                    tk2dSpriteCollectionData collection = SpriteObjectSpriteCollection;
                    int frameSpriteId = SpriteBuilder.AddSpriteToCollection(idleSpritePaths[i], collection);
                    tk2dSpriteDefinition frameDef = collection.spriteDefinitions[frameSpriteId];
                    frames.Add(new tk2dSpriteAnimationFrame { spriteId = frameSpriteId, spriteCollection = collection });
                    frameDef.ConstructOffsetsFromAnchor(tk2dBaseSprite.Anchor.LowerLeft);
                }
                idleClip.frames = frames.ToArray();
                idleClip.wrapMode = tk2dSpriteAnimationClip.WrapMode.Loop;
                animator.Library.clips = animation.clips.Concat(new tk2dSpriteAnimationClip[] { idleClip }).ToArray();
                animator.playAutomatically = true;
                animator.DefaultClipId = animator.GetClipIdByName("idle");
                clips.Add(idleClip);

                tk2dSpriteAnimationClip[] array = clips.ToArray();
                animator.Library.clips = array;
                animator.playAutomatically = true;
                animator.DefaultClipId = animator.GetClipIdByName("idle");

            }


            if (sealSpritePaths.Length >= 1)
            {
                tk2dSpriteAnimationClip sealClip = new tk2dSpriteAnimationClip() { name = "chainseal", frames = new tk2dSpriteAnimationFrame[0], fps = sealAnimFPS };
                List<tk2dSpriteAnimationFrame> sealFrames = new List<tk2dSpriteAnimationFrame>();
                for (int i = 0; i < sealSpritePaths.Length; i++)
                {
                    tk2dSpriteCollectionData collection = SpriteObjectSpriteCollection;
                    int frameSpriteId = SpriteBuilder.AddSpriteToCollection(sealSpritePaths[i], collection);
                    tk2dSpriteDefinition frameDef = collection.spriteDefinitions[frameSpriteId];
                    sealFrames.Add(new tk2dSpriteAnimationFrame { spriteId = frameSpriteId, spriteCollection = collection });
                    frameDef.ConstructOffsetsFromAnchor(tk2dBaseSprite.Anchor.LowerLeft);
                }
                sealClip.frames = sealFrames.ToArray();
                sealClip.wrapMode = tk2dSpriteAnimationClip.WrapMode.Once;
                animator.Library.clips = animation.clips.Concat(new tk2dSpriteAnimationClip[] { sealClip }).ToArray();
                clips.Add(sealClip);
                dungeonDoorSubsidiaryBlocker.sealChainAnimationName = "chainseal";
            }

            if (unsealSpritePaths.Length >= 1)
            {
                tk2dSpriteAnimationClip unsealClip = new tk2dSpriteAnimationClip() { name = "chainunseal", frames = new tk2dSpriteAnimationFrame[0], fps = unsealAnimFPS };
                List<tk2dSpriteAnimationFrame> unsealFrames = new List<tk2dSpriteAnimationFrame>();
                for (int i = 0; i < unsealSpritePaths.Length; i++)
                {
                    tk2dSpriteCollectionData collection = SpriteObjectSpriteCollection;
                    int frameSpriteId = SpriteBuilder.AddSpriteToCollection(unsealSpritePaths[i], collection);
                    tk2dSpriteDefinition frameDef = collection.spriteDefinitions[frameSpriteId];
                    unsealFrames.Add(new tk2dSpriteAnimationFrame { spriteId = frameSpriteId, spriteCollection = collection });
                    frameDef.ConstructOffsetsFromAnchor(tk2dBaseSprite.Anchor.LowerLeft);
                }
                unsealClip.frames = unsealFrames.ToArray();
                unsealClip.wrapMode = tk2dSpriteAnimationClip.WrapMode.Once;
                animator.Library.clips = animation.clips.Concat(new tk2dSpriteAnimationClip[] { unsealClip }).ToArray();
                clips.Add(unsealClip);
                dungeonDoorSubsidiaryBlocker.unsealChainAnimationName = "chainunseal";
            }

            if (playerNearSealedDoorAnimPaths.Length >= 1 && playerNearSealedDoorAnimPaths != null)
            {
                tk2dSpriteAnimationClip playernearblockerClip = new tk2dSpriteAnimationClip() { name = "chainplayernearblocker", frames = new tk2dSpriteAnimationFrame[0], fps = playerNearSealedDoorAnimFPS };
                List<tk2dSpriteAnimationFrame> playernearblockerFrames = new List<tk2dSpriteAnimationFrame>();
                for (int i = 0; i < playerNearSealedDoorAnimPaths.Length; i++)
                {
                    tk2dSpriteCollectionData collection = SpriteObjectSpriteCollection;
                    int frameSpriteId = SpriteBuilder.AddSpriteToCollection(playerNearSealedDoorAnimPaths[i], collection);
                    tk2dSpriteDefinition frameDef = collection.spriteDefinitions[frameSpriteId];
                    playernearblockerFrames.Add(new tk2dSpriteAnimationFrame { spriteId = frameSpriteId, spriteCollection = collection });
                    frameDef.ConstructOffsetsFromAnchor(tk2dBaseSprite.Anchor.LowerLeft);
                }
                playernearblockerClip.frames = playernearblockerFrames.ToArray();
                playernearblockerClip.wrapMode = tk2dSpriteAnimationClip.WrapMode.Loop;
                animator.Library.clips = animation.clips.Concat(new tk2dSpriteAnimationClip[] { playernearblockerClip }).ToArray();
                clips.Add(playernearblockerClip);
                dungeonDoorSubsidiaryBlocker.playerNearChainAnimationName = "chainplayernearblocker";
            }
            gameObject.transform.parent = parent.transform;
            return animator;
        }

        /// <summary>
        /// Unfinished, do not use!
        /// </summary>
        private static DungeonDoorController GenerateDungeonDoorController(string name, DungeonDoorController.DoorModule[] doorModules, DungeonDoorController.DungeonDoorMode dungeonDoorMode, bool IsNorthSouthDoor, bool hasSubsidiaryDoors, bool hidesSealAnimators, DungeonDoorSubsidiaryBlocker blocker = null, bool isLocked = false)
        {
            DungeonDoorController controller = new DungeonDoorController();
            controller.name = name;
            controller.doorModules = doorModules;
            controller.messageToDisplay = "what";
            controller.northSouth = IsNorthSouthDoor;
            controller.SupportsSubsidiaryDoors = hasSubsidiaryDoors;
            controller.hideSealAnimators = hidesSealAnimators;
            

            controller.isLocked = isLocked;

            if (blocker != null)
            {
                controller.sealAnimators = new tk2dSpriteAnimator[] { blocker.sealAnimator };
                controller.sealAnimationName = blocker.sealAnimationName;
                controller.unsealAnimationName = blocker.sealAnimationName;
                controller.sealAnimationName = blocker.sealAnimationName;

            }

            //controller.exitDefinition
            //controller.messageTransformPoint = GenerateTransformObject().transform;
            return controller;
        }

        /// <summary>
        /// Unfinished, do not use!
        /// </summary>
        private static DungeonDoorController.DoorModule GenerateDoorModule(string name, string[] idleSpritePaths, string[] closeAnimPaths, string[] openAimPaths, int AnimFPS = 5)
        {
            GameObject gameObject = SpriteBuilder.SpriteFromResource(idleSpritePaths[0], null, false);
            FakePrefab.MarkAsFakePrefab(gameObject);
            gameObject.name = name;

            tk2dSpriteCollectionData SpriteObjectSpriteCollection = SpriteBuilder.ConstructCollection(gameObject, (name + "_Collection"));
            int spriteID = SpriteBuilder.AddSpriteToCollection(idleSpritePaths[0], SpriteObjectSpriteCollection);
            tk2dSprite sprite = gameObject.GetOrAddComponent<tk2dSprite>();
            sprite.SetSprite(SpriteObjectSpriteCollection, spriteID);

            tk2dSpriteAnimator animator = gameObject.GetOrAddComponent<tk2dSpriteAnimator>();
            tk2dSpriteAnimation animation = gameObject.AddComponent<tk2dSpriteAnimation>();
            animation.clips = new tk2dSpriteAnimationClip[0];
            animator.Library = animation;

            List<tk2dSpriteAnimationClip> clips = new List<tk2dSpriteAnimationClip>();
            if (idleSpritePaths.Length > 0)
            {
                tk2dSpriteAnimationClip idleClip = new tk2dSpriteAnimationClip() { name = "idle", frames = new tk2dSpriteAnimationFrame[0], fps = AnimFPS };
                List<tk2dSpriteAnimationFrame> frames = new List<tk2dSpriteAnimationFrame>();
                for (int i = 0; i < idleSpritePaths.Length; i++)
                {
                    tk2dSpriteCollectionData collection = SpriteObjectSpriteCollection;
                    int frameSpriteId = SpriteBuilder.AddSpriteToCollection(idleSpritePaths[i], collection);
                    tk2dSpriteDefinition frameDef = collection.spriteDefinitions[frameSpriteId];
                    frames.Add(new tk2dSpriteAnimationFrame { spriteId = frameSpriteId, spriteCollection = collection });
                    frameDef.ConstructOffsetsFromAnchor(tk2dBaseSprite.Anchor.LowerLeft);
                }
                idleClip.frames = frames.ToArray();
                idleClip.wrapMode = tk2dSpriteAnimationClip.WrapMode.Loop;
                animator.Library.clips = animation.clips.Concat(new tk2dSpriteAnimationClip[] { idleClip }).ToArray();
                animator.playAutomatically = true;
                animator.DefaultClipId = animator.GetClipIdByName("idle");
                clips.Add(idleClip);
                tk2dSpriteAnimationClip[] array = clips.ToArray();
                animator.Library.clips = array;
                animator.playAutomatically = true;
                animator.DefaultClipId = animator.GetClipIdByName("idle");
            }
            if (closeAnimPaths.Length > 0)
            {
                clips.Add(AddAnimation("close", animator, animation, 5, closeAnimPaths, SpriteObjectSpriteCollection, tk2dSpriteAnimationClip.WrapMode.Once));
                tk2dSpriteAnimationClip[] array = clips.ToArray();
                animator.Library.clips = array;
            }
            if (closeAnimPaths.Length > 0)
            {
                clips.Add(AddAnimation("open", animator, animation, 5, openAimPaths, SpriteObjectSpriteCollection, tk2dSpriteAnimationClip.WrapMode.Once));
                tk2dSpriteAnimationClip[] array = clips.ToArray();
                animator.Library.clips = array;
            }


            DungeonDoorController.DoorModule mod = new DungeonDoorController.DoorModule();
            mod.animator = animator;
            mod.closedDepth = -1; //idk what this is for
            mod.closeAnimationName = "close";
            mod.openAnimationName = "open";
            
            gameObject.AddComponent(mod.GetType());
            return mod;
        }

        /// <summary>
        /// Generates, and returns a tk2dSpriteAnimationClip. Can be used to add new Tk2daAnimations to an object, as long as it has a tk2dSpriteAnimator.
        /// </summary>
        /// <param name="clipName">The name of your animation clip.</param>
        /// <param name="animator">Your objects tk2dSpriteAnimator.</param>
        /// <param name="animator">Your objects tk2dSpriteAnimation component.</param>
        /// <param name="FPS">Your animations FPS.</param>
        /// <param name="SpritePaths">All the spritepaths to your animation.</param>
        /// <param name="SpriteObjectSpriteCollection">Your objects SpriteObjectSpriteCollection. Can be gotten by getting the tk2dsprite of the object and accessing the Collection variable. Ex: gameObject.GetComponent<Tk2dBaseSprite>().Collection</param>
        /// <param name="wrapMode">Your animations wrap mode.</param>

        public static tk2dSpriteAnimationClip AddAnimation(string clipName,tk2dSpriteAnimator animator, tk2dSpriteAnimation animation, int FPS, string[] SpritePaths, tk2dSpriteCollectionData SpriteObjectSpriteCollection, tk2dSpriteAnimationClip.WrapMode wrapMode)
        {
            tk2dSpriteAnimationClip clip = new tk2dSpriteAnimationClip() { name = clipName, frames = new tk2dSpriteAnimationFrame[0], fps = FPS };
            List<tk2dSpriteAnimationFrame> frames = new List<tk2dSpriteAnimationFrame>();
            for (int i = 0; i < SpritePaths.Length; i++)
            {
                tk2dSpriteCollectionData collection = SpriteObjectSpriteCollection;
                int frameSpriteId = SpriteBuilder.AddSpriteToCollection(SpritePaths[i], collection);
                tk2dSpriteDefinition frameDef = collection.spriteDefinitions[frameSpriteId];
                frames.Add(new tk2dSpriteAnimationFrame { spriteId = frameSpriteId, spriteCollection = collection });
                frameDef.ConstructOffsetsFromAnchor(tk2dBaseSprite.Anchor.LowerLeft);
            }
            clip.frames = frames.ToArray();
            clip.wrapMode = wrapMode;
            animator.Library.clips = animation.clips.Concat(new tk2dSpriteAnimationClip[] { clip }).ToArray();
            return clip;
        }

        /// <summary>
        /// Generates, and returns a simple GameObject with a sprite / animation.
        /// </summary>
        /// <param name="name">Your objects name.</param>
        /// <param name="SpritePaths">Your spritepath(s) to your decal.</param>
        /// <param name="AnimFPS">The FPS of the animation, if it will have one.</param>

        public static GameObject GenerateDecalObject(string name, string[] SpritePaths, int AnimFPS = 5)
        {
            GameObject gameObject = SpriteBuilder.SpriteFromResource(SpritePaths[0], null, false);
            FakePrefab.MarkAsFakePrefab(gameObject);
            gameObject.name = name;
            gameObject.layer = 20;

            tk2dSpriteCollectionData SpriteObjectSpriteCollection = SpriteBuilder.ConstructCollection(gameObject, (name + "_Collection"));
            int spriteID = SpriteBuilder.AddSpriteToCollection(SpritePaths[0], SpriteObjectSpriteCollection);
            tk2dSprite sprite = gameObject.GetOrAddComponent<tk2dSprite>();
            sprite.SetSprite(SpriteObjectSpriteCollection, spriteID);
            sprite.usesOverrideMaterial = true;
            sprite.renderer.material = new Material(ShaderCache.Acquire("Brave/LitTk2dCustomFalloffTilted"));

            tk2dSpriteAnimator animator = gameObject.GetOrAddComponent<tk2dSpriteAnimator>();
            tk2dSpriteAnimation animation = gameObject.AddComponent<tk2dSpriteAnimation>();
            animation.clips = new tk2dSpriteAnimationClip[0];
            animator.Library = animation;

            List<tk2dSpriteAnimationClip> clips = new List<tk2dSpriteAnimationClip>();
            if (SpritePaths.Length >= 1)
            {
                tk2dSpriteAnimationClip idleClip = new tk2dSpriteAnimationClip() { name = "idle", frames = new tk2dSpriteAnimationFrame[0], fps = AnimFPS };
                List<tk2dSpriteAnimationFrame> frames = new List<tk2dSpriteAnimationFrame>();
                for (int i = 0; i < SpritePaths.Length; i++)
                {
                    tk2dSpriteCollectionData collection = SpriteObjectSpriteCollection;
                    int frameSpriteId = SpriteBuilder.AddSpriteToCollection(SpritePaths[i], collection);
                    tk2dSpriteDefinition frameDef = collection.spriteDefinitions[frameSpriteId];
                    frames.Add(new tk2dSpriteAnimationFrame { spriteId = frameSpriteId, spriteCollection = collection });
                    frameDef.ConstructOffsetsFromAnchor(tk2dBaseSprite.Anchor.LowerLeft);
                }
                idleClip.frames = frames.ToArray();
                idleClip.wrapMode = tk2dSpriteAnimationClip.WrapMode.Loop;
                animator.Library.clips = animation.clips.Concat(new tk2dSpriteAnimationClip[] { idleClip }).ToArray();
                animator.playAutomatically = true;
                animator.DefaultClipId = animator.GetClipIdByName("idle");
                clips.Add(idleClip);
                tk2dSpriteAnimationClip[] array = clips.ToArray();
                animator.Library.clips = array;
                animator.playAutomatically = true;
                animator.DefaultClipId = animator.GetClipIdByName("idle");
            }
            return gameObject;
        }
    



        /// <summary>
        /// Generates, and returns a KickableObject. This is for generating a basic one, it returns it so you can additionally modify it without cluttering up the setup method too much. Reminder, KickableObjects have a MinorBreakable component that you could modify as well!
        /// </summary>
        /// <param name="name">The name of your kickable. Keep it simple, its used in generating your animations, so no special characters.</param>
        /// <param name="idleSpritePaths">Your sprite paths. Only insert one path if you don't want it to be animated.</param>
        /// <param name="rollNorthPaths">The sprite paths for the animation for when it rolls NORTH.</param>
        /// <param name="rollSouthPaths">The sprite paths for the animation for when it rolls SOUTH.</param>
        /// <param name="rollEastPaths">The sprite paths for the animation for when it rolls EAST.</param>
        /// <param name="rollWestPaths">The sprite paths for the animation for when it rolls WEST.</param>
        /// <param name="impactNorthPaths">The sprite paths for the animation for when it is broken during a rolling animation state. This one is for when its facing NORTH.</param>
        /// <param name="impactSouthPaths">The sprite paths for the animation for when it is broken during a rolling animation state. This one is for when its facing SOUTH.</param>
        /// <param name="impactEastPaths">The sprite paths for the animation for when it is broken during a rolling animation state. This one is for when its facing EAST.</param>
        /// <param name="impactWestPaths">The sprite paths for the animation for when it is broken during a rolling animation state. This one is for when its facing WEST.</param>
        /// <param name="impactNotRollingPaths">The sprite paths for the animation for when it is broken before it has been kicked.</param>
        /// <param name="RolledIntoBreakPaths">The sprite paths for the animation for when it is broken WHEN the player DODGEROLLS into the kickable.</param>
        /// <param name="idleAnimFPS">The FPS of your idle animation.</param>
        /// <param name="rollAnimFPS">The FPS of your all your rolling animations. No, I will not add support for each direction having its own FPS, fuck off and fuck you.</param>
        /// <param name="breakAnimFPS">The FPS of your all your breaking animations. No, I will not add support for each direction having its own FPS, again, fuck off and fuck you.</param>
        /// <param name="breakNotRollingFPS">The FPS of your broken-before-kicked animation.</param>
        /// <param name="breakRolledIntoFPS">The FPS of your broken-when-dodgerolled-into animation.</param>
        /// <param name="UsesCustomColliderValues">Setting this to true will let you use custom collider sizes and offsets. Keeping it false will use no offsets and generate a size based on the sprites size.</param>
        /// <param name="ColliderSizeX">The X Value of your collider. Only used if UsesCustomColliderValues is true. Note that 1 here means 1 *pixel*, and not 1 tile!</param>
        /// <param name="ColliderSizeY">The Y Value of your collider. Only used if UsesCustomColliderValues is true. Note that 1 here means 1 *pixel*, and not 1 tile!</param>
        /// <param name="ColliderOffsetX">The X Value of your colliders offset. Only used if UsesCustomColliderValues is true.</param>
        /// <param name="ColliderOffsetY">The Y Value of your colliders offset. Only used if UsesCustomColliderValues is true.</param>
        /// <param name="HasAdditionalCollisions">If true, adds a BulletBlocker, EnemyBlocker and PlayerBlocker CollisionLayer to your kickable .</param>
        /// <param name="AdditionalCollisionsUseColliderSizes">If true, uses the collider sizes and offsets that you give later, else it will use the same sizes as given previously.</param>
        /// <param name="AdiitionalColliderSizeX">The X Value of your additional colliders. Only used if AdditionalCollisionsUseColliderSizes is true. Note that 1 here means 1 *pixel*, and not 1 tile!</param>
        /// <param name="AdiitionalColliderSizeY">The Y Value of your additional colliders. Only used if AdditionalCollisionsUseColliderSizes is true. Note that 1 here means 1 *pixel*, and not 1 tile!</param>
        /// <param name="AdiitionalColliderOffsetX">The X offset of your additional colliders. Only used if AdditionalCollisionsUseColliderSizes is true. Note that 1 here means 1 *pixel*, and not 1 tile!</param>
        /// <param name="AdiitionalColliderSizeY">The Y offset of your additional colliders. Only used if AdditionalCollisionsUseColliderSizes is true. Note that 1 here means 1 *pixel*, and not 1 tile!</param>
        /// <param name="breakAudioEvent">The name of the sound that plays when your kickable is broken.</param>
        /// <param name="rollingSpeed">The speed at which your kickable moves.</param>
        /// <param name="collisionLayerList">Sets the collision layer/s of the MajorBreakable. leaving this as null will set it to HighObstacle AND BulletBlocker, however basegame MajorBreakables can use different ones, and at times multiple at once.</param>

        public static KickableObject GenerateKickableObject(string name, string[] idleSpritePaths, string[] rollNorthPaths, string[] rollSouthPaths, string[] rollEastPaths, string[] rollWestPaths, string[] impactNorthPaths, string[] impactSouthPaths, string[] impactEastPaths, string[] impactWestPaths,string[] impactNotRollingPaths, string[]RolledIntoBreakPaths, int idleAnimFPS = 4,int rollAnimFPS = 5, int breakAnimFPS = 4, int breakNotRollingFPS = 4, int breakRolledIntoFPS = 4,bool UsesCustomColliderValues = false, int ColliderSizeX = 16, int ColliderSizeY = 8, int ColliderOffsetX = 0, int ColliderOffsetY = 8, bool HasAdditionalCollisions = true, bool AdditionalCollisionsUseColliderSizes = true, int AdiitionalColliderSizeX = 8, int AdiitionalColliderSizeY = 8, int AdiitionalColliderOffsetX = 0, int AdiitionalColliderOffsetY = 0, string breakAudioEvent = "Play_OBJ_barrel_break_01", float rollingSpeed = 3, List<CollisionLayer> collisionLayerList = null, bool usesWorldShader = true)
        {
            Texture2D textureFromResource = ResourceExtractor.GetTextureFromResource(idleSpritePaths[0]);
            GameObject gameObject = SpriteBuilder.SpriteFromResource(idleSpritePaths[0], null, false);
            FakePrefab.MarkAsFakePrefab(gameObject);
            gameObject.name = name;
            KickableObject kickable = gameObject.AddComponent<KickableObject>();
            MinorBreakable breakable = gameObject.AddComponent<MinorBreakable>();

            tk2dSpriteCollectionData KickableSpriteCollection = SpriteBuilder.ConstructCollection(gameObject, (name + "_Collection"));
            int spriteID = SpriteBuilder.AddSpriteToCollection(idleSpritePaths[0], KickableSpriteCollection);
            tk2dSprite sprite = gameObject.GetOrAddComponent<tk2dSprite>();
            sprite.SetSprite(KickableSpriteCollection, spriteID);

            IntVector2 intVector = new IntVector2(ColliderSizeX, ColliderSizeY);
            IntVector2 colliderOffset = new IntVector2(ColliderOffsetX, ColliderOffsetY);
            IntVector2 colliderSize = new IntVector2(intVector.x, intVector.y);

            IntVector2 intVectorAdditional = new IntVector2(AdiitionalColliderSizeX, AdiitionalColliderSizeY);
            IntVector2 AdditionalcolliderSize = new IntVector2(intVectorAdditional.x, intVectorAdditional.y);
            IntVector2 AdditionalcolliderOffset = new IntVector2(AdiitionalColliderOffsetX, AdiitionalColliderOffsetY);


            if (UsesCustomColliderValues == false)
            {
                IntVector2 nonCustomintVector = new IntVector2(textureFromResource.width, textureFromResource.height);
                colliderSize = new IntVector2(nonCustomintVector.x, nonCustomintVector.y);
            }
            if (AdditionalCollisionsUseColliderSizes == true)
            {
                AdditionalcolliderSize = colliderSize;
                AdditionalcolliderOffset = colliderOffset;
            }

            SpeculativeRigidbody speculativeRigidbody = sprite.SetUpEmptySpeculativeRigidbody(colliderOffset, colliderSize);
            if (collisionLayerList == null)
            {
                speculativeRigidbody.PixelColliders.Add(new PixelCollider
                {
                    ColliderGenerationMode = PixelCollider.PixelColliderGeneration.Manual,
                    CollisionLayer = CollisionLayer.HighObstacle,
                    IsTrigger = false,
                    BagleUseFirstFrameOnly = false,
                    SpecifyBagelFrame = string.Empty,
                    BagelColliderNumber = 0,
                    ManualOffsetX = colliderOffset.x,
                    ManualOffsetY = colliderOffset.y,
                    ManualWidth = colliderSize.x,
                    ManualHeight = colliderSize.y,
                    ManualDiameter = 0,
                    ManualLeftX = 0,
                    ManualLeftY = 0,
                    ManualRightX = 0,
                    ManualRightY = 0,
                });
                

            }
            else
            {
                foreach (CollisionLayer layer in collisionLayerList)
                {
                    speculativeRigidbody.PixelColliders.Add(new PixelCollider
                    {
                        ColliderGenerationMode = PixelCollider.PixelColliderGeneration.Manual,
                        CollisionLayer = layer,
                        IsTrigger = false,
                        BagleUseFirstFrameOnly = false,
                        SpecifyBagelFrame = string.Empty,
                        BagelColliderNumber = 0,
                        ManualOffsetX = colliderOffset.x,
                        ManualOffsetY = colliderOffset.y,
                        ManualWidth = colliderSize.x,
                        ManualHeight = colliderSize.y,
                        ManualDiameter = 0,
                        ManualLeftX = 0,
                        ManualLeftY = 0,
                        ManualRightX = 0,
                        ManualRightY = 0,
                    });
                }
            }
            tk2dSpriteAnimator animator = gameObject.GetOrAddComponent<tk2dSpriteAnimator>();
            tk2dSpriteAnimation animation = gameObject.AddComponent<tk2dSpriteAnimation>();
            animation.clips = new tk2dSpriteAnimationClip[0];
            animator.Library = animation;

            List<tk2dSpriteAnimationClip> clips = new List<tk2dSpriteAnimationClip>();
            if (idleSpritePaths.Length >= 1)
            {
                tk2dSpriteAnimationClip idleClip = new tk2dSpriteAnimationClip() { name = "idle", frames = new tk2dSpriteAnimationFrame[0], fps = idleAnimFPS };
                List<tk2dSpriteAnimationFrame> frames = new List<tk2dSpriteAnimationFrame>();
                for (int i = 0; i < idleSpritePaths.Length; i++)
                {
                    tk2dSpriteCollectionData collection = KickableSpriteCollection;
                    int frameSpriteId = SpriteBuilder.AddSpriteToCollection(idleSpritePaths[i], collection);
                    tk2dSpriteDefinition frameDef = collection.spriteDefinitions[frameSpriteId];
                    frameDef.ConstructOffsetsFromAnchor(tk2dBaseSprite.Anchor.LowerLeft);
                    frames.Add(new tk2dSpriteAnimationFrame { spriteId = frameSpriteId, spriteCollection = collection });
                }
                idleClip.frames = frames.ToArray();
                idleClip.wrapMode = tk2dSpriteAnimationClip.WrapMode.Loop;
                clips.Add(idleClip);
            }
               
            tk2dSpriteAnimationClip rollNorth = AddAnimation(animator, KickableSpriteCollection, rollNorthPaths, name + "_roll_north", rollAnimFPS, tk2dSpriteAnimationClip.WrapMode.Loop);
            tk2dSpriteAnimationClip rollSouth = AddAnimation(animator, KickableSpriteCollection, rollSouthPaths, name + "_roll_south", rollAnimFPS, tk2dSpriteAnimationClip.WrapMode.Loop);
            tk2dSpriteAnimationClip rollEast = AddAnimation(animator, KickableSpriteCollection, rollEastPaths, name + "_roll_east", rollAnimFPS, tk2dSpriteAnimationClip.WrapMode.Loop);
            tk2dSpriteAnimationClip rollWest = AddAnimation(animator, KickableSpriteCollection, rollWestPaths, name + "_roll_west", rollAnimFPS, tk2dSpriteAnimationClip.WrapMode.Loop);
            kickable.rollAnimations = new string[]
            {
                name + "_roll_north",
                name + "_roll_west",
                name + "_roll_south",
                name + "_roll_east",
            };
            clips.AddRange(new List<tk2dSpriteAnimationClip>() { rollNorth, rollSouth, rollEast, rollWest });
            tk2dSpriteAnimationClip impactNorth = AddAnimation(animator, KickableSpriteCollection, impactNorthPaths, name + "_impact_north", breakAnimFPS, tk2dSpriteAnimationClip.WrapMode.Once);
            tk2dSpriteAnimationClip impactSouth = AddAnimation(animator, KickableSpriteCollection, impactSouthPaths, name + "_impact_south", breakAnimFPS, tk2dSpriteAnimationClip.WrapMode.Once);
            tk2dSpriteAnimationClip impactEast = AddAnimation(animator, KickableSpriteCollection, impactEastPaths, name + "_impact_east", breakAnimFPS, tk2dSpriteAnimationClip.WrapMode.Once);
            tk2dSpriteAnimationClip impactWest = AddAnimation(animator, KickableSpriteCollection, impactWestPaths, name + "_impact_west", breakAnimFPS, tk2dSpriteAnimationClip.WrapMode.Once);
            kickable.impactAnimations = new string[]
            {
                name + "_impact_north",
                name + "_impact_west",
                name + "_impact_south",
                name + "_impact_east",
            };
            clips.AddRange(new List<tk2dSpriteAnimationClip>() { impactNorth, impactSouth, impactEast, impactWest });
            tk2dSpriteAnimationClip breakNotRolling = AddAnimation(animator, KickableSpriteCollection, impactNotRollingPaths, name + "_impact_nonroll", breakNotRollingFPS, tk2dSpriteAnimationClip.WrapMode.Once);
            clips.Add(breakNotRolling);

            tk2dSpriteAnimationClip rolledInto = AddAnimation(animator, KickableSpriteCollection, RolledIntoBreakPaths, name + "_rolled_into", breakRolledIntoFPS, tk2dSpriteAnimationClip.WrapMode.Once);
            clips.Add(rolledInto);

            kickable.RollingBreakAnim = name + "_rolled_into";
            breakable.breakAnimName = name + "_impact_nonroll";

            tk2dSpriteAnimationClip[] array = clips.ToArray();
            animator.Library.clips = array;
            animator.playAutomatically = true;
            animator.DefaultClipId = animator.GetClipIdByName("idle");
            breakable.breakAudioEventName = breakAudioEvent;

            kickable.sprite = sprite;
            kickable.spriteAnimator = animator;

            kickable.AllowTopWallTraversal = true; 
            kickable.rollSpeed = rollingSpeed;

            if (usesWorldShader == true)
            {
                //kickable.sprite.renderer.material.shader = worldShader;
                //kickable.sprite.renderer.material.EnableKeyword("BRIGHTNESS_CLAMP_ON");

            }
            return kickable; 
        }
        private static tk2dSpriteAnimationClip AddAnimation(tk2dSpriteAnimator animator, tk2dSpriteCollectionData Tablecollection, string[] spritePaths, string clipName, int FPS, tk2dSpriteAnimationClip.WrapMode wrapMode)
        {
            tk2dSpriteAnimation animation = animator.gameObject.AddComponent<tk2dSpriteAnimation>();
            animation.clips = new tk2dSpriteAnimationClip[0];
            animator.Library = animation;
            tk2dSpriteAnimationClip idleClip = new tk2dSpriteAnimationClip() { name = clipName, frames = new tk2dSpriteAnimationFrame[0], fps = FPS };
            List<tk2dSpriteAnimationFrame> frames = new List<tk2dSpriteAnimationFrame>();
            for (int i = 0; i < spritePaths.Length; i++)
            {
                tk2dSpriteCollectionData collection = Tablecollection;
                int frameSpriteId = SpriteBuilder.AddSpriteToCollection(spritePaths[i], collection);
                tk2dSpriteDefinition frameDef = collection.spriteDefinitions[frameSpriteId];
                frames.Add(new tk2dSpriteAnimationFrame { spriteId = frameSpriteId, spriteCollection = collection });
            }
            idleClip.frames = frames.ToArray();
            idleClip.wrapMode = wrapMode;
            return idleClip;
        }

        /// <summary>
        /// (Unstable, no guaranteed good results!) Generates, and returns a FlippableCover. This is for generating a basic one, it returns it so you can additionally modify it without cluttering up the setup method too much. Reminder, FlippableCovers have a MajorBreakable component that you could modify as well!
        /// </summary>
        /// <param name="name">The name of your kickable. Keep it simple, its used in generating your animations, so no special characters.</param>
        /// <param name="idleSpritePaths">Your sprite paths. Only insert one path if you don't want it to be animated.</param>
        /// <param name="outlinePaths">Your sprite paths for the *outlines* that appear when you are nearby a table. Of note, your array of path should be in a SPECIFIC order, with North being 1st, East being 2nd, West being 3rd and South being forth.</param>
        /// <param name="northFlipPaths">Your sprite paths for the flip animations facing NORTH.</param>
        /// <param name="southFlipPaths">Your sprite paths for the flip animations facing SOUTH.</param>
        /// <param name="eastFlipPaths">Your sprite paths for the flip animations facing EAST.</param>
        /// <param name="westFlipPaths">Your sprite paths for the flip animations facing WEST.</param>
        /// <param name="northBreakPaths">Your sprite paths for the flipped break animations facing NORTH.</param>
        /// <param name="southBreakPaths">Your sprite paths for the flipped break animations facing SOUTH.</param>
        /// <param name="eastBreakPaths">Your sprite paths for the flipped break animations facing EAST.</param>
        /// <param name="westBreakPaths">Your sprite paths for the flipped break animations facing WEST.</param>
        /// <param name="unflippedBreakPaths">Your sprite paths for the break animations when the table has NOT been flipped yet.</param>
        /// <param name="IdleFPS">The FPS of your idle animation.</param>
        /// <param name="FlipFPS">The FPS of all your flip animations.</param>
        /// <param name="BreakFPS">The FPS of all your break-while-flipped animations.</param>
        /// <param name="UnflippedBreakFPS">The FPS of your break-while-unflipped animation.</param>
        /// <param name="UsesCustomColliderValues">Setting this to true will let you use custom collider sizes and offsets. Keeping it false will use no offsets and generate a size based on the sprites size.</param>
        /// <param name="ColliderSizeX">The X Value of your collider. Only used if UsesCustomColliderValues is true. Note that 1 here means 1 *pixel*, and not 1 tile!</param>
        /// <param name="ColliderSizeY">The Y Value of your collider. Only used if UsesCustomColliderValues is true. Note that 1 here means 1 *pixel*, and not 1 tile!</param>
        /// <param name="ColliderOffsetX">The X Value of your colliders offset. Only used if UsesCustomColliderValues is true.</param>
        /// <param name="ColliderOffsetY">The Y Value of your colliders offset. Only used if UsesCustomColliderValues is true.</param>
        /// <param name="FlippedColliderSizeX_Horizontal">The X Value of your collider for when the table is flipped NORTH or SOUTH. My code *should* automatically place the hit box appropriate to the edge of the table. Note that 1 here means 1 *pixel*, and not 1 tile!</param>
        /// <param name="FlippedColliderSizeY_Horizontal">The Y Value of your collider for when the table is flipped NORTH or SOUTH. My code *should* automatically place the hit box appropriate to the edge of the table. Note that 1 here means 1 *pixel*, and not 1 tile!</param>
        /// <param name="FlippedColliderSizeX_Vertical">The X Value of your collider for when the table is flipped EAST or WEST. My code *should* automatically place the hit box appropriate to the edge of the table. Note that 1 here means 1 *pixel*, and not 1 tile!</param>
        /// <param name="FlippedColliderSizeY_Vertical">The Y Value of your collider for when the table is flipped EAST or WEST. My code *should* automatically place the hit box appropriate to the edge of the table. Note that 1 here means 1 *pixel*, and not 1 tile!</param>
        /// <param name="flipStyle">The directions in which your table is able to be flipped.</param>
        /// <param name="TableHP">The amount of HP your table has. Breaks when its HP reaches 0.</param>
        /// <param name="ShadowSpritePath">currently does nothing. leave it as null.</param>
        /// <param name="breakAnimPercentages_AND_SpritePathsandBreakDirectionsNorth">(NOTE: SPRITE NAME MUST INCLUDE LOWERCASE NORTH) Switches the tables flipped sprite to one given when its at a certain percentage of HP. The percentage should be a value like 50 if you want it to switch at 50% HP. The string you have to give is a SPRITE PATH to the sprite you want it to switch to, and the ENUM you set to a direction is to set what DIRECTION that sprite is for./param>
        /// <param name="breakAnimPercentages_AND_SpritePathsandBreakDirectionsSouth">(NOTE: SPRITE NAME MUST INCLUDE LOWERCASE SOUTH) Switches the tables flipped sprite to one given when its at a certain percentage of HP. The percentage should be a value like 50 if you want it to switch at 50% HP. The string you have to give is a SPRITE PATH to the sprite you want it to switch to, and the ENUM you set to a direction is to set what DIRECTION that sprite is for./param>
        /// <param name="breakAnimPercentages_AND_SpritePathsandBreakDirectionsEast">(NOTE: SPRITE NAME MUST INCLUDE LOWERCASE EAST) Switches the tables flipped sprite to one given when its at a certain percentage of HP. The percentage should be a value like 50 if you want it to switch at 50% HP. The string you have to give is a SPRITE PATH to the sprite you want it to switch to, and the ENUM you set to a direction is to set what DIRECTION that sprite is for./param>
        /// <param name="breakAnimPercentages_AND_SpritePathsandBreakDirectionsWest">(NOTE: SPRITE NAME MUST INCLUDE LOWERCASE WEST) Switches the tables flipped sprite to one given when its at a certain percentage of HP. The percentage should be a value like 50 if you want it to switch at 50% HP. The string you have to give is a SPRITE PATH to the sprite you want it to switch to, and the ENUM you set to a direction is to set what DIRECTION that sprite is for./param>

        /// <param name="unflippedBreakAnimPercentagesAndSpritePaths">Switches the tables idle sprite to one given when its at a certain percentage of HP. The percentage should be a value like 50 if you want it to switch at 50% HP. The string you have to give is a SPRITE PATH to the sprite you want it to switch to./param>

        public static FlippableCover GenerateTable(string name, string[] idleSpritePaths, string[] outlinePaths, string[] northFlipPaths, string[] southFlipPaths, string[] eastFlipPaths, string[] westFlipPaths, string[] northBreakPaths, string[] southBreakPaths, string[] eastBreakPaths, string[] westBreakPaths, string[] unflippedBreakPaths, int IdleFPS = 4, int FlipFPS = 6, int BreakFPS = 7, int UnflippedBreakFPS = 5, bool UsesCustomColliderValues = false, int ColliderSizeX = 16, int ColliderSizeY = 8, int ColliderOffsetX = 0, int ColliderOffsetY = 8, int FlippedColliderSizeX_Horizontal = 20, int FlippedColliderSizeY_Horizontal = 2, int FlippedColliderSizeX_Vertical= 4, int FlippedColliderSizeY_Vertical = 8, FlippableCover.FlipStyle flipStyle = FlippableCover.FlipStyle.ANY, float TableHP = 90, string ShadowSpritePath = null,  
            Dictionary<float, string> breakAnimPercentages_AND_SpritePathsandBreakDirectionsNorth = null,
            Dictionary<float, string> breakAnimPercentages_AND_SpritePathsandBreakDirectionsSouth = null,
            Dictionary<float, string> breakAnimPercentages_AND_SpritePathsandBreakDirectionsEast = null,
            Dictionary<float, string> breakAnimPercentages_AND_SpritePathsandBreakDirectionsWest = null,
            Dictionary<float, string> unflippedBreakAnimPercentagesAndSpritePaths = null, bool IsSlideable = true, bool hasDecorations = true, float chanceToDecorateTable = 1)
        
        {
            Texture2D textureFromResource = ResourceExtractor.GetTextureFromResource(idleSpritePaths[0]);
            GameObject gameObject = SpriteBuilder.SpriteFromResource(idleSpritePaths[0], null, false);
            FakePrefab.MarkAsFakePrefab(gameObject);
            gameObject.name = name + "_Table";
            FlippableCover table = gameObject.AddComponent<FlippableCover>();
            MajorBreakable majorBreakable = gameObject.AddComponent<MajorBreakable>();

            tk2dSpriteCollectionData TableCollection = SpriteBuilder.ConstructCollection(gameObject, (name + "_Collection"));
            int spriteID = SpriteBuilder.AddSpriteToCollection(idleSpritePaths[0], TableCollection);
            tk2dSprite sprite = gameObject.GetOrAddComponent<tk2dSprite>();
            tk2dBaseSprite baseSprite = gameObject.GetOrAddComponent<tk2dBaseSprite>();
            baseSprite.SetSprite(TableCollection, spriteID);
            sprite.SetSprite(TableCollection, spriteID);

            baseSprite.HeightOffGround = 0;
            sprite.HeightOffGround = 0;
            gameObject.layer = 0;

            IntVector2 colliderOffset = new IntVector2(ColliderOffsetX, ColliderOffsetY);
            IntVector2 colliderSize = new IntVector2(ColliderSizeX, ColliderSizeY);
            if (UsesCustomColliderValues == false)
            {
                IntVector2 nonCustomintVector = new IntVector2(textureFromResource.width, textureFromResource.height);
                colliderSize = new IntVector2(nonCustomintVector.x, nonCustomintVector.y);
            }
            majorBreakable.HitPoints = TableHP;
            tk2dSpriteAnimator animator = gameObject.GetOrAddComponent<tk2dSpriteAnimator>();
            tk2dSpriteAnimation animation = gameObject.AddComponent<tk2dSpriteAnimation>();
            animation.clips = new tk2dSpriteAnimationClip[0];
            animator.Library = animation;

            List<tk2dSpriteAnimationClip> clips = new List<tk2dSpriteAnimationClip>();

            if (idleSpritePaths.Length >= 1)
            {
                tk2dSpriteAnimationClip idleClip = new tk2dSpriteAnimationClip() { name = "idle", frames = new tk2dSpriteAnimationFrame[0], fps = IdleFPS };
                List<tk2dSpriteAnimationFrame> frames = new List<tk2dSpriteAnimationFrame>();
                for (int i = 0; i < idleSpritePaths.Length; i++)
                {
                    tk2dSpriteCollectionData collection = TableCollection;
                    int frameSpriteId = SpriteBuilder.AddSpriteToCollection(idleSpritePaths[i], collection);
                    tk2dSpriteDefinition frameDef = collection.spriteDefinitions[frameSpriteId];
                    frameDef.ConstructOffsetsFromAnchor(tk2dBaseSprite.Anchor.LowerLeft);
                    tk2dSpriteDefinition frameDefMod = GenerateColliderForSpriteDefinition(frameDef, new Vector3(colliderSize.x, colliderSize.y), new Vector3(colliderOffset.x, colliderOffset.y));
                    frameDef = frameDefMod;
                    frames.Add(new tk2dSpriteAnimationFrame { spriteId = frameSpriteId, spriteCollection = collection });
                }
                idleClip.frames = frames.ToArray();
                idleClip.wrapMode = tk2dSpriteAnimationClip.WrapMode.Loop;
                clips.Add(idleClip);
            }

            table.flipStyle = flipStyle;

            table.outlineNorth = GenerateTableOutlineObject("North_" + name, outlinePaths[0], table.gameObject, TableCollection);
            table.outlineEast = GenerateTableOutlineObject("East_" + name, outlinePaths[1], table.gameObject, TableCollection);
            table.outlineWest = GenerateTableOutlineObject("West_" + name, outlinePaths[2], table.gameObject, TableCollection);
            table.outlineSouth = GenerateTableOutlineObject("South_" + name, outlinePaths[3], table.gameObject, TableCollection);

            if (ShadowSpritePath != null)
            {
                GameObject shadowObject = SpriteBuilder.SpriteFromResource(ShadowSpritePath, null, false);
                FakePrefab.MarkAsFakePrefab(shadowObject);
                int shadowID = SpriteBuilder.AddSpriteToCollection(ShadowSpritePath, TableCollection);
                tk2dSprite shadowSprite = shadowObject.GetComponent<tk2dSprite>();
                shadowSprite.SetSprite(TableCollection, shadowID);
                table.shadowSprite = shadowSprite;
                shadowObject.transform.parent = gameObject.transform;
            }
            bool DisableLeftAndRight = flipStyle == FlippableCover.FlipStyle.ONLY_FLIPS_UP_DOWN | flipStyle == FlippableCover.FlipStyle.NO_FLIPS;
            bool DisableUpAndDown = flipStyle == FlippableCover.FlipStyle.ONLY_FLIPS_LEFT_RIGHT | flipStyle == FlippableCover.FlipStyle.NO_FLIPS;
            if (DisableUpAndDown != true)
            {
                tk2dSpriteAnimationClip flipUp = AddTableAnimation(animator, TableCollection, northFlipPaths, name + "_flip_north", FlipFPS, new Vector3(FlippedColliderSizeX_Horizontal, FlippedColliderSizeY_Horizontal + colliderSize.y), new Vector3(colliderOffset.x, (ColliderOffsetY+colliderSize.y)- FlippedColliderSizeY_Horizontal));
                tk2dSpriteAnimationClip flipDown = AddTableAnimation(animator, TableCollection, southFlipPaths, name + "_flip_south", FlipFPS, new Vector3(FlippedColliderSizeX_Horizontal, FlippedColliderSizeY_Horizontal), new Vector3(colliderOffset.x, colliderOffset.y));
                tk2dSpriteAnimationClip breakUp = AddTableAnimation(animator, TableCollection, northBreakPaths, name + "_break_north", BreakFPS, new Vector3(colliderSize.x, colliderSize.y), new Vector3(colliderOffset.x, colliderOffset.y));
                tk2dSpriteAnimationClip breakDown = AddTableAnimation(animator, TableCollection, southBreakPaths, name + "_break_south", BreakFPS, new Vector3(colliderSize.x, colliderSize.y), new Vector3(colliderOffset.x, colliderOffset.y));
                clips.Add(flipUp);
                clips.Add(flipDown);
                clips.Add(breakUp);
                clips.Add(breakDown);
            }
            if (DisableLeftAndRight != true)
            {
                tk2dSpriteAnimationClip flipLeft = AddTableAnimation(animator, TableCollection, westFlipPaths, name + "_flip_east", FlipFPS, new Vector3(FlippedColliderSizeX_Vertical*2, FlippedColliderSizeY_Vertical), new Vector3((ColliderOffsetX+colliderSize.x), colliderOffset.y));
                tk2dSpriteAnimationClip flipRight = AddTableAnimation(animator, TableCollection, eastFlipPaths, name + "_flip_west", FlipFPS, new Vector3(FlippedColliderSizeX_Vertical, FlippedColliderSizeY_Vertical), new Vector3(colliderOffset.x, colliderOffset.y));//FlippedColliderSizeX_Vertical
                tk2dSpriteAnimationClip breakLeft = AddTableAnimation(animator, TableCollection, westBreakPaths, name + "_break_east", BreakFPS, new Vector3(colliderSize.x, colliderSize.y), new Vector3(colliderOffset.x, colliderOffset.y));
                tk2dSpriteAnimationClip breakRight = AddTableAnimation(animator, TableCollection, eastBreakPaths, name + "_break_west", BreakFPS, new Vector3(colliderSize.x, colliderSize.y), new Vector3(colliderOffset.x, colliderOffset.y));
                clips.Add(flipLeft);
                clips.Add(flipRight);
                clips.Add(breakLeft);
                clips.Add(breakRight);
            }
            tk2dSpriteAnimationClip breakUnflipped = AddTableAnimation(animator, TableCollection, unflippedBreakPaths, name + "_break_unflipped", UnflippedBreakFPS, new Vector3(colliderSize.x, colliderSize.y), new Vector3(colliderOffset.x, colliderOffset.y));
            clips.Add(breakUnflipped);

            table.flipAnimation = name + "_flip_{0}";
            table.breakAnimation = name + "_break_{0}";
            table.unflippedBreakAnimation = name + "_break_unflipped";

            animator.Library.clips = clips.ToArray();
            animator.playAutomatically = true;
            animator.DefaultClipId = animator.GetClipIdByName("idle");
        
            SpeculativeRigidbody speculativeRigidbody = sprite.SetUpEmptySpeculativeRigidbody(colliderOffset, colliderSize);
            speculativeRigidbody.PixelColliders.Add(new PixelCollider
            {
                ColliderGenerationMode = PixelCollider.PixelColliderGeneration.Tk2dPolygon,
                CollisionLayer = CollisionLayer.LowObstacle,

                IsTrigger = false,
                Enabled = true,
                BagleUseFirstFrameOnly = true,
                ManualOffsetX = colliderOffset.x,
                ManualOffsetY = colliderOffset.y,
                ManualDiameter = 0,
            });
            speculativeRigidbody.PixelColliders.Add(new PixelCollider
            {
                ColliderGenerationMode = PixelCollider.PixelColliderGeneration.Tk2dPolygon,
                CollisionLayer = CollisionLayer.BulletBlocker,
                IsTrigger = false,
                Enabled = false,
                BagleUseFirstFrameOnly = true,
                ManualOffsetX = colliderOffset.x,
                ManualOffsetY = colliderOffset.y,
                ManualDiameter = 0,
            });
            table.sprite = sprite;
            table.spriteAnimator = animator;
            table.specRigidbody = speculativeRigidbody;
            animator.transform.position = table.transform.position;
            animator.transform.parent = table.transform;
            table.majorBreakable.destroyedOnBreak = false;

            if (breakAnimPercentages_AND_SpritePathsandBreakDirectionsNorth != null || breakAnimPercentages_AND_SpritePathsandBreakDirectionsSouth != null|| breakAnimPercentages_AND_SpritePathsandBreakDirectionsEast != null || breakAnimPercentages_AND_SpritePathsandBreakDirectionsWest != null)
            {
                List<BreakFrame> breakFrameList = new List<BreakFrame>();
                if (breakAnimPercentages_AND_SpritePathsandBreakDirectionsNorth != null)
                {
                    foreach (var Entry in breakAnimPercentages_AND_SpritePathsandBreakDirectionsNorth)
                    {
                        BreakFrame breakFrame = new BreakFrame();
                        breakFrame.healthPercentage = Entry.Key;
                        int SpriteID = SpriteBuilder.AddSpriteToCollection(Entry.Value, TableCollection);
                        string ConvertedName = TableCollection.spriteDefinitions[SpriteID].name;
                        ETGModConsole.Log(TableCollection.spriteDefinitions[SpriteID].name);
                        if (ConvertedName.ToLower().Contains("north"))
                        {
                            ConvertedName = ReturnString(ConvertedName, "north");
                        }
                        breakFrame.sprite = ConvertedName;
                        breakFrameList.Add(breakFrame);
                    }
                }
                   

                if (breakAnimPercentages_AND_SpritePathsandBreakDirectionsSouth != null)
                {
                    foreach (var Entry in breakAnimPercentages_AND_SpritePathsandBreakDirectionsSouth)
                    {
                        BreakFrame breakFrame = new BreakFrame();
                        breakFrame.healthPercentage = Entry.Key;
                        int SpriteID = SpriteBuilder.AddSpriteToCollection(Entry.Value, TableCollection);
                        string ConvertedName = TableCollection.spriteDefinitions[SpriteID].name;
                        ETGModConsole.Log(TableCollection.spriteDefinitions[SpriteID].name);
                        if (ConvertedName.ToLower().Contains("south"))
                        {
                            ConvertedName = ReturnString(ConvertedName, "south");
                        }
                        breakFrame.sprite = ConvertedName;
                        breakFrameList.Add(breakFrame);
                    }
                }
                    
                if (breakAnimPercentages_AND_SpritePathsandBreakDirectionsEast != null)
                {
                    foreach (var Entry in breakAnimPercentages_AND_SpritePathsandBreakDirectionsEast)
                    {
                        BreakFrame breakFrame = new BreakFrame();
                        breakFrame.healthPercentage = Entry.Key;
                        int SpriteID = SpriteBuilder.AddSpriteToCollection(Entry.Value, TableCollection);
                        string ConvertedName = TableCollection.spriteDefinitions[SpriteID].name;
                        if (ConvertedName.ToLower().Contains("east"))
                        {
                            ConvertedName = ReturnString(ConvertedName, "east");
                        }
                        ETGModConsole.Log(ConvertedName);
                        breakFrame.sprite = ConvertedName;
                        breakFrameList.Add(breakFrame);
                    }
                }
               
                if (breakAnimPercentages_AND_SpritePathsandBreakDirectionsWest != null)
                {
                    foreach (var Entry in breakAnimPercentages_AND_SpritePathsandBreakDirectionsWest)
                    {
                        BreakFrame breakFrame = new BreakFrame();
                        breakFrame.healthPercentage = Entry.Key;
                        int SpriteID = SpriteBuilder.AddSpriteToCollection(Entry.Value, TableCollection);
                        string ConvertedName = TableCollection.spriteDefinitions[SpriteID].name;
                        if (ConvertedName.ToLower().Contains("west"))
                        {
                            ConvertedName = ReturnString(ConvertedName, "west");
                        }
                        ETGModConsole.Log(ConvertedName);
                        breakFrame.sprite = ConvertedName;
                        breakFrameList.Add(breakFrame);
                    }
                }
                   
                BreakFrame[] array = breakFrameList.ToArray();
                table.prebreakFrames = array;
            }


            /*
            if (breakAnimPercentages_AND_SpritePathsandBreakDirections != null)
            {
                List<BreakFrame> breakFrameList = new List<BreakFrame>();
                foreach (var Entry in breakAnimPercentages_AND_SpritePathsandBreakDirections)
                {
                    Dictionary<string, BreakDirection> dict = Entry.Value;
                    foreach (var EntryLayer2 in dict)
                    {
                        BreakFrame breakFrame = new BreakFrame();
                        breakFrame.healthPercentage = Entry.Key;
                        int SpriteID = SpriteBuilder.AddSpriteToCollection(EntryLayer2.Key, TableCollection);
                        tk2dSpriteDefinition def = TableCollection.spriteDefinitions[SpriteID];
                        breakFrame.sprite = def.name;
                        breakFrameList.Add(breakFrame);
                    }
                }
                BreakFrame[] array = breakFrameList.ToArray(); //This part heres fucky
                table.prebreakFrames = array;
               

            }
            */
            if (unflippedBreakAnimPercentagesAndSpritePaths != null)
            {
                List<BreakFrame> breakFrameList = new List<BreakFrame>();
                foreach (var Entry in unflippedBreakAnimPercentagesAndSpritePaths)
                {
                    BreakFrame breakFrame = new BreakFrame();
                    breakFrame.healthPercentage = Entry.Key;
                    int SpriteID = SpriteBuilder.AddSpriteToCollection(Entry.Value, TableCollection);
                    breakFrame.sprite = TableCollection.spriteDefinitions[SpriteID].name;
                    breakFrameList.Add(breakFrame);
                }
                BreakFrame[] array = breakFrameList.ToArray();
                table.prebreakFramesUnflipped = array;
            }
            
            if (IsSlideable == true){table.gameObject.GetOrAddComponent<SlideSurface>();}
            if (hasDecorations == true) 
            {
                SurfaceDecorator decorator = table.gameObject.GetOrAddComponent<SurfaceDecorator>();
                decorator.chanceToDecorate = chanceToDecorateTable;
                decorator.parentSprite = table.GetComponent<tk2dSprite>();
            }
            return table;
        }

        private static string ReturnString(string str, string oldChar)
        {
            return str.Replace(oldChar, "{0}");
        }
        private static GameObject GenerateTableOutlineObject(string name, string outlinePath, GameObject parent, tk2dSpriteCollectionData collection)
        {
            GameObject gameObject = SpriteBuilder.SpriteFromResource(outlinePath, null, false);
            FakePrefab.MarkAsFakePrefab(gameObject);
            gameObject.name = name + "_Outline";
            gameObject.layer = 0;
            gameObject.GetComponent<tk2dSprite>().HeightOffGround = 0.1f;
            SpriteBuilder.AddSpriteToCollection(outlinePath, collection);

            gameObject.transform.parent = parent.transform;
            return gameObject;
        }
        private static SpeculativeRigidbody SetUpEmptySpeculativeRigidbody(this tk2dSprite sprite, IntVector2 offset, IntVector2 dimensions)
        {
            var body = sprite.gameObject.GetOrAddComponent<SpeculativeRigidbody>();
            body.PixelColliders = new List<PixelCollider>() { };
            return body;
        }
        private static tk2dSpriteAnimationClip AddTableAnimation(tk2dSpriteAnimator animator, tk2dSpriteCollectionData Tablecollection, string[] spritePaths, string clipName, int FPS, Vector3 colliderSize, Vector3 colliderOffset)
        {
            tk2dSpriteAnimation animation = animator.gameObject.AddComponent<tk2dSpriteAnimation>();
            animation.clips = new tk2dSpriteAnimationClip[0];
            animator.Library = animation;
            tk2dSpriteAnimationClip idleClip = new tk2dSpriteAnimationClip() { name = clipName, frames = new tk2dSpriteAnimationFrame[0], fps = FPS };
            List<tk2dSpriteAnimationFrame> frames = new List<tk2dSpriteAnimationFrame>();
            for (int i = 0; i < spritePaths.Length; i++)
            {
                tk2dSpriteCollectionData collection = Tablecollection;
                int frameSpriteId = SpriteBuilder.AddSpriteToCollection(spritePaths[i], collection);
                tk2dSpriteDefinition frameDef = collection.spriteDefinitions[frameSpriteId];
                frameDef.ConstructOffsetsFromAnchor(tk2dBaseSprite.Anchor.LowerLeft);
                
                if (clipName.Contains("break"))
                {
                    tk2dSpriteDefinition frameDefMod = GenerateNoColliderForSpriteDefinition(frameDef);
                    frameDef = frameDefMod;
                }
                else
                {
                    tk2dSpriteDefinition frameDefMod = GenerateColliderForSpriteDefinition(frameDef, colliderSize, colliderOffset);
                    frameDef = frameDefMod;
                }
                frames.Add(new tk2dSpriteAnimationFrame { spriteId = frameSpriteId, spriteCollection = collection });
            }
            idleClip.frames = frames.ToArray();
            idleClip.wrapMode = tk2dSpriteAnimationClip.WrapMode.Once;
            return idleClip;
        }
        private static tk2dSpriteDefinition GenerateColliderForSpriteDefinition(tk2dSpriteDefinition frameDef, Vector3 colliderSize, Vector3 colliderOffset)
        {
            frameDef.colliderVertices = new Vector3[] { new Vector3(colliderSize.x / 32, colliderSize.y / 32), new Vector3(colliderSize.x / 32, colliderSize.y / 32) };
            frameDef.collisionLayer = CollisionLayer.HighObstacle;
            frameDef.colliderConvex = false;
            frameDef.colliderType = tk2dSpriteDefinition.ColliderType.Box;
            frameDef.colliderSmoothSphereCollisions = false;
            frameDef.complexGeometry = false;
            frameDef.flipped = tk2dSpriteDefinition.FlipMode.Tk2d;            
            if (colliderOffset != null)
            {
                AddOffset(frameDef, new Vector2(colliderOffset.x/32, colliderOffset.y/32));         
            }
            
            return frameDef;
        }
        private static tk2dSpriteDefinition GenerateNoColliderForSpriteDefinition(tk2dSpriteDefinition frameDef)
        {
            frameDef.colliderVertices = new Vector3[] { new Vector3(0,0)};
            frameDef.collisionLayer = CollisionLayer.HighObstacle;
            frameDef.colliderConvex = false;
            frameDef.colliderType = tk2dSpriteDefinition.ColliderType.None;
            frameDef.colliderSmoothSphereCollisions = false;
            frameDef.complexGeometry = false;
            frameDef.flipped = tk2dSpriteDefinition.FlipMode.Tk2d;
            return frameDef;
        }
        private static void AddOffset(this tk2dSpriteDefinition def, Vector2 offset)
        {
            float xOffset = offset.x;
            float yOffset = offset.y;
            if (def.colliderVertices != null && def.colliderVertices.Length > 0)
            {
                def.colliderVertices[0] += new Vector3(xOffset, yOffset, 0);
               
            }
        }
        public enum BreakDirection
        {
            NORTH,
            SOUTH,
            EAST, 
            WEST
        };

        /// <summary>
        /// Generates, and returns a NoteDoer using a MajorBreakable. This is as much customization as you'll probably get for now.
        /// </summary>
        /// <param name="majorBreakable">Your MajorBreakable that you're turning into a Note.</param>
        /// <param name="textboxSpawnPoint">The transform position from where the textbox will spawn.</param>
        /// <param name="stringKey">What your note says.</param>
        /// <param name="DestroyedAfterRead">If true, destroys the note after being read.</param>
        /// <param name="noteBackgroundType">The background your note will have.</param>
        public static NoteDoer GenerateNoteDoer(MajorBreakable majorBreakable, Transform textboxSpawnPoint, string stringKey, bool DestroyedAfterRead = false, NoteDoer.NoteBackgroundType noteBackgroundType = NoteDoer.NoteBackgroundType.NOTE)
        {
            NoteDoer note = majorBreakable.gameObject.AddComponent<NoteDoer>();
            note.textboxSpawnPoint = textboxSpawnPoint;
            note.DestroyedOnFinish = DestroyedAfterRead;
            note.stringKey = stringKey;
            note.noteBackgroundType = noteBackgroundType;
            return note;
        }
        /// <summary>
        /// Generates, and returns a GameObject that can be used as a transform position.
        /// </summary>
        public static GameObject GenerateTransformObject(GameObject attacher, Vector2 attachpoint, string name = "shootPoint")
        {
            GameObject shootpoint = new GameObject(name);
            shootpoint.transform.parent = attacher.transform;
            shootpoint.transform.position = attachpoint;
            return attacher.transform.Find(name).gameObject;
        }

        /// <summary>
        /// Generates, and returns a MajorBreakable. This is for generating a basic one, it returns it so you can additionally modify it without cluttering up the setup method too much.
        /// </summary>
        /// <param name="name">The name of your breakable. Not very useful, but I figured it would be important to set it.</param>
        /// <param name="idleSpritePaths">Your sprite paths. Only insert one path if you don't want it to be animated.</param>
        /// <param name="idleAnimFPS">The FPS of your breakables idle animation.</param>
        /// <param name="breakSpritePaths">Your sprite paths for the break animation. You can set this to null if you dont want a break animation.</param>
        /// <param name="breakAnimFPS">The FPS of your breakables breaking animation.</param>
        /// <param name="HP">The amount of damage your MajorBreakable can sustain before breaking.</param>
        /// <param name="ShadowSpritePath">The spritepath of your shadow. Leave this null to not have a shadow.</param>
        /// <param name="ShadowOffsetX">The X value of the shadows offset. Note that 1 here means 1 *tile*, and not 1 pixel!</param>
        /// <param name="ShadowOffsetY">The Y value of the shadows offset. Note that 1 here means 1 *tile*, and not 1 pixel!</param>
        /// <param name="UsesCustomColliderValues">Setting this to true will let you use custom collider sizes and offsets. Keeping it false will use no offsets and generate a size based on the sprites size.</param>
        /// <param name="ColliderSizeX">The X Value of your collider. Only used if UsesCustomColliderValues is true. Note that 1 here means 1 *pixel*, and not 1 tile!</param>
        /// <param name="ColliderSizeY">The Y Value of your collider. Only used if UsesCustomColliderValues is true. Note that 1 here means 1 *pixel*, and not 1 tile!</param>
        /// <param name="ColliderOffsetX">The X Value of your colliders offset. Only used if UsesCustomColliderValues is true.</param>
        /// <param name="ColliderOffsetY">The Y Value of your colliders offset. Only used if UsesCustomColliderValues is true.</param>
        /// <param name="DistribleShards">When shards spawn, if set to true, will spawn the shards at random positions inside the bounds of your breakables sprite.</param>
        /// <param name="breakVFX">The VFX that plays when the breakable is broken. Keep this as null to not have any VFX there.</param>
        /// <param name="damagedVFX">The VFX that plays when the breakable is damaged. Keep this as null to not have any VFX there.</param>
        /// <param name="BlocksPaths">Will act as a blocker and will not let enemies path find through it, I think.</param>
        /// <param name="collisionLayerList">Sets the collision layer/s of the MajorBreakable. leaving this as null will set it to HighObstacle AND BulletBlocker, however basegame MajorBreakables can use different ones, and at times multiple at once.</param>

        public static MajorBreakable GenerateMajorBreakable(string name, string[] idleSpritePaths, int idleAnimFPS = 2, string[] breakSpritePaths = null, int breakAnimFPS = 5, float HP = 100, bool UsesCustomColliderValues = false, int ColliderSizeX = 16, int ColliderSizeY = 8, int ColliderOffsetX = 0, int ColliderOffsetY = 8, bool DistribleShards = true, VFXPool breakVFX = null, VFXPool damagedVFX = null, bool BlocksPaths = false, List<CollisionLayer> collisionLayerList = null, Dictionary<float, string> preBreakframesAndHPPercentages = null, bool usesWorldShader = true)
        {
            Texture2D textureFromResource = ResourceExtractor.GetTextureFromResource(idleSpritePaths[0]);
            GameObject gameObject = SpriteBuilder.SpriteFromResource(idleSpritePaths[0], null, false);
            FakePrefab.MarkAsFakePrefab(gameObject);
            gameObject.name = name;
            MajorBreakable breakable = gameObject.AddComponent<MajorBreakable>();

            tk2dSpriteCollectionData MajorBreakableSpriteCollection = SpriteBuilder.ConstructCollection(gameObject, (name + "_Collection"));
            int spriteID = SpriteBuilder.AddSpriteToCollection(idleSpritePaths[0], MajorBreakableSpriteCollection);
            tk2dSprite sprite = gameObject.GetOrAddComponent<tk2dSprite>();
            sprite.SetSprite(MajorBreakableSpriteCollection, spriteID);

            IntVector2 intVector = new IntVector2(ColliderSizeX, ColliderSizeY);
            IntVector2 colliderOffset = new IntVector2(ColliderOffsetX, ColliderOffsetY);
            IntVector2 colliderSize = new IntVector2(intVector.x, intVector.y);

            if (UsesCustomColliderValues == false)
            {
                IntVector2 nonCustomintVector = new IntVector2(textureFromResource.width, textureFromResource.height);
                colliderSize = new IntVector2(nonCustomintVector.x, nonCustomintVector.y);
            }

            SpeculativeRigidbody speculativeRigidbody = sprite.SetUpEmptySpeculativeRigidbody(colliderOffset, colliderSize);
            if (collisionLayerList == null)
            {
                speculativeRigidbody.PixelColliders.Add(new PixelCollider
                {
                    ColliderGenerationMode = PixelCollider.PixelColliderGeneration.Manual,
                    CollisionLayer = CollisionLayer.HighObstacle,
                    IsTrigger = false,
                    BagleUseFirstFrameOnly = false,
                    SpecifyBagelFrame = string.Empty,
                    BagelColliderNumber = 0,
                    ManualOffsetX = colliderOffset.x,
                    ManualOffsetY = colliderOffset.y,
                    ManualWidth = colliderSize.x,
                    ManualHeight = colliderSize.y,
                    ManualDiameter = 0,
                    ManualLeftX = 0,
                    ManualLeftY = 0,
                    ManualRightX = 0,
                    ManualRightY = 0,
                });
                speculativeRigidbody.PixelColliders.Add(new PixelCollider
                {
                    ColliderGenerationMode = PixelCollider.PixelColliderGeneration.Manual,
                    CollisionLayer = CollisionLayer.BulletBlocker,
                    IsTrigger = false,
                    BagleUseFirstFrameOnly = false,
                    SpecifyBagelFrame = string.Empty,
                    BagelColliderNumber = 0,
                    ManualOffsetX = colliderOffset.x,
                    ManualOffsetY = colliderOffset.y,
                    ManualWidth = colliderSize.x,
                    ManualHeight = colliderSize.y,
                    ManualDiameter = 0,
                    ManualLeftX = 0,
                    ManualLeftY = 0,
                    ManualRightX = 0,
                    ManualRightY = 0,
                });
               
            }
            else
            {
                foreach (CollisionLayer layer in collisionLayerList)
                {
                    speculativeRigidbody.PixelColliders.Add(new PixelCollider
                    {
                        ColliderGenerationMode = PixelCollider.PixelColliderGeneration.Manual,
                        CollisionLayer = layer,
                        IsTrigger = false,
                        BagleUseFirstFrameOnly = false,
                        SpecifyBagelFrame = string.Empty,
                        BagelColliderNumber = 0,
                        ManualOffsetX = colliderOffset.x,
                        ManualOffsetY = colliderOffset.y,
                        ManualWidth = colliderSize.x,
                        ManualHeight = colliderSize.y,
                        ManualDiameter = 0,
                        ManualLeftX = 0,
                        ManualLeftY = 0,
                        ManualRightX = 0,
                        ManualRightY = 0,
                    });
                }
            }

            


            tk2dSpriteAnimator animator = gameObject.GetOrAddComponent<tk2dSpriteAnimator>();
            tk2dSpriteAnimation animation = gameObject.AddComponent<tk2dSpriteAnimation>();
            animation.clips = new tk2dSpriteAnimationClip[0];
            animator.Library = animation;

            List<tk2dSpriteAnimationClip> clips = new List<tk2dSpriteAnimationClip>();
            if (idleSpritePaths.Length >= 1)
            {
                tk2dSpriteAnimationClip idleClip = new tk2dSpriteAnimationClip() { name = "idle", frames = new tk2dSpriteAnimationFrame[0], fps = idleAnimFPS };
                List<tk2dSpriteAnimationFrame> frames = new List<tk2dSpriteAnimationFrame>();
                for (int i = 0; i < idleSpritePaths.Length; i++)
                {
                    tk2dSpriteCollectionData collection = MajorBreakableSpriteCollection;
                    int frameSpriteId = SpriteBuilder.AddSpriteToCollection(idleSpritePaths[i], collection);
                    tk2dSpriteDefinition frameDef = collection.spriteDefinitions[frameSpriteId];
                    frames.Add(new tk2dSpriteAnimationFrame { spriteId = frameSpriteId, spriteCollection = collection });
                    frameDef.ConstructOffsetsFromAnchor(tk2dBaseSprite.Anchor.LowerLeft);
                }
                idleClip.frames = frames.ToArray();
                idleClip.wrapMode = tk2dSpriteAnimationClip.WrapMode.Loop;
                animator.Library.clips = animation.clips.Concat(new tk2dSpriteAnimationClip[] { idleClip }).ToArray();
                animator.playAutomatically = true;
                animator.DefaultClipId = animator.GetClipIdByName("idle");
                clips.Add(idleClip);
            }
            if (breakSpritePaths != null)
            {
                tk2dSpriteAnimation breakAnimation = gameObject.AddComponent<tk2dSpriteAnimation>();
                breakAnimation.clips = new tk2dSpriteAnimationClip[0];
                tk2dSpriteAnimationClip breakClip = new tk2dSpriteAnimationClip() { name = "break", frames = new tk2dSpriteAnimationFrame[0], fps = breakAnimFPS };
                List<tk2dSpriteAnimationFrame> breakFrames = new List<tk2dSpriteAnimationFrame>();
                for (int i = 0; i < breakSpritePaths.Length; i++)
                {
                    tk2dSpriteCollectionData collection = MajorBreakableSpriteCollection;
                    int frameSpriteId = SpriteBuilder.AddSpriteToCollection(breakSpritePaths[i], collection);
                    tk2dSpriteDefinition frameDef = collection.spriteDefinitions[frameSpriteId];
                    frameDef.ConstructOffsetsFromAnchor(tk2dBaseSprite.Anchor.LowerLeft);
                    breakFrames.Add(new tk2dSpriteAnimationFrame { spriteId = frameSpriteId, spriteCollection = collection });
                }
                breakClip.frames = breakFrames.ToArray();
                breakClip.wrapMode = tk2dSpriteAnimationClip.WrapMode.Once;
                clips.Add(breakClip);
                tk2dSpriteAnimationClip[] array = clips.ToArray();
                animator.Library.clips = array;
                animator.playAutomatically = true;
                animator.DefaultClipId = animator.GetClipIdByName("idle");
                breakable.breakAnimation = "break";
            }
            breakable.sprite = sprite;
            breakable.sprite.transform.position = breakable.sprite.transform.position;
            breakable.specRigidbody = speculativeRigidbody;
            breakable.spriteAnimator = animator;
            breakable.HitPoints = HP;
            breakable.HandlePathBlocking = BlocksPaths;
           
            if (breakVFX != null) { breakable.breakVfx = breakVFX; }
            if (damagedVFX != null) { breakable.damageVfx = damagedVFX; }

            if (preBreakframesAndHPPercentages != null)
            {
                List<BreakFrame> breakFrameList = new List<BreakFrame>();
                foreach (var Entry in preBreakframesAndHPPercentages)
                {
                    BreakFrame breakFrame = new BreakFrame();
                    breakFrame.healthPercentage = Entry.Key;
                    int SpriteID = SpriteBuilder.AddSpriteToCollection(Entry.Value, MajorBreakableSpriteCollection);
                    breakFrame.sprite = MajorBreakableSpriteCollection.spriteDefinitions[SpriteID].name;
                    breakFrameList.Add(breakFrame);
                }
                BreakFrame[] array = breakFrameList.ToArray();
                breakable.prebreakFrames = array;
            }
            breakable.distributeShards = DistribleShards;

            if (usesWorldShader == true)
            {
                //breakable.sprite.renderer.material.shader = worldShader;
                //breakable.sprite.renderer.material.EnableKeyword("BRIGHTNESS_CLAMP_ON");

            }
            return breakable;
        }
        /// <summary>
        /// Generates, and returns a MinorBreakable. This is for generating a basic one, it returns it so you can additionally modify it without cluttering up the setup method too much.
        /// </summary>
        /// <param name="name">The name of your breakable. Not very useful, but I figured it would be important to set it.</param>
        /// <param name="idleSpritePaths">Your sprite paths. Only insert one path if you don't want it to be animated.</param>
        /// <param name="idleAnimFPS">The FPS of your breakables idle animation.</param>
        /// <param name="breakSpritePaths">Your sprite paths for the break animation. You can set this to null if you dont want a break animation.</param>
        /// <param name="breakAnimFPS">The FPS of your breakables breaking animation.</param>
        /// <param name="breakAudioEvent">The sound that plays when your breakable is broken. You can set it to null for it to not play a sound.</param>
        /// <param name="ShadowSpritePath">The spritepath of your shadow. Leave this null to not have a shadow.</param>
        /// <param name="ShadowOffsetX">The X value of the shadows offset.  Note that 1 here means 1 *tile*, and not 1 pixel!</param>
        /// <param name="ShadowOffsetY">The Y value of the shadows offset.  Note that 1 here means 1 *tile*, and not 1 pixel!</param>
        /// <param name="UsesCustomColliderValues">Setting this to true will let you use custom collider sizes and offsets. Keeping it false will use no offsets and generate a size based on the sprites size.</param>
        /// <param name="ColliderSizeX">The X Value of your collider. Only used if UsesCustomColliderValues is true.  Note that 1 here means 1 *pixel*, and not 1 tile!</param>
        /// <param name="ColliderSizeY">The Y Value of your collider. Only used if UsesCustomColliderValues is true.  Note that 1 here means 1 *pixel*, and not 1 tile!</param>
        /// <param name="ColliderOffsetX">The X Value of your colliders offset. Only used if UsesCustomColliderValues is true.</param>
        /// <param name="ColliderOffsetY">The Y Value of your colliders offset. Only used if UsesCustomColliderValues is true.</param>
        /// <param name="DestroyVFX">The VFX that plays when your breakable is destroyed.</param>
        /// <param name="collisionLayerList">Sets the collision layer/s of the MinorBreakable. leaving this as null will set it to HighObstacle, however basegame MinorBreakables can use different ones, and at times multiple at once.</param>

        public static MinorBreakable GenerateMinorBreakable(string name, string[] idleSpritePaths, int idleAnimFPS = 1, string[] breakSpritePaths = null, int breakAnimFPS = 5, string breakAudioEvent = "Play_OBJ_pot_shatter_01", bool UsesCustomColliderValues = false, int ColliderSizeX = 16, int ColliderSizeY = 8, int ColliderOffsetX = 0, int ColliderOffsetY = 8, GameObject DestroyVFX = null, List<CollisionLayer> collisionLayerList = null, bool usesWorldShader = true)     
        {
            Texture2D textureFromResource = ResourceExtractor.GetTextureFromResource(idleSpritePaths[0]);
            GameObject gameObject = SpriteBuilder.SpriteFromResource(idleSpritePaths[0], null, false);
            FakePrefab.MarkAsFakePrefab(gameObject);
            gameObject.name = name;
            MinorBreakable breakable = gameObject.AddComponent<MinorBreakable>();

            tk2dSpriteCollectionData MinorBreakableSpriteCollection = SpriteBuilder.ConstructCollection(gameObject, (name + "_Collection"));
            int spriteID = SpriteBuilder.AddSpriteToCollection(idleSpritePaths[0], MinorBreakableSpriteCollection);
            tk2dSprite sprite = gameObject.GetOrAddComponent<tk2dSprite>();
            sprite.SetSprite(MinorBreakableSpriteCollection, spriteID);

            IntVector2 intVector = new IntVector2(ColliderSizeX, ColliderSizeY);
            IntVector2 colliderOffset = new IntVector2(ColliderOffsetX, ColliderOffsetY);
            IntVector2 colliderSize = new IntVector2(intVector.x, intVector.y);

            if (UsesCustomColliderValues == false)
            {
                IntVector2 nonCustomintVector = new IntVector2(textureFromResource.width, textureFromResource.height);
                colliderSize = new IntVector2(nonCustomintVector.x, nonCustomintVector.y);
            }

            SpeculativeRigidbody speculativeRigidbody = sprite.SetUpEmptySpeculativeRigidbody(colliderOffset, colliderSize);
            if (collisionLayerList == null)
            {
                speculativeRigidbody.PixelColliders.Add(new PixelCollider
                {
                    ColliderGenerationMode = PixelCollider.PixelColliderGeneration.Manual,
                    CollisionLayer = CollisionLayer.HighObstacle,
                    IsTrigger = false,
                    BagleUseFirstFrameOnly = false,
                    SpecifyBagelFrame = string.Empty,
                    BagelColliderNumber = 0,
                    ManualOffsetX = colliderOffset.x,
                    ManualOffsetY = colliderOffset.y,
                    ManualWidth = colliderSize.x,
                    ManualHeight = colliderSize.y,
                    ManualDiameter = 0,
                    ManualLeftX = 0,
                    ManualLeftY = 0,
                    ManualRightX = 0,
                    ManualRightY = 0,
                });
            }
            else
            {
                foreach (CollisionLayer layer in collisionLayerList)
                {
                    speculativeRigidbody.PixelColliders.Add(new PixelCollider
                    {
                        ColliderGenerationMode = PixelCollider.PixelColliderGeneration.Manual,
                        CollisionLayer = layer,
                        IsTrigger = false,
                        BagleUseFirstFrameOnly = false,
                        SpecifyBagelFrame = string.Empty,
                        BagelColliderNumber = 0,
                        ManualOffsetX = colliderOffset.x,
                        ManualOffsetY = colliderOffset.y,
                        ManualWidth = colliderSize.x,
                        ManualHeight = colliderSize.y,
                        ManualDiameter = 0,
                        ManualLeftX = 0,
                        ManualLeftY = 0,
                        ManualRightX = 0,
                        ManualRightY = 0,
                    });
                }
            }

          
            tk2dSpriteAnimator animator = gameObject.GetOrAddComponent<tk2dSpriteAnimator>();
            tk2dSpriteAnimation animation = gameObject.AddComponent<tk2dSpriteAnimation>();
            animation.clips = new tk2dSpriteAnimationClip[0];
            animator.Library = animation;

            List<tk2dSpriteAnimationClip> clips = new List<tk2dSpriteAnimationClip>();
            if (idleSpritePaths.Length >= 1)
            {
                tk2dSpriteAnimationClip idleClip = new tk2dSpriteAnimationClip() { name = "idle", frames = new tk2dSpriteAnimationFrame[0], fps = idleAnimFPS };
                List<tk2dSpriteAnimationFrame> frames = new List<tk2dSpriteAnimationFrame>();
                for (int i = 0; i < idleSpritePaths.Length; i++)
                {
                    tk2dSpriteCollectionData collection = MinorBreakableSpriteCollection;
                    int frameSpriteId = SpriteBuilder.AddSpriteToCollection(idleSpritePaths[i], collection);
                    tk2dSpriteDefinition frameDef = collection.spriteDefinitions[frameSpriteId];
                    frameDef.ConstructOffsetsFromAnchor(tk2dBaseSprite.Anchor.LowerLeft);
                    frames.Add(new tk2dSpriteAnimationFrame { spriteId = frameSpriteId, spriteCollection = collection });
                }
                idleClip.frames = frames.ToArray();
                idleClip.wrapMode = tk2dSpriteAnimationClip.WrapMode.Loop;
                animator.Library.clips = animation.clips.Concat(new tk2dSpriteAnimationClip[] { idleClip }).ToArray();
                animator.playAutomatically = true;
                animator.DefaultClipId = animator.GetClipIdByName("idle");
                clips.Add(idleClip);
            }
            if (breakSpritePaths != null)
            {
                tk2dSpriteAnimation breakAnimation = gameObject.AddComponent<tk2dSpriteAnimation>();
                breakAnimation.clips = new tk2dSpriteAnimationClip[0];
                tk2dSpriteAnimationClip breakClip = new tk2dSpriteAnimationClip() { name = "break", frames = new tk2dSpriteAnimationFrame[0], fps = breakAnimFPS };
                List<tk2dSpriteAnimationFrame> breakFrames = new List<tk2dSpriteAnimationFrame>();
                for (int i = 0; i < breakSpritePaths.Length; i++)
                {
                    tk2dSpriteCollectionData collection = MinorBreakableSpriteCollection;
                    int frameSpriteId = SpriteBuilder.AddSpriteToCollection(breakSpritePaths[i], collection);
                    tk2dSpriteDefinition frameDef = collection.spriteDefinitions[frameSpriteId];
                    frameDef.ConstructOffsetsFromAnchor(tk2dBaseSprite.Anchor.LowerLeft);
                    breakFrames.Add(new tk2dSpriteAnimationFrame { spriteId = frameSpriteId, spriteCollection = collection });
                }
                breakClip.frames = breakFrames.ToArray();
                breakClip.wrapMode = tk2dSpriteAnimationClip.WrapMode.Once;
                clips.Add(breakClip);
                tk2dSpriteAnimationClip[] array = clips.ToArray();
                animator.Library.clips = array;
                animator.playAutomatically = true;
                animator.DefaultClipId = animator.GetClipIdByName("idle");
                breakable.breakAnimName = "break";
            }
            breakable.sprite = sprite;
            breakable.specRigidbody = speculativeRigidbody;
            breakable.spriteAnimator = animator;
            breakable.breakAudioEventName = breakAudioEvent;

          
            if (DestroyVFX != null) { breakable.AdditionalVFXObject = DestroyVFX; }
            return breakable;
        }

        /// <summary>
        /// Generates, and returns a WaftingDebrisObject that you can add to a ShardCluster, which in turn can be used by your breakable  
        /// </summary>
        /// <param name="waftDuration">Takes a random value between the X and Y value given and uses that as a value for how long it wafts *every* waft.</param>
        /// <param name="waftDistance">Takes a random value between the X and Y value given and uses that as a value for the distance it wafts.</param>
        /// <param name="initialBurstDuration">Takes the X & Y value given as a potential peak height.</param>
        /// 
        /// <param name="debrisObjectsCanRotate">Enables/Disables whether your shards can rotate in-flight.</param>
        /// <param name="LifeSpanMin">The minimum flight-time of your shards.</param>
        /// <param name="LifeSpanMax">The maximum flight-time of your shards.</param>
        /// <param name="AngularVelocity">How much your shards will rotate in-flight.</param>
        /// <param name="AngularVelocityVariance">Adds/removes some angular velocity to your shards when created. For example, having 40 AngularVelocity and an AngularVelocityVariance of 12 will set the AngularVelocity of your shards from anywhere between 28 and 52.</param>
        /// <param name="shadowSprite">The sprite of the shadow your DebrisObject will use. Leave this as null to not have a shadow.</param>
        /// <param name="Mass">Default of 1. The amount of additional weight applied to your DebrisObject</param>
        /// <param name="AudioEventName">The sound that will play when the shards contact the ground.</param>
        /// <param name="BounceVFX">The VFX that plays when your shards bounce.</param>
        /// <param name="DebrisBounceCount">The amount of times your shards will bounce.</param>
        /// <param name="DoesGoopOnRest">If true, will spawn goop on itself when it is in a resting state.</param>
        /// <param name="GoopType">The goop it will spawn if DoesGoopOnRest is true.</param>
        /// <param name="GoopRadius">The radius of the spawned goop.</param>

        public static WaftingDebrisObject GenerateWaftingDebrisObject(string shardSpritePath, Vector2 waftDuration, Vector2 waftDistance, Vector2 initialBurstDuration, bool debrisObjectsCanRotate = true, float LifeSpanMin = 0.33f, float LifeSpanMax = 2f, float AngularVelocity = 540, float AngularVelocityVariance = 180f, tk2dSprite shadowSprite = null, float Mass = 1, string AudioEventName = null, GameObject BounceVFX = null, int DebrisBounceCount = 0, bool DoesGoopOnRest = false, GoopDefinition GoopType = null, float GoopRadius = 1f, bool usesWorldShader = true)
        {
            GameObject debrisObject = SpriteBuilder.SpriteFromResource(shardSpritePath, null, false);
            FakePrefab.MarkAsFakePrefab(debrisObject);
            tk2dSprite tk2dsprite = debrisObject.GetComponent<tk2dSprite>();
            WaftingDebrisObject DebrisObj = debrisObject.AddComponent<WaftingDebrisObject>();
            DebrisObj.canRotate = debrisObjectsCanRotate;
            DebrisObj.lifespanMin = LifeSpanMin;
            DebrisObj.lifespanMax = LifeSpanMax;
            DebrisObj.bounceCount = DebrisBounceCount;
            DebrisObj.angularVelocity = AngularVelocity;
            DebrisObj.angularVelocityVariance = AngularVelocityVariance;
            if (AudioEventName != null) { DebrisObj.audioEventName = AudioEventName; }
            if (BounceVFX != null) { DebrisObj.optionalBounceVFX = BounceVFX; }
            DebrisObj.sprite = tk2dsprite;
            DebrisObj.DoesGoopOnRest = DoesGoopOnRest;
            if (GoopType != null) { DebrisObj.AssignedGoop = GoopType; } else if (GoopType == null && DebrisObj.DoesGoopOnRest == true) { DebrisObj.DoesGoopOnRest = false; }
            DebrisObj.GoopRadius = GoopRadius;
            if (shadowSprite != null) { DebrisObj.shadowSprite = shadowSprite; }
            DebrisObj.inertialMass = Mass;

            DebrisObj.waftDuration = waftDuration;
            DebrisObj.waftDistance = waftDistance;
            DebrisObj.initialBurstDuration = initialBurstDuration;

            if (usesWorldShader == true){//DebrisObj.sprite.renderer.material.shader = worldShader;
               // DebrisObj.sprite.renderer.material.EnableKeyword("BRIGHTNESS_CLAMP_ON");
            }
            return DebrisObj;
        }

        /// <summary>
        /// Generates, and returns an animated WaftingDebrisObject that you can add to a ShardCluster, which in turn can be used by your breakable  
        /// </summary>
        /// <param name="waftDuration">Takes a random value between the X and Y value given and uses that as a value for how long it wafts *every* waft.</param>
        /// <param name="waftDistance">Takes a random value between the X and Y value given and uses that as a value for the distance it wafts.</param>
        /// <param name="initialBurstDuration">Takes the X & Y value given as a potential peak height.</param>
        /// <param name="FPS">The FPS of your DebrisObject.</param>
        /// <param name="wrapMode">The wrap mode of the animated DebrisObject.</param>
        /// <param name="debrisObjectsCanRotate">Enables/Disables whether your shards can rotate in-flight.</param>
        /// <param name="LifeSpanMin">The minimum flight-time of your shards.</param>
        /// <param name="LifeSpanMax">The maximum flight-time of your shards.</param>
        /// <param name="AngularVelocity">How much your shards will rotate in-flight.</param>
        /// <param name="AngularVelocityVariance">Adds/removes some angular velocity to your shards when created. For example, having 40 AngularVelocity and an AngularVelocityVariance of 12 will set the AngularVelocity of your shards from anywhere between 28 and 52.</param>
        /// <param name="shadowSprite">The sprite of the shadow your DebrisObject will use. Leave this as null to not have a shadow.</param>
        /// <param name="Mass">Default of 1. The amount of additional weight applied to your DebrisObject</param>
        /// <param name="AudioEventName">The sound that will play when the shards hit the ground.</param>
        /// <param name="BounceVFX">The VFX that plays when your shards bounce.</param>
        /// <param name="DebrisBounceCount">The amount of times your shards will bounce.</param>
        /// <param name="DoesGoopOnRest">If true, will spawn goop on itself when it is in a resting state.</param>
        /// <param name="GoopType">The goop it will spawn if DoesGoopOnRest is true.</param>
        /// <param name="GoopRadius">The radius of the spawned goop.</param>

        public static WaftingDebrisObject GenerateAnimatedWaftingDebrisObject(string[] shardSpritePaths, Vector2 waftDuration, Vector2 waftDistance, Vector2 initialBurstDuration, int FPS = 12, tk2dSpriteAnimationClip.WrapMode wrapMode = tk2dSpriteAnimationClip.WrapMode.Loop, bool debrisObjectsCanRotate = true, float LifeSpanMin = 0.33f, float LifeSpanMax = 2f, float AngularVelocity = 540, float AngularVelocityVariance = 180f, tk2dSprite shadowSprite = null, float Mass = 1, string AudioEventName = null, GameObject BounceVFX = null, int DebrisBounceCount = 0, bool DoesGoopOnRest = false, GoopDefinition GoopType = null, float GoopRadius = 1f, bool usesWorldShader = true)
        {
            GameObject debrisObject = SpriteBuilder.SpriteFromResource(shardSpritePaths[0], null, false);
            FakePrefab.MarkAsFakePrefab(debrisObject);
            WaftingDebrisObject DebrisObj = debrisObject.AddComponent<WaftingDebrisObject>();

            tk2dSpriteCollectionData VFXSpriteCollection = SpriteBuilder.ConstructCollection(debrisObject, (shardSpritePaths[0] + "_Collection"));
            int spriteID = SpriteBuilder.AddSpriteToCollection(shardSpritePaths[0], VFXSpriteCollection);
            tk2dSprite sprite = debrisObject.GetOrAddComponent<tk2dSprite>();
            sprite.SetSprite(VFXSpriteCollection, spriteID);

            tk2dSpriteAnimator animator = debrisObject.GetOrAddComponent<tk2dSpriteAnimator>();
            tk2dSpriteAnimation animation = debrisObject.AddComponent<tk2dSpriteAnimation>();
            animation.clips = new tk2dSpriteAnimationClip[0];
            animator.Library = animation;
            tk2dSpriteAnimationClip idleClip = new tk2dSpriteAnimationClip() { name = "idle", frames = new tk2dSpriteAnimationFrame[0], fps = FPS };
            List<tk2dSpriteAnimationFrame> frames = new List<tk2dSpriteAnimationFrame>();
            for (int i = 0; i < shardSpritePaths.Length; i++)
            {
                tk2dSpriteCollectionData collection = VFXSpriteCollection;
                int frameSpriteId = SpriteBuilder.AddSpriteToCollection(shardSpritePaths[i], collection);
                tk2dSpriteDefinition frameDef = collection.spriteDefinitions[frameSpriteId];
                frames.Add(new tk2dSpriteAnimationFrame { spriteId = frameSpriteId, spriteCollection = collection });
            }
            idleClip.frames = frames.ToArray();
            idleClip.wrapMode = wrapMode;
            animator.Library.clips = animation.clips.Concat(new tk2dSpriteAnimationClip[] { idleClip }).ToArray();
            animator.DefaultClipId = animator.GetClipIdByName("idle");
            DebrisObj.waftAnimationName = "idle";
            DebrisObj.canRotate = debrisObjectsCanRotate;
            DebrisObj.lifespanMin = LifeSpanMin;
            DebrisObj.lifespanMax = LifeSpanMax;
            DebrisObj.bounceCount = DebrisBounceCount;
            DebrisObj.angularVelocity = AngularVelocity;
            DebrisObj.angularVelocityVariance = AngularVelocityVariance;
            if (AudioEventName != null) { DebrisObj.audioEventName = AudioEventName; }
            if (BounceVFX != null) { DebrisObj.optionalBounceVFX = BounceVFX; }
            DebrisObj.sprite = sprite;
            DebrisObj.DoesGoopOnRest = DoesGoopOnRest;
            if (GoopType != null) { DebrisObj.AssignedGoop = GoopType; } else if (GoopType == null && DebrisObj.DoesGoopOnRest == true) { DebrisObj.DoesGoopOnRest = false; }
            DebrisObj.GoopRadius = GoopRadius;
            if (shadowSprite != null) { DebrisObj.shadowSprite = shadowSprite; }
            DebrisObj.inertialMass = Mass;

            DebrisObj.waftDuration = waftDuration;
            DebrisObj.waftDistance = waftDistance;
            DebrisObj.initialBurstDuration = initialBurstDuration;
            if (usesWorldShader == true)
            {
                //DebrisObj.sprite.renderer.material.shader = worldShader;
                //DebrisObj.sprite.renderer.material.EnableKeyword("BRIGHTNESS_CLAMP_ON");
            }
            return DebrisObj;
        }


        /// <summary>
        /// Generates, and returns an array of WaftingDebrisObjects that you can add to a ShardCluster, which in turn can be used by your breakable. note that each Debris Object generated here will all use the same values you gave it
        /// </summary>
        /// <param name="waftDuration">Takes a random value between the X and Y value given and uses that as a value for how long it wafts *every* waft.</param>
        /// <param name="waftDistance">Takes a random value between the X and Y value given and uses that as a value for the distance it wafts.</param>
        /// <param name="initialBurstDuration">Takes the X & Y value given as a potential peak height.</param>
        /// <param name="debrisObjectsCanRotate">Enables/Disables whether your shards can rotate in-flight.</param>
        /// <param name="LifeSpanMin">The minimum flight-time of your shards.</param>
        /// <param name="LifeSpanMax">The maximum flight-time of your shards.</param>
        /// <param name="AngularVelocity">How much your shards will rotate in-flight.</param>
        /// <param name="AngularVelocityVariance">Adds/removes some angular velocity to your shards when created. For example, having 40 AngularVelocity and an AngularVelocityVariance of 12 will set the AngularVelocity of your shards from anywhere between 28 and 52.</param>
        /// <param name="shadowSprite">The sprite of the shadow your DebrisObject will use. Leave this as null to not have a shadow.</param>
        /// <param name="Mass">Default of 1. The amount of additional weight applied to your DebrisObject</param>
        /// <param name="AudioEventName">The sound that will play when the shards hit the ground.</param>
        /// <param name="BounceVFX">The VFX that plays when your shards bounce.</param>
        /// <param name="DebrisBounceCount">The amount of times your shards will bounce.</param>
        /// <param name="DoesGoopOnRest">If true, will spawn goop on itself when it is in a resting state.</param>
        /// <param name="GoopType">The goop it will spawn if DoesGoopOnRest is true.</param>
        /// <param name="GoopRadius">The radius of the spawned goop.</param>
        public static WaftingDebrisObject[] GenerateWaftingDebrisObjects(string[] shardSpritePaths, Vector2 waftDuration, Vector2 waftDistance, Vector2 initialBurstDuration, bool debrisObjectsCanRotate = true, float LifeSpanMin = 0.33f, float LifeSpanMax = 2f, float AngularVelocity = 540, float AngularVelocityVariance = 180f, tk2dSprite shadowSprite = null, float Mass = 1, string AudioEventName = null, GameObject BounceVFX = null, int DebrisBounceCount = 0, bool DoesGoopOnRest = false, GoopDefinition GoopType = null, float GoopRadius = 1f, bool usesWorldShader = true)
        {
            List<WaftingDebrisObject> DebrisObjectList = new List<WaftingDebrisObject>();
            for (int i = 0; i < shardSpritePaths.Length; i++)
            {
                GameObject debrisObject = SpriteBuilder.SpriteFromResource(shardSpritePaths[i], null, false);
                FakePrefab.MarkAsFakePrefab(debrisObject);
                tk2dSprite tk2dsprite = debrisObject.GetComponent<tk2dSprite>();
                WaftingDebrisObject DebrisObj = debrisObject.AddComponent<WaftingDebrisObject>();
                DebrisObj.canRotate = debrisObjectsCanRotate;
                DebrisObj.lifespanMin = LifeSpanMin;
                DebrisObj.lifespanMax = LifeSpanMax;
                DebrisObj.bounceCount = DebrisBounceCount;
                DebrisObj.angularVelocity = AngularVelocity;
                DebrisObj.angularVelocityVariance = AngularVelocityVariance;
                if (AudioEventName != null) { DebrisObj.audioEventName = AudioEventName; }
                if (BounceVFX != null) { DebrisObj.optionalBounceVFX = BounceVFX; }
                DebrisObj.sprite = tk2dsprite;
                DebrisObj.DoesGoopOnRest = DoesGoopOnRest;
                if (GoopType != null) { DebrisObj.AssignedGoop = GoopType; } else if (GoopType == null && DebrisObj.DoesGoopOnRest == true) { DebrisObj.DoesGoopOnRest = false; }
                DebrisObj.GoopRadius = GoopRadius;
                if (shadowSprite != null) { DebrisObj.shadowSprite = shadowSprite; }
                DebrisObj.inertialMass = Mass;
                DebrisObj.waftDuration = waftDuration;
                DebrisObj.waftDistance = waftDistance;
                DebrisObj.initialBurstDuration = initialBurstDuration;
                if (usesWorldShader == true)
                {
                   //DebrisObj.sprite.renderer.material.shader = worldShader;
                   //DebrisObj.sprite.renderer.material.EnableKeyword("BRIGHTNESS_CLAMP_ON");
                }
                DebrisObjectList.Add(DebrisObj);
            }
            WaftingDebrisObject[] DebrisArray = DebrisObjectList.ToArray();
            return DebrisArray;
        }



        /// <summary>
        /// Generates, and returns an array of animated WaftingDebrisObjects that you can add to a ShardCluster, which in turn can be used by your breakable. note that each Debris Object generated here will all use the same values you gave it
        /// </summary>
        /// <param name="FPS">The FPS of your DebrisObject.</param>
        /// <param name="wrapMode">The wrap mode of the animated DebrisObject.</param>
        /// <param name="debrisObjectsCanRotate">Enables/Disables whether your shards can rotate in-flight.</param>
        /// <param name="LifeSpanMin">The minimum flight-time of your shards.</param>
        /// <param name="LifeSpanMax">The maximum flight-time of your shards.</param>
        /// <param name="AngularVelocity">How much your shards will rotate in-flight.</param>
        /// <param name="AngularVelocityVariance">Adds/removes some angular velocity to your shards when created. For example, having 40 AngularVelocity and an AngularVelocityVariance of 12 will set the AngularVelocity of your shards from anywhere between 28 and 52.</param>
        /// <param name="shadowSprite">The sprite of the shadow your DebrisObject will use. Leave this as null to not have a shadow.</param>
        /// <param name="Mass">Default of 1. The amount of additional weight applied to your DebrisObject</param>
        /// <param name="AudioEventName">The sound that will play when the shards hit the ground.</param>
        /// <param name="BounceVFX">The VFX that plays when your shards bounce.</param>
        /// <param name="DebrisBounceCount">The amount of times your shards will bounce.</param>
        /// <param name="DoesGoopOnRest">If true, will spawn goop on itself when it is in a resting state.</param>
        /// <param name="GoopType">The goop it will spawn if DoesGoopOnRest is true.</param>
        /// <param name="GoopRadius">The radius of the spawned goop.</param>

        public static WaftingDebrisObject[] GenerateAnimatedWaftingDebrisObjects(List<string[]> shardSpritePathsList, Vector2 waftDuration, Vector2 waftDistance, Vector2 initialBurstDuration, int FPS = 12, tk2dSpriteAnimationClip.WrapMode wrapMode = tk2dSpriteAnimationClip.WrapMode.Loop, bool debrisObjectsCanRotate = true, tk2dSprite shadowSprite = null, float Mass = 1, float LifeSpanMin = 0.33f, float LifeSpanMax = 2f, float AngularVelocity = 540, float AngularVelocityVariance = 180f, string AudioEventName = null, GameObject BounceVFX = null, int DebrisBounceCount = 0, bool DoesGoopOnRest = false, GoopDefinition GoopType = null, float GoopRadius = 1f, bool usesWorldShader = true)
        {
            List<WaftingDebrisObject> DebrisObjectList = new List<WaftingDebrisObject>();
            for (int i = 0; i < shardSpritePathsList.Count; i++)
            {
                string[] paths = shardSpritePathsList[i];
                for (int e = 0; e < paths.Length; e++)
                {
                    GameObject debrisObject = SpriteBuilder.SpriteFromResource(paths[0], null, false);
                    FakePrefab.MarkAsFakePrefab(debrisObject);
                    WaftingDebrisObject DebrisObj = debrisObject.AddComponent<WaftingDebrisObject>();
                    tk2dSpriteCollectionData VFXSpriteCollection = SpriteBuilder.ConstructCollection(debrisObject, (paths[0] + "_Collection"));
                    int spriteID = SpriteBuilder.AddSpriteToCollection(paths[0], VFXSpriteCollection);
                    tk2dSprite sprite = debrisObject.GetOrAddComponent<tk2dSprite>();
                    sprite.SetSprite(VFXSpriteCollection, spriteID);

                    tk2dSpriteAnimator animator = debrisObject.GetOrAddComponent<tk2dSpriteAnimator>();
                    tk2dSpriteAnimation animation = debrisObject.AddComponent<tk2dSpriteAnimation>();
                    animation.clips = new tk2dSpriteAnimationClip[0];
                    animator.Library = animation;
                    tk2dSpriteAnimationClip idleClip = new tk2dSpriteAnimationClip() { name = "idle", frames = new tk2dSpriteAnimationFrame[0], fps = FPS };
                    List<tk2dSpriteAnimationFrame> frames = new List<tk2dSpriteAnimationFrame>();
                    for (int q = 0; q < paths.Length; q++)
                    {
                        tk2dSpriteCollectionData collection = VFXSpriteCollection;
                        int frameSpriteId = SpriteBuilder.AddSpriteToCollection(paths[q], collection);
                        tk2dSpriteDefinition frameDef = collection.spriteDefinitions[frameSpriteId];
                        frames.Add(new tk2dSpriteAnimationFrame { spriteId = frameSpriteId, spriteCollection = collection });
                    }
                    idleClip.frames = frames.ToArray();
                    idleClip.wrapMode = wrapMode;
                    animator.Library.clips = animation.clips.Concat(new tk2dSpriteAnimationClip[] { idleClip }).ToArray();
                    animator.DefaultClipId = animator.GetClipIdByName("idle");
                    DebrisObj.waftAnimationName = "idle";
                    DebrisObj.canRotate = debrisObjectsCanRotate;
                    DebrisObj.lifespanMin = LifeSpanMin;
                    DebrisObj.lifespanMax = LifeSpanMax;
                    DebrisObj.bounceCount = DebrisBounceCount;
                    DebrisObj.angularVelocity = AngularVelocity;
                    DebrisObj.angularVelocityVariance = AngularVelocityVariance;
                    if (AudioEventName != null) { DebrisObj.audioEventName = AudioEventName; }
                    if (BounceVFX != null) { DebrisObj.optionalBounceVFX = BounceVFX; }
                    DebrisObj.sprite = sprite;
                    DebrisObj.DoesGoopOnRest = DoesGoopOnRest;
                    if (GoopType != null) { DebrisObj.AssignedGoop = GoopType; } else if (GoopType == null && DebrisObj.DoesGoopOnRest == true) { DebrisObj.DoesGoopOnRest = false; }
                    DebrisObj.GoopRadius = GoopRadius;
                    if (shadowSprite != null) { DebrisObj.shadowSprite = shadowSprite; }
                    DebrisObj.inertialMass = Mass;
                    DebrisObj.waftDuration = waftDuration;
                    DebrisObj.waftDistance = waftDistance;
                    DebrisObj.initialBurstDuration = initialBurstDuration;
                    if (usesWorldShader == true)
                    {
                        //DebrisObj.sprite.renderer.material.shader = worldShader;
                        //DebrisObj.sprite.renderer.material.EnableKeyword("BRIGHTNESS_CLAMP_ON");
                    }
                    DebrisObjectList.Add(DebrisObj);
                }
            }
            WaftingDebrisObject[] DebrisArray = DebrisObjectList.ToArray();
            return DebrisArray;
        }


        /// <summary>
        /// Generates, and returns a DebrisObject that you can add to a ShardCluster, which in turn can be used by your breakable  
        /// </summary>
        /// <param name="debrisObjectsCanRotate">Enables/Disables whether your shards can rotate in-flight.</param>
        /// <param name="LifeSpanMin">The minimum flight-time of your shards.</param>
        /// <param name="LifeSpanMax">The maximum flight-time of your shards.</param>
        /// <param name="AngularVelocity">How much your shards will rotate in-flight.</param>
        /// <param name="AngularVelocityVariance">Adds/removes some angular velocity to your shards when created. For example, having 40 AngularVelocity and an AngularVelocityVariance of 12 will set the AngularVelocity of your shards from anywhere between 28 and 52.</param>
        /// <param name="shadowSprite">The sprite of the shadow your DebrisObject will use. Leave this as null to not have a shadow.</param>
        /// <param name="Mass">Default of 1. The amount of additional weight applied to your DebrisObject</param>
        /// <param name="AudioEventName">The sound that will play when the shards contact the ground.</param>
        /// <param name="BounceVFX">The VFX that plays when your shards bounce.</param>
        /// <param name="DebrisBounceCount">The amount of times your shards will bounce.</param>
        /// <param name="DoesGoopOnRest">If true, will spawn goop on itself when it is in a resting state.</param>
        /// <param name="GoopType">The goop it will spawn if DoesGoopOnRest is true.</param>
        /// <param name="GoopRadius">The radius of the spawned goop.</param>

        public static DebrisObject GenerateDebrisObject(string shardSpritePath, bool debrisObjectsCanRotate = true, float LifeSpanMin = 0.33f, float LifeSpanMax = 2f, float AngularVelocity = 540, float AngularVelocityVariance = 180f, tk2dSprite shadowSprite = null, float Mass = 1, string AudioEventName = null, GameObject BounceVFX = null, int DebrisBounceCount = 0, bool DoesGoopOnRest = false, GoopDefinition GoopType = null, float GoopRadius = 1f, bool usesWorldShader = true)
        {
            GameObject debrisObject = SpriteBuilder.SpriteFromResource(shardSpritePath, null, false);
            FakePrefab.MarkAsFakePrefab(debrisObject);
            tk2dSprite tk2dsprite = debrisObject.GetComponent<tk2dSprite>();
            DebrisObject DebrisObj = debrisObject.AddComponent<DebrisObject>();
            DebrisObj.canRotate = debrisObjectsCanRotate;
            DebrisObj.lifespanMin = LifeSpanMin;
            DebrisObj.lifespanMax = LifeSpanMax;
            DebrisObj.bounceCount = DebrisBounceCount;
            DebrisObj.angularVelocity = AngularVelocity;
            DebrisObj.angularVelocityVariance = AngularVelocityVariance;
            if (AudioEventName != null) { DebrisObj.audioEventName = AudioEventName; }
            if (BounceVFX != null) { DebrisObj.optionalBounceVFX = BounceVFX; }
            DebrisObj.sprite = tk2dsprite;
            DebrisObj.DoesGoopOnRest = DoesGoopOnRest;
            if (GoopType != null) { DebrisObj.AssignedGoop = GoopType; } else if(GoopType == null && DebrisObj.DoesGoopOnRest == true) { DebrisObj.DoesGoopOnRest = false; }
            DebrisObj.GoopRadius = GoopRadius;
            if (shadowSprite != null) { DebrisObj.shadowSprite = shadowSprite; }
            DebrisObj.inertialMass = Mass;
            if (usesWorldShader == true)
            {
                //DebrisObj.sprite.renderer.material.shader = worldShader;
                //DebrisObj.sprite.renderer.material.EnableKeyword("BRIGHTNESS_CLAMP_ON");
            }
            return DebrisObj;
        }
        /// <summary>
        /// Generates, and returns an animated DebrisObject that you can add to a ShardCluster, which in turn can be used by your breakable  
        /// </summary>
        /// <param name="FPS">The FPS of your DebrisObject.</param>
        /// <param name="wrapMode">The wrap mode of the animated DebrisObject.</param>
        /// <param name="debrisObjectsCanRotate">Enables/Disables whether your shards can rotate in-flight.</param>
        /// <param name="LifeSpanMin">The minimum flight-time of your shards.</param>
        /// <param name="LifeSpanMax">The maximum flight-time of your shards.</param>
        /// <param name="AngularVelocity">How much your shards will rotate in-flight.</param>
        /// <param name="AngularVelocityVariance">Adds/removes some angular velocity to your shards when created. For example, having 40 AngularVelocity and an AngularVelocityVariance of 12 will set the AngularVelocity of your shards from anywhere between 28 and 52.</param>
        /// <param name="shadowSprite">The sprite of the shadow your DebrisObject will use. Leave this as null to not have a shadow.</param>
        /// <param name="Mass">Default of 1. The amount of additional weight applied to your DebrisObject</param>
        /// <param name="AudioEventName">The sound that will play when the shards hit the ground.</param>
        /// <param name="BounceVFX">The VFX that plays when your shards bounce.</param>
        /// <param name="DebrisBounceCount">The amount of times your shards will bounce.</param>
        /// <param name="DoesGoopOnRest">If true, will spawn goop on itself when it is in a resting state.</param>
        /// <param name="GoopType">The goop it will spawn if DoesGoopOnRest is true.</param>
        /// <param name="GoopRadius">The radius of the spawned goop.</param>
        public static DebrisObject GenerateAnimatedDebrisObject(string[] shardSpritePaths, int FPS = 12, tk2dSpriteAnimationClip.WrapMode wrapMode = tk2dSpriteAnimationClip.WrapMode.Loop, bool debrisObjectsCanRotate = true, float LifeSpanMin = 0.33f, float LifeSpanMax = 2f, float AngularVelocity = 540, float AngularVelocityVariance = 180f, tk2dSprite shadowSprite = null, float Mass = 1, string AudioEventName = null, GameObject BounceVFX = null, int DebrisBounceCount = 0, bool DoesGoopOnRest = false, GoopDefinition GoopType = null, float GoopRadius = 1f, bool usesWorldShader = true)
        {
            GameObject debrisObject = SpriteBuilder.SpriteFromResource(shardSpritePaths[0], null, false);
            FakePrefab.MarkAsFakePrefab(debrisObject);
            DebrisObject DebrisObj = debrisObject.AddComponent<DebrisObject>();

            tk2dSpriteCollectionData VFXSpriteCollection = SpriteBuilder.ConstructCollection(debrisObject, (shardSpritePaths[0] + "_Collection"));
            int spriteID = SpriteBuilder.AddSpriteToCollection(shardSpritePaths[0], VFXSpriteCollection);
            tk2dSprite sprite = debrisObject.GetOrAddComponent<tk2dSprite>();
            sprite.SetSprite(VFXSpriteCollection, spriteID);

            tk2dSpriteAnimator animator = debrisObject.GetOrAddComponent<tk2dSpriteAnimator>();
            tk2dSpriteAnimation animation = debrisObject.AddComponent<tk2dSpriteAnimation>();
            animation.clips = new tk2dSpriteAnimationClip[0];
            animator.Library = animation;
            tk2dSpriteAnimationClip idleClip = new tk2dSpriteAnimationClip() { name = "idle", frames = new tk2dSpriteAnimationFrame[0], fps = FPS };
            List<tk2dSpriteAnimationFrame> frames = new List<tk2dSpriteAnimationFrame>();
            for (int i = 0; i < shardSpritePaths.Length; i++)
            {
                tk2dSpriteCollectionData collection = VFXSpriteCollection;
                int frameSpriteId = SpriteBuilder.AddSpriteToCollection(shardSpritePaths[i], collection);
                tk2dSpriteDefinition frameDef = collection.spriteDefinitions[frameSpriteId];
                frames.Add(new tk2dSpriteAnimationFrame { spriteId = frameSpriteId, spriteCollection = collection });
            }
            idleClip.frames = frames.ToArray();
            idleClip.wrapMode = wrapMode;
            animator.Library.clips = animation.clips.Concat(new tk2dSpriteAnimationClip[] { idleClip }).ToArray();
            animator.playAutomatically = true;
            animator.DefaultClipId = animator.GetClipIdByName("idle");
            DebrisObj.canRotate = debrisObjectsCanRotate;
            DebrisObj.lifespanMin = LifeSpanMin;
            DebrisObj.lifespanMax = LifeSpanMax;
            DebrisObj.bounceCount = DebrisBounceCount;
            DebrisObj.angularVelocity = AngularVelocity;
            DebrisObj.angularVelocityVariance = AngularVelocityVariance;
            if (AudioEventName != null) { DebrisObj.audioEventName = AudioEventName; }
            if (BounceVFX != null) { DebrisObj.optionalBounceVFX = BounceVFX; }
            DebrisObj.sprite = sprite;
            DebrisObj.DoesGoopOnRest = DoesGoopOnRest;
            if (GoopType != null) { DebrisObj.AssignedGoop = GoopType; } else if (GoopType == null && DebrisObj.DoesGoopOnRest == true) { DebrisObj.DoesGoopOnRest = false; }
            DebrisObj.GoopRadius = GoopRadius;
            if (shadowSprite != null) { DebrisObj.shadowSprite = shadowSprite; }
            DebrisObj.inertialMass = Mass;
            if (usesWorldShader == true)
            {
                //DebrisObj.sprite.renderer.material.shader = worldShader;
                //DebrisObj.sprite.renderer.material.EnableKeyword("BRIGHTNESS_CLAMP_ON");
            }
            return DebrisObj;
        }
        /// <summary>
        /// Generates, and returns an array of DebrisObjects that you can add to a ShardCluster, which in turn can be used by your breakable. note that each Debris Object generated here will all use the same values you gave it
        /// </summary>
        /// <param name="debrisObjectsCanRotate">Enables/Disables whether your shards can rotate in-flight.</param>
        /// <param name="LifeSpanMin">The minimum flight-time of your shards.</param>
        /// <param name="LifeSpanMax">The maximum flight-time of your shards.</param>
        /// <param name="AngularVelocity">How much your shards will rotate in-flight.</param>
        /// <param name="AngularVelocityVariance">Adds/removes some angular velocity to your shards when created. For example, having 40 AngularVelocity and an AngularVelocityVariance of 12 will set the AngularVelocity of your shards from anywhere between 28 and 52.</param>
        /// <param name="shadowSprite">The sprite of the shadow your DebrisObject will use. Leave this as null to not have a shadow.</param>
        /// <param name="Mass">Default of 1. The amount of additional weight applied to your DebrisObject</param>
        /// <param name="AudioEventName">The sound that will play when the shards hit the ground.</param>
        /// <param name="BounceVFX">The VFX that plays when your shards bounce.</param>
        /// <param name="DebrisBounceCount">The amount of times your shards will bounce.</param>
        /// <param name="DoesGoopOnRest">If true, will spawn goop on itself when it is in a resting state.</param>
        /// <param name="GoopType">The goop it will spawn if DoesGoopOnRest is true.</param>
        /// <param name="GoopRadius">The radius of the spawned goop.</param>
        public static DebrisObject[] GenerateDebrisObjects(string[] shardSpritePaths, bool debrisObjectsCanRotate = true, float LifeSpanMin = 0.33f, float LifeSpanMax = 2f, float AngularVelocity = 540, float AngularVelocityVariance = 180f, tk2dSprite shadowSprite = null, float Mass = 1, string AudioEventName = null, GameObject BounceVFX = null, int DebrisBounceCount = 0, bool DoesGoopOnRest = false, GoopDefinition GoopType = null, float GoopRadius = 1f, bool usesWorldShader = true)
        {
            List<DebrisObject> DebrisObjectList = new List<DebrisObject>();
            for (int i = 0; i < shardSpritePaths.Length; i++)
            {
                GameObject debrisObject = SpriteBuilder.SpriteFromResource(shardSpritePaths[i], null, false);
                FakePrefab.MarkAsFakePrefab(debrisObject);
                tk2dSprite tk2dsprite = debrisObject.GetComponent<tk2dSprite>();
                DebrisObject DebrisObj = debrisObject.AddComponent<DebrisObject>();
                DebrisObj.canRotate = debrisObjectsCanRotate;
                DebrisObj.lifespanMin = LifeSpanMin;
                DebrisObj.lifespanMax = LifeSpanMax;
                DebrisObj.bounceCount = DebrisBounceCount;
                DebrisObj.angularVelocity = AngularVelocity;
                DebrisObj.angularVelocityVariance = AngularVelocityVariance;
                if (AudioEventName != null) { DebrisObj.audioEventName = AudioEventName; }
                if (BounceVFX != null) { DebrisObj.optionalBounceVFX = BounceVFX; }
                DebrisObj.sprite = tk2dsprite;
                DebrisObj.DoesGoopOnRest = DoesGoopOnRest;
                if (GoopType != null) { DebrisObj.AssignedGoop = GoopType; } else if (GoopType == null && DebrisObj.DoesGoopOnRest == true) { DebrisObj.DoesGoopOnRest = false; }
                DebrisObj.GoopRadius = GoopRadius;
                if (shadowSprite != null) { DebrisObj.shadowSprite = shadowSprite; }
                DebrisObj.inertialMass = Mass;
                if (usesWorldShader == true)
                {
                    //DebrisObj.sprite.renderer.material.shader = worldShader;
                    //DebrisObj.sprite.renderer.material.EnableKeyword("BRIGHTNESS_CLAMP_ON");
                }
                DebrisObjectList.Add(DebrisObj);
            }
            DebrisObject[] DebrisArray = DebrisObjectList.ToArray();
            return DebrisArray;
        }

        /// <summary>
        /// Generates, and returns an array of animated DebrisObjects that you can add to a ShardCluster, which in turn can be used by your breakable. note that each Debris Object generated here will all use the same values you gave it
        /// </summary>
        /// <param name="FPS">The FPS of your DebrisObject.</param>
        /// <param name="wrapMode">The wrap mode of the animated DebrisObject.</param>
        /// <param name="debrisObjectsCanRotate">Enables/Disables whether your shards can rotate in-flight.</param>
        /// <param name="LifeSpanMin">The minimum flight-time of your shards.</param>
        /// <param name="LifeSpanMax">The maximum flight-time of your shards.</param>
        /// <param name="AngularVelocity">How much your shards will rotate in-flight.</param>
        /// <param name="AngularVelocityVariance">Adds/removes some angular velocity to your shards when created. For example, having 40 AngularVelocity and an AngularVelocityVariance of 12 will set the AngularVelocity of your shards from anywhere between 28 and 52.</param>
        /// <param name="shadowSprite">The sprite of the shadow your DebrisObject will use. Leave this as null to not have a shadow.</param>
        /// <param name="Mass">Default of 1. The amount of additional weight applied to your DebrisObject</param>
        /// <param name="AudioEventName">The sound that will play when the shards hit the ground.</param>
        /// <param name="BounceVFX">The VFX that plays when your shards bounce.</param>
        /// <param name="DebrisBounceCount">The amount of times your shards will bounce.</param>
        /// <param name="DoesGoopOnRest">If true, will spawn goop on itself when it is in a resting state.</param>
        /// <param name="GoopType">The goop it will spawn if DoesGoopOnRest is true.</param>
        /// <param name="GoopRadius">The radius of the spawned goop.</param>

        public static DebrisObject[] GenerateAnimatedDebrisObjects(List<string[]> shardSpritePathsList, int FPS = 12, tk2dSpriteAnimationClip.WrapMode wrapMode = tk2dSpriteAnimationClip.WrapMode.Loop, bool debrisObjectsCanRotate = true, tk2dSprite shadowSprite = null, float Mass = 1, float LifeSpanMin = 0.33f, float LifeSpanMax = 2f, float AngularVelocity = 540, float AngularVelocityVariance = 180f, string AudioEventName = null, GameObject BounceVFX = null, int DebrisBounceCount = 0, bool DoesGoopOnRest = false, GoopDefinition GoopType = null, float GoopRadius = 1f, bool usesWorldShader = true)
        {
            List<DebrisObject> DebrisObjectList = new List<DebrisObject>();
            for (int i = 0; i < shardSpritePathsList.Count; i++)
            {
                string[] paths = shardSpritePathsList[i];
                for (int e = 0; e < paths.Length; e++)
                {
                    GameObject debrisObject = SpriteBuilder.SpriteFromResource(paths[0], null, false);
                    FakePrefab.MarkAsFakePrefab(debrisObject);
                    DebrisObject DebrisObj = debrisObject.AddComponent<DebrisObject>();
                    tk2dSpriteCollectionData VFXSpriteCollection = SpriteBuilder.ConstructCollection(debrisObject, (paths[0] + "_Collection"));
                    int spriteID = SpriteBuilder.AddSpriteToCollection(paths[0], VFXSpriteCollection);
                    tk2dSprite sprite = debrisObject.GetOrAddComponent<tk2dSprite>();
                    sprite.SetSprite(VFXSpriteCollection, spriteID);

                    tk2dSpriteAnimator animator = debrisObject.GetOrAddComponent<tk2dSpriteAnimator>();
                    tk2dSpriteAnimation animation = debrisObject.AddComponent<tk2dSpriteAnimation>();
                    animation.clips = new tk2dSpriteAnimationClip[0];
                    animator.Library = animation;
                    tk2dSpriteAnimationClip idleClip = new tk2dSpriteAnimationClip() { name = "idle", frames = new tk2dSpriteAnimationFrame[0], fps = FPS };
                    List<tk2dSpriteAnimationFrame> frames = new List<tk2dSpriteAnimationFrame>();
                    for (int q = 0; q < paths.Length; q++)
                    {
                        tk2dSpriteCollectionData collection = VFXSpriteCollection;
                        int frameSpriteId = SpriteBuilder.AddSpriteToCollection(paths[q], collection);
                        tk2dSpriteDefinition frameDef = collection.spriteDefinitions[frameSpriteId];
                        frames.Add(new tk2dSpriteAnimationFrame { spriteId = frameSpriteId, spriteCollection = collection });
                    }
                    idleClip.frames = frames.ToArray();
                    idleClip.wrapMode = wrapMode;
                    animator.Library.clips = animation.clips.Concat(new tk2dSpriteAnimationClip[] { idleClip }).ToArray();
                    animator.playAutomatically = true;
                    animator.DefaultClipId = animator.GetClipIdByName("idle");
                    DebrisObj.canRotate = debrisObjectsCanRotate;
                    DebrisObj.lifespanMin = LifeSpanMin;
                    DebrisObj.lifespanMax = LifeSpanMax;
                    DebrisObj.bounceCount = DebrisBounceCount;
                    DebrisObj.angularVelocity = AngularVelocity;
                    DebrisObj.angularVelocityVariance = AngularVelocityVariance;
                    if (AudioEventName != null) { DebrisObj.audioEventName = AudioEventName; }
                    if (BounceVFX != null) { DebrisObj.optionalBounceVFX = BounceVFX; }
                    DebrisObj.sprite = sprite;
                    DebrisObj.DoesGoopOnRest = DoesGoopOnRest;
                    if (GoopType != null) { DebrisObj.AssignedGoop = GoopType; } else if (GoopType == null && DebrisObj.DoesGoopOnRest == true) { DebrisObj.DoesGoopOnRest = false; }
                    DebrisObj.GoopRadius = GoopRadius;
                    if (shadowSprite != null) { DebrisObj.shadowSprite = shadowSprite; }
                    DebrisObj.inertialMass = Mass;
                    if (usesWorldShader == true)
                    {
                        //DebrisObj.sprite.renderer.material.shader = worldShader;
                        //DebrisObj.sprite.renderer.material.EnableKeyword("BRIGHTNESS_CLAMP_ON");
                    }
                    DebrisObjectList.Add(DebrisObj);
                }
            }
            DebrisObject[] DebrisArray = DebrisObjectList.ToArray();
            return DebrisArray;
        }
        /// <summary>
        /// Generates, and returns a ShardCluster that you can add to your breakable to have it create shards. 
        /// </summary>
        /// <param name="clusterForceMultiplier">The force applied multiplicatively onto the shards when they're created.</param>
        /// <param name="ClusterLaunchStrength">The amount of force applied multiplicatively to your shards when created.</param>
        /// <param name="MinClusterAmount">The minimum amount of shards that the shard cluster will spawn.</param>
        /// <param name="MaxClusterAmount">The maximum amount of shards that the shard cluster will spawn.</param>
        /// <param name="clusterRotationMultiplier">The amount of rotation applied multiplicatively.</param>
        public static ShardCluster GenerateShardCluster(DebrisObject[] debrisObjects, float ClusterLaunchStrength = 0.5f, float clusterForceMultiplier = 2, int MinClusterAmount = 1, int MaxClusterAmount = 5, float clusterRotationMultiplier = 1)
        {
            ShardCluster cluster = new ShardCluster();
            DebrisObject[] DebrisObjectArray = debrisObjects;
            cluster.forceMultiplier = clusterForceMultiplier;
            cluster.rotationMultiplier = clusterRotationMultiplier;
            cluster.minFromCluster = MinClusterAmount;
            cluster.maxFromCluster = MaxClusterAmount;
            cluster.forceAxialMultiplier = new Vector3(ClusterLaunchStrength, ClusterLaunchStrength, ClusterLaunchStrength);
            cluster.clusterObjects = DebrisObjectArray;
            return cluster;
        }
        /// <summary>
        /// Generates, and returns an array of ShardClusters that you can add to your breakable to have it create shards. note that each ShardCluster generated here will all use the same values you gave it
        /// </summary>
        /// <param name="clusterForceMultiplier">The force applied multiplicatively onto the shards when they're created.</param>
        /// <param name="ClusterLaunchStrength">The amount of force applied multiplicatively to your shards when created.</param>
        /// <param name="MinClusterAmount">The minimum amount of shards that the shard cluster will spawn.</param>
        /// <param name="MaxClusterAmount">The maximum amount of shards that the shard cluster will spawn.</param>
        /// <param name="clusterRotationMultiplier">The amount of rotation applied multiplicatively.</param>
        public static ShardCluster[] GenerateShardClustersFromArray(List<DebrisObject[]> debrisObjectList, float ClusterLaunchStrength = 0.5f, float clusterForceMultiplier = 2, int MinClusterAmount = 1, int MaxClusterAmount = 5, float clusterRotationMultiplier = 1)
        {
            List<ShardCluster> ShardClusters = new List<ShardCluster>();
            for (int i = 0; i < debrisObjectList.Count; i++)
            {
                ShardCluster cluster = new ShardCluster();
                DebrisObject[] DebrisObjectArray = debrisObjectList[i];
                cluster.forceMultiplier = clusterForceMultiplier;
                cluster.rotationMultiplier = clusterRotationMultiplier;
                cluster.minFromCluster = MinClusterAmount;
                cluster.maxFromCluster = MaxClusterAmount;
                cluster.forceAxialMultiplier = new Vector3(ClusterLaunchStrength, ClusterLaunchStrength, ClusterLaunchStrength);
                cluster.clusterObjects = DebrisObjectArray;
                ShardClusters.Add(cluster);
            }
            ShardCluster[] clusterArray = ShardClusters.ToArray();
            return clusterArray;
        }

        /// <summary>
        /// Generates, and returns an array of DungeonPlaceable
        /// </summary>
        /// <param name="gameObjects">The dictionary of objects that get added to the placeable, and their respective chances of appearing. A single dungeon placeable can have multiple choices of object it can be.</param>
        /// <param name="placeableWidth">The width of the placeable. try to make all your gameobjects relatively the same size as the length and width of the placeable.</param>
        /// <param name="placeableLength">The length of the placeable. try to make all your gameobjects relatively the same size as the length and width of the placeable.</param>
        /// <param name="dungeonPrerequisites">the prerequisite required for the object to appear. Leave this as null to have no prerequisite.</param>
        public static DungeonPlaceable GenerateDungeonPlaceable(Dictionary<GameObject, float> gameObjects, int placeableWidth = 1, int placeableLength = 1, DungeonPrerequisite[] dungeonPrerequisites = null)
        {
            if (dungeonPrerequisites == null) {dungeonPrerequisites = new DungeonPrerequisite[0];}
            DungeonPlaceable placeableContents = ScriptableObject.CreateInstance<DungeonPlaceable>();
            {
                placeableContents.width = placeableWidth;
                placeableContents.height = placeableLength;
                placeableContents.respectsEncounterableDifferentiator = true;
                placeableContents.variantTiers = new List<DungeonPlaceableVariant>();
            }
            foreach (var Entry in gameObjects)
            {
                DungeonPlaceableVariant variant = new DungeonPlaceableVariant();
                variant.percentChance = Entry.Value;
                variant.prerequisites = dungeonPrerequisites;
                variant.nonDatabasePlaceable = Entry.Key;
                placeableContents.variantTiers.Add(variant);
            }
            return placeableContents;
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BepInEx;
using Tracker.Components;
using EntityStates;
using HG;
using R2API;
using R2API.ScriptableObjects;
using RoR2;
using RoR2.ContentManagement;
using RoR2.UI;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using UnityEngine.Networking;
using RiskOfOptions;

using ModdedDamageType = R2API.DamageAPI.ModdedDamageType;
using System.Runtime.CompilerServices;

namespace Tracker
{
	[BepInDependency("com.bepis.r2api.content_management")]
	[BepInDependency("com.bepis.r2api")]
	[BepInDependency("com.bepis.r2api.damagetype")]
	[BepInDependency("com.bepis.r2api.language")]
	[BepInDependency("com.bepis.r2api.prefab")]
	[BepInDependency("com.bepis.r2api.unlockable")]

	[BepInDependency("com.rune580.riskofoptions", BepInDependency.DependencyFlags.SoftDependency)]

	[BepInPlugin("com.nayDPz.Tracker", "Tracker", "1.0.0")]
	public class TrackerMain : BaseUnityPlugin
	{
		

		public const string GUID = "com.nayDPz.Tracker";
		public const string MODNAME = "Tracker";
		public const string VERSION = "0.0.1";

		public static GameObject bodyPrefab;
		public static BodyIndex bodyIndex;
		public static ModdedDamageType applyStunMark;
		public static ModdedDamageType knockupOnHit;
		public static ModdedDamageType armorShredOnHit;
		public static R2APISerializableContentPack serializableContentPack;

		internal List<Type> entityStates;

		private static uint _bankID;
		private static uint _bankID2;
		public static TrackerMain Instance { get; private set; }
		public static PluginInfo PInfo { get; private set; }


		internal static bool RiskOfOptionsInstalled;

		private void Awake()
		{
			Instance = this;
			PInfo = this.Info;
			Log.Init(Logger);

			Languages.Init();
			Tracker.Config.ReadConfig();

			if (RiskOfOptionsInstalled = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.rune580.riskofoptions"))
            {
				SetupOptions();
			}

			Assets.PopulateAssets();
			TrackerContent.Initialize();

			bodyPrefab = Assets.mainAssetBundle.LoadAsset<GameObject>("TrackerBody");
			serializableContentPack = Assets.mainAssetBundle.LoadAsset<R2APISerializableContentPack>("ContentPack");
			
			new ContentPacks().Initialize();

			knockupOnHit = DamageAPI.ReserveDamageType();
			applyStunMark = DamageAPI.ReserveDamageType();
			armorShredOnHit = DamageAPI.ReserveDamageType();

			// XDDD
			ClonesToContentPackSHOULDNOTEXIST();
			SetupBody(bodyPrefab);
			Hook();

			ContentManager.onContentPacksAssigned += LateSetup;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private void SetupOptions()
        {			

        }

		[SystemInitializer]
		public static void LoadSoundBank()
		{
			if (Application.isBatchMode)
			{
				return;
			}
			try
			{
				var path = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
				AkSoundEngine.AddBasePath(path);
				var result = AkSoundEngine.LoadBank("DaredevilSoundBank.bnk", out _bankID);
				if (result != AKRESULT.AK_Success)
					Log.LogError("SoundBank Load Failed: " + result);

				result = AkSoundEngine.LoadBank("UltrakillSoundBank.bnk", out _bankID);
				if (result != AKRESULT.AK_Success)
					Log.LogError("SoundBank Load Failed: " + result);				
			}
			catch (Exception e)
			{
				Log.LogError(e);
			}
		}

		[SystemInitializer(typeof(BodyCatalog))]
		private static void FindBodyIndex()
		{
			bodyIndex = bodyPrefab.GetComponent<CharacterBody>().bodyIndex;
		}

		private void SetupBody(GameObject bodyPrefab)
		{
			bodyPrefab.GetComponent<CameraTargetParams>().cameraParams = Addressables.LoadAssetAsync<CharacterCameraParams>("RoR2/Base/Common/ccpStandard.asset").WaitForCompletion();
			bodyPrefab.GetComponent<CharacterBody>()._defaultCrosshairPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/UI/StandardCrosshair.prefab").WaitForCompletion();
			bodyPrefab.GetComponent<ModelLocator>().modelTransform.gameObject.GetComponent<FootstepHandler>().footstepDustPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/GenericFootstepDust.prefab").WaitForCompletion();
			bodyPrefab.GetComponent<CharacterBody>().preferredPodPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/SurvivorPod/SurvivorPod.prefab").WaitForCompletion();

		}

		//item displays
		private void LateSetup(ReadOnlyArray<ReadOnlyContentPack> obj)
		{
		}


		/// <summary>
		/// /////////////////////////////////////////////xxxxxxxxxxDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD
		/// </summary>
		private void ClonesToContentPackSHOULDNOTEXIST()
		{
			GameObject gameObject = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("Prefabs/CharacterMasters/MercMonsterMaster"), "DaredevilMonsterMaster", true);
			gameObject.GetComponent<CharacterMaster>().bodyPrefab = bodyPrefab;
			serializableContentPack.masterPrefabs = new GameObject[1] { gameObject };
			GameObject[] array = serializableContentPack.projectilePrefabs;
			ArrayHelper.Append(ref array, Assets.clonedVanillaProjectiles);
			serializableContentPack.projectilePrefabs = array;
		}

		internal static class ArrayHelper
		{
			public static T[] Append<T>(ref T[] array, List<T> list)
			{
				int num = array.Length;
				int count = list.Count;
				Array.Resize(ref array, num + count);
				list.CopyTo(array, num);
				return array;
			}
			public static Func<T[], T[]> AppendDel<T>(List<T> list)
			{
				return (T[] r) => Append(ref r, list);
			}
		}

		private void Hook()
		{
            On.RoR2.CharacterBody.RecalculateStats += CharacterBody_RecalculateStats;
            On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
		}



		private void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
			orig(self, damageInfo);
		}


        private void CharacterBody_RecalculateStats(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
        {
			orig(self);
		}
	}
}

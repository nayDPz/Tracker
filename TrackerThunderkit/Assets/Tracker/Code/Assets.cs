using System.Collections.Generic;
using System.IO;
using System.Linq;
using R2API;
using RoR2;
using RoR2.Projectile;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Tracker
{
	internal static class Assets
	{
		internal static AssetBundle mainAssetBundle;

		internal static GameObject lineVisualiserPrefab;

		internal static GameObject sawProjectile;
		internal static GameObject rebounderProjectile;

		internal static Material matTPInOut;
		internal static Material matHuntressFlash;
		internal static Material matHuntressFlashExpanded;
		internal static Material matHuntressFlashBright;

		internal static GameObject rocketProjectilePrefab;

		internal static GameObject huntressTracer;
		internal static GameObject railgunTracer;
		internal static GameObject muzzleFlashRailgun;
		public static Dictionary<string, string> ShaderSwap = new Dictionary<string, string>()
		{
			{"stubbed hopoo games/deferred/standard",  "HGStandard.shader"},
			{"stubbed hopoo games/fx/cloud remap",  "HGCloudRemap.shader"},
			{"stubbed hopoo games/fx/distortion",  "HGDistortion.shader"}
		};

		public static List<GameObject> clonedVanillaProjectiles = new List<GameObject>();
		public static string AssetBundlePath => System.IO.Path.Combine(System.IO.Path.GetDirectoryName(TrackerMain.PInfo.Location), "trackerassets");

		internal static void PopulateAssets()
		{
			mainAssetBundle = AssetBundle.LoadFromFile(AssetBundlePath);
			SwapShaders(mainAssetBundle);
			lineVisualiserPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/BasicThrowableVisualizer.prefab").WaitForCompletion();

			ContentAddition.AddEffect(mainAssetBundle.LoadAsset<GameObject>("Explosion"));
			ContentAddition.AddEffect(mainAssetBundle.LoadAsset<GameObject>("BigExplosion"));

			// xDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD
			rocketProjectilePrefab = mainAssetBundle.LoadAsset<GameObject>("RocketProjectile");

			sawProjectile = mainAssetBundle.LoadAsset<GameObject>("SawProjectile");
			rebounderProjectile = mainAssetBundle.LoadAsset<GameObject>("RebounderProjectile");


			/////////// trash TRASH TREASH TRASH
			GameObject mageghost = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Mage/MageLightningBombGhost.prefab").WaitForCompletion();
			rebounderProjectile.GetComponent<ProjectileController>().ghostPrefab = mageghost;

			GameObject lunarghost = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/LunarGolem/LunarGolemTwinShotProjectileGhost.prefab").WaitForCompletion();
			mainAssetBundle.LoadAsset<GameObject>("TrackerChargeProjectile").GetComponent<ProjectileController>().ghostPrefab = lunarghost;
			////////////////////////////
			///



			muzzleFlashRailgun = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/Railgunner/MuzzleflashRailgun.prefab").WaitForCompletion();

			matTPInOut = Addressables.LoadAssetAsync<Material>("RoR2/Base/Common/matTPInOut.mat").WaitForCompletion();
			matHuntressFlash = Addressables.LoadAssetAsync<Material>("RoR2/Base/Huntress/matHuntressFlash.mat").WaitForCompletion();
			matHuntressFlashBright = Addressables.LoadAssetAsync<Material>("RoR2/Base/Huntress/matHuntressFlashBright.mat").WaitForCompletion();
			matHuntressFlashExpanded = Addressables.LoadAssetAsync<Material>("RoR2/Base/Huntress/matHuntressFlashExpanded.mat").WaitForCompletion();

			railgunTracer = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/Railgunner/TracerRailgunSuper.prefab").WaitForCompletion();
			huntressTracer = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Huntress/TracerHuntressSnipe.prefab").WaitForCompletion();
		}

		internal static void SwapShaders(AssetBundle assetBundle)
		{
			Material[] mats = assetBundle.LoadAllAssets<Material>();

			foreach (Material mat in mats)
			{
				//Log.LogDebug(mat.name + "||" + mat.shader.name + " ----------------------------- ");
				if (!mat.shader.name.StartsWith("Stubbed")) continue;

				if (ShaderSwap.TryGetValue(mat.shader.name.ToLower(), out string shaderKey))
                {
					Shader shader = Addressables.LoadAssetAsync<Shader>("RoR2/Base/Shaders/" + shaderKey).WaitForCompletion();
					if (shader)
					{
						mat.shader = shader;
						//Log.LogInfo("Swapped shader for " + mat.name);
					}
				}

				
			}
		}
	}
}



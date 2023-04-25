using EntityStates;
using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace Tracker.States
{
	public class FireRebounder : BaseSkillState
	{

		public GameObject projectilePrefab = Assets.rebounderProjectile;
		public string soundString = Sounds.rocketLauncherShoot;
		public GameObject muzzleEffectPrefab = null;// Assets.muzzleFlashRocket;
		public static float bloom = 0.5f;
		public static float recoilAmplitude = 7f;
		public static float baseDuration = 1.5f;
		public static float earlyExitTime = 0.2f;
		public static float damageCoefficient = 3f;
		public static float force = 1500f;

		public float duration;
		public override void OnEnter()
		{
			base.OnEnter();
			this.duration = baseDuration / attackSpeedStat;
			StartAimMode();
			Fire();
			PlayAnimation("Gesture, Override", "FireSecondary", "fireBigBig.playbackRate", duration);
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (base.fixedAge >= this.duration && base.isAuthority)
			{
				outer.SetNextStateToMain();
			}
		}

		private void Fire()
		{
			Util.PlaySound(soundString, base.gameObject);
			EffectManager.SimpleMuzzleFlash(muzzleEffectPrefab, base.gameObject, "MuzzleBigGun", true);
			AddRecoil(-1f * recoilAmplitude, -1.5f * recoilAmplitude, -0.25f * recoilAmplitude, 0.25f * recoilAmplitude);
			base.characterBody.AddSpreadBloom(bloom);
			Ray aimRay = GetAimRay();
			if (base.isAuthority)
			{
				FireProjectileInfo fireProjectileInfo = default(FireProjectileInfo);
				fireProjectileInfo.projectilePrefab = projectilePrefab;
				fireProjectileInfo.position = aimRay.origin;
				fireProjectileInfo.rotation = Util.QuaternionSafeLookRotation(aimRay.direction);
				fireProjectileInfo.owner = base.gameObject;
				fireProjectileInfo.damage = damageStat * damageCoefficient;
				fireProjectileInfo.force = force;
				fireProjectileInfo.crit = RollCrit();
				ProjectileManager.instance.FireProjectile(fireProjectileInfo);
			}
		}

		public override InterruptPriority GetMinimumInterruptPriority()
		{
			return (base.fixedAge >= this.duration * earlyExitTime) ? InterruptPriority.Any : InterruptPriority.PrioritySkill;
		}
	}
}


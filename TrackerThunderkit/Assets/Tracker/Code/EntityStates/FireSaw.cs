using EntityStates;
using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace Tracker.States
{
	public class FireSaw : BaseSkillState
	{

		public GameObject projectilePrefab = Assets.sawProjectile;
		public string soundString = Sounds.rocketLauncherShoot;
		public GameObject muzzleEffectPrefab = null;// Assets.muzzleFlashRocket;
		public static float bloom = 0.5f;
		public static float recoilAmplitude = 7f;
		public static float fireTime = 0.45f;
		public static float baseDuration = 1.5f;
		public static float earlyExitTime = 0.5f;
		public static float damageCoefficientPerSecond = 15f;
		public static float force = 100f;

		private bool hasFired;
		public float duration;

		public static float smallHopForce = 9f;
		public static float antiGravityForce = 9f;
		public override void OnEnter()
		{
			base.OnEnter();
			this.duration = baseDuration / attackSpeedStat;
			StartAimMode();
			PlayAnimation("Gesture, Override", "FireSpecial", "fireSpecial.playbackRate", duration);

			if(!base.isGrounded)
			 base.SmallHop(base.characterMotor, 12f);
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (!this.hasFired && base.fixedAge >= this.duration * fireTime)
            {
				this.hasFired = true;
				this.Fire();
            }
			else
				base.characterMotor.velocity.y = base.characterMotor.velocity.y + FireSaw.antiGravityForce * Time.fixedDeltaTime * (1f - base.fixedAge / this.duration);

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
				fireProjectileInfo.damage = damageStat * damageCoefficientPerSecond;
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


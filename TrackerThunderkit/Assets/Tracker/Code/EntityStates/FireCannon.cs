using System;
using EntityStates;
using RoR2;

namespace Tracker.States
{
	public class FireCannon : GenericBulletBaseState
	{

		public float charge;

		public static float maxChargeDamageCoefficient = 6f;
		public static float minChargeDamageCoefficient = 1.5f;
		public override void OnEnter()
		{
			baseDuration = .33f;
			damageCoefficient = Util.Remap(charge, 0, 1, minChargeDamageCoefficient, maxChargeDamageCoefficient);
			bulletRadius = .8f;
			force = 1000f;
			tracerEffectPrefab = Assets.huntressTracer;
			maxDistance = 300f;
			muzzleName = "MuzzleBigGun";
			muzzleFlashPrefab = Assets.muzzleFlashRailgun;

			base.GetModelAnimator().SetFloat("cannonCharge", this.charge);

			base.OnEnter();
		}

		public override void DoFireEffects()
		{
			base.DoFireEffects();
			Util.PlaySound(Sounds.jetpackBurst, base.gameObject);
		}

		public override void ModifyBullet(BulletAttack bulletAttack)
		{
			bulletAttack.smartCollision = true;
			bulletAttack.tracerEffectPrefab = Assets.huntressTracer;
			bulletAttack.falloffModel = BulletAttack.FalloffModel.None;
		}

		public override void PlayFireAnimation()
		{
			base.PlayAnimation("Gesture, Override", "FirePrimary");
		}
	}
}



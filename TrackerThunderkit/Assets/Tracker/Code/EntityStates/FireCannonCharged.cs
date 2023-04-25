using System;
using EntityStates;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace Tracker.States
{
	public class FireCannonCharged : GenericBulletBaseState
	{

		public static float earlyExitTime = 0.25f;
		public static float selfKnockbackForce = 2200f;
		public override void OnEnter()
		{
			baseDuration = 1.5f;
			damageCoefficient = 10f;
			bulletRadius = 3f;
			force = 4000f;
			tracerEffectPrefab = Assets.railgunTracer;
			maxDistance = 1000f;
			muzzleName = "MuzzleBigGun";
			muzzleFlashPrefab = Assets.muzzleFlashRailgun;

			base.GetModelAnimator().SetFloat("cannonCharge", 1f);

			if(NetworkServer.active)
            {
				if (base.characterBody.HasBuff(TrackerContent.Buffs.chargeCannon))
					base.characterBody.RemoveBuff(TrackerContent.Buffs.chargeCannon);
			}
			

			base.OnEnter();
		}

        public override void FireBullet(Ray aimRay)
        {
            base.FireBullet(aimRay);

			if(base.isAuthority)
            {
				if(base.characterMotor && !base.characterMotor.isGrounded)
                {
					base.characterMotor.ApplyForce((aimRay.direction * -1f) * selfKnockbackForce);
                }
            }
        }

        public override void DoFireEffects()
		{
			base.DoFireEffects();
			Util.PlaySound(Sounds.railgunShoot, base.gameObject);
			Util.PlaySound(Sounds.railgunEquipStop, base.gameObject);
		}

		public override void ModifyBullet(BulletAttack bulletAttack)
		{
			bulletAttack.stopperMask = 0;
			bulletAttack.tracerEffectPrefab = Assets.railgunTracer;
			bulletAttack.falloffModel = BulletAttack.FalloffModel.None;
		}

		public override void PlayFireAnimation()
		{
			base.PlayAnimation("Gesture, Override", "FirePrimaryCharged");
		}

        public override InterruptPriority GetMinimumInterruptPriority()
        {
			return base.fixedAge >= this.duration * earlyExitTime ? InterruptPriority.Any : InterruptPriority.Skill;
        }

    }
}



using EntityStates;
using RoR2;
using UnityEngine.Networking;
using UnityEngine;

namespace Tracker.States
{
    public class ChargeCannon : BaseSkillState
    {
        public static float maxChargeDuration = 1f;
        public static float minBloomRadius = 0.1f;
        public static float maxBloomRadius = 0.5f;

        private float chargeDuration;

        private float charge;

        private Animator animator;
        public override void OnEnter()
        {
            base.OnEnter();

            base.StartAimMode(Mathf.Infinity);
            this.chargeDuration = maxChargeDuration / this.attackSpeedStat;
            this.animator = base.GetModelAnimator();
            this.animator.SetBool("aimCannon", true);


            Util.PlaySound(Sounds.shotgunChargeBomb, base.gameObject);

            if (NetworkServer.active)
            {
                if (base.characterBody.HasBuff(TrackerContent.Buffs.chargeCannon))
                    this.chargeDuration = 0f;
            }
            
            //sound
            //fx
            //crosshair


        }

        private EntityState GetNextState()
        {
            bool charged = base.fixedAge >= this.chargeDuration;

            if (charged) return new FireCannonCharged();

            return new FireCannon { charge = this.CalculateCharge() };
        }

        private float CalculateCharge()
        {
            return Mathf.Clamp01(base.fixedAge / this.chargeDuration);
        }

        public override void Update()
        {
            base.Update();
            base.characterBody.SetSpreadBloom(Util.Remap(this.CalculateCharge(), 0f, 1f, minBloomRadius, maxBloomRadius));
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();

            this.animator.SetFloat("cannonShake", this.CalculateCharge());
            if(!base.IsKeyDownAuthority())
            {
                this.outer.SetNextState(this.GetNextState());
            }

        }

        public override void OnExit()
        {
            base.OnExit();

            Util.PlaySound(Sounds.shotgunChargeBombStop, base.gameObject);

            base.StartAimMode();
            this.animator.SetFloat("cannonShake", 0f);
            this.animator.SetBool("aimCannon", false);
        }

    }
}

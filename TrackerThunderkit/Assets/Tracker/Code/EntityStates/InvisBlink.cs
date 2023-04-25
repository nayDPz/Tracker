using EntityStates;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using Tracker.Components;
using UnityEngine.Networking;

namespace Tracker.States
{
    public class InvisBlink : BaseSkillState
    {
        public static float baseDuration = 0.5f;
        private float blinkDuration;
        public float blinkDistance = 8f;
        public float invisDuration = 4f;

        private float blinkSpeed;
        private Vector3 blinkVector;
        private Transform modelTransform;
        private CharacterModel characterModel;
        private HurtBoxGroup hurtboxGroup;

        public override void OnEnter()
        {
            base.OnEnter();

            this.blinkVector = base.inputBank.moveVector;

            this.modelTransform = base.GetModelTransform();

            this.blinkDuration = baseDuration / this.moveSpeedStat;

            this.blinkSpeed = this.blinkDistance / this.blinkDuration;

            if (base.characterBody)
            {
                if (NetworkServer.active)
                {
                    base.characterBody.AddBuff(RoR2Content.Buffs.Cloak);
                    base.characterBody.AddBuff(RoR2Content.Buffs.CloakSpeed);
                }
                base.characterBody.onSkillActivatedAuthority += this.OnSkillActivatedAuthority;
            }

            if (this.modelTransform)
            {
                TemporaryOverlay temporaryOverlay = this.modelTransform.gameObject.AddComponent<TemporaryOverlay>();
                temporaryOverlay.duration = 0.25f;
                temporaryOverlay.animateShaderAlpha = true;
                temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                temporaryOverlay.destroyComponentOnEnd = true;
                temporaryOverlay.originalMaterial = Assets.matHuntressFlash;
                temporaryOverlay.AddToCharacerModel(this.modelTransform.GetComponent<CharacterModel>());

                this.characterModel = this.modelTransform.GetComponent<CharacterModel>();
                this.hurtboxGroup = this.modelTransform.GetComponent<HurtBoxGroup>();
            }

            if (this.characterModel)
            {
                this.characterModel.invisibilityCount = 1;
            }
            if (this.hurtboxGroup)
            {
                this.hurtboxGroup.hurtBoxesDeactivatorCounter++;
            }


            EffectData effectData = new EffectData();
            effectData.rotation = Util.QuaternionSafeLookRotation(this.blinkVector);
            effectData.origin = Util.GetCorePosition(base.gameObject);
            EffectManager.SpawnEffect(EntityStates.Huntress.BlinkState.blinkPrefab, effectData, false);

            Util.PlaySound(Sounds.blink, base.gameObject);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (base.fixedAge < this.blinkDuration && base.characterMotor && base.characterDirection)
            {
                base.characterMotor.velocity = Vector3.zero;
                base.characterMotor.rootMotion += this.blinkVector * this.blinkSpeed * Time.fixedDeltaTime;
            }
            if (base.fixedAge >= this.blinkDuration + this.invisDuration && base.isAuthority)
            {
                this.outer.SetNextStateToMain();
            }


        }

        private void OnSkillActivatedAuthority(GenericSkill skill)
		{
			if (skill.skillDef.isCombatSkill)
			{
				this.outer.SetNextStateToMain();
			}
		}
        public override void OnExit()
        {
            base.OnExit();

            if (this.modelTransform)
            {
                TemporaryOverlay temporaryOverlay = this.modelTransform.gameObject.AddComponent<TemporaryOverlay>();
                temporaryOverlay.duration = 0.6f;
                temporaryOverlay.animateShaderAlpha = true;
                temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                temporaryOverlay.destroyComponentOnEnd = true;
                temporaryOverlay.originalMaterial = Assets.matHuntressFlashBright;
                temporaryOverlay.AddToCharacerModel(this.modelTransform.GetComponent<CharacterModel>());
                TemporaryOverlay temporaryOverlay2 = this.modelTransform.gameObject.AddComponent<TemporaryOverlay>();
                temporaryOverlay2.duration = 0.7f;
                temporaryOverlay2.animateShaderAlpha = true;
                temporaryOverlay2.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                temporaryOverlay2.destroyComponentOnEnd = true;
                temporaryOverlay2.originalMaterial = Assets.matHuntressFlashExpanded;
                temporaryOverlay2.AddToCharacerModel(this.modelTransform.GetComponent<CharacterModel>());
            }
            if (this.characterModel)
            {
                this.characterModel.invisibilityCount--;
            }
            if (this.hurtboxGroup)
            {
                this.hurtboxGroup.hurtBoxesDeactivatorCounter--;
            }

            if (base.characterBody)
            {
                base.characterBody.onSkillActivatedAuthority -= this.OnSkillActivatedAuthority;
                if (NetworkServer.active)
                {
                    base.characterBody.RemoveBuff(RoR2Content.Buffs.CloakSpeed);
                    base.characterBody.RemoveBuff(RoR2Content.Buffs.Cloak);
                }
            }

            Util.PlaySound(Sounds.blinkEnd, base.gameObject);
        }


    }
}

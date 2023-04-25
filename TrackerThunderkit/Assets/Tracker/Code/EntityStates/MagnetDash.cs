using EntityStates;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using Tracker.Components;

namespace Tracker.States
{
    public class MagnetDash : BaseSkillState
    {

        //temp
        public static float dashForce = 3000f;
        public override void OnEnter()
        {
            base.OnEnter();

            base.characterMotor.ApplyForce(base.inputBank.moveVector * dashForce);
            ProjectileMagnetController.ProjectileMagnetOwnership component = base.GetComponent<ProjectileMagnetController.ProjectileMagnetOwnership>();
            if (component) component.MagnetizeAll();

            this.outer.SetNextStateToMain();
        }
    }
}

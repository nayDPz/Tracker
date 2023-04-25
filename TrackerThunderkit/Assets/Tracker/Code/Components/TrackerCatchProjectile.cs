using UnityEngine;
using RoR2;
using RoR2.Projectile;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace Tracker.Components
{
    public class TrackerCatchProjectile : MonoBehaviour
    {
        public static float gravity = 15f;

        public float radius = 3f;
        private ProjectileController controller;

        private Rigidbody rigidBody;
        private void Awake()
        {
            this.controller = base.GetComponent<ProjectileController>();
            this.rigidBody = base.GetComponent<Rigidbody>();
            this.rigidBody.useGravity = false;
        }


        private void FixedUpdate()
        {
            if(this.rigidBody)
            {
                this.rigidBody.velocity += Vector3.down * gravity * Time.fixedDeltaTime;
            }

            if (!this.controller.owner) return;

            if((base.transform.position - this.controller.owner.transform.position).magnitude <= this.radius)
            {
                this.Catch();
                
            }
        }

        public void Catch()
        {
            Util.PlaySound(Sounds.punchProjectile, base.gameObject);

            if (!this.controller.owner) return;


            CharacterBody body = this.controller.owner.GetComponent<CharacterBody>();

            if (NetworkServer.active)
            {
                body.AddBuff(TrackerContent.Buffs.chargeCannon);
            }

            Destroy(base.gameObject);
        }
    }
}

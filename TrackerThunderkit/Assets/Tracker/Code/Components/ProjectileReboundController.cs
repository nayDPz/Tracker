using UnityEngine;
using RoR2;
using RoR2.Projectile;

namespace Tracker.Components
{
    public class ProjectileReboundController : MonoBehaviour, IProjectileImpactBehavior
    {
        public float maxReboundOffset = 3f;
        public float timeToReboundTarget = 1f;
        public GameObject reboundProjectilePrefab;

        public ProjectileController controller;
        private GameObject owner;

        private Vector3 target;

        private bool inRebound;
        public float fireReboundTimer = 0.75f;

        private void Awake()
        {
            this.controller = base.GetComponent<ProjectileController>();
        }
        private void Start()
        {
            this.owner = this.controller.owner;
        }


        private void FixedUpdate()
        {
            if (!inRebound) return;
            this.fireReboundTimer -= Time.fixedDeltaTime;
            if (this.fireReboundTimer <= 0)
            {
                this.FireRebound();
                Destroy(base.gameObject);
            }
        }

        public void StartRebound()
        {
            this.inRebound = true;

            Util.PlaySound(Sounds.jetpackChargeUp, base.gameObject);
        }

        private void PickTarget()
        {
            if (!this.owner) return;

            CharacterMotor motor = this.owner.GetComponent<CharacterMotor>();

            if (!motor) return;

            Vector3 velocity = motor.velocity;
            velocity.y = 0f;
            this.target = (velocity * timeToReboundTarget) + this.owner.transform.position;
        }

        private void FireRebound()
        {
            Util.PlaySound(Sounds.jetpackBurst, base.gameObject);


            this.PickTarget();

            Vector3 between = this.target - base.transform.position;
            Vector3 betweenXZ = new Vector3(between.x, 0, between.z);
            Vector3 fireDirectionXZ = betweenXZ.normalized;
            float yOffset = this.target.y - base.transform.position.y;

            float xzSpeed = (betweenXZ / this.timeToReboundTarget).magnitude;
            float initialYSpeed = Trajectory.CalculateInitialYSpeed(this.timeToReboundTarget, yOffset, -TrackerCatchProjectile.gravity);

            
            Vector3 fireDirection = new Vector3(fireDirectionXZ.x * xzSpeed, initialYSpeed, fireDirectionXZ.z * xzSpeed);
            float projectileSpeed = fireDirection.magnitude;
            fireDirection = fireDirection.normalized;

            Quaternion rotation = Util.QuaternionSafeLookRotation(fireDirection);


            ProjectileManager.instance.FireProjectile(reboundProjectilePrefab, base.transform.position, rotation, this.owner, 0f, 0f, false, speedOverride: projectileSpeed);


        }


        // wont fucking destory on world for some reason
        public void OnProjectileImpact(ProjectileImpactInfo impactInfo)
        {
            Collider collider = impactInfo.collider;
            if (collider)
            {
                HurtBox component = collider.GetComponent<HurtBox>();
                if (!component)
                {
                    Destroy(base.gameObject);
                }
            }
        }
    }
}

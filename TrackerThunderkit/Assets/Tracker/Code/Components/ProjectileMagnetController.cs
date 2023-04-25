using UnityEngine;
using RoR2;
using RoR2.Projectile;
using System.Collections.Generic;
using UnityEngine.Events;
namespace Tracker.Components
{
    public class ProjectileMagnetController : MonoBehaviour
    {
		public UnityEvent catchEvent;
		public UnityEvent onMagnetize;
		public Rigidbody rigidBody;

		public string catchSoundString = Sounds.blink;
		public string returnSoundString = Sounds.v1Jump;
		public float catchHopVelocity = 8f;
		public float returnSpeed = 100f;
		public float returnDamage = 10f;
		public float catchRadius = 2.5f;

		private bool magnetized;
		private GameObject owner;
		private ProjectileDamage damage;

		private void Awake()
        {
			this.rigidBody = base.GetComponent<Rigidbody>();

			this.damage = base.GetComponent<ProjectileDamage>();

        }

		private void Start()
        {

			ProjectileController projectileController = base.GetComponent<ProjectileController>();
			this.owner = projectileController.owner;
			if (this.owner)
			{
				ProjectileMagnetOwnership component = this.owner.GetComponent<ProjectileMagnetOwnership>();
				if (!component)
				{
					component = this.owner.AddComponent<ProjectileMagnetOwnership>();
				}
				component.AddProjectile(this);
			}
		}

		private void FixedUpdate()
        {
			if(this.magnetized)
            {
				if(this.rigidBody)
                {
					Vector3 between = (this.owner.transform.position - base.transform.position);
					Vector3 direction = between.normalized;

					if(between.magnitude <= catchRadius)
                    {
						OnReturn();
						return;
                    }

					this.rigidBody.MovePosition(base.transform.position + direction * returnSpeed * Time.fixedDeltaTime);
                }
            }
        }

        public void Magnetize()
        {
			this.magnetized = true;

			if (onMagnetize != null)
				onMagnetize.Invoke();

			ProjectileSimple s = base.GetComponent<ProjectileSimple>();
			if (s) s.lifetime = 10f;

			if (this.rigidBody)
            {
				this.rigidBody.isKinematic = true;
            }

		}

		public void OnReturn()
        {
			this.magnetized = false;

			if (catchEvent != null)
				catchEvent.Invoke();

			CharacterMotor motor = owner.GetComponent<CharacterMotor>();
			if (motor && !motor.isGrounded) motor.velocity = new Vector3(motor.velocity.x, Mathf.Max(motor.velocity.y, catchHopVelocity), motor.velocity.z);

			Destroy(base.gameObject);
        }

		public class ProjectileMagnetOwnership : MonoBehaviour
		{
			public List<ProjectileMagnetController> projectiles = new List<ProjectileMagnetController>();
			private uint soundID;

			public void AddProjectile(ProjectileMagnetController component)
			{
				this.projectiles.Add(component);
			}

			private void FixedUpdate()
			{
				if (projectiles == null)
				{
					Destroy(this);
					return;
				}
				CleanList();
			}

			private void CleanList()
			{
				for (int i = projectiles.Count - 1; i >= 0; i--)
				{
					if (!this.projectiles[i])
					{
						this.projectiles.RemoveAt(i);
					}
				}
			}

			public void MagnetizeAll()
			{
				soundID = Util.PlaySound(Sounds.blink, base.gameObject);
				foreach (ProjectileMagnetController projectile in projectiles)
				{
					projectile.Magnetize();
				}
			}

			private void OnDestroy()
			{
				AkSoundEngine.StopPlayingID(soundID);
			}
		}
	}
}

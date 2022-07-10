using System;
using UnityEngine;

public class VPAnimator : VehiclePart
{
	protected bool hasPlayer = false;

	public int zeroVelocity = 0;

	protected Animator animator;

	protected EntityPlayerLocal player;

    public override void SetProperties(DynamicProperties _properties)
	{
		base.SetProperties(_properties);

		player = GameManager.Instance.World.GetPrimaryPlayer();
	}

	public override void InitPrefabConnections()
	{
		base.InitPrefabConnections();
		Transform animTransform = GetTransform();
		if (animTransform)
		{
			animator = animTransform.GetComponentInParent<Animator>();
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);

		if (!hasPlayer)
		{
			if (vehicle.entity.HasDriver && player && vehicle.entity.AttachedMainEntity.entityId == player.entityId)
			{
				OnPlayerEnter();
			}
			else
			{
				return;
			}
		}

		if (!vehicle.entity.HasDriver)
		{
			OnPlayerExit();
			return;
		}

		if (this.IsBroken())
		{
			return;
		}
		EntityAlive entityAlive = this.vehicle.entity.AttachedMainEntity as EntityAlive;
		if (entityAlive)
		{
			entityAlive.CurrentMovementTag = EntityAlive.MovementTagDriving;
			if (this.vehicle.CurrentIsAccel)
			{
				if (this.vehicle.IsTurbo)
				{
					RunStart();
				}
				if (!this.vehicle.IsTurbo && this.vehicle.CurrentIsAccel)
                {
                    RunStop();
                }
                WalkStart();
			}
		}
		if (!this.vehicle.CurrentIsAccel && this.vehicle.CurrentVelocity.z <= zeroVelocity)
        {
            MovementStop();
        }
    }

    protected virtual void OnPlayerEnter()
	{
		hasPlayer = true;
	}

	protected virtual void OnPlayerExit()
	{
		hasPlayer = false;
		if (!hasPlayer)
		{
			animator.SetBool("walk", false);
			animator.SetBool("run", false);
		}
	}

	private void RunStart()
	{
		if (hasPlayer)
		{
			animator.SetBool("run", true);
		}
		else
		{
			animator.SetBool("run", false);
		}
	}

	private void RunStop()
	{
		animator.SetBool("run", false);
	}

	private void WalkStart()
	{
		if (hasPlayer)
		{
			animator.SetBool("walk", true);
		}
	}

	private void MovementStop()
	{
		animator.SetBool("walk", false);
		animator.SetBool("run", false);
	}
}

using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000333 RID: 819
public class ExplosionDamageArea : MonoBehaviour
{
	// Token: 0x060025E7 RID: 9703 RVA: 0x00107245 File Offset: 0x00105445
	private void Awake()
	{
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			base.enabled = false;
		}
	}

	// Token: 0x060025E8 RID: 9704 RVA: 0x0010725C File Offset: 0x0010545C
	private Entity getEntityFromCollider(Collider col)
	{
		Transform transform = col.transform;
		if (!transform.tag.StartsWith("E_") && !transform.CompareTag("Item"))
		{
			return null;
		}
		if (transform.CompareTag("Item"))
		{
			return null;
		}
		Transform transform2 = null;
		if (transform.tag.StartsWith("E_BP_"))
		{
			transform2 = GameUtils.GetHitRootTransform(transform.tag, transform);
		}
		EntityAlive entityAlive = (transform2 != null) ? transform2.GetComponent<EntityAlive>() : null;
		if (entityAlive == null || entityAlive.IsDead())
		{
			return null;
		}
		return entityAlive;
	}

	// Token: 0x060025E9 RID: 9705 RVA: 0x001072E8 File Offset: 0x001054E8
	private void OnTriggerEnter(Collider other)
	{
		EntityAlive entityAlive = this.getEntityFromCollider(other) as EntityAlive;
		if (entityAlive == null)
		{
			return;
		}
		if (this.BuffActions != null)
		{
			for (int i = 0; i < this.BuffActions.Count; i++)
			{
				entityAlive.Buffs.AddBuff(this.BuffActions[i], -1, true, false, false);
			}
		}
	}

	// Token: 0x040018EB RID: 6379
	public List<string> BuffActions;

	// Token: 0x040018EC RID: 6380
	public int InitiatorEntityId = -1;
}

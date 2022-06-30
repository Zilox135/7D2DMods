using System;
using System.Collections.Generic;
using UnityEngine;

public class BlockPowerUp : BlockHay
{
	protected ExplosionData explosion;

	public override void Init()
	{
		base.Init();
		this.explosion = new ExplosionData(this.Properties);
	}

	public override bool OnEntityCollidedWithBlock(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, Entity _targetEntity)
	{
		if (this.Properties.Values.ContainsKey("Buff"))
		{
			if (_targetEntity as EntityAlive != null)
			{
				string[] array = this.Properties.Values["Buff"].Split(new char[]
				{
					','
				});
				if (_targetEntity as EntityAlive != null)
				{
					for (int i = 0; i < array.Length; i++)
					{
						EntityAlive entity = _targetEntity as EntityAlive;
						entity.Buffs.AddBuff(array[i].Trim(), _targetEntity.entityId, true, false, false);
						this.explode(_world, _clrIdx, _blockPos.ToVector3());
						_world.SetBlockRPC(_clrIdx, _blockPos, BlockValue.Air);
					}
				}
			}
		}
		return true;
	}

	public override int OnBlockDamaged(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, int _damagePoints, int _entityIdThatDamaged, bool _bUseHarvestTool, bool _bBypassMaxDamage, int _recDepth = 0)
	{
		if (_damagePoints > 0 && this.Properties.Values.ContainsKey("Buff"))
		{
			EntityAlive entityAlive = _world.GetEntity(_entityIdThatDamaged) as EntityAlive;
			if (entityAlive != null && entityAlive as EntityTurret == null)
			{
				ItemAction itemAction = entityAlive.inventory.holdingItemData.item.Actions[0];
				if (!(itemAction is ItemActionRanged) || (itemAction is ItemActionRanged && (itemAction as ItemActionRanged).HitmaskOverride != null && (itemAction as ItemActionRanged).HitmaskOverride.Value == "Melee"))
				{
					string[] array = this.Properties.Values["Buff"].Split(new char[]
					{
						','
					});
					if (entityAlive != null)
					{
						for (int i = 0; i < array.Length; i++)
						{
							entityAlive.Buffs.AddBuff(array[i].Trim(), entityAlive.entityId, true, false, false);
							this.explode(_world, _clrIdx, _blockPos.ToVector3());
							_world.SetBlockRPC(_clrIdx, _blockPos, BlockValue.Air);
						}
					}
				}
			}
		}

		return base.OnBlockDamaged(_world, _clrIdx, _blockPos, _blockValue, _damagePoints, _entityIdThatDamaged, _bUseHarvestTool, _bBypassMaxDamage, _recDepth);
	}

	private void explode(WorldBase _world, int _clrIdx, Vector3 _pos)
	{
		ChunkCluster chunkCluster = _world.ChunkClusters[_clrIdx];
		if (chunkCluster != null)
		{
			_pos = chunkCluster.ToWorldPosition(_pos + new Vector3(0.5f, 0.5f, 0.5f));
		}
		_world.GetGameManager().ExplosionServer(_clrIdx, _pos, World.worldToBlockPos(_pos), Quaternion.identity, this.explosion, -1, 0.1f, true, null);
	}

	public override bool UpdateTick(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, bool _bRandomTick, ulong _ticksIfLoaded, GameRandom _rnd)
	{
		this.explode(_world, _clrIdx, _blockPos.ToVector3());
		return true;
	}
}

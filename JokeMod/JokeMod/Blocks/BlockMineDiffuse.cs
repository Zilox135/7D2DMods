using System;
using System.Collections.Generic;
using System.Globalization;
using Audio;
using Platform;
using UnityEngine;

public class BlockMineDiffuse : Block
{
	protected static string PropDiffuseTime = "Diffuse.Time";

	protected static string PropDiffuseItem = "Diffuse.Item";

	protected static string PropDiffuseFailChance = "Diffuse.FailChance";

	protected float diffuseTime;

	protected string diffuseItem;

	protected float diffuseFailChance;

	public float DiffuseTimeLeft = -1f;


	protected static string PropDiffuseStartSound = "Diffuse.StartSound";

	private string DiffuseStartSound = "stonedestroy";

	protected static string PropDiffuseFailSound = "Diffuse.FailSound";

	private string DiffuseFailSound = "stonedestroy";

	protected static string PropDiffuseSuccessSound = "Diffuse.SuccessSound";

	private string DiffuseSuccessSound = "stonedestroy";

	private BlockActivationCommand[] cmds = new BlockActivationCommand[]
	{
		new BlockActivationCommand("diffuse", "wrench", false, false)
	};


	protected static string PropTriggerDelay = "TriggerDelay";

	protected static string PropTriggerSound = "TriggerSound";

	protected ExplosionData explosion;

	private float TriggerDelay = 0.6f;

	private string TriggerSound = "landmine_trigger";

	private float BaseEntityDamage;

	public override bool AllowBlockTriggers
	{
		get
		{
			return true;
		}
	}

	public override void Init()
	{
		base.Init();
		if (this.Properties.Values.ContainsKey(BlockMineDiffuse.PropDiffuseTime))
		{
			this.diffuseTime = StringParsers.ParseFloat(this.Properties.Values[BlockMineDiffuse.PropDiffuseTime], 0, -1, NumberStyles.Any);
		}
		else
		{
			this.diffuseTime = 15f;
		}
		if (this.Properties.Values.ContainsKey(BlockMineDiffuse.PropDiffuseItem))
		{
			this.diffuseItem = this.Properties.Values[BlockMineDiffuse.PropDiffuseItem];
		}
		if (this.Properties.Values.ContainsKey(BlockMineDiffuse.PropDiffuseFailChance))
		{
			this.diffuseFailChance = StringParsers.ParseFloat(this.Properties.Values[BlockMineDiffuse.PropDiffuseFailChance], 0, -1, NumberStyles.Any);
		}
		if (this.Properties.Values.ContainsKey(BlockMineDiffuse.PropDiffuseStartSound))
		{
			this.DiffuseStartSound = this.Properties.Values[BlockMineDiffuse.PropDiffuseStartSound];
		}
		if (this.Properties.Values.ContainsKey(BlockMineDiffuse.PropDiffuseFailSound))
		{
			this.DiffuseFailSound = this.Properties.Values[BlockMineDiffuse.PropDiffuseFailSound];
		}
		if (this.Properties.Values.ContainsKey(BlockMineDiffuse.PropDiffuseSuccessSound))
		{
			this.DiffuseSuccessSound = this.Properties.Values[BlockMineDiffuse.PropDiffuseSuccessSound];
		}
		else
		{
			this.diffuseFailChance = 0f;
		}

		this.explosion = new ExplosionData(this.Properties);
		this.BaseEntityDamage = this.explosion.EntityDamage;
		if (this.Properties.Values.ContainsKey(BlockMineDiffuse.PropTriggerDelay))
		{
			this.TriggerDelay = StringParsers.ParseFloat(this.Properties.Values[BlockMineDiffuse.PropTriggerDelay], 0, -1, NumberStyles.Any);
		}
		if (this.Properties.Values.ContainsKey(BlockMineDiffuse.PropTriggerSound))
		{
			this.TriggerSound = this.Properties.Values[BlockMineDiffuse.PropTriggerSound];
		}
	}

	public override string GetActivationText(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		PlayerActionsLocal playerInput = ((EntityPlayerLocal)_entityFocusing).playerInput;
		string arg = playerInput.Activate.GetBindingXuiMarkupString(XUiUtils.EmptyBindingStyle.EmptyString, XUiUtils.DisplayStyle.Plain, null) + playerInput.PermanentActions.Activate.GetBindingXuiMarkupString(XUiUtils.EmptyBindingStyle.EmptyString, XUiUtils.DisplayStyle.Plain, null);
		string localizedBlockName = _blockValue.Block.GetLocalizedBlockName();
		return string.Format(Localization.Get("tooltipDiffuse"), arg, localizedBlockName);
	}

	public override BlockActivationCommand[] GetBlockActivationCommands(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		this.cmds[0].enabled = (this.diffuseItem != null);
		return this.cmds;
	}

	public override void OnBlockAdded(WorldBase world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
	{
		base.OnBlockAdded(world, _chunk, _blockPos, _blockValue);
		if (_blockValue.ischild)
		{
			return;
		}
	}

	public override void OnBlockRemoved(WorldBase world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
	{
		base.OnBlockRemoved(world, _chunk, _blockPos, _blockValue);
	}

	public override Block.DestroyedResult OnBlockDestroyedBy(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, int _entityId, bool _bUseHarvestTool)
	{
		return Block.DestroyedResult.Downgrade;
	}

	public override bool OnBlockActivated(int _indexInBlockActivationCommands, WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _player)
	{
		if (_blockValue.ischild)
		{
			Vector3i parentPos = _blockValue.Block.multiBlockPos.GetParentPos(_blockPos, _blockValue);
			BlockValue block = _world.GetBlock(parentPos);
			return this.OnBlockActivated(_indexInBlockActivationCommands, _world, _cIdx, parentPos, block, _player);
		}
		switch (_indexInBlockActivationCommands)
		{
			case 0:
				{
					LocalPlayerUI playerUI = (_player as EntityPlayerLocal).PlayerUI;
					ItemValue item = ItemClass.GetItem(this.diffuseItem, false);
					if (playerUI.xui.PlayerInventory.GetItemCount(item) == 0)
					{
						playerUI.xui.CollectedItemList.AddItemStack(new ItemStack(item, 0), true);
						GameManager.ShowTooltip(_player as EntityPlayerLocal, Localization.Get("ttMissingDiffuseItem"));
						return true;
					}
					playerUI.windowManager.Open("timer", true, false, true);
					XUiC_Timer childByType = playerUI.xui.GetChildByType<XUiC_Timer>();
					TimerEventData timerEventData = new TimerEventData();
					timerEventData.CloseEvent += this.EventData_CloseEvent;
					float alternateTime = -1f;
					if (_player.rand.RandomRange(1f) < EffectManager.GetValue(PassiveEffects.LockPickBreakChance, _player.inventory.holdingItemItemValue, this.diffuseFailChance, _player, null, default(FastTags), true, true, true, true, 1, true))
					{
						float value = EffectManager.GetValue(PassiveEffects.LockPickTime, _player.inventory.holdingItemItemValue, this.diffuseTime, _player, null, default(FastTags), true, true, true, true, 1, true);
						float num = value - ((DiffuseTimeLeft == -1f) ? (value - 1f) : (DiffuseTimeLeft + 1f));
						alternateTime = _player.rand.RandomRange(num + 1f, value - 1f);
					}
					timerEventData.Data = new object[]
					{
				_cIdx,
				_blockValue,
				_blockPos,
				_player,
				item
					};
					timerEventData.Event += this.EventData_Event;
					timerEventData.alternateTime = alternateTime;
					timerEventData.AlternateEvent += this.EventData_CloseEvent;
					childByType.SetTimer(EffectManager.GetValue(PassiveEffects.LockPickTime, _player.inventory.holdingItemItemValue, this.diffuseTime, _player, null, default(FastTags), true, true, true, true, 1, true), timerEventData, DiffuseTimeLeft, "");
					Manager.BroadcastPlayByLocalPlayer(_blockPos.ToVector3() + Vector3.one * 0.5f, DiffuseStartSound);
					return true;
				}
			default:
				return false;
		}
	}

	private void EventData_CloseEvent(TimerEventData timerData)
	{
		object[] array = (object[])timerData.Data;
		Vector3i blockPos = (Vector3i)array[2];
		EntityPlayerLocal entityPlayerLocal = array[3] as EntityPlayerLocal;
		ItemValue itemValue = array[4] as ItemValue;
		LocalPlayerUI uiforPlayer = LocalPlayerUI.GetUIForPlayer(entityPlayerLocal);
		Manager.BroadcastPlayByLocalPlayer(blockPos.ToVector3() + Vector3.one * 0.5f, DiffuseFailSound);
		ItemStack itemStack = new ItemStack(itemValue, 1);
		uiforPlayer.xui.PlayerInventory.RemoveItem(itemStack);
		GameManager.ShowTooltip(entityPlayerLocal, Localization.Get("ttDiffuseItemBroken"));
		uiforPlayer.xui.CollectedItemList.RemoveItemStack(itemStack);
		TileEntitySecureLootContainer tileEntitySecureLootContainer = GameManager.Instance.World.GetTileEntity((int)array[0], blockPos) as TileEntitySecureLootContainer;
		if (tileEntitySecureLootContainer == null)
		{
			return;
		}
		tileEntitySecureLootContainer.PickTimeLeft = Mathf.Max(this.diffuseTime * 0.25f, timerData.timeLeft);
		this.ResetEventData(timerData);
	}

	private void EventData_Event(TimerEventData timerData)
	{
		World world = GameManager.Instance.World;
		object[] array = (object[])timerData.Data;
		int clrIdx = (int)array[0];
		BlockValue blockValue = (BlockValue)array[1];
		Vector3i vector3i = (Vector3i)array[2];
		BlockValue block = world.GetBlock(vector3i);
		EntityPlayerLocal entityPlayerLocal = array[3] as EntityPlayerLocal;
		object obj = array[4];
		LocalPlayerUI.GetUIForPlayer(entityPlayerLocal);
		if (!this.DowngradeBlock.isair)
		{
			BlockValue blockValue2 = this.DowngradeBlock;
			blockValue2 = BlockPlaceholderMap.Instance.Replace(blockValue2, world.GetGameRandom(), vector3i.x, vector3i.z, false);
			blockValue2.rotation = block.rotation;
			blockValue2.meta = block.meta;
			world.SetBlockRPC(clrIdx, vector3i, blockValue2, blockValue2.Block.Density);
		}
		Manager.BroadcastPlayByLocalPlayer(vector3i.ToVector3() + Vector3.one * 0.5f, DiffuseSuccessSound);
		this.ResetEventData(timerData);
	}

	public override bool OnBlockActivated(WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _player)
	{
		if (_blockValue.ischild)
		{
			Vector3i parentPos = _blockValue.Block.multiBlockPos.GetParentPos(_blockPos, _blockValue);
			BlockValue block = _world.GetBlock(parentPos);
			return this.OnBlockActivated(_world, _cIdx, parentPos, block, _player);
		}
		return true;
	}

	public override bool IsWaterBlocked(IBlockAccess _world, Vector3i _blockPos, BlockValue _blockValue, BlockFaceFlag _sides)
	{
		return true;
	}

	private void ResetEventData(TimerEventData timerData)
	{
		timerData.AlternateEvent -= this.EventData_CloseEvent;
		timerData.CloseEvent -= this.EventData_CloseEvent;
		timerData.Event -= this.EventData_Event;
	}

	public override void OnTriggered(WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, List<BlockChangeInfo> _blockChanges)
	{
		base.OnTriggered(_world, _cIdx, _blockPos, _blockValue, _blockChanges);
		if (!this.DowngradeBlock.isair)
		{
			BlockValue blockValue = this.DowngradeBlock;
			blockValue = BlockPlaceholderMap.Instance.Replace(blockValue, _world.GetGameRandom(), _blockPos.x, _blockPos.z, false);
			blockValue.rotation = _blockValue.rotation;
			blockValue.meta = _blockValue.meta;
			_world.SetBlockRPC(_cIdx, _blockPos, blockValue, blockValue.Block.Density);
		}
	}

	public override void OnEntityWalking(WorldBase _world, int _x, int _y, int _z, BlockValue _blockValue, Entity entity)
	{
		if (EffectManager.GetValue(PassiveEffects.LandMineImmunity, null, 0f, entity as EntityAlive, null, default(FastTags), true, true, true, true, 1, true) == 0f)
		{
			if (entity as EntityPlayer != null)
			{
				if ((entity as EntityPlayer).IsSpectator)
				{
					return;
				}
				GameManager.Instance.PlaySoundAtPositionServer(new Vector3((float)_x, (float)_y, (float)_z), this.TriggerSound, (AudioRolloffMode)1, 5, entity.entityId);
			}
			float num = this.TriggerDelay;
			if (entity as EntityAlive != null)
			{
				num = EffectManager.GetValue(PassiveEffects.LandMineTriggerDelay, null, this.TriggerDelay, entity as EntityAlive, null, default(FastTags), true, true, true, true, 1, true);
			}
			this.explosion.EntityDamage = EffectManager.GetValue(PassiveEffects.TrapIncomingDamage, null, this.BaseEntityDamage, entity as EntityAlive, null, default(FastTags), true, true, true, true, 1, true);
			_world.GetWBT().AddScheduledBlockUpdate((_world.GetChunkFromWorldPos(_x, _y, _z) as Chunk).ClrIdx, new Vector3i(_x, _y, _z), this.blockID, (ulong)(num * 20f));
		}
	}

	public override int OnBlockDamaged(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, int _damagePoints, int _entityId, bool _bUseHarvestTool, bool _bBypassMaxDamage, int _recDepth = 0)
	{
		if (_world.GetGameRandom().RandomFloat <= (float)Mathf.Clamp(Mathf.Clamp(_damagePoints, 0, _blockValue.Block.MaxDamage - 1), 1, 30000) / (float)_blockValue.Block.MaxDamage)
		{
			this.explode(_world, _clrIdx, _blockPos.ToVector3(), _entityId);
		}
		return _blockValue.damage;
	}

	public override Block.DestroyedResult OnBlockDestroyedByExplosion(WorldBase _world, int _clrIdx, Vector3i _pos, BlockValue _blockValue, int _playerIdx)
	{
		if (_world.GetGameRandom().RandomFloat < 0.33f)
		{
			this.explode(_world, _clrIdx, _pos.ToVector3(), _playerIdx);
			return Block.DestroyedResult.Remove;
		}
		return Block.DestroyedResult.Keep;
	}

	private void explode(WorldBase _world, int _clrIdx, Vector3 _pos, int _entityId)
	{
		ChunkCluster chunkCluster = _world.ChunkClusters[_clrIdx];
		if (chunkCluster != null)
		{
			_pos = chunkCluster.ToWorldPosition(_pos + new Vector3(0.5f, 0.5f, 0.5f));
		}
		_world.GetGameManager().ExplosionServer(_clrIdx, _pos, World.worldToBlockPos(_pos), Quaternion.identity, this.explosion, -1, 0.1f, true, null);
	}

	public override bool IsMovementBlocked(IBlockAccess _world, Vector3i _blockPos, BlockValue blockDef, BlockFace face)
	{
		return false;
	}

	public override bool UpdateTick(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, bool _bRandomTick, ulong _ticksIfLoaded, GameRandom _rnd)
	{
		this.explode(_world, _clrIdx, _blockPos.ToVector3(), -1);
		return true;
	}
}

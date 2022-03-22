using System;
using System.Collections.Generic;
using Platform;

public class BlockRizcoPC : Block
{

	private float TakeDelay = 2f;

	protected static string PropTraderID = "TraderID";

	protected int traderID;

	private List<int> buffActions;

	private BlockActivationCommand[] cmds = new BlockActivationCommand[]
	{
		new BlockActivationCommand("trade", "vending", false, false),
		new BlockActivationCommand("keypad", "keypad", false, false),
		new BlockActivationCommand("take", "hand", false, false)
	};

	public BlockRizcoPC()
	{
		this.HasTileEntity = true;
	}

	public override void Init()
	{
		base.Init();
		if (!this.Properties.Values.ContainsKey(BlockRizcoPC.PropTraderID))
		{
			throw new Exception("Block with name " + base.GetBlockName() + " doesnt have a trader ID.");
		}
		int.TryParse(this.Properties.Values[BlockRizcoPC.PropTraderID], out this.traderID);
	}

	// When the block is placed down in the world
	public override void PlaceBlock(WorldBase _world, BlockPlacement.Result _result, EntityAlive _ea)
	{
		base.PlaceBlock(_world, _result, _ea);
		TileEntityVendingMachine tileEntityVendingMachine = _world.GetTileEntity(_result.clrIdx, _result.blockPos) as TileEntityVendingMachine;
		if (tileEntityVendingMachine != null && _ea != null && _ea.entityType == EntityType.Player && TraderInfo.traderInfoList[this.traderID].PlayerOwned)
		{
			tileEntityVendingMachine.SetOwner(PlatformManager.InternalLocalUserIdentifier);
		}
	}

	// When the player is looking at the block
	public override string GetActivationText(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		TileEntityVendingMachine tileEntityVendingMachine = _world.GetTileEntity(_clrIdx, _blockPos) as TileEntityVendingMachine;
		PlayerActionsLocal playerInput = ((EntityPlayerLocal)_entityFocusing).playerInput;
		if (tileEntityVendingMachine != null)
		{
			string arg = playerInput.Activate.GetBindingXuiMarkupString(XUiUtils.EmptyBindingStyle.EmptyString, XUiUtils.DisplayStyle.Plain, null) + playerInput.PermanentActions.Activate.GetBindingXuiMarkupString(XUiUtils.EmptyBindingStyle.EmptyString, XUiUtils.DisplayStyle.Plain, null);
			string arg2 = _blockValue.Block.GetLocalizedBlockName();
			if ((tileEntityVendingMachine.IsRentable || tileEntityVendingMachine.TraderData.TraderInfo.PlayerOwned) && tileEntityVendingMachine.GetOwner() != null)
			{
				PersistentPlayerData playerData = GameManager.Instance.persistentPlayers.GetPlayerData(tileEntityVendingMachine.GetOwner());
				string arg3;
				if (playerData != null)
				{
					arg3 = GameUtils.SafeStringFormat(playerData.PlayerName);
				}
				else
				{
					arg3 = Localization.Get("sleepingBagPlayerUnknown");
				}
				arg2 = string.Format(Localization.Get("xuiVendingWithOwner"), arg3);
			}
			return string.Format(Localization.Get("vendingMachineActivate"), arg, arg2);
		}
		return "";
	}

	// Activate radial menu commands
	public override BlockActivationCommand[] GetBlockActivationCommands(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		TileEntityVendingMachine tileEntityVendingMachine = _world.GetTileEntity(_clrIdx, _blockPos) as TileEntityVendingMachine;
		if (tileEntityVendingMachine == null)
		{
			return new BlockActivationCommand[0];
		}
		PlatformUserIdentifierAbs internalLocalUserIdentifier = PlatformManager.InternalLocalUserIdentifier;
		PersistentPlayerData playerData = _world.GetGameManager().GetPersistentPlayerList().GetPlayerData(tileEntityVendingMachine.GetOwner());
		bool flag = tileEntityVendingMachine.LocalPlayerIsOwner();
		if (!flag)
		{
			if (playerData != null && playerData.ACL != null)
			{
				playerData.ACL.Contains(internalLocalUserIdentifier);
			}
		}
		bool playerOwned = TraderInfo.traderInfoList[this.traderID].PlayerOwned;
		this.cmds[0].enabled = true;
		this.cmds[1].enabled = (playerOwned && flag && tileEntityVendingMachine.TraderData.PrimaryInventory.Count == 0);
		this.cmds[2].enabled = (_world.IsMyLandProtectedBlock(_blockPos, _world.GetGameManager().GetPersistentLocalPlayer(), false) && (double)this.TakeDelay > 0.0);
		return this.cmds;
	}

	// After the block is placed
	public override void OnBlockAdded(WorldBase world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
	{
		base.OnBlockAdded(world, _chunk, _blockPos, _blockValue);
		if (_blockValue.ischild)
		{
			return;
		}
		if (!(world.GetTileEntity(_chunk.ClrIdx, _blockPos) is TileEntityVendingMachine))
		{
			_chunk.AddTileEntity(new TileEntityVendingMachine(_chunk)
			{
				localChunkPos = World.toBlock(_blockPos),
				//TraderData = new TraderData(),
				TraderData =
				{
					TraderID = this.traderID
				}
			});
		}
	}

	// When the block is destroyed
	public override void OnBlockRemoved(WorldBase world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
	{
		base.OnBlockRemoved(world, _chunk, _blockPos, _blockValue);
		_chunk.RemoveTileEntityAt<TileEntityVendingMachine>((World)world, _blockPos);
	}

	// After block is destroyed
	public override Block.DestroyedResult OnBlockDestroyedBy(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, int _entityId, bool _bUseHarvestTool)
	{
		TileEntityVendingMachine tileEntityVendingMachine = _world.GetTileEntity(_clrIdx, _blockPos) as TileEntityVendingMachine;
		return Block.DestroyedResult.Downgrade;
	}

	// Buying and selling vending machine function
	public override bool OnBlockActivated(WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _player)
	{
		if (_world.GetBlock(_blockPos.x, _blockPos.y - 1, _blockPos.z).Block.HasTag(BlockTags.Door))
		{
			_blockPos = new Vector3i(_blockPos.x, _blockPos.y - 1, _blockPos.z);
			return this.OnBlockActivated(_world, _cIdx, _blockPos, _blockValue, _player);
		}
		TileEntityVendingMachine tileEntityVendingMachine = _world.GetTileEntity(_cIdx, _blockPos) as TileEntityVendingMachine;
		if (tileEntityVendingMachine == null)
		{
			return false;
		}
		_player.AimingGun = false;
		Vector3i blockPos = tileEntityVendingMachine.ToWorldPos();
		_world.GetGameManager().TELockServer(_cIdx, blockPos, tileEntityVendingMachine.entityId, _player.entityId, null);
		return true;
	}

	// Pick up vending machine function
	public override bool OnBlockActivated(int _indexInBlockActivationCommands, WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _player)
	{
		if (_indexInBlockActivationCommands == 0)
		{
			return this.OnBlockActivated(_world, _cIdx, _blockPos, _blockValue, _player);
		}
		if (_indexInBlockActivationCommands != 2)
		{
			return false;
		}
		this.TakeItemWithTimer(_cIdx, _blockPos, _blockValue, _player);
		return true;
	}

	// Pick up item with the timer
	public void TakeItemWithTimer(int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _player)
	{
		if (_blockValue.damage > 0)
		{
			GameManager.ShowTooltip(_player as EntityPlayerLocal, Localization.Get("ttRepairBeforePickup"), string.Empty, "ui_denied", null);
			return;
		}
		LocalPlayerUI playerUI = (_player as EntityPlayerLocal).PlayerUI;
		playerUI.windowManager.Open("timer", true, false, true);
		XUiC_Timer childByType = playerUI.xui.GetChildByType<XUiC_Timer>();
		TimerEventData timerEventData = new TimerEventData();
		timerEventData.Data = new object[]
		{
			_cIdx,
			_blockValue,
			_blockPos,
			_player
		};
		timerEventData.Event += this.EventData_Event;
		childByType.SetTimer(this.TakeDelay, timerEventData, -1f, "");
	}

	// Pickup item event, puts it into the player inventory 
	private void EventData_Event(TimerEventData timerData)
	{
		World world = GameManager.Instance.World;
		object[] array = (object[])timerData.Data;
		int clrIdx = (int)array[0];
		BlockValue blockValue = (BlockValue)array[1];
		Vector3i vector3i = (Vector3i)array[2];
		BlockValue block = world.GetBlock(vector3i);
		EntityPlayerLocal entityPlayerLocal = array[3] as EntityPlayerLocal;
		if (block.damage > 0)
		{
			GameManager.ShowTooltip(entityPlayerLocal, Localization.Get("ttRepairBeforePickup"), string.Empty, "ui_denied", null);
			return;
		}
		if (block.type != blockValue.type)
		{
			GameManager.ShowTooltip(entityPlayerLocal, Localization.Get("ttBlockMissingPickup"), string.Empty, "ui_denied", null);
			return;
		}
		TileEntityVendingMachine tileEntityVendingMachine = world.GetTileEntity(clrIdx, vector3i) as TileEntityVendingMachine;
		if (tileEntityVendingMachine.IsUserAccessing())
		{
			GameManager.ShowTooltip(entityPlayerLocal, Localization.Get("ttCantPickupInUse"), string.Empty, "ui_denied", null);
			return;
		}
		LocalPlayerUI uiforPlayer = LocalPlayerUI.GetUIForPlayer(entityPlayerLocal);
		ItemStack itemStack = new ItemStack(block.ToItemValue(), 1);
		if (!uiforPlayer.xui.PlayerInventory.AddItem(itemStack))
		{
			uiforPlayer.xui.PlayerInventory.DropItem(itemStack);
		}
		world.SetBlockRPC(clrIdx, vector3i, BlockValue.Air);
	}

	// When the block takes damage, adding buffs
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
						}
					}
				}
			}
		}
		return base.OnBlockDamaged(_world, _clrIdx, _blockPos, _blockValue, _damagePoints, _entityIdThatDamaged, _bUseHarvestTool, _bBypassMaxDamage, _recDepth);
	}

	// Prevent it from being used in water
	public override bool IsWaterBlocked(IBlockAccess _world, Vector3i _blockPos, BlockValue _blockValue, BlockFaceFlag _sides)
	{
		return true;
	}
}

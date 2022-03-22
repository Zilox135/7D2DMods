using System;
using System.Globalization;

// Token: 0x0200012A RID: 298
public class BlockBuffs : Block
{
	// Token: 0x06000EE4 RID: 3812 RVA: 0x0006646C File Offset: 0x0006466C
	public BlockBuffs()
	{
		this.HasTileEntity = true;
	}

	// Token: 0x06000EE5 RID: 3813 RVA: 0x000664D0 File Offset: 0x000646D0
	public override void Init()
	{
		base.Init();
		if (this.Properties.Values.ContainsKey("TakeDelay"))
		{
			this.TakeDelay = StringParsers.ParseFloat(this.Properties.Values["TakeDelay"], 0, -1, NumberStyles.Any);
		}
		else
		{
			this.TakeDelay = 2f;
		}
		this.WorkstationData = new WorkstationData(base.GetBlockName(), this.Properties);
		CraftingManager.AddWorkstationData(this.WorkstationData);
	}

	// Token: 0x06000EE6 RID: 3814 RVA: 0x00066550 File Offset: 0x00064750
	public override void OnBlockAdded(WorldBase world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
	{
		base.OnBlockAdded(world, _chunk, _blockPos, _blockValue);
		if (_blockValue.ischild)
		{
			return;
		}
		_chunk.AddTileEntity(new TileEntityVendingMachine(_chunk)
		{
			localChunkPos = World.toBlock(_blockPos)
		});
	}

	// Token: 0x06000EE7 RID: 3815 RVA: 0x0006658C File Offset: 0x0006478C
	public override void OnBlockRemoved(WorldBase world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
	{
		base.OnBlockRemoved(world, _chunk, _blockPos, _blockValue);
		_chunk.RemoveTileEntityAt<TileEntityVendingMachine>((World)world, World.toBlock(_blockPos));
	}

	// Token: 0x06000EE8 RID: 3816 RVA: 0x000665AC File Offset: 0x000647AC
	public override void PlaceBlock(WorldBase _world, BlockPlacement.Result _result, EntityAlive _ea)
	{
		base.PlaceBlock(_world, _result, _ea);
		TileEntityVendingMachine tileEntityVendingMachine = (TileEntityVendingMachine)_world.GetTileEntity(_result.clrIdx, _result.blockPos);
	}

	// Token: 0x06000EE9 RID: 3817 RVA: 0x000665E4 File Offset: 0x000647E4
	public override bool OnBlockActivated(WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _player)
	{
		TileEntityVendingMachine tileEntityVendingMachine = (TileEntityVendingMachine)_world.GetTileEntity(_cIdx, _blockPos);
		if (tileEntityVendingMachine == null)
		{
			return false;
		}
		_player.AimingGun = false;
		Vector3i blockPos = tileEntityVendingMachine.ToWorldPos();
		if (tileEntityVendingMachine.IsUserAccessing())
		{
			_player.PlayOneShot("ui_denied", false);
			return false;
		}
		_world.GetGameManager().TELockServer(_cIdx, blockPos, tileEntityVendingMachine.entityId, _player.entityId, null);
		return true;
	}

	// Token: 0x06000EEA RID: 3818 RVA: 0x00066646 File Offset: 0x00064846
	public override void OnBlockValueChanged(WorldBase _world, Chunk _chunk, int _clrIdx, Vector3i _blockPos, BlockValue _oldBlockValue, BlockValue _newBlockValue)
	{
		base.OnBlockValueChanged(_world, _chunk, _clrIdx, _blockPos, _oldBlockValue, _newBlockValue);
	}

	// Token: 0x06000EEB RID: 3819 RVA: 0x00066663 File Offset: 0x00064863
	public override byte GetLightValue(BlockValue _blockValue)
	{
		return 0;
	}

	// Token: 0x06000EEF RID: 3823 RVA: 0x000666F6 File Offset: 0x000648F6
	public override bool OnBlockActivated(int _indexInBlockActivationCommands, WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _player)
	{
		if (_indexInBlockActivationCommands == 0)
		{
			return this.OnBlockActivated(_world, _cIdx, _blockPos, _blockValue, _player);
		}
		if (_indexInBlockActivationCommands != 1)
		{
			return false;
		}
		this.TakeItemWithTimer(_cIdx, _blockPos, _blockValue, _player);
		return true;
	}

	// Token: 0x06000EF0 RID: 3824 RVA: 0x00066720 File Offset: 0x00064920
	public override BlockActivationCommand[] GetBlockActivationCommands(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		bool flag = _world.IsMyLandProtectedBlock(_blockPos, _world.GetGameManager().GetPersistentLocalPlayer(), false);
		TileEntityVendingMachine tileEntityVendingMachine = (TileEntityVendingMachine)_world.GetTileEntity(_clrIdx, _blockPos);
		this.cmds[1].enabled = (flag && this.TakeDelay > 0f);
		return this.cmds;
	}

	// Token: 0x06000EF1 RID: 3825 RVA: 0x0006678C File Offset: 0x0006498C
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

	// Token: 0x06000EF2 RID: 3826 RVA: 0x0006684C File Offset: 0x00064A4C
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

	// Token: 0x04000A23 RID: 2595
	private float TakeDelay = 2f;

	// Token: 0x04000A24 RID: 2596
	public WorkstationData WorkstationData;

	// Token: 0x04000A25 RID: 2597
	private BlockActivationCommand[] cmds = new BlockActivationCommand[]
	{
		new BlockActivationCommand("open", "campfire", true, false),
		new BlockActivationCommand("take", "hand", false, false)
	};
}

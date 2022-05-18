using System;
using System.Collections.Generic;
using System.Globalization;

public class BlockPickup : Block
{

	private float TakeDelay = 2f;

	protected static string PropPickupSound = "PickupSound";

	private string PickupSound = "StinkScreamer";

	private BlockActivationCommand[] cmds = new BlockActivationCommand[]
	{
		new BlockActivationCommand("Search", "search", false, false),
		new BlockActivationCommand("take", "hand", false, false)
	};

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

		if (this.Properties.Values.ContainsKey(BlockPickup.PropPickupSound))
		{
			this.PickupSound = this.Properties.Values[BlockPickup.PropPickupSound];
		}
	}

	public override bool OnBlockActivated(int _indexInBlockActivationCommands, WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _player)
	{
		if (_indexInBlockActivationCommands == 0)
        {
			return false;
        }
        if (_indexInBlockActivationCommands != 1)
		{
			return false;
		}
		this.TakeItemWithTimer(_cIdx, _blockPos, _blockValue, _player);
		return true;
	}

    public override BlockActivationCommand[] GetBlockActivationCommands(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		this.cmds[0].enabled = true;
		this.cmds[1].enabled = (this.TakeDelay > 0f);
		return this.cmds;
	}

	public void TakeItemWithTimer(int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _player)
	{
 		if (_blockValue.damage > 5)
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
		_player.PlayOneShot(this.PickupSound, false);
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
		if (block.damage > 5)
		{
			GameManager.ShowTooltip(entityPlayerLocal, Localization.Get("ttRepairBeforePickup"), string.Empty, "ui_denied", null);
			return;
		}
		if (block.type != blockValue.type)
		{
			GameManager.ShowTooltip(entityPlayerLocal, Localization.Get("ttBlockMissingPickup"), string.Empty, "ui_denied", null);
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
}

using UnityEngine;
using System;

public class BlockTrashCan : BlockLoot
{
	public void EmptyBin(WorldBase _world, Vector3i _blockPos, EntityAlive _player)
	{
		TileEntityLootContainer container = GameManager.Instance.World.GetTileEntity(0, _blockPos) as TileEntityLootContainer;
		if(container != null)
		{
			ItemStack[] items = container.items;
			foreach(ItemStack itemStack in items)
			{
				container.RemoveItem(itemStack.itemValue);
			}
			container.SetModified();
		}
		DisplayToolTipText("Trash Can Emptied", _player);
	}
	
	public override bool OnBlockActivated(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _player)
	{
		TileEntityLootContainer container = GameManager.Instance.World.GetTileEntity(0, _blockPos) as TileEntityLootContainer;
		if(!container.IsEmpty() && Input.GetKey(KeyCode.LeftShift))
		{
			EmptyBin(_world, _blockPos, _player);
			Audio.Manager.BroadcastPlayByLocalPlayer(_blockPos.ToVector3() + Vector3.one * 0.5f, "UseActions/close_trashcan");
			return true;
		}
		base.OnBlockActivated(_world, _clrIdx, _blockPos, _blockValue, _player);
		return true;
	}
	
	public override string GetActivationText(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		TileEntityLootContainer container = GameManager.Instance.World.GetTileEntity(0, _blockPos) as TileEntityLootContainer;
		PlayerActionsLocal playerInput = ((EntityPlayerLocal)_entityFocusing).playerInput;
		string arg2 = playerInput.Activate.GetBindingXuiMarkupString() + playerInput.PermanentActions.Activate.GetBindingXuiMarkupString();
		if(!container.IsEmpty() && Input.GetKey(KeyCode.LeftShift))
		{
			return arg2 +" Empty Trash Can";
		}
		return arg2 +" Open Trash Can";
	}
	
	private DateTime dteNextToolTipDisplayTime;
	
	public void DisplayToolTipText(string str, EntityAlive _entity)
    {
		EntityPlayerLocal entity = _entity as EntityPlayerLocal;
		if (DateTime.Now > dteNextToolTipDisplayTime)
        {
            GameManager.ShowTooltip(entity, str);
			dteNextToolTipDisplayTime = DateTime.Now.AddSeconds(3);
        }
    }
}
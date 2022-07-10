using System;
using UnityEngine;

public class XUiC_PlayerStatsWindow : XUiController
{

	private readonly CachedStringFormatter<int> playerDeathsFormatter = new CachedStringFormatter<int>((int _i) => _i.ToString());

	private readonly CachedStringFormatter<int> playerHealthFormatter = new CachedStringFormatter<int>((int _i) => _i.ToString());

	private readonly CachedStringFormatter<int> playerStaminaFormatter = new CachedStringFormatter<int>((int _i) => _i.ToString());

	private readonly CachedStringFormatter<int> playerMaxHealthFormatter = new CachedStringFormatter<int>((int _i) => _i.ToString());

	private readonly CachedStringFormatter<int> playerMaxStaminaFormatter = new CachedStringFormatter<int>((int _i) => _i.ToString());

	private readonly CachedStringFormatter<float> playerFoodFormatter = new CachedStringFormatter<float>((float _i) => _i.ToCultureInvariantString("0"));

	private readonly CachedStringFormatter<float> playerWaterFormatter = new CachedStringFormatter<float>((float _i) => _i.ToCultureInvariantString("0"));

	private readonly CachedStringFormatter<float> playerLevelFillFormatter = new CachedStringFormatter<float>((float _i) => _i.ToCultureInvariantString());

	private readonly CachedStringFormatter<float> playerFoodFillFormatter = new CachedStringFormatter<float>((float _i) => _i.ToCultureInvariantString());

	private readonly CachedStringFormatter<int> playerFoodMaxFormatter = new CachedStringFormatter<int>((int _i) => _i.ToString());

	private readonly CachedStringFormatter<int> playerWaterMaxFormatter = new CachedStringFormatter<int>((int _i) => _i.ToString());

	private readonly CachedStringFormatter<int> playerItemsCraftedFormatter = new CachedStringFormatter<int>((int _i) => _i.ToString());

	private readonly CachedStringFormatter<int> playerPvpKillsFormatter = new CachedStringFormatter<int>((int _i) => _i.ToString());

	private readonly CachedStringFormatter<int> playerZombieKillsFormatter = new CachedStringFormatter<int>((int _i) => _i.ToString());

	private readonly CachedStringFormatter<int> playerXpToNextLevelFormatter = new CachedStringFormatter<int>((int _i) => _i.ToString());

	private readonly CachedStringFormatter<int> playerArmorRatingFormatter = new CachedStringFormatter<int>((int _i) => _i.ToString());

	private readonly CachedStringFormatter<int> playerLevelFormatter = new CachedStringFormatter<int>((int _i) => _i.ToString());

	private float updateTime;

	private EntityPlayer player;

	internal sealed class PrivateImplementationDetails
	{
		internal static uint ComputeStringHash(string s)
		{
			uint num = new uint();
			if (s != null)
			{
				num = 0x811c9dc5;
				for (int i = 0; i < s.Length; i++)
				{
					num = (s[i] ^ num) * 0x1000193;
				}
			}
			return num;
		}
	}

	public override void Init()
	{
		base.Init();
	}

	public override bool GetBindingValue(ref string value, string bindingName)
	{
		if (bindingName != null)
		{
			uint num = PrivateImplementationDetails.ComputeStringHash(bindingName);
			if (num <= 2395478116U)
			{
				if (num <= 782575427U)
				{
					if (num <= 145239280U)
					{
						if (num <= 75296162U)
						{
							if (num != 8937094U)
							{
								if (num == 75296162U)
								{
									if (bindingName == "playerfoodtitle")
									{
										value = Localization.Get("xuiFood");
										return true;
									}
								}
							}
							else if (bindingName == "playerhealth")
							{
								value = ((this.player != null) ? this.playerHealthFormatter.Format((int)XUiM_Player.GetHealth(this.player)) : "");
								return true;
							}
						}
						else if (num != 125916223U)
						{
							if (num == 145239280U)
							{
								if (bindingName == "playerarmorratingtitle")
								{
									value = Localization.Get("statPhysicalDamageResist");
									return true;
								}
							}
						}
						else if (bindingName == "playercurrentlife")
						{
							value = ((this.player != null) ? XUiM_Player.GetCurrentLife(this.player) : "");
							return true;
						}
					}
					else if (num <= 304163417U)
					{
						if (num != 234495987U)
						{
							if (num == 304163417U)
							{
								if (bindingName == "playercoretemp")
								{
									value = ((this.player != null) ? XUiM_Player.GetCoreTemp(this.player) : "");
									return true;
								}
							}
						}
						else if (bindingName == "playerdeathstitle")
						{
							value = Localization.Get("xuiDeaths");
							return true;
						}
					}
					else if (num != 672688503U)
					{
						if (num != 696376978U)
						{
							if (num == 782575427U)
							{
								if (bindingName == "playerdeaths")
								{
									value = ((this.player != null) ? this.playerDeathsFormatter.Format(XUiM_Player.GetDeaths(this.player)) : "");
									return true;
								}
							}
						}
						else if (bindingName == "playerlootstagetitle")
						{
							value = Localization.Get("xuiLootstage");
							return true;
						}
					}
					else if (bindingName == "playercurrentlifetitle")
					{
						value = Localization.Get("xuiCurrentLife");
						return true;
					}
				}
				else if (num <= 1478649187U)
				{
					if (num <= 965025103U)
					{
						if (num != 885900949U)
						{
							if (num == 965025103U)
							{
								if (bindingName == "playerwater")
								{
									value = ((this.player != null) ? this.playerWaterFormatter.Format(XUiM_Player.GetWater(this.player)) : "");
									return true;
								}
							}
						}
						else if (bindingName == "playertravelledtitle")
						{
							value = Localization.Get("xuiKMTravelled");
							return true;
						}
					}
					else if (num != 1009276468U)
					{
						if (num != 1477941828U)
						{
							if (num == 1478649187U)
							{
								return false;
							}
						}
						else if (bindingName == "playerlongestlife")
						{
							value = ((this.player != null) ? XUiM_Player.GetLongestLife(this.player) : "");
							return true;
						}
					}
					else if (bindingName == "playerxptonextleveltitle")
					{
						value = Localization.Get("xuiXPToNextLevel");
						return true;
					}
				}
				else if (num <= 2023588471U)
				{
					if (num != 1811778199U)
					{
						if (num == 2023588471U)
						{
							if (bindingName == "playerzombiekillstitle")
							{
								value = Localization.Get("xuiZombieKills");
								return true;
							}
						}
					}
					else if (bindingName == "playeritemscraftedtitle")
					{
						value = Localization.Get("xuiItemsCrafted");
						return true;
					}
				}
				else if (num != 2186126559U)
				{
					if (num != 2219475343U)
					{
						if (num == 2395478116U)
						{
							if (bindingName == "playerlootstage")
							{
								value = ((this.player != null) ? this.player.GetHighestPartyLootStage(0f, 0f).ToString() : "");
								return true;
							}
						}
					}
					else if (bindingName == "playermaxstamina")
					{
						value = ((this.player != null) ? this.playerMaxStaminaFormatter.Format((int)XUiM_Player.GetMaxStamina(this.player)) : "");
						return true;
					}
				}
				else if (bindingName == "playeritemscrafted")
				{
					value = ((this.player != null) ? this.playerItemsCraftedFormatter.Format(XUiM_Player.GetItemsCrafted(this.player)) : "");
					return true;
				}
			}
			else if (num <= 3537464933U)
			{
				if (num <= 3249756066U)
				{
					if (num <= 2587631291U)
					{
						if (num != 2532548756U)
						{
							if (num == 2587631291U)
							{
								return false;
							}
						}
						else if (bindingName == "playerfood")
						{
							value = ((this.player != null) ? this.playerFoodFormatter.Format(XUiM_Player.GetFood(this.player)) : "");
							return true;
						}
					}
					else if (num != 2974192615U)
					{
						if (num != 3042900123U)
						{
							if (num == 3249756066U)
							{
								if (bindingName == "playerfoodmax")
								{
									value = ((this.player != null) ? this.playerFoodMaxFormatter.Format(XUiM_Player.GetFoodMax(this.player)) : "");
									return true;
								}
							}
						}
						else if (bindingName == "playerpvpkills")
						{
							value = ((this.player != null) ? this.playerPvpKillsFormatter.Format(XUiM_Player.GetPlayerKills(this.player)) : "");
							return true;
						}
					}
					else if (bindingName == "playerwatertitle")
					{
						value = Localization.Get("xuiWater");
						return true;
					}
				}
				else if (num <= 3275992332U)
				{
					if (num != 3257770903U)
					{
						if (num == 3275992332U)
						{
							if (bindingName == "playermaxhealth")
							{
								value = ((this.player != null) ? this.playerMaxHealthFormatter.Format((int)XUiM_Player.GetMaxHealth(this.player)) : "");
								return true;
							}
						}
					}
				}
				else if (num != 3371877161U)
				{
					if (num != 3484390642U)
					{
						if (num == 3537464933U)
						{
							if (bindingName == "playerstamina")
							{
								value = ((this.player != null) ? this.playerStaminaFormatter.Format((int)XUiM_Player.GetStamina(this.player)) : "");
								return true;
							}
						}
					}
					else if (bindingName == "playerlongestlifetitle")
					{
						value = Localization.Get("xuiLongestLife");
						return true;
					}
				}
				else if (bindingName == "playerstaminatitle")
				{
					value = Localization.Get("lblStamina");
					return true;
				}
			}
			else if (num <= 3931175545U)
			{
				if (num <= 3712331684U)
				{
					if (num != 3705263762U)
					{
						if (num == 3712331684U)
						{
							if (bindingName == "playermodifiedcurrentfood")
							{
								value = ((this.player != null) ? this.playerFoodFormatter.Format(XUiM_Player.GetModifiedCurrentFood(this.player)) : "");
								return true;
							}
						}
					}
					else if (bindingName == "playerxptonextlevel")
					{
						value = ((this.player != null) ? this.playerXpToNextLevelFormatter.Format(XUiM_Player.GetXPToNextLevel(this.player) + this.player.Progression.ExpDeficit) : "");
						return true;
					}
				}
				else if (num != 3887827771U)
				{
					if (num != 3900606022U)
					{
						if (num == 3931175545U)
						{
							if (bindingName == "playertravelled")
							{
								value = ((this.player != null) ? XUiM_Player.GetKMTraveled(this.player) : "");
								return true;
							}
						}
					}
					else if (bindingName == "playerarmorrating")
					{
						value = ((this.player != null) ? this.playerArmorRatingFormatter.Format((int)EffectManager.GetValue(PassiveEffects.PhysicalDamageResist, null, 0f, this.player, null, default(FastTags), true, true, true, true, 1, true)) : "");
						return true;
					}
				}
				else if (bindingName == "playerpvpkillstitle")
				{
					value = Localization.Get("xuiPlayerKills");
					return true;
				}
			}
			else if (num <= 4031300656U)
			{
				if (num != 4025935093U)
				{
					if (num == 4031300656U)
					{
						if (bindingName == "playerhealthtitle")
						{
							value = Localization.Get("lblHealth");
							return true;
						}
					}
				}
				else if (bindingName == "playercoretemptitle")
				{
					value = Localization.Get("xuiFeelsLike");
					return true;
				}
			}
			else if (num != 4077864767U)
			{
				if (num != 4107995367U)
				{
					if (num == 4159374943U)
					{
						if (bindingName == "playermodifiedcurrentwater")
						{
							value = ((this.player != null) ? this.playerWaterFormatter.Format(XUiM_Player.GetModifiedCurrentWater(this.player)) : "");
							return true;
						}
					}
				}
				else if (bindingName == "playerwatermax")
				{
					value = ((this.player != null) ? this.playerWaterMaxFormatter.Format(XUiM_Player.GetWaterMax(this.player)) : "");
					return true;
				}
			}
			else if (bindingName == "playerzombiekills")
			{
				value = ((this.player != null) ? this.playerZombieKillsFormatter.Format(XUiM_Player.GetZombieKills(this.player)) : "");
				return true;
			}
			if (num != 83123014U)
			{
				if (num == 126665037U)
				{
					if (bindingName == "playerfoodfill")
					{
						value = ((this.player != null) ? this.playerFoodFillFormatter.Format(XUiM_Player.GetFoodPercent(this.player)) : "");
						return true;
					}
				}
			}
			else if (bindingName == "playerlevel")
			{
				value = ((this.player != null) ? this.playerLevelFormatter.Format(XUiM_Player.GetLevel(this.player)) : "");
				return true;
			}
		}
		return false;
	}

	public override void Update(float _dt)
	{
		if (base.ViewComponent.IsVisible && Time.time > this.updateTime)
		{
			this.updateTime = Time.time + 0.25f;
			base.RefreshBindings(this.IsDirty);
			if (this.IsDirty)
			{
				this.IsDirty = false;
			}
		}
		base.Update(_dt);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.IsDirty = true;
		this.player = base.xui.playerUI.entityPlayer;
	}
}

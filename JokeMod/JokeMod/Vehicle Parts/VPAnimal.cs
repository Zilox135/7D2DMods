﻿using System;
using System.Collections.Generic;
using System.Globalization;
using Audio;
using UnityEngine;

public class VPAnimal : VehiclePart
{
	private const float cIdleFuelPercent = 0.1f;

	private const float cTurboFuelPercent = 2f;

	private float fuelKmPerL;

	private float foodDrain;

	private float foodDrainTurbo;

	public bool isRunning;

	private bool isBadlyDamaged;

	private int acceleratePhase;

	private float rpm;

	private int gearIndex;

	private bool isDecelSoundPlayed;

	private string accelDecelSoundName;

	private List<VPAnimal.Gear> gears = new List<VPAnimal.Gear>();

	private VPAnimal.SoundRange[] gearSoundRanges;

	private class Gear
	{
		public float rpmMin;

		public float rpmMax;

		public float rpmDecel;

		public float rpmAccel;

		public float rpmDownShiftPoint;

		public float rpmUpShiftPoint;

		public float rpmDownShiftTo;

		public float rpmUpShiftTo;

		public string accelSoundName;

		public string decelSoundName;

		public VPAnimal.SoundRange[] soundRanges;
	}

	private class SoundRange
	{
		public float pitchMin;

		public float pitchMax;

		public float volumeMin;

		public float volumeMax;

		public float pitchFadeMin;

		public float pitchFadeMax;

		public float pitchFadeRange;

		public string name;

		public Handle soundHandle;
	}

	public override void SetProperties(DynamicProperties _properties)
	{
		base.SetProperties(_properties);
		StringParsers.TryParseFloat(base.GetProperty("fuelKmPerL"), out this.fuelKmPerL, 0, -1, NumberStyles.Any);
		_properties.ParseVec("foodDrain", ref this.foodDrain, ref this.foodDrainTurbo);
		this.gears.Clear();
		for (int i = 1; i < 9; i++)
		{
			string property = base.GetProperty("gear" + i.ToString());
			if (property.Length == 0)
			{
				break;
			}
			string[] array = property.Split(new char[]
			{
				','
			});
			VPAnimal.Gear gear = new VPAnimal.Gear();
			this.gears.Add(gear);
			int num = 0;
			StringParsers.TryParseFloat(array[num++], out gear.rpmMin, 0, -1, NumberStyles.Any);
			StringParsers.TryParseFloat(array[num++], out gear.rpmMax, 0, -1, NumberStyles.Any);
			StringParsers.TryParseFloat(array[num++], out gear.rpmDecel, 0, -1, NumberStyles.Any);
			StringParsers.TryParseFloat(array[num++], out gear.rpmDownShiftPoint, 0, -1, NumberStyles.Any);
			StringParsers.TryParseFloat(array[num++], out gear.rpmDownShiftTo, 0, -1, NumberStyles.Any);
			StringParsers.TryParseFloat(array[num++], out gear.rpmAccel, 0, -1, NumberStyles.Any);
			StringParsers.TryParseFloat(array[num++], out gear.rpmUpShiftPoint, 0, -1, NumberStyles.Any);
			StringParsers.TryParseFloat(array[num++], out gear.rpmUpShiftTo, 0, -1, NumberStyles.Any);
			gear.accelSoundName = array[num++].Trim();
			gear.decelSoundName = array[num++].Trim();
			int num2 = (array.Length - num) / 8;
			if (num2 > 0)
			{
				gear.soundRanges = new VPAnimal.SoundRange[num2];
				for (int j = 0; j < num2; j++)
				{
					VPAnimal.SoundRange soundRange = new VPAnimal.SoundRange();
					gear.soundRanges[j] = soundRange;
					int num3 = num + j * 8;
					StringParsers.TryParseFloat(array[num3], out soundRange.pitchMin, 0, -1, NumberStyles.Any);
					StringParsers.TryParseFloat(array[num3 + 1], out soundRange.pitchMax, 0, -1, NumberStyles.Any);
					StringParsers.TryParseFloat(array[num3 + 2], out soundRange.volumeMin, 0, -1, NumberStyles.Any);
					StringParsers.TryParseFloat(array[num3 + 3], out soundRange.volumeMax, 0, -1, NumberStyles.Any);
					StringParsers.TryParseFloat(array[num3 + 4], out soundRange.pitchFadeMin, 0, -1, NumberStyles.Any);
					StringParsers.TryParseFloat(array[num3 + 5], out soundRange.pitchFadeMax, 0, -1, NumberStyles.Any);
					StringParsers.TryParseFloat(array[num3 + 6], out soundRange.pitchFadeRange, 0, -1, NumberStyles.Any);
					soundRange.pitchFadeRange += 1E-05f;
					soundRange.name = array[num3 + 7].Trim();
				}
			}
		}
	}

	public override void Update(float _dt)
	{
		if (this.IsBroken())
		{
			if (!this.isBadlyDamaged)
			{
				this.CheckParticleEffect();
			}
			this.stopEngine(false);
			return;
		}
		this.CheckParticleEffect();
		EntityAlive entityAlive = this.vehicle.entity.AttachedMainEntity as EntityAlive;
		if (entityAlive)
		{
			entityAlive.CurrentMovementTag = EntityAlive.MovementTagDriving;
			float value = 0f;
			if (this.vehicle.CurrentIsAccel)
			{
				value = this.foodDrain;
				if (this.vehicle.IsTurbo)
				{
					value = this.foodDrainTurbo;
				}
			}
			entityAlive.SetCVar("_vehicleFood", value);
		}
		if (!this.isRunning)
		{
			return;
		}
		float magnitude = this.vehicle.CurrentVelocity.magnitude;
		float num = _dt / (this.fuelKmPerL * 1000f);
		if (this.vehicle.IsTurbo)
		{
			num *= 2f;
		}
		if (this.vehicle.CurrentIsAccel)
		{
			num *= magnitude;
		}
		else
		{
			num *= this.vehicle.VelocityMax * 0.1f;
		}
		num *= this.vehicle.EffectFuelUsePer;
		this.vehicle.EmitEvent(VPFuelTank.Action.RemoveFuel, num);
	}

	private float CheckParticleEffect()
	{
		float healthPercentage = base.GetHealthPercentage();
		Transform particleTransform = base.GetParticleTransform();
		if (!particleTransform)
		{
			return healthPercentage;
		}
		bool activeSelf = particleTransform.gameObject.activeSelf;
		if (!activeSelf && healthPercentage <= 0.25f)
		{
			particleTransform.gameObject.SetActive(true);
			this.isBadlyDamaged = true;
		}
		else if (activeSelf && healthPercentage > 0.25f)
		{
			particleTransform.gameObject.SetActive(false);
			this.isBadlyDamaged = false;
		}
		return healthPercentage;
	}

	private void updateEngineSimulation()
	{
		if (!this.isRunning)
		{
			return;
		}
		float num = 500f;
		float num2 = 5000f;
		float num3 = -2400f;
		float num4 = 2700f;
		float num5 = 2700f;
		float num6 = 5000f;
		float num7 = 1500f;
		float num8 = 2800f;
		if (this.gears.Count > 0)
		{
			VPAnimal.Gear gear = this.gears[this.gearIndex];
			num = gear.rpmMin;
			num2 = gear.rpmMax;
			num3 = gear.rpmDecel;
			num4 = gear.rpmDownShiftPoint;
			num5 = gear.rpmDownShiftTo;
			num8 = gear.rpmAccel;
			num6 = gear.rpmUpShiftPoint;
			num7 = gear.rpmUpShiftTo;
		}
		if (this.vehicle.CurrentIsAccel)
		{
			this.rpm += num8 * Time.deltaTime;
			this.rpm = Mathf.Min(this.rpm, num2);
			if (this.rpm >= num6 && this.gearIndex < this.gears.Count - 1 && this.vehicle.CurrentForwardVelocity > 4f)
			{
				this.gearIndex++;
				this.rpm = num7;
				this.vehicle.entity.AddRelativeForce(new Vector3(0f, 0.2f, -2f), (ForceMode)2);
				VPAnimal.Gear gear2 = this.gears[this.gearIndex];
				this.playAccelDecelSound(gear2.accelSoundName);
			}
			if (this.acceleratePhase <= 0)
			{
				if (this.gears.Count > 0)
				{
					VPAnimal.Gear gear3 = this.gears[this.gearIndex];
					this.isDecelSoundPlayed = false;
					this.playAccelDecelSound(gear3.accelSoundName);
				}
				this.acceleratePhase = 1;
			}
			float rpmPercent = (this.rpm - num) / (num2 - num);
			this.updateEngineSounds(rpmPercent);
			return;
		}
		if (this.acceleratePhase >= 0)
		{
			float num9 = num3;
			if (Mathf.Abs(this.vehicle.CurrentForwardVelocity) < 2f)
			{
				num9 *= 2f;
			}
			this.rpm += num9 * Time.deltaTime;
			if (this.rpm <= num4)
			{
				if (this.gears.Count > 0 && !this.isDecelSoundPlayed)
				{
					this.isDecelSoundPlayed = true;
					VPAnimal.Gear gear4 = this.gears[this.gearIndex];
					this.playAccelDecelSound(gear4.decelSoundName);
				}
				if (this.gearIndex <= 0)
				{
					this.acceleratePhase = -1;
					this.updateEngineSounds(0f);
					return;
				}
				this.acceleratePhase = 0;
				this.gearIndex = 0;
				if (num5 > 0f)
				{
					this.rpm = num5;
					return;
				}
			}
			else
			{
				float rpmPercent2 = (this.rpm - num) / (num2 - num);
				this.updateEngineSounds(rpmPercent2);
			}
		}
	}

	public override void HandleEvent(Vehicle.Event _event, Vehicle _vehicle, float _arg)
	{
		switch (_event)
		{
		case Vehicle.Event.Start:
			if (!this.IsBroken())
			{
				this.startEngine();
				return;
			}
			break;
		case Vehicle.Event.Started:
		case Vehicle.Event.Stopped:
			break;
		case Vehicle.Event.Stop:
		{
			EntityAlive entityAlive = this.vehicle.entity.AttachedMainEntity as EntityAlive;
			if (entityAlive)
			{
				entityAlive.SetCVar("_vehicleFood", 0f);
			}
			this.stopEngine(false);
			return;
		}
		case Vehicle.Event.SimulationUpdate:
			if (!this.IsBroken())
			{
				this.updateEngineSimulation();
			}
			break;
		default:
			return;
		}
	}

	public override void HandleEvent(VPFuelTank.Event _event, VehiclePart _part, float _arg)
	{
		if (this.IsBroken() || _part == null || !(_part is VPFuelTank))
		{
			return;
		}
		if (_event == VPFuelTank.Event.FuelDepleted)
		{
			this.stopEngine(true);
		}
	}

	private void startEngine()
	{
		if (this.isRunning)
		{
			return;
		}
		this.isRunning = true;
		if (this.vehicle.GetFuelLevel() > 0f)
		{
			this.playSound(this.properties.Values["sound_start"]);
			this.gearIndex = 0;
			this.updateEngineSounds(0f);
		}
		this.vehicle.entity.IsEngineRunning = true;
		base.fireGlobalEvent(Vehicle.Event.Started);
	}

	private void stopEngine(bool _outOfFuel = false)
	{
		if (!this.isRunning)
		{
			return;
		}
		this.isRunning = false;
		this.stopEngineSounds();
		if (!_outOfFuel)
		{
			this.playSound(this.properties.Values["sound_shut_off"]);
		}
		else
		{
			this.playSound(this.properties.Values["sound_no_fuel_shut_off"]);
		}
		this.vehicle.entity.IsEngineRunning = false;
		base.fireGlobalEvent(Vehicle.Event.Stopped);
	}

	private void playSound(string _sound)
	{
		if (this.vehicle.entity != null && !this.vehicle.entity.isEntityRemote)
		{
			this.vehicle.entity.PlayOneShot(_sound, false);
		}
	}

	private void stopSound(string _sound)
	{
		if (this.vehicle.entity != null && !this.vehicle.entity.isEntityRemote)
		{
			this.vehicle.entity.StopOneShot(_sound);
		}
	}

	private void changeSoundLoop(string soundName, ref Handle handle)
	{
		this.stopSoundLoop(ref handle);
		this.playSoundLoop(soundName, ref handle);
	}

	private void playSoundLoop(string soundName, ref Handle handle)
	{
		if (handle != null)
		{
			return;
		}
		if (this.vehicle.entity != null && !this.vehicle.entity.isEntityRemote)
		{
			handle = Manager.Play(this.vehicle.entity, soundName, 1f, true);
		}
	}

	private void stopSoundLoop(ref Handle handle)
	{
		if (handle != null)
		{
			handle.Stop(this.vehicle.entity.entityId);
			handle = null;
		}
	}

	private void playAccelDecelSound(string name)
	{
		if (this.accelDecelSoundName != null)
		{
			this.stopSound(this.accelDecelSoundName);
		}
		if (name != null && name.Length > 0)
		{
			this.playSound(name);
		}
		this.accelDecelSoundName = name;
	}

	private void updateEngineSounds(float rpmPercent)
	{
		if (this.gears.Count > 0)
		{
			for (int i = 0; i < this.gears.Count; i++)
			{
				VPAnimal.Gear gear = this.gears[i];
				if (i == this.gearIndex)
				{
					for (int j = 0; j < gear.soundRanges.Length; j++)
					{
						VPAnimal.SoundRange soundRange = gear.soundRanges[j];
						float num = Mathf.Lerp(soundRange.pitchMin, soundRange.pitchMax, rpmPercent);
						float num2 = Mathf.Lerp(soundRange.volumeMin, soundRange.volumeMax, rpmPercent);
						float num3 = 1f;
						float num4 = soundRange.pitchFadeMin - num;
						if (num4 > 0f)
						{
							num3 = Mathf.Lerp(1f, 0f, num4 / soundRange.pitchFadeRange);
						}
						else
						{
							float num5 = num - soundRange.pitchFadeMax;
							if (num5 > 0f)
							{
								num3 = Mathf.Lerp(1f, 0f, num5 / soundRange.pitchFadeRange);
							}
						}
						float num6 = num2 * num3;
						if (num6 < 0.01f)
						{
							if (soundRange.soundHandle != null)
							{
								this.stopSoundLoop(ref soundRange.soundHandle);
							}
						}
						else
						{
							if (soundRange.soundHandle == null)
							{
								this.playSoundLoop(soundRange.name, ref soundRange.soundHandle);
							}
							if (soundRange.soundHandle != null)
							{
								float num7 = num;
								if (this.vehicle.IsTurbo)
								{
									num7 *= 1.3f;
								}
								soundRange.soundHandle.SetPitch(num7);
								soundRange.soundHandle.SetVolume(num6);
							}
						}
					}
				}
				else
				{
					for (int k = 0; k < gear.soundRanges.Length; k++)
					{
						VPAnimal.SoundRange soundRange2 = gear.soundRanges[k];
						if (soundRange2.soundHandle != null)
						{
							this.stopSoundLoop(ref soundRange2.soundHandle);
						}
					}
				}
			}
		}
	}

	private void stopEngineSounds()
	{
		if (this.gears.Count > 0)
		{
			for (int i = 0; i < this.gears.Count; i++)
			{
				VPAnimal.Gear gear = this.gears[i];
				for (int j = 0; j < gear.soundRanges.Length; j++)
				{
					VPAnimal.SoundRange soundRange = gear.soundRanges[j];
					if (soundRange.soundHandle != null)
					{
						this.stopSoundLoop(ref soundRange.soundHandle);
					}
				}
			}
		}
		this.playAccelDecelSound(null);
	}
}

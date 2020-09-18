﻿/*
 * Originally written by 
 * 
 * 
*/
using System;
using System.Linq;
using Harmony;
using MelonLoader;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.XR;
using Valve.VR;

namespace RecenterVR
{
	public class Center : MelonMod
	{
		public override void OnApplicationStart()
		{
			SceneManager.sceneLoaded += new UnityAction<Scene, LoadSceneMode>(OnSceneLoaded);
            MelonPrefs.RegisterCategory("RecenterVR", "RecenterVR Hotkeys");
			MelonPrefs.RegisterString("RecenterVR", "recenter_pc", "t", "Recenter (Keyboard)");
			MelonPrefs.RegisterString("RecenterVR", "recenter_xbox", "joystick button 0,joystick button 3", "Recenter (Controller)");
			MelonPrefs.RegisterString("RecenterVR", "hide_hud", "u", "Hide HUD (Keyboard)");
			MelonPrefs.RegisterString("RecenterVR", "reset_audio", "i", "Reset audio (Keyboard)");
			MelonPrefs.RegisterString("RecenterVR", "list_joysticks", "f1", "List connected joysticks (Keyboard)");
			GetJoySticks();
			MelonLogger.Log("OnApplicationStart > Check \"UserData/modprefs.ini\" for settings");
		}

		private bool CheckKeysPressed(string option) {
			var binds = MelonPrefs.GetString("RecenterVR", option);
			var trues = new bool[] { };
            foreach (var bind in binds.Split(','))
            {
				trues.Add(Input.GetKeyDown(bind));
            }
			return trues.All(t => t == true);
		}

		private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
		{
			if (scene.name == "StartScreen")
			{
				RecenterCamera();
			}
		}
		private static void RecenterCamera()
		{
			MelonLogger.Log("Recentering VR camera...");
			if (XRSettings.loadedDeviceName == "Oculus")
			{
				InputTracking.Recenter(); // XRInputSubsystem.TryRecenter();
			}
			else if (XRSettings.loadedDeviceName == "OpenVR")
			{
				OpenVR.System.ResetSeatedZeroPose();
				OpenVR.Compositor.SetTrackingSpace(ETrackingUniverseOrigin.TrackingUniverseSeated);
			}
		}

		public override void OnUpdate()
		{
			if (XRSettings.enabled) {
				if (CheckKeysPressed("recenter_pc") || CheckKeysPressed("recenter_xbox"))
				{
					RecenterCamera();
				}
			}
			if (CheckKeysPressed("hide_hud"))
			{
				MelonLogger.Log("Hiding HUD...");
				GameModeUtils.SetGameMode(GameModeOption.Creative, GameModeOption.None);
				HideForScreenshots.Hide(HideForScreenshots.HideType.HUD);
				HideForScreenshots.Hide(HideForScreenshots.HideType.Mask);
				return;
			}
			if (CheckKeysPressed("reset_audio"))
			{
				MelonLogger.Log("Resetting audio...");
				AudioConfiguration old = AudioSettings.GetConfiguration();
				MelonLogger.Log(string.Format("Old: {0}", old.speakerMode));
				AudioSettings.Reset(AudioSettings.GetConfiguration());
				AudioConfiguration _new = AudioSettings.GetConfiguration();
				MelonLogger.Log(string.Format("New: {0}", _new.speakerMode));
				return;
			}
			if (CheckKeysPressed("list_joysticks"))
			{
				MelonLogger.Log("Getting joysticks...");
				this.GetJoySticks();
			}
		}

		private string[] GetJoySticks()
		{
			string[] joysticks = Input.GetJoystickNames();
			MelonLogger.Log("Connected Joysticks:");
			foreach (string joystick in joysticks)
			{
				MelonLogger.Log("\t- " + joystick);
			}
			return joysticks;
		}
	}
}
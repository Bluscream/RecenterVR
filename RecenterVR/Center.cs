/*
 * Originally written by @Raicuparta#0015
 * Modified by Bluscream
*/
using System.Collections.Generic;
using System.Linq;
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
		private const string mod = "RecenterVR";
		private const string category = "RecenterVR Hotkeys";
		private InputDevice leftController;
		private InputDevice rightController;
		public override void OnApplicationStart()
		{
			SceneManager.sceneLoaded += new UnityAction<Scene, LoadSceneMode>(OnSceneLoaded);
			MelonPrefs.RegisterCategory(mod, mod);
			MelonPrefs.RegisterBool(mod, "seated", true, "Recenter as Seated VR");
			MelonPrefs.RegisterBool(mod, "auto_recenter", true, "Automatically Recenter");
			MelonPrefs.RegisterBool(mod, "auto_graphics", true, "Automatically Adjust graphics");
			MelonPrefs.RegisterBool(mod, "auto_controller", true, "Automatically enable controller");
			MelonPrefs.RegisterCategory(category, category);
			MelonPrefs.RegisterString(category, "List of Keys:", "https://docs.unity3d.com/Manual/class-InputManager.html", "Description", true);
			MelonPrefs.RegisterString(category, "recenter", "t:joystick button 0,joystick button 3", "Recenter");
			MelonPrefs.RegisterString(category, "hide_hud", "u", "Hide HUD");
			MelonPrefs.RegisterString(category, "reset_audio", "i", "Reset audio");
			MelonPrefs.RegisterString(category, "list_joysticks", "f1", "List connected joysticks");
			GetJoySticks();
			MelonLogger.Log("OnApplicationStart > Check \"UserData/modprefs.ini\" for settings");
		}

		private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
		{
			if (MelonPrefs.GetBool(mod, "auto_controller")) GameInput.SetControllerEnabled(true);
			if (XRSettings.enabled && scene.name == "StartScreen")
			{
				if (MelonPrefs.GetBool(mod, "auto_graphics")) SetVRGraphics();
				if (MelonPrefs.GetBool(mod, "auto_recenter")) RecenterCamera();
			}
		}
		private static void SetVRBindings()
        {
			// GameInput.automaticControllerLayout = 
			// GameInput.SetBindingInternal(GameInput.Device.Controller, GameInput.Button button, GameInput.BindingSet bindingSet, int inputIndex)

		}
		private void CheckVRBindings()
        {
			if (!XRSettings.enabled)
				return;
			var device = leftController;
			if (device.isValid)
            {
				bool triggerValue;
				//if (device.TryGetFeatureValue(CommonUsages.gripButton, out triggerValue) && triggerValue)
				if (OVRInput.Get(OVRInput.Button.SecondaryThumbstickDown))
				{
					IngameMenu.main.Open();
					// GameInput.SetBinding(GameInput.Device.Controller, GameInput.Button.LeftHand, GameInput.BindingSet.Primary, "");
				}
			}
        }
		private static void SetVRGraphics()
		{
			// MelonLogger.Log("Setting VR graphics...");
			if (UwePostProcessingManager.GetDofEnabled())
			{
				UwePostProcessingManager.ToggleDof(false); MelonLogger.Log("[Graphics] Disabled Depth of field");
			}
			if (UwePostProcessingManager.GetAaMode() > 0)
			{
				UwePostProcessingManager.SetAaMode(0); MelonLogger.Log("[Graphics] Forced FAA");
			}
			if (UwePostProcessingManager.GetMotionBlurQuality() > 0)
			{
				UwePostProcessingManager.SetMotionBlurQuality(0); MelonLogger.Log("[Graphics] Disabled Motion blur");
			}
			if (UwePostProcessingManager.GetBloomLensDirtEnabled())
			{
				UwePostProcessingManager.ToggleBloomLensDirt(false); MelonLogger.Log("[Graphics] Disabled Lent Dirt");
			}
			// uGUI_OptionsPanel.SyncQualityPresetSelection();
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
				OpenVR.Compositor.SetTrackingSpace(MelonPrefs.GetBool(mod, "seated") ? ETrackingUniverseOrigin.TrackingUniverseSeated : ETrackingUniverseOrigin.TrackingUniverseStanding);
			}
		}

		private bool CheckKeysPressed(string option)
		{
			var bindings = MelonPrefs.GetString(category, option);
			foreach (var binds in bindings.Split(':'))
			{
				var bind_trues = new List<bool>();
				foreach (var bind in binds.Split(','))
				{
					bind_trues.Add(Input.GetKeyDown(bind));
				}
				if (bind_trues.Count() > 0 && bind_trues.All(t => t == true))
				{
					return true;
				}
			}
			return false;
		}

		public override void OnUpdate()
		{
			CheckVRBindings();
			if (XRSettings.enabled && CheckKeysPressed("recenter")) {
				RecenterCamera();
			} else if (CheckKeysPressed("hide_hud")) {
				MelonLogger.Log("Hiding HUD...");
				GameModeUtils.SetGameMode(GameModeOption.Creative, GameModeOption.None);
				HideForScreenshots.Hide(HideForScreenshots.HideType.HUD);
				HideForScreenshots.Hide(HideForScreenshots.HideType.Mask);
			} else if (CheckKeysPressed("reset_audio")) {
				MelonLogger.Log("Resetting audio...");
				AudioConfiguration old = AudioSettings.GetConfiguration();
				MelonLogger.Log(string.Format("Old: {0}", old.speakerMode));
				AudioSettings.Reset(AudioSettings.GetConfiguration());
				AudioConfiguration _new = AudioSettings.GetConfiguration();
				MelonLogger.Log(string.Format("New: {0}", _new.speakerMode));
			} else if (CheckKeysPressed("list_joysticks")) {
				MelonLogger.Log("Getting joysticks...");
				GetJoySticks();
			}
		}

		private void GetJoySticks()
		{
			string[] joysticks = Input.GetJoystickNames();
			MelonLogger.Log("Connected Joysticks:");
			foreach (string joystick in joysticks)
			{
				MelonLogger.Log("\t- " + joystick);
			}
			var inputDevices = new List<InputDevice>();
            InputDevices.GetDevices(inputDevices);
			MelonLogger.Log("Connected VR Devices:");
			foreach (var device in inputDevices)
			{
				if (device.characteristics == InputDeviceCharacteristics.Controller)
				{
					if (device.characteristics == InputDeviceCharacteristics.Left) leftController = device;
					else if (device.characteristics == InputDeviceCharacteristics.Right) rightController = device;
				}
				MelonLogger.Log($"\t- {device.name} ({device.role}/{device.characteristics})");
			}
		}
	}
}
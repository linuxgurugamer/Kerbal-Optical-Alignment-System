//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

/* TODO: Is there a better way to do any of this?  It feels like it's starting to get messy... 
 *            
 * 
 * */
using System;
using System.IO;
using UnityEngine;

namespace DPCamera {
	public class DPCamera : PartModule {
		[KSPField]
		public Vector3
			cameraPosition = Vector3.zero;		//Eyepoint
		[KSPField]
		public Vector3
			cameraForward = Vector3.forward;	//Viewing TOWARD Direction
		[KSPField]
		public Vector3
			cameraUp = Vector3.up;			//Viewing UP Direction
		[KSPField]
		public string
			cameraTransformName = "";		//transform 
		[KSPField]
		public float
			cameraFoV = 60;				//Zoom (FOV angle)
		[KSPField(isPersistant = false)]
		public float
			cameraClip = 0.01f;			//Distance at which clipping occurs
		[KSPField(isPersistant = false)]
		public string
			cameraName = "DPCam";			//Name Ident
		public FlightCamera nativeCam;
		protected static Transform storedCamParent;
		protected static Quaternion storedCamRotation = Quaternion.identity;
		protected static Vector3 storedCamPosition = Vector3.zero;
		protected static float storedCamFoV;
		protected static float storedCamClipPlane;
		bool inDPCam;
		bool unlockReady;
		bool mapWasActive;
		static bool buttonIsDeactivate;
		public Texture2D crosshairTexture;
		public float crosshairScale = 1;

		/*
	* This event is active when controlling the vessel with the part. 
	* Adds "Clicklable Button" to rightclick GUI. When the new button is
	* clicked, the ActivateEvent() function is called.
	*/
		[KSPEvent(guiActive = true, guiName = "View from Here")]
		public void ActivateEvent () {
			if (!inDPCam) {
				printToLog ("[DPCamera] - Enter DPCam View", 1);
				ScreenMessages.PostScreenMessage ("Docking View Activated - Press " + GameSettings.CAMERA_MODE.primary + " to Exit", 5.0f, ScreenMessageStyle.UPPER_CENTER);
				
				nativeCam = FlightCamera.fetch;
				
				// Grab all Standard ingame camera parameters
				storedCamParent = nativeCam.transform.parent;
				storedCamClipPlane = Camera.main.nearClipPlane;
				storedCamFoV = Camera.main.fieldOfView;
				storedCamPosition = nativeCam.transform.localPosition;
				storedCamRotation = nativeCam.transform.localRotation;
				
				// Set parameters to new values
				setDPCam ();
				
				// Lockout the "View" camera mode key from being used when in DPCamera view
				InputLockManager.SetControlLock (ControlTypes.CAMERAMODES, "DPCamLock");
				// Remove the rightclick GUI so it's not in the way
				UIPartActionController.Instance.Deselect (true);
				// Flag for FixedUpdate conditional statement below
				inDPCam = true;
				// This will hide the Activate event, and show the Deactivate event.
				buttonIsDeactivate = true;
			} else {
				printToLog ("[DPCamera] DPCam Activated when while inDPCam", 2);
			}
		}
		
		/* Because sometimes you can rightclick and get the menu back (Shielded Docking Port) 
	 */
		
		[KSPEvent(guiActive = true, guiName = "Close this Window", active = false)]
		public void DeactivateEvent () {
			// Remove the rightclick GUI so it's not in the way
			UIPartActionController.Instance.Deselect (true);
		}
		
		public void setDPCam ()	{
			printToLog ("[DPCamera] - Set DPCam View Values", 1);
			nativeCam.setTarget (null);
			nativeCam.transform.parent = (cameraTransformName.Length > 0) ? part.FindModelTransform (cameraTransformName) : part.transform;
			Camera.main.nearClipPlane = cameraClip;
			nativeCam.SetFoV (cameraFoV);
			nativeCam.transform.localPosition = cameraPosition;
			nativeCam.transform.localRotation = Quaternion.LookRotation (cameraForward, cameraUp);
		}

		public void unsetDPCam () {
			if (inDPCam) {
				inDPCam = false;
				// Set parameters to old values
				printToLog ("[DPCamera] - Clear DPCam View Values and reset to stored values", 1);
				nativeCam.transform.parent = storedCamParent;
				nativeCam.transform.localPosition = storedCamPosition;
				nativeCam.transform.localRotation = storedCamRotation;
				Camera.main.nearClipPlane = storedCamClipPlane;
				nativeCam.SetFoV (storedCamFoV);
				if (FlightGlobals.ActiveVessel != null && HighLogic.LoadedScene == GameScenes.FLIGHT) {
					nativeCam.setTarget (FlightGlobals.ActiveVessel.transform);
				}
				
				// This will hide the Deactivate event, and show the Activate event.
				buttonIsDeactivate = false;
			}
		}
		
		/*
	* Escape DPCamera view - Escape happens when the Camera Mode, Next Vessel, or 
	* Previous Vessel keys are pressed (Default 'C','[', and ']' respectively.  Using these GameSettings
	* rather then hard coding ensures that they will track with any changes the player makes
	* in key assignments.
	*/
		
		public void FixedUpdate ()
		{
			if (part.State == PartStates.DEAD) {
				printToLog ("[DPCamera] - Dead", 1);
				this.unsetDPCam ();
				unlockReady = true;
			}
			if (unlockReady) {
				InputLockManager.RemoveControlLock ("DPCamLock");
				unlockReady = false;
			}
			if (inDPCam) {
				if (!MapView.MapIsEnabled && (Input.GetKeyDown (GameSettings.CAMERA_MODE.primary) || GameSettings.FOCUS_NEXT_VESSEL.GetKeyDown() || GameSettings.FOCUS_PREV_VESSEL.GetKeyDown () )) {
					printToLog ("[DPCamera] - Keypress Detected", 1);
					this.unsetDPCam ();
					unlockReady = true;
				}
				if (nativeCam.Target != null) {
					printToLog ("[DPCamera] - Target not null", 1);
					this.unsetDPCam ();
					unlockReady = true;
				}
				if (MapView.MapIsEnabled && !mapWasActive) {
					mapWasActive = true;			
				}
				if (mapWasActive && !MapView.MapIsEnabled) {
					this.setDPCam ();
					mapWasActive = false;
				}
			} else {
				if (Input.GetKeyUp (GameSettings.CAMERA_MODE.primary) && InputLockManager.IsLocked (ControlTypes.CAMERAMODES)) {
					printToLog ("[DPCamera] - Cam Key released with lock still set", 1);
				}
			}
			if (buttonIsDeactivate) {
				Events ["ActivateEvent"].active = false;
				Events ["DeactivateEvent"].active = true;
			} else {
				Events ["ActivateEvent"].active = true;
				Events ["DeactivateEvent"].active = false;
			}
		}
		
		/* Adds Recticle to view 
	*/
		void OnGUI () {
			//if not paused and in DPCamera view
			if (Time.timeScale != 0 && inDPCam && !MapView.MapIsEnabled) {
				if (crosshairTexture != null)
					GUI.DrawTexture (new Rect ((Screen.width - crosshairTexture.width * crosshairScale) / 2, (Screen.height - crosshairTexture.height * crosshairScale) / 2, crosshairTexture.width * crosshairScale, crosshairTexture.height * crosshairScale), crosshairTexture);
				else
					printToLog ("[DPCamera] No reticle texture set in the Inspector", 2);
			}
		}
		
		/* Initialize Routine - Load Texture
	*/
		public override void OnStart (StartState state)	{
			printToLog ("[DPCamera] OnStart Called: State was " + state, 1);
			base.OnStart (state);

			if (HighLogic.LoadedScene != GameScenes.FLIGHT)
				return;

			crosshairTexture = LoadTextureFile ("reticle.png");
			part.OnJustAboutToBeDestroyed += cleanupDPCam;
		}

		private void onGameSceneLoadRequested(GameScenes gameScene) {
			printToLog ("[DPCamera] - Game Scene Load Requested: " + gameScene, 1);

			if (HighLogic.LoadedScene != GameScenes.FLIGHT)
				return;

			if (inDPCam) {
				printToLog ("[DPCamera] - So we kill our camera", 1);
				this.unsetDPCam ();
				unlockReady = true;
			}
		}

		public override void OnAwake() {
			printToLog ("[DPCamera] - OnAwake: " + HighLogic.LoadedScene, 1);
			base.OnAwake();

			GameEvents.onGameSceneLoadRequested.Add(onGameSceneLoadRequested);
		}

		/* Stolen from MovieTime plugin :)
	*/
		public static Texture2D LoadTextureFile (string fileName) {
			try {
				string path;
				
				path = KSPUtil.ApplicationRootPath.Replace (@"\", "/") + "/GameData/FP_KOAS/Textures/" + fileName;

				byte[] texture = File.ReadAllBytes (path);
				Texture2D retVal = new Texture2D (1, 1);
				retVal.LoadImage (texture);
				return retVal;
			} catch (Exception ex) {
				Debug.Log (string.Format ("[DPCamera] LoadTextureFile exception: {0}", ex.Message));
			}
			return null;
		}

		void cleanupDPCam() {
			printToLog ("[DPCamera] - cleanup Fired", 2);
			this.unsetDPCam ();
			unlockReady = true;
		}

		void printToLog(string outText, int styleFlag) {
#if DEBUG
			switch (styleFlag) {
			case 1:
				Debug.Log (outText);
				break;
			case 2:
				Debug.LogWarning (outText);
				break;
			case 3:
				Debug.LogError (outText);
				break;
			default:
				Debug.LogError ("[DPCamera] Improper call to internal logger.");
				break;
			}
#endif
		}
	}
}
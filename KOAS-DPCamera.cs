//
//  KOAS-DPCamera.cs
//
//  Author:
//      Ted Thompson <ted@federalproductions.com>
//
//  Copyright (c) 2016 Ted
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.

/* TODO: none 
 *            
 * 
 * */
using System;
using System.IO;
using UnityEngine;

namespace DPCamera
{
    public class DPCamera : PartModule
    {
        [KSPField]
        public Vector3
            cameraPosition = Vector3.zero;      //Eyepoint
        [KSPField]
        public Vector3
            cameraForward = Vector3.forward;    //Viewing TOWARD Direction
        [KSPField]
        public Vector3
            cameraUp = Vector3.up;          //Viewing UP Direction
        [KSPField]
        public string
            cameraTransformName = "";       //transform 
        [KSPField]
        public float
            cameraFoV = 60;             //Zoom (FOV angle)
        [KSPField]
        public Boolean
            reticleVisible = true;
        [KSPField(isPersistant = false)]
        public float
            cameraClip = 0.01f;         //Distance at which clipping occurs
        [KSPField(isPersistant = false)]
        public string
            cameraName = "DPCam";           //Name Ident
        public FlightCamera nativeCam;
        protected static Transform storedCamParent;
        protected static Transform DPCamTransform;
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
        public void ActivateEvent()
        {
            if (!inDPCam) {
                printToLog("Enter DPCam View", 1);
                ScreenMessages.PostScreenMessage("Docking View Activated - Press " + GameSettings.CAMERA_MODE.primary + " to Exit", 5.0f, ScreenMessageStyle.UPPER_CENTER);

                nativeCam = FlightCamera.fetch;

                // Grab all Standard ingame camera parameters
                storedCamParent = nativeCam.transform.parent;
                storedCamClipPlane = Camera.main.nearClipPlane;
                storedCamFoV = Camera.main.fieldOfView;
                storedCamPosition = nativeCam.transform.localPosition;
                storedCamRotation = nativeCam.transform.localRotation;

                // Set parameters to new values
                getDPCamTransform();
                setDPCam();

                // Lockout the "View" camera mode key from being used when in DPCamera view
                InputLockManager.SetControlLock(ControlTypes.CAMERAMODES, "DPCamLock");
                // Remove the rightclick GUI so it's not in the way
                UIPartActionController.Instance.Deselect(true);
                // Flag for FixedUpdate conditional statement below
                inDPCam = true;
                // This will hide the Activate event, and show the Deactivate event.
                buttonIsDeactivate = true;
            } else {
                printToLog("DPCam Activated when while inDPCam", 2);
            }
        }

        /* Because sometimes you can rightclick and get the menu back (Shielded Docking Port) 
	 */

        [KSPEvent(guiActive = true, guiName = "Close this Window", active = false)]
        public void DeactivateEvent()
        {
            // Remove the rightclick GUI so it's not in the way
            UIPartActionController.Instance.Deselect(true);
        }

        public void getDPCamTransform()
        {
            printToLog("Get DPCam transform", 1);
            DPCamTransform = (cameraTransformName.Length > 0) ? part.FindModelTransform(cameraTransformName) : part.transform;
        }

        public void setDPCam()
        {
            printToLog("Set DPCam View Values", 1);
            getDPCamTransform();
            nativeCam.SetTargetNone();
            nativeCam.transform.parent = DPCamTransform;
            nativeCam.DeactivateUpdate();
            Camera.main.nearClipPlane = cameraClip;
            nativeCam.SetFoV(cameraFoV);
            nativeCam.transform.localPosition = cameraPosition;
            nativeCam.transform.localRotation = Quaternion.LookRotation(cameraForward, cameraUp);
            // FlightCamera.SetTarget(nativeCam.transform.localRotation);
            GameEvents.onPartCouple.Add(onPartCouple);

        }

        public void unsetDPCam()
        {
            if (inDPCam) {
                inDPCam = false;
                // Set parameters to old values
                printToLog("Clear DPCam View Values and reset to stored values", 1);
                nativeCam.transform.parent = storedCamParent;
                nativeCam.transform.localPosition = storedCamPosition;
                nativeCam.transform.localRotation = storedCamRotation;
                Camera.main.nearClipPlane = storedCamClipPlane;
                nativeCam.SetFoV(storedCamFoV);
                if (FlightGlobals.ActiveVessel != null && HighLogic.LoadedScene == GameScenes.FLIGHT) {
                    FlightCamera.SetTarget(FlightGlobals.ActiveVessel.transform);
                }
                // This will hide the Deactivate event, and show the Activate event.
                buttonIsDeactivate = false;
                GameEvents.onPartCouple.Remove(onPartCouple);

            }
        }
        private void onPartCouple(GameEvents.FromToAction<Part, Part> action)
        {
            if (HighLogic.LoadedSceneIsFlight && FlightGlobals.ActiveVessel)
            {

                if (this.part.vessel == FlightGlobals.ActiveVessel)
                {
                    ScreenMessages.PostScreenMessage("EXTERNAL CAMERA LOS", 5.0f, ScreenMessageStyle.UPPER_CENTER);
                    this.unsetDPCam();
                    unlockReady = true;
                }

                Debug.LogError("Docked FROM: " + action.from.vessel.vesselName);
                Debug.LogError("Docked TO: " + action.to.vessel.vesselName);

                Debug.LogError("Docked TO Type Vessel: " + action.to.vessel.vesselType);

                Debug.LogError("Docked FROM ID: " + action.from.vessel.id.ToString());
                Debug.LogError("Docked TO ID: " + action.to.vessel.id.ToString());
               
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
				printToLog ("Dead", 1);
				this.unsetDPCam ();
				unlockReady = true;
			}
			if (unlockReady && (Input.GetKeyUp (GameSettings.CAMERA_MODE.primary) || GameSettings.FOCUS_NEXT_VESSEL.GetKeyUp () || GameSettings.FOCUS_PREV_VESSEL.GetKeyUp ())) {
				InputLockManager.RemoveControlLock ("DPCamLock");
				unlockReady = false;
			}
			if (inDPCam) {
				if (!MapView.MapIsEnabled && (Input.GetKeyDown (GameSettings.CAMERA_MODE.primary) || GameSettings.FOCUS_NEXT_VESSEL.GetKeyDown () || GameSettings.FOCUS_PREV_VESSEL.GetKeyDown ())) {
					printToLog ("Keypress Detected", 1);
					if (CameraManager.Instance.currentCameraMode == CameraManager.CameraMode.IVA) {
						CameraManager.Instance.SetCameraMode (CameraManager.CameraMode.Flight);
						this.setDPCam ();
						return;
					}
					this.unsetDPCam ();
					unlockReady = true;
				}

                Debug.Log("nativeCam.targetMode: " + nativeCam.targetMode.ToString() + "  unlockReady: " + unlockReady.ToString());
                if (nativeCam.Target != null)
                    Debug.Log("nativeCam.Target not null");
                if (nativeCam.targetMode != FlightCamera.TargetMode.None  && !unlockReady) {
                //if (nativeCam.Target != null && !unlockReady) {
                    printToLog ("Target not null", 1);
					this.unsetDPCam ();
					InputLockManager.RemoveControlLock ("DPCamLock");
					// unlockReady = true;
				}

                if (this.part.vessel != FlightGlobals.ActiveVessel) {
					ScreenMessages.PostScreenMessage ("EXTERNAL CAMERA LOS", 5.0f, ScreenMessageStyle.UPPER_CENTER);
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
					printToLog ("Cam Key released with lock still set", 1);
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
		void OnGUI ()
		{
			//if not paused and in DPCamera view
			if (Time.timeScale != 0 && inDPCam && !MapView.MapIsEnabled && reticleVisible && CameraManager.Instance.currentCameraMode != CameraManager.CameraMode.IVA) {
				if (crosshairTexture != null)
					GUI.DrawTexture (new Rect ((Screen.width - crosshairTexture.width * crosshairScale) / 2, (Screen.height - crosshairTexture.height * crosshairScale) / 2, crosshairTexture.width * crosshairScale, crosshairTexture.height * crosshairScale), crosshairTexture);
				else
					printToLog ("No reticle texture set in the Inspector", 2);
			}
		}
		
		/* Initialize Routine - Load Texture
	*/
		public override void OnStart (StartState state)
		{
			printToLog ("OnStart Called: State was " + state, 1);
			base.OnStart (state);

			if (HighLogic.LoadedScene != GameScenes.FLIGHT)
				return;

			crosshairTexture = LoadTextureFile ("reticle.png");
			part.OnJustAboutToBeDestroyed += cleanupDPCam;
		}

		private void onGameSceneLoadRequested (GameScenes gameScene)
		{
			printToLog ("Game Scene Load Requested: " + gameScene, 1);

			if (HighLogic.LoadedScene != GameScenes.FLIGHT)
				return;

			if (inDPCam) {
				printToLog ("So we kill our camera", 1);
				this.unsetDPCam ();
				unlockReady = true;
			}
		}

		public override void OnAwake ()
		{
			printToLog ("OnAwake: " + HighLogic.LoadedScene, 1);
			base.OnAwake ();

			GameEvents.onGameSceneLoadRequested.Add (onGameSceneLoadRequested);
		}

		public void OnDestroy ()
		{
			printToLog ("OnDestroy", 1);
			cleanupDPCam ();
			InputLockManager.RemoveControlLock ("DPCamLock");
            GameEvents.onPartCouple.Remove(onPartCouple);
        }
		
		public void OnUnload ()
		{
			printToLog ("OnUnload", 1);
			cleanupDPCam ();
			InputLockManager.RemoveControlLock ("DPCamLock");
		}

		/* Stolen from MovieTime plugin :)
	*/
		public static Texture2D LoadTextureFile (string fileName)
		{
			try {
				string path;
				
				path = KSPUtil.ApplicationRootPath.Replace (@"\", "/") + "/GameData/FP_KOAS/Textures/" + fileName;

				byte[] texture = File.ReadAllBytes (path);
				Texture2D retVal = new Texture2D (1, 1);
				retVal.LoadImage (texture);
				return retVal;
			} catch (Exception ex) {
				Debug.Log (string.Format ("LoadTextureFile exception: {0}", ex.Message));
			}
			return null;
		}

		void cleanupDPCam ()
		{
			printToLog ("CleanupDPCam Fired", 2);
			this.unsetDPCam ();
			unlockReady = true;
		}

		void printToLog (string outText, int styleFlag)
		{
#if DEBUG
			switch (styleFlag) {
			case 1:
				Debug.Log ("[DPCamera] - " + outText);
				break;
			case 2:
				Debug.LogWarning ("[DPCamera] - " + outText);
				break;
			case 3:
				Debug.LogError ("[DPCamera] - " + outText);
				break;
			default:
				Debug.LogError ("[DPCamera] - Improper call to internal logger.");
				break;
			}
#endif
		}
	}
}

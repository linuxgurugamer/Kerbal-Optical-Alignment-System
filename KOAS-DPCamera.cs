//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

/* TODO: Bugs 
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
		public Vector3 cameraPosition = Vector3.zero;	//Eyepoint
		[KSPField]
		public Vector3 cameraForward = Vector3.forward;	//Viewing Direction
		[KSPField]
		public Vector3 cameraUp = Vector3.up;		//UP direction
		[KSPField]
		public string cameraTransformName = "";		//transform
		[KSPField]
		public float cameraFoV = 60;			//Zoom (FOV angle)
		[KSPField(isPersistant = false)]
		public float cameraClip = 0.01f;		//Distance at which clipping occurs
		[KSPField(isPersistant = false)]
		public string cameraName = "DPCam";		//Name Ident
		public FlightCamera stdCam;
		protected static Transform sOrigParent;
		protected static Quaternion sOrigRotation = Quaternion.identity;
		protected static Vector3 sOrigPosition = Vector3.zero;
		protected static float sOrigFov;
		protected static float sOrigClip;
		bool inDPCam;
		bool unlockReady;
		bool mapWasActive;
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
			if (!inDPCam)
			{
				ScreenMessages.PostScreenMessage("Docking View Activated - Press " + GameSettings.CAMERA_MODE.primary + " to Escape", 5.0f, ScreenMessageStyle.UPPER_CENTER);
				
				stdCam = FlightCamera.fetch;
				
				// Grab all Standard ingame camera parameters
				sOrigParent = stdCam.transform.parent;
				sOrigClip = Camera.main.nearClipPlane;
				sOrigFov = Camera.main.fieldOfView;
				sOrigPosition = stdCam.transform.localPosition;
				sOrigRotation = stdCam.transform.localRotation;
				
				// Set parameters to new values
				setDPCam ();
				
				// Lockout the "View" camera mode key from being used when in DPCamera view
				InputLockManager.SetControlLock(ControlTypes.CAMERAMODES, "DPCamLock");
				// Remove the rightclick GUI so it's not in the way
				UIPartActionController.Instance.Deselect(true);
				// Flag for FixedUpdate conditional statement below
				inDPCam = true;
				// This will hide the Activate event, and show the Deactivate event.
				Events["ActivateEvent"].active = false;
				Events["DeactivateEvent"].active = true;
			}
			else
			{
				Debug.LogWarning ("[DPCamera] DPCam Activated when while inDPCam");
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
		
		public void setDPCam()
		{
			stdCam.setTarget (null);
			stdCam.transform.parent = (cameraTransformName.Length > 0) ? part.FindModelTransform(cameraTransformName) : part.transform;
			Camera.main.nearClipPlane = cameraClip;
			stdCam.SetFoV (cameraFoV);
			stdCam.transform.localPosition = cameraPosition;
			stdCam.transform.localRotation = Quaternion.LookRotation(cameraForward, cameraUp);
		}

		public void unsetDPCam()
		{
			// Set parameters to old values
			stdCam.transform.parent = sOrigParent;
			stdCam.transform.localPosition = sOrigPosition;
			stdCam.transform.localRotation = sOrigRotation;
			Camera.main.nearClipPlane = sOrigClip;
			stdCam.SetFoV (sOrigFov);
			if (FlightGlobals.ActiveVessel != null && HighLogic.LoadedScene == GameScenes.FLIGHT)
			{
				stdCam.setTarget(FlightGlobals.ActiveVessel.transform);
			}
			
			// This will hide the Deactivate event, and show the Activate event.
			Events["ActivateEvent"].active = true;
			Events["DeactivateEvent"].active = false;
			
			// Flag for FixedUpdate conditional statement, keeps this block from executing
			inDPCam = false;
			unlockReady = true;
		}
		
		/*
	* Escape DPCamera view - Escape happens when the Camera Mode, Next Vessel, or 
	* Previous Vessel keys are pressed (Default 'C','[', and ']' respectively.  Using these GameSettings
	* rather then hard coding ensures that they will track with any changes the player makes
	* in key assignments.
	*/
		
		public void FixedUpdate()
		{
			if (inDPCam && !MapView.MapIsEnabled && (Input.GetKeyDown (GameSettings.CAMERA_MODE.primary) || GameSettings.FOCUS_NEXT_VESSEL.GetKeyDown () || GameSettings.FOCUS_PREV_VESSEL.GetKeyDown () ))
			{
				unsetDPCam ();		
			}
			if (inDPCam && stdCam.Target != null)
			{
				unsetDPCam ();
			}
			if (unlockReady && Input.GetKeyUp (GameSettings.CAMERA_MODE.primary))
			{
				InputLockManager.RemoveControlLock("DPCamLock");
				unlockReady = false;
			}
			if (MapView.MapIsEnabled)
			{
				mapWasActive = true;			
			}
			if (mapWasActive && !MapView.MapIsEnabled)
			{
				setDPCam ();
				mapWasActive = false;
			}
			// base.OnFixedUpdate();
		}
		
		/* Adds Recticle to view 
	*/
		void OnGUI()
		{
			//if not paused and in DPCamera view
			if (Time.timeScale != 0 && inDPCam && !MapView.MapIsEnabled)
			{
				if(crosshairTexture!=null)
					GUI.DrawTexture(new Rect((Screen.width-crosshairTexture.width*crosshairScale)/2 ,(Screen.height-crosshairTexture.height*crosshairScale)/2, crosshairTexture.width*crosshairScale, crosshairTexture.height*crosshairScale),crosshairTexture);
				else
					Debug.LogWarning("[DPCamera] No reticle texture set in the Inspector");
			}
		}
		
		/* Initialize Routine - Load Texture
	*/
		public override void OnStart(StartState state)
		{
			
			crosshairTexture = LoadTextureFile("reticle.png");
			
			base.OnStart (state);
		}
		
		/* Stolen from MovieTime plugin :)
	*/
		public static Texture2D LoadTextureFile(string fileName) 
		{
			try {
				string path;
				
				path = KSPUtil.ApplicationRootPath.Replace(@"\", "/") + "/GameData/FP_KOAS/Textures/" + fileName;

				byte[] texture = File.ReadAllBytes(path);
				Texture2D retVal = new Texture2D(1, 1);
				retVal.LoadImage(texture);
				return retVal;
			} catch (Exception ex) {
				Debug.LogError(string.Format("[DPCamera] LoadTextureFile exception: {0}", ex.Message));
			}
			return null;
		}
	}
}
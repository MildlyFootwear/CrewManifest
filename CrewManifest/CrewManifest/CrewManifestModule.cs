﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Toolbar;

namespace CrewManifest
{

    
    public class CrewManifestModule : PartModule
    {
        [KSPEvent(guiActive = true, guiName = "Destroy Part", active = true)]
        public void DestoryPart()
        {
            if (this.part != null)
                this.part.temperature = 5000;
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            if(this.part != null && part.name == "crewManifest")
                Events["DestoryPart"].active = true;
            else
                Events["DestoryPart"].active = false;
        }
    }
    
      
    [KSPAddon(KSPAddon.Startup.EveryScene, false)]
    public class ManifestBehaviour : MonoBehaviour
    {
        //Game object that keeps us running
        public static GameObject GameObjectInstance;
        public static SettingsManager Settings = new SettingsManager();
        private float interval = 30F;
        private float intervalCrewCheck = 0.5f;

        private IButton button;

        public void Awake()
        {
            DontDestroyOnLoad(this);
            Settings.Load();
            InvokeRepeating("RunSave", interval, interval);
            InvokeRepeating("CrewCheck", intervalCrewCheck, intervalCrewCheck);

            button = ToolbarManager.Instance.add("CrewManifest", "CrewManifest");
            button.TexturePath = "CrewManifest/Plugins/IconOff_24";
            button.ToolTip = "Crew Manifest Settings";
            button.Visibility = new GameScenesVisibility(GameScenes.FLIGHT);
            button.OnClick += (e) =>
            {
                if (!MapView.MapIsEnabled && !PauseMenu.isOpen && !FlightResultsDialog.isDisplaying  &&
                    FlightGlobals.fetch != null && FlightGlobals.ActiveVessel != null &&
                    ManifestController.GetInstance(FlightGlobals.ActiveVessel).CanDrawButton
                    )
                {
                    button.TexturePath = ManifestController.GetInstance(FlightGlobals.ActiveVessel).ShowWindow ? "CrewManifest/Plugins/IconOff_24" : "CrewManifest/Plugins/IconOn_24";
                    ManifestController.GetInstance(FlightGlobals.ActiveVessel).ShowWindow = !ManifestController.GetInstance(FlightGlobals.ActiveVessel).ShowWindow;
                }
            };
        }

        public void OnDestroy()
        {
            CancelInvoke("RunSave");
            CancelInvoke("CrewCheck");
            button.Destroy();
        }

        public void OnGUI()
        {
            Resources.SetupGUI();

            if(Settings.ShowDebugger)
                Settings.DebuggerPosition = GUILayout.Window(398643, Settings.DebuggerPosition, DrawDebugger, "Manifest Debug Console", GUILayout.MinHeight(20));
        }
        
        public void Update()
        {
            if (FlightGlobals.fetch != null && FlightGlobals.ActiveVessel != null)
            {
                if (HighLogic.LoadedScene == GameScenes.FLIGHT)
                {
                    //Instantiate the controller for the active vessel.
                    ManifestController.GetInstance(FlightGlobals.ActiveVessel).CanDrawButton = true;
                }
            }
        }

        public void RunSave()
        {
            Save();
        }

        private void Save()
        {
            if (HighLogic.LoadedScene == GameScenes.FLIGHT && FlightGlobals.fetch != null && FlightGlobals.ActiveVessel != null)
            {
                ManifestUtilities.LogMessage("Saving Manifest Settings...", "Info");
                Settings.Save();
            }
        }

 


        private void DrawDebugger(int windowId)
        {
            GUILayout.BeginVertical();

            ManifestUtilities.DebugScrollPosition = GUILayout.BeginScrollView(ManifestUtilities.DebugScrollPosition, GUILayout.Height(300), GUILayout.Width(500));
            GUILayout.BeginVertical();

            foreach(string error in ManifestUtilities.Errors)
                GUILayout.TextArea(error, GUILayout.Width(460));

            GUILayout.EndVertical();
            GUILayout.EndScrollView();

            GUILayout.EndVertical();
            GUI.DragWindow(new Rect(0, 0, Screen.width, 30));
        }
    }
}

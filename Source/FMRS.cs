/*
 * The MIT License (MIT)
 * 
 * Copyright (c) 2014 SIT89
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
*/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using UnityEngine;
using KSP.IO;


namespace FMRS
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class FMRS : FMRS_Core
    {
        public ApplicationLauncherButton Stock_Toolbar_Button;      

/*************************************************************************************************************************/
        public FMRS()
        {
            //nothing
        }


/*************************************************************************************************************************/
        void Awake()
        {
            FMRS_core_awake();

            Debug.Log("#### FMRS Version: " + mod_vers);

            if (Debug_Active)
                Debug.Log("#### FMRS: FMRS On Awake");

            stb_texture = new Texture2D(36, 36, TextureFormat.RGBA32, false);
            stb_texture.LoadImage(System.IO.File.ReadAllBytes(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "StockToolbar.png")));
            GameEvents.onGUIApplicationLauncherReady.Add(add_toolbar_button);
        }


/*************************************************************************************************************************/
        void Start()
        {
            if (ToolbarManager.ToolbarAvailable)
            {
                Toolbar_Button = ToolbarManager.Instance.add("FMRS", "FMRSbutton");

                blz_toolbar_available = true;

                if (_SETTING_Enabled)
                    Toolbar_Button.TexturePath = "FMRS/icon_enabled";
                else
                    Toolbar_Button.TexturePath = "FMRS/icon_disabled";

                Toolbar_Button.ToolTip = "Flight Manager for Reusable Stages";
                Toolbar_Button.Visibility = new GameScenesVisibility(GameScenes.FLIGHT);
                Toolbar_Button.OnClick += (e) => toolbar_button_clicked();
            }

            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: entering Start()");

            GameEvents.onLaunch.Add(launch_routine);

            if (FlightGlobals.activeTarget.Landed && !_SAVE_Has_Launched)
                _SAVE_Has_Closed = true;

            if (FlightGlobals.ActiveVessel.situation == Vessel.Situations.PRELAUNCH)
            {
                if (Debug_Active)
                    Debug.Log("#### FMRS: ActiveVessel is prelaunch");
                _SAVE_Has_Closed = false;
            }

            if (_SETTING_Enabled && !_SAVE_Has_Closed)
            {
                if (FlightGlobals.ActiveVessel.situation != Vessel.Situations.PRELAUNCH && _SAVE_Has_Launched)
                    if (_SAVE_Main_Vessel == FlightGlobals.ActiveVessel.id)
                        GamePersistence.SaveGame("persistent", HighLogic.SaveFolder, SaveMode.OVERWRITE);

                flight_scene_start_routine();
            }

            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: leaving Start ()");
        }


/*************************************************************************************************************************/
        void Update()
        {
            flight_scene_update_routine();

            /*if (ThrottleLogger != null)
                ThrottleLogger.Update(Debug_Active, Debug_Level_1_Active);*/
        }


/*************************************************************************************************************************/
        void FixedUpdate()
        {
            /*if (ThrottleLogger != null)
                ThrottleLogger.FixedUpdate();*/
        }


/*************************************************************************************************************************/
        void OnDestroy()
        {
            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: enter OnDestroy()");

            if (ToolbarManager.ToolbarAvailable)
                Toolbar_Button.Destroy();

            GameEvents.onGUIApplicationLauncherReady.Remove(add_toolbar_button);
            ApplicationLauncher.Instance.RemoveModApplication(Stock_Toolbar_Button);

            disable_FMRS();

            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: leave OnDestroy()");
        }


/*************************************************************************************************************************/
        public void add_toolbar_button()
        {
            Stock_Toolbar_Button = ApplicationLauncher.Instance.AddModApplication(
                toolbar_button_clicked,
                toolbar_button_clicked,
                null, null, null, null,
                ApplicationLauncher.AppScenes.FLIGHT | ApplicationLauncher.AppScenes.MAPVIEW,
                stb_texture);
        }
    }





/*************************************************************************************************************************/
/*************************************************************************************************************************/
/*************************************************************************************************************************/
/*************************************************************************************************************************/
/*************************************************************************************************************************/
    [KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
    public class FMRS_Space_Center : FMRS_Core
    {
        float delay = 35;


/*************************************************************************************************************************/
        public void FMRS()
        {
            //nothing
        }


/*************************************************************************************************************************/
        void Awake()
        {
            FMRS_core_awake();

            Debug.Log("#### FMRS Version: " + mod_vers);

            if (_SETTING_Enabled)
                get_dropped_vessels();

            if (Debug_Active)
                Debug.Log("#### FMRS: FMRS_Space_Center On Awake");
        }


/*************************************************************************************************************************/
        void Start()
        {
            if (Debug_Active)
                Debug.Log("#### FMRS: FMRS_Space_Center On Start");
        }


/*************************************************************************************************************************/
        void Update()
        {
            //nothing
        }


/*************************************************************************************************************************/
        void FixedUpdate()
        {
            Game savegame;
            ProtoVessel temp_proto;
            int load_vessel;

            if (_SAVE_Kick_To_Main)
            {
                if (delay > 0)
                    delay--;
                else
                {
                    _SAVE_Kick_To_Main = false;
                    write_save_values_to_file();

                    savegame = GamePersistence.LoadGame("FMRS_main_save", HighLogic.SaveFolder + "/FMRS", false, false);
                    temp_proto = savegame.flightState.protoVessels.Find(p => p.vesselID == _SAVE_Main_Vessel);

                    if (temp_proto != null)
                    {
                        for (load_vessel = 0; load_vessel < savegame.flightState.protoVessels.Count && savegame.flightState.protoVessels[load_vessel].vesselID.ToString() != temp_proto.vesselID.ToString(); load_vessel++) ;

                        if (load_vessel < savegame.flightState.protoVessels.Count)
                            FlightDriver.StartAndFocusVessel(savegame, load_vessel);
                    }
                }
            }
        }


/*************************************************************************************************************************/
        void OnDestroy()
        {
            disable_FMRS();
        }
    }
}
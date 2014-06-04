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
using UnityEngine;
using KSP.IO;



namespace FMRS
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class FMRS : FMRS_Core
    {
        private IButton Toolbar_Button;


/*************************************************************************************************************************/
        public FMRS()
        {
            //nothing
        }


/*************************************************************************************************************************/
        void Awake()
        {
            load_save_file();

            if (Debug_Active)
                Debug.Log("#### FMRS: FMRS On Awake");
            
            if (ToolbarManager.ToolbarAvailable)
            {
                Toolbar_Button = ToolbarManager.Instance.add("FMRS", "FMRSbutton");

                if (_SETTING_Enabled)
                    Toolbar_Button.TexturePath = "FMRS/icon_enabled";
                else
                    Toolbar_Button.TexturePath = "FMRS/icon_disabled";

                Toolbar_Button.ToolTip = "Flight Manager for Reusable Stages";
                Toolbar_Button.Visibility = new GameScenesVisibility(GameScenes.FLIGHT);
                Toolbar_Button.OnClick += (e) => toolbar_button_clicked();
            }
            else
            {
                _SETTING_Enabled = true;
            }
        }


/*************************************************************************************************************************/
        void Start()
        {
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
                    if (get_save_value("_SAVE_Main_Vessel") == FlightGlobals.ActiveVessel.id.ToString())
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
        }


/*************************************************************************************************************************/
        void FixedUpdate()
        {
            //nothing
        }


/*************************************************************************************************************************/
        void OnDestroy()
        {
            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: enter OnDestroy()");

            if (ToolbarManager.ToolbarAvailable)
                Toolbar_Button.Destroy();

            disable_FMRS();

            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: leave OnDestroy()");
        }

/*************************************************************************************************************************/
        public void toolbar_button_clicked()
        {
            if (Debug_Active)
                Debug.Log("#### FMRS: Toolbar Button Clicked");

            if (_SETTING_Enabled)
            {
                if (Debug_Active)
                    Debug.Log("#### FMRS: disable plugin form toolbar");

                _SETTING_Enabled = false;
                _SAVE_Has_Closed = true;

                Toolbar_Button.TexturePath = "FMRS/icon_disabled";

                delete_dropped_vessels();
                disable_FMRS();

                if (FlightGlobals.ActiveVessel.id.ToString() != get_save_value("_SAVE_Main_Vessel") && _SAVE_Has_Launched)
                    jump_to_vessel("Main");
            }
            else
            {
                if (Debug_Active)
                    Debug.Log("#### FMRS: enable plugin form toolbar");

                Toolbar_Button.TexturePath = "FMRS/icon_enabled";
                _SETTING_Enabled = true;

                if (FlightGlobals.ActiveVessel.situation == Vessel.Situations.PRELAUNCH)
                {
                    if (Debug_Active)
                        Debug.Log("#### FMRS: start plugin on launchpad");

                    GameEvents.onLaunch.Add(launch_routine);
                    flight_scene_start_routine();
                }

                if (FlightGlobals.ActiveVessel.Landed)
                {
                    if (Debug_Active)
                        Debug.Log("#### FMRS: start plugin not on launchpad");

                    reset_n_launchpad = false;
                    n_launchpad_preflight = true;
                    GameEvents.onLaunch.Add(launch_routine);
                    flight_scene_start_routine();
                }
            }
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

/*************************************************************************************************************************/
        public void FMRS()
        {
            //nothing
        }


/*************************************************************************************************************************/
        void Awake()
        {
            load_save_file();

            if (_SETTING_Enabled)
                get_dropped_vessels();

            if (Debug_Active)
                Debug.Log("#### FMRS: FMRS_Space_Center On Awake");
        }


/*************************************************************************************************************************/
        void Start()
        {
            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS_Space_Center: entering Start()");

            Game savegame;
            ProtoVessel temp_proto;
            int load_vessel;

            if (Debug_Active)
                Debug.Log("#### FMRS: FMRS_Space_Center On Start");

            if (_SAVE_Has_Recovered)
            {
                _SAVE_Has_Recovered = false;

                write_save_values_to_file();

                savegame = GamePersistence.LoadGame("FMRS_main_save", HighLogic.SaveFolder + "/FMRS", false, false);

                temp_proto = savegame.flightState.protoVessels.Find(p => p.vesselID.ToString() == get_save_value("_SAVE_Recovered_Vessel"));
                if (temp_proto != null)
                    savegame.flightState.protoVessels.Remove(temp_proto);

                for (load_vessel = 0; load_vessel < savegame.flightState.protoVessels.Count && savegame.flightState.protoVessels[load_vessel].vesselID.ToString() != get_save_value("_SAVE_Main_Vessel"); load_vessel++) ;

                if (load_vessel < savegame.flightState.protoVessels.Count)
                    FlightDriver.StartAndFocusVessel(savegame, load_vessel);
            }

            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS_Space_Center: entering Start()");
        }


/*************************************************************************************************************************/
        void Update()
        {
            //nothing
        }


/*************************************************************************************************************************/
        void FixedUpdate()
        {
        }


/*************************************************************************************************************************/
        void OnDestroy()
        {
        }
    }
}
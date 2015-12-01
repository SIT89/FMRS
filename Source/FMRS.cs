/*
 * The MIT License (MIT)
 * 
 * Copyright (c) 2015 SIT89
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
        public ApplicationLauncherButton Stock_Toolbar_Button = new ApplicationLauncherButton();

/*************************************************************************************************************************/
        public FMRS()
        {
            //nothing
        }


/*************************************************************************************************************************/
        void Awake()
        {
            Debug.Log("#### FMRS: FMRS On Awake"); 
            
            FMRS_core_awake();

            stb_texture = new Texture2D(38, 38);
            stb_texture.LoadImage(System.IO.File.ReadAllBytes(Path.Combine(KSPUtil.ApplicationRootPath, "GameData/FMRS/icons/tb_st_di.png")));

            if (ApplicationLauncher.Ready == true)
            {
                add_toolbar_button();
            }

            Debug.Log("#### FMRS Version: " + mod_vers);
            
            _SAVE_SaveFolder = HighLogic.SaveFolder;
        }

        
/*************************************************************************************************************************/
        void Start()
        {
            RenderingManager.AddToPostDrawQueue(3, new Callback(drawGUI));     

            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: entering Start()");

            if (!_SAVE_Has_Launched)
                _SETTING_Enabled = false;

            if (FlightGlobals.ActiveVessel.situation == Vessel.Situations.PRELAUNCH)
            {
                if (Debug_Active)
                    Debug.Log("#### FMRS: ActiveVessel is prelaunch");
                _SETTING_Enabled = true;
                GameEvents.onLaunch.Add(launch_routine);
            }

            if (ToolbarManager.ToolbarAvailable)
            {
                Toolbar_Button = ToolbarManager.Instance.add("FMRS", "FMRSbutton");

                blz_toolbar_available = true;

                if (_SETTING_Enabled)
                    Toolbar_Button.TexturePath = "FMRS/icons/tb_blz_en";
                else
                    Toolbar_Button.TexturePath = "FMRS/icons/tb_blz_di";

                Toolbar_Button.ToolTip = "Flight Manager for Reusable Stages";
                Toolbar_Button.Visibility = new GameScenesVisibility(GameScenes.FLIGHT);
                Toolbar_Button.OnClick += (e) => toolbar_button_clicked();
            }

            if (_SETTING_Enabled)
            {
                flight_scene_start_routine();
                stb_texture.LoadImage(System.IO.File.ReadAllBytes(Path.Combine(KSPUtil.ApplicationRootPath, "GameData/FMRS/icons/tb_st_en.png")));
            }

            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: leaving Start ()");
        }


/*************************************************************************************************************************/
        void Update()
        {
            flight_scene_update_routine();

            if (ThrottleLogger != null)
                ThrottleLogger.Update(Debug_Active, Debug_Level_1_Active);

            if(ThrottleReplay != null)
                ThrottleReplay.Update(Debug_Active, Debug_Level_1_Active);
        }


/*************************************************************************************************************************/
        void FixedUpdate()
        {
            if (ThrottleLogger != null)
                ThrottleLogger.LogThrottle(FlightGlobals.ActiveVessel.ctrlState.mainThrottle);

            //fixed_update_routine();
        }


/*************************************************************************************************************************/
        void OnDestroy()
        {
            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: enter OnDestroy()");

            destroy_FMRS();

            if (ToolbarManager.ToolbarAvailable)
                Toolbar_Button.Destroy();

            GameEvents.onGUIApplicationLauncherReady.Remove(add_toolbar_button);
            ApplicationLauncher.Instance.RemoveModApplication(Stock_Toolbar_Button);

            RenderingManager.RemoveFromPostDrawQueue(3, new Callback(drawGUI));

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
                (Texture)stb_texture);
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
        private float delay = 35;


/*************************************************************************************************************************/
        public FMRS_Space_Center()
        {
            //nothing
        }


/*************************************************************************************************************************/
        void Awake()
        {
            FMRS_core_awake();

            Debug.Log("#### FMRS Version: " + mod_vers);

            _SAVE_SaveFolder = HighLogic.SaveFolder;

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
                    if (Debug_Active)
                        Debug.Log("#### FMRS: FMRS_Space_Center kick to main");

                    _SAVE_Kick_To_Main = false;
                    write_save_values_to_file();

                    savegame = GamePersistence.LoadGame("FMRS_main_save", HighLogic.SaveFolder, false, false);
                    temp_proto = savegame.flightState.protoVessels.Find(p => p.vesselID == _SAVE_Main_Vessel);

                    if (temp_proto != null)
                    {
                        for (load_vessel = 0; load_vessel < savegame.flightState.protoVessels.Count && savegame.flightState.protoVessels[load_vessel].vesselID.ToString() != temp_proto.vesselID.ToString(); load_vessel++) ;

                        if (load_vessel < savegame.flightState.protoVessels.Count)
                            FlightDriver.StartAndFocusVessel(savegame, load_vessel);
                    }

                    write_save_values_to_file();
                    write_recover_file();
                }
            }
        }


/*************************************************************************************************************************/
        void OnDestroy()
        {
        }
    }




/*************************************************************************************************************************/
/*************************************************************************************************************************/
/*************************************************************************************************************************/
/*************************************************************************************************************************/
/*************************************************************************************************************************/
    [KSPAddon(KSPAddon.Startup.TrackingStation, false)]
    public class FMRS_TrackingStation : FMRS_Core
    {
        private float delay = 35;


/*************************************************************************************************************************/
        public FMRS_TrackingStation()
        {
            //nothing
        }


/*************************************************************************************************************************/
        void Awake()
        {
            FMRS_core_awake();

            Debug.Log("#### FMRS Version: " + mod_vers);

            _SAVE_SaveFolder = HighLogic.SaveFolder;

            if (Debug_Active)
                Debug.Log("#### FMRS: TrackingStation On Awake");
        }


/*************************************************************************************************************************/
        void Start()
        {
            if (Debug_Active)
                Debug.Log("#### FMRS: TrackingStation On Start");
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
                    if (Debug_Active)
                        Debug.Log("#### FMRS: TrackingStation kick to main");

                    _SAVE_Kick_To_Main = false;
                    write_save_values_to_file();

                    savegame = GamePersistence.LoadGame("FMRS_main_save", HighLogic.SaveFolder, false, false);
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
            destroy_FMRS();
        }
    }




/*************************************************************************************************************************/
/*************************************************************************************************************************/
/*************************************************************************************************************************/
/*************************************************************************************************************************/
/*************************************************************************************************************************/
    [KSPAddon(KSPAddon.Startup.MainMenu, false)]
    public class FMRS_Main_Menu : FMRS_Core
    {
/*************************************************************************************************************************/
        public FMRS_Main_Menu()
        {
            //nothing
        }


/*************************************************************************************************************************/
        void Awake()
        {
            FMRS_core_awake();

            Debug.Log("#### FMRS Version: " + mod_vers);

            if (Debug_Active)
                Debug.Log("#### FMRS: FMRS_MainMenu On Awake");
        }


/*************************************************************************************************************************/
        void Start()
        {
            if (Debug_Active)
                Debug.Log("#### FMRS: FMRS_MainMenu On Start");

            if (_SETTING_Enabled)
            {
                _SETTING_Enabled = false;
                Debug.Log("#### FMRS: FMRS_MainMenu _SAVE_Has_Closed");
            }
            if (_SAVE_Has_Launched)
            {
                _SAVE_Has_Launched = false;
                Debug.Log("#### FMRS: FMRS_MainMenu _SAVE_Has_Launched");
            }
            if(_SAVE_Switched_To_Dropped)
            {
                Debug.Log("#### FMRS: FMRS_MainMenu _SAVE_Switched_To_Dropped");
                Game loadgame = GamePersistence.LoadGame("FMRS_main_save", _SAVE_SaveFolder, false, false);
                GamePersistence.SaveGame(loadgame, "persistent", _SAVE_SaveFolder, SaveMode.OVERWRITE);
                _SAVE_Switched_To_Dropped = false;
            }

            write_save_values_to_file();
            write_recover_file();
        }


/*************************************************************************************************************************/
        void Update()
        {
            //nothing
        }


/*************************************************************************************************************************/
        void FixedUpdate()
        {
            //nothing
        }


/*************************************************************************************************************************/
        void OnDestroy()
        {
            //nothing
        }
    }
}
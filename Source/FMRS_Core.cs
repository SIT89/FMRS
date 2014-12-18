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
using Contracts;


namespace FMRS
{
    public class FMRS_Core : FMRS_Util
    {
        public struct science_data_sent
        {
            public string id;
            public float amount;
        }
        public struct killed_kerbal_str
        {
            public string name;
            public float rep;
            public Guid vessel_id;
        }
        
        private string version_number = "0.3.01";
        public bool plugin_active = false;
        public double Time_Trigger_Staging, Time_Trigger_Start_Delay, Time_Trigger_Cuto;
        public bool timer_staging_active = false, timer_start_delay_active = false, timer_cuto_active = false;
        public double Timer_Stage_Delay = 0.2, Timer_Start_Delay = 2;
        public double last_staging_event = 0;
        public string quicksave_file_name;
        public Guid anz_id;
        //private bool lost_root_part = false;
        public bool separated_vessel, staged_vessel;
        public bool n_launchpad_preflight = false, flight_preflight = false;
        public bool main_ui_active = false;
        public bool revert_to_launch = false;
        public bool really_close = false;
        public bool show_setting = false;
        public bool blz_toolbar_available = false;
        public IButton Toolbar_Button;
        public Texture2D stb_texture;
        public bool can_restart, can_q_save_load;
        private int nr_save_files = 0;
        public Vector2 scroll_Vector = Vector2.zero;
        GUIStyle button_main, button_green, button_red, button_yellow, button_small, button_small_red, button_big;
        GUIStyle text_main, text_green, text_cyan, text_red, text_yellow, text_heading;
        GUIStyle area_style, scrollbar_stlye;
        private bool skin_init = false;
        private List<science_data_sent> science_sent = new List<science_data_sent>();
        private float current_rep, last_rep_change;
        private List<killed_kerbal_str> killed_kerbals = new List<killed_kerbal_str>();
        private Dictionary<Guid, List<Contract>> contract_complete = new Dictionary<Guid, List<Contract>>();
        private List<Guid> loaded_vessels = new List<Guid>();
        private List<Guid> damaged_vessels = new List<Guid>();
        private List<string> damaged_buildings = new List<string>();
        //private Dictionary<Guid, double> landing_time = new Dictionary<Guid, double>();
        public FMRS_THL.FMRS_THL_Log ThrottleLogger;
        public FMRS_THL.FMRS_THL_Rep ThrottleReplay;
        
#if DEBUG  //**************************
        public string[] debug_message = { "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", ""};
        public Rect debug_windowPos;
#endif //**************************

#if BETA //**************************
        string beta_version = ".01";
        public Rect beta_windowPos;
#endif   //**************************




/*************************************************************************************************************************/
        public void drawGUI()
        {
            if (!skin_init)
                init_skin();
            GUI.skin = HighLogic.Skin;

            if (main_ui_active)
            {
                windowPos = GUILayout.Window(1, windowPos, MainGUI, "FMRS " + mod_vers, GUILayout.MinWidth(100));
                windowPos.x = Mathf.Clamp(windowPos.x, 0, Screen.width - windowPos.width);
                windowPos.y = Mathf.Clamp(windowPos.y, 0, Screen.height - windowPos.height);

#if BETA //**************************
                beta_windowPos.x = windowPos.x;
                beta_windowPos.y = windowPos.y + windowPos.height;
                beta_windowPos =  GUILayout.Window(3,beta_windowPos, BetaGUI, "FMRS Beta");
#endif //**************************
            }

#if DEBUG //**************************
            if (main_ui_active)
            {
                debug_windowPos.x = windowPos.x;
                debug_windowPos.y = windowPos.y + windowPos.height;
            }
            debug_windowPos = GUILayout.Window(2, debug_windowPos, DebugGUI, "FMRS Debug Info");
#endif //**************************
        }


/*************************************************************************************************************************/
        public void MainGUI(int windowID)
        {      
            List<string> save_files = new List<string>();
            Vessel temp_vessel;
            string temp_string;
            Guid guid_delete_vessel = FlightGlobals.ActiveVessel.id;
            bool delete_vessel = false;
            float scrollbar_size;
            bool scrollbar_enable = false;
            float window_height, window_width, scrollbar_width_offset;

            window_height = 60;
            window_width = 280;

            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();

            if (!_SETTING_Minimize)
            {
                if (_SAVE_Has_Launched)
                    GUILayout.Box("Mission Time: " + get_time_string(Planetarium.GetUniversalTime() - _SAVE_Launched_At), text_main, GUILayout.Width(188));
                else
                    GUILayout.Box("Mission Time: " + "00:00", text_main, GUILayout.Width(137));
            }
            else
                GUILayout.Space(5);

            if (_SETTING_Armed)
                temp_string = "Armed";
            else
                temp_string = "Arm";
  
            if (!_SAVE_Has_Launched)
                _SETTING_Armed = GUILayout.Toggle(_SETTING_Armed, temp_string, button_small, GUILayout.Width(50));
            else
                if (_SETTING_Minimize)
                    GUILayout.Box("Flight", text_main, GUILayout.Width(50));

            if (!_SETTING_Minimize)
                show_setting = GUILayout.Toggle(show_setting, "s", button_small, GUILayout.Width(25));

            _SETTING_Minimize = GUILayout.Toggle(_SETTING_Minimize, "_", button_small, GUILayout.Width(25));
            if (really_close && _SETTING_Minimize)
                _SETTING_Minimize = false;

            if (!_SETTING_Minimize)
                really_close = GUILayout.Toggle(really_close, "x", button_small, GUILayout.Width(25));
            else
                window_width = 105;

            GUILayout.EndHorizontal();

            if (really_close)
            {
                if (_SAVE_Has_Launched)
                {
                    GUILayout.Space(5);
                    window_height += 5;

                    GUILayout.Box("Plugin will be reset!", text_heading, GUILayout.Width(266));
                    window_height += 29;
                    GUILayout.Box("Close?", text_heading, GUILayout.Width(266));
                    GUILayout.BeginHorizontal();

                    if (GUILayout.Button("YES", button_big, GUILayout.Width(132)))
                        close_FMRS();

                    if (GUILayout.Button("NO", button_big, GUILayout.Width(133)))
                        really_close = false;

                    GUILayout.EndHorizontal();
                    window_height += 58;
                }
                else
                    close_FMRS();
            }

            if (really_close == false && _SETTING_Minimize == false && show_setting == true)
            {
                GUILayout.Space(5);
                window_height += 5;
                GUILayout.BeginVertical(area_style, GUILayout.Width(266));
                GUILayout.Space((5 * 30) + 5);
                _SETTING_Messages = GUI.Toggle(new Rect(5, 35 + (30 * 1), 25, 25), _SETTING_Messages, "Messaging System");
                window_height += 30;
                _SETTING_Auto_Cut_Off = GUI.Toggle(new Rect(5, 35 + (30 *2), 25, 25), _SETTING_Auto_Cut_Off, "Auto Cut Off Engines");
                window_height += 30;
                _SETTING_Auto_Recover = GUI.Toggle(new Rect(5, 35 + (30 * 3), 25, 25), _SETTING_Auto_Recover, "Auto Recover Landed Crafts");
                window_height += 30;
                _SETTING_Throttle_Log = GUI.Toggle(new Rect(6, 35 + (30 * 4), 25, 25), _SETTING_Throttle_Log, "Throttle Logger WIP");
                window_height += 30;
                Debug_Active = GUI.Toggle(new Rect(5, 35 + (30 * 5), 25, 25), Debug_Active, "write debug messages to log file");
                GUILayout.EndVertical();
                window_height += 42;                
            }

            if (really_close == false && _SETTING_Minimize == false && show_setting == false)
            {
                if (Vessels_dropped.Count > 0)
                {
                    GUILayout.Space(5);
                    window_height += 5;

                    GUILayout.Box("Separated Stages:", text_heading, GUILayout.Width(266));
                    window_height += 33;

                    foreach (KeyValuePair<Guid, string> temp_keyvalue in Vessels_dropped)
                    {
                        if (!save_files.Contains(temp_keyvalue.Value))
                            save_files.Add(temp_keyvalue.Value);
                    }

                    save_files.Sort(delegate(string x, string y)
                    {
                        return get_save_value(save_cat.SAVEFILE, y).CompareTo(get_save_value(save_cat.SAVEFILE, x));
                    });

                    nr_save_files = save_files.Count;

                    scrollbar_size = nr_save_files * 61;
                    scrollbar_size += (Vessels_dropped.Count - nr_save_files) * 25;
                    if (_SAVE_Switched_To_Dropped && can_q_save_load)
                        scrollbar_size += 43;

                    if (scrollbar_size > 225)
                    {
                        scrollbar_enable = true;
                        scroll_Vector = GUILayout.BeginScrollView(scroll_Vector, scrollbar_stlye, GUILayout.Width(266), GUILayout.Height(225));
                        GUILayout.BeginVertical();
                        window_height += 220;
                        scrollbar_width_offset = 0;
                    }
                    else
                    {
                        GUILayout.BeginVertical(area_style, GUILayout.Width(266));
                        window_height += scrollbar_size;
                        scrollbar_width_offset = 20;
                    }

                    while (save_files.Count != 0)
                    {
                        GUILayout.Space(5);
                        GUILayout.BeginVertical(area_style);
                        if (save_files.Last().Contains("separated_"))
                            GUILayout.Box("Separated at " + get_time_string(Convert.ToDouble(get_save_value(save_cat.SAVEFILE, save_files.Last())) - _SAVE_Launched_At), text_main, GUILayout.Width(230 + scrollbar_width_offset));
                        else
                            GUILayout.Box("Stage " + save_files.Last().Substring(10) + " separated at " + get_time_string(Convert.ToDouble(get_save_value(save_cat.SAVEFILE, save_files.Last())) - _SAVE_Launched_At), text_main, GUILayout.Width(230 + scrollbar_width_offset));

                        foreach (KeyValuePair<Guid, string> vessel_in_savefile in Vessels_dropped)
                        {
                            if (vessel_in_savefile.Value == save_files.Last())
                            {
                                GUILayout.BeginHorizontal();
                                if (get_vessel_state(vessel_in_savefile.Key) == vesselstate.RECOVERED)
                                {
                                    GUILayout.Box(Vessels_dropped_names[vessel_in_savefile.Key] + " recovered", text_cyan, GUILayout.Width(205 + scrollbar_width_offset));
                                }
                                else if (get_vessel_state(vessel_in_savefile.Key) == vesselstate.LANDED)
                                {
                                    GUILayout.Box(Vessels_dropped_names[vessel_in_savefile.Key] + " landed", text_green, GUILayout.Width(205 + scrollbar_width_offset));
                                }
                                else if (vessel_in_savefile.Key == FlightGlobals.ActiveVessel.id || vessel_in_savefile.Key == anz_id)
                                {
                                    float temp_float = 230 + scrollbar_width_offset;
                                    if (can_q_save_load)
                                    {
                                        GUILayout.EndHorizontal();
                                        GUILayout.Space(5);
                                        GUILayout.BeginVertical(area_style, GUILayout.Width(230));
                                        temp_float = 222 + scrollbar_width_offset;
                                    }
                                    if (FlightGlobals.ActiveVessel.LandedOrSplashed)
                                    {
                                        GUILayout.Box(Vessels_dropped_names[vessel_in_savefile.Key] + " landed", text_green, GUILayout.Width(temp_float));
                                    }
                                    else
                                    {
                                        GUILayout.Box("contr.: " + Vessels_dropped_names[vessel_in_savefile.Key], text_yellow, GUILayout.Width(temp_float));
                                    }
                                    if (can_q_save_load)
                                        if (GUILayout.Button("Jump back to Separation", button_main, GUILayout.Width(222 + scrollbar_width_offset)))
                                            jump_to_vessel(vessel_in_savefile.Key, false);

                                    if (can_q_save_load)
                                    {
                                        GUILayout.EndVertical();
                                        GUILayout.Space(5);
                                        GUILayout.BeginHorizontal();
                                    }
                                }
                                else if (get_vessel_state(vessel_in_savefile.Key) == vesselstate.DESTROYED)
                                {
                                    GUILayout.Box(Vessels_dropped_names[vessel_in_savefile.Key] + " destroyed", text_red, GUILayout.Width(205 + scrollbar_width_offset));
                                }
                                else
                                {
                                    temp_vessel = FlightGlobals.Vessels.Find(p => p.id == vessel_in_savefile.Key);

                                    if (temp_vessel == null)
                                    {
                                        if (GUILayout.Button(Vessels_dropped_names[vessel_in_savefile.Key], button_main, GUILayout.Width(205 + scrollbar_width_offset)))
                                            jump_to_vessel(vessel_in_savefile.Key, true);
                                    }
                                    else
                                    {
                                        if (loaded_vessels.Contains(temp_vessel.id) && _SAVE_Switched_To_Dropped)
                                        {
                                            if (temp_vessel.LandedOrSplashed)
                                            {
                                                if (GUILayout.Button(Vessels_dropped_names[vessel_in_savefile.Key] + " landed", button_green, GUILayout.Width(205 + scrollbar_width_offset)))
                                                    FlightGlobals.ForceSetActiveVessel(temp_vessel);
                                            }
                                            else
                                            {
                                                if (GUILayout.Button(Vessels_dropped_names[vessel_in_savefile.Key] + " is near", button_yellow, GUILayout.Width(205 + scrollbar_width_offset)))
                                                    FlightGlobals.ForceSetActiveVessel(temp_vessel);
                                            }
                                        }
                                        else
                                        {
                                            if (GUILayout.Button(Vessels_dropped_names[vessel_in_savefile.Key], button_main, GUILayout.Width(205 + scrollbar_width_offset)))
                                                jump_to_vessel(vessel_in_savefile.Key, true);
                                        }
                                    }
                                }

                                if (vessel_in_savefile.Key != FlightGlobals.ActiveVessel.id && vessel_in_savefile.Key != anz_id)
                                {
                                    if (GUILayout.Button("X", button_small_red, GUILayout.Width(25)))
                                    {
                                        guid_delete_vessel = vessel_in_savefile.Key;
                                        delete_vessel = true;
                                    }
                                }
                                GUILayout.EndHorizontal();
                                button_main.normal.textColor = button_main.focused.textColor = Color.white;
                            }
                        }
                        GUILayout.EndVertical();
                        temp_string = save_files.Last();
                        save_files.Remove(temp_string);
                    }

                    if (scrollbar_enable)
                    {
                        GUILayout.EndVertical();
                        GUILayout.EndScrollView();
                    }
                    else
                        GUILayout.EndVertical();
                }

                if (_SAVE_Switched_To_Dropped)
                {
                    GUILayout.Space(5);
                    window_height += 5;
                    if (GUILayout.Button("Jump back to Main Mission", button_big, GUILayout.Width(266)))
                    {
                        jump_to_vessel("Main");
                    }
                    window_height += 31;
                }

                if (_SAVE_Has_Launched && can_restart)
                {
                    GUILayout.Space(5);
                    window_height += 5;

                    if (revert_to_launch)
                    { 
                        GUILayout.Box("Revert Flight?", text_heading, GUILayout.Width(266));

                        GUILayout.BeginHorizontal();

                        if (GUILayout.Button("YES", button_big, GUILayout.Width(132)))
                        {
                            _SETTING_Enabled = false;
                            jump_to_vessel(_SAVE_Main_Vessel, "before_launch");
                        }
                        if (GUILayout.Button("NO", button_big, GUILayout.Width(133)))
                            revert_to_launch = false;

                        GUILayout.EndHorizontal();
                        window_height += 58;
                    }
                    else
                    {
                        if (_SAVE_Flight_Reset)
                            revert_to_launch = GUILayout.Toggle(revert_to_launch, "Revert To Plugin Start", button_big, GUILayout.Width(266));
                        else
                            revert_to_launch = GUILayout.Toggle(revert_to_launch, "Revert To Launch", button_big, GUILayout.Width(266));
                        window_height += 31;
                    }
                }
            }
            GUILayout.EndVertical();

            if(delete_vessel && guid_delete_vessel != FlightGlobals.ActiveVessel.id)
                delete_dropped_vessel(guid_delete_vessel);

            windowPos.height = window_height;
            windowPos.width = window_width;

            GUI.DragWindow(new Rect(0, 0, 10000, 20));
        }


#if DEBUG
/*************************************************************************************************************************/
        public void DebugGUI(int windowID)
        {
            GUILayout.BeginVertical();

            GUILayout.Space(10);
            GUI.Toggle(new Rect(5, 3, 25, 25), plugin_active, " ");

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("print savefile", button_small, GUILayout.Width(132)))
                write_save_values_to_file();
            if (GUILayout.Button("read savefile", button_small, GUILayout.Width(132)))
                load_save_file();
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            Debug_Level_1_Active = GUILayout.Toggle(Debug_Level_1_Active, "debug lv 1", button_small, GUILayout.Width(132));
            Debug_Level_2_Active = GUILayout.Toggle(Debug_Level_2_Active, "debug lv 2", button_small, GUILayout.Width(132));
            GUILayout.EndHorizontal();
            GUILayout.Space(5);

            debug_message[0] = loaded_vessels.Count().ToString();
            //debug_message[1] = "";
            //debug_message[2] = "";
            //debug_message[3] = "";
            //debug_message[4] = "";
            //debug_message[5] = "";
            //debug_message[6] = "";
            //debug_message[7] = "";
            //debug_message[8] = "";
            //debug_message[9] = "";
            //debug_message[10] = "";
            //debug_message[11] = "";
            //debug_message[12] = "";
            //debug_message[13] = "";
            //debug_message[14] = "";
            //debug_message[15] = "";
            //debug_message[16] = "";
            //debug_message[17] = "";
            //debug_message[18] = "";
            //debug_message[19] = "";

            foreach (string temp_string in debug_message)
                if (temp_string != "")
                    GUILayout.Box(temp_string, text_main, GUILayout.Width(266));

            GUILayout.EndVertical();
            GUI.DragWindow(new Rect(0, 0, 10000, 20));
        }
#endif



#if BETA
/*************************************************************************************************************************/
        public void BetaGUI(int windowID)
        {
            GUILayout.BeginVertical();

            if (GUILayout.Button("mark bug", button_small, GUILayout.Width(115)))
                Debug.Log("#### FMRS: ##################### BUG MARKER #####################");

            GUILayout.EndVertical();
            GUI.DragWindow(new Rect(0, 0, 10000, 20));
        }
#endif


/*************************************************************************************************************************/
        public void staging_routine(EventReport event_input)
        {
            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: entering staging_routine(EventReport event_input) " + event_input.sender);

            Time_Trigger_Staging = Planetarium.GetUniversalTime();
            timer_staging_active = true;
            staged_vessel = true;
            separated_vessel = false;
            timer_cuto_active = true;
            Time_Trigger_Cuto = Planetarium.GetUniversalTime();

            if (Debug_Active)
                Debug.Log("#### FMRS: Has Staged");
            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: leaving staging_routine(EventReport event_imput)");
        }


/*************************************************************************************************************************/
        public void launch_routine(EventReport event_input)
        {
            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: entering launch_routine(EventReport event_imput) " + event_input.sender);
            if (Debug_Active)
                Debug.Log("#### FMRS: LAUNCH");

            if (!_SETTING_Armed)
            {
                close_FMRS();
                return;
            }

            if (!_SAVE_Has_Launched || flight_preflight)
            {
                _SAVE_Has_Launched = true;
                _SAVE_Launched_At = Planetarium.GetUniversalTime();
                fill_Vessels_list();
                GameEvents.onStageSeparation.Add(staging_routine);
                GameEvents.onVesselCreate.Add(vessel_create_routine);
            }

            can_restart = HighLogic.CurrentGame.Parameters.Flight.CanRestart;
            if (HighLogic.CurrentGame.Parameters.Flight.CanQuickLoad && HighLogic.CurrentGame.Parameters.Flight.CanQuickSave)
                can_q_save_load = true;
            else
                can_q_save_load = false;

            if (_SETTING_Throttle_Log)
            {
                ThrottleLogger = new FMRS_THL.FMRS_THL_Log(Debug_Active, Debug_Level_1_Active);
                ThrottleLogger.flush_record_file();
                ThrottleLogger.StartLog();
            }

            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: leaving launch_routine(EventReport event_imput)");
        }


/*************************************************************************************************************************/
        public void fill_Vessels_list()
        {
            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: entering fill_Vessels_list()");

            foreach (Vessel temp_vessel in FlightGlobals.Vessels)
            {
                if (!Vessels.Contains(temp_vessel.id))
                {
                    Vessels.Add(temp_vessel.id);
                    if (Debug_Active)
                        Debug.Log("#### FMRS: " + temp_vessel.vesselName + " Found");
                }
            }

            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: leaving fill_Vessels_list()");
        }


/*************************************************************************************************************************/
        public bool search_for_new_vessels(string save_file_name)
        {
            bool new_vessel_found = false, controllable = false;
            
            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: entering search_for_new_vessels(string save_file_name) " + save_file_name);

            foreach (Vessel temp_vessel in FlightGlobals.Vessels)
            {
                controllable = false;

                if (!Vessels.Contains(temp_vessel.id))
                {
                    if (temp_vessel.isCommandable &&
                        temp_vessel.IsControllable &&
                        temp_vessel.vesselType != VesselType.EVA &&
                        temp_vessel.vesselType != VesselType.Flag &&
                        temp_vessel.vesselType != VesselType.SpaceObject &&
                        temp_vessel.vesselType != VesselType.Unknown)

                        controllable = true;
                    else
                    {
                        foreach (ProtoPartSnapshot proto_part in temp_vessel.protoVessel.protoPartSnapshots)
                        {
                            List<ProtoPartModuleSnapshot> proto_modules = proto_part.modules;
                            ProtoPartModuleSnapshot module = null;

                            if (proto_modules != null)
                            {
                                module = proto_part.modules.Find(p => p.moduleName == "RealChuteModule");
                                if (module != null)
                                    controllable = true;
                            }

                            if (proto_part.protoCrewNames.Count > 0)
                                controllable = true;
                        }
                    }
                    foreach(Part p in temp_vessel.Parts)
                    {
                        foreach(PartModule pm in p.Modules)
                        {
                            if (pm.moduleName == "FMRS_PM")
                            {
                                if((pm as FMRS_PM).parent_vessel != "00000000-0000-0000-0000-000000000000")
                                {
                                    controllable = false;
                                    break;
                                }
                            }
                        }
                        break;
                    }

                    if(controllable)
                    {
                        if (Debug_Active)
                            Debug.Log("#### FMRS: " + temp_vessel.vesselName + " Found and will be added to the dicts");

                        Vessels_dropped.Add(temp_vessel.id, save_file_name);
                        Vessels_dropped_names.Add(temp_vessel.id, temp_vessel.vesselName);
                        Vessel_State.Add(temp_vessel.id, vesselstate.FLY);
                        foreach (Part p in temp_vessel.Parts)
                        {
                            foreach (PartModule pm in p.Modules)
                                if (pm.moduleName == "FMRS_PM")
                                    pm.StartCoroutine("setid");
                        }              
  
                        foreach (ProtoPartSnapshot part_snapshot in temp_vessel.protoVessel.protoPartSnapshots)
                        {
                            foreach (ProtoCrewMember member in part_snapshot.protoModuleCrew)
                            {
                                if (!Kerbal_dropped.ContainsKey(member.name))
                                    Kerbal_dropped.Add(member.name, temp_vessel.id);
                            }                       
                        }
                        new_vessel_found = true;
                    }
                    Vessels.Add(temp_vessel.id);
                }
            }

            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: leaving search_for_new_vessels(string save_file_name)");

            return (new_vessel_found);
        }


/*************************************************************************************************************************/
        public List<ProtoVessel> search_for_new_vessels(Game loadgame, Game savegame)
        {
            List<ProtoVessel> return_list = new List<ProtoVessel>();

            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: entering List<Guid> search_for_new_vessels(Game loadgame, Game savegame)");

            foreach (ProtoVessel vessel_load in loadgame.flightState.protoVessels)
            {
                if (vessel_load.landed || vessel_load.splashed)

                    if (savegame.flightState.protoVessels.Find(v => v.vesselID == vessel_load.vesselID) == null)
                    {
                        return_list.Add(vessel_load);
                        if (Debug_Active)
                            Debug.Log("#### FMRS: " + vessel_load.vesselName + " Found and added to list");
                    }
            }

            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: leaving List<Guid> search_for_new_vessels(Game loadgame, Game savegame)");

            return (return_list);
        }


/*************************************************************************************************************************/
        public void jump_to_vessel(Guid vessel_id, bool save_landed)
        {
            int load_vessel;

            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: entering jump_to_vessel(Guid vessel_id) " + vessel_id.ToString() + " " + save_landed.ToString());

            if (Debug_Active)
                Debug.Log("#### FMRS: Jump to " + vessel_id.ToString());

            if (save_landed)
            {
                if (FlightGlobals.ActiveVessel.id == _SAVE_Main_Vessel)
                {
                    GamePersistence.SaveGame("FMRS_main_save", HighLogic.SaveFolder , SaveMode.OVERWRITE);
                }
                save_landed_vessel(true);
            }

            Game loadgame = GamePersistence.LoadGame(get_save_value(save_cat.DROPPED, vessel_id.ToString()), HighLogic.SaveFolder + "/FMRS", false, false);

            if (loadgame != null && loadgame.compatible && loadgame.flightState != null)
            {
                if (Debug_Active)
                    Debug.Log("#### FMRS: try to load gamefile " + get_save_value(save_cat.DROPPED, vessel_id.ToString()));

                for (load_vessel = 0; load_vessel < loadgame.flightState.protoVessels.Count && loadgame.flightState.protoVessels[load_vessel].vesselID != vessel_id; load_vessel++) ;
                if (load_vessel <= loadgame.flightState.protoVessels.Count)
                {
                    if (Debug_Active)
                        Debug.Log("#### FMRS: FMRS_save found, Vessel found, try to start");

                    if (vessel_id != _SAVE_Main_Vessel)
                    {
                        _SAVE_Switched_To_Savefile = get_save_value(save_cat.DROPPED, vessel_id.ToString());
                        _SAVE_Switched_To_Dropped = true;
                    }
                    else
                        _SAVE_Switched_To_Dropped = false;
                    FlightDriver.StartAndFocusVessel(loadgame, load_vessel);
                }
            }

            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: leaving jump_to_vessel(Guid vessel_id)");
        }


/*************************************************************************************************************************/
        public void jump_to_vessel(string main)
        {
            Game loadgame;
            int load_vessel;

            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: entering jump_to_vessel(string main)");
            if (Debug_Active)
                Debug.Log("#### FMRS: Jump to Main");

            if (!_SAVE_Switched_To_Dropped)
                return;

            save_landed_vessel(true);

            loadgame = GamePersistence.LoadGame("FMRS_main_save", HighLogic.SaveFolder , false, false);

            if (loadgame != null && loadgame.compatible && loadgame.flightState != null)
            {
                if (Debug_Active)
                    Debug.Log("#### FMRS: try to load gamefile FMRS_main_save");

                for (load_vessel = 0; load_vessel <= loadgame.flightState.protoVessels.Count && loadgame.flightState.protoVessels[load_vessel].vesselID != _SAVE_Main_Vessel; load_vessel++) ;
                if (load_vessel <= loadgame.flightState.protoVessels.Count)
                {
                    if (Debug_Active)
                        Debug.Log("#### FMRS: FMRS_main_save found, main vessel found, try to load");

                    _SAVE_Switched_To_Savefile = "FMRS_main_save";
                    _SAVE_Switched_To_Dropped = false;
                    FlightDriver.StartAndFocusVessel(loadgame, load_vessel);
                }
                else
                    if (Debug_Active)
                        Debug.Log("#### FMRS: main vessel not found in savefile");
            }
            else
                if (Debug_Active)
                    Debug.Log("#### FMRS: Loading gamefile FMRS_main_save failed");

            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: leaving jump_to_vessel(string main)");
        }


/*************************************************************************************************************************/
        public void jump_to_vessel(Guid vessel_id, string save_file)
        {
            Game loadgame;
            int load_vessel;

            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: entering jump_to_vessel(string vessel_id,string save_file " + vessel_id.ToString() + " " + save_file);
            if (Debug_Active)
                Debug.Log("#### FMRS: Jump to " + save_file);

            loadgame = GamePersistence.LoadGame(save_file, HighLogic.SaveFolder + "/FMRS", false, false);

            if (loadgame != null && loadgame.compatible && loadgame.flightState != null)
            {
                if (Debug_Active)
                    Debug.Log("#### FMRS: try to load gamefile " + save_file);

                for (load_vessel = 0; load_vessel < loadgame.flightState.protoVessels.Count && loadgame.flightState.protoVessels[load_vessel].vesselID != vessel_id; load_vessel++);
                if (load_vessel <= loadgame.flightState.protoVessels.Count)
                {
                    if (vessel_id != _SAVE_Main_Vessel)
                    {
                        _SAVE_Switched_To_Savefile = save_file;
                        _SAVE_Switched_To_Dropped = true;
                    }
                    else
                        _SAVE_Switched_To_Dropped = false;
                    FlightDriver.StartAndFocusVessel(loadgame, load_vessel);
                }
            }
            else
                if (Debug_Active)
                    Debug.Log("#### FMRS: Loading gamefile " + save_file + " failed");

            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: leaving jump_to_vessel(string vessel_id,string save_file)");
        }


/*************************************************************************************************************************/
        public void save_landed_vessel(bool auto_recover_allowed)
        {
            Game loadgame, savegame;
            Guid temp_guid;
            int ReferenceBodyIndex = 0;
            List<Guid> id_list = new List<Guid>();
            List<ProtoVessel> vessel_list = new List<ProtoVessel>();
            Dictionary<Guid, List<ProtoVessel>> vessel_dict = new Dictionary<Guid, List<ProtoVessel>>();
            string message;

            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: entering save_landed_vessel(bool auto_recover_allowed) " + auto_recover_allowed.ToString());
            if (Debug_Active)
                Debug.Log("#### FMRS: save landed vessels");

            GamePersistence.SaveGame("FMRS_quicksave", HighLogic.SaveFolder + "/FMRS", SaveMode.OVERWRITE);
            loadgame = GamePersistence.LoadGame("FMRS_quicksave", HighLogic.SaveFolder + "/FMRS", false, false);
            savegame = GamePersistence.LoadGame("FMRS_main_save", HighLogic.SaveFolder , false, false);

            foreach (Guid id in loaded_vessels)
            {
                if (FlightGlobals.Vessels.Find(v => v.id == id) == null)
                    if (!damaged_vessels.Contains(id))
                        damaged_vessels.Add(id);
            }

            foreach (Vessel v in FlightGlobals.Vessels.FindAll(v => v.loaded))
                id_list.Add(v.id);

            vessel_list = loadgame.flightState.protoVessels.FindAll(tpv => id_list.Contains(tpv.vesselID));

            foreach (Guid id in loaded_vessels)
                vessel_dict.Add(id, new List<ProtoVessel>());

            foreach (ProtoVessel pv in vessel_list)
            {
                if (loaded_vessels.Contains(pv.vesselID))
                    vessel_dict[pv.vesselID].Add(pv);
                else
                {
                    foreach (ProtoPartSnapshot ps in pv.protoPartSnapshots)
                    {
                        foreach (ProtoPartModuleSnapshot ppms in ps.modules)
                        {
                            if (ppms.moduleName == "FMRS_PM")
                            {
                                try
                                {
                                    temp_guid = new Guid(ppms.moduleValues.GetValue("parent_vessel"));
                                    if (loaded_vessels.Contains(temp_guid))
                                        vessel_dict[temp_guid].Add(pv);
                                }
                                catch (Exception)
                                {
                                    Debug.Log("#### FMRS: Exception: save_landed_vessel: temp_guid = new Guid(ppms.moduleValues.GetValue(parent_vessel));");
                                }

                            }
                            break;
                        }
                    }
                }
                ReferenceBodyIndex = pv.orbitSnapShot.ReferenceBodyIndex;
            }

            id_list.Clear();
            foreach (KeyValuePair<Guid, List<ProtoVessel>> kvp in vessel_dict)
                id_list.Add(kvp.Key);
            foreach(Guid id in id_list)
                if (vessel_dict[id].Count == 0)
                    vessel_dict.Remove(id);
            id_list.Clear();
            foreach (KeyValuePair<Guid, List<ProtoVessel>> kvp in vessel_dict)
                id_list.Add(kvp.Key);

            foreach (Guid id in id_list)
            {
                if (id_list.Count == 1 && vessel_dict[id].Count == 0 && get_vessel_state(id) != vesselstate.RECOVERED)
                    set_vessel_state(id, vesselstate.DESTROYED);

                if (get_vessel_state(id) == vesselstate.RECOVERED)
                    vessel_dict.Remove(id);
            }

            if (vessel_dict.Count != 0)
            {
                if (Debug_Active)
                    Debug.Log("#### FMRS: save landed vessel or recover");

                foreach (KeyValuePair<Guid, List<ProtoVessel>> kvp in vessel_dict)
                {
                    if (_SETTING_Auto_Recover && auto_recover_allowed && ReferenceBodyIndex == 1)
                        savegame = recover_vessel(kvp.Key, kvp.Value, loadgame, savegame);
                    else
                    {
                        ProtoVessel temp_proto_del2 = savegame.flightState.protoVessels.Find(prtv => prtv.vesselID == kvp.Key);
                        if (temp_proto_del2 != null)
                            savegame.flightState.protoVessels.Remove(temp_proto_del2);

                        foreach (ProtoVessel pv in kvp.Value)
                        {
                            ProtoVessel temp_proto_del = savegame.flightState.protoVessels.Find(prtv => prtv.vesselID == pv.vesselID);
                            if (temp_proto_del != null)
                                savegame.flightState.protoVessels.Remove(temp_proto_del);

                            if (pv.landed || pv.splashed)
                            {    
                                savegame.flightState.protoVessels.Add(pv);
                                set_vessel_state(kvp.Key, vesselstate.LANDED);
                            }
                        }     
                    }
                }
            }

            if (HighLogic.CurrentGame.Mode == Game.Modes.CAREER)
            {
                message = "";
                if (contract_complete.Count > 0)
                {
                    foreach (KeyValuePair<Guid, List<Contract>> kvp in contract_complete)
                    {
                        if (loaded_vessels.Contains(kvp.Key))
                        {
                            foreach (Contract c in kvp.Value)
                            {
                                set_recoverd_value("contract", "complete", c.ContractID.ToString());
                                message += c.Title + "@";
                            }
                        }
                    }
                    if (message != "")
                        set_recoverd_value("message", "FMRS: Completed Contracts", message);
                }
                foreach (KeyValuePair<Guid, List<ProtoVessel>> kvp in vessel_dict)
                {
                    if (contract_complete.ContainsKey(kvp.Key))
                        contract_complete.Remove(kvp.Key);
                }
            }

            foreach (killed_kerbal_str killed in killed_kerbals)
            {
                if (Debug_Active)
                    Debug.Log("#### FMRS: Kerbal " + killed.name + " killed on that flight?");

                message = "";

                if (loaded_vessels.Contains(killed.vessel_id))
                {
                    if (Debug_Active)
                        Debug.Log("#### FMRS: Kerbal " + killed.name + " killed");

                    foreach (ProtoCrewMember member in savegame.CrewRoster.Crew)
                    {
                        if (member.name == killed.name)
                        {
                            member.rosterStatus = ProtoCrewMember.RosterStatus.Dead;
                            set_recoverd_value("kerbal", "kill", killed.rep.ToString());
                            message += killed.name + " killed: " + Math.Round(killed.rep, 2).ToString() + " Reputation@";
                        }
                    }
                }
                if (message != "")
                    set_recoverd_value("message", "FMRS: Kerbal Killed", message);
            }

            if (damaged_buildings.Count > 0)
            {
                message = "";
                foreach (string build in damaged_buildings)
                {
                    set_recoverd_value("building", "destroyed", build);
                    message += build + "@";
                }
                if(message!="")
                    set_recoverd_value("message", "FMRS: Building Destroyed", message);
            }

            GamePersistence.SaveGame(savegame, "FMRS_main_save", HighLogic.SaveFolder , SaveMode.OVERWRITE);
            GamePersistence.SaveGame(savegame, "persistent", HighLogic.SaveFolder, SaveMode.OVERWRITE);

            write_recover_file();
            write_save_values_to_file();

            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: leaving save_landed_vessel(bool auto_recover_allowed)");
        }


/*************************************************************************************************************************/
        public void crash_handler(EventReport report)
        {
            List<Guid> new_vessels = new List<Guid>();

            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: enter crash_handler (EventReport report) " + report.sender);
            if (Debug_Active)
                Debug.Log("#### FMRS: crash detected");

            /*if (FlightGlobals.ActiveVessel.state == Vessel.State.DEAD && !lost_root_part)
            {
                Debug.Log("#### FMRS: lost root part");

                lost_root_part = true;
                anz_id = FlightGlobals.ActiveVessel.id;
            }*/

            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: leave crash_handler (EventReport report)");
        }


/*************************************************************************************************************************/
        public void vessel_change_handler(Vessel change_vessel)
        {
            if (Debug_Active)
                Debug.Log(" #### FMRS: changed to " + FlightGlobals.ActiveVessel.vesselName);

            if(!_SAVE_Switched_To_Dropped)
            {
                if (last_staging_event < Planetarium.GetUniversalTime() + 10 && last_staging_event != 0)
                {
                    if (FlightGlobals.ActiveVessel.id != _SAVE_Main_Vessel && Vessels_dropped.ContainsKey(FlightGlobals.ActiveVessel.id))
                    {
                        Vessel temp_vessel = FlightGlobals.Vessels.Find(v => v.id == _SAVE_Main_Vessel);
                        if (temp_vessel != null)
                        {
                            if (Vessels_dropped.ContainsKey(_SAVE_Main_Vessel))
                                Vessels_dropped.Remove(_SAVE_Main_Vessel);
                            if (Vessel_State.ContainsKey(_SAVE_Main_Vessel))
                                Vessel_State.Remove(_SAVE_Main_Vessel);
                            if (Vessels_dropped_names.ContainsKey(_SAVE_Main_Vessel))
                                Vessels_dropped_names.Remove(_SAVE_Main_Vessel);
                            if (Vessel_State.ContainsKey(_SAVE_Main_Vessel))
                                Vessel_State.Remove(_SAVE_Main_Vessel);

                            Vessels_dropped.Add(_SAVE_Main_Vessel, Vessels_dropped[FlightGlobals.ActiveVessel.id]);
                            Vessel_State.Add(_SAVE_Main_Vessel, vesselstate.FLY);
                            Vessels_dropped_names.Add(_SAVE_Main_Vessel, temp_vessel.vesselName);

                            foreach (ProtoCrewMember crew_member in temp_vessel.protoVessel.GetVesselCrew())
                                Kerbal_dropped.Add(crew_member.name, _SAVE_Main_Vessel);

                            if (Vessels_dropped.ContainsKey(FlightGlobals.ActiveVessel.id))
                                Vessels_dropped.Remove(FlightGlobals.ActiveVessel.id);
                            if (Vessels_dropped_names.ContainsKey(FlightGlobals.ActiveVessel.id))
                                Vessels_dropped_names.Remove(FlightGlobals.ActiveVessel.id);
                            if (Vessel_State.ContainsKey(FlightGlobals.ActiveVessel.id))
                                Vessel_State.Remove(FlightGlobals.ActiveVessel.id);

                            if (Kerbal_dropped.ContainsValue(FlightGlobals.ActiveVessel.id))
                            {
                                List<string> kerbals = new List<string>();
                                foreach (KeyValuePair<string, Guid> kerbal in Kerbal_dropped)
                                    if (kerbal.Value == FlightGlobals.ActiveVessel.id)
                                        kerbals.Add(kerbal.Key);
                                foreach (string kerbal in kerbals)
                                    Kerbal_dropped.Remove(kerbal);
                            }

                            _SAVE_Main_Vessel = FlightGlobals.ActiveVessel.id;
                        }
                    }
                }
                else
                {
                    if (!Vessels_dropped.ContainsKey(change_vessel.id))
                        close_FMRS();
                }

            }
            else
            {
                if (!Vessels_dropped.ContainsKey(FlightGlobals.ActiveVessel.id))
                {
                    foreach (Part p in FlightGlobals.ActiveVessel.Parts)
                    {
                        foreach (PartModule pm in p.Modules)
                        {
                            if (pm.moduleName == "FMRS_PM")
                            {
                                Guid temp_guid = new Guid("00000000-0000-0000-0000-000000000000");

                                try { temp_guid = new Guid((pm as FMRS_PM).parent_vessel); }
                                catch (Exception) { }

                                if (Vessels_dropped.ContainsKey(temp_guid))
                                    if (Vessels_dropped[temp_guid] == _SAVE_Switched_To_Savefile)
                                        anz_id = temp_guid;
                            }
                        }
                        break;
                    }
                }
            }
        }


/*************************************************************************************************************************/
        public void destroy_FMRS()
        {
            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: enter destroy_FMRS()");

            plugin_active = false;

            if (!_SETTING_Enabled)
                _SAVE_Has_Launched = false;

            if (ThrottleLogger != null)
                ThrottleLogger.EndLog();

            if (ThrottleReplay != null)
                ThrottleReplay.EndReplay();

            write_save_values_to_file();
            write_recover_file();

            if (main_ui_active)
            {
                main_ui_active = false;
                if (Debug_Active)
                    Debug.Log("#### FMRS: close drawMainGUI");
            }

            GameEvents.onStageSeparation.Remove(staging_routine);
            GameEvents.onLaunch.Remove(launch_routine);
            GameEvents.onCollision.Remove(crash_handler);
            GameEvents.onCrash.Remove(crash_handler);
            GameEvents.onCrashSplashdown.Remove(crash_handler);
            GameEvents.onVesselChange.Remove(vessel_change_handler);
            GameEvents.OnVesselRecoveryRequested.Remove(recovery_requested_handler);
            GameEvents.onGameSceneLoadRequested.Remove(scene_change_handler);
            GameEvents.onVesselCreate.Remove(vessel_create_routine);
            GameEvents.Contract.onCompleted.Remove(contract_routine);
            GameEvents.OnScienceRecieved.Remove(science_sent_routine);
            GameEvents.onCrewKilled.Remove(crew_killed_handler);
            GameEvents.OnReputationChanged.Remove(rep_changed);
            GameEvents.onVesselGoOnRails.Remove(vessel_on_rails);
            GameEvents.OnKSCStructureCollapsing.Remove(building_destroyed);
            GameEvents.onVesselGoOffRails.Remove(vessel_off_rails);
            GameEvents.onVesselSituationChange.Remove(vessel_state_changed);
            
            n_launchpad_preflight = false;

            if (ThrottleLogger != null)
                ThrottleLogger.EndLog();

            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: leave destroy_FMRS()");
        }


/*************************************************************************************************************************/
        public void recovery_requested_handler(Vessel vessel_recovered)
        {
            Guid parent_id;
            List<Guid> id_list = new List<Guid>();
            List<ProtoVessel> temp_vessel_list = new List<ProtoVessel>();
            List<ProtoVessel> vessel_list = new List<ProtoVessel>();

            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: enter recovery_requested_handler(Vessel input) " + vessel_recovered.vesselName);
            if (Debug_Active)
                Debug.Log("#### FMRS: Vessel recovery_requested_handler");

            if (vessel_recovered.id == _SAVE_Main_Vessel)
            {
                if (Debug_Active)
                    Debug.Log("#### FMRS: Vessel is main Vessel disable plugin");
                _SETTING_Enabled = false;
                return;
            }

            parent_id = vessel_recovered.id;

            if (!loaded_vessels.Contains(parent_id))
            {
                foreach (ProtoPartModuleSnapshot ppms in Enumerable.First<ProtoPartSnapshot>(vessel_recovered.protoVessel.protoPartSnapshots).modules)
                    if (ppms.moduleName == "FMRS_PM")
                        parent_id = new Guid(ppms.moduleValues.GetValue("parent_vessel"));
            }
            if (loaded_vessels.Contains(parent_id))
            {
                if (get_vessel_state(parent_id) != vesselstate.RECOVERED)
                {
                    if (HighLogic.CurrentGame.Mode == Game.Modes.CAREER || HighLogic.CurrentGame.Mode == Game.Modes.SCIENCE_SANDBOX)
                    {
                        GamePersistence.SaveGame("recover_save", HighLogic.SaveFolder + "/FMRS", SaveMode.OVERWRITE);
                        Game recover_save = GamePersistence.LoadGame("recover_save", HighLogic.SaveFolder + "/FMRS", false, false);
                        Game savegame = GamePersistence.LoadGame("FMRS_main_save", HighLogic.SaveFolder , false, false);

                        foreach (Vessel v in FlightGlobals.Vessels.FindAll(v => v.loaded))
                            id_list.Add(v.id);

                        temp_vessel_list = recover_save.flightState.protoVessels.FindAll(tpv => id_list.Contains(tpv.vesselID) && (tpv.landed || tpv.splashed));

                        foreach (ProtoVessel pv in temp_vessel_list)
                        {
                            if (pv.vesselID == parent_id)
                                vessel_list.Add(pv);
                            else
                            {
                                foreach (ProtoPartSnapshot ps in pv.protoPartSnapshots)
                                {
                                    foreach (ProtoPartModuleSnapshot ppms in ps.modules)
                                        if (ppms.moduleName == "FMRS_PM")
                                            if (new Guid(ppms.moduleValues.GetValue("parent_vessel")) == parent_id)
                                                vessel_list.Add(pv);
                                    break;
                                }
                            }
                        }

                        savegame = recover_vessel(parent_id, vessel_list, recover_save, savegame);

                        ProtoVessel temp_proto = savegame.flightState.protoVessels.Find(p => p.vesselID == vessel_recovered.id);
                        if (temp_proto != null)
                            savegame.flightState.protoVessels.Remove(temp_proto);

                        GamePersistence.SaveGame(savegame, "FMRS_main_save", HighLogic.SaveFolder , SaveMode.OVERWRITE);
                    }
                    else
                        set_vessel_state(vessel_recovered.id, vesselstate.RECOVERED);

                }
            }

            save_landed_vessel(false);
            _SAVE_Kick_To_Main = true;
            _SAVE_Switched_To_Dropped = false;

            write_save_values_to_file();

            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: leave recovery_requested_handler(Vessel input)");
        }
        

/*************************************************************************************************************************/
        public void scene_change_handler(GameScenes input_scene)
        {

            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: enter scene_change_handler(GameScenes input_scene) " + input_scene.ToString());
            if (Debug_Active)
                Debug.Log("#### FMRS: scene_change_handler");

            if (input_scene != GameScenes.FLIGHT)
            {
                if (Debug_Active)
                    Debug.Log("#### FMRS: switch to not flight scene");

                if (_SAVE_Kick_To_Main)
                    return;

                if (Debug_Active)
                    Debug.Log("#### FMRS: has not recovered");

                if (_SAVE_Switched_To_Dropped)
                {
                    if (Debug_Active)
                        Debug.Log("#### FMRS: scene change while flying dropped, kick to main in space center and tracking station");

                    set_recoverd_value("warning", "FMRS Info:", "You have switched scenes, while controlling a dropped vessel.@Next time, please use the 'Jump back to Main Mission' button, before switching scenes.");
                    _SAVE_Kick_To_Main = true;
                    _SAVE_Switched_To_Dropped = false;
                }
                else
                    _SETTING_Enabled = false;

                save_landed_vessel(true);
            }

            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: leave scene_change_handler(GameScenes input_scene)");

        }


/*************************************************************************************************************************/
        public void flight_scene_start_routine()
        {
            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: entering flight_scene_start_routine()");
            if (Debug_Active)
                Debug.Log("#### FMRS: FMRS flight_scene_start_routine");

            plugin_active = true;

            if (FlightGlobals.ActiveVessel.situation == Vessel.Situations.PRELAUNCH || n_launchpad_preflight || flight_preflight)
            {
                if (Debug_Active)
                    Debug.Log("#### FMRS: FMRS Vessel is prelaunch");

                delete_dropped_vessels();
                _SETTING_Enabled = true;
                _SAVE_Switched_To_Dropped = false;
                _SAVE_Kick_To_Main = false;
                _SAVE_Main_Vessel = FlightGlobals.ActiveVessel.id;
                _SAVE_Launched_At = 0;
                _SAVE_Has_Launched = false;

                if (flight_preflight)   
                    _SAVE_Flight_Reset = true;

                foreach (Part p in FlightGlobals.ActiveVessel.Parts)
                {
                    foreach (PartModule pm in p.Modules)
                        if (pm.moduleName == "FMRS_PM")
                            pm.StartCoroutine("resetid");
                }  

                recover_values.Clear();

                GamePersistence.SaveGame("before_launch", HighLogic.SaveFolder + "/FMRS", SaveMode.OVERWRITE);
                GamePersistence.SaveGame("FMRS_main_save", HighLogic.SaveFolder , SaveMode.OVERWRITE);            
            }

            can_restart = HighLogic.CurrentGame.Parameters.Flight.CanRestart;
            if (HighLogic.CurrentGame.Parameters.Flight.CanQuickLoad && HighLogic.CurrentGame.Parameters.Flight.CanQuickSave)
                can_q_save_load = true;
            else
                can_q_save_load = false;

            if (FlightGlobals.ActiveVessel.id == _SAVE_Main_Vessel)
                _SAVE_Switched_To_Dropped = false;
            else
                _SAVE_Switched_To_Dropped = true;


            Time_Trigger_Start_Delay = Planetarium.GetUniversalTime();
            timer_start_delay_active = true;

            if(_SAVE_Switched_To_Dropped)
            {
                foreach(KeyValuePair<Guid,string> kvp in Vessels_dropped)
                {
                    if (kvp.Value == _SAVE_Switched_To_Savefile)
                        if (get_vessel_state(kvp.Key) == vesselstate.FLY)
                            loaded_vessels.Add(kvp.Key);
                }
                if (Debug_Active)
                    Debug.Log("#### FMRS: loaded_vessels: " + loaded_vessels.Count.ToString());

                if (_SETTING_Throttle_Log)
                {
                    ThrottleReplay = new FMRS_THL.FMRS_THL_Rep(Debug_Active, Debug_Level_1_Active);

                    foreach (Vessel v in FlightGlobals.Vessels)
                    {
                        if (v.id == _SAVE_Main_Vessel)
                        {
                            //if (v.loaded)
                            {
                                Debug.Log("#### FMRS: appling flybywire callback to main vessel");
                                
                                v.OnFlyByWire += new FlightInputCallback(ThrottleReplay.flybywire);                               
                            }
                        }
                    }
                }
            }

            if ((windowPos.x == 0) && (windowPos.y == 0))
            {
                windowPos = new Rect(Screen.width / 2, Screen.height / 2, 10, 10);
                write_save_values_to_file();
            }
            
            Vessels.Clear();
            fill_Vessels_list();

            timer_staging_active = false;
            really_close = false;
            revert_to_launch = false; 
            separated_vessel = false;
            staged_vessel = false;

            current_rep = Reputation.CurrentRep;
            last_rep_change = 0;

            if (_SAVE_Switched_To_Dropped)
            {
                GameEvents.OnVesselRecoveryRequested.Add(recovery_requested_handler);
                GameEvents.Contract.onCompleted.Add(contract_routine);
                GameEvents.OnScienceRecieved.Add(science_sent_routine);
                GameEvents.onCollision.Add(crash_handler);
                GameEvents.onCrash.Add(crash_handler);
                GameEvents.onCrashSplashdown.Add(crash_handler);
                GameEvents.onCrewKilled.Add(crew_killed_handler);
                GameEvents.OnReputationChanged.Add(rep_changed);
                GameEvents.OnKSCStructureCollapsing.Add(building_destroyed);
                GameEvents.onVesselGoOffRails.Add(vessel_off_rails);
                GameEvents.onVesselSituationChange.Add(vessel_state_changed);
            }
            else
            {
                if (_SAVE_Has_Launched)
                {
                    GameEvents.onStageSeparation.Add(staging_routine);
                    GameEvents.onVesselCreate.Add(vessel_create_routine);
                }
            }
                
            GameEvents.onVesselGoOnRails.Add(vessel_on_rails);
            GameEvents.onVesselChange.Add(vessel_change_handler);
            GameEvents.onGameSceneLoadRequested.Add(scene_change_handler);

            if (!main_ui_active)
            {
                main_ui_active = true;
                if (Debug_Active)
                    Debug.Log("#### FMRS: activate drawMainGUI");
            }

            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: leaving flight_scene_start_routine()");
        }


/*************************************************************************************************************************/
        public void main_vessel_changed(string save_file)
        {
            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: enter main_vessel_changed(string save_file) " + save_file);
            if (Debug_Active)
                Debug.Log("#### FMRS: switching main vessel");
            
            ProtoVessel temp_proto;
            Game loadgame = GamePersistence.LoadGame(save_file, HighLogic.SaveFolder + "/FMRS", false, false);

            if (loadgame != null && loadgame.compatible && loadgame.flightState != null)
            {
                temp_proto = loadgame.flightState.protoVessels.Find(p => p.vesselID == _SAVE_Main_Vessel);
                if (temp_proto != null)
                {
                    if (Vessels_dropped.ContainsKey(temp_proto.vesselID))
                        delete_dropped_vessel(temp_proto.vesselID);

                    Vessels_dropped.Add(temp_proto.vesselID, quicksave_file_name);
                    Vessels_dropped_names.Add(temp_proto.vesselID, temp_proto.vesselName);
                    Vessels.Add(temp_proto.vesselID);
                }
                else
                    if (Debug_Active)
                        Debug.Log("#### FMRS: main vessel not found");

                if (Vessels_dropped.ContainsKey(FlightGlobals.ActiveVessel.id))
                    delete_dropped_vessel(FlightGlobals.ActiveVessel.id);

               _SAVE_Main_Vessel = FlightGlobals.ActiveVessel.id;
            }
            else
                if (Debug_Active)
                    Debug.Log("#### FMRS: unable to load savefile");

            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: leaving main_vessel_changed(string save_file)");
        }


/*************************************************************************************************************************/
        public void vessel_create_routine(Vessel input)
        {
            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: enter vessel_create_routine(Vessel input) " + input.id.ToString());
            if (Debug_Active)
                Debug.Log("#### FMRS: Vessel created");
            
            if (!staged_vessel)
            {
                Time_Trigger_Staging = Planetarium.GetUniversalTime() + 1;
                separated_vessel = true;
                timer_staging_active = true;
                timer_cuto_active = true;
                Time_Trigger_Cuto = Planetarium.GetUniversalTime();
            }

            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: leaving vessel_create_routine(Vessel input)");
        }


/*************************************************************************************************************************/
        public void flight_scene_update_routine()
        {
            if (_SETTING_Enabled)
            {
                if (timer_start_delay_active)
                    if ((Timer_Start_Delay + Time_Trigger_Start_Delay) <= Planetarium.GetUniversalTime())
                    {
                        if (!_SAVE_Switched_To_Dropped)
                        {
                            write_recovered_values_to_save();
                        }
                        else
                        {
                            List<Guid> temp_guid_list = new List<Guid>();
                            foreach(Guid id in loaded_vessels)
                            {
                                if (FlightGlobals.Vessels.Find(v => v.id == id) == null)
                                    temp_guid_list.Add(id);
                            }
                            foreach (Guid id in temp_guid_list)
                            {
                                if (Debug_Active)
                                    Debug.Log("#### FMRS: loaded_vessels: removing " + id.ToString());
                                loaded_vessels.Remove(id);
                            }
                            
                            if (Debug_Active)
                                Debug.Log("#### FMRS: loaded_vessels: " + loaded_vessels.Count.ToString());
                        }
                        timer_start_delay_active = false;
                    }
        
                if (timer_staging_active)
                {
                    if (timer_cuto_active)
                    {
                        if ((Time_Trigger_Cuto + 0.1) <= Planetarium.GetUniversalTime())
                        {
                            if (Debug_Active)
                                Debug.Log("#### FMRS: auto thrust cut off");

                            foreach (Vessel temp_vessel in FlightGlobals.Vessels)
                            {
                                if (!Vessels.Contains(temp_vessel.id))
                                {
                                    if (_SETTING_Auto_Cut_Off)
                                        temp_vessel.ctrlState.mainThrottle = 0;
                                }
                            }
                            timer_cuto_active = false;
                        }
                    }

                    if ((Time_Trigger_Staging + Timer_Stage_Delay) <= Planetarium.GetUniversalTime())
                    {
                        if (Debug_Active)
                            Debug.Log("#### FMRS: Has Staged Delayed");

                        last_staging_event = Planetarium.GetUniversalTime();

                        timer_staging_active = false;

                        quicksave_file_name = gamesave_name + FlightGlobals.ActiveVessel.currentStage.ToString();

                        if(Vessels_dropped.ContainsValue(quicksave_file_name) || (separated_vessel && !staged_vessel))
                        {
                            int nr_save_file = 0;

                            foreach (KeyValuePair<Guid, string> temp_keyvalues in Vessels_dropped)
                            {
                                if (temp_keyvalues.Value.Contains("separated_"))
                                    if (nr_save_file <= Convert.ToInt16(temp_keyvalues.Value.Substring(20)))
                                        nr_save_file = Convert.ToInt16(temp_keyvalues.Value.Substring(20)) + 1;
                            }

                            quicksave_file_name = gamesave_name + "separated_" + nr_save_file;
                        }

                        separated_vessel = false;
                        staged_vessel = false;

                        if (search_for_new_vessels(quicksave_file_name))
                        {
                            GamePersistence.SaveGame(quicksave_file_name, HighLogic.SaveFolder + "/FMRS", SaveMode.OVERWRITE);

                            if (_SAVE_Main_Vessel != FlightGlobals.ActiveVessel.id && !_SAVE_Switched_To_Dropped)
                                main_vessel_changed(quicksave_file_name);

                            set_save_value(save_cat.SAVEFILE, quicksave_file_name, Planetarium.GetUniversalTime().ToString());
                            write_save_values_to_file();
                        }
                    }
                }
                
                if (n_launchpad_preflight && !FlightGlobals.ActiveVessel.Landed)
                {
                    EventReport dummy_event = null;

                    if (Debug_Active)
                        Debug.Log("#### FMRS: non launchpad launch");
                    n_launchpad_preflight = false;
                    launch_routine(dummy_event);
                }
            }
        }


/*************************************************************************************************************************/
        public void contract_routine(Contracts.Contract input_contract)
        {
             if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: enter vcontract_routine(Contracts.Contract input_contract) " + input_contract.Title);
            if (Debug_Active)
                Debug.Log("#### FMRS: contract " + input_contract.Title + " " + input_contract.ContractState.ToString());

            if (HighLogic.CurrentGame.Mode != Game.Modes.CAREER)
            {
                if (Debug_Active)
                    Debug.Log("#### FMRS: not in carreer mode, leave contract_routine(Contracts.Contract input_contract)");
                return;
            }

            if (!contract_complete.ContainsKey(FlightGlobals.ActiveVessel.id))
                contract_complete.Add(FlightGlobals.ActiveVessel.id, new List<Contract>());

            contract_complete[FlightGlobals.ActiveVessel.id].Add(input_contract);

            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: leave vcontract_routine(Contracts.Contract input_contract)");
        }


/*************************************************************************************************************************/
        public void science_sent_routine(float science, ScienceSubject input_science_sub)
        {
            science_data_sent data;

            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: enter science_routine(float amount, ScienceSubject input_science_sub) " + science.ToString() + " " + input_science_sub.title);
            if (Debug_Active)
                Debug.Log("#### FMRS: Science received");

            if (HighLogic.CurrentGame.Mode != Game.Modes.CAREER && HighLogic.CurrentGame.Mode != Game.Modes.SCIENCE_SANDBOX)
            {
                if (Debug_Active)
                    Debug.Log("#### FMRS: not in sience carreer mode, leave science_routine(float amount, ScienceSubject input_science_sub)");
                return;
            }

            if (Debug_Active)
                Debug.Log("#### FMRS: science sent: " + input_science_sub.id + " + " + science.ToString());

            data.id = input_science_sub.id;
            data.amount = science;
            science_sent.Add(data);
        
            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: leave science_routine(float amount, ScienceSubject input_science_sub)");
        }


/*************************************************************************************************************************/
        public void FMRS_core_awake()
        {
            mod_vers = "v";

#if DEBUG //**************************
            mod_vers = "x";
#endif //**************************

#if BETA //**************************
            mod_vers = "b.";
#endif //**************************

            mod_vers += version_number;

#if BETA //**************************
            mod_vers += beta_version;
#endif //**************************
            init_Save_File_Content();
            load_save_file();
        }


/*************************************************************************************************************************/
        private void init_skin()
        {
            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: enter sinit_skin()");
            if (Debug_Active)
                Debug.Log("#### FMRS: init_skin");

            GUIStyle MyButton = new GUIStyle(HighLogic.Skin.button);
            GUIStyle MyTextArea = new GUIStyle(HighLogic.Skin.textArea);
            GUIStyle MyScrollView = new GUIStyle(HighLogic.Skin.scrollView);
            
            MyButton.fontSize = 15;
            MyTextArea.fontSize = 15;
            MyButton.normal.textColor = Color.white;
            MyButton.hover.textColor = Color.yellow;
            MyButton.onNormal.textColor = Color.green;
            MyButton.padding = new RectOffset(5, 3, 3, 3);
            MyButton.border = new RectOffset(3, 3, 3, 3);
            MyButton.margin = new RectOffset(1, 1, 1, 1);
            MyButton.overflow = new RectOffset(1, 1, 1, 1);
            MyButton.alignment = TextAnchor.MiddleLeft;
            MyButton.wordWrap = false;
            MyButton.clipping = TextClipping.Clip;

            MyTextArea.padding = new RectOffset(3, 3, 4, 2);
            MyTextArea.border = new RectOffset(3, 3, 3, 3);
            MyTextArea.margin = new RectOffset(1, 1, 1, 1);
            MyTextArea.overflow = new RectOffset(1, 1, 1, 1);
            MyTextArea.alignment = TextAnchor.MiddleLeft;
            MyTextArea.wordWrap = false;
            MyTextArea.clipping = TextClipping.Clip;

            button_main = new GUIStyle(MyButton);
            button_green = new GUIStyle(MyButton);
            button_green.normal.textColor = button_green.focused.textColor = Color.green;
            button_red = new GUIStyle(MyButton);
            button_red.normal.textColor = button_red.focused.textColor = Color.red;
            button_yellow = new GUIStyle(MyButton);
            button_yellow.normal.textColor = button_yellow.focused.textColor = Color.yellow;
            button_small = new GUIStyle(MyButton);
            button_small.padding = new RectOffset(2, 3, 3, 3);
            button_small.alignment = TextAnchor.MiddleCenter;
            button_small_red = new GUIStyle(button_small);
            button_small_red.normal.textColor = button_small_red.focused.textColor = Color.red;

            button_big = new GUIStyle(MyButton);
            button_big.padding = new RectOffset(6, 6, 6, 6);
            button_big.alignment = TextAnchor.MiddleCenter;

            text_main = new GUIStyle(MyTextArea);
            text_green = new GUIStyle(MyTextArea);
            text_green.normal.textColor = Color.green;
            text_cyan = new GUIStyle(MyTextArea);
            text_cyan.normal.textColor = Color.cyan;
            text_red = new GUIStyle(MyTextArea);
            text_red.normal.textColor = Color.red;
            text_yellow = new GUIStyle(MyTextArea);
            text_yellow.normal.textColor = Color.yellow;
            text_heading = new GUIStyle(MyTextArea);
            text_heading.fontSize = 16;
            text_heading.fontStyle = FontStyle.Bold;
            text_heading.alignment = TextAnchor.MiddleCenter;

            area_style = new GUIStyle(MyTextArea);
            area_style.active = area_style.hover = area_style.normal;

            scrollbar_stlye = new GUIStyle(MyScrollView);
            scrollbar_stlye.padding = new RectOffset(3, 3, 3, 3);
            scrollbar_stlye.border = new RectOffset(3, 3, 3, 3);
            scrollbar_stlye.margin = new RectOffset(1, 1, 1, 1);
            scrollbar_stlye.overflow = new RectOffset(1, 1, 1, 1);

            skin_init = true;

            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: leave init_skin()");
        }


/*************************************************************************************************************************/
        private float vessels_cost(ProtoVessel temp_vessel)
        {
            float partcost, float_dummy, vesselcost = 0;
            PartResourceDefinition resc_def;
            
            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: enter float vessels_cost(ProtoVessel temp_vessel) " + temp_vessel.vesselID.ToString());
            if (Debug_Active)
                Debug.Log("#### FMRS: Calculate cost from: " + temp_vessel.vesselName);

            foreach (ProtoPartSnapshot part in temp_vessel.protoPartSnapshots)
            {
                ShipConstruction.GetPartCosts(part, part.partInfo, out partcost, out float_dummy);
                foreach (ProtoPartResourceSnapshot resc in part.resources)
                {
                    resc_def = PartResourceLibrary.Instance.resourceDefinitions[resc.resourceName];
                    partcost += float.Parse(resc.resourceValues.GetValue("amount")) * resc_def.unitCost;
                }
                vesselcost += partcost;
            }

            if (Debug_Active)
                Debug.Log("#### FMRS cost: " + vesselcost.ToString());
            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: leave float vessels_cost(ProtoVessel temp_vessel)");

            return(vesselcost);
        }


/*************************************************************************************************************************/
        private List<ScienceData> recover_science(ProtoVessel proto_vessel)
        {
            List<ScienceData> science_data = new List<ScienceData>();

            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: enter recover_science(GUI Vessel_id, Game save) " + proto_vessel.vesselID.ToString());
            if (Debug_Active)
                Debug.Log("#### FMRS: Try recover science from: " + proto_vessel.vesselName);

            foreach (ProtoPartSnapshot part in proto_vessel.protoPartSnapshots)
            {
                foreach (ProtoPartModuleSnapshot module in part.modules)
                {
                    foreach (ConfigNode science_node in module.moduleValues.GetNodes("ScienceData"))
                    {
                        science_data.Add(new ScienceData(science_node));
                        if (Debug_Active)
                            Debug.Log("#### FMRS: ScienceData: " + science_node.GetValue("subjectID") + " found");
                    }
                }
            }
            
            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: leave recover_science(GUI Vessel_id, Game save)");

            return science_data;
        }


/*************************************************************************************************************************/
        private Game recover_vessel(Guid parent_vessel, List<ProtoVessel> recover_vessels, Game recover_save, Game savegame)
        {
            float cost, total_cost = 0, total_rec_fac = 0, rec_fact, science = 0, strat_rec_fact = 1, temp_float;
            string message = "", kerbal_mess = "", contract_mess = "";
            List<Strategies.Strategy> strat_list;
            bool recovered = false;

            if (Debug_Active)
                Debug.Log("#### FMRS: enter Game recover_vessel(Guid parent_vessel, List<ProtoVessel> recover_vessels, Game recover_save, Game savegame): " + parent_vessel.ToString());

            strat_list = Strategies.StrategySystem.Instance.Strategies.FindAll(str => str.IsActive);
            if (strat_list != null)
            {
                if (Debug_Active)
                    Debug.Log("#### FMRS: active Strategies found");

                foreach (Strategies.Strategy str in strat_list)
                {
                    foreach (Strategies.StrategyEffectConfig conf in str.Config.Effects)
                    {
                        if (conf.Config.HasValue("valueId"))
                            if (conf.Config.GetValue("valueId") == "VesselRecoveryFactor")
                            {
                                if (Debug_Active)
                                    Debug.Log("#### FMRS: active recovery strategy found");

                                strat_rec_fact = float.Parse(conf.Config.GetValue("minValue"));
                                temp_float = float.Parse(conf.Config.GetValue("maxValue"));
                                temp_float = temp_float - strat_rec_fact;
                                temp_float *= str.Factor;
                                strat_rec_fact += temp_float;
                                Debug.Log("#### FMRS: Recovery Factor Strategy = " + strat_rec_fact.ToString());
                            }
                    }
                }
            }

            foreach (ProtoVessel pv in recover_vessels)
            {
                if(pv.landed || pv.splashed)
                {
                    if (Debug_Active)
                        Debug.Log("#### FMRS: Recover Vessel: " + pv.vesselName);

                    if (pv.GetVesselCrew().Count > 0)
                    {
                        foreach (ProtoCrewMember crew_member in pv.GetVesselCrew())
                        {
                            foreach (ProtoCrewMember member in savegame.CrewRoster.Crew)
                            {
                                if (member.name == crew_member.name)
                                {
                                    member.rosterStatus = ProtoCrewMember.RosterStatus.Available;
                                    kerbal_mess += "       " + member.name + "@";

                                    if (Debug_Active)
                                        Debug.Log("#### FMRS: " + crew_member.name + " set Available");
                                }
                            }
                        }
                    }

                    if (HighLogic.CurrentGame.Mode == Game.Modes.CAREER)
                    {
                        cost = vessels_cost(pv);
                        rec_fact = calc_recovery_factor(pv);
                        total_rec_fac += rec_fact;
                        cost *= rec_fact;
                        cost *= strat_rec_fact;
                        total_cost += cost;
                        set_recoverd_value("fund", "add", cost.ToString());
                    }

                    if (HighLogic.CurrentGame.Mode == Game.Modes.CAREER || HighLogic.CurrentGame.Mode == Game.Modes.SCIENCE_SANDBOX)
                    {
                        foreach (ScienceData recovered_data in recover_science(pv))
                        {
                            ScienceSubject temp_sub = ResearchAndDevelopment.GetSubjectByID(recovered_data.subjectID);
                            string temp_string = temp_sub.id + "@" + temp_sub.dataScale.ToString() + "@" + temp_sub.subjectValue.ToString() + "@" + temp_sub.scienceCap.ToString();
                            set_recoverd_value("science", temp_string, Math.Round(recovered_data.dataAmount, 2).ToString());
                            science += recovered_data.dataAmount;
                        }
                        foreach (science_data_sent science_sub in science_sent)
                        {
                            ScienceSubject temp_sub = ResearchAndDevelopment.GetSubjectByID(science_sub.id);
                            string temp_string = temp_sub.id + "@" + temp_sub.dataScale.ToString() + "@" + temp_sub.subjectValue.ToString() + "@" + temp_sub.scienceCap.ToString();
                            set_recoverd_value("science_sent", temp_string, Math.Round(science_sub.amount, 2).ToString());
                            science += science_sub.amount;
                        }
                    }

                    if (HighLogic.CurrentGame.Mode == Game.Modes.CAREER)
                    {
                        if (contract_complete.Count > 0)
                        {
                            if (contract_complete.ContainsKey(pv.vesselID))
                            {
                                foreach (Contract c in contract_complete[pv.vesselID])
                                {
                                    set_recoverd_value("contract", "complete", c.ContractID.ToString());
                                    contract_mess += "       " + c.Title + "@";
                                }
                                contract_complete.Remove(pv.vesselID);
                            }
                        }
                    }

                    ProtoVessel temp_proto = savegame.flightState.protoVessels.Find(p => p.vesselID == pv.vesselID);
                    if (temp_proto != null)
                        savegame.flightState.protoVessels.Remove(temp_proto);

                    recovered = true;
                }
            }

            if (recovered)
            {
                if (recover_vessels.Count() > 1 || damaged_vessels.Contains(parent_vessel))
                    message += "Vessel has been damaged!@";

                if (kerbal_mess != "")
                {
                    message += "@Kerbals:@";
                    message += kerbal_mess;
                }

                if (HighLogic.CurrentGame.Mode == Game.Modes.CAREER)
                {
                    message += "@Funds: +" + Math.Round(total_cost, 2).ToString() + "@";
                    total_rec_fac /= recover_vessels.Count;
                    message += "Recovery Factor Distance: " + (Math.Round(total_rec_fac, 2) * 100).ToString() + "%@";
                    if (strat_rec_fact != 1)
                        message += "Recovery Factor Strategy: " + (Math.Round(strat_rec_fact, 2) * 100).ToString() + "%@";
                }

                if (science > 0)
                    message += "@Scienec Data: +" + science.ToString() + " Mbit@@";

                if (contract_mess != "")
                {
                    message += "@Completed Contracts:@";
                    message += contract_mess;
                }

                message += "@Parts & Resources:@";
                message += get_vessel_part_list(recover_vessels);
                message += "@";

                if (message != "")
                    set_recoverd_value("message", "FMRS Recovery Info: " + Vessels_dropped_names[parent_vessel], message);

                set_vessel_state(parent_vessel, vesselstate.RECOVERED);

                if (loaded_vessels.Contains(parent_vessel))
                {
                    if (Debug_Active)
                        Debug.Log("#### FMRS: loaded_vessels: removing " + parent_vessel.ToString());
                    loaded_vessels.Remove(parent_vessel);
                    if (Debug_Active)
                        Debug.Log("#### FMRS: loaded_vessels: " + loaded_vessels.Count.ToString());
                }
            }

            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: leave recover_vessel(Guid parent_vessel, List<ProtoVessel> recover_vessels, Game recover_save, Game savegame)");

            return savegame;
        }

        
/*************************************************************************************************************************/
        private float calc_recovery_factor(ProtoVessel proto_Vessel)
        {
            float factor, landing_distance;

            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: enter calc_recovery_factor(ProtoVessel proto_Vessel) " + proto_Vessel.vesselID.ToString());
            if (Debug_Active)
                Debug.Log("#### FMRS: calculate recovery factor for " + proto_Vessel.vesselName);

            if (proto_Vessel.landedAt.Contains("LaunchPad") || proto_Vessel.landedAt.Contains("Runway"))
            {
                factor = 1;
                if (Debug_Active)
                    Debug.Log("#### FMRS: landed at launchpad or runway");
            }
            else
            {
                landing_distance = (float)SpaceCenter.Instance.GreatCircleDistance(SpaceCenter.Instance.cb.GetRelSurfaceNVector(proto_Vessel.latitude, proto_Vessel.longitude));
                factor = Mathf.Lerp(0.98f, 0.1f, (float)(landing_distance / (SpaceCenter.Instance.cb.Radius * Math.PI)));
            }

            if (Debug_Active)
                Debug.Log("#### FMRS: recovery factor " + factor.ToString() );
            return factor;
        }


/*************************************************************************************************************************/
        private void write_recovered_values_to_save()
        {    
            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: enter add_recvoered_values_to_save()");
            if (Debug_Active)
                Debug.Log("#### FMRS: add recovered values to game save");

            foreach (recover_value recover_data in recover_values)
            {
                if (HighLogic.CurrentGame.Mode == Game.Modes.CAREER)
                    if (recover_data.cat == "fund")
                    {
                        if (recover_data.key == "add")
                        {
                            if (Debug_Active)
                                Debug.Log("#### FMRS: adding " + recover_data.value + " funds");
                            Funding.Instance.AddFunds(float.Parse(recover_data.value), TransactionReasons.VesselRecovery);                 
                        }
                    }
                if (HighLogic.CurrentGame.Mode == Game.Modes.CAREER || HighLogic.CurrentGame.Mode == Game.Modes.SCIENCE_SANDBOX)
                {
                    if (recover_data.cat == "science")
                    {
                        if (Debug_Active)
                            Debug.Log("#### FMRS: adding " + recover_data.key + " " + recover_data.value + " science");

                        string[] line = recover_data.key.Split('@');
                        ScienceSubject subject;
                        subject = ResearchAndDevelopment.GetSubjectByID(line[0].Trim());
                        if (subject == null)
                        {
                            if (Debug_Active)
                                Debug.Log("#### FMRS: subject not found");

                            subject = new ScienceSubject(line[0].Trim(), line[1].Trim(), float.Parse(line[2].Trim()), float.Parse(line[3].Trim()), float.Parse(line[4].Trim()));
                        }
                        ResearchAndDevelopment.Instance.SubmitScienceData(float.Parse(recover_data.value), subject, 1f);
                    }
                    if (recover_data.cat == "science_sent")
                    {
                        if (Debug_Active)
                            Debug.Log("#### FMRS: adding " + recover_data.key + " " + recover_data.value + " science sent");

                        string[] line = recover_data.key.Split('@');
                        ScienceSubject subject;
                        subject = ResearchAndDevelopment.GetSubjectByID(line[0].Trim());
                        if (subject == null)
                        {
                            if (Debug_Active)
                                Debug.Log("#### FMRS: science subject not found");

                            subject = new ScienceSubject(line[0].Trim(), line[1].Trim(), float.Parse(line[2].Trim()), float.Parse(line[3].Trim()), float.Parse(line[4].Trim()));
                        }
                        ResearchAndDevelopment.Instance.SubmitScienceData(float.Parse(recover_data.value), subject, 1f);
                    }
                }
                if (HighLogic.CurrentGame.Mode == Game.Modes.CAREER)
                    if (recover_data.cat == "kerbal")
                    {
                        if (recover_data.key == "kill")
                        {
                            Reputation.Instance.AddReputation(float.Parse(recover_data.value), TransactionReasons.VesselLoss);
                        }
                    }

                if (HighLogic.CurrentGame.Mode == Game.Modes.CAREER)
                    if (recover_data.cat == "contract")
                    {
                        if (recover_data.key == "complete")
                        {
                            Contract temp_contract = null;
                            if (ContractSystem.Instance.ContractsFinished.Find(c => c.ContractID == long.Parse(recover_data.value)) == null) //sithilfe
                            {
                                temp_contract = ContractSystem.Instance.Contracts.Find(c => c.ContractID == long.Parse(recover_data.value));
                                if (temp_contract != null)
                                    if (temp_contract.ContractState != Contract.State.Completed)
                                    {
                                        if (Debug_Active)
                                            Debug.Log("#### FMRS: contract " + temp_contract.Title + " completed");

                                        temp_contract.Complete();
                                    }
                            }
                        }
                    }

                if (recover_data.cat == "message" && _SETTING_Messages)
                {
                    if (Debug_Active)
                        Debug.Log("#### FMRS: printing message");
                    MessageSystem.Instance.AddMessage(new MessageSystem.Message(recover_data.key, recover_data.value.Replace("@", System.Environment.NewLine), MessageSystemButton.MessageButtonColor.GREEN, MessageSystemButton.ButtonIcons.MESSAGE));
                }

                if (recover_data.cat == "warning")
                {
                    if (Debug_Active)
                        Debug.Log("#### FMRS: printing warning");
                    MessageSystem.Instance.AddMessage(new MessageSystem.Message(recover_data.key, recover_data.value.Replace("@", System.Environment.NewLine), MessageSystemButton.MessageButtonColor.RED, MessageSystemButton.ButtonIcons.ALERT));
                }
            }

            recover_values.Clear();
            write_recover_file();
            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: leave add_recvoered_values_to_save()");
        }

                
/*************************************************************************************************************************/
        void crew_killed_handler(EventReport report)
        {
            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: enter crew_killed_handler(EventReport report) " + report.sender);
            if (Debug_Active)
                Debug.Log("#### FMRS: crew member killed: " + report.sender + " rep los: " + last_rep_change.ToString());
            
            if (HighLogic.CurrentGame.Mode != Game.Modes.CAREER)
                return;

            if (Kerbal_dropped.ContainsKey(report.sender))
            {
                killed_kerbal_str killed;
                killed.name = report.sender;
                killed.rep = last_rep_change;
                killed.vessel_id = Kerbal_dropped[killed.name];
                killed_kerbals.Add(killed);

                if (Debug_Active)
                    Debug.Log("#### FMRS: " + report.sender + " was in dropped stage");
            }

            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: leave crew_killed_handler(EventReport report)");
        }


/*************************************************************************************************************************/
        void rep_changed(float rep, TransactionReasons reason)
        {
            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: enter rep_changed(float rep, TransactionReasons reason) " + rep.ToString() + " " + reason.ToString());

            if (HighLogic.CurrentGame.Mode != Game.Modes.CAREER)
                return;

            last_rep_change = rep - current_rep;

            if (Debug_Active)
                Debug.Log("#### FMRS: rep changed: " + rep.ToString() + " " + last_rep_change.ToString());

            current_rep = rep;

            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: leave rep_changed(float rep)");
        }


/*************************************************************************************************************************/
        void vessel_on_rails(Vessel vessel)
        {
            List<ProtoCrewMember> member_list = new List<ProtoCrewMember>();

            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: enter vessel_on_rails(Vessel vessel) " + vessel.id.ToString());
            if (Debug_Active)
                Debug.Log("#### FMRS: Vessel will be on rails: " + vessel.vesselName);

            if (Vessels_dropped.ContainsKey(vessel.id) && !_SAVE_Switched_To_Dropped)
            {
                if (Debug_Active)
                    Debug.Log("#### FMRS: this vessel is listed in dropped dict");

                foreach (ProtoPartSnapshot part_snapshot in vessel.protoVessel.protoPartSnapshots)
                {
                    foreach (ProtoCrewMember member in part_snapshot.protoModuleCrew)
                        member_list.Add(member);
                    foreach(ProtoCrewMember member in member_list)
                    {
                        if (Debug_Active)
                            Debug.Log("#### FMRS: remove crew member " + member.name);
                        part_snapshot.RemoveCrew(member);
                    }
                    member_list.Clear();
                }
            }

            if (_SAVE_Switched_To_Dropped)
            {
                if (loaded_vessels.Contains(vessel.id) && vessel.id != FlightGlobals.ActiveVessel.id && !vessel.Landed && !vessel.Splashed)
                {
                    if (Debug_Active)
                        Debug.Log("#### FMRS: loaded_vessels: removing " + vessel.id.ToString());
                    loaded_vessels.Remove(vessel.id);

                    if (Debug_Active)
                        Debug.Log("#### FMRS: vessel " + vessel.name + " removed from loaded_vessels");
                }

                if (vessel.id == _SAVE_Main_Vessel)
                {
                    if (ThrottleReplay != null)
                    {
                        vessel.OnFlyByWire -= new FlightInputCallback(ThrottleReplay.flybywire);
                        ThrottleReplay.EndReplay();
                    }
                }
            }

            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: leave vessel_on_rails(Vessel vessel)");
        }


/*************************************************************************************************************************/
        void vessel_off_rails(Vessel vessel)
        {
            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: enter vessel_off_rails(Vessel vessel) " + vessel.id.ToString());
            if (Debug_Active)
                Debug.Log("#### FMRS: Vessel will be off rails: " + vessel.vesselName);

            if (_SAVE_Switched_To_Dropped)
            {
                Vessel temp_vessel;
                temp_vessel = FlightGlobals.Vessels.Find(v => v.id == vessel.id);
                if (temp_vessel != null)
                    if (temp_vessel.loaded)
                        if (Vessels_dropped.ContainsKey(vessel.id))
                            if (Vessels_dropped[vessel.id] == _SAVE_Switched_To_Savefile)
                                if (!loaded_vessels.Contains(vessel.id))
                                {
                                    if (Debug_Active)
                                        Debug.Log("#### FMRS: Vessel will be off rails: adding to loaded_vessels");
                                    loaded_vessels.Add(vessel.id);
                                }

                if (vessel.id == FlightGlobals.ActiveVessel.id)
                {
                    List<Vessel> temp_vessel_list = new List<Vessel>();
                    temp_vessel_list = FlightGlobals.Vessels.FindAll(v => Vessels_dropped.ContainsKey(v.id) && v.loaded);
                    foreach (Vessel v in temp_vessel_list)
                        if (Vessels_dropped[v.id] == _SAVE_Switched_To_Savefile)
                            if (!loaded_vessels.Contains(v.id))
                            {
                                if (Debug_Active)
                                    Debug.Log("#### FMRS: Vessel will be off rails: adding to loaded_vessels");
                                loaded_vessels.Add(v.id);
                            }
                }
                if (Debug_Active)
                    Debug.Log("#### FMRS: loaded_vessels: " + loaded_vessels.Count.ToString());
            }  

            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: leave vessel_off_rails(Vessel vessel)");
        }


/*************************************************************************************************************************/
        private string get_vessel_part_list(List<ProtoVessel> vessel_list)
        {
            string return_string;
            Dictionary<string, int> parts = new Dictionary<string, int>();
            Dictionary<string, float> resources = new Dictionary<string, float>();

            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: enter get_vessel_part_list(ProtoVessel vessel)");
            if (Debug_Active)
                Debug.Log("#### FMRS: get vessel part list");

            foreach(ProtoVessel pv in vessel_list)
            {
                foreach (ProtoPartSnapshot part in pv.protoPartSnapshots)
                {
                    if (parts.ContainsKey(part.partInfo.title))
                        parts[part.partInfo.title]++;
                    else
                        parts.Add(part.partInfo.title, 1);

                    foreach (ProtoPartResourceSnapshot resc in part.resources)
                    {
                        if (resc.resourceName != "ElectricCharge")
                        {
                            if (resources.ContainsKey(resc.resourceName))
                                resources[resc.resourceName] += float.Parse(resc.resourceValues.GetValue("amount"));
                            else
                                resources.Add(resc.resourceName, float.Parse(resc.resourceValues.GetValue("amount")));
                        }
                    }
                }
            }
        
            return_string="       ";
            foreach (KeyValuePair<string, int> keyvlaue in parts)
            {
                return_string += keyvlaue.Value.ToString();
                return_string += " x ";
                return_string += keyvlaue.Key;
                return_string += "@       ";
            }
            foreach (KeyValuePair<string, float> keyvlaue in resources)
            {
                return_string += keyvlaue.Key;
                return_string += ": ";
                return_string += Math.Round(keyvlaue.Value, 2).ToString();
                return_string += "@       ";
            }   
            
            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: leave get_vessel_part_list(ProtoVessel vessel)");

            return return_string;      
        }


/*************************************************************************************************************************/
        public void toolbar_button_clicked()
        {
            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: enter toolbar_button_clicked()");
            if (Debug_Active)
                Debug.Log("#### FMRS: Toolbar Button Clicked");

            if(_SETTING_Enabled)
            {              
                if (!_SAVE_Has_Launched)
                    close_FMRS();
                else
                {
                    if (really_close)
                        really_close = false;
                    else
                        really_close = true;
                }
            }
            else
                toolbar_open();
          
            write_save_values_to_file();

            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: leave toolbar_button_clicked()");
        }


/*************************************************************************************************************************/
        public void close_FMRS()
        {
            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: enter close_FMRS()");
            if (Debug_Active)
                Debug.Log("#### FMRS: close plugin");

            _SETTING_Enabled = false;
            _SAVE_Has_Launched = false;
            delete_dropped_vessels();
            really_close = false;
            _SAVE_Flight_Reset = false;

            if (blz_toolbar_available)
                Toolbar_Button.TexturePath = "FMRS/icons/tb_blz_di";
            stb_texture.LoadImage(System.IO.File.ReadAllBytes(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "icons/tb_st_di.png")));

            if (_SAVE_Has_Launched && _SAVE_Switched_To_Dropped)
                jump_to_vessel("Main");

            destroy_FMRS();

            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: leave close_FMRS()");
        }


/*************************************************************************************************************************/
        public void toolbar_open()
        {
            bool arm_save = false;

            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: enter toolbar_open()");
            if (Debug_Active)
                Debug.Log("#### FMRS: enable plugin form toolbar");

            if (blz_toolbar_available)
                Toolbar_Button.TexturePath = "FMRS/icons/tb_blz_en";
            stb_texture.LoadImage(System.IO.File.ReadAllBytes(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "icons/tb_st_en.png")));

            _SETTING_Enabled = true;

            if (FlightGlobals.ActiveVessel.situation == Vessel.Situations.PRELAUNCH)
            {
                if (Debug_Active)
                    Debug.Log("#### FMRS: start plugin on launchpad");

                GameEvents.onLaunch.Add(launch_routine);
                flight_scene_start_routine();
            }
            else if (FlightGlobals.ActiveVessel.Landed)
            {
                if (Debug_Active)
                    Debug.Log("#### FMRS: start plugin not on launchpad");

                n_launchpad_preflight = true;
                flight_scene_start_routine();
            }
            else
            {
                if (Debug_Active)
                    Debug.Log("#### FMRS: start plugin during flight");

                if (!_SETTING_Armed)
                {
                    _SETTING_Armed = true;
                    arm_save = true;
                }
                flight_preflight = true;
                flight_scene_start_routine();
                EventReport dummy_event = null;
                launch_routine(dummy_event);
                if (arm_save)
                    _SETTING_Armed = false;
                flight_preflight = false;
            }

            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: leave toolbar_open()");
        }


/*************************************************************************************************************************/
        private void building_destroyed(DestructibleBuilding building)
        {
            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: enter building_destroyed(DestructibleBuilding building) " + building.name);
            if (Debug_Active)
                Debug.Log("#### FMRS: " + building.name + " destroyed");

            if (!damaged_buildings.Contains(building.name))
                damaged_buildings.Add(building.name);

            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: leaving building_destroyed(DestructibleBuilding building)");
        }


/*************************************************************************************************************************/
        private void vessel_state_changed(GameEvents.HostedFromToAction<Vessel, Vessel.Situations> input)
        {
            /*if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: enter vessel_state_changed(DestructibleBuilding building) " + input.host.ToString());
            if (Debug_Active)
                Debug.Log("#### FMRS: " + input.host.ToString() + " destroyed");

            

            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: leaving vessel_state_changed(DestructibleBuilding building)");*/
        }
    }
}
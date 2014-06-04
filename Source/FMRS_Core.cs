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
    public class FMRS_Core : FMRS_Util
    {
        public double Timer_Trigger;
        public bool timer_active;
        public double Timer_Delay = 1;
        public string quicksave_file_name;
        public Guid destr_vessel_id;
        public bool undocked_vessel, staged_vessel;
        public bool reset_n_launchpad = false, n_launchpad_preflight = false;
        public bool main_ui_active = false, reset_ui_active = false;
        bool scrollbar_enable = false, n_scrollbar = false;
        bool reset_window_size;
        int nr_save_files = 0;
        public Vector2 scroll_Vector = Vector2.zero;


/*************************************************************************************************************************/
        public void MainGUI(int windowID)
        {
            List<string> save_files = new List<string>();
            Vessel temp_vessel;
            string temp_string;

            GUIStyle Button_Style_Main = new GUIStyle(GUI.skin.button);
            Button_Style_Main.normal.textColor = Button_Style_Main.focused.textColor = Color.white;
            Button_Style_Main.hover.textColor = Button_Style_Main.active.textColor = Color.yellow;
            Button_Style_Main.onNormal.textColor = Button_Style_Main.onFocused.textColor = Button_Style_Main.onHover.textColor = Button_Style_Main.onActive.textColor = Color.green;
            Button_Style_Main.padding = new RectOffset(8, 4, 4, 4);
            Button_Style_Main.alignment = TextAnchor.MiddleLeft;

            GUIStyle Button_Style_Landed = new GUIStyle(Button_Style_Main);
            Button_Style_Landed.normal.textColor = Button_Style_Landed.focused.textColor = Color.green;

            GUIStyle Button_Style_Destroyed = new GUIStyle(Button_Style_Main);
            Button_Style_Destroyed.normal.textColor = Button_Style_Destroyed.focused.textColor = Color.red;

            GUIStyle Button_Style_Small = new GUIStyle(Button_Style_Main);
            Button_Style_Small.padding = new RectOffset(3, 4, 4, 4);
            Button_Style_Small.alignment = TextAnchor.MiddleCenter;

            GUIStyle Button_Style_Big = new GUIStyle(Button_Style_Main);
            Button_Style_Big.padding = new RectOffset(8, 8, 8, 8);
            Button_Style_Big.alignment = TextAnchor.MiddleCenter;

            GUIStyle Button_Style_Controll = new GUIStyle(Button_Style_Main);
            Button_Style_Controll.normal.textColor = Button_Style_Controll.focused.textColor = Color.yellow;

            GUIStyle Text_Style_Main = new GUIStyle(GUI.skin.textArea);
            Text_Style_Main.padding = new RectOffset(4, 4, 8, 8);

            GUIStyle Text_Style_Landed = new GUIStyle(Text_Style_Main);
            Text_Style_Landed.normal.textColor = Color.green;
            Text_Style_Landed.padding = new RectOffset(4, 4, 5, 5);

            GUIStyle Text_Style_Destroyed = new GUIStyle(Text_Style_Main);
            Text_Style_Destroyed.normal.textColor = Color.red;
            Text_Style_Destroyed.padding = new RectOffset(4, 4, 5, 5);

            GUIStyle Text_Style_Controlling = new GUIStyle(Text_Style_Main);
            Text_Style_Controlling.normal.textColor = Color.yellow;
            Text_Style_Landed.padding = new RectOffset(4, 4, 5, 5);

            GUIStyle Text_Style_Heading = new GUIStyle(Text_Style_Main);
            Text_Style_Heading.fontStyle = FontStyle.Bold;
            Text_Style_Heading.fontSize = 16;
            Text_Style_Heading.alignment = TextAnchor.MiddleCenter;

            GUIStyle areaStyle = new GUIStyle(HighLogic.Skin.textArea);
            areaStyle.active = areaStyle.hover = areaStyle.normal;

            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();

            if (nr_save_files > 0 && windowPos.height > 450)
                scrollbar_enable = true;
            else
                scrollbar_enable = false;

            if (nr_save_files > 0 && !scrollbar_enable)
                n_scrollbar = true;
            else
                n_scrollbar = false;

            if(reset_window_size)
            {
                windowPos.height = 0;
                windowPos.width = 0;
                reset_window_size = false;
            }

            if (!minimize_window)
            {
                if (_SAVE_Has_Launched)
                    GUILayout.Box("Mission Time: " + get_time_string(Planetarium.GetUniversalTime() - Convert.ToDouble(get_save_value("_SAVE_Launched_At"))), Text_Style_Main, GUILayout.Width(260));
                else
                    GUILayout.Box("Mission Time: " + get_time_string(0), Text_Style_Main, GUILayout.Width(210));
            }

            if (armed)
                temp_string = "Armed";
            else
                temp_string = "Arm";

            if (!_SAVE_Has_Launched)
                armed = GUILayout.Toggle(armed, temp_string, Button_Style_Small, GUILayout.Width(50));
            else
                if (minimize_window)
                    GUILayout.Box("Flight", Text_Style_Main, GUILayout.Width(50));

            minimize_window = GUILayout.Toggle(minimize_window, "_", Button_Style_Small, GUILayout.Width(25));
            really_close = GUILayout.Toggle(really_close, "X", Button_Style_Small, GUILayout.Width(25));

            GUILayout.EndHorizontal();

            if (minimize_window != minimize_window_old)
            {
                minimize_window_old = minimize_window;
                reset_window_size = true;
            }

            if (really_close != really_close_old)
            {
                really_close_old = really_close;
                reset_window_size = true;
            }

            if (really_close)
            {
                GUILayout.Space(10);
                GUILayout.Box("Close Really?", Text_Style_Heading, GUILayout.Width(310));

                GUILayout.BeginHorizontal();

                if (GUILayout.Button("YES", Button_Style_Big, GUILayout.Width(155)))
                {
                    if (Debug_Active)
                        Debug.Log("#### FMRS: close FMRS from UI");

                    delete_dropped_vessels();
                    _SAVE_Has_Closed = true;

                    if (FlightGlobals.ActiveVessel.situation != Vessel.Situations.PRELAUNCH && _SAVE_Has_Launched &&
                        FlightGlobals.ActiveVessel.id.ToString() != get_save_value("_SAVE_Main_Vessel") && !n_launchpad_preflight)
                        jump_to_vessel("Main");
                    
                    disable_FMRS();
                }
                if (GUILayout.Button("NO", Button_Style_Big, GUILayout.Width(155)))
                    really_close = false;

                GUILayout.EndHorizontal();
            }

            if (really_close == false && minimize_window == false)
            {
                if (Vessels_dropped.Count > 0)
                {
                    GUILayout.Space(10);
                    GUILayout.Box("Separated Stages:", Text_Style_Heading, GUILayout.Width(310));
                }

                foreach (KeyValuePair<Guid, string> temp_keyvalue in Vessels_dropped)
                {
                    if (!save_files.Contains(temp_keyvalue.Value))
                        save_files.Add(temp_keyvalue.Value);
                }

                save_files.Sort(delegate(string x, string y)
                {
                    return get_save_value(y).CompareTo(get_save_value(x));
                });

                nr_save_files = save_files.Count;

                if (scrollbar_enable)
                {
                    scroll_Vector = GUILayout.BeginScrollView(scroll_Vector, GUILayout.Width(310), GUILayout.Height(300));
                    GUILayout.BeginVertical();
                }

                if (n_scrollbar)
                    GUILayout.BeginVertical(areaStyle, GUILayout.Width(310));

                while (save_files.Count != 0)
                {
                    GUILayout.Space(5);
                    GUILayout.BeginVertical(areaStyle);
                    if(save_files.Last().Contains("_undocked_"))
                        GUILayout.Box("Undocked at " + get_time_string(Convert.ToDouble(get_save_value(save_files.Last())) - Convert.ToDouble(get_save_value("_SAVE_Launched_At"))), Text_Style_Main, GUILayout.Width(220));
                    else if (save_files.Last().Contains("_separated_"))
                        GUILayout.Box("Separated at " + get_time_string(Convert.ToDouble(get_save_value(save_files.Last())) - Convert.ToDouble(get_save_value("_SAVE_Launched_At"))), Text_Style_Main, GUILayout.Width(220));
                    else
                        GUILayout.Box("Stage " + save_files.Last().Substring(9) + " separated at " + get_time_string(Convert.ToDouble(get_save_value(save_files.Last())) - Convert.ToDouble(get_save_value("_SAVE_Launched_At"))), Text_Style_Main, GUILayout.Width(220));

                    foreach (KeyValuePair<Guid, string> vessel_in_savefile in Vessels_dropped)
                    {
                        if (vessel_in_savefile.Value == save_files.Last())
                        {
                            GUILayout.BeginHorizontal();
                            if (Vessels_dropped_landed.ContainsKey(vessel_in_savefile.Key))
                            {
                                GUILayout.Box(Vessels_dropped_names[vessel_in_savefile.Key] + " landed", Text_Style_Landed, GUILayout.Width(220));
                            }
                            else if (vessel_in_savefile.Key == FlightGlobals.ActiveVessel.id || vessel_in_savefile.Key == destr_vessel_id)
                            {
                                GUILayout.EndHorizontal();
                                GUILayout.BeginVertical(areaStyle);

                                if (Vessels_dropped_destroyed.ContainsKey(vessel_in_savefile.Key))
                                {
                                    GUILayout.Box(Vessels_dropped_names[vessel_in_savefile.Key] + " destroyed", Text_Style_Destroyed, GUILayout.Width(220));
                                }
                                else if (FlightGlobals.ActiveVessel.LandedOrSplashed)
                                {
                                    GUILayout.Box(Vessels_dropped_names[vessel_in_savefile.Key] + " landed", Text_Style_Landed, GUILayout.Width(220));
                                }
                                else
                                {
                                    GUILayout.Box("controlling: " + Vessels_dropped_names[vessel_in_savefile.Key], Text_Style_Controlling, GUILayout.Width(220));
                                }
                                if (GUILayout.Button("Jump back to Separation", Button_Style_Main, GUILayout.Width(225)))
                                    jump_to_vessel(vessel_in_savefile.Key, false);

                                GUILayout.EndVertical();
                                GUILayout.BeginHorizontal();
                            }
                            else if (Vessels_dropped_destroyed.ContainsKey(vessel_in_savefile.Key))
                            {
                                GUILayout.Box(Vessels_dropped_names[vessel_in_savefile.Key] + " damaged", Text_Style_Destroyed, GUILayout.Width(225));
                            }
                            else
                            {
                                temp_vessel = FlightGlobals.Vessels.Find(p => p.id == vessel_in_savefile.Key);

                                if (temp_vessel == null)
                                {
                                    if (GUILayout.Button(Vessels_dropped_names[vessel_in_savefile.Key], Button_Style_Main, GUILayout.Width(220)))
                                        jump_to_vessel(vessel_in_savefile.Key, true);
                                }
                                else
                                {
                                    if (temp_vessel.loaded && FlightGlobals.ActiveVessel.id.ToString() != get_save_value("_SAVE_Main_Vessel"))
                                    {
                                        if (temp_vessel.LandedOrSplashed)
                                        {
                                            if (GUILayout.Button(Vessels_dropped_names[vessel_in_savefile.Key] + " landed", Button_Style_Landed, GUILayout.Width(220)))
                                                FlightGlobals.ForceSetActiveVessel(temp_vessel);
                                        }
                                        else
                                        {
                                            if (GUILayout.Button(Vessels_dropped_names[vessel_in_savefile.Key] + " is near", Button_Style_Controll, GUILayout.Width(220)))
                                                FlightGlobals.ForceSetActiveVessel(temp_vessel);
                                        }
                                    }
                                    else
                                    {
                                        if (GUILayout.Button(Vessels_dropped_names[vessel_in_savefile.Key], Button_Style_Main, GUILayout.Width(220)))
                                            jump_to_vessel(vessel_in_savefile.Key, true);
                                    }
                                }
                            }

                            if (vessel_in_savefile.Key != FlightGlobals.ActiveVessel.id && vessel_in_savefile.Key != destr_vessel_id)
                            {
                                Button_Style_Small.normal.textColor = Button_Style_Small.focused.textColor = Color.red;
                                if (GUILayout.Button("X", Button_Style_Small, GUILayout.Width(25)))
                                {
                                    save_landed_vessel();
                                    delete_dropped_vessel(vessel_in_savefile.Key);
                                    reset_window_size = true;
                                }
                            }

                            GUILayout.EndHorizontal();
                            Button_Style_Main.normal.textColor = Button_Style_Main.focused.textColor = Color.white;
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

                if (n_scrollbar)
                    GUILayout.EndVertical();

                GUILayout.Space(5);

                if ((get_save_value("_SAVE_Main_Vessel") != FlightGlobals.ActiveVessel.id.ToString()))
                {

                    if (GUILayout.Button("Jump back to Main Mission", Button_Style_Big, GUILayout.Width(310)))
                    {
                        jump_to_vessel("Main");
                    }
                }

                if (_SAVE_Has_Launched)
                {
                    if (revert_to_launch != revert_to_launch_old)
                    {
                        revert_to_launch_old = revert_to_launch;
                        reset_window_size = true;
                    }

                    if (revert_to_launch)
                    {
                        GUILayout.Box("Really?", Text_Style_Heading, GUILayout.Width(310));

                        GUILayout.BeginHorizontal();

                        if (GUILayout.Button("YES", Button_Style_Big, GUILayout.Width(155)))
                        {
                            jump_to_vessel(get_save_value("_SAVE_Main_Vessel"), "before_launch");
                        }
                        if (GUILayout.Button("NO", Button_Style_Big, GUILayout.Width(155)))
                            revert_to_launch = false;

                        GUILayout.EndHorizontal();
                    }
                    else
                        revert_to_launch = GUILayout.Toggle(revert_to_launch, "Revert To Launch", Button_Style_Big, GUILayout.Width(310));
                }
            }

            GUILayout.EndVertical();
            GUI.DragWindow(new Rect(0, 0, 10000, 20));
        }


/*************************************************************************************************************************/
        public void staging_routine(EventReport event_imput)
        {
            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: entering staging_routine(EventReport event_imput)");

            Timer_Trigger = Planetarium.GetUniversalTime();
            timer_active = true;
            staged_vessel = true;
            undocked_vessel = false;

            if (Debug_Active)
                Debug.Log("#### FMRS: Has Staged");
            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: leaving staging_routine(EventReport event_imput)");
        }


/*************************************************************************************************************************/
        public void launch_routine(EventReport event_imput)
        {
            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: entering launch_routine(EventReport event_imput)");

            if (!_SAVE_Has_Launched)
            {
                _SAVE_Has_Launched = true;
                set_save_value("_SAVE_Launched_At", Planetarium.GetUniversalTime().ToString());
            }


            if (!armed)
            {
                _SAVE_Has_Closed = true;
                disable_FMRS();
                return;
            }

            reset_window_size = true;

            write_save_values_to_file();

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
            bool new_vessel_found = false;

            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: entering search_for_new_vessels(string save_file_name)");

            foreach (Vessel temp_vessel in FlightGlobals.Vessels)
            {
                if (!Vessels.Contains(temp_vessel.id))
                {
                    if (temp_vessel.isCommandable && temp_vessel.vesselType != VesselType.EVA && temp_vessel.vesselType != VesselType.Flag && temp_vessel.vesselType != VesselType.Debris && temp_vessel.vesselType != VesselType.SpaceObject && temp_vessel.vesselType != VesselType.Unknown)
                    {
                        if (Debug_Active)
                            Debug.Log("#### FMRS: " + temp_vessel.vesselName + " Found and will be added to the dicts");

                        Vessels_dropped.Add(temp_vessel.id, save_file_name);
                        Vessels_dropped_names.Add(temp_vessel.id, temp_vessel.vesselName);
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
        public List<Guid> search_for_new_vessels()
        {
            List<Guid> return_list = new List<Guid>();

            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: entering List<Guid> search_for_new_vessels()");

            foreach (Vessel temp_vessel in FlightGlobals.Vessels)
            {
                if (!Vessels.Contains(temp_vessel.id))
                {
                    return_list.Add(temp_vessel.id);
                    if (Debug_Active)
                        Debug.Log("#### FMRS: " + temp_vessel.vesselName + " Found and added to list");
                }
            }

            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: leaving List<Guid> search_for_new_vessels()");

            return (return_list);
        }


/*************************************************************************************************************************/
        public void drawMainGUI()
        {
            GUI.skin = HighLogic.Skin;
            windowPos = GUILayout.Window(1, windowPos, MainGUI, "FMRS " + mod_version, GUILayout.MinWidth(100), GUILayout.MaxHeight(460));
        }


/*************************************************************************************************************************/
        public void jump_to_vessel(Guid vessel_id, bool save_landed)
        {
            int load_vessel;

            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: entering jump_to_vessel(Guid vessel_id)");

            if (Debug_Active)
                Debug.Log("#### FMRS: Jump to " + vessel_id.ToString());

            if (save_landed)
            {
                if (FlightGlobals.ActiveVessel.id.ToString() == get_save_value("_SAVE_Main_Vessel"))
                {
                    GamePersistence.SaveGame("FMRS_main_save", HighLogic.SaveFolder + "/FMRS", SaveMode.OVERWRITE);
                }
                save_landed_vessel();
            }

            Game loadgame = GamePersistence.LoadGame(get_save_value(vessel_id.ToString()), HighLogic.SaveFolder + "/FMRS", false, false);

            if (loadgame != null && loadgame.compatible && loadgame.flightState != null)
            {
                if (Debug_Active)
                    Debug.Log("#### FMRS: try to laod gamefile " + get_save_value(vessel_id.ToString()));

                for (load_vessel = 0; load_vessel < loadgame.flightState.protoVessels.Count && loadgame.flightState.protoVessels[load_vessel].vesselID != vessel_id; load_vessel++) ;
                if (load_vessel <= loadgame.flightState.protoVessels.Count)
                {
                    if (Debug_Active)
                        Debug.Log("#### FMRS: FMRS_save found, Vessel found, try to start");

                    _SAVE_Switched_To_Dropped = true;
                    FlightDriver.StartAndFocusVessel(loadgame, load_vessel);
                }
            }
            else
                if (Debug_Active)
                    Debug.Log("#### FMRS: Loading gamefile " + get_save_value(vessel_id.ToString()));

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

            if (FlightGlobals.ActiveVessel.id.ToString() == get_save_value("_SAVE_Main_Vessel"))
                return;

            save_landed_vessel();

            loadgame = GamePersistence.LoadGame("FMRS_main_save", HighLogic.SaveFolder + "/FMRS", false, false);

            if (loadgame != null && loadgame.compatible && loadgame.flightState != null)
            {
                if (Debug_Active)
                    Debug.Log("#### FMRS: try to load gamefile FMRS_main_save");

                for (load_vessel = 0; load_vessel <= loadgame.flightState.protoVessels.Count && loadgame.flightState.protoVessels[load_vessel].vesselID.ToString() != get_save_value("_SAVE_Main_Vessel"); load_vessel++) ;
                if (load_vessel <= loadgame.flightState.protoVessels.Count)
                {
                    if (Debug_Active)
                        Debug.Log("#### FMRS: FMRS_main_save found, main vessel found, try to start");
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
        public void jump_to_vessel(string vessel_id, string save_file)
        {
            Game loadgame;
            int load_vessel;

            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: entering jump_to_vessel(string vessel_id,string save_file)");
            if (Debug_Active)
                Debug.Log("#### FMRS: Jump to " + save_file);

            loadgame = GamePersistence.LoadGame(save_file, HighLogic.SaveFolder + "/FMRS", false, false);

            if (loadgame != null && loadgame.compatible && loadgame.flightState != null)
            {
                if (Debug_Active)
                    Debug.Log("#### FMRS: try to laod gamefile" + save_file);

                for (load_vessel = 0; load_vessel < loadgame.flightState.protoVessels.Count && loadgame.flightState.protoVessels[load_vessel].vesselID.ToString() != vessel_id; load_vessel++) ;
                FlightDriver.StartAndFocusVessel(loadgame, load_vessel);
            }
            else
                if (Debug_Active)
                    Debug.Log("#### FMRS: Loading gamefile " + save_file + " failed");

            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: leaving jump_to_vessel(string vessel_id,string save_file)");
        }


/*************************************************************************************************************************/
        public void save_landed_vessel()
        {
            Game loadgame, savegame;
            ProtoVessel temp_proto;
            List<Vessel> vessel_list = new List<Vessel>();

            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: entering save_landed_vessel()");

            if (FlightGlobals.ActiveVessel.LandedOrSplashed)
            {
                if (Debug_Active)
                    Debug.Log("#### FMRS: active vessel has landed, splashed down or has been destroyed");

                if (FlightGlobals.ActiveVessel.id.ToString() != get_save_value("_SAVE_Main_Vessel"))
                {
                    if (Debug_Active)
                        Debug.Log("#### FMRS: vessel is not main vessel");

                    if (!Vessels_dropped_landed.ContainsKey(FlightGlobals.ActiveVessel.id))
                    {
                        if (Debug_Active)
                            Debug.Log("#### FMRS: Vessel " + FlightGlobals.ActiveVessel.id.ToString() + " save as landed");

                        if (Vessels_dropped_landed.ContainsKey(FlightGlobals.ActiveVessel.id))
                            Vessels_dropped_landed.Add(FlightGlobals.ActiveVessel.id, true);
                    }
                    else
                        if (Debug_Active)
                            Debug.Log("#### FMRS: vessel has been destroyed");

                    vessel_list = FlightGlobals.Vessels.FindAll(p => p.loaded && Vessels_dropped.ContainsKey(p.id) && p.state != Vessel.State.DEAD && p.LandedOrSplashed && !Vessels_dropped_destroyed.ContainsKey(p.id));
                    foreach (Vessel temp_vessel in vessel_list)
                    {
                        if (!Vessels_dropped_landed.ContainsKey(temp_vessel.id))
                            Vessels_dropped_landed.Add(temp_vessel.id, true);
                    }

                    if (Debug_Active)
                        Debug.Log("#### FMRS: SAVING MAIN SAVE");

                    GamePersistence.SaveGame("FMRS_quicksave", HighLogic.SaveFolder + "/FMRS", SaveMode.OVERWRITE);
                    loadgame = GamePersistence.LoadGame("FMRS_quicksave", HighLogic.SaveFolder + "/FMRS", false, false);
                    savegame = GamePersistence.LoadGame("FMRS_main_save", HighLogic.SaveFolder + "/FMRS", false, false);

                    List<Guid> new_vessel = search_for_new_vessels();

                    temp_proto = loadgame.flightState.protoVessels.Find(p => p.vesselID == FlightGlobals.ActiveVessel.id && FlightGlobals.ActiveVessel.state != Vessel.State.DEAD);
                    if (temp_proto != null && !new_vessel.Contains(temp_proto.vesselID))
                        new_vessel.Add(temp_proto.vesselID);

                    foreach (Vessel temp_vessel in vessel_list)
                    {
                        temp_proto = loadgame.flightState.protoVessels.Find(p => p.vesselID == temp_vessel.id);
                        if (temp_proto != null && !new_vessel.Contains(temp_proto.vesselID))
                            new_vessel.Add(temp_proto.vesselID);
                    }

                    foreach (KeyValuePair<Guid, bool> temp_keyvalues in Vessels_dropped_destroyed)
                    {
                        temp_proto = savegame.flightState.protoVessels.Find(p => p.vesselID == temp_keyvalues.Key);
                        if (temp_proto != null)
                            savegame.flightState.protoVessels.Remove(temp_proto);
                    }

                    if (new_vessel.Count != 0)
                    {
                        new_vessel.ForEach(delegate(Guid vessel_id)
                        {
                            savegame.flightState.protoVessels.ForEach(delegate(ProtoVessel prot_vessel)
                            {
                                if (prot_vessel.vesselID == vessel_id)
                                {
                                    savegame.flightState.protoVessels.Remove(prot_vessel);
                                }
                            });
                            temp_proto = loadgame.flightState.protoVessels.Find(p => p.vesselID == vessel_id);

                            if (savegame.flightState.protoVessels.Contains(temp_proto))
                                savegame.flightState.protoVessels.Remove(temp_proto);

                            savegame.flightState.protoVessels.Add(temp_proto);
                        });
                        GamePersistence.SaveGame(savegame, "FMRS_main_save", HighLogic.SaveFolder + "/FMRS", SaveMode.OVERWRITE);
                        GamePersistence.SaveGame(savegame, "persistent", HighLogic.SaveFolder, SaveMode.OVERWRITE);
                    }
                }
            }

            write_save_values_to_file();

            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: leaving save_landed_vessel()");
        }


/*************************************************************************************************************************/
        public void crash_handler(EventReport report)
        {
            List<Guid> new_vessels = new List<Guid>();

            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: enter crash_handler (EventReport report)");
            if (Debug_Active)
                Debug.Log("#### FMRS: crash detectd");

            if (FlightGlobals.ActiveVessel.state == Vessel.State.DEAD)
            {
                Debug.Log("#### FMRS: lost command");

                if (Vessels_dropped.ContainsKey(FlightGlobals.ActiveVessel.id) && !Vessels_dropped_destroyed.ContainsKey(FlightGlobals.ActiveVessel.id))
                    Vessels_dropped_destroyed.Add(FlightGlobals.ActiveVessel.id, true);

                destr_vessel_id = FlightGlobals.ActiveVessel.id;
            }
            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: leave crash_handler (EventReport report)");
        }


/*************************************************************************************************************************/
        public void vessel_change_handler(Vessel change_vessel)
        {
            if (Debug_Active)
                Debug.Log(" #### FMRS: changed to " + FlightGlobals.ActiveVessel.vesselName);
        }


/*************************************************************************************************************************/
        public void delete_dropped_vessels()
        {
            List<string> temp_list = new List<string>();
            int values_removed = 0;

            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: entering delete_dropped_vessels()");

            foreach (KeyValuePair<string, string> temp_key_pair in Save_File_Content)
            {
                if ((!temp_key_pair.Key.Contains("_SETTING_")) && (!temp_key_pair.Key.Contains("_SAVE_")))
                {
                    temp_list.Add(temp_key_pair.Key);
                    values_removed++;
                    if (Debug_Active)
                        Debug.Log("#### FMRS: " + temp_key_pair.Key.ToString() + " deleted");
                }
            }
            temp_list.ForEach(delegate(string temp_string) { Save_File_Content.Remove(temp_string); });
            Vessels_dropped.Clear();
            Vessels_dropped_names.Clear();
            Vessels_dropped_landed.Clear();
            Vessels_dropped_destroyed.Clear();
            write_save_values_to_file(values_removed);

            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: leaving delete_dropped_vessels()");
        }


/*************************************************************************************************************************/
        public void delete_dropped_vessel(Guid vessel_guid)
        {
            List<string> temp_list = new List<string>();
            int values_removed = 0;

            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: entering delete_dropped_vessel(Guid vessel_guid)");

            if (Debug_Active)
                Debug.Log("#### FMRS: remove vessel" + vessel_guid.ToString() + Vessels_dropped_names[vessel_guid]);

            Vessels_dropped.Remove(vessel_guid);
            Vessels_dropped_names.Remove(vessel_guid);
            if (Vessels_dropped_landed.ContainsKey(vessel_guid))
                Vessels_dropped_landed.Remove(vessel_guid);
            if (Vessels_dropped_destroyed.ContainsKey(vessel_guid))
                Vessels_dropped_destroyed.Remove(vessel_guid);

            foreach (KeyValuePair<string, string> temp_key_pair in Save_File_Content)
            {
                if ((!temp_key_pair.Key.Contains("_SETTING_")) && (!temp_key_pair.Key.Contains("_SAVE_")) && (temp_key_pair.Key.Contains(vessel_guid.ToString())))
                {
                    temp_list.Add(temp_key_pair.Key);
                    values_removed++;
                    if (Debug_Active)
                        Debug.Log("#### FMRS: " + temp_key_pair.Key.ToString() + " deleted");
                }
            }
            temp_list.ForEach(delegate(string temp_string) { Save_File_Content.Remove(temp_string); });
            write_save_values_to_file(values_removed);

            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: leaving delete_dropped_vessel(Guid vessel_guid)");
        }


/*************************************************************************************************************************/
        public void disable_FMRS()
        {
            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: enter disable_FMRS()");

            if (_SAVE_Has_Closed)
                _SAVE_Has_Launched = false;

            write_save_values_to_file();

            if (main_ui_active)
            {
                RenderingManager.RemoveFromPostDrawQueue(3, new Callback(drawMainGUI));
                main_ui_active = false;
                if (Debug_Active)
                    Debug.Log("#### FMRS: close drawMainGUI");
            }
            if (reset_ui_active)
            {
                RenderingManager.RemoveFromPostDrawQueue(3, new Callback(drawResetGUI));
                reset_ui_active = false;
                if (Debug_Active)
                    Debug.Log("#### FMRS: close drawResetGUI");
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
            
            reset_n_launchpad = false;
            n_launchpad_preflight = false;

            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: leave disable_FMRS()");
        }


/*************************************************************************************************************************/
        public void recovery_requested_handler(Vessel input)
        {
            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: enter recovery_requested_handler(Vessel input)");
            if (Debug_Active)
                Debug.Log("#### FMRS: Vessel recovery_requested_handler");

            save_landed_vessel();

            _SAVE_Has_Recovered = true;
            set_save_value("_SAVE_Recovered_Vessel", input.id.ToString());

            delete_dropped_vessel(input.id);
            set_save_value("_RECOVERED_" + input.id.ToString(), true.ToString());

            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: leave recovery_requested_handler(Vessel input)");
        }


/*************************************************************************************************************************/
        public void scene_change_handler(GameScenes input_scene)
        {

            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: enter scene_change_handler(GameScenes input_scene)");
            if (Debug_Active)
                Debug.Log("#### FMRS: scene_change_handler");

            if (input_scene != GameScenes.FLIGHT)
            {
                if (Debug_Active)
                    Debug.Log("#### FMRS: switch to not flight scene");

                if (_SAVE_Has_Recovered)
                    return;

                if (Debug_Active)
                    Debug.Log("#### FMRS: has not recovered");

                save_landed_vessel();

                _SAVE_Has_Closed = true;
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
                Debug.Log("#### FMRS: FMRS On Start");

            if (FlightGlobals.ActiveVessel.situation == Vessel.Situations.PRELAUNCH || n_launchpad_preflight)
            {
                delete_dropped_vessels();

                _SAVE_Has_Launched = false;
                _SAVE_Has_Closed = false;
                set_save_value("_SAVE_Main_Vessel", FlightGlobals.ActiveVessel.id.ToString());
                set_save_value("_SAVE_Launched_At", false.ToString());
                _SAVE_Switched_To_Dropped = false;

                GamePersistence.SaveGame("before_launch", HighLogic.SaveFolder + "/FMRS", SaveMode.OVERWRITE);
            }
            else
            {
                get_dropped_vessels();
            }

            if (FlightGlobals.ActiveVessel.id.ToString() == get_save_value("_SAVE_Main_Vessel"))
                _SAVE_Switched_To_Dropped = false;

            if (Vessels_dropped.ContainsKey(FlightGlobals.ActiveVessel.id))
            {
                int values_removed = 0;

                if (Vessels_dropped_destroyed.ContainsKey(FlightGlobals.ActiveVessel.id))
                {
                    Vessels_dropped_destroyed.Remove(FlightGlobals.ActiveVessel.id);
                    values_removed++;
                    if (Debug_Active)
                        Debug.Log("#### FMRS: delete " + FlightGlobals.ActiveVessel.id.ToString() + " destroyed message");
                }
                if (Vessels_dropped_landed.ContainsKey(FlightGlobals.ActiveVessel.id))
                {
                    Vessels_dropped_landed.Remove(FlightGlobals.ActiveVessel.id);
                    values_removed++;
                    if (Debug_Active)
                        Debug.Log("#### FMRS: delete " + FlightGlobals.ActiveVessel.id.ToString() + " landed message");
                }

                write_save_values_to_file(values_removed);
            }

            if ((windowPos.x == 0) && (windowPos.y == 0))
            {
                windowPos = new Rect(Screen.width / 2, Screen.height / 2, 10, 10);
                write_save_values_to_file();
            }
            
            Vessels.Clear();
            fill_Vessels_list();

            timer_active = false;
            really_close = false;
            really_close_old = false;
            revert_to_launch = false;
            revert_to_launch_old = false;
            undocked_vessel = false;
            staged_vessel = false;
            GameEvents.onStageSeparation.Add(staging_routine);
            GameEvents.onVesselCreate.Add(vessel_create_routine);
            GameEvents.onCollision.Add(crash_handler);
            GameEvents.onCrash.Add(crash_handler);
            GameEvents.onCrashSplashdown.Add(crash_handler);
            GameEvents.onVesselChange.Add(vessel_change_handler);
            GameEvents.OnVesselRecoveryRequested.Add(recovery_requested_handler);
            GameEvents.onGameSceneLoadRequested.Add(scene_change_handler);

            if (!main_ui_active)
            {
                reset_window_size = true;
                RenderingManager.AddToPostDrawQueue(3, new Callback(drawMainGUI));
                main_ui_active = true;
                reset_window_size = true;
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
                Debug.Log("#### FMRS: enter main_vessel_changed(string save_file)");
            if (Debug_Active)
                Debug.Log("#### FMRS: switching main vessel");
            
            ProtoVessel temp_proto;
            Game loadgame = GamePersistence.LoadGame(save_file, HighLogic.SaveFolder + "/FMRS", false, false);

            if (loadgame != null && loadgame.compatible && loadgame.flightState != null)
            {
                temp_proto = loadgame.flightState.protoVessels.Find(p => p.vesselID.ToString() == get_save_value("_SAVE_Main_Vessel"));
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

                set_save_value("_SAVE_Main_Vessel", FlightGlobals.ActiveVessel.id.ToString());
            }
            else
                if (Debug_Active)
                    Debug.Log("#### FMRS: unable to load savefile");

            reset_window_size = true;

            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: leaving main_vessel_changed(string save_file)");
        }


/*************************************************************************************************************************/
        public void vessel_create_routine(Vessel input)
        {
            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: enter vessel_create_routine(Vessel input)");
            if (Debug_Active)
                Debug.Log("#### FMRS: Vessel created");

            Timer_Trigger = Planetarium.GetUniversalTime();
            timer_active = true;
            if (!staged_vessel)
                undocked_vessel = true;

            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: leaving vessel_create_routine(Vessel input)");
        }


/*************************************************************************************************************************/
        public void flight_scene_update_routine()
        {
            if (_SETTING_Enabled)
            {
                if (timer_active)
                {
                    if ((Timer_Trigger + Timer_Delay) <= Planetarium.GetUniversalTime())
                    {
                        if (Debug_Active)
                            Debug.Log("#### FMRS: Has Staged Delayed");

                        timer_active = false;

                        if (undocked_vessel && !staged_vessel)
                        {
                            int nr_save_file = 0;

                            foreach (KeyValuePair<Guid, string> temp_keyvalues in Vessels_dropped)
                            {
                                if (temp_keyvalues.Value.Contains("_undocked_"))
                                    if (nr_save_file <= Convert.ToInt16(temp_keyvalues.Value.Substring(19)))
                                        nr_save_file = Convert.ToInt16(temp_keyvalues.Value.Substring(19)) + 1;
                            }

                            quicksave_file_name = gamesave_name + "_undocked_" + nr_save_file;
                        }
                        else
                        {
                            quicksave_file_name = gamesave_name + FlightGlobals.ActiveVessel.currentStage.ToString();
                            if(Vessels_dropped.ContainsValue(quicksave_file_name))
                            {
                                int nr_save_file = 0;

                                foreach (KeyValuePair<Guid, string> temp_keyvalues in Vessels_dropped)
                                {
                                    if (temp_keyvalues.Value.Contains("_separated_"))
                                        if (nr_save_file <= Convert.ToInt16(temp_keyvalues.Value.Substring(20)))
                                            nr_save_file = Convert.ToInt16(temp_keyvalues.Value.Substring(20)) + 1;
                                }

                                quicksave_file_name = gamesave_name + "_separated_" + nr_save_file;
                            }
                        }

                        undocked_vessel = false;
                        staged_vessel = false;

                        if (search_for_new_vessels(quicksave_file_name))
                        {
                            GamePersistence.SaveGame(quicksave_file_name, HighLogic.SaveFolder + "/FMRS", SaveMode.OVERWRITE);

                            if (get_save_value("_SAVE_Main_Vessel") != FlightGlobals.ActiveVessel.id.ToString() && !_SAVE_Switched_To_Dropped)
                                main_vessel_changed(quicksave_file_name);

                            set_save_value(quicksave_file_name, Planetarium.GetUniversalTime().ToString());
                            write_save_values_to_file();
                        }
                    }
                }

                if (FlightGlobals.ActiveVessel.Landed && FlightGlobals.ActiveVessel.situation != Vessel.Situations.PRELAUNCH
                    && _SAVE_Has_Closed && !n_launchpad_preflight)
                {
                    if (!reset_n_launchpad)
                    {
                        if (Debug_Active)
                            Debug.Log("#### FMRS: activate reset window");

                        reset_window_size = true;

                        if (!reset_ui_active)
                        {
                            windowPos.height = 0;
                            windowPos.width = 0;
                            RenderingManager.AddToPostDrawQueue(3, new Callback(drawResetGUI));
                            reset_ui_active = true;
                            if (Debug_Active)
                                Debug.Log("#### FMRS: activate drawResetGUI");
                        }
                        reset_n_launchpad = true;
                        n_launchpad_preflight = false;
                    }
                }
                else
                {
                    if (reset_n_launchpad)
                    {
                        if (Debug_Active)
                            Debug.Log("#### FMRS: disable reset window");

                        if (reset_ui_active)
                        {
                            RenderingManager.RemoveFromPostDrawQueue(3, new Callback(drawResetGUI));
                            reset_ui_active = false;
                            if (Debug_Active)
                                Debug.Log("#### FMRS: remove drawResetGUI");
                        }

                        reset_n_launchpad = false;
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
        public void drawResetGUI()
        {
            GUI.skin = HighLogic.Skin;
            windowPos = GUILayout.Window(1, windowPos, ResetGUI, "FMRS " + mod_version, GUILayout.MinWidth(100));
        }


/*************************************************************************************************************************/
        public void ResetGUI(int windowID)
        {
            GUIStyle Button_Style = new GUIStyle(GUI.skin.button);
            Button_Style.normal.textColor = Button_Style.focused.textColor = Color.white;
            Button_Style.hover.textColor = Button_Style.active.textColor = Color.yellow;
            Button_Style.onNormal.textColor = Button_Style.onFocused.textColor = Button_Style.onHover.textColor = Button_Style.onActive.textColor = Color.green;
            Button_Style.padding = new RectOffset(3, 4, 4, 4);
            Button_Style.alignment = TextAnchor.MiddleCenter;

            GUILayout.BeginVertical();

            if (GUILayout.Button("Reset", Button_Style, GUILayout.Width(100)))
            {
                if (Debug_Active)
                    Debug.Log("#### FMRS: ActiveVessel is prelaunch, not on Launchpad");

                _SAVE_Has_Closed = false;

                n_launchpad_preflight = true;
                reset_n_launchpad = false;
                if (reset_ui_active)
                {
                    RenderingManager.RemoveFromPostDrawQueue(3, new Callback(drawResetGUI));
                    reset_ui_active = false;
                    if (Debug_Active)
                        Debug.Log("#### FMRS: remove drawResetGUI");
                }

                flight_scene_start_routine();
                
            }

            GUILayout.EndVertical();

            GUI.DragWindow(new Rect(0, 0, 10000, 20));
        }
    }
}
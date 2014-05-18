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
    public class FMRS_Core : MonoBehaviour
    {
        public const string mod_version = "v0.1.02";
        public const string gamesave_name = "FMRS_save";

        public Dictionary<string, string> Save_File_Content = new Dictionary<string, string>();
        public List<Guid> Vessels = new List<Guid>();
        public Dictionary<Guid, string> Vessels_dropped = new Dictionary<Guid, string>();
        public Dictionary<Guid, string> Vessels_dropped_names = new Dictionary<Guid, string>();
        public Dictionary<Guid, bool> Vessels_dropped_landed = new Dictionary<Guid, bool>();
        public Dictionary<Guid, bool> Vessels_dropped_destroyed = new Dictionary<Guid, bool>();
        public Rect windowPos;
        public Vessel dummy_vessel = null;
        public double Timer_Trigger;
        public bool timer_active;
        public bool really_close, minimize_window, really_close_old, minimize_window_old, revert_to_launch, revert_to_launch_old;
        public bool armed;
        public double Timer_Delay = 1;
        public string quicksave_file_name;
        public bool Debug_Active = true;
        public bool Debug_Level_1_Active = false;
        public Guid destr_vessel_id;
        public bool undocked_vessel;

        public Vector2 scroll_Vector = Vector2.zero;


/*************************************************************************************************************************/
        public void WindowGUI(int windowID)
        {
            List<string> save_files = new List<string>();
            string temp_string;
            bool scrollbar_enable = false;
            Vessel temp_vessel;

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

            if (!minimize_window)
            {
                if (get_save_value("_SAVE_Has_Launched") == true.ToString())
                    GUILayout.Box("Mission Time: " + get_time_string(Planetarium.GetUniversalTime() - Convert.ToDouble(get_save_value("_SAVE_Launched_At"))), Text_Style_Main, GUILayout.Width(260));
                else
                    GUILayout.Box("Mission Time: " + get_time_string(0), Text_Style_Main, GUILayout.Width(210));
            }

            if (armed)
                temp_string = "Armed";
            else
                temp_string = "Arm";

            if (get_save_value("_SAVE_Has_Launched") == false.ToString())
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
                windowPos.height = 0;
                windowPos.width = 0;
            }

            if (really_close != really_close_old)
            {
                really_close_old = really_close;
                windowPos.height = 0;
                windowPos.width = 0;
            }

            if (really_close)
            {
                GUILayout.Space(10);
                GUILayout.Box("Close Really?", Text_Style_Heading, GUILayout.Width(310));

                GUILayout.BeginHorizontal();

                if (GUILayout.Button("YES", Button_Style_Big, GUILayout.Width(155)))
                {
                    delete_dropped_vessels();
                    set_save_value("_SAVE_Has_Closed", true.ToString());
                    disable_FMRS();
                    if (FlightGlobals.ActiveVessel.situation != Vessel.Situations.PRELAUNCH && get_save_value("_SAVE_Has_Launched") == true.ToString() && FlightGlobals.ActiveVessel.id.ToString() != get_save_value("_SAVE_Main_Vessel"))
                        jump_to_vessel("Main");
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

                if (save_files.Count > 0)
                {
                    scroll_Vector = GUILayout.BeginScrollView(scroll_Vector, GUILayout.Width(310), GUILayout.Height(300));
                    GUILayout.BeginVertical();
                    scrollbar_enable = true;
                }
                else
                    scrollbar_enable = false;

                while (save_files.Count != 0)
                {

                    GUILayout.Space(5);
                    GUILayout.BeginVertical(areaStyle);
                    if(save_files.Last().Contains("_undocked_"))
                        GUILayout.Box("Undocked at " + get_time_string(Convert.ToDouble(get_save_value(save_files.Last())) - Convert.ToDouble(get_save_value("_SAVE_Launched_At"))), Text_Style_Main, GUILayout.Width(220));
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
                                    windowPos.height = 0;
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

                GUILayout.Space(5);

                if ((get_save_value("_SAVE_Main_Vessel") != FlightGlobals.ActiveVessel.id.ToString()))
                {

                    if (GUILayout.Button("Jump back to Main Mission", Button_Style_Big, GUILayout.Width(310)))
                    {
                        jump_to_vessel("Main");
                    }
                }

                if (get_save_value("_SAVE_Has_Launched") == true.ToString())
                {
                    if (revert_to_launch != revert_to_launch_old)
                    {
                        revert_to_launch_old = revert_to_launch;
                        windowPos.height = 0;
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

            if (get_save_value("_SAVE_Has_Launched") == false.ToString())
            {
                set_save_value("_SAVE_Has_Launched", true.ToString());
                set_save_value("_SAVE_Launched_At", Planetarium.GetUniversalTime().ToString());
            }


            if (!armed)
            {
                disable_FMRS();
                return;
            }

            windowPos.height = 0;

            write_save_values_to_file();

            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: leaving launch_routine(EventReport event_imput)");
        }


/*************************************************************************************************************************/
        public void set_save_value(string key, string value)
        {
            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: entering set_save_value(string key,string value)");

            if (Save_File_Content.ContainsKey(key))
            {
                Save_File_Content[key] = value;
            }
            else
            {
                Save_File_Content.Add(key, value);
            }

            if (Debug_Active)
                Debug.Log("#### FMRS: " + key + " = " + value + " saved");

            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: leaving set_save_value(string key,string value)");
        }


/*************************************************************************************************************************/
        public string get_save_value(string key)
        {
            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: entering get_save_value(string key) #### FMRS: NO LEAVE MESSAGE");

            if (Save_File_Content.ContainsKey(key))
                return (Save_File_Content[key]);
            else
                return (false.ToString());
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
        public void drawGUI()
        {
            GUI.skin = HighLogic.Skin;
            windowPos = GUILayout.Window(1, windowPos, WindowGUI, "FMRS " + mod_version, GUILayout.MinWidth(100));
        }


/*************************************************************************************************************************/
        public void write_save_values_to_file()
        {
            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: entering write_save_values_to_file()");

            set_save_value("_SETTING_Window_X", windowPos.x.ToString());
            set_save_value("_SETTING_Window_Y", windowPos.y.ToString());
            set_save_value("_SETTING_Armed", armed.ToString());
            set_save_value("_SETTING_Minimized", minimize_window.ToString());

            write_vessel_dict_to_Save_File_Content();

            TextWriter file = File.CreateText<FMRS>("save.txt", dummy_vessel);

            foreach (KeyValuePair<string, string> writevalue in Save_File_Content)
            {
                file.WriteLine(writevalue.Key + "=" + writevalue.Value);
            }
            file.Close();

            if (Debug_Active)
                Debug.Log("#### FMRS: Save File written in void write_save_values_to_file()");
            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: leaving write_save_values_to_file()");
        }


/*************************************************************************************************************************/
        public void write_save_values_to_file(int values_removed)
        {
            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: entering write_save_values_to_file(int values_removed)");

            set_save_value("_SETTING_Window_X", windowPos.x.ToString());
            set_save_value("_SETTING_Window_Y", windowPos.y.ToString());

            write_vessel_dict_to_Save_File_Content();

            TextWriter file = File.CreateText<FMRS>("save.txt", dummy_vessel);

            foreach (KeyValuePair<string, string> writevalue in Save_File_Content)
            {
                file.WriteLine(writevalue.Key + "=" + writevalue.Value);
            }
            while (values_removed != 0)
            {
                file.WriteLine("");
                values_removed--;
            }
            file.Close();

            if (Debug_Active)
                Debug.Log("#### FMRS: Save File written in private void write_save_values_to_file(int values_removed)");
            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: leaving save_values_to_file(int values_removed)");
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

                    set_save_value("_SAVE_Switched_To_Dropped", true.ToString());
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
                    set_save_value("_SAVE_Switched_To_Dropped", false.ToString());
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
        public void get_dropped_vessels()
        {
            Guid temp_guid;

            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: entering get_dropped_vessels()");

            foreach (KeyValuePair<string, string> temp_key_pair in Save_File_Content)
            {
                if ((!temp_key_pair.Key.Contains("_SETTING_")) && (!temp_key_pair.Key.Contains("_SAVE_")) && (!temp_key_pair.Key.Contains(gamesave_name)))
                {
                    if (!temp_key_pair.Key.Contains("_NAME_") && !temp_key_pair.Key.Contains("_LANDED_") && !temp_key_pair.Key.Contains("_DESTROYED_"))
                    {
                        if (Debug_Active)
                            Debug.Log(" #### FMRS: try to parse " + temp_key_pair.Key.ToString());

                        temp_guid = new Guid(temp_key_pair.Key);
                        Vessels_dropped.Add(temp_guid, temp_key_pair.Value);

                        if (Debug_Active)
                            Debug.Log(" #### FMRS: " + temp_guid.ToString() + " set to " + temp_key_pair.Value + " in Vessels_dropped");
                    }
                    else
                    {
                        if (temp_key_pair.Key.Contains("_NAME_"))
                        {
                            if (Debug_Active)
                                Debug.Log(" #### FMRS: try to parse " + temp_key_pair.Key.ToString());

                            temp_guid = new Guid(temp_key_pair.Key.Substring(6));
                            Vessels_dropped_names.Add(temp_guid, temp_key_pair.Value);

                            if (Debug_Active)
                                Debug.Log(" #### FMRS: " + temp_guid.ToString() + " set to " + temp_key_pair.Value + " in Vessels_dropped_names");
                        }
                        else if (temp_key_pair.Key.Contains("_LANDED_"))
                        {
                            if (Debug_Active)
                                Debug.Log(" #### FMRS: try to parse " + temp_key_pair.Key.ToString());

                            temp_guid = new Guid(temp_key_pair.Key.Substring(8));
                            Vessels_dropped_landed.Add(temp_guid, Convert.ToBoolean(temp_key_pair.Value));

                            if (Debug_Active)
                                Debug.Log(" #### FMRS: " + temp_guid.ToString() + " set to " + temp_key_pair.Value + " in Vessels_dropped_landed");
                        }
                        else if (temp_key_pair.Key.Contains("_DESTROYED_"))
                        {
                            if (Debug_Active)
                                Debug.Log(" #### FMRS: try to parse " + temp_key_pair.Key.ToString());

                            temp_guid = new Guid(temp_key_pair.Key.Substring(11));
                            Vessels_dropped_destroyed.Add(temp_guid, Convert.ToBoolean(temp_key_pair.Value));

                            if (Debug_Active)
                                Debug.Log(" #### FMRS: " + temp_guid.ToString() + " set to " + temp_key_pair.Value + temp_key_pair.Value + " in Vessels_dropped_destroyed");
                        }
                    }
                }
            }

            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: leaving get_dropped_vessels()");
        }


/*************************************************************************************************************************/
        public void write_vessel_dict_to_Save_File_Content()
        {
            List<string> delete_values = new List<string>();
            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: entering write_vessel_dict_to_Save_File_Content()");

            foreach (KeyValuePair<string, string> save_value in Save_File_Content)
            {
                if (!save_value.Key.Contains("_SAVE_") && !save_value.Key.Contains("_SETTING_") && !save_value.Key.Contains(gamesave_name))
                    delete_values.Add(save_value.Key);
            }

            delete_values.ForEach(delegate(string string_value)
            {
                Save_File_Content.Remove(string_value);
            });

            foreach (KeyValuePair<Guid, string> write_keyvalue in Vessels_dropped)
            {
                set_save_value(write_keyvalue.Key.ToString(), write_keyvalue.Value);
                if (Debug_Active)
                    Debug.Log("#### FMRS: write " + write_keyvalue.Key.ToString() + " to Save_File_Content");
            }

            foreach (KeyValuePair<Guid, string> write_keyvalue in Vessels_dropped_names)
            {
                set_save_value("_NAME_" + write_keyvalue.Key.ToString(), write_keyvalue.Value);
                if (Debug_Active)
                    Debug.Log("#### FMRS: write _NAME_" + write_keyvalue.Key.ToString() + " to Save_File_Content");
            }

            foreach (KeyValuePair<Guid, bool> write_keyvalue in Vessels_dropped_landed)
            {
                set_save_value("_LANDED_" + write_keyvalue.Key.ToString(), write_keyvalue.Value.ToString());
                if (Debug_Active)
                    Debug.Log("#### FMRS: write _LANDED_" + write_keyvalue.Key.ToString() + " to Save_File_Content");
            }

            foreach (KeyValuePair<Guid, bool> write_keyvalue in Vessels_dropped_destroyed)
            {
                set_save_value("_DESTROYED_" + write_keyvalue.Key.ToString(), write_keyvalue.Value.ToString());
                if (Debug_Active)
                    Debug.Log("#### FMRS: write _DESTROYED_" + write_keyvalue.Key.ToString() + " to Save_File_Content");
            }

            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: leaving write_vessel_dict_to_Save_File_Content()");
        }


/*************************************************************************************************************************/
        public void disable_FMRS()
        {
            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: enter disable_mod()");

            write_save_values_to_file();
            RenderingManager.RemoveFromPostDrawQueue(3, new Callback(drawGUI));
            GameEvents.onStageSeparation.Remove(staging_routine);
            GameEvents.onLaunch.Remove(launch_routine);
            GameEvents.onCollision.Remove(crash_handler);
            GameEvents.onCrash.Remove(crash_handler);
            GameEvents.onCrashSplashdown.Remove(crash_handler);
            GameEvents.onVesselChange.Remove(vessel_change_handler);
            GameEvents.OnVesselRecoveryRequested.Remove(recovery_requested_handler);
            GameEvents.onGameSceneLoadRequested.Remove(scene_change_handler);
            GameEvents.onVesselCreate.Remove(vessel_create_routine);

            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: leave disable_mod()");
        }


/*************************************************************************************************************************/
        public string get_time_string(double dtime)
        {
            Int16 s, min, ds;
            Int32 time;
            string return_string;

            dtime *= 100;

            time = Convert.ToInt32(dtime);

            s = 0;
            ds = 0;
            min = 0;

            if (time >= 6000)
            {
                while (time >= 6000)
                {
                    min++;
                    time -= 6000;
                }
            }

            if (time >= 100)
            {
                while (time >= 100)
                {
                    s++;
                    time -= 100;
                }
            }

            if (time >= 10)
            {
                while (time >= 10)
                {
                    ds++;
                    time -= 10;
                }
            }

            if (min < 10)
                return_string = "0" + min.ToString();
            else
                return_string = min.ToString();

            return_string += ":";

            if (s < 10)
                return_string += "0" + s.ToString();
            else
                return_string += s.ToString();

            return_string += "." + ds.ToString();

            return (return_string);
        }


/*************************************************************************************************************************/
        public void flush_save_file()
        {
            int anz_lines;
            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: enter flush_save_file()");

            if (Debug_Active)
                Debug.Log("#### FMRS: flush save file");

            string[] lines = File.ReadAllLines<FMRS>("save.txt", dummy_vessel);
            anz_lines = lines.Length;

            if (Debug_Active)
                Debug.Log("#### FMRS: delete " + anz_lines.ToString() + " lines");

            TextWriter file = File.CreateText<FMRS>("save.txt", dummy_vessel);
            while (anz_lines != 0)
            {
                file.WriteLine("");
                anz_lines--;
            }
            file.Close();

            Save_File_Content.Clear();
            init_save_file();
            read_save_file();

            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: leave flush_save_file()");
        }


/*************************************************************************************************************************/
        public void read_save_file()
        {
            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: enter read_save_file()");

            if (Debug_Active)
                Debug.Log("#### FMRS: read save file");

            Save_File_Content.Clear();
            string[] lines = File.ReadAllLines<FMRS>("save.txt", dummy_vessel);

            for (int i = 0; i < lines.Length; i++)
            {
                string[] line = lines[i].Split('=');
                Save_File_Content.Add(line[0].Trim(), line[1].Trim());
            }

            foreach (KeyValuePair<string, string> readvalue in Save_File_Content)
            {
                if (Debug_Active)
                    Debug.Log(readvalue.Key + "=" + readvalue.Value);
            }

            windowPos.x = Convert.ToInt32(get_save_value("_SETTING_Window_X"));
            windowPos.y = Convert.ToInt32(get_save_value("_SETTING_Window_Y"));
            Debug_Active = Convert.ToBoolean(get_save_value("_SETTING_Debug"));
            if (get_save_value("_SETTING_Debug_Level") == "1" && Debug_Active)
                Debug_Level_1_Active = true;

            if (get_save_value("_SETTING_Armed") == true.ToString())
                armed = true;
            else
                armed = false;

            if (get_save_value("_SETTING_Minimized") == true.ToString())
                minimize_window = true;
            else
                minimize_window = false;
            minimize_window_old = minimize_window;

            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: leave read_save_file()");
        }


/*************************************************************************************************************************/
        public void init_save_file()
        {
            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: enter init_save_file()");

            if (Debug_Active)
                Debug.Log("#### FMRS: init save file");

            set_save_value("_SETTING_Version", mod_version);
            set_save_value("_SETTING_Window_X", windowPos.x.ToString());
            set_save_value("_SETTING_Window_Y", windowPos.y.ToString());
            set_save_value("_SETTING_Enabled", true.ToString());
            set_save_value("_SETTING_Armed", true.ToString());
            set_save_value("_SETTING_Minimized", false.ToString());
            set_save_value("_SETTING_Debug", false.ToString());
            set_save_value("_SETTING_Debug_Level", "0");
            set_save_value("_SAVE_Main_Vessel", "null");
            set_save_value("_SAVE_Launched_At", "null");
            set_save_value("_SAVE_Has_Launched", false.ToString());
            set_save_value("_SAVE_Has_Closed", false.ToString());
            set_save_value("_SAVE_Has_Recovered", false.ToString());
            set_save_value("_SAVE_Recovered_Vessel", "null");
            set_save_value("_SAVE_Switched_To_Dropped", false.ToString());

#if DEBUG
            set_save_value("_SETTING_Debug", true.ToString());
            set_save_value("_SETTING_Debug_Level", "0");

#endif

            Debug_Active = Convert.ToBoolean(get_save_value("_SETTING_Debug"));
            if (get_save_value("_SETTING_Debug_Level") == "1" && Debug_Active)
                Debug_Level_1_Active = true;

            write_save_values_to_file();

            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: leave init_save_file()");
        }


/*************************************************************************************************************************/
        public void recovery_requested_handler(Vessel input)
        {
            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: enter recovery_requested_handler(Vessel input)");
            if (Debug_Active)
                Debug.Log("#### FMRS: Vessel recovery_requested_handler");

            save_landed_vessel();

            set_save_value("_SAVE_Has_Recovered", true.ToString());
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
               

            if (FlightGlobals.ActiveVessel.situation == Vessel.Situations.PRELAUNCH)
            {
                delete_dropped_vessels();

                set_save_value("_SAVE_Has_Launched", false.ToString());
                set_save_value("_SAVE_Main_Vessel", FlightGlobals.ActiveVessel.id.ToString());
                set_save_value("_SAVE_Launched_At", false.ToString());
                set_save_value("_SAVE_Switched_To_Dropped", false.ToString());

                GamePersistence.SaveGame("before_launch", HighLogic.SaveFolder + "/FMRS", SaveMode.OVERWRITE);
            }
            else
            {
                get_dropped_vessels();
            }

            if (FlightGlobals.ActiveVessel.id.ToString() == get_save_value("_SAVE_Main_Vessel"))
                set_save_value("_SAVE_Switched_To_Dropped", false.ToString());

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
            windowPos.height = 0;

            Vessels.Clear();
            fill_Vessels_list();

            timer_active = false;
            really_close = false;
            really_close_old = false;
            revert_to_launch = false;
            revert_to_launch_old = false;
            undocked_vessel = false;
            GameEvents.onStageSeparation.Add(staging_routine);
            GameEvents.onVesselCreate.Add(vessel_create_routine);
            GameEvents.onCollision.Add(crash_handler);
            GameEvents.onCrash.Add(crash_handler);
            GameEvents.onCrashSplashdown.Add(crash_handler);
            GameEvents.onVesselChange.Add(vessel_change_handler);
            GameEvents.OnVesselRecoveryRequested.Add(recovery_requested_handler);
            GameEvents.onGameSceneLoadRequested.Add(scene_change_handler);

            RenderingManager.AddToPostDrawQueue(3, new Callback(drawGUI));

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

            windowPos.height = 0;

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
            undocked_vessel = true;

            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: leaving vessel_create_routine(Vessel input)");
        }


/*************************************************************************************************************************/
        public void flight_scene_update_routine()
        {
            if (timer_active)
            {
                if ((Timer_Trigger + Timer_Delay) <= Planetarium.GetUniversalTime())
                {
                    if (Debug_Active)
                        Debug.Log("#### FMRS: Has Staged Delayed");

                    timer_active = false;

                    if (undocked_vessel)
                    {
                        int nr_save_file = 0;

                        foreach (KeyValuePair<Guid, string> temp_keyvalues in Vessels_dropped)
                        {
                            if (temp_keyvalues.Value.Contains("_undocked_"))
                                if (nr_save_file <= Convert.ToInt16(temp_keyvalues.Value.Substring(19)))
                                    nr_save_file = Convert.ToInt16(temp_keyvalues.Value.Substring(19)) + 1;
                        }

                        quicksave_file_name = gamesave_name + "_undocked_" + nr_save_file;
                        undocked_vessel = false;
                    }
                    else
                        quicksave_file_name = gamesave_name + FlightGlobals.ActiveVessel.currentStage.ToString();

                    if (search_for_new_vessels(quicksave_file_name))
                    {
                        GamePersistence.SaveGame(quicksave_file_name, HighLogic.SaveFolder + "/FMRS", SaveMode.OVERWRITE);

                        if (get_save_value("_SAVE_Main_Vessel") != FlightGlobals.ActiveVessel.id.ToString() && get_save_value("_SAVE_Switched_To_Dropped") == false.ToString())
                            main_vessel_changed(quicksave_file_name);

                        set_save_value(quicksave_file_name, Planetarium.GetUniversalTime().ToString());
                        write_save_values_to_file();
                    }
                }
            }
        }
    }
}

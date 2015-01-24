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
    public partial class FMRS_Core : FMRS_Util , IFMRS
    {
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
                _SETTING_Auto_Cut_Off = GUI.Toggle(new Rect(5, 35 + (30 * 2), 25, 25), _SETTING_Auto_Cut_Off, "Auto Cut Off Engines");
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

            if (delete_vessel && guid_delete_vessel != FlightGlobals.ActiveVessel.id)
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

            //debug_message[0] = "";
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
    }
}

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
    public class FMRS_Util : MonoBehaviour
    {
        public const string mod_version = "v0.1.03";
        public const string gamesave_name = "FMRS_save";

        public List<Guid> Vessels = new List<Guid>();
        public Dictionary<Guid, string> Vessels_dropped = new Dictionary<Guid, string>();
        public Dictionary<Guid, string> Vessels_dropped_names = new Dictionary<Guid, string>();
        public Dictionary<Guid, bool> Vessels_dropped_landed = new Dictionary<Guid, bool>();
        public Dictionary<Guid, bool> Vessels_dropped_destroyed = new Dictionary<Guid, bool>();
        public bool Debug_Active = true;
        public bool Debug_Level_1_Active = false, Debug_Level_2_Active = false;
        public Dictionary<string, string> Save_File_Content = new Dictionary<string, string>();
        public Rect windowPos;
        public bool revert_to_launch = false, revert_to_launch_old = false;
        public bool really_close = false, really_close_old = false;
        public bool minimize_window = false, minimize_window_old = false;
        public bool armed = false;
        public bool _SETTING_Enabled, _SAVE_Has_Launched, _SAVE_Has_Closed, _SAVE_Has_Recovered, _SAVE_Switched_To_Dropped;



/*************************************************************************************************************************/
        public void set_save_value(string key, string value)
        {
            if (Debug_Level_2_Active)
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
                Debug.Log("#### FMRS: set_save_value :" + key + " = " + value);

            if (Debug_Level_2_Active)
                Debug.Log("#### FMRS: leaving set_save_value(string key,string value)");
        }


/*************************************************************************************************************************/
        public string get_save_value(string key)
        {
            if (Debug_Level_2_Active)
                Debug.Log("#### FMRS: entering get_save_value(string key) #### FMRS: NO LEAVE MESSAGE");

            if (Save_File_Content.ContainsKey(key))
                return (Save_File_Content[key]);
            else
                return (false.ToString());
        }


/*************************************************************************************************************************/
        public void write_save_values_to_file()
        {
            Vessel dummy_vessel = null;

            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: entering write_save_values_to_file()");

            set_save_value("_SETTING_Window_X", windowPos.x.ToString());
            set_save_value("_SETTING_Window_Y", windowPos.y.ToString());
            set_save_value("_SETTING_Armed", armed.ToString());
            set_save_value("_SETTING_Minimized", minimize_window.ToString());
            set_save_value("_SETTING_Enabled", _SETTING_Enabled.ToString());
            set_save_value("_SAVE_Has_Launched", _SAVE_Has_Launched.ToString());
            set_save_value("_SAVE_Has_Closed", _SAVE_Has_Closed.ToString());
            set_save_value("_SAVE_Has_Recovered", _SAVE_Has_Recovered.ToString());
            set_save_value("_SAVE_Switched_To_Dropped", _SAVE_Switched_To_Dropped.ToString());

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
            Vessel dummy_vessel = null;

            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: entering write_save_values_to_file(int values_removed)");

            set_save_value("_SETTING_Window_X", windowPos.x.ToString());
            set_save_value("_SETTING_Window_Y", windowPos.y.ToString());
            set_save_value("_SETTING_Armed", armed.ToString());
            set_save_value("_SETTING_Minimized", minimize_window.ToString());
            set_save_value("_SETTING_Enabled", _SETTING_Enabled.ToString());
            set_save_value("_SAVE_Has_Launched", _SAVE_Has_Launched.ToString());
            set_save_value("_SAVE_Has_Closed", _SAVE_Has_Closed.ToString());

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
            Vessel dummy_vessel = null;
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
            Vessel dummy_vessel = null;

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

            if (get_save_value("_SETTING_Debug_Level") == "2" && Debug_Active)
            {
                Debug_Level_1_Active = true;
                Debug_Level_2_Active = true;
            }

            if (get_save_value("_SETTING_Armed") == true.ToString())
                armed = true;
            else
                armed = false;
            
            if (get_save_value("_SETTING_Minimized") == true.ToString())
                minimize_window = true;
            else
                minimize_window = false;
            minimize_window_old = minimize_window;

            _SETTING_Enabled = Convert.ToBoolean(get_save_value("_SETTING_Enabled"));
            _SAVE_Has_Launched = Convert.ToBoolean(get_save_value("_SAVE_Has_Launched"));
            _SAVE_Has_Closed = Convert.ToBoolean(get_save_value("_SAVE_Has_Closed"));
            _SAVE_Has_Recovered = Convert.ToBoolean(get_save_value("_SAVE_Has_Recovered"));
            _SAVE_Switched_To_Dropped = Convert.ToBoolean(get_save_value("_SAVE_Switched_To_Dropped"));

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
        public void load_save_file()
        {
            Vessel dummy_vessel = null;

            if (!File.Exists<FMRS>("save.txt", dummy_vessel))
                init_save_file();
            read_save_file();

            if (get_save_value("_SETTING_Version") != mod_version)
            {
                flush_save_file();
            }
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
    }
}

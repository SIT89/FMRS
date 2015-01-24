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
    public partial class FMRS_Core : FMRS_Util, IFMRS
    {
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

            if (SwitchedToDropped == false)
            {
                if (Debug_Active)
                    Debug.Log("#### FMRS: in Main Save, leaving save_landed_vessel");
                return;
            }

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

            write_recover_file();
            write_save_values_to_file();

            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: leaving save_landed_vessel(bool auto_recover_allowed)");
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
                    foreach (Part p in temp_vessel.Parts)
                    {
                        foreach (PartModule pm in p.Modules)
                        {
                            if (pm.moduleName == "FMRS_PM")
                            {
                                if ((pm as FMRS_PM).parent_vessel != "00000000-0000-0000-0000-000000000000")
                                {
                                    controllable = false;
                                    break;
                                }
                            }
                        }
                        break;
                    }

                    if (controllable)
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
                    GamePersistence.SaveGame("FMRS_main_save", HighLogic.SaveFolder, SaveMode.OVERWRITE);
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

            loadgame = GamePersistence.LoadGame("FMRS_main_save", HighLogic.SaveFolder, false, false);

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

                for (load_vessel = 0; load_vessel < loadgame.flightState.protoVessels.Count && loadgame.flightState.protoVessels[load_vessel].vesselID != vessel_id; load_vessel++) ;
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
                    foreach (ProtoCrewMember member in member_list)
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
    }
}

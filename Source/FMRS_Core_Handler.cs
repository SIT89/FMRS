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

/*************************************************************************************************************************/
        private void attach_handlers()
        {
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
        }


/*************************************************************************************************************************/
        private void detach_handlers()
        {
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
        }


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

            if (!_SAVE_Switched_To_Dropped)
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
                    save_landed_vessel(true);
                }
                else
                    _SETTING_Enabled = false;
            }

            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: leave scene_change_handler(GameScenes input_scene)");
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

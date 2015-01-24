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
                if (pv.landed || pv.splashed)
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
                    message += "Vessel has been damaged!@@";

                if (kerbal_mess != "")
                {
                    message += "Kerbals:@";
                    message += kerbal_mess;
                    message += "@";
                }

                if (HighLogic.CurrentGame.Mode == Game.Modes.CAREER)
                {
                    message += "Funds: +" + Math.Round(total_cost, 2).ToString() + "@";
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
        public void recovery_requested_handler(Vessel vessel_recovered)
        {
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

            save_landed_vessel(true);
            _SAVE_Kick_To_Main = true;
            _SAVE_Switched_To_Dropped = false;

            write_save_values_to_file();

            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: leave recovery_requested_handler(Vessel input)");
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

            return (vesselcost);
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

            foreach (ProtoVessel pv in vessel_list)
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

            return_string = "       ";
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
                Debug.Log("#### FMRS: recovery factor " + factor.ToString());
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

            //recover_values.Clear();
            //write_recover_file();
            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: leave add_recvoered_values_to_save()");
        }
    }
}

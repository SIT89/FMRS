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
using UnityEngine;


namespace FMRS
{
    class FMRS_PM:PartModule
    {
        public string parent_vessel;


/*************************************************************************************************************************/
        public override void OnLoad(ConfigNode node)
        {
            try
            {
                parent_vessel = node.GetValue("parent_vessel");
            }
            catch (Exception)
            {
                parent_vessel = "00000000-0000-0000-0000-000000000000";
            }
        }


/*************************************************************************************************************************/
        public override void OnSave(ConfigNode node)
        {
            node.AddValue("MM_DYNAMIC", "true");
            node.AddValue("parent_vessel", parent_vessel);
        }

        
/*************************************************************************************************************************/
        public void setid()
        {
            parent_vessel = this.vessel.id.ToString();
        }


/*************************************************************************************************************************/
        public void resetid()
        {
            parent_vessel = "00000000-0000-0000-0000-000000000000";
        }
    }
}
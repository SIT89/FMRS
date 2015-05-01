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
using Contracts;


namespace FMRS
{
    public interface IFMRS
    {
        string FMRS_Version
        {
            get;
        }
        bool Active
        {
            get;
        }
        bool SwitchedToDropped
        {
            get;
        }
        List<Guid> VesselList
        {
            get;
        }

        //event ClickHandler OnClick;
    }





/*************************************************************************************************************************/
/*************************************************************************************************************************/
/*************************************************************************************************************************/
/*************************************************************************************************************************/
/*************************************************************************************************************************/
    public partial class FMRS_Core : FMRS_Util, IFMRS
    {
/*************************************************************************************************************************/
        public static IFMRS Instance
        {
            get;
            private set;
        }


/*************************************************************************************************************************/
        public string FMRS_Version
        {
            get
            {
                return version_number;
            }
        }


/*************************************************************************************************************************/
        public bool Active
        {
            get
            {
                return _SAVE_Has_Launched & _SETTING_Enabled;
            }
        }


/*************************************************************************************************************************/
        public bool SwitchedToDropped
        {
            get
            {
                return _SAVE_Switched_To_Dropped;
            }
        }


/*************************************************************************************************************************/
        public List<Guid> VesselList
        {
            get
            {
                List<Guid> return_list = new List<Guid>();
                foreach (Guid id in Vessels_dropped.Keys)
                    return_list.Add(id);
                return return_list;
            }
        }
    }
}

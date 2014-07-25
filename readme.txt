[0.24.2] Flight Manager for Reusable Stages [FMRS] v0.1.04
==========================================================


Description
===========

	FMRS lets you actually fly back your launch stages.
	Fly them back to the launch site or just let them come down on parachutes.
	After landing or splash down, switch back to your main vessel and continue your mission without losing your boosters.
	If you want to do your launches SpaceX's Falcon 9 style, this is your plugin!

	FMRS supports the Blizzy78 Toolbar Plugin.
	http://forum.kerbalspaceprogram.com/threads/60863-0-23-5-Toolbar-1-7-1-Common-API-for-draggable-resizable-buttons-toolbar


Instruction
============

	Attach probe cores to your stages which you would like to bring back.
	Go to the launch pad and arm FMRS.
	Launch you mission as usual.
	Every separated stage which is controllable will be listed in the window of the plugin.
	After reaching orbit you can switch back to the stage separations and control now your reusable stages.
	Bring them back down to Kerbin.
	After that switch back to your main mission or to other Stages.
	When all your stages are landed close the FMRS window and continue your mission.


Installation
============

	Copy the FMRS Folder into the GameData Folder of you KSP install.


Known issues
============

	Recovering your Stages immediately after landing is currently not as stable as it should be.
		I recommend to do all the landings and then switch back to the main mission.
		Close the plugin and after that go to the tracking station and recover your launch vehicles.

	Recovering your Stages immediately after landing will lead into science and fund loss.

	KSP Bugs
	Loading nearby vessels in atmosphere.
	http://bugs.kerbalspaceprogram.com/issues/2429
	Loading deployed parachutes.
	http://forum.kerbalspaceprogram.com/threads/75302-Parachutes-lost-on-quickload-in-atmosphere?highlight=quickload


Change log
============

	v0.1.04
		compiled with 0.24.2 x64
		reset window appearance modified (current vessel must be commandable and no flag, kerbal or debris)
		moved toolbar init form Awake() to Start() routine
		changed convertion of the window postion values

	v0.1.03
		the plugin now can be reset, to preflight, if the controlled vessel has landed on a surface
		window height adapts to displayed content
		fixed an issue with the manual activation of decouplers by mouse click
		scene change (other scene than flight scene) now closes the plugin

	v0.1.02
		decoupling with docking ports works now as well

	v0.1.01
		fixed an issue, when the controlled command part is not in the same stage as the root part
	
	v0.1.00
		initial release	


Forum Thread
http://forum.kerbalspaceprogram.com/threads/80292-0-23-5-Flight-Manager-for-Reusable-Stages-%28FMRS%29
Source
https://github.com/SIT89/FMRS
Curse
http://kerbal.curseforge.com/plugins/220566-fmrs-v0-1-00



This plugin is licensed under the MIT license.

/* The MIT License (MIT)
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
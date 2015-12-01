[1.0.5] Flight Manager for Reusable Stages [FMRS] v1.0.01
==========================================================


Description
===========

	FMRS lets you jump back and forth in time.
	It generates save points after separating controllable probes, ship, landers, ...
	So you can launch your mission into space, then jump back in time to the separation and have full control over your dropped vessels.

	You can do SpaceX´s Falcon 9 style launches and fly your first stage back to the launch site.
	Or launch a space plane on the back of a normal plane. Bring the space plan into space and then fly the launch plane back to the runway.
	Drop scientific probes out of flying planes. Let them descend to the ground and do research.

	The separated vessels will be added to your main save after landing, or automatically recovered.
	
	Forum Thread
	http://forum.kerbalspaceprogram.com/threads/80292-0-25-0-Flight-Manager-for-Reusable-Stages-%28FMRS-v0-2-01%29
	Source
	https://github.com/SIT89/FMRS
	Kerbal Stuff
	https://kerbalstuff.com/mod/172/Flight%20Manager%20for%20Reusable%20Stages%20%5BFMRS%5D
	Curse
	http://kerbal.curseforge.com/plugins/220566-fmrs-v0-1-00


Instruction
============

	Every vessel which should be listed needs to have a probe core, command pod or a RealChute on it.
	Go to the launch pad or runway and arm FMRS.
	Launch you mission as usual.
	Separate your boosters, probes, ...
	Every separated vessel which is controllable or has a RealChute will be listed in the window of the plugin.
	Now you can jump back to the time of separation and have full control over these vessels.
	After you have landed, jump to other dropped crafts or go back to your main vessel and close the plugin.


Features
========
	
	Creates save points after separation of controllable vessels and lets you jump back to the separation and control these vessels.
		After you have landed, these vessels will be added to your main save or recovered.

	Recover function.
		You can recover the landed vessel by clicking the in game recover button or use the auto recover function.
		The costs of the vessel will be refunded including a calculated recovery factor.
		All stored scientific data will be added to your main save.
		You will get a message which lists all recovered values of your craft as soon as you jump back to your main vessel.

	Automatic recover of separated and landed vessels (can be enabled in the settings).
		Recovers the landed vessels automatically, before jumping to other crafts

	Automatic engine cut off of separated crafts (can be enabled in the settings).
		FRMS will cut of the engines of separated crafts immediately after separation.

	Killed Kerbal tracking.
		If you kill a Kerbal during flying a dropped craft, you will lose reputation in your main save.

	Toolbar support.
		FMRS supports the stock toolbar and Blizzy78's Toolbar Plugin.
		http://forum.kerbalspaceprogram.com/threads/60863-0-23-5-Toolbar-1-7-1-Common-API-for-draggable-resizable-buttons-toolbar


Installation
============

	Copy the FMRS Folder into the GameData Folder of you KSP install.


Known issues
============

	Transmitting science during flying a dropped vessel won't return you all science points.
	Contracts can´t be completed while controlling a dropped vessel.

	If you encounter any bug, please contact me on the forums.
	Check the "write debug messages to log file" option in the settings, recreate the bug and send me the log file.
	Thank you!


	KSP Bugs
	Loading nearby vessels in atmosphere.
	http://bugs.kerbalspaceprogram.com/issues/2429
	

Change log
============
	v1.0.01
		built for KSP 1.0.5

	v1.0.00
		compatible with KSP 1.0.2

	v0.3.02
		bug fix: Recovering main vessel leads into mission progress loss

	v0.3.01
		compiled with 0.90
		minor fixes to support the update
		bug fix: changes the default text size of the game
		
	v0.3.00
		massive code overhaul of the essential functions (ModuleManager is now required)
		KSP-AVC support added	
		strategy support added (recovery factor)
		messaging system now can be disabled in the settings
		max window height reduced
		some minor bug fixes
		WIP feature ThrustLogger added

	v0.2.03
		serious bug fixed:	switching to spacecenter or trackingstation, while flying a dropped vessel, doesn't kicks you to your main vessel
							also effects the recover button above the altimeter
		decouple detection delay increased
		auto thrust cut off delay decreased
		main menu module added (deletes wrong save values after a game crash)

	v0.2.02
		bug fix: starting plugin with toolbar could lead into loading an old save file
		bug fix: switching to tracking station, while flying dropped stage, doesn't kicks you to main vessel
		bug fix: the disabled texture for the stock toolbar is not loaded
		recovering vessel in sandbox now prints a recovery message
		bounce sup
		sion for the reset button gui added

	v0.2.01
		compiled with 0.25 x64
		stock toolbar support added
		"Jump back to Separation" button is now dependent to the settings in the debug toolbar (Quicksave & Quickload)
		all non staged vessels are all listed as separtated (no more undocked listings)
		recovery message now lists parts and resources
		after separtion the main vessel can be changed for 10 sec ("Focus Next Vessel", "Focus Prev Vessel")
		bug fix: kerbal in main vessel killed, while flying separated stage, leads to rep loss in main save
		bug fix: closing the plugin using the toolbar kicks you not to the main save
		bug fix: reading empty save file entrys = nullreference exception

	v0.2.00
		!!!!!! BEFORE UPDATING FROM v0.1.xx TO v0.2.00 DELETE THE FOLDER "PluginData" INSIDE "GameData\FMRS" !!!!!!!

		settings window added ("s" button next to the "_" button)
		auto engine cut off for dropped vessels added
		(auto) recover function added (returns stored science date an funds)
		tracking for killed kerbals added (kill kerbal while flying dropped craft = reputation loss)
		recovering crafts and killing kerbals generate messages
		changed window style, layout and resizing
		text box for recovered vessels added
		recognizes stages with RealChute as controllable
		window is now clamped inside the screen
		"Revert To Launch" button is now dependent to the settings in the debug toolbar
		save file structure changed
		bug fix: window disappears after decoupling before launch
		bug fix: plugin doesn´t resets after revert to launch (to a not on launch pad launch)
		various smaller tweaks to support the update

	v0.1.04
		compiled with 0.24.2 x64
		reset window appearance modified (current vessel must be commendable and no flag, kerbal or debris)
		moved toolbar init from the Awake() into the Start() routine
		changed conversion of the window position values

	v0.1.03
		the plugin now can be reset, to pre-flight, if the controlled vessel has landed on a surface
		window height adapts to displayed content
		fixed an issue with the manual activation of decouplers by mouse click
		scene change (other scene than flight scene) now closes the plugin

	v0.1.02
		decoupling with docking ports works now as well

	v0.1.01
		fixed an issue, when the controlled command part is not in the same stage as the root part
	
	v0.1.00
		initial release	


Licence
=======

	This plugin is licensed under the MIT license.

	/* The MIT License (MIT)
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
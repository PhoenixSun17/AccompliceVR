
Overview
========

The main scenes in this project:

AVRPilot.unity

 - This scene or the built executable is the main program that runs
 alongside a running SteamVR. It does two things: creates the overlay
 insertion for the avatar of the CoPilot, and also creates the video
 stream. 


AVRCoPilot.unity

 - This scene or the built executable is the main program that runs
 alongside a running SteamVR. It does two things: creates the overlay
 insertion for the avatar of the CoPilot, and also creates the video
 stream.


DummyOverlayer.unity

 - This create a simple window with a box that can be controlled with
 WASD. This just tests the ability to insert an object into a SteamVR
 overlay. Minimal proof of concept that this is possible.

TestOverlayer.unity

 - This is a test scene to insert a Ubiq avatar into an overlay. No streaming.

TestStreamReceiver.unity

 - This is a test video receiver. It put the video on a large board. 

TestStreamSender.unity

 - This is a test video sender. It just copies the video from
 SteamVR. CoPilot is essentially TestStreamReceiver PLUS
 TestOverlayer.


ToDo
====

 - Currently the AVRPilot can only host one remote CoPilot, but
 extending this to multiple avatars would be straightforward.


Bugs
====

 - Does the avatar in the co-pilot actually face the right way? OR is the portal
 put in the wrong position? Could be a SteamVR to Unity conversion.

 - Scale for portal is not quite right, sometimes clips an object is if get very
 close.

 - Two executable problem. I can't run any two of the executables from
 this project from the shell or explorer. The first one exits when the
 second one starts.


 Fixed
 =====

 -  CoPilot crashing on exit. Various conditions for crash, but neither
 TestStreamSender nor TestOverlayer crash on exit (OpenVR was called exit 
 twice)

  - AVRCoPilot needs to reconnect on streams. (Needs testing)

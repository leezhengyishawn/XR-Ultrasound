Scenes:
-----
Assets/Scenes/Lobby - scene containing the "begin" button
Assets/Scenes/VR Ultrasound Room - scene containing the dummy button

Scripts:
-----
Aim.cs - script that makes the robot in the Lobby scene turn its head to face the player

followcam.cs - script that makes the floating screen in VR Ultrasound Room scene always face the player and stay upright and above the ground

SceneChange.cs - script that makes the app go from Lobby scene to Vr Ultrasound Room scene when begin button is selected

SmoothTurnProvider.cs - script that allows player to rotate camera using either left or right vive controller joystick. currently, if they move the joystick, it will turn for 3 seconds in the direction the joystick was moved in. i intend to make it so that it will turn while the joystick is pushed in a certain direction. it's a little motion-sickness inducing but necessary if they don't want to turn around in the real world.

Trigger.cs - silly script that makes a bonk sound when anything hits the dummy's collider
# 1. XR Ultrasound
XR Ultrasound was a contract job I took for ADI Design after I graduated from university. The project was for an American chain of ultrasound clinics that had outlets all around the country. The client wanted a proof of concept for a training tool to allow ultrasound technicians to be trained by doctors/instructors remotely. 

# 2. Overview
The whole project is divided into three parts: 
  1) The VR HTC Vive app that is run by the instructor. 
  2) The AR iPhone app for the ultrasound technician. 
  3) The key generating applet that both the AR and VR apps access in order to use the video streaming. This process is done automatically once both parties connect to the Agora streaming service.

# 3. How to Use
Both ADI VR and ADI AR were created in Unity 2020.3 but the HTC Vive is needed to run the VR app and an LiDAR capable phone (iPhone 14 Plus in this case) is needed to run the AR app.

The technician would first scan the patient using the iPhone's LiDAR to create a point cloud, an approximate model is then created in the VR space for the instructor to interact with. 

The technician would then mount the iPhone on a custom visor to create a pseudo-VR headset. The reasoning to use the iPhone instead of another VR set was that it was more readily available and easier to train. This way, costs would be lower and the only one who needed to know how to use the VR set was the instructor.

When the instructor places the virtual ultrasound transducer on the VR model, a copy would appear in AR on the living patient. That way, the instructor could show at what angle and at what pressure to apply the transducer.

The app uses Photon to synchronize the objects between the apps. Video feed from the iPhone is streamed through Agora. The UX process was streamlined as much as possible so the only things needed was to turn on the app and scan the patient.

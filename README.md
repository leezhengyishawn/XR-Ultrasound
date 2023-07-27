# 1. XR Ultrasound
XR Ultrasound was a contract job I took for ADI Design after I graduated from university. The project was for an American chain of ultrasound clinics that had outlets all around the country. The client wanted a proof of concept for a training tool to allow ultrasound technicians to be trained by doctors/instructors remotely. 

# 2. Overview
The whole project is divided into three parts: 
  1) The VR HTC Vive app that is run by the instructor. 
  2) The AR iPhone app for the ultrasound technician. 
  3) The key generating applet that both the AR and VR apps access in order to use the video streaming. This process is done automatically once both parties connect to the Agora streaming service.

![image](https://github.com/leezhengyishawn/XR-Ultrasound/assets/100258469/ec3aa768-6db5-4f77-ba29-ac920baacbd3)

*A demonstration of the app from the instructors point of view*

# 3. How to Use
Both ADI VR and ADI AR were created in Unity 2020.3 but the HTC Vive is needed to run the VR app and an LiDAR capable phone (iPhone 14 Plus in this case) is needed to run the AR app.

The technician would first scan the patient using the iPhone's LiDAR to create a point cloud, an approximate model is then created in the VR space for the instructor to interact with. 
![point cloud scan](https://github.com/leezhengyishawn/XR-Ultrasound/assets/100258469/ae332af0-fb0b-4106-aed2-445081dd1672)
*Point cloud in progress. Technicians would move the camera around the patient to capture the whole model. The extraneous bits were then removed and a rough skeleton is created and sent to the VR portion.*

The technician would then mount the iPhone on a custom visor to create a pseudo-VR headset. The reasoning to use the iPhone instead of another VR set was that it was more readily available and easier to train. This way, costs would be lower and the only one who needed to know how to use the VR set was the instructor.

When the instructor places the virtual ultrasound transducer on the VR model, a copy would appear in AR on the living patient. That way, the instructor could show at what angle and at what pressure to apply the transducer.
![image](https://github.com/leezhengyishawn/XR-Ultrasound/assets/100258469/0035f863-f408-477d-9c47-3655c522f982)
*Underneath the human models is a mannequin with hitboxes so the transducer doesn't phas through. Both parties do not see this.*

The app uses Photon to synchronize the objects between the apps. Video feed from the iPhone is streamed through Agora. The UX process was streamlined as much as possible so the only things needed was to turn on the app and scan the patient.

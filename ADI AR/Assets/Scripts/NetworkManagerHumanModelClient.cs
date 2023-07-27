using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit;
namespace ADI.XR
{
    public class NetworkManagerHumanModelClient : MonoBehaviourPunCallbacks
    {
        public TextMeshProUGUI connectionStatus;
        public TextMeshProUGUI debugText;

        public GameObject humanModelPrefab;
        public Transform humanModelRoot;

        public Slider sliderScaleHead;
        public Slider sliderScaleChest;
        public Slider sliderScaleStomach;
        public Slider sliderScaleArms;
        public Slider sliderScaleForearms;
        public Slider sliderScaleLegs;
        public Slider sliderTransHead;
        public Slider sliderTransChest;
        public Slider sliderTransStomach;

        GameObject probe = null;
        GameObject probeModel = null;

        Transform localHumanTransform;


        public void Start()
        {
            PhotonNetwork.ConnectUsingSettings();
        }

        public override void OnConnectedToMaster()
        {
            base.OnConnectedToMaster();
            RoomOptions roomOptions = new RoomOptions();
            roomOptions.MaxPlayers = 10;
            roomOptions.IsVisible = true;
            roomOptions.IsOpen = true;
            roomOptions.CleanupCacheOnLeave = false;
            PhotonNetwork.JoinOrCreateRoom("Main Room", roomOptions, TypedLobby.Default);
        }

        public override void OnJoinedRoom()
        {
            base.OnJoinedRoom();
            connectionStatus.text = "CONNECTED";
        }

        public void Update()
        {
            /*if (probe == null && FindObjectOfType<XRGrabInteractable>() != null)
            {
                probe = FindObjectOfType<XRGrabInteractable>().gameObject;
                probe.transform.parent = GameObject.Find("AR Session Origin").transform;
            }*/
            if (probe == null && FindObjectOfType<XRGrabInteractable>() != null)
            {
                probe = FindObjectOfType<XRGrabInteractable>().gameObject;
                probeModel = GameObject.Find("probe");
            }
            else if (probe != null)
            {
                debugText.text = "Pos: " + probe.transform.localPosition.ToString() + "\nRot: " + probe.transform.localEulerAngles.ToString();
            }


        }


        public void SendModelStats()
        {
            photonView.RPC("NetworkSetModelStats", RpcTarget.Others,
                sliderScaleHead.value, sliderScaleChest.value, sliderScaleStomach.value,
                sliderScaleArms.value, sliderScaleForearms.value, sliderScaleLegs.value,
                sliderTransHead.value, sliderTransChest.value, sliderTransStomach.value);

            localHumanTransform = FindObjectOfType<BoneController>().transform;

            probe.transform.SetParent(localHumanTransform);

            UIManager.Instance.ToggleSingleUI(3);
            FindObjectOfType<BoneController>().m_skinnedMeshRenderer.enabled = false;
        }

        [PunRPC]
        public void NetworkSetModelStats(float scaleHead, float scaleChest, float scaleStomach,
                                         float scaleArms, float scaleForearms, float scaleLegs,
                                         float transHead, float transChest, float transStomach)
        {
            GameObject vrHuman = null;
            vrHuman = Instantiate(humanModelPrefab, humanModelRoot);
            vrHuman.transform.localPosition = Vector3.zero;
            vrHuman.transform.localRotation = Quaternion.identity;


            Vector3 newScale = Vector3.one + (Vector3.one * scaleHead);
            GameObject head = GameObject.Find("Head_Collider");
            head.transform.localScale = newScale;
            head.transform.GetChild(0).transform.localScale = new Vector3(1f / newScale.x, 1f / newScale.y, 1f / newScale.z);

            newScale = Vector3.one + (Vector3.one * scaleChest);
            GameObject chest = GameObject.Find("Chest_Collider");
            chest.transform.localScale = newScale;
            chest.transform.GetChild(0).transform.localScale = new Vector3(1f / newScale.x, 1f / newScale.y, 1f / newScale.z);

            newScale = Vector3.one + (Vector3.one * scaleStomach);
            GameObject stomach = GameObject.Find("Stomach_Collider");
            stomach.transform.localScale = newScale;
            stomach.transform.GetChild(0).transform.localScale = new Vector3(1f / newScale.x, 1f / newScale.y, 1f / newScale.z);

            newScale = Vector3.one + (new Vector3(1, 0, 0) * scaleArms);
            GameObject leftArm = GameObject.Find("LeftArm_Collider");
            leftArm.transform.localScale = newScale;
            leftArm.transform.GetChild(0).transform.localScale = new Vector3(1f / newScale.x, 1f / newScale.y, 1f / newScale.z);
            GameObject rightArm = GameObject.Find("RightArm_Collider");
            rightArm.transform.localScale = newScale;
            rightArm.transform.GetChild(0).transform.localScale = new Vector3(1f / newScale.x, 1f / newScale.y, 1f / newScale.z);

            newScale = Vector3.one + (new Vector3(1, 0, 0) * scaleForearms);
            GameObject leftForeArm = GameObject.Find("LeftForearm");
            leftForeArm.transform.localScale = newScale;
            leftForeArm.transform.GetChild(0).transform.localScale = new Vector3(1f / newScale.x, 1f / newScale.y, 1f / newScale.z);
            GameObject rightForeArm = GameObject.Find("RightForearm");
            rightForeArm.transform.localScale = newScale;
            rightForeArm.transform.GetChild(0).transform.localScale = new Vector3(1f / newScale.x, 1f / newScale.y, 1f / newScale.z);

            newScale = Vector3.one + (new Vector3(1, 0, 0) * scaleLegs);
            GameObject leftUpLeg = GameObject.Find("LeftUpLeg_Collider");
            leftUpLeg.transform.localScale = newScale;
            leftUpLeg.transform.GetChild(0).transform.localScale = new Vector3(1f / newScale.x, 1f / newScale.y, 1f / newScale.z);
            GameObject rightUpLeg = GameObject.Find("RightUpLeg_Collider");
            rightUpLeg.transform.localScale = newScale;
            rightUpLeg.transform.GetChild(0).transform.localScale = new Vector3(1f / newScale.x, 1f / newScale.y, 1f / newScale.z);
            GameObject leftLeg = GameObject.Find("LeftLeg");
            leftLeg.transform.localScale = newScale;
            leftLeg.transform.GetChild(0).transform.localScale = new Vector3(1f / newScale.x, 1f / newScale.y, 1f / newScale.z);
            GameObject rightLeg = GameObject.Find("RightLeg");
            rightLeg.transform.localScale = newScale;
            rightLeg.transform.GetChild(0).transform.localScale = new Vector3(1f / newScale.x, 1f / newScale.y, 1f / newScale.z);

            head.transform.localPosition = head.transform.localPosition + new Vector3(1, 0, 0) * transHead;
            chest.transform.localPosition = chest.transform.localPosition + new Vector3(1, 0, 0) * transChest;
            stomach.transform.localPosition = stomach.transform.localPosition + new Vector3(1, 0, 0) * transStomach;
        }

        /// <summary>
        /// The human models on the VR and AR side are not guaranteed
        /// </summary>
        /// <param name="probeLocalPos"></param>
        /// <param name="humanRemotePos"></param>
        [PunRPC]
        public void NetworkSetProbePostion(Vector3 probeLocalPos, Vector3 probeLocalRot,
                                           Vector3 probeModelLocalPos, Vector3 probeModelLocalRot)
        {
            if (FindObjectOfType<BoneController>().transform != null)
            {
                localHumanTransform = FindObjectOfType<BoneController>().transform;
                probe.transform.SetParent(localHumanTransform);
            }

            probe.transform.localPosition = probeLocalPos;
            probe.transform.localEulerAngles = probeLocalRot;

            probeModel.transform.localPosition = probeModelLocalPos;
            probeModel.transform.localEulerAngles = probeModelLocalRot;

            /*
            probe.transform.localEulerAngles = probeLocalRot - humanRemoteRot + localHumanTransform.eulerAngles;
            probe.transform.localPosition = probeLocalPos - humanRemotePos + localHumanTransform.position;
            Debug.Log("Probe " + probeLocalPos + " " + probeLocalRot);
            Debug.Log("Other HUman " + humanRemotePos + " " + humanRemoteRot);*/
        }
    }
}

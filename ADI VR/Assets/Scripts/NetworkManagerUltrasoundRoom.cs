using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace ADI.XR
{
    public class NetworkManagerUltrasoundRoom : MonoBehaviourPunCallbacks
    {
        public Transform probeSpawnPos;
        public GameObject probePrefab;

        public Collider localsphere;
        public Transform localstrengthBarMask;

        public GameObject humanModelPrefab;
        public Transform humanModelRoot;

        public Transform localHumanTransform;

        [HideInInspector] public GameObject probe;
        [HideInInspector] public GameObject probeModel;

        // Start is called before the first frame update
        void Start()
        {
            if (PhotonNetwork.InRoom)
                return;

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
            probe = PhotonNetwork.Instantiate("Probe (Draggable)", probeSpawnPos.position, probeSpawnPos.rotation);
            probe.GetComponentInChildren<CollisionScript>().sphere = localsphere;
            probe.GetComponentInChildren<CollisionScript>().strengthBarMask = localstrengthBarMask;
            probeModel = GameObject.Find("probe");
        }


        [PunRPC]
        public void NetworkSetModelStats(float scaleHead, float scaleChest, float scaleStomach,
                                             float scaleArms, float scaleForearms, float scaleLegs,
                                             float transHead, float transChest, float transStomach)
        {
            if (FindObjectOfType<ADI.XR.BoneController>() != null)
            {
                probe.transform.SetParent(null);
                Destroy(FindObjectOfType<ADI.XR.BoneController>().gameObject);
            }

            GameObject vrHuman = Instantiate(humanModelPrefab, humanModelRoot);

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
            vrHuman.transform.SetParent(null);
            localHumanTransform = vrHuman.transform;
            probe.transform.SetParent(vrHuman.transform);
        }

        [PunRPC]
        public void NetworkSetProbePostion(Vector3 probeLocalPos, Vector3 probeLocalRot, Vector3 probeModelLocalPos, Vector3 probeModelLocalRot)
        {
            //probe.transform.eulerAngles = probeLocalRot - humanRemoteRot + localHumanTransform.eulerAngles;
            //probe.transform.position = probeLocalPos - humanRemotePos + localHumanTransform.position;
        }

        public void CallNetworkSetProbePosition()
        {
            if (localHumanTransform == null)
                return;
            probe.transform.SetParent(FindObjectOfType<ADI.XR.BoneController>().transform);
            photonView.RPC(nameof(NetworkSetProbePostion), RpcTarget.Others,
                           probe.transform.localPosition, probe.transform.localEulerAngles,
                           probeModel.transform.localPosition, probeModel.transform.localEulerAngles);
        }
    }
}
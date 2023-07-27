using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using TMPro;
namespace ADI.XR
{
    public class HumanBodyTracker : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("The Skeleton prefab to be controlled.")]
        GameObject m_SkeletonPrefab;

        [SerializeField]
        [Tooltip("The ARHumanBodyManager which will produce body tracking events.")]
        ARHumanBodyManager m_HumanBodyManager;


        [SerializeField]
        [Range(-10f, 10f)]
        public float xOffset;

        [SerializeField]
        [Range(-10f, 10f)]
        public float yOffset;

        [SerializeField]
        [Range(-10f, 10f)]
        public float zOffset;

        public int humanCount = 0;
        public bool trackHumans = true;

        public TextMeshProUGUI humanPosText;
        public TextMeshProUGUI cameraPosText;
        public TextMeshProUGUI cameraRotText;
        GameObject spawnedSkeleton;

        /// <summary>
        /// Get/Set the <c>ARHumanBodyManager</c>.
        /// </summary>
        public ARHumanBodyManager humanBodyManager
        {
            get { return m_HumanBodyManager; }
            set { m_HumanBodyManager = value; }
        }

        /// <summary>
        /// Get/Set the skeleton prefab.
        /// </summary>
        public GameObject skeletonPrefab
        {
            get { return m_SkeletonPrefab; }
            set { m_SkeletonPrefab = value; }
        }

        Dictionary<TrackableId, BoneController> m_SkeletonTracker = new Dictionary<TrackableId, BoneController>();

        void OnEnable()
        {
            Debug.Assert(m_HumanBodyManager != null, "Human body manager is required.");
            m_HumanBodyManager.humanBodiesChanged += OnHumanBodiesChanged;
        }

        void OnDisable()
        {
            if (m_HumanBodyManager != null)
                m_HumanBodyManager.humanBodiesChanged -= OnHumanBodiesChanged;
        }

        private void Update()
        {
            if (spawnedSkeleton!=null)
            {
                humanPosText.text = $"Ske: {spawnedSkeleton.transform.position.x}, {spawnedSkeleton.transform.position.y}, {spawnedSkeleton.transform.position.z}";
            }
            cameraPosText.text = string.Format("Pos: {0:#.00}, {1:#.00}, {2:#.00}", Camera.main.transform.position.x, Camera.main.transform.position.y, Camera.main.transform.position.z);
            cameraRotText.text = string.Format("Rot: {0:#.00}, {1:#.00}, {2:#.00}", Camera.main.transform.localEulerAngles.x, Camera.main.transform.localEulerAngles.y, Camera.main.transform.localEulerAngles.z);
            //cameraPosText.text = $"Cam: {Camera.main.transform.position.x}, {Camera.main.transform.position.y}, {Camera.main.transform.position.z}";
        }

        void OnHumanBodiesChanged(ARHumanBodiesChangedEventArgs eventArgs)
        {
            if (trackHumans == false)
                return;

            BoneController boneController;

            //Adding the skeleton
            foreach (var humanBody in eventArgs.added)
            {
                if (!m_SkeletonTracker.TryGetValue(humanBody.trackableId, out boneController))
                {
                    Debug.Log($"Adding a new skeleton [{humanBody.trackableId}].");
                    spawnedSkeleton = Instantiate(m_SkeletonPrefab, humanBody.transform);
                    boneController = spawnedSkeleton.GetComponent<BoneController>();

                    spawnedSkeleton.transform.position = spawnedSkeleton.transform.position + new Vector3(xOffset, yOffset, zOffset);

                    m_SkeletonTracker.Add(humanBody.trackableId, boneController);
                }

                boneController.InitializeSkeletonJoints();
                boneController.ApplyBodyPose(humanBody);

                ++humanCount;
            }

            //Updating the skeleton
            foreach (var humanBody in eventArgs.updated)
            {
                if (m_SkeletonTracker.TryGetValue(humanBody.trackableId, out boneController))
                {
                    boneController.ApplyBodyPose(humanBody);

                    boneController.transform.position = humanBody.transform.position + new Vector3(xOffset, yOffset, zOffset);
                }
            }

            foreach (var humanBody in eventArgs.removed)
            {
                Debug.Log($"Removing a skeleton [{humanBody.trackableId}].");
                if (m_SkeletonTracker.TryGetValue(humanBody.trackableId, out boneController))
                {
                    Destroy(boneController.gameObject);
                    m_SkeletonTracker.Remove(humanBody.trackableId);
                }

                --humanCount;

                if (humanCount == 0)
                    spawnedSkeleton = null;
            }
        }
    }
}
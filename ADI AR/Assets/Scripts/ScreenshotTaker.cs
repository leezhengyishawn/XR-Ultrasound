using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ADI.XR;
using TMPro;
public class ScreenshotTaker : MonoBehaviour
{
    public Button takeScreenshotBtn;

    public GameObject captureModelUI;
    public GameObject configureModelUI;
    public GameObject editModelUI;

    public HumanBodyTracker tracker;

    public TextMeshProUGUI humanPosInARSpace;

    int oldCullingMaskVal;

    // Update is called once per frame
    void Update()
    {
        if (tracker.humanCount != 1)
        {
            takeScreenshotBtn.enabled = false;
            takeScreenshotBtn.image.color = Color.red;
        }
        else
        {
            takeScreenshotBtn.enabled = true;
            takeScreenshotBtn.image.color = Color.green;
        }
    }

    public void TakeSnapShot()
    {
        tracker.trackHumans = false;
        UIManager.Instance.ToggleSingleUI(1);

        var skeleton = FindObjectOfType<BoneController>();
        skeleton.transform.parent = null;
        skeleton.enabled = false;

        //oldCullingMaskVal = Camera.main.cullingMask;
        //Camera.main.cullingMask = 0;

        humanPosInARSpace.text = "Model Pos: " + skeleton.transform.localPosition.ToString();
    }

    public void BackToRecordingHuman()
    {
        tracker.trackHumans = true;
        UIManager.Instance.ToggleSingleUI(0);

        //foreach (var skeleon in FindObjectsOfType<BoneController>())
            //Destroy(skeleon.gameObject);
        /*
        Destroy(FindObjectOfType<BoneController>().gameObject);

        var skeleton = FindObjectOfType<BoneController>();
        skeleton.transform.parent = null;
        skeleton.enabled = false;

        Camera.main.cullingMask = oldCullingMaskVal;*/
    }

    public void EditModel(bool enable)
    {
        configureModelUI.SetActive(!enable);
        editModelUI.SetActive(enable);
    }

}

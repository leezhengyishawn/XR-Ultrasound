using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ADI.XR;
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get { return FindObjectOfType<UIManager>(); } }

    public GameObject captureUI;
    public GameObject previewUI;
    public GameObject editUI;
    public GameObject returnToPreviewUI;

    public GameObject[] UI;

    private void Start()
    {
    }

    public void ToggleSingleUI(int index)
    {
        foreach (var go in UI)
            go.SetActive(false);

        UI[index].SetActive(true);

        if (index == 3)
            FindObjectOfType<BoneController>().m_skinnedMeshRenderer.enabled = false;
        else
            FindObjectOfType<BoneController>().m_skinnedMeshRenderer.enabled = true;
    }

    public void ToggleUI(bool tog)
    {
        foreach (var go in UI)
            go.SetActive(tog);
    }

    public void ResetAll()
    {
        var sliders = FindObjectsOfType<UIStatSlider>();
        foreach (var slider in sliders)
            slider.ResetTransform();
    }
}

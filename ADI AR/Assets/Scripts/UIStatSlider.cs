using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class UIStatSlider : MonoBehaviour
{
    public string bodyName;
    public string[] nameOfObjectsToLink;
    [HeaderAttribute("Object References")]
    public List<GameObject> linkedObjects = new List<GameObject>();
    public Text text;
    public Slider slider;

    public Vector3 scalePower; //For limbs we only need to scale x. Set vector to (1,0,0)

    public List<Vector3> origPos;


    private void OnEnable()
    {
        LinkObjects();
    }

    // Update is called once per frame
    void Update()
    {
        text.text = bodyName + ": " + slider.value;
    }

    public void LinkObjects()
    {
        linkedObjects.Clear();
        foreach (string name in nameOfObjectsToLink)
        {
            linkedObjects.Add(GameObject.Find(name));
            origPos.Add(GameObject.Find(name).transform.localPosition);
        }
    }

    public void ScaleObjects()
    {
        foreach(GameObject obj in linkedObjects)
        {
            Vector3 newScale = Vector3.one + (scalePower * slider.value);
            obj.transform.localScale = newScale;
            obj.transform.GetChild(0).transform.localScale = new Vector3(1f / newScale.x, 1f / newScale.y, 1f / newScale.z);
        }
    }

    public void TranslateObject()
    {
        for(int i =0; i < linkedObjects.Count; ++i)
        {
            Vector3 newOffset = (scalePower * slider.value);
            linkedObjects[i].transform.localPosition = origPos[i] + newOffset;
        }
    }


    public void ResetTransform()
    {
        slider.value = 0;
        ScaleObjects();
        TranslateObject();
    }
}

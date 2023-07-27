using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class SceneChange : MonoBehaviour
{
    public void Room()
    {
        SceneManager.LoadScene("VR Ultrasound Room");
    }
    public void Lobby()
    {
        SceneManager.LoadScene("Lobby");
    }
    
}
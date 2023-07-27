using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    //[SerializeField]
    public Text displayText;
    public Button button;
    private Text btnText;

    void Start()
    {
        btnText = button.GetComponentInChildren<Text>();
    }

    // Update is called once per frame
    public void ConnectToServer()
    {
        if (Equals(btnText.text, "Create"))
        {
            PhotonNetwork.LocalPlayer.NickName = "Doctor";
            PhotonNetwork.ConnectUsingSettings();
            displayText.text = "Trying to connect...";
        }
        else if (Equals(btnText.text, "Start"))
        {
            SceneManager.LoadScene("VR Ultrasound Room");
            displayText.text = "Starting session.";
        }
    }

    public override void OnConnectedToMaster()
    {
        displayText.text = "Connected to server. Creating room.";
        base.OnConnectedToMaster();
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 10;
        roomOptions.IsVisible = true;
        roomOptions.IsOpen = true;
        PhotonNetwork.JoinOrCreateRoom("Main Room", roomOptions, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        displayText.text = "Created Main Room. Please start the session when ready.\n\nParticipants:\n";
        displayText.text += PhotonNetwork.LocalPlayer.NickName;
        btnText.text = "Start";
        base.OnJoinedRoom();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        displayText.text += "\n";
        displayText.text += newPlayer.NickName;
        base.OnPlayerEnteredRoom(newPlayer);
    }
}

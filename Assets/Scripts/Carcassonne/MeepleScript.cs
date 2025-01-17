﻿using Photon.Pun;
using UnityEngine;

public class MeepleScript : MonoBehaviourPun
{
    // Start is called before the first frame update
    public Material[] materials = new Material[5];
    public PointScript.Direction direction;
    public TileScript.Geography geography;
    public bool free;

    public int x, z;

    public PlayerScript.Player playerScriptPlayer;

    private void Start()
    {
        free = true;
        x = 0;
        z = 0;
        // id = 1;
    }

    public void createByPlayer(PlayerScript.Player player)
    {
        //free = true;
        playerScriptPlayer = player;
        //playerId = player.getID();
        //GetComponentInChildren<MeshRenderer>().material = player.GetMaterial();
        //GetComponentInChildren<Rigidbody>().useGravity = false;
        //GetComponentInChildren<BoxCollider>().enabled = false;
        //GetComponentInChildren<MeshRenderer>().enabled = false;
        //material = GetComponentInChildren<Renderer>().material;
    }

    public void OnSnapMeeple()
    {
        GameObject.Find("GameController").GetComponent<GameControllerScript>().SetMeepleSnapPos();
    }

    public void assignAttributes(int x, int z, PointScript.Direction direction, TileScript.Geography geography)
    {
        this.direction = direction;
        this.geography = geography;

        this.x = x;
        this.z = z;

        /*
        switch (direction)
        {
            case PointScript.Direction.NORTH:
                this.x = x;
                this.z = z + .5f;
                break;
            case PointScript.Direction.EAST:
                this.x = x + .5f;
                this.z = z;
                break;
            case PointScript.Direction.SOUTH:
                this.x = x;
                this.z = z - .5f;
                break;
            case PointScript.Direction.WEST:
                this.x = x - .5f;
                this.z = z;
                break;
            default:
                this.x = x;
                this.z = z;
                break;
        }
        */
    }

    public void SetMeepleOwner()
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount > 1)
            if (tag == "Meeple 1")
            {
                Debug.Log("PLATER: " + playerScriptPlayer.photonUser.name);
                // Debug.Log("ÄGARE INNAN: " + photonView.Owner.NickName);
                photonView.TransferOwnership(PhotonNetwork.PlayerList[1]);
                // Debug.Log("ÄGARE EFTER: " + photonView.Owner.NickName);
            }
    }
}
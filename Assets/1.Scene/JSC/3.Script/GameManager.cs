using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public enum PlayerType
{
    None,
    VR,
    PC
}

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance = null;

    [Header("None -> VR -> PC")]
    public GameObject[] VRorPC;

    [Header("VRSetting")]
    public GameObject[] VRGameObeject;

    [Header("PCSetting")]
    public GameObject[] PCGameObeject;

    public PlayerType playerType;
    NetworkIdentity meId;
    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject);
        }
/*        else
        {
            Destroy(gameObject);
            return;
        }*/
        
    }
    public override void OnStartClient()
    {
        meId = GetComponent<NetworkIdentity>();
    }

    public void VRCharacter()
    {

        playerType = PlayerType.VR;
        foreach (GameObject obj in PCGameObeject)
        {
            if (obj.activeSelf)
                obj.SetActive(false);
        }
        foreach (GameObject obj in VRGameObeject)
        {
            if (!obj.activeSelf)
                obj.SetActive(true);
        }
        CmdChangeVR(meId.connectionToClient);
        //if (NetworkManager.singleton is ChoiceNetworkManager manager)
        //    manager.CreateVRPhysicalHands();
    }
    public void PCCharacter()
    {
        playerType = PlayerType.PC;
        foreach (GameObject obj in VRGameObeject)
        {
            if (obj.activeSelf)
                obj.SetActive(false);
        }
        foreach (GameObject obj in PCGameObeject)
        {
            if(!obj.activeSelf)
                obj.SetActive(true);
        }
        CmdChangePC(meId.connectionToClient);
    }

    [Command(requiresAuthority = false)]
    public void CmdChangeVR(NetworkConnectionToClient conn)
    {
        if (NetworkManager.singleton is ChoiceNetworkManager manager)
        {
            manager.ReplacePlayer(conn, VRorPC[1]);
        }
    }
    [Command(requiresAuthority = false)]
    public void CmdChangePC(NetworkConnectionToClient conn)
    {

        if (NetworkManager.singleton is ChoiceNetworkManager manager)
        {
            manager.ReplacePlayer(conn, VRorPC[2]);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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

    public readonly SyncList<GameObject> PlayerSyncList = new SyncList<GameObject>();

    public List<GameObject> PlayerList = new List<GameObject>();
    public GameObject LobbyUi;
    public GameObject WinnerUi;
    public Text WinnerText;
    private void Awake()
    {
        if (Instance == null)
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
    public override void OnStartServer()
    {
        base.OnStartServer();
        UpdatePlayerList();
    }
    public override void OnStartClient()
    {
        meId = GetComponent<NetworkIdentity>();
        Debug.Log("OnStartClient");
    }


/*    public void VRCharacter()
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
            if (!obj.activeSelf)
                obj.SetActive(true);
        }
        CmdChangePC(meId.connectionToClient);
    }*/

    [Server]
    public void UpdatePlayerList()
    {
        StartCoroutine(UpdatePlayerList_co());
    }
    IEnumerator UpdatePlayerList_co()
    {
        while (true)
        {
            foreach (GameObject obj in PlayerSyncList)
            {
                if (obj == null)
                    PlayerSyncList.Remove(obj);
            }
            for (int i = 0; i < PlayerList.Count; i++)
            {

                if (i < PlayerSyncList.Count)
                {
                    RpcUpdatePlayerList(i, true);
                }
                else
                {
                    RpcUpdatePlayerList(i, false);
                }
            }
            yield return null;
        }
    }
    [Server]
    public void GameStart()
    {
        for(int i = 0; i < PlayerSyncList.Count; i++)
        {
            
            if (i== 0)
            {
                //VR 캐릭터로 변경
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
                var playerId = PlayerSyncList[i].GetComponent<NetworkIdentity>();
                if (NetworkManager.singleton is ChoiceNetworkManager manager)
                {
                    manager.ReplacePlayer(playerId.connectionToClient, VRorPC[1]);
                }

            }
            else
            {
                //PC 캐릭터로 변경
                //playerType = PlayerType.PC;

                var playerId = PlayerSyncList[i].GetComponent<NetworkIdentity>();
                Debug.Log(PlayerSyncList[i].name);

                //CmdChangePC(playerId.connectionToClient);
                if (NetworkManager.singleton is ChoiceNetworkManager manager)
                {
                    manager.ReplacePlayer(playerId.connectionToClient, VRorPC[2]);
                }
            }
        }
        RpcGameStart();
        StopCoroutine(UpdatePlayerList_co());

    }
    public void ViewPCplayerUI()
    {
        foreach (GameObject obj in VRGameObeject)
        {
            if (obj.activeSelf)
                obj.SetActive(false);
        }
        foreach (GameObject obj in PCGameObeject)
        {
            if (!obj.activeSelf)
                obj.SetActive(true);
        }
    }
    public void ViewWinnerUI(PlayerType Type)
    {
        WinnerUi.SetActive(true);
        CmdViewWinnerUI(Type);
    }
    [Command(requiresAuthority = false)]
    public void CmdViewWinnerUI(PlayerType Type)
    {
        RpcViewWinnerUI(Type);
    }
    [ClientRpc]
    public void RpcViewWinnerUI(PlayerType Type)
    {
        if (Type == PlayerType.VR)
        {
            WinnerText.text = "PC Win!";

        }
        else
        {
            WinnerText.text = "VR Win!";

        }
        //Time.timeScale = 0;
    }

    [ClientRpc]
    public void RpcGameStart()
    {
        LobbyUi.SetActive(false);
    }
    [ClientRpc]
    public void RpcUpdatePlayerList(int i, bool value)
    {
        if (PlayerList[i].activeSelf != value)
            PlayerList[i].SetActive(value);
    }


    [Command(requiresAuthority = false)]
    public void CmdChangeVR(NetworkConnectionToClient conn)
    {

    }
/*    [Command(requiresAuthority = false)]
    public void CmdChangePC(NetworkConnectionToClient conn)
    {

        if (NetworkManager.singleton is ChoiceNetworkManager manager)
        {
            manager.ReplacePlayer(conn, VRorPC[2]);
        }
    }*/
}

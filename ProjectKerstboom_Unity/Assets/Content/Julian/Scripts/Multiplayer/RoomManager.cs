using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using System.IO;

public class RoomManager : MonoBehaviourPunCallbacks
{
    public static RoomManager Instance;

    private void Awake()
    {
        if(Instance == null)
        {
            DontDestroyOnLoad(gameObject);
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }


    public override void OnEnable()
    {
        base.OnEnable();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }



    public override void OnDisable()
    {
        base.OnDisable();
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode arg1)
    {
        // Spawn All the needed thinks as soon as the game scene is loaded
        if (scene.buildIndex == 1)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "GameManager"), transform.position, Quaternion.identity);
            }

            PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Player"), transform.position, Quaternion.identity);
        }

        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Weapon_Hammer"), transform.position + (Vector3.up * 4) + (Vector3.right * 4), Quaternion.identity);
        }
    }

}

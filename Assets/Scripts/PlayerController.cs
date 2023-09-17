using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PlayerController : MonoBehaviourPunCallbacks, IPunObservable
{
    [HideInInspector]
    public int id;

    [Header("Info")]
    public float moveSpeed;
    public float jumpForce;
    public GameObject hatObject;
    public GameObject playerMesh;
    public AudioClip bing;


    [HideInInspector]
    public float curHatTime;

    [Header("Components")]
    public Rigidbody rig;
    public Player photonPlayer;
    //private GameManager gameManager;
   



    [PunRPC]
    public void Initialize( Player player)
    {
         photonPlayer = player;
         id = player.ActorNumber;

        GameManager.instance.players[id - 1] = this;

        if (id == 1)
        {
            GameManager.instance.GiveHat(id, true);
            //was going to change each player to different colors but cup would also change material so no fancy colors ;(
            //gameManager = FindObjectOfType<GameManager>();
           // Material material = gameManager.GetMaterialForPlayer(id);
            //SetPlayerMaterial(material);
        }

        if (!photonView.IsMine)
            rig.isKinematic = true;

    }

    /*
    private void SetPlayerMaterial(Material material)
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>(playerMesh);
        foreach (Renderer renderer in renderers)
        {
            renderer.material = material;
        }
    }
    */

    private void Update()
    {
        if(PhotonNetwork.IsMasterClient)
        {
            if(curHatTime >= GameManager.instance.timeToWin && !GameManager.instance.gameEnded)
            {
                GameManager.instance.gameEnded = true;
                GameManager.instance.photonView.RPC("WinGame", RpcTarget.All, id);

            }
        }

        if (photonView.IsMine)
        {
            Move();

            if (Input.GetKeyDown(KeyCode.Space))
                TryJump();

            if (hatObject.activeInHierarchy)
            {
                curHatTime += Time.deltaTime;
            }
        }


    }

    void Move ()
    {
        float x = Input.GetAxis("Horizontal") * moveSpeed;
        float z = Input.GetAxis("Vertical") * moveSpeed;

        rig.velocity = new Vector3(x, rig.velocity.y, z);

    }

    void TryJump()
    {
        Ray ray = new Ray(transform.position, Vector3.down);

        if(Physics.Raycast(ray, 0.7f))
        {
            rig.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    public void SetHat (bool hasHat)
    {
        hatObject.SetActive(hasHat);
        AudioSource audio = GetComponent<AudioSource>();
        audio.clip = bing;
        audio.Play();

    }

    void OnCollisionEnter(Collision collision)
    {
        if (!photonView.IsMine)
            return;

        if(collision.gameObject.CompareTag("Player"))
        {
            if(GameManager.instance.GetPlayer(collision.gameObject).id == GameManager.instance.playerWithHat)
            {
                if(GameManager.instance.CanGetHat())
                {
                    GameManager.instance.photonView.RPC("GiveHat", RpcTarget.All, id, false);
                }
            }
        }
    }

    public void OnPhotonSerializeView (PhotonStream stream, PhotonMessageInfo info)
    {
        if(stream.IsWriting)
        {
            stream.SendNext(curHatTime);
        }
        else if(stream.IsReading)
        {
            curHatTime = (float)stream.ReceiveNext();
        }
    }
}


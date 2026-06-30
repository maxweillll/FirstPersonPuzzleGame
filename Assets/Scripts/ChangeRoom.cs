using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class ChangeRoom : MonoBehaviour
{
    public GameObject F_tip;
    public bool inRoom;
    public bool canInRoom;
    // Start is called before the first frame update
    void Start()
    {
        F_tip.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (inRoom && canInRoom)
        {
            Debug.Log("切换场景");
            SceneManager.LoadScene("GameOutRoom");
        }
    }

    public void GetInRoomButton(InputAction.CallbackContext context)
    {
        inRoom = context.ReadValueAsButton();
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("按F进入");
        F_tip.SetActive(true);
        canInRoom = true;
    }
    private void OnTriggerExit(Collider other)
    {
        F_tip.SetActive(false);
        canInRoom = false;
    }
}

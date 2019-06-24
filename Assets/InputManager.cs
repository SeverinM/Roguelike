using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    [SerializeField]
    float speedCam;

    [SerializeField]
    Transform cam;

    private void Update()
    {
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            cam.position += Vector3.left * speedCam * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.RightArrow))
        {
            cam.position += Vector3.right * speedCam * Time.deltaTime;
        }

        if(Input.GetKey(KeyCode.UpArrow))
        {
            cam.position += Vector3.up * speedCam * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.DownArrow))
        {
            cam.position += Vector3.down * speedCam * Time.deltaTime;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [SerializeField] private Camera mainCam;
    [SerializeField] private Camera overviewCam;

    [SerializeField] private float camMoveSpeed;
    [SerializeField] private float camScrollSpeed;

    [SerializeField] private GameObject orthographicButton;

    private bool mainCamActive = true;

    private bool mouseIsDown;

    private Camera activeCamera;
    private MazeGenerator generator;

    // Start is called before the first frame update
    void Start()
    {
        generator = GetComponent<MazeGenerator>();
        activeCamera = mainCam;
    }

    // Update is called once per frame
    void Update()
    {
        mouseIsDown = Input.GetMouseButton(0);

        //Drag controls that work if the left mouse button is held down and if the mouse is then moved.
        float mouseX = Input.GetAxis("Mouse X");
        if (mouseIsDown && mouseX != 0)
        {
            activeCamera.transform.Translate(new Vector3(camMoveSpeed * mouseX * Time.deltaTime, 0, 0));
        }
        float mouseY = Input.GetAxis("Mouse Y");
        if (mouseIsDown && mouseY != 0)
        {
            activeCamera.transform.Translate(new Vector3(0, 0, camMoveSpeed * mouseY * Time.deltaTime), Space.World);
        }

        //Controls that allow the user to zoom in and out by scrolling the mousewheel.
        float scroll = Input.mouseScrollDelta.y;
        if (scroll != 0)
        {
            if (activeCamera.orthographic)
            {
                activeCamera.orthographicSize = Mathf.Clamp(activeCamera.orthographicSize + scroll * Time.deltaTime * 100, 2, 100);
                return;
            }
            activeCamera.transform.Translate(new Vector3(0, 0, camScrollSpeed * scroll * Time.deltaTime));
        }
    }
    public void FocusCamera()
    {
        //This code centers the camera on the maze if the Focus Camera button is pressed. The mainCam is at an angle, and gets an additional correction.
        if(activeCamera == mainCam)
        {
            activeCamera.transform.position = new Vector3(generator.MazeWidth / 2, activeCamera.transform.position.y, generator.MazeHeight / 2 - 10);
            return;
        }
        activeCamera.transform.position = new Vector3(generator.MazeWidth / 2, activeCamera.transform.position.y, generator.MazeHeight / 2);
    }

    public void SwapCamera()
    {
        //Swap the cameras by turning one off and the other on. Also controls the orthographicButton, since only the overviewCam can be orthographic.
        mainCamActive = !mainCamActive;
        mainCam.gameObject.SetActive(mainCamActive);
        overviewCam.gameObject.SetActive(!mainCamActive);
        orthographicButton.SetActive(!mainCamActive);
        if (mainCamActive)
        {
            activeCamera = mainCam;
            return;
        }
        activeCamera = overviewCam;
    }

    public void SetOrthographic()
    {
        //Switch the overviewCam between orthographic and non-orthographic. Also set the Y to 15, if it's orthographic (this resolves shading issues if zoomed out too far.)
        overviewCam.orthographic = !overviewCam.orthographic;
        if (overviewCam.orthographic)
            overviewCam.transform.position = new Vector3(overviewCam.transform.position.x, 15, overviewCam.transform.position.z);
    }
}

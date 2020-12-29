using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARLesson : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI systemState,planesState,pointCloudState;
    [SerializeField] private ARPlaneManager arPlaneManager;
    [SerializeField] private ARPointCloudManager arPointCloudManager;
    [SerializeField] private ARSessionOrigin arSessionOrigin;
    [SerializeField] private ARCameraManager arCameraManager;
    [SerializeField] private ARRaycastManager m_RaycastManager;
    [SerializeField] private GameObject robot_Prefab,robot,ballPrefab;
    [SerializeField] private Light dirLight;
    [SerializeField] private Image swatchImage;

    List<ARRaycastHit> m_Hits = new List<ARRaycastHit>();

    void OnEnable()
    {
        ARSession.stateChanged += ARSessionStateChanged;
        arPlaneManager.planesChanged += ARPlanesChanged;
        arPointCloudManager.pointCloudsChanged += ARPointCloudsChanged;
        arCameraManager.frameReceived += ARFrameReceived;
    }

    void OnDisable()
    {
        ARSession.stateChanged -= ARSessionStateChanged;
        arPlaneManager.planesChanged -= ARPlanesChanged;
        arPointCloudManager.pointCloudsChanged -= ARPointCloudsChanged;
        arCameraManager.frameReceived -= ARFrameReceived;
    }

    void Update()
    {
        if (Input.touchCount == 0)
            return;

        if (m_RaycastManager.Raycast(Input.GetTouch(0).position, m_Hits,TrackableType.Planes))
        {
            // Only returns true if there is at least one hit
            HandleRaycast(m_Hits[0]);
        }
    }

    void HandleRaycast(ARRaycastHit hit)
    {
        // Determine if it is a plane
        if ((hit.hitType & TrackableType.Planes) != 0)
        {
            // Look up the plane by id
            var plane = arPlaneManager.GetPlane(hit.trackableId);

            // Do something with 'plane':
            Debug.Log($"Hit a plane with alignment {plane.alignment}");

            if (robot == null)
            {
                robot = GameObject.Instantiate(robot_Prefab, hit.pose.position, hit.pose.rotation);
            }
            else
            {
                robot.transform.position = hit.pose.position;
                robot.transform.rotation = hit.pose.rotation;
            }

        }
        else
        {
            // What type of thing did we hit?
            Debug.Log($"Raycast hit a {hit.hitType}");
        }
    }


    void ARSessionStateChanged(ARSessionStateChangedEventArgs args)
    {
        systemState.SetText(args.state.ToString());
    }

    void ARPlanesChanged(ARPlanesChangedEventArgs args)
    {
        planesState.SetText(args.added.ToString());
    }

    void ARPointCloudsChanged(ARPointCloudChangedEventArgs args)
    {
        pointCloudState.SetText(args.added.ToString());
    }

    void ARFrameReceived(ARCameraFrameEventArgs args)
    {
        var avgBrightness = args.lightEstimation.averageBrightness;
        var avgColorTemperature = args.lightEstimation.averageColorTemperature;
        var colorCorrection = args.lightEstimation.colorCorrection;
        if (colorCorrection != null)
        {
            dirLight.color = colorCorrection.Value;
            swatchImage.color = colorCorrection.Value;
        }

        if (avgColorTemperature != null)
        {
            dirLight.colorTemperature = avgColorTemperature.Value;
        }

        if (avgBrightness != null)
        {
            dirLight.intensity = avgBrightness.Value;
        }

    }

    public void ShootBall()
    {
        GameObject newBall = Instantiate<GameObject>(ballPrefab);
        newBall.transform.position = Camera.main.transform.position;
        Rigidbody rb = newBall.GetComponent<Rigidbody>();
        rb.AddForce(5000 * Camera.main.transform.forward);
    }
}

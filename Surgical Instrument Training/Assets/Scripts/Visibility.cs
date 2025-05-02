using UnityEngine;
using Mediapipe.Tasks.Vision.HandLandmarker;
using System.Collections;
using System.Collections.Generic;
using Vuforia;

public enum InstrumentType
{
    Scissors,
    Scalpel
}


// This code manages the logic for the visibility of each augmentation depending on the hand tracking state and the orientation of the instrument.
public class InstrumentVisibilityManager : MonoBehaviour
{
    [Header("Hand Tracking")]
    public VuforiaHandTracking handTrackingScript; // Vuforia Hand Tracking script

    [Header("3D Hand Rig & World-Space Alignment")]
    public GameObject handRigged;      // rigged_grasping_hands
    public Material blueGhost;         // Material when not aligned
    public Material greenGhost;        // Material when aligned

    [Header("Grip Zone & Orientation")]
    public GameObject orientedInstrument;   // Oriented_Instrument prefab
    public GameObject gripZoneInstrument;   // grip_zone_instrument prefab
    public GameObject modelTarget;          // Model Target
    public GameObject openHandInstance;     // open_hand prefab

    [Header("Visual Arrow Components")]
    public GameObject doubleArrow;   // Double Arrow for orientation instrument
    public GameObject cylinder;      // Cylinder for double arrow
    public GameObject arrowhead1;    // arrowhead_1 
    public GameObject arrowhead2;    // arrowhead_2

    [Header("Arrow Materials")]
    public Material redMaterial;     // Red Colour to apply to the arrow when not aligned
    public Material greenMaterial;   // Green Colour to apply to the arrow when aligned


    [Header("Dynamic Guideline")]
    public LineRenderer dynamicLine;           // LineRenderer for dynamic line
    public Transform  openHandTransform;      // Open surgeon's hand instance


    [Header("Orientation Mode")]
    public InstrumentType instrumentType = InstrumentType.Scissors; // Setting for the instrument type
    [Tooltip("Cosine threshold for deciding correct orientation (e.g. >0.9)")]
    public float orientationThreshold = 0.9f; // Change this value to adjust the orientation threshold

    void Start()
    {
        // Initial visibility setup
        gripZoneInstrument.SetActive(true);
        handRigged.SetActive(true);
        orientedInstrument.SetActive(false);
        dynamicLine.enabled = false;
    }

    private bool _drawLine = false;

    void Update()
    {
        // For the hand tracking, we check if the hand is aligned with the instrument
        if (handTrackingScript != null 
            && handRigged != null
            && handRigged.activeSelf)
        {
            var result = handTrackingScript.LatestResult;

            if (result.handLandmarks != null && result.handLandmarks.Count > 0)
            {
                // aligned: show green ghost and schedule hide
                handRigged.GetComponent<Renderer>().material = greenGhost;
                // Hide the grip zone after a delay of 2 seconds
                StartCoroutine(HideGripZoneAfterDelay(2f));
            }
            else
            {
                // not tracked: keep blue ghost
                handRigged.GetComponent<Renderer>().material = blueGhost;
            }
        }


        // Logic for drawing the dynamic line when the screen is tapped
        if (Input.GetMouseButtonDown(0))
        {
            _drawLine = true;
            orientedInstrument.SetActive(true);
            dynamicLine.enabled = true;
        }

        // If drawing is enabled, update the line endpoints
        if (_drawLine && dynamicLine != null && orientedInstrument != null && openHandTransform != null)
        {
            dynamicLine.positionCount = 2;
            dynamicLine.SetPosition(0, orientedInstrument.transform.position);
            dynamicLine.SetPosition(1, openHandTransform.position);
        }

        // Orientation checking for the instrument and changing the arrow color based on the orientation
        if (orientedInstrument.activeSelf)
        {
            Vector3 fwd = orientedInstrument.transform.forward;
            bool correct = false;

            switch (instrumentType)
            {
                case InstrumentType.Scissors:
                    // point vertically upwards
                    correct = Vector3.Dot(fwd, Vector3.up) > orientationThreshold;
                    break;

                case InstrumentType.Scalpel:
                    // point horizontally backwards
                    correct = Vector3.Dot(fwd, Vector3.forward) > orientationThreshold;
                    break;
            }

            ChangeArrowColor(correct ? greenMaterial : redMaterial);
        }
    }

    // Logic for changing the visibility of the grip zone and hand rigged
    private IEnumerator HideGripZoneAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (gripZoneInstrument.activeSelf || handRigged.activeSelf)
        {
            gripZoneInstrument.SetActive(false);
            handRigged.SetActive(false);
        }
        orientedInstrument.SetActive(true);
    }

    // Logic for changing the color of the arrow based on the orientation
    private void ChangeArrowColor(Material newMaterial)
    {
        if (cylinder != null)
            cylinder.GetComponent<Renderer>().material = newMaterial;
        if (arrowhead1 != null && arrowhead1.transform.childCount > 0)
            arrowhead1.transform.GetChild(0).GetComponent<Renderer>().material = newMaterial;
        if (arrowhead2 != null && arrowhead2.transform.childCount > 0)
            arrowhead2.transform.GetChild(0).GetComponent<Renderer>().material = newMaterial;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Niantic.ARDK.AR;
using Niantic.ARDK.Utilities;
using Niantic.ARDK.AR.ARSessionEventArgs;
using Niantic.ARDK.AR.HitTest;
using Niantic.ARDK.Utilities.Input.Legacy;
public class Occlusion : MonoBehaviour
{
    public Camera ARCamera;
    public GameObject character;
    IARSession session;

    // Start is called before the first frame update
    void Start()
    {
        ARSessionFactory.SessionInitialized += OnSessionInitialized;
    }

    void OnSessionInitialized(AnyARSessionInitializedArgs args)
    {
        ARSessionFactory.SessionInitialized -= OnSessionInitialized;
        session = args.Session;
    }

    // Update is called once per frame
    void Update()
    {
        if (PlatformAgnosticInput.touchCount < 0)
        {
            return;
        }

        var touch = PlatformAgnosticInput.GetTouch(0);
        if(touch.phase==TouchPhase.Began)
        {
            TouchBegan(touch);
        }
        
    }

    private void TouchBegan(Touch touch)
    {
        var currentFrame = session.CurrentFrame;
        if (currentFrame == null) { return; }
        if (ARCamera == null) { return; }

        var hitTestResults = currentFrame.HitTest(ARCamera.pixelWidth, ARCamera.pixelHeight, touch.position, ARHitTestResultType.ExistingPlaneUsingExtent |
            ARHitTestResultType.EstimatedHorizontalPlane);
        if (hitTestResults.Count == 0)
        {
            return;
        }
        character.transform.position = hitTestResults[0].WorldTransform.ToPosition();
        character.transform.LookAt(
            new Vector3(
                currentFrame.Camera.Transform[0, 3],
                character.transform.position.y,
                currentFrame.Camera.Transform[2, 3]
                )
            );


    }
    

}

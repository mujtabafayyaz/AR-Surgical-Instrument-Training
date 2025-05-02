using UnityEngine;
using Mediapipe;
using Mediapipe.Tasks.Vision.HandLandmarker;
using Stopwatch = System.Diagnostics.Stopwatch;
using System.Collections;
using UnityEngine.UI;



// code based on MediaPipe Unity Plugin tutorial: https://github.com/homuler/MediaPipeUnityPlugin/blob/d816caad57340e687f5b351b917d2b52ba417734/docs/Tutorial-Task-API.md
// credit: Homuler - https://github.com/homuler
public class VuforiaHandTracking : MonoBehaviour
{
    public CameraImageAccess cameraImageAccess;
    [SerializeField] private TextAsset modelAsset;

    public HandLandmarkerResult LatestResult { get; private set; }


    private IEnumerator Start()
    {
        if (cameraImageAccess == null)
        {
            Debug.LogError("CameraImageAccess reference not set. Aborting hand tracking.");
            yield break;
        }
        else
        {
            Debug.Log("CameraImageAccess reference found");
        }

        if (modelAsset == null || modelAsset.bytes == null || modelAsset.bytes.Length == 0)
        {
            Debug.LogError("ModelAsset is null or empty. Make sure you assigned the .task file in the Inspector.");
            yield break;
        }
        else
        {
            Debug.Log($"ModelAsset loaded, size = {modelAsset.bytes.Length} bytes");
        }

        // Build HandLandmarkerOptions
        Debug.Log("Creating HandLandmarkerOptions...");
        var options = new HandLandmarkerOptions(
            baseOptions: new Mediapipe.Tasks.Core.BaseOptions(
                Mediapipe.Tasks.Core.BaseOptions.Delegate.GPU,
                modelAssetBuffer: modelAsset.bytes
            ),
            runningMode: Mediapipe.Tasks.Vision.Core.RunningMode.LIVE_STREAM,
            numHands: 1,
            minHandDetectionConfidence: 0.5f,
            minHandPresenceConfidence: 0.5f,
            minTrackingConfidence: 0.5f,
            resultCallback: OnHandLandmarkDetectionOutput
        );
        Debug.Log("HandLandmarkerOptions created: " +
                  $"Delegate=GPU, RunningMode=LIVE_STREAM, NumHands=1, " +
                  $"MinDetectionConf=0.5, MinPresenceConf=0.5, MinTrackingConf=0.5");

        // Create the detector
        Debug.Log("Creating HandLandmarker from options...");
        using var handLandmarker = HandLandmarker.CreateFromOptions(options);
        Debug.Log("HandLandmarker instance created");

        // Prepare timing and frame loop
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        Debug.Log("⏱️ Stopwatch started for timestamping");

        var waitForEndOfFrame = new WaitForEndOfFrame();

        // Grab initial texture reference
        Texture2D textureFrame = null;

        int frameCount = 0;
        while (true)
        {
            frameCount++;
            textureFrame = cameraImageAccess.CameraTexture;
            // Flio
            if (textureFrame == null)
            {
                // Debug.LogWarning($"[Frame {frameCount}] Texture not yet available—skipping this frame.");
                yield return waitForEndOfFrame;
                continue;
            }

            // Debug.Log($"[Frame {frameCount}] Texture size: {textureFrame.width}x{textureFrame.height}");

            // Force re-upload of pixels (sometimes needed to keep data fresh)
            var pixels = textureFrame.GetPixels32();
            // Debug.Log($"[Frame {frameCount}] Retrieved {pixels.Length} pixels");
            textureFrame.SetPixels32(pixels);
            textureFrame.Apply();
            // Debug.Log($"[Frame {frameCount}] textureFrame.Apply() done");

            // Wrap Unity Texture2D into MediaPipe Image
            using var image = new Mediapipe.Image(textureFrame);
            long timestamp = stopwatch.ElapsedMilliseconds;
            // Debug.Log($"[Frame {frameCount}] Calling DetectAsync at timestamp {timestamp}ms");

            // Kick off async detection
            handLandmarker.DetectAsync(image, timestamp);

            yield return waitForEndOfFrame;
        }
    }

    private void OnHandLandmarkDetectionOutput(HandLandmarkerResult result, Mediapipe.Image image, long timestamp)
    {
        // print result;
        // Debug.Log("HandLandmarker result output:");
        // Debug.Log(result);

        LatestResult = result;
    }

    private void OnDestroy()
    {
        Debug.Log("Destroying VuforiaHandTracking instance");
    }
}

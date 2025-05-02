using UnityEngine;
using UnityEngine.UI;
using Vuforia;
using Image = Vuforia.Image;


// This script is responsible for accessing the camera image and displaying it on a UI RawImage component.
// It registers the pixel format with Vuforia, retrieves the camera image, and updates the texture accordingly.
// The script is taken from the Vuforia Engine documentation and adapted for use with instruments
// Reference: https://developer.vuforia.com/library/vuforia-engine/platform-support/camera/working-camera-unity/#use-an-opengl-texture
public class CameraImageAccess : MonoBehaviour
{
    const PixelFormat PIXEL_FORMAT = PixelFormat.RGB888;
    const TextureFormat TEXTURE_FORMAT = TextureFormat.RGB24;

    public RawImage RawImage;

    Texture2D mTexture;
    public Texture2D CameraTexture => mTexture;

    bool mFormatRegistered;

    void Start()
    {
        Debug.Log("CameraImageAccess script started.");

        // Register Vuforia Engine life-cycle callbacks:
        VuforiaApplication.Instance.OnVuforiaStarted += OnVuforiaStarted;
        VuforiaApplication.Instance.OnVuforiaStopped += OnVuforiaStopped;
        if (VuforiaBehaviour.Instance != null)
        {
            Debug.Log("VuforiaBehaviour Instance found, registering OnStateUpdated.");
            VuforiaBehaviour.Instance.World.OnStateUpdated += OnVuforiaUpdated;
        }
        else
        {
            Debug.LogError("VuforiaBehaviour Instance is NULL. Camera tracking might not work.");
        }
    }

    public void OnDestroy()
    {
        Debug.Log("CameraImageAccess script destroyed. Cleaning up.");

        // Unregister Vuforia Engine life-cycle callbacks:
        if (VuforiaBehaviour.Instance != null)
        {
            Debug.Log("Unregistering OnStateUpdated from VuforiaBehaviour.");
            VuforiaBehaviour.Instance.World.OnStateUpdated -= OnVuforiaUpdated;
        }

        VuforiaApplication.Instance.OnVuforiaStarted -= OnVuforiaStarted;
        VuforiaApplication.Instance.OnVuforiaStopped -= OnVuforiaStopped;

        if (VuforiaApplication.Instance.IsRunning)
        {
            Debug.Log("Vuforia is still running. Unregistering format.");
            UnregisterFormat();
        }

        if (mTexture != null)
        {
            Debug.Log("Destroying Texture2D object.");
            Destroy(mTexture);
        }
    }

    private void OnVuforiaStarted()
    {
        PixelFormat pixelFormat = PixelFormat.RGB888;
        bool success = VuforiaBehaviour.Instance.CameraDevice.SetFrameFormat(pixelFormat, true);

        // Vuforia has started, now register camera image format
        if (success)
        {
            Debug.Log("Successfully registered pixel format " + pixelFormat.ToString());
        }
        else
        {
            Debug.LogError(
                "Failed to register pixel format " + pixelFormat.ToString() +
                "\n the format may be unsupported by your device;" +
                "\n consider using a different pixel format.");
        }
    }

    void OnVuforiaStopped()
    {
        Debug.Log("Vuforia stopped. Cleaning up resources.");
        UnregisterFormat();
        if (mTexture != null)
        {
            Debug.Log("Destroying Texture2D after Vuforia stopped.");
            Destroy(mTexture);
        }
    }

    void OnVuforiaUpdated()
    {
        // Debug.Log("Vuforia updated. Trying to retrieve camera image.");

        var image = VuforiaBehaviour.Instance.CameraDevice.GetCameraImage(PIXEL_FORMAT);
        if (image == null || image.Pixels == null || image.Width <= 0 || image.Height <= 0)
        {
            // Debug.LogWarning("Camera image is NULL, EMPTY, or has zero dimensions. Retrying in next frame...");
            return;
        }

        if (mTexture == null || mTexture.width != image.Width || mTexture.height != image.Height)
        {
            // Debug.Log($"Creating new Texture2D of size {image.Width}x{image.Height}");
            mTexture = new Texture2D(image.Width, image.Height, TEXTURE_FORMAT, false);
        }

        // Debug.Log("Attempting to apply image to Texture2D.");

        image.CopyToTexture(mTexture, true);
        mTexture.Apply();

        // Debug.Log("Texture2D updated successfully.");

        RawImage.texture = mTexture;
        RawImage.material.mainTexture = mTexture;
        // Debug.Log("Applied Texture2D to RawImage UI element.");
    }


    void UnregisterFormat()
    {
        Debug.Log($"Unregistering pixel format {PIXEL_FORMAT} from Vuforia Camera.");
        VuforiaBehaviour.Instance.CameraDevice.SetFrameFormat(PIXEL_FORMAT, false);
        mFormatRegistered = false;
    }
}
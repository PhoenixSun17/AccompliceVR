using UnityEngine;
using Valve.VR;
using System.IO;

public class Screenshot : MonoBehaviour
{
    public uint handle = 0;
    public int textureWidth;
    public int textureHeight;
    private Texture2D leftTexture;

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            var error = EVRScreenshotError.None;
            var screenshotFilename = "screenshot.png";
            var previewFilename = "preview.png";
            var screenshotPath = System.IO.Path.Combine(Application.dataPath, screenshotFilename);
            var previewPath = System.IO.Path.Combine(Application.dataPath, previewFilename);

            error = OpenVR.Screenshots.TakeStereoScreenshot(ref handle, previewPath, screenshotPath);

            if (error != EVRScreenshotError.None) {
                Debug.LogWarning("Failed to capture screenshot. Error code: " + error);
                return;
            }

            // Load the PNG file from disk
            byte[] fileData = File.ReadAllBytes(previewPath);

            // Create a new Texture2D object
            Texture2D tex = new Texture2D(2, 2);

            // Load the PNG data into the Texture2D object
            tex.LoadImage(fileData);
            tex.Reinitialize(1024, 1024);

            textureWidth = tex.width;
            textureHeight = tex.height;

            var screenshotFilename2 = "screenshot2.png";
            var screenshotPath2 = System.IO.Path.Combine(Application.dataPath, screenshotFilename2);
            byte[] pngData = tex.EncodeToPNG();

            // Write the PNG data to a file
            File.WriteAllBytes(screenshotPath2, pngData);
        }
    }
}
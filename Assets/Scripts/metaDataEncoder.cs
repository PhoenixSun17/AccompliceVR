using Unity.RenderStreaming;
using UnityEngine;

public class MetadataVideoProcessor : MonoBehaviour
{
    [SerializeField] 
    public VideoStreamSender videoSender;
    public Streamer renderStreamer;
    private Texture2D mirrorTexture;

    void Start()
    {
        //mirrorTexture = renderStreamer.DisplayImage.mainTexture;
        //videoSender.sourceTexture = EmbedMetadata(videoSender.sourceTexture);
    }

    void EmbedMetadata(Texture2D texture)
    {
        //
        Color[] metaPixels = new Color[16 * 16];

        // 
        for (int i = 0; i < metaPixels.Length; i++)
        {
            metaPixels[i] = new Color(
                (i % 16) / 16f,
                (Time.frameCount % 256) / 255f,
                0,
                1 //
            );
        }

        texture.SetPixels(
            texture.width - 16, 0,
            16, 16,
            metaPixels
        );
        texture.Apply();
    }
}

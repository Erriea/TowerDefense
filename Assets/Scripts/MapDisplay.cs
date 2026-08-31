using UnityEngine;

public class MapDisplay : MonoBehaviour
{
    // public Renderer textureRender;
    //
    // public void DrawNoiseMap(float[,] noiseMap)
    // {
    //     int width = noiseMap.GetLength(0);
    //     int height = noiseMap.GetLength(1);
    //     
    //     //Texture2D texture = new Texture2D(width, height);
    //     
    //     if (width <= 0 || height <= 0)
    //     {
    //         Debug.LogError($"Invalid noise map size: {width} x {height}");
    //         return;
    //     }
    //
    //     Texture2D texture = new Texture2D(
    //         width,
    //         height,
    //         TextureFormat.RGBA32,
    //         false
    //     );
    //     
    //     Color[] colourMap = new Color[width * height];
    //     for (int y = 0; y < height; y++)
    //     {
    //         for (int x = 0; x < width; x++)
    //         {
    //             //colourMap[y * width + x] = Color.Lerp(Color.black, Color.white, noiseMap[x, y]); // a percentage color between black and white (perlin noise)
    //             float noiseValue = noiseMap[x, y];
    //
    //             colourMap[y * width + x] =
    //                 Color.Lerp(Color.black, Color.white, noiseValue);
    //         }
    //     }
    //     
    //     texture.SetPixels(colourMap); 
    //     texture.Apply(false);
    //     
    //     textureRender.sharedMaterial.mainTexture = texture; // allows us to see the perlin texture rendered in our unity project (before the game is played)
    //     textureRender.transform.localScale = new Vector3(width, 1, height);
    // }
}

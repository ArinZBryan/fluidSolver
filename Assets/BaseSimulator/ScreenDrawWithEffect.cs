using System.Linq;
using Unity.Collections;
using UnityEngine;

public class ScreenDrawWithEffect : MonoBehaviour
{
    public Color penColor;
    public Color baseColor;

    public int penSize = 1;

    public static Texture2D drawTex;

    int mouseX;
    int mouseY;

    // Start is called before the first frame update
    void Start()
    {

        //force pen texture to always be visible
        if (penColor.a != 1)
        {
            penColor.a = 1;
        }

        drawTex = new Texture2D(Screen.width, Screen.height);
        drawTex.filterMode = FilterMode.Point;

        drawTex.SetPixels(Enumerable.Repeat(baseColor, Screen.width * Screen.height).ToArray());
        drawTex.Apply();
    }

    // Update is called once per frame
    void Update()
    {
        mouseX = (int)Input.mousePosition.x;
        mouseY = (int)Input.mousePosition.y;

        if (Input.GetMouseButton(0))
        {
            drawTex.SetPixels(mouseX, mouseY, penSize, penSize, Enumerable.Repeat(penColor, penSize * penSize).ToArray());
        }

        if (Input.GetMouseButton(1))
        {
            drawTex.SetPixels(mouseX, mouseY, penSize, penSize, Enumerable.Repeat(baseColor, penSize * penSize).ToArray());
            
        }

        //Graphics.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), drawTex);

        Vector4[,] vecArray = textureToVector4s(drawTex.GetPixels(), drawTex.width, drawTex.height);

        drawTex.Apply();
    }

    void OnGUI()
    {
        if (Event.current.type.Equals(EventType.Repaint))
        {
            Graphics.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), drawTex);
        }
    }

    Vector4[,] textureToVector4s(Color[] colors, int width, int height)
    {
        int x, y;
        Vector4[,] ret = new Vector4[width, height];
        for (int i = 0; i < colors.Length; i++)
        {
            x = i % width;
            y = i / width;

            Vector4 curVec = ret[x, y];
            curVec.x = colors[i].r; curVec.y = colors[i].g; curVec.z = colors[i].b; curVec.w = colors[i].a;
        }
        return ret;
    }

}

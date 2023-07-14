using System.Linq;
using UnityEngine;

public class ScreenDraw : MonoBehaviour
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
            drawTex.Apply();
        }

        if (Input.GetMouseButton(1))
        {
            drawTex.SetPixels(mouseX, mouseY, penSize, penSize, Enumerable.Repeat(baseColor, penSize * penSize).ToArray());
            drawTex.Apply();
        }

        Graphics.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), drawTex);

    }

    void OnGUI()
    {
        if (Event.current.type.Equals(EventType.Repaint))
        {
            Graphics.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), drawTex);
        }
    }


}

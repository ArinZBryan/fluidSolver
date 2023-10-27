# Things to learn

## Unity2D
- [x] **Write texture to screen**

This is performed using the following line of code
```c#
UnityEngine.Graphics.DrawTexture(Rect screenRect, Texture2D texture)
```
Note that `UnityEngine` can usually be omitted if included at the top of the file.

- [x] **Generate texture in managed C#**

The code below generates a texture of size `textureWidth`, `textureHeight`. This texture is then filled with the color defined.
```c#
Texture2D texture = new Texture2D(textureWidth, textureHeight);
Color[] pixels = new Color[textureWidth * textureHeight];
for (int i = 0; i < pixels.Length; i++)
{
    pixels[i] = new Color(r,g,b,a);
}
texture.SetPixels(pixels);
texture.Apply();
```
It is important to note that there are colour presets, such as `Color.red`.a


- [x] **Compute Shaders**
	- [x] **Compute Shader Dispatch**
	- [x] **Writing Compute Shaders**
	- [x] **Reading the return value of the shaders**

## Unity3D
- [ ] Generate volume from array
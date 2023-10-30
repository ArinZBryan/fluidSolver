using UnityEngine;

interface ISimulator
{
    public RenderTexture getCurrentTexture();
    public RenderTexture getNextTexture();
    public RenderTexture getGurrentExportableTexture();
}
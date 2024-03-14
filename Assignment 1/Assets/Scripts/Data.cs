using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Data
{
    public List<TextureUV> textureUVs = new List<TextureUV>();
}

[System.Serializable]
public struct TextureUV
{
    public int nameID;
    public float pixelStartX;
    public float pixelStartY;
    public float pixelStartX2;
    public float pixelEndY;
    public float pixelEndX;
    public float pixelStartY2;
    public float pixelEndX2;
    public float pixelEndY2;
}
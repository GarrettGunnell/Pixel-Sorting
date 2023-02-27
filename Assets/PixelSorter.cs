using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PixelSorter : MonoBehaviour {
    public ComputeShader pixelSorter;

    public Texture image;

    public bool useImage = false;

    private RenderTexture maskTex, sortedTex;

    void OnEnable() {
        maskTex = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.R8, RenderTextureReadWrite.Linear);
        maskTex.enableRandomWrite = true;
        maskTex.Create();

        sortedTex = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        sortedTex.enableRandomWrite = true;
        sortedTex.Create();
    }

    void OnDisable() {
        maskTex.Release();
        maskTex = null;

        sortedTex.Release();
        sortedTex = null;
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination) {
        Graphics.Blit(source, destination);
    }
}

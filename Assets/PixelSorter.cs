using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PixelSorter : MonoBehaviour {
    public ComputeShader pixelSorter;

    public Texture image;

    public bool useImage = false;

    [Range(0.0f, 0.5f)]
    public float lowThreshold = 0.2f;
    
    [Range(0.5f, 1.0f)]
    public float highThreshold = 0.8f;

    private RenderTexture maskTex, colorTex, sortedTex;

    private ComputeBuffer testBuffer;

    void RegenerateRenderTextures() {
        maskTex = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.R8, RenderTextureReadWrite.Linear);
        maskTex.enableRandomWrite = true;
        maskTex.Create();

        colorTex = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        colorTex.enableRandomWrite = true;
        colorTex.Create();

        sortedTex = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        sortedTex.enableRandomWrite = true;
        sortedTex.Create();
    }

    void OnEnable() {
        RegenerateRenderTextures();

        testBuffer = new ComputeBuffer(32, 4);

        int[] testArray = {6, 14, 1, 15, 4, 13, 16, 11, 5, 2, 10, 12, 8, 7, 3, 9};

        testBuffer.SetData(testArray);

        pixelSorter.SetBuffer(1, "_NumberBuffer", testBuffer);

        pixelSorter.Dispatch(1, 1, 1, 1);

        testBuffer.GetData(testArray);

        string outString = "";
        for (int i = 0; i < 16; ++i) {
            outString += testArray[i].ToString() + " ";
        }

        Debug.Log(outString);
    }

    void Update() {
        if (Screen.width != maskTex.width)
            RegenerateRenderTextures();
    }

    void OnDisable() {
        testBuffer.Release();
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination) {
        Graphics.Blit(useImage ? image : source, colorTex);

        pixelSorter.SetFloat("_LowThreshold", lowThreshold);
        pixelSorter.SetFloat("_HighThreshold", highThreshold);
        pixelSorter.SetTexture(0, "_Mask", maskTex);
        pixelSorter.SetTexture(0, "_ColorBuffer", colorTex);

        pixelSorter.Dispatch(0, Mathf.CeilToInt(Screen.width / 8.0f),Mathf.CeilToInt(Screen.height / 8.0f), 1);

        Graphics.Blit(colorTex, destination);
    }
}

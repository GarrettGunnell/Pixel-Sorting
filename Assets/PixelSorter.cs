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

    public bool reverseSorting = false;

    private RenderTexture maskTex, colorTex, sortedTex;

    private ComputeBuffer testBuffer, sortedTestBuffer;

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
        /*
        testBuffer = new ComputeBuffer(16, 4);
        sortedTestBuffer = new ComputeBuffer(16, 4);

        int[] testArray = {6, 14, 1, 15, 4, 13, 16, 11, 5, 2, 10, 12, 8, 7, 3, 9};

        testBuffer.SetData(testArray);

        pixelSorter.SetBuffer(3, "_NumberBuffer", testBuffer);
        pixelSorter.SetBuffer(3, "_SortedNumberBuffer", sortedTestBuffer);

        pixelSorter.Dispatch(3, 1, 1, 1);

        sortedTestBuffer.GetData(testArray);

        string outString = "";
        for (int i = 0; i < 16; ++i) {
            outString += testArray[i].ToString() + " ";
        }

        Debug.Log(outString);
        */
    }

    void Update() {
        if (Screen.width != maskTex.width)
            RegenerateRenderTextures();
    }

    void OnDisable() {
        //testBuffer.Release();
        //sortedTestBuffer.Release();
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination) {
        Graphics.Blit(useImage ? image : source, colorTex);

        pixelSorter.SetFloat("_LowThreshold", lowThreshold);
        pixelSorter.SetFloat("_HighThreshold", highThreshold);
        pixelSorter.SetInt("_BufferWidth", source.width);
        pixelSorter.SetInt("_BufferHeight", source.height);
        pixelSorter.SetInt("_ReverseSorting", reverseSorting ? 1 : 0);
        pixelSorter.SetTexture(0, "_Mask", maskTex);
        pixelSorter.SetTexture(0, "_ColorBuffer", colorTex);

        pixelSorter.Dispatch(0, Mathf.CeilToInt(Screen.width / 8.0f),Mathf.CeilToInt(Screen.height / 8.0f), 1);

        pixelSorter.SetTexture(4, "_ColorBuffer", colorTex);
        pixelSorter.SetTexture(4, "_SortedBuffer", sortedTex);

        pixelSorter.Dispatch(4, 1, Screen.height, 1);

        Graphics.Blit(sortedTex, destination);
    }
}

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

    public bool debugMask = false;

    public bool debugSpans = false;

    public bool visualizeSpans = false;

    public bool debugSorting = false;

    public enum SortMode {
        Lightness = 0,
        Saturation,
        Hue,
        Intensity
    } public SortMode sortBy;

    public bool horizontalSorting = false;

    public bool reverseSorting = false;

    private RenderTexture maskTex, spanTex, colorTex, hslTex, sortedTex;

    private ComputeBuffer testBuffer, sortedTestBuffer;

    void RegenerateRenderTextures() {
        maskTex = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.R8, RenderTextureReadWrite.Linear);
        maskTex.enableRandomWrite = true;
        maskTex.Create();

        spanTex = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.RHalf, RenderTextureReadWrite.Linear);
        spanTex.enableRandomWrite = true;
        spanTex.Create();

        colorTex = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
        colorTex.enableRandomWrite = true;
        colorTex.Create();

        hslTex = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
        hslTex.enableRandomWrite = true;
        hslTex.Create();

        sortedTex = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
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
        pixelSorter.SetInt("_BufferWidth", Screen.width);
        pixelSorter.SetInt("_BufferHeight", Screen.height);
        pixelSorter.SetInt("_ReverseSorting", reverseSorting ? 1 : 0);
        pixelSorter.SetInt("_HorizontalSorting", horizontalSorting ? 1 : 0);
        pixelSorter.SetInt("_SortBy", (int)sortBy);
        pixelSorter.SetTexture(0, "_Mask", maskTex);
        pixelSorter.SetTexture(0, "_ColorBuffer", colorTex);

        pixelSorter.Dispatch(0, Mathf.CeilToInt(Screen.width / 8.0f), Mathf.CeilToInt(Screen.height / 8.0f), 1);

        pixelSorter.SetTexture(4, "_ClearBuffer", spanTex);
        pixelSorter.Dispatch(4, Mathf.CeilToInt(Screen.width / 8.0f), Mathf.CeilToInt(Screen.height / 8.0f), 1);

        pixelSorter.SetTexture(5, "_SpanBuffer", spanTex);
        pixelSorter.SetTexture(5, "_Mask", maskTex);

        pixelSorter.Dispatch(5, horizontalSorting ? 1 : Screen.width, horizontalSorting ? Screen.height : 1, 1);

        pixelSorter.SetTexture(4, "_ClearBuffer", sortedTex);
        pixelSorter.Dispatch(4, Mathf.CeilToInt(Screen.width / 8.0f), Mathf.CeilToInt(Screen.height / 8.0f), 1);

        if (visualizeSpans) {
            pixelSorter.SetTexture(6, "_ColorBuffer", colorTex);
            pixelSorter.SetTexture(6, "_SortedBuffer", sortedTex);
            pixelSorter.SetTexture(6, "_SpanBuffer", spanTex);

            pixelSorter.Dispatch(6, Mathf.CeilToInt(Screen.width / 8.0f), Mathf.CeilToInt(Screen.height / 8.0f), 1); 
        } else {
            pixelSorter.SetTexture(7, "_ColorBuffer", colorTex);
            pixelSorter.SetTexture(7, "_HSLBuffer", hslTex);

            pixelSorter.Dispatch(7, Mathf.CeilToInt(Screen.width / 8.0f), Mathf.CeilToInt(Screen.height / 8.0f), 1); 

            pixelSorter.SetTexture(8, "_HSLBuffer", hslTex);
            pixelSorter.SetTexture(8, "_SortedBuffer", sortedTex);
            pixelSorter.SetTexture(8, "_SpanBuffer", spanTex);

            pixelSorter.Dispatch(8, Screen.width, Screen.height, 1);

            pixelSorter.SetTexture(9, "_SortedBuffer", sortedTex);

            pixelSorter.Dispatch(9, Mathf.CeilToInt(Screen.width / 8.0f), Mathf.CeilToInt(Screen.height / 8.0f), 1); 
            
            pixelSorter.SetTexture(10, "_Mask", maskTex);
            pixelSorter.SetTexture(10, "_ColorBuffer", colorTex);
            pixelSorter.SetTexture(10, "_SortedBuffer", sortedTex);
            
            if (!debugSorting)
                pixelSorter.Dispatch(10, Mathf.CeilToInt(Screen.width / 8.0f), Mathf.CeilToInt(Screen.height / 8.0f), 1); 
        }

        if (debugMask)
            Graphics.Blit(maskTex, destination);
        else if (debugSpans)
            Graphics.Blit(spanTex, destination);
        else
            Graphics.Blit(sortedTex, destination);
    }
}

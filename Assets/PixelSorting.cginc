half luminance(half3 color) {
    return dot(color, half3(0.299f, 0.587f, 0.114f));
}

half hash(uint n) {
    // integer hash copied from Hugo Elias
	n = (n << 13U) ^ n;
    n = n * (n * n * 15731U + 0x789221U) + 0x1376312589U;
    return half(n & uint(0x7fffffffU)) / half(0x7fffffff);
}

// Conversions from https://www.chilliant.com/rgb2hsv.html
float Epsilon = 1e-10;
 
float3 RGBtoHCV(in float3 RGB) {
    // Based on work by Sam Hocevar and Emil Persson
    float4 P = (RGB.g < RGB.b) ? float4(RGB.bg, -1.0, 2.0/3.0) : float4(RGB.gb, 0.0, -1.0/3.0);
    float4 Q = (RGB.r < P.x) ? float4(P.xyw, RGB.r) : float4(RGB.r, P.yzx);
    float C = Q.x - min(Q.w, Q.y);
    float H = abs((Q.w - Q.y) / (6 * C + Epsilon) + Q.z);
    return float3(H, C, Q.x);
}

float3 RGBtoHSL(in float3 RGB) {
    float3 HCV = RGBtoHCV(RGB);
    float L = HCV.z - HCV.y * 0.5;
    float S = HCV.y / (1 - abs(L * 2 - 1) + Epsilon);
    return float3(HCV.x, S, L);
}

float3 HUEtoRGB(in float H) {
    float R = abs(H * 6 - 3) - 1;
    float G = 2 - abs(H * 6 - 2);
    float B = 2 - abs(H * 6 - 4);
    return saturate(float3(R,G,B));
}

float3 HSLtoRGB(in float3 HSL) {
    float3 RGB = HUEtoRGB(HSL.x);
    float C = (1 - abs(2 * HSL.z - 1)) * HSL.y;
    return (RGB - 0.5) * C + HSL.z;
}
#ifndef MOUSE_SCREEN_DITHER_INCLUDED
#define MOUSE_SCREEN_DITHER_INCLUDED

float MouseDitherNoise(float2 pixelPosition)
{
    return frac(52.9829189 * frac(dot(pixelPosition, float2(0.06711056, 0.00583715))));
}

void MouseScreenDither_float(
    float4 ScreenPosition,
    float ObjectEnabled,
    float BaseAlpha,
    float RoofAlpha,
    float3 WorldPosition,
    out float Alpha,
    out float AlphaClipThreshold)
{
    float2 screenPixel = ScreenPosition.xy * _ScreenParams.xy;

    float alpha = saturate(BaseAlpha * RoofAlpha);
    float ditherActive = RoofAlpha < 0.999 ? 1.0 : 0.0;

    Alpha = alpha;
    AlphaClipThreshold = lerp(-1.0, MouseDitherNoise(screenPixel), ditherActive);
}

void MouseScreenDither_half(
    half4 ScreenPosition,
    half ObjectEnabled,
    half BaseAlpha,
    half RoofAlpha,
    half3 WorldPosition,
    out half Alpha,
    out half AlphaClipThreshold)
{
    float alpha;
    float threshold;

    MouseScreenDither_float(
        (float4)ScreenPosition,
        (float)ObjectEnabled,
        (float)BaseAlpha,
        (float)RoofAlpha,
        (float3)WorldPosition,
        alpha,
        threshold
    );

    Alpha = (half)alpha;
    AlphaClipThreshold = (half)threshold;
}

#endif

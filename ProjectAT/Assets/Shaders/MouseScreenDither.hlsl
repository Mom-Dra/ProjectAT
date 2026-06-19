#ifndef MOUSE_SCREEN_DITHER_INCLUDED
#define MOUSE_SCREEN_DITHER_INCLUDED

float4 _MouseDitherParams; // x: mouse pixel x, y: mouse pixel y, z: radius px, w: softness px
float _MouseDitherActive;

float4 _PlayerDitherPositionRadius; // xyz: player world position, w: radius world
float4 _PlayerDitherSettings;       // x: active, y: softness world, z: opacity, w: unused

float MouseDitherNoise(float2 pixelPosition)
{
    return frac(52.9829189 * frac(dot(pixelPosition, float2(0.06711056, 0.00583715))));
}

float CalculateRangeMask(float distanceValue, float radius, float softness)
{
    radius = max(radius, 0.0);
    softness = max(softness, 0.0);

    return softness <= 0.001
        ? 1.0 - step(radius, distanceValue)
        : 1.0 - smoothstep(max(radius - softness, 0.0), radius, distanceValue);
}

void MouseScreenDither_float(
    float4 ScreenPosition,
    float3 WorldPosition,
    float ObjectEnabled,
    float BaseAlpha,
    out float Alpha,
    out float AlphaClipThreshold)
{
    float2 screenPixel = ScreenPosition.xy * _ScreenParams.xy;

    float mouseDistance = distance(screenPixel, _MouseDitherParams.xy);
    float mouseMask = CalculateRangeMask(mouseDistance, _MouseDitherParams.z, _MouseDitherParams.w);
    mouseMask *= saturate(_MouseDitherActive);

    float playerDistance = distance(WorldPosition.xz, _PlayerDitherPositionRadius.xz);
    float playerMask = CalculateRangeMask(
        playerDistance,
        _PlayerDitherPositionRadius.w,
        _PlayerDitherSettings.y
    );
    playerMask *= saturate(_PlayerDitherSettings.x);

    float objectEnabled = saturate(ObjectEnabled);
    float influence = max(mouseMask, playerMask) * objectEnabled;

    float opacity = lerp(1.0, saturate(_PlayerDitherSettings.z), influence);

    Alpha = saturate(BaseAlpha * opacity);
    AlphaClipThreshold = lerp(-1.0, MouseDitherNoise(screenPixel), influence);
}

void MouseScreenDither_half(
    half4 ScreenPosition,
    half3 WorldPosition,
    half ObjectEnabled,
    half BaseAlpha,
    out half Alpha,
    out half AlphaClipThreshold)
{
    float alpha;
    float threshold;

    MouseScreenDither_float(
        (float4)ScreenPosition,
        (float3)WorldPosition,
        (float)ObjectEnabled,
        (float)BaseAlpha,
        alpha,
        threshold
    );

    Alpha = (half)alpha;
    AlphaClipThreshold = (half)threshold;
}

#endif

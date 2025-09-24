using Codice.Client.Common.GameUI;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UIElements;

[CustomEditor(typeof(FieldOfViewNetcode))]
public class FieldOfViewNetcodeEditor : Editor
{
    private void OnSceneGUI()
    {
        FieldOfViewNetcode fow = target as FieldOfViewNetcode;
        Handles.color = Color.white;
        Handles.DrawWireArc(fow.transform.position, Vector3.up, Vector3.forward, 360, fow.ViewRadius);

        Vector3 viewAngleA = fow.DirFromLocalAngle(-fow.ViewAngle / 2);
        Vector3 viewAngleB = fow.DirFromLocalAngle(fow.ViewAngle / 2);

        Handles.DrawLine(fow.transform.position, fow.transform.position + viewAngleA * fow.ViewRadius);
        Handles.DrawLine(fow.transform.position, fow.transform.position + viewAngleB * fow.ViewRadius);

        Handles.color = Color.red;
        foreach ((Transform visibleTarget, float distance) in fow.VisibleTargets)
            Handles.DrawLine(fow.transform.position, visibleTarget.position);
    }
}

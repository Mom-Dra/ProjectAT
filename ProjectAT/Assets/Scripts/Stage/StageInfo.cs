using System;
using System.Collections.Generic;
using ProjectAT.Mission;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "StageInfo", menuName = "Scriptable Objects/StageInfo")]
public sealed class StageInfo : ScriptableObject
{
    [Header("Stage Number")]
    [SerializeField, Min(0)] private int stageNumber;
    
    [Header("StageInfo")]
    [SerializeField] private string stageName;
    [SerializeField, TextArea(3,8)] private string stageDescription;
    
    [FormerlySerializedAs("thumbnail")]
    [SerializeField] private Sprite stageThumbnail;

    [Header("Audio")]
    [FormerlySerializedAs("bgm")]
    [SerializeField] private AudioClip stageBasicBgm;

    [Header("Missions")]
    [SerializeField] private MissionData[] stageObjectives = Array.Empty<MissionData>();

    public int StageNumber => stageNumber;
    public string StageName => stageName;
    public string StageDescription => stageDescription;
    public Sprite StageThumbnail => stageThumbnail;
    public AudioClip StageBasicBgm => stageBasicBgm;

    public IReadOnlyList<MissionData> StageObjectives => stageObjectives?? Array.Empty<MissionData>();
}

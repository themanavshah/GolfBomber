using UnityEngine;

[CreateAssetMenu(fileName = "NewBuildingType", menuName = "GolfBomber/Building Type")]
public class BuildingType : ScriptableObject
{
    public string displayName = "Building";
    public int points = 10;
    public AudioClip destroySound;
    [Range(0f, 1f)] public float destroySoundSpatial = 0f;
    public float destroyVolume = 1f;
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class MotionDataImporter : MonoBehaviour
{
    [SerializeField]
    TextAsset _motionDataJson = default;

    public MotionData motionData = default;

    [ContextMenu("ImportMotionDataFromJson")]
    void ImportMotionDataFromJson()
    {
        if (_motionDataJson != null)
        {
            motionData = JsonUtility.FromJson<MotionData>(_motionDataJson.text);
        }
        else
        {
            Debug.Log("MotionDataJson is null");
        }
    }
}

[System.Serializable]
public class MotionData
{
    public float framerate;
    public float totalsecond; 
    public Layer[] layers;
}

[System.Serializable]
public class Layer
{
    public string name;
    public Key[] keys;
}

[System.Serializable]
public class Key
{
    public int frame;
    public Position position;
}

[System.Serializable]
public class Position
{
    public float x;
    public float y;
}

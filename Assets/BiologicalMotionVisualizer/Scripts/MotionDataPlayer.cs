using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class MotionDataPlayer : MonoBehaviour
{
    [Header("Script References")]
    [SerializeField]
    MotionDataImporter _motionDataImporter = default;

    [Header("Private Variables")]
    [Range(0.5f, 5.0f)]
    public float PlaySpeed = 1.0f;
    
    [SerializeField]
    float _playTimer = 0.0f;

    
    [SerializeField]
    bool _isPlay = false;

    [SerializeField]
    public bool isLoop = false;

    [SerializeField]
    bool _isExistMotionData = false;

    public Rect InRange  = new Rect(0, 0, 1920, 1080);
    public Rect OutRange = new Rect(-8.88888888f, -5.0f, 8.88888888f, 5.0f);

    void Start()
    {
        if (_motionDataImporter != null)
        {
            if (_motionDataImporter.motionData != null)
            {
                if (_motionDataImporter.motionData.totalsecond > 0.0f)
                {
                    _isExistMotionData = true;
                }
                else
                {
                    Debug.Log("Total second is zero");
                }
            }
            else
            {
                Debug.Log("Motion data is null.");
            }
        }
        else
        {
            Debug.Log("MotionDataImporter is not assign.");
        }
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Space))
        {
            _isPlay ^= true;
        }

        if (_isExistMotionData == true)
        {
            if (_isPlay == true)
            {               
                _playTimer += Time.deltaTime * PlaySpeed;

                if (_playTimer >= _motionDataImporter.motionData.totalsecond)
                {
                    _playTimer = 0.0f;
                    if (!isLoop)
                    {
                        _isPlay = false;
                    }
                }
            }
        }
    }

    void OnDrawGizmos()
    {
        if (_isExistMotionData)
        {
            var layersNum = _motionDataImporter.motionData.layers.Length;
            for (var i = 0; i < layersNum; i++)
            {
                int frame = Mathf.FloorToInt(_playTimer * _motionDataImporter.motionData.framerate);

                Vector3 pos = new Vector3(
                    _motionDataImporter.motionData.layers[i].keys[frame].position.x,
                    _motionDataImporter.motionData.layers[i].keys[frame].position.y,
                    0.0f                    
                );
                pos.x = Map(pos.x, InRange.x, InRange.width,  OutRange.x, OutRange.width );
                pos.y = Map(pos.y, InRange.y, InRange.height, OutRange.y, OutRange.height);

                Gizmos.DrawSphere(pos, 0.1f);
                
            }
        }
    }

    void OnGUI()
    {
        var sX = 10;
        var sY = 10;
        var dY = 24;
        var r00 = new Rect(sX, sY + dY * 0, 1024, 32);
        var r01 = new Rect(sX, sY + dY * 1, 1024, 32);
        var r02 = new Rect(sX, sY + dY * 2, 1024, 32);
        var r03 = new Rect(sX, sY + dY * 3, 1024, 32);

        GUI.Label(r00, "isExistMotionData : " + _isExistMotionData.ToString());

        if (_isExistMotionData == true)
        {
            GUI.Label(r01, "framerate : " + _motionDataImporter.motionData.framerate.ToString());
            GUI.Label(r02, "totalsecond : " + _motionDataImporter.motionData.totalsecond.ToString());
        }
    }

    public float Map(float x, float in_min, float in_max, float out_min, float out_max, bool clamp = false)
    {
        if (clamp) x = Mathf.Max(in_min, Mathf.Min(x, in_max));
        return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
    }
}

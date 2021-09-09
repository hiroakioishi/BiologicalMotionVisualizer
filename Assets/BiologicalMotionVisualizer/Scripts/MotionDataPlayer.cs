using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using static Unity.Mathematics.math;

[ExecuteAlways]
public class MotionDataPlayer : MonoBehaviour
{
    /// <summary>
    /// 動きのデータ
    /// </summary>
    [Header("Script References")]
    [SerializeField]
    MotionDataImporter _motionDataImporter = default;

    /// <summary>
    /// 再生スピード
    /// </summary>
    [Header("Private Variables")]
    [Range(0.5f, 5.0f)]
    public float PlaySpeed = 1.0f;

    /// <summary>
    /// 再生タイマー
    /// </summary>
    [SerializeField]
    float _playTimer = 0.0f;

    /// <summary>
    /// GLオブジェクト描画のためのマテリアル
    /// </summary>
    Material _glMat = default;

    bool _isExistMotionData = false;
    
    /// <summary>
    /// 再生中かどうか
    /// </summary>
    [Header("Flags")]
    public bool IsPlay = false;    
    /// <summary>
    /// 再生をループさせるかどうか
    /// </summary>
    public bool IsLoop = false;
    /// <summary>
    /// すべてのラインを描画するかどうか
    /// </summary>
    public bool EnableDrawAllLines = true;
    /// <summary>
    /// ボーンを線として描画するかどうか
    /// </summary>
    public bool DrawBoneLine = true;
    /// <summary>
    /// ジョイントを円として描画するかどうか
    /// </summary>
    public bool DrawJointCircle = true;
    /// <summary>
    /// ジョイントを結んだ三角形を描画するかどうか
    /// </summary>
    public bool DrawTriangle = true;
    /// <summary>
    /// ボーンの線の幅
    /// </summary>
    public float BoneLineWidth = 1.0f;
    /// <summary>
    /// ジョイントの円の半径
    /// </summary>
    public float JointCircleRadius = 0.5f;

    /// <summary>
    /// ジョイントの円の色
    /// </summary>
    public Color JointColor    = Color.white;
    /// <summary>
    /// ボーンのラインの色
    /// </summary>
    public Color LineColor     = Color.white;
    /// <summary>
    /// 三角形の色
    /// </summary>
    public Color TriangleColor = Color.white;

    /// <summary>
    /// AfterEffectsでの座標の範囲
    /// </summary>
    [Header("RenderArea")]
    public Rect InRange = new Rect(0, 0, 1920, 1080);
    /// <summary>
    /// ワールド座標の範囲
    /// </summary>
    public Rect OutRange = new Rect(-8.88888888f, -5.0f, 8.88888888f, 5.0f);

    /// <summary>
    /// ボーンのジョイントのペアのリスト
    /// </summary>
    public List<BoneJointPair> BonePairList = new List<BoneJointPair>();
    
    /// <summary>
    /// 三角形の頂点となるジョイントのリスト
    /// </summary>
    public List<TriangleJointPair> TrianglePairList = new List<TriangleJointPair>();

    [Header("Debug")]
    public bool EnableDrawGizmos = false;

    [System.Serializable]
    public class BoneJointPair
    {
        public int Joint0;
        public int Joint1;
    }

    [System.Serializable]
    public class TriangleJointPair
    {
        public int Joint0;
        public int Joint1;
        public int Joint2;
    }

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
            IsPlay ^= true;
        }

        if (_isExistMotionData == true)
        {
            if (IsPlay == true)
            {
                _playTimer += Time.deltaTime * PlaySpeed;

                if (_playTimer >= _motionDataImporter.motionData.totalsecond)
                {
                    _playTimer = 0.0f;
                    if (!IsLoop)
                    {
                        IsPlay = false;
                    }
                }
            }
        }
    }

    void OnRenderObject()
    {
        if (_glMat == null)
        {
            Shader shader = Shader.Find("Hidden/Internal-Colored");
            _glMat = new Material(shader);
            _glMat.hideFlags = HideFlags.HideAndDontSave;
        }

        if (_glMat == null)
            return;

        // フレーム数計算
        int frame = Mathf.FloorToInt(_playTimer * _motionDataImporter.motionData.framerate);

        // ライン描画のためのマテリアルセット
        _glMat.SetPass(0);


        GL.PushMatrix();
        GL.MultMatrix(transform.localToWorldMatrix);

        // 参照
        // https://wiki.unity3d.com/index.php/VectorLine

        // ボーンのラインを描画
        if (DrawBoneLine)
        {
            GL.Begin(GL.QUADS);

            // ラインの色セット
            GL.Color(LineColor);

            if (EnableDrawAllLines)
            {
                var layersNum = _motionDataImporter.motionData.layers.Length;
                for (var i = 0; i < layersNum; i++)
                {
                    for (var j = i; j < layersNum; j++)
                    {
                        var p0 = AEScreenCoordToWorldPos(_motionDataImporter.motionData.layers[i].keys[frame].position);
                        var p1 = AEScreenCoordToWorldPos(_motionDataImporter.motionData.layers[j].keys[frame].position);

                        var perpendicular = (new Vector3(p1.y, p0.x, 0.0f) - new Vector3(p0.y, p1.x, 0.0f)).normalized * BoneLineWidth;
                        var v1 = new Vector3(p0.x, p0.y, 0.0f);
                        var v2 = new Vector3(p1.x, p1.y, 0.0f);

                        GL.Vertex((v1 - perpendicular));
                        GL.Vertex((v1 + perpendicular));
                        GL.Vertex((v2 + perpendicular));
                        GL.Vertex((v2 - perpendicular));
                    }
                }
            }
            else
            {
                var layersNum = _motionDataImporter.motionData.layers.Length;
                for (var i = 0; i < BonePairList.Count; i++)
                {
                    int j0 = BonePairList[i].Joint0;
                    int j1 = BonePairList[i].Joint1;

                    if (j0 < layersNum && j1 < layersNum)
                    {
                        var p0 = AEScreenCoordToWorldPos(_motionDataImporter.motionData.layers[BonePairList[i].Joint0].keys[frame].position);
                        var p1 = AEScreenCoordToWorldPos(_motionDataImporter.motionData.layers[BonePairList[i].Joint1].keys[frame].position);

                        p0.z = p1.z = 0.2f;

                        var perpendicular = (new Vector3(p1.y, p0.x, 0.0f) - new Vector3(p0.y, p1.x, 0.0f)).normalized * BoneLineWidth;
                        var v1 = new Vector3(p0.x, p0.y, 0.0f);
                        var v2 = new Vector3(p1.x, p1.y, 0.0f);

                        GL.Vertex((v1 - perpendicular));
                        GL.Vertex((v1 + perpendicular));
                        GL.Vertex((v2 + perpendicular));
                        GL.Vertex((v2 - perpendicular));
                    }
                }
            }
            GL.End();
        }

        // ジョイントの円を描画
        if (DrawJointCircle)
        {
            var layersNum = _motionDataImporter.motionData.layers.Length;
            for (var i = 0; i < layersNum; i++)
            {
                var p0 = AEScreenCoordToWorldPos(_motionDataImporter.motionData.layers[i].keys[frame].position);
                DrawGLCircle(p0, JointCircleRadius, JointColor);
            }
        }


        // トライアングルを描画
        if (DrawTriangle)
        {
            GL.Begin(GL.TRIANGLES);

            var layersNum = _motionDataImporter.motionData.layers.Length;
            for (var i = 0; i < TrianglePairList.Count; i++)
            {
                int j0 = TrianglePairList[i].Joint0;
                int j1 = TrianglePairList[i].Joint1;
                int j2 = TrianglePairList[i].Joint2;

                if (j0 < layersNum && j1 < layersNum && j2 < layersNum)
                {
                    var p0 = AEScreenCoordToWorldPos(_motionDataImporter.motionData.layers[TrianglePairList[i].Joint0].keys[frame].position);
                    var p1 = AEScreenCoordToWorldPos(_motionDataImporter.motionData.layers[TrianglePairList[i].Joint1].keys[frame].position);
                    var p2 = AEScreenCoordToWorldPos(_motionDataImporter.motionData.layers[TrianglePairList[i].Joint2].keys[frame].position);

                    p0.z = p1.z = p2.z = 0.1f;

                    GL.Color(TriangleColor);

                    GL.Vertex(p0);
                    GL.Vertex(p1);
                    GL.Vertex(p2);
                }
            }
            GL.End();
        }

        GL.PopMatrix();
    }



    void OnDrawGizmos()
    {
        if (EnableDrawGizmos && _isExistMotionData)
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
                pos.x = Map(pos.x, InRange.x, InRange.width, OutRange.x, OutRange.width);
                pos.y = Map(pos.y, InRange.y, InRange.height, OutRange.y, OutRange.height);

                Gizmos.DrawSphere(pos, 0.1f);

            }
        }
    }

    void OnDestroy()
    {
        if (_glMat != null)
        {
            if (Application.isEditor)
                Material.DestroyImmediate(_glMat);
            else
                Material.Destroy(_glMat);
            _glMat = null;
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

    float Map(float x, float in_min, float in_max, float out_min, float out_max, bool clamp = false)
    {
        if (clamp) x = Mathf.Max(in_min, Mathf.Min(x, in_max));
        return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
    }

    float3 AEScreenCoordToWorldPos(Position p)
    {
        return float3(
            Map(p.x, InRange.x, InRange.width, OutRange.x, OutRange.width),
            Map(p.y, InRange.y, InRange.height, OutRange.y, OutRange.height),
            0.0f
        );
    }

    void DrawGLCircle(float3 p, float r, Color color)
    {
        GL.Begin(GL.TRIANGLES);

        GL.Color(color);

        int circleResolution = 16;
        for (var i = 0; i < circleResolution; i++)
        {
            var angDiv = (Mathf.PI * 2.0f) / circleResolution;
            var ang0 = angDiv * i;
            var ang1 = angDiv * (i + 1);

            GL.Vertex(p);
            GL.Vertex(p + float3(r * cos(ang0), r * sin(ang0), 0.0f));
            GL.Vertex(p + float3(r * cos(ang1), r * sin(ang1), 0.0f));
        }

        GL.End();
    }
}

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
    /// �����̃f�[�^
    /// </summary>
    [Header("Script References")]
    [SerializeField]
    MotionDataImporter _motionDataImporter = default;

    /// <summary>
    /// �Đ��X�s�[�h
    /// </summary>
    [Header("Private Variables")]
    [Range(0.5f, 5.0f)]
    public float PlaySpeed = 1.0f;

    /// <summary>
    /// �Đ��^�C�}�[
    /// </summary>
    [SerializeField]
    float _playTimer = 0.0f;

    /// <summary>
    /// GL�I�u�W�F�N�g�`��̂��߂̃}�e���A��
    /// </summary>
    Material _glMat = default;

    bool _isExistMotionData = false;
    
    /// <summary>
    /// �Đ������ǂ���
    /// </summary>
    [Header("Flags")]
    public bool IsPlay = false;    
    /// <summary>
    /// �Đ������[�v�����邩�ǂ���
    /// </summary>
    public bool IsLoop = false;
    /// <summary>
    /// ���ׂẴ��C����`�悷�邩�ǂ���
    /// </summary>
    public bool EnableDrawAllLines = true;
    /// <summary>
    /// �{�[������Ƃ��ĕ`�悷�邩�ǂ���
    /// </summary>
    public bool DrawBoneLine = true;
    /// <summary>
    /// �W���C���g���~�Ƃ��ĕ`�悷�邩�ǂ���
    /// </summary>
    public bool DrawJointCircle = true;
    /// <summary>
    /// �W���C���g�����񂾎O�p�`��`�悷�邩�ǂ���
    /// </summary>
    public bool DrawTriangle = true;
    /// <summary>
    /// �{�[���̐��̕�
    /// </summary>
    public float BoneLineWidth = 1.0f;
    /// <summary>
    /// �W���C���g�̉~�̔��a
    /// </summary>
    public float JointCircleRadius = 0.5f;

    /// <summary>
    /// �W���C���g�̉~�̐F
    /// </summary>
    public Color JointColor    = Color.white;
    /// <summary>
    /// �{�[���̃��C���̐F
    /// </summary>
    public Color LineColor     = Color.white;
    /// <summary>
    /// �O�p�`�̐F
    /// </summary>
    public Color TriangleColor = Color.white;

    /// <summary>
    /// AfterEffects�ł̍��W�͈̔�
    /// </summary>
    [Header("RenderArea")]
    public Rect InRange = new Rect(0, 0, 1920, 1080);
    /// <summary>
    /// ���[���h���W�͈̔�
    /// </summary>
    public Rect OutRange = new Rect(-8.88888888f, -5.0f, 8.88888888f, 5.0f);

    /// <summary>
    /// �{�[���̃W���C���g�̃y�A�̃��X�g
    /// </summary>
    public List<BoneJointPair> BonePairList = new List<BoneJointPair>();
    
    /// <summary>
    /// �O�p�`�̒��_�ƂȂ�W���C���g�̃��X�g
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

        // �t���[�����v�Z
        int frame = Mathf.FloorToInt(_playTimer * _motionDataImporter.motionData.framerate);

        // ���C���`��̂��߂̃}�e���A���Z�b�g
        _glMat.SetPass(0);


        GL.PushMatrix();
        GL.MultMatrix(transform.localToWorldMatrix);

        // �Q��
        // https://wiki.unity3d.com/index.php/VectorLine

        // �{�[���̃��C����`��
        if (DrawBoneLine)
        {
            GL.Begin(GL.QUADS);

            // ���C���̐F�Z�b�g
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

        // �W���C���g�̉~��`��
        if (DrawJointCircle)
        {
            var layersNum = _motionDataImporter.motionData.layers.Length;
            for (var i = 0; i < layersNum; i++)
            {
                var p0 = AEScreenCoordToWorldPos(_motionDataImporter.motionData.layers[i].keys[frame].position);
                DrawGLCircle(p0, JointCircleRadius, JointColor);
            }
        }


        // �g���C�A���O����`��
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

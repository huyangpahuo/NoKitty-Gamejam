using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FloatingFonts : MonoBehaviour
{
    [Header("振幅")]
    public float amplitude = 5f;       // 波动幅度
    [Header("频率")]
    public float frequency = 2f; // 频率（速度）
    [Header("波长")]
    public float waveLength = 0.5f;    // 波长（相邻字符相位差）
    [Header("随机字母相位")]
    public bool randomPhase = true;    // 每个字符起始相位随机

    private TMP_Text textMesh;
    private TMP_TextInfo textInfo;
    private Vector3[][] originalVertices;
    private float[] phaseOffsets;

    private void Awake()
    {
        textMesh = GetComponent<TMP_Text>();
    }

    private void Start()
    {
        textMesh.ForceMeshUpdate();
        textInfo = textMesh.textInfo;

        int charCount = textInfo.characterCount;
        originalVertices = new Vector3[charCount][];
        phaseOffsets = new float[charCount];

        for (int i = 0; i < charCount; i++)
        {
            if (!textInfo.characterInfo[i].isVisible) continue;

            int matIndex = textInfo.characterInfo[i].materialReferenceIndex;
            int vertIndex = textInfo.characterInfo[i].vertexIndex;
            var verts = textInfo.meshInfo[matIndex].vertices;

            originalVertices[i] = new Vector3[4];
            for (int j = 0; j < 4; j++)
            {
                originalVertices[i][j] = verts[vertIndex + j];
            }

            phaseOffsets[i] = randomPhase
                ? Random.Range(0f, Mathf.PI * 2f)
                : i * waveLength;
        }
    }

    private void Update()
    {
        textMesh.ForceMeshUpdate();
        textInfo = textMesh.textInfo;

        for (int i = 0; i < textInfo.characterCount; i++)
        {
            if (!textInfo.characterInfo[i].isVisible) continue;

            int matIndex = textInfo.characterInfo[i].materialReferenceIndex;
            int vertIndex = textInfo.characterInfo[i].vertexIndex;
            var verts = textInfo.meshInfo[matIndex].vertices;

            float wave = Mathf.Sin(Time.time * frequency + phaseOffsets[i]) * amplitude;

            for (int j = 0; j < 4; j++)
            {
                Vector3 orig = originalVertices[i][j];
                verts[vertIndex + j] = orig + new Vector3(0f, wave, 0f);
            }
        }

        // 更新网格
        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            var meshInfo = textInfo.meshInfo[i];
            meshInfo.mesh.vertices = meshInfo.vertices;
            textMesh.UpdateGeometry(meshInfo.mesh, i);
        }
    }
}

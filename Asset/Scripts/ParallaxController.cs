using System;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxController : MonoBehaviour
{
    [Serializable]
    public class LayerSetting
    {
        [Tooltip("挂有 ParallaxLayer 的物体")]
        public ParallaxLayer layer;

        [Range(-2f, 2f)]
        public float speed = 0.2f;
    }

    [Header("跟随目标")]
    [SerializeField] private Transform target;

    [Header("背景层")]
    [SerializeField] private List<LayerSetting> layers = new List<LayerSetting>();

    private bool initialized;
    private float lastX;

    private void Start()
    {
        if (target == null)
        {
            Debug.LogError("ParallaxController：Target为空！");
            enabled = false;
            return;
        }

        lastX = target.position.x;
    }

    private void LateUpdate()
    {
        if (!initialized)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");

            if (player == null)
                return;

            target = player.transform;
            lastX = target.position.x;
            initialized = true;
        }


        float deltaX = target.position.x - lastX;

        foreach (LayerSetting setting in layers)
        {
            if (setting.layer == null)
                continue;

            Transform t = setting.layer.transform;

            Vector3 pos = t.position;
            pos.x += deltaX * setting.speed;
            t.position = pos;
        }

        lastX = target.position.x;
    }
}
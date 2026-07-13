using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Story07 : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        StoryManager.Instance.PlayStory("Scene06_01");
        SettingsManager.UnlockLevel("6");
    }

    // Update is called once per frame
    void Update()
    {

    }
}

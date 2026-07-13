using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Story06 : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        StoryManager.Instance.PlayStory("Scene05_01");
        SettingsManager.UnlockLevel("5");
    }

    // Update is called once per frame
    void Update()
    {

    }
}

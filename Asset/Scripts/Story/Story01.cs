using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Story01 : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        StoryManager.Instance.PlayStory("IntroStory");
        SettingsManager.UnlockLevel("1");
    }

    // Update is called once per frame
    void Update()
    {

    }
}

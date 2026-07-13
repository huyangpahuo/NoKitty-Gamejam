using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Story02 : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        StoryManager.Instance.PlayStory("Scene01_01");
    }

    // Update is called once per frame
    void Update()
    {

    }
}

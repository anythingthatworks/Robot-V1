using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This code was largely grabbed from the one way collision platforms youtube tutorial by Blackthornprod.  

public class OneWayPlatform : MonoBehaviour

{

    private PlatformEffector2D effector;

    // Start is called before the first frame update
    void Start()
    {
        effector = GetComponent<PlatformEffector2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetAxisRaw("Vertical") == -1)
        {
            effector.rotationalOffset = 180f; //really inefficient to do this every frame if we're gonna have a bunch of one way platforms but w/e for now
        }
        else if (Input.GetAxisRaw("Jump") == 1)
        {
            effector.rotationalOffset = 0f;
        }
    }
}

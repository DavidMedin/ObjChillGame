using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class billboard : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var constraint = GetComponent<ParentConstraint>();
        constraint.SetSource(0,new ConstraintSource{sourceTransform = Camera.main.transform,weight=1.0f});
        constraint.SetRotationOffset(0,new Vector3(-90.0f,0,0));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

using UnityEngine;
using System.Collections;
 
public class Arrow0 : MonoBehaviour {
     
    public GameObject goTarget;
    private GameObject[] _players; 

    void Start()
    {
        _players = GameObject.FindGameObjectsWithTag("Player");
        goTarget = _players[0];
        
        Vector3 v3Pos = Camera.main.WorldToViewportPoint(goTarget.transform.position);
        transform.position = Camera.main.ViewportToWorldPoint(v3Pos);
    }
    
    void Update () {
        PositionArrow();        
    }
     
    void PositionArrow()
    {
        
        GetComponent<Renderer>().enabled = false;
        
        Vector3 v3Pos = Camera.main.WorldToViewportPoint(goTarget.transform.position);
        
        if (v3Pos.z < Camera.main.nearClipPlane)
            return;  // Object is behind the camera
        
        if (v3Pos.x >= 0.0f && v3Pos.x <= 1.0f && v3Pos.y >= 0.0f && v3Pos.y <= 1.0f)
            return; // Object center is visible
                
        GetComponent<Renderer>().enabled = true;
        v3Pos.x -= 0.5f;  // Translate to use center of viewport
        v3Pos.y -= 0.5f; 
        v3Pos.z = 0;      // I think I can do this rather than do a 
        //   a full projection onto the plane
        
        float fAngle = Mathf.Atan2 (v3Pos.x, v3Pos.y);
        transform.localEulerAngles = new Vector3(-60.0f, fAngle * Mathf.Rad2Deg, 0.0f);
        //transform.localEulerAngles = new Vector3(0.0f, -fAngle * Mathf.Rad2Deg, 0.0f);

        
        v3Pos.x = 0.5f * Mathf.Sin (fAngle) + 0.5f;  // Place on ellipse touching 
        v3Pos.z = 0.5f * Mathf.Cos (fAngle) + 0.5f;  //   side of viewport
        v3Pos.y = Camera.main.nearClipPlane + 0.01f;  // Looking from neg to pos Z;
        transform.position = Camera.main.ViewportToWorldPoint(v3Pos);
    }
}
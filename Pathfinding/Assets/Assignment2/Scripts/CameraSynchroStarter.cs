using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CameraSynchroStarter : MonoBehaviour {

	// Use this for initialization
    bool leftClickFlag = true;
    
    public List<GameObject> mobiles;
    
    void Start()
    {
    }
    
    void Update () 
    {
        /***Left Click****/
        if (Input.GetKey(KeyCode.Mouse0) && leftClickFlag)
            leftClickFlag = false;
        
        if (!Input.GetKey(KeyCode.Mouse0) && !leftClickFlag)
        {
            foreach(GameObject g in this.mobiles) {
                ISynchroStart a = (ISynchroStart) g.GetComponent(typeof(ISynchroStart));
                a.prepare();
            }
            foreach(GameObject g in this.mobiles) {
                ISynchroStart a = (ISynchroStart) g.GetComponent(typeof(ISynchroStart));
                a.act();
            }
            this.mobiles.Clear();
            leftClickFlag = true;
        }
    }
}

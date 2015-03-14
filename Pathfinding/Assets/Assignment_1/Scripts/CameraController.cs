using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {
	
	RaycastHit hit;
	bool leftClickFlag = true;
	
	public GameObject mobile;

	IMotionModel mobileScript;
	
	void Start()
	{
		if (mobile != null)
		{
			mobileScript = (IMotionModel) mobile.GetComponent(typeof(IMotionModel));
		}
	}
	
	void Update () 
	{
		/***Left Click****/
		if (Input.GetKey(KeyCode.Mouse0) && leftClickFlag)
			leftClickFlag = false;
		
		if (!Input.GetKey(KeyCode.Mouse0) && !leftClickFlag)
		{
			leftClickFlag = true;
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			if (Physics.Raycast(ray, out hit, 400))
			{
				float X = hit.point.x;
				float Z = hit.point.z;
				Vector3 target = new Vector3(X, mobile.transform.position.y, Z);
				
				mobileScript.MoveOrder(target);
			}
		}
	}
}

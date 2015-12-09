using UnityEngine;
using System.Collections;

public class CameraUpSea : MonoBehaviour {

	public float levelMin=0.1f; 
	public float targetLevel=1f;
	public float slopeToTarget=0.1f;
	public PlusieurVagues vagues;
	float seaLevelUnder;
	public CameraOscillations cam;
	float y;

	void Update() {
		// keep camera upside levelMin
		Vector3 positionUnder = transform.position; 
		positionUnder.y = 0f;
		seaLevelUnder = vagues.SeaManager.CalculeImage(positionUnder).y;
		// straight
		positionUnder = transform.position+1f*Vector3.forward; 
		positionUnder.y = 0f;
		seaLevelUnder = Mathf.Max (seaLevelUnder, vagues.SeaManager.CalculeImage (positionUnder).y);
		positionUnder = transform.position+2.5f*Vector3.forward; 
		positionUnder.y = 0f;
		seaLevelUnder = Mathf.Max (seaLevelUnder, vagues.SeaManager.CalculeImage (positionUnder).y);
		positionUnder = transform.position+10f*Vector3.forward; 
		positionUnder.y = 0f;
		seaLevelUnder = Mathf.Max (seaLevelUnder, vagues.SeaManager.CalculeImage (positionUnder).y);
		positionUnder = transform.position+20f*Vector3.forward; 
		positionUnder.y = 0f;
		seaLevelUnder = Mathf.Max (seaLevelUnder, vagues.SeaManager.CalculeImage (positionUnder).y);
		positionUnder = transform.position+30f*Vector3.forward; 
		positionUnder.y = 0f;
		seaLevelUnder = Mathf.Max (seaLevelUnder, vagues.SeaManager.CalculeImage (positionUnder).y);
		// left
		positionUnder = transform.position+1f*Vector3.left; 
		positionUnder.y = 0f;
		seaLevelUnder = Mathf.Max (seaLevelUnder, vagues.SeaManager.CalculeImage (positionUnder).y);
		positionUnder = transform.position+2.5f*Vector3.left; 
		positionUnder.y = 0f;
		seaLevelUnder = Mathf.Max (seaLevelUnder, vagues.SeaManager.CalculeImage (positionUnder).y);
		positionUnder = transform.position+5f*Vector3.left; 
		positionUnder.y = 0f;
		seaLevelUnder = Mathf.Max (seaLevelUnder, vagues.SeaManager.CalculeImage (positionUnder).y);
		positionUnder = transform.position+10f*Vector3.left; 
		positionUnder.y = 0f;
		seaLevelUnder = Mathf.Max (seaLevelUnder, vagues.SeaManager.CalculeImage (positionUnder).y);
		// right
		positionUnder = transform.position+1f*Vector3.right; 
		positionUnder.y = 0f;
		seaLevelUnder = Mathf.Max (seaLevelUnder, vagues.SeaManager.CalculeImage (positionUnder).y);
		positionUnder = transform.position+2.5f*Vector3.right; 
		positionUnder.y = 0f;
		seaLevelUnder = Mathf.Max (seaLevelUnder, vagues.SeaManager.CalculeImage (positionUnder).y);
		positionUnder = transform.position+5f*Vector3.right; 
		positionUnder.y = 0f;
		seaLevelUnder = Mathf.Max (seaLevelUnder, vagues.SeaManager.CalculeImage (positionUnder).y);
		positionUnder = transform.position+10f*Vector3.right; 
		positionUnder.y = 0f;
		seaLevelUnder = Mathf.Max (seaLevelUnder, vagues.SeaManager.CalculeImage (positionUnder).y);
		// then ajust camera position to be up sea
		if (transform.position.y < seaLevelUnder + levelMin) {
			transform.Translate (0f, (seaLevelUnder + levelMin - transform.position.y), 0f, Space.World);
		}
		// reach target level
		y = transform.position.y-(targetLevel+seaLevelUnder);
		if (Mathf.Abs (y)>levelMin) transform.Translate(0, -Mathf.Sign(y)*slopeToTarget*Time.deltaTime, 0);
	}
}

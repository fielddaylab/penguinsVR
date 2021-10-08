//NSF Penguins VR Experience
//Ross Tredinnick - WID Virtual Environments Group / Field Day Lab - 2021

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Egg : MonoBehaviour
{
	bool _isTaken;
	Vector3 _startPosition;
	Quaternion _startRotation;
	
	public bool IsTaken
	{
		get { return _isTaken; }
		set { _isTaken = value; }
	}
	
    // Start is called before the first frame update
    void Start()
    {
        _isTaken = false;
		_startPosition = transform.position;
		_startRotation = transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
	
	public void Reset()
	{
		gameObject.transform.SetParent(null, false);
		transform.position = _startPosition;
		transform.rotation = _startRotation;
		_isTaken = false;
	}
}

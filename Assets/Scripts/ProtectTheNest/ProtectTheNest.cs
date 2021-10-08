//NSF Penguins VR Experience
//Ross Tredinnick - WID Virtual Environments Group / Field Day Lab - 2021

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProtectTheNest : MiniGameController
{
    [SerializeField]
    SkuaSpawner _skuaSpawner = null;

	[SerializeField]
	float _moveFrequency = 2f;
	public float MoveFrequency => _moveFrequency;

    [SerializeField]
    float _gameTimeLimit = 60f;
    public float GameTimeLimit => _gameTimeLimit;

	[SerializeField]
	GameObject _eggTimer;
	
	[SerializeField]
	GameObject _theEgg;
	public GameObject TheEgg => _theEgg;

    float _skuaMoveTime = 0f;
    float _timeWithoutEgg = 0f;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(!_isGameActive)
        {
            return;
        }

        UpdateTime();

        if(_timeWithoutEgg - _startTime > _gameTimeLimit)
		{
			//game is over...
			//do a fade out...
            EndGame();
            return;
		}

        if(_skuaSpawner != null)
        {
            _skuaSpawner.CheckForSpawn(_currentTime - _startTime);

            if(_currentTime - _skuaMoveTime > _moveFrequency)
            {
                _skuaSpawner.MoveSkuas();
                _skuaMoveTime = _currentTime;
            }

            if(_eggTimer != null)
            {
                if(!EggIsTaken())
                {
                    _timeWithoutEgg += Time.deltaTime;
                    float t = _timeWithoutEgg - _startTime;
                    float timeLeft = _gameTimeLimit - t;
                    if(timeLeft > 0f)
                    {
                        System.TimeSpan ts = System.TimeSpan.FromSeconds(timeLeft);
                        _eggTimer.GetComponent<TMPro.TextMeshPro>().text = string.Format("{0:D2}:{1:D2}", ts.Minutes, ts.Seconds);
                    }
                    else
                    {
                        System.TimeSpan ts = System.TimeSpan.FromSeconds(0);
                        _eggTimer.GetComponent<TMPro.TextMeshPro>().text = string.Format("{0:D2}:{1:D2}", ts.Minutes, ts.Seconds);
                    }
                }
            }
        }


    }

	bool EggIsTaken()
	{
        if(_theEgg != null)
        {
		    return _theEgg.GetComponent<Egg>().IsTaken;
        }

        return false;
	}

    public virtual void StartGame()
    {
        base.StartGame();

        _skuaMoveTime = _startTime;
        _timeWithoutEgg = _startTime;

    }

    public virtual void EndGame()
    {
        if(_skuaSpawner != null)
        {
            _skuaSpawner.ClearGame();
        }
        
        if(_theEgg != null)
        {
		     _theEgg.GetComponent<Egg>().IsTaken = false;
        }

        base.EndGame();
    }
}

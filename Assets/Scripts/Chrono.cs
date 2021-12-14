using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chrono : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
	float suspendTimer;
	float haltedTimer;
    float gameTimer;
	bool fl_stop;
	bool fl_init;


	/*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
	Chrono() {
		suspendTimer	= 0;
		haltedTimer		= Get();
        gameTimer       = Time.time;

		Reset();
		fl_stop			= true;
		fl_init			= false;
	}


	/*------------------------------------------------------------------------
	RENVOIE LA VALEUR DU CHRONO ACTUEL (millisecondes)
	------------------------------------------------------------------------*/
	float Get() {
		if (fl_stop) {
			return haltedTimer;
		}
		else {
			return Time.time - gameTimer;
		}
	}

	/*------------------------------------------------------------------------
	GESTION DU CHRONO
	------------------------------------------------------------------------*/
	void Reset() {
		fl_init			= true;
		fl_stop			= false;
		suspendTimer	= 0;
		gameTimer		= Time.time;
	}

	void Begin() {
		if (suspendTimer != 0) {
			var d = Time.time - suspendTimer;
			gameTimer += d;
		}
		haltedTimer = 0;
		fl_stop = false;
		suspendTimer = 0;
	}

	void Stop() {
		if (fl_stop) {
			return;
		}
		haltedTimer = Get();
		fl_stop = true;
		suspendTimer = Time.time;
	}



	/*------------------------------------------------------------------------
	GAIN DE TEMPS
	------------------------------------------------------------------------*/
	void TimeShift(float n) {
		gameTimer = Mathf.Min(Time.time, gameTimer+n);
	}
}

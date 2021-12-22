using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace GUI{

public class SoccerInterface
{
	static Color GLOW_COLOR	= GameInterface.GLOW_COLOR;

	MovieClip mc;

	GameMode game;
	List<Field> scores;
	Field time;

	/*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
	SoccerInterface(GameMode game) {
		this.game	= game;
		Init();
	}


	/*------------------------------------------------------------------------
	INITIALISATION
	------------------------------------------------------------------------*/
	void Init() {
		mc		= game.depthMan.Attach("hammer_interf_game",Data.DP_TOP);
		mc._x	= -game.xOffset;
		mc._y	= 0;
		mc.GotoAndStop(3);
		mc.cacheAsBitmap = true;

		scores		= new List<Field>();
        scores.Add(mc.FindSub("score0") as Field);
        scores.Add(mc.FindSub("score1") as Field);
		time		= mc.FindSub("time") as Field;

		FxManager.AddGlow(mc.FindSub("score0"), GLOW_COLOR, 2);
		FxManager.AddGlow(mc.FindSub("score1"), GLOW_COLOR, 2);
		FxManager.AddGlow(mc.FindSub("time"), GLOW_COLOR, 2);

		SetScore(0, 0);
		SetScore(1, 0);
	}


	/*------------------------------------------------------------------------
	MET � JOUR UN SCORE
	------------------------------------------------------------------------*/
	void SetScore(int pid, int n) {
		scores[pid].field.text = n.ToString();
	}


	/*------------------------------------------------------------------------
	MET � JOUR LE TEMPS
	------------------------------------------------------------------------*/
	void SetTime(string str) {
		time.field.text = str;
	}


	/*------------------------------------------------------------------------
	DESTRUCTION
	------------------------------------------------------------------------*/
	void DestroyThis() {
		mc.RemoveMovieClip();
	}


	/*------------------------------------------------------------------------
	MAIN
	------------------------------------------------------------------------*/
	void Update() {
	}
}

}

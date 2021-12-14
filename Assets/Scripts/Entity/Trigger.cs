using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Entity;

public class Trigger : Entity
{

	bool fl_largeTrigger;

	/*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
	public Trigger() : base() {
		fl_largeTrigger = false;
	}


	/*------------------------------------------------------------------------
	S'AJOUTE � UNE CASE DONN�E
	------------------------------------------------------------------------*/
	void TAddSingle(int cx, int cy) {
		if (cx<0 | cx>=Data.LEVEL_WIDTH | cy<0 | cy>=Data.LEVEL_HEIGHT) {
			return ;
		}
		world.triggers[cx][cy].push(this) ;
	}


	/*------------------------------------------------------------------------
	QUITTE UNE CASE DONN�E
	------------------------------------------------------------------------*/
	void TRemSingle(int cx, int cy) {
		if (cx<0 | cx>=Data.LEVEL_WIDTH | cy<0 | cy>=Data.LEVEL_HEIGHT) {
			return ;
		}
		var list = world.triggers[cx][cy] ;
		for (var i=0;i<list.Count;i++) {
			if ( list[i]==this ) {
				list.RemoveAt(i) ;
				i-- ;
			}
		}
	}


	/*------------------------------------------------------------------------
	AJOUTE L'ENTIT� � L'ENSEMBLE DES CASES QU'ELLE OCCUPE
	------------------------------------------------------------------------*/
	void TAdd(int cx, int cy) {
		TAddSingle(cx,cy) ;
		TAddSingle(cx-1,cy) ;
		TAddSingle(cx+1,cy) ;
		TAddSingle(cx,cy-1) ;
		if (fl_largeTrigger) {
			TAddSingle(cx,cy+1) ;
		}
	}

	/*------------------------------------------------------------------------
	RETIRE L'ENTIT� DE L'ENSEMBLE DES CASES QU'ELLE OCCUPE
	------------------------------------------------------------------------*/
	void TRem(int cx, int cy) {
		TRemSingle(cx,cy) ;
		TRemSingle(cx-1,cy) ;
		TRemSingle(cx+1,cy) ;
		TRemSingle(cx,cy-1) ;
		if (fl_largeTrigger) {
			TRemSingle(cx,cy+1) ;
		}
	}


	/*------------------------------------------------------------------------
	T�L�PORTE L'ENTIT� � UN AUTRE POINT
	------------------------------------------------------------------------*/
	void MoveTo(float x, float y) {
		TRem(cx, cy);
		this.x=x;
		this.y=y-1;
		UpdateCoords() ;
		TAdd(cx, cy);
	}


	/*------------------------------------------------------------------------
	T�L�PORTE L'ENTIT� � UN AUTRE POINT (PAR CASE)
	------------------------------------------------------------------------*/
	void MoveToCase(int cx, int cy) {
		MoveTo(this.x_ctr(cx), this.y_ctr(cy));
	}


	/*------------------------------------------------------------------------
	RENVOIE LA LISTE DES ENTIT�S D'UN TYPE DONN� DANS LA CASE COURANTE
	------------------------------------------------------------------------*/
	List<Entity> GetByType(int type) {
		List<Entity> list = world.triggers[cx][cy] ;
		List<Entity> res = new List<Entity>() ;
		foreach (Entity e in list) {
			if ( (e.types & type) != 0 ) {
				res.Add(e) ;
			}
		}

		return res ;
	}
}

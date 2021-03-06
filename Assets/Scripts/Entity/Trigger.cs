using System.Collections.Generic;

public class Trigger : Entity
{
    protected bool fl_largeTrigger;

    /*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
    public Trigger(string reference) : base(reference)
    {
        fl_largeTrigger = false;
    }


    /*------------------------------------------------------------------------
	S'AJOUTE � UNE CASE DONN�E
	------------------------------------------------------------------------*/
    protected void TAddSingle(int cx, int cy)
    {
        if (cx < 0 | cx >= Data.LEVEL_WIDTH | cy < 0 | cy >= Data.LEVEL_HEIGHT)
        {
            return;
        }
        world.triggers[cx][cy].Add(this);
    }


    /*------------------------------------------------------------------------
	QUITTE UNE CASE DONN�E
	------------------------------------------------------------------------*/
    protected void TRemSingle(int cx, int cy)
    {
        if (cx < 0 | cx >= Data.LEVEL_WIDTH | cy < 0 | cy >= Data.LEVEL_HEIGHT)
        {
            return;
        }
        List<Entity> list = world.triggers[cx][cy];
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i] == this)
            {
                list.RemoveAt(i);
                i--;
            }
        }
    }


    /*------------------------------------------------------------------------
	AJOUTE L'ENTIT� � L'ENSEMBLE DES CASES QU'ELLE OCCUPE
	------------------------------------------------------------------------*/
    protected virtual void TAdd(int cx, int cy)
    {
        TAddSingle(cx, cy);
        TAddSingle(cx - 1, cy);
        TAddSingle(cx + 1, cy);
        TAddSingle(cx, cy + 1);
        if (fl_largeTrigger)
        {
            TAddSingle(cx, cy - 1);
        }
    }

    /*------------------------------------------------------------------------
	RETIRE L'ENTIT� DE L'ENSEMBLE DES CASES QU'ELLE OCCUPE
	------------------------------------------------------------------------*/
    protected virtual void TRem(int cx, int cy)
    {
        TRemSingle(cx, cy);
        TRemSingle(cx - 1, cy);
        TRemSingle(cx + 1, cy);
        TRemSingle(cx, cy + 1);
        if (fl_largeTrigger)
        {
            TRemSingle(cx, cy - 1);
        }
    }


    /*------------------------------------------------------------------------
	T�L�PORTE L'ENTIT� � UN AUTRE POINT
	------------------------------------------------------------------------*/
    public void MoveTo(float x, float y)
    {
        TRem(cx, cy);
        this.x = x;
        this.y = y+1;
        UpdateCoords();
        TAdd(cx, cy);
    }


    /*------------------------------------------------------------------------
	T�L�PORTE L'ENTIT� � UN AUTRE POINT (PAR CASE)
	------------------------------------------------------------------------*/
    public void MoveToCase(int cx, int cy)
    {
        MoveTo(Trigger.x_ctr(cx), Trigger.y_ctr(cy));
    }


    /*------------------------------------------------------------------------
	RENVOIE LA LISTE DES ENTIT�S D'UN TYPE DONN� DANS LA CASE COURANTE
	------------------------------------------------------------------------*/
    protected List<IEntity> GetByType(int type)
    {
        if (cx < 0 | cx >= Data.LEVEL_WIDTH | cy < 0 | cy >= Data.LEVEL_HEIGHT)
        {
            return new List<IEntity>();
        }
        List<Entity> list = world.triggers[cx][cy];
        List<IEntity> res = new List<IEntity>();
        foreach (Entity e in list)
        {
            if ((e.types & type) != 0)
            {
                res.Add(e);
            }
        }
        return res;
    }
}

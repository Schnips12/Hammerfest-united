public struct Stat
{
    public float current;
    public float total;


    /*------------------------------------------------------------------------
	OP�RATIONS COURANTES
	------------------------------------------------------------------------*/
    public void Inc(float n)
    {
        current += n;
    }

    public void Reset()
    {
        total += current;
        current = 0;
    }
}
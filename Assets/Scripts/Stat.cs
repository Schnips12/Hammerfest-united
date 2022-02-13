public partial class StatsManager
{
    private struct Stat
    {
        public float current;
        public float total;

        /// <summary>Increases the current value of the amount n.</summary>
        public void Inc(float n)
        {
            current += n;
        }

        /// <summary>Sets the current value to zero after updating the total.</summary>
        public void Reset()
        {
            total += current;
            current = 0;
        }
    }
}
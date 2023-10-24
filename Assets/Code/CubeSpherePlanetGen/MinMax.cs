using System;

namespace CubeSphere
{
    public class MinMax
    {
        public float Min { get; private set; }
        public float Max { get; private set; }

        public MinMax()
        {
            Min = float.MaxValue;
            Max = float.MinValue;
        }

        public void UpdateValues(float v)
        {
            if (v > Max)
            {
                Max = v;
            }
            if (v < Min)
            {
                Min = v;
            }
        }
    }
}

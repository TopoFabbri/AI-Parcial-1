using System;

namespace Model.Tools.Time
{
    public sealed class Timer
    {
        private readonly DateTime startTime;
        
        public float TimeElapsed => (float)(Time.DateTime - startTime).TotalSeconds;
        
        public Timer()
        {
            startTime = Time.DateTime;
        }
    }
}
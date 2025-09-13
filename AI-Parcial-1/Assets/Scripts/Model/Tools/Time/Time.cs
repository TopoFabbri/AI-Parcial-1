using System;

namespace Model.Tools.Time
{
    public static class Time
    {
        public static float TimeElapsed
        {
            get
            {
                if (StartTime != default) return (float)(DateTime.UtcNow - StartTime).TotalSeconds;
                
                return -1f;
            }
        }

        public static void Start()
        {
            StartTime = DateTime.Now;
        }

        public static void Update()
        {
            TickTime = (float)(DateTime - _lastTickTime).TotalSeconds;
            _lastTickTime = DateTime;
        }

        public static DateTime DateTime => DateTime.UtcNow;
        public static DateTime DateTimeNow => DateTime.Now;
        public static DateTime StartTime { get; private set; }
        public static float TickTime { get; private set; }

        private static DateTime _lastTickTime;
    }
}
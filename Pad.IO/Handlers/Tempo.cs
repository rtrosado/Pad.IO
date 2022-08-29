namespace Pad.IO.Handlers
{
    using System.Diagnostics;

    internal class Tempo
    {
        public Stopwatch stopwatch { get; set; }
        public long lastRenderTime { get; set; }
        public long thisRenderTime { get; set; }
        public long lapseRenderTime { get; set; }
        public float framerate { get; set; }
        public int frameCount { get; set; }

        public Tempo(float constrain)
        {
            stopwatch = Stopwatch.StartNew();
            lastRenderTime = stopwatch.ElapsedTicks;
            thisRenderTime = stopwatch.ElapsedTicks;
            lapseRenderTime = thisRenderTime - lastRenderTime;
            framerate = 1.0f / constrain * Stopwatch.Frequency;
            frameCount = 0;
        }

        public void start()
        {
            thisRenderTime = stopwatch.ElapsedTicks;
            frameCount++;
        }

        public void stop() => 
            lastRenderTime = thisRenderTime;
        public void waitConstrainFramerateLoop()
        {
            lapseRenderTime = stopwatch.ElapsedTicks - thisRenderTime;
            while (lapseRenderTime / framerate < 1)
                lapseRenderTime = stopwatch.ElapsedTicks - thisRenderTime;
        }

        public string getActualFramerate() =>
            ((int)(1 / (((thisRenderTime - lastRenderTime) / (float)Stopwatch.Frequency)))).ToString();
    }
}

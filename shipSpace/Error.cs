using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;
using System.Diagnostics;


namespace shipSpace
{
    public abstract class Error
    {
        public Timer timer;
        public double value {
            get
            {
                return GetErrorValue();
            }
            set
            {
                this.value = value;
            }
        }
        public Stopwatch stopWatch;

        public virtual double GetErrorValue()
        {
            return value;
        }
    }

    public class EOffset : Error
    {
        public EOffset(float value)
        {
            this.value = value;
        }
    }

    public class EDrift : Error
    {
        double drift;

        public EDrift(double drift)
        {
            //Error starts with 0, then begins drifting
            this.value = 0;
            this.drift = drift;

            stopWatch = new Stopwatch();
            stopWatch.Start();
        }

        public override double GetErrorValue()
        {
            return value + drift * stopWatch.ElapsedMilliseconds/1000;
        }
    }

    public class EFreq : Error
    {
        private double frequency;
        private double amplitude;
        private double offset;

        public EFreq(float frequency, float amplitude, float offset)
        {
            this.frequency = frequency;
            this.amplitude = amplitude;
            this.offset = offset;

            stopWatch = new Stopwatch();
            stopWatch.Start();
        }

        public override double GetErrorValue()
        {
            return amplitude * Math.Sin((frequency * stopWatch.ElapsedMilliseconds/1000)) + offset;
        }
    }
}

using System;
using System.Linq;

namespace AlPipe.Synth
{
    public delegate float OscillatorFunction(float phase);

    public static class OscillatorFunctions
    {
        private const double InvPi = 1 / Math.PI;
        private const float TwoPi = (float) (2 * Math.PI);

        public static float Sine(float phase)
        {
            return (float) Math.Sin(phase);
        }

        public static float Square(float phase)
        {
            return phase < Math.PI ? 1 : -1;
        }

        public static float Triangle(float phase)
        {
            return (float) (phase < Math.PI ? -1 + 2 * phase * InvPi : 3 - 2 * phase * InvPi);
        }

        public static float SawTooth(float phase)
        {
            return (float) (1 - phase * InvPi);
        }

        public static OscillatorFunction Pulse(float width)
        {
            return phase => phase < width * TwoPi ? 1 : -1;
        }
    }

    /// <summary>
    /// <see cref="ISampleStream{T}"/> that generates a sine wave.
    /// </summary>
    public class SynthStream : ISampleStream<float>
    {
        private OscillatorFunction _oscillator;
        private float _phase;
        private const float TwoPi = (float) (2 * Math.PI);

        /// <inheritdoc />
        public bool IsStatic => false;

        /// <inheritdoc />
        public long Length => 0;

        /// <inheritdoc />
        public TimeSpan Duration => TimeSpan.Zero;

        /// <inheritdoc />
        public long SamplePosition { get; set; }

        /// <inheritdoc />
        public TimeSpan TimePosition { get; set; }

        /// <inheritdoc />
        public Format Format { get; }

        /// <summary>
        /// Frequency of the sine wave.
        /// </summary>
        public float Frequency { get; set; }

        /// <summary>
        /// Get or set the amplitude of the sine wave. Amplitude of 1 is full volume, 0 is silent.
        /// </summary>
        public float Amplitude { get; set; }

        /// <summary>
        /// Phase of the sine wave.
        /// </summary>
        public float Phase
        {
            get => _phase;
            set => _phase = (value % TwoPi + TwoPi) % TwoPi; // normalize to [0, TwoPi[
        }

        /// <summary>
        /// The oscillator function that is used.
        /// </summary>
        public OscillatorFunction Oscillator
        {
            get => _oscillator;
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));
                _oscillator = value;
            }
        }

        /// <summary>
        /// Create a new synth generator stream.
        /// </summary>
        /// <param name="frequency">Frequency of the oscillator.</param>
        /// <param name="amplitude">Amplitude of the synth. Default is 1.</param>
        /// <param name="phase">Starting phase of the oscillator. Default is 0.</param>
        /// <param name="oscillator">Oscillator to use. Defaults to sine wave.</param>
        public SynthStream(float frequency, float amplitude = 1, float phase = 0, OscillatorFunction oscillator = null)
        {
            Format = new Format(1, 44100);
            Frequency = frequency;
            Amplitude = amplitude;
            Phase = phase;
            Oscillator = oscillator ?? OscillatorFunctions.Sine;
        }

        /// <inheritdoc />
        public int Read(float[] samples, int offset, int count)
        {
            var phaseStep = (float) (2 * Math.PI * Frequency) / Format.SampleRate;
            for (var i = 0; i < count; i++)
            {
                samples[i + offset] = Amplitude * Oscillator(Phase);
                Phase += phaseStep;
                if (Phase > TwoPi)
                    Phase -= TwoPi;
            }

            if (SamplesRead != null)
            {
                var args = new SamplesEventArgs<float>(samples.Skip(offset), count);
                SamplesRead?.Invoke(this, args);
            }
            return count;
        }

        /// <inheritdoc />
        public event EventHandler<SamplesEventArgs<float>> SamplesRead;
    }
}

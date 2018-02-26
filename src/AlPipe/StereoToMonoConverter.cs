using System;

namespace AlPipe
{
    /// <summary>
    /// Convert a stereo stream to mono by averaging left and right samples.
    /// </summary>
    public class StereoToMonoConverter : StreamConverterBase<float, float>
    {
        public override Format Format { get; }

        public StereoToMonoConverter(ISampleStream<float> source) : base(source)
        {
            if (source.Format.Channels != 2)
                throw new ArgumentException("Source must provide stereo data.", nameof(source));
            Format = new Format(1, source.Format.SampleRate);
        }

        public override int Read(float[] samples, int offset, int count)
        {
            var stereoCount = count * 2;
            EnsureBufferSize(stereoCount);
            var sRead = Source.Read(Buffer, 0, stereoCount);
            var mRead = sRead / 2;
            for (var mIndex = 0; mIndex < mRead; mIndex++)
            {
                var sIndex = mIndex * 2;
                var mean = (Buffer[sIndex] + Buffer[sIndex + 1]) * .5f;
                samples[offset + mIndex] = mean;
            }

            return mRead;
        }
    }
}

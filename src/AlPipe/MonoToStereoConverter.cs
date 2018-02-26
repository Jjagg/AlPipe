using System;

namespace AlPipe
{
    /// <summary>
    /// Convert a mono stream to stereo by duplicating samples.
    /// </summary>
    public class MonoToStereoConverter : StreamConverterBase<float, float>
    {
        public override Format Format { get; }

        public MonoToStereoConverter(ISampleStream<float> source) : base(source)
        {
            if (source.Format.Channels != 1)
                throw new ArgumentException("Source must provide mono data.", nameof(source));
            Format = new Format(2, source.Format.SampleRate);
        }

        public override int Read(float[] samples, int offset, int count)
        {
            var monoCount = count / 2;
            EnsureBufferSize(monoCount);
            var mRead = Source.Read(Buffer, 0, monoCount);
            var sRead = mRead * 2;
            for (var mIndex = 0; mIndex < mRead; mIndex++)
            {
                var sIndex = mIndex * 2;
                var sample = Buffer[offset + mIndex];
                samples[offset + sIndex] = sample;
                samples[offset + sIndex + 1] = sample;
            }

            return sRead;
        }
    }
}

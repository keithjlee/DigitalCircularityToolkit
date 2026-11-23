using System;

namespace DigitalCircularityToolkit.Objects
{
    /// <summary>
    /// A lazily computed value derived from a design object's frame, samples and/or geometry (P3). It remembers the
    /// inputs it was computed for and recomputes when they differ, so a <c>with</c> copy that changes the frame never
    /// returns a stale hull. Record equality ignores it (every Memo equals every other), so two objects with the same
    /// data are equal whether or not their caches are filled. Thread-safe: an entry is immutable and replaced whole.
    /// Use as a non-readonly field and call <see cref="Get"/> on the field itself (it is a mutable struct).
    /// </summary>
    public struct Memo<T> : IEquatable<Memo<T>>
    {
        private sealed class Entry
        {
            public readonly ObjectFrame Frame;
            public readonly object Samples;
            public readonly object Geometry;
            public readonly Lazy<T> Value;

            public Entry(in ObjectFrame frame, object samples, object geometry, Func<T> compute)
            {
                Frame = frame;
                Samples = samples;
                Geometry = geometry;
                Value = new Lazy<T>(compute);
            }

            public bool Matches(in ObjectFrame frame, object samples, object geometry) =>
                ReferenceEquals(Samples, samples) && ReferenceEquals(Geometry, geometry) && Frame.Equals(frame);
        }

        private Entry _entry;

        /// <summary>
        /// The value for these inputs, computed on first use. Pass <c>default</c> / null for inputs the value does not
        /// depend on (e.g. a 3D hull depends only on the samples), so changing them does not trigger a recompute.
        /// </summary>
        public T Get(in ObjectFrame frame, object samples, object geometry, Func<T> compute)
        {
            Entry e = _entry;
            if (e == null || !e.Matches(frame, samples, geometry))
            {
                e = new Entry(frame, samples, geometry, compute);
                _entry = e;
            }
            return e.Value.Value;
        }

        /// <summary>Provide the value for these inputs in advance (still computed lazily), e.g. a hull carried along by a transform.</summary>
        public void Set(in ObjectFrame frame, object samples, object geometry, Func<T> compute)
        {
            _entry = new Entry(frame, samples, geometry, compute);
        }

        public bool Equals(Memo<T> other) => true;
        public override bool Equals(object obj) => obj is Memo<T>;
        public override int GetHashCode() => 0;
    }
}

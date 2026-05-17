using System;
using RA3WaterSystem.Data;
using Unity.Mathematics;

namespace RA3WaterSystem
{
    /// <summary>
    /// Analytic Gerstner evaluation matching the legacy Job implementation’s <c>HeightJob.Execute</c> logic—for sparse / offline /
    /// editor sampling without scheduling jobs or registering GUIDs.
    /// </summary>
    public static class GerstnerWavesAnalytic
    {
        public readonly struct GerstnerSampleResult
        {
            /// <summary>Gerstner displacement accumulated from zero—the same semantics as Job <c>OutPosition</c> (xz horizontal, y vertical).</summary>
            public float3 Displacement { get; }
            /// <summary>Unit surface normal—the same semantics as Job <c>OutNormal</c>.</summary>
            public float3 Normal { get; }

            public GerstnerSampleResult(float3 displacement, float3 normal)
            {
                Displacement = displacement;
                Normal = normal;
            }

            /// <summary>Sea surface world Y when the flat baseline (mean plane) is <paramref name="baselineWorldY"/>.</summary>
            public float SurfaceWorldY(float baselineWorldY) => baselineWorldY + Displacement.y;
        }

        /// <inheritdoc cref="Evaluate(System.ReadOnlySpan{RA3WaterSystem.Data.Wave},Unity.Mathematics.float3,float)"/>
        public static GerstnerSampleResult Evaluate(Wave[] waves, float3 worldPosition, float time) =>
            waves == null
                ? throw new ArgumentNullException(nameof(waves))
                : Evaluate(new ReadOnlySpan<Wave>(waves), worldPosition, time);

        /// <summary>
        /// Evaluates Gerstner height offset and tilt at <paramref name="worldPosition"/>.
        /// Only <c>xz</c> of <paramref name="worldPosition"/> participates in phase (matches Job behavior).
        /// </summary>
        public static GerstnerSampleResult Evaluate(ReadOnlySpan<Wave> waves, float3 worldPosition, float time)
        {
            if (waves.Length == 0)
            {
                return new GerstnerSampleResult(float3.zero, new float3(0f, 1f, 0f));
            }

#if STATIC_EVERYTHING
            var t = 0.0f;
#else
            var t = time;
#endif

            var waveCountMulti = 1f / waves.Length;
            var wavePos = new float3(0f, 0f, 0f);
            var waveNorm = new float3(0f, 0f, 0f);

            var pos = worldPosition.xz;

            for (var waveIdx = 0; waveIdx < waves.Length; waveIdx++)
            {
                var wd = waves[waveIdx];

                var amplitude = wd.amplitude;
                var direction = wd.direction;
                var wavelength = wd.wavelength;
                var omniPos = wd.origin;

                var w = 6.28318f / wavelength;
                var wSpeed = math.sqrt(9.8f * w);
                const float peak = 0.8f;
                var qi = peak / (amplitude * w * waves.Length);

                var windDir = new float2(0f, 0f);

                direction = math.radians(direction);
                var windDirInput = new float2(math.sin(direction), math.cos(direction)) * (1 - wd.onmiDir);
                var windOmniInput = (pos - omniPos) * wd.onmiDir;

                windDir += windDirInput;
                windDir += windOmniInput;
                windDir = math.normalize(windDir);
                var dir = math.dot(windDir, pos - (omniPos * wd.onmiDir));

                var calc = dir * w + -t * wSpeed;
                var cosCalc = math.cos(calc);
                var sinCalc = math.sin(calc);

                wavePos.x += qi * amplitude * windDir.x * cosCalc;
                wavePos.z += qi * amplitude * windDir.y * cosCalc;
                wavePos.y += sinCalc * amplitude * waveCountMulti;

                var wa = w * amplitude;
                var norm = new float3(-(windDir.xy * wa * cosCalc),
                    1 - (qi * wa * sinCalc));
                waveNorm += (norm * waveCountMulti) * amplitude;
            }

            return new GerstnerSampleResult(wavePos, math.normalize(waveNorm.xzy));
        }
    }
}

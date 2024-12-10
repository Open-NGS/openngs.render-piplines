using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OpenNGS.RenderPipline.Editor
{
    public static class ColorExtensions
    {
        public static void ToLDR(this Color hdr, out Color ldr, out float intensity)
        {
            // specifies the max byte value to use when decomposing a float color into bytes with exposure
            // this is the value used by Photoshop
            const float k_MaxByteForOverexposedColor = 191;

            hdr.a = 1.0f;
            ldr = hdr;
            intensity = 1.0f;

            var maxColorComponent = hdr.maxColorComponent;
            if (maxColorComponent != 0f)
            {
                // calibrate exposure to the max float color component
                var scaleFactor = k_MaxByteForOverexposedColor / maxColorComponent;

                ldr.r = Mathf.Min(k_MaxByteForOverexposedColor, scaleFactor * hdr.r) / 255f;
                ldr.g = Mathf.Min(k_MaxByteForOverexposedColor, scaleFactor * hdr.g) / 255f;
                ldr.b = Mathf.Min(k_MaxByteForOverexposedColor, scaleFactor * hdr.b) / 255f;

                intensity = 255f / scaleFactor;
            }
        }
    }
}

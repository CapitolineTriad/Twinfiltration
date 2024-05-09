using UnityEngine;

namespace Twinfiltration
{
    public struct SpringCoefs
    {
        public float positionPosCoef, positionVelCoef;
        public float velocityPosCoef, velocityVelCoef;
    }

    public static class SpringUtils
    {
        public static bool Approximately(float f1, float f2, float epsilon = 0.01f)
        {
            return f1 - epsilon < f2 && f1 + epsilon > f2;
        }

        public static void UpdateDampedSpringCoef(ref SpringCoefs springCoefs, float deltaTime, float angularFrequency, float dampingRatio)
        {
            if (dampingRatio < 0f) dampingRatio = 0f;
            if (angularFrequency < 0f) angularFrequency = 0f;

            // The spring will not move so return.
            if (angularFrequency < Mathf.Epsilon)
                return;

            // Over-damped spring, will not bounce but will reach position slower.
            if (dampingRatio > 1f + Mathf.Epsilon)
            {
                float za = -angularFrequency * dampingRatio;
                float zb = angularFrequency * Mathf.Sqrt(dampingRatio * dampingRatio - 1f);
                float z1 = za - zb;
                float z2 = za + zb;

                float e1 = Mathf.Exp(z1 * deltaTime);
                float e2 = Mathf.Exp(z2 * deltaTime);

                float invTwoZb = 1f / (2f * zb);

                float e1OverTwoZb = e1 * invTwoZb;
                float e2OverTwoZb = e2 * invTwoZb;

                float z1e1OverTwoZb = z1 * e1OverTwoZb;
                float z2e2OverTwoZb = z2 * e2OverTwoZb;

                springCoefs.positionPosCoef = e1OverTwoZb * z2 - z2e2OverTwoZb + e2;
                springCoefs.positionVelCoef = -e1OverTwoZb + e2OverTwoZb;

                springCoefs.velocityPosCoef = (z1e1OverTwoZb - z2e2OverTwoZb + e2) * z2;
                springCoefs.velocityVelCoef = -z1e1OverTwoZb + z2e2OverTwoZb;
            }
            // Under-damped spring, will bounce until it reaches equilibrium.
            else if (dampingRatio < 1f - Mathf.Epsilon)
            {
                float omegaZeta = angularFrequency * dampingRatio;
                float alpha = angularFrequency * Mathf.Sqrt(1f - dampingRatio * dampingRatio);

                float expTerm = Mathf.Exp(-omegaZeta * deltaTime);
                float cosTerm = Mathf.Cos(alpha * deltaTime);
                float sinTerm = Mathf.Sin(alpha * deltaTime);

                float invAlpha = 1f / alpha;

                float expSin = expTerm * sinTerm;
                float expCos = expTerm * cosTerm;
                float expOmegaZetaSinOverAlpha = expTerm * omegaZeta * sinTerm * invAlpha;

                springCoefs.positionPosCoef = expCos + expOmegaZetaSinOverAlpha;
                springCoefs.positionVelCoef = expSin * invAlpha;

                springCoefs.velocityPosCoef = -expSin * alpha - omegaZeta * expOmegaZetaSinOverAlpha;
                springCoefs.velocityVelCoef = expCos - expOmegaZetaSinOverAlpha;
            }
            // Critically damped spring, will reach equilibrium in minimal time without bouncing.
            else
            {
                float expTerm = Mathf.Exp(-angularFrequency * deltaTime);
                float timeExp = deltaTime * expTerm;
                float timeExpFreq = timeExp * angularFrequency;

                springCoefs.positionPosCoef = timeExpFreq + expTerm;
                springCoefs.positionVelCoef = timeExp;

                springCoefs.velocityPosCoef = -angularFrequency * timeExpFreq;
                springCoefs.velocityVelCoef = -timeExpFreq + expTerm;
            }
        }

        public static void UpdateDampedSpringMotion(ref float currVal, ref float currVelocity, float targetVal, SpringCoefs springCoefs)
        {
            float oldPos = currVal - targetVal; // Update position in equilibrium-relative space.
            float oldVelocity = currVelocity;

            currVal = oldPos * springCoefs.positionPosCoef + oldVelocity * springCoefs.positionVelCoef + targetVal;
            currVelocity = oldPos * springCoefs.velocityPosCoef + oldVelocity * springCoefs.velocityVelCoef;
        }

        public static void UpdateDampedSpringMotion(ref float currVal, ref float currVelocity, ref SpringCoefs springCoefs, float targetVal, float deltaTime, float angularFrequency, float dampingRatio)
        {
            if (dampingRatio < 0f) dampingRatio = 0f;
            if (angularFrequency < 0f) angularFrequency = 0f;

            // The spring will not move so return.
            if (angularFrequency < Mathf.Epsilon)
                return;

            UpdateDampedSpringCoef(ref springCoefs, deltaTime, angularFrequency, dampingRatio);
            UpdateDampedSpringMotion(ref currVal, ref currVelocity, targetVal, springCoefs);
        }
    }
}

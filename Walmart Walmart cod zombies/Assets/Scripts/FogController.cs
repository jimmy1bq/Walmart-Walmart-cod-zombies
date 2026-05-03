using UnityEngine;

namespace UnityEffects.Fog
{

    public class FogController : MonoBehaviour
    {
        [SerializeField] private Color fogColor = new Color(0.603922f, 0.686275f, 0.733333f);

        [SerializeField] private FogMode fogMode = FogMode.ExponentialSquared;

        // Tune this to control how quickly fog thickens. Lower = thinner/longer range.
        [SerializeField][Range(0.001f, 0.5f)] private float fogDensity = 0.04f;

        // Only used when fogMode is set to Linear.
        [SerializeField] private float fogStartDistance = 1f;
        [SerializeField] private float fogEndDistance = 50f;

        private void Start()
        {
            Apply();
        }

        public void Apply()
        {
            RenderSettings.fog = true;
            RenderSettings.fogColor = fogColor;
            RenderSettings.fogMode = fogMode;

            if (fogMode == FogMode.Linear)
            {
                RenderSettings.fogStartDistance = fogStartDistance;
                RenderSettings.fogEndDistance = fogEndDistance;
            }
            else
            {
                RenderSettings.fogDensity = fogDensity;
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            Apply();
        }
#endif
    }
}
using UnityEngine;

namespace UnityEffects.Fog
{
    /// Applies the atmospheric fog settings ported from world_env_default.tres.
    /// Attach this to any persistent GameObject in your scene (e.g. GameManager).
    public class FogController : MonoBehaviour
    {
        [SerializeField] private Color fogColor = new Color(0.603922f, 0.686275f, 0.733333f);

        // fog_depth_begin = 1.0 in Godot
        [SerializeField] private float fogStartDistance = 1f;

        // Godot's fog_depth_curve (0.31864) controls falloff shape.
        // In Unity Linear mode, tune fogEndDistance to match visual density.
        [SerializeField] private float fogEndDistance = 50f;

        [SerializeField] private FogMode fogMode = FogMode.Linear;

        private void Start()
        {
            Apply();
        }

        public void Apply()
        {
            RenderSettings.fog = true;
            RenderSettings.fogColor = fogColor;
            RenderSettings.fogMode = fogMode;
            RenderSettings.fogStartDistance = fogStartDistance;
            RenderSettings.fogEndDistance = fogEndDistance;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            Apply();
        }
#endif
    }
}

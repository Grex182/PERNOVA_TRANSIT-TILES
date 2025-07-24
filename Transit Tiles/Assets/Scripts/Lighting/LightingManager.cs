using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class LightingManager : MonoBehaviour
{
    //References
    [SerializeField] private Light DirectionalLight;
    [SerializeField] private List<Light> TrainLights = new List<Light>();
    [SerializeField] private List<Light> StationLights = new List<Light>();
    [SerializeField] private LightingPreset Preset;
    //Variables
    [SerializeField, Range(0f, 24f)] private float TimeOfDay;
    [SerializeField] float timeSpeed;
    [SerializeField] Vector3 multiplyAngle;
    [SerializeField] Vector3 additionalAngle;
    

    private void Update()
    {
        if (Preset == null)
            return;

        if(Application.isPlaying)
        {
            TimeOfDay += Time.deltaTime * timeSpeed;
            TimeOfDay %= 24;
            UpdateLighting(TimeOfDay / 24f);
        }
        else
        {
            UpdateLighting(TimeOfDay / 24f);
        }
    }

    private void UpdateLighting(float timePercent)
    {
        RenderSettings.ambientLight = Preset.AmbientColor.Evaluate(timePercent);
        RenderSettings.fogColor = Preset.FogColor.Evaluate(timePercent);

        if(DirectionalLight!=null)
        {
            DirectionalLight.color = Preset.DirectionalColor.Evaluate(timePercent);
            DirectionalLight.transform.localRotation = Quaternion.Euler(new Vector3((timePercent * multiplyAngle.x)+additionalAngle.x, (timePercent * multiplyAngle.y) + additionalAngle.y, (timePercent * multiplyAngle.z) + additionalAngle.z));
        }
    }

    public void GetSceneLights()
    {
        Light[] lights = GameObject.FindObjectsOfType<Light>();
        foreach (Light light in lights)
        {
            if (light.type == LightType.Point)
            {
                TrainLights.Add(light);
                return;
            }
            if (light.type == LightType.Spot)
            {
                StationLights.Add(light);
                return;
            }
        }
    }


    private void OnValidate()
    {
            if (DirectionalLight != null)
            return;
        
        if(RenderSettings.sun != null)
        {
            DirectionalLight = RenderSettings.sun;
        }
        else
        {
            Light[] lights = GameObject.FindObjectsOfType<Light>();
            foreach (Light light in lights)
            {
                if (light.type == LightType.Directional)
                {
                    DirectionalLight = light;
                    return;
                }
                
            }
        }
    }
}

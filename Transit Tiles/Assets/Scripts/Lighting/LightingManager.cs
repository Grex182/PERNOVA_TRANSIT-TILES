using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class LightingManager : MonoBehaviour
{
    //References
    [SerializeField] private Light DirectionalLight;
    [SerializeField] private Light[] TrainLights;
    [SerializeField] private float TrainLightPower;
    [SerializeField] private Light[] StationLights;
    [SerializeField] private float StationLightPower;
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
            UpdateLights(TimeOfDay);

        }
        else
        {
            UpdateLighting(TimeOfDay / 24f);
            UpdateLights(TimeOfDay);
        }
    }

    private void UpdateLights(float timeDay)
    {
        float LightValue = 0.05f * (timeDay - 5f) * (timeDay - 19f);
        LightValue = Mathf.Clamp01(LightValue);

        foreach (Light light in TrainLights)
        {
            light.intensity = LightValue * TrainLightPower;
        }
        foreach (Light light in StationLights)
        {
            light.intensity = LightValue * StationLightPower;
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TornadoBanditsStudio.LowPolyDungeonPack
{
	/// <summary>
	/// Light intensity bounce
	/// </summary>
	public class TBS_LightBounce : MonoBehaviour 
	{
		[SerializeField] private float duration = 1f;
        private float maxLightIntenstity = 2;
		private Light light;
		private float activatedTime;

		void Awake ()
		{
			light = this.GetComponent<Light> ();
            maxLightIntenstity = light.intensity;
        }

		void OnEnable ()
		{
			activatedTime = Time.time;
		}

		void Update ()
		{
			float phi = (Time.time - activatedTime)/ duration * 2f * Mathf.PI;
			float intensity = Mathf.Cos (phi) * 0.5f + 0.5f;
			light.intensity = intensity * maxLightIntenstity;
		}
	}
}

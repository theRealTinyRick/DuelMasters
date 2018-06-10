using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TornadoBanditsStudio.LowPolyDungeonPack
{
	[RequireComponent (typeof (Light))]
	/// <summary>
	/// Light flickering effect
	/// </summary>
	public class TBS_FlickeringLight : MonoBehaviour 
	{
		[SerializeField] AnimationCurve lightIntenstityCurve = AnimationCurve.EaseInOut (0, 0, 1, 1);

		private float activatedTime;
		private Light light;

		[Range (0, 8)][SerializeField] private float intensityToReach = 5;
		[SerializeField] private float speed = 1;

		void Awake ()
		{
			light = this.GetComponent<Light> ();
		}

		void OnEnable ()
		{
			//Set the time when it was activated
			activatedTime = Time.time;
		}

		void Update ()
		{
			float time = Time.time - activatedTime;
			float evaluateCurve = lightIntenstityCurve.Evaluate (time * speed) * intensityToReach;
			light.intensity = evaluateCurve;
		}
	}
}

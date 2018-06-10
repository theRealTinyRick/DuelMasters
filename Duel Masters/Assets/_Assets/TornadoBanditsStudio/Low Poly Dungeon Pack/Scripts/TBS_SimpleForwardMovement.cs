using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TornadoBanditsStudio.LowPolyDungeonPack
{
	/// <summary>
	/// Simple forward movement
	/// </summary>
    public class TBS_SimpleForwardMovement : MonoBehaviour
    {
        [SerializeField]
        private float speed_ = 2f; //The speed of movement

        void Update ()
        {
            //Translate forward this transform with a speed per second.
            this.transform.Translate (Vector3.forward * speed_ * Time.deltaTime);
        }
    }
}

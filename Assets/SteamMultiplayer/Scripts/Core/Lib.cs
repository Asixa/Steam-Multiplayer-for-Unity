using System;
using UnityEngine;

namespace SteamMultiplayer
{
    public class Lib : MonoBehaviour
    {
        #region 重写Unity数学模型

        #region 重写Vector3

        [Serializable]
        public struct M_Vector3
        {
            public float x, y, z;

            public M_Vector3(Vector3 v)
            {
                x = v.x;
                y = v.y;
                z = v.z;
            }
        }

        public static Vector3 To_Vector3(M_Vector3 input)
        {
            return new Vector3(input.x, input.y, input.z);
        }

        #endregion

        #region 重写 Quaternion

        [Serializable]
        public struct M_Quaternion
        {
            public float x, y, z, w;
            public M_Vector3 eulerAngles;

            public M_Quaternion(Quaternion input)
            {
                x = input.x;
                y = input.y;
                z = input.z;
                w = input.w;
                eulerAngles = new M_Vector3(input.eulerAngles);
            }
        }

        public static Quaternion To_Quaternion(M_Quaternion input)
        {
            return new Quaternion
            {
                x = input.x,
                y = input.y,
                z = input.z,
                w = input.w,
                eulerAngles = To_Vector3(input.eulerAngles)
            };
        }

        #endregion

        #endregion
    }
}

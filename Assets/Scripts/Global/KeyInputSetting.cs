#if UNITY_WEBGL && !UNITY_EDITOR
using UnityEngine;

namespace Global
{
    public class KeyInputSetting : MonoBehaviour
    {

        private void Start()
        {
            WebGLInput.mobileKeyboardSupport = true;
            WebGLInput.captureAllKeyboardInput = false;
        }
    }
}
#endif
using UnityEngine;

namespace Script
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
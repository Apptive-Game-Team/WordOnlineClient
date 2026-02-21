using UnityEngine;

namespace Scripts.Global
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
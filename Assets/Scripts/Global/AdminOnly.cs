using Global.Util;
using UnityEngine;

namespace Global
{
    /// <summary>
    /// Attach this component to any GameObject that should only be visible to admin users.
    /// The GameObject is deactivated during Awake when the current JWT token does not
    /// contain the ADMIN role.
    ///
    /// Note: The check runs once at scene load. If the user re-authenticates
    /// with a new token during the same session, reload the scene to re-evaluate.
    /// </summary>
    public class AdminOnly : MonoBehaviour
    {
        private void Awake()
        {
            if (SceneContext.JwtToken == null || !JwtHelper.IsAdmin(SceneContext.JwtToken))
            {
                gameObject.SetActive(false);
            }
        }
    }
}

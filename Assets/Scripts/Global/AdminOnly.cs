using System.Collections;
using Global.Util;
using UnityEngine;

namespace Global
{
    /// <summary>
    /// Attach this component to any GameObject that should only be visible to admin users.
    /// The GameObject is deactivated when the current JWT token does not contain the
    /// ADMIN role, or when JWKS cannot be fetched or verified — the safe default is
    /// hidden. Other components on the same GameObject (e.g. <c>MatchingServerDropdown</c>)
    /// rely on the GameObject already being inactive by the time their own Start() would
    /// run, so hiding happens synchronously in Awake() whenever possible.
    ///
    /// Three flows set <see cref="SceneContext.JwtToken"/> (normal login, guest login,
    /// registration), but only normal login pre-fetches JWKS. This component is the only
    /// caller of <see cref="JwtHelper.IsAdmin"/>, so instead of adding a JWKS fetch to
    /// every login flow, it fetches JWKS itself, once, right before deciding, when the
    /// cache is still empty. <see cref="JwksService"/> caches the result, so this is a
    /// no-op after the first successful fetch in the session.
    ///
    /// When JWKS still needs to be fetched, this GameObject is deactivated immediately
    /// (safe default) and the fetch runs as a coroutine on <see cref="SceneContext"/>
    /// instead of on this component, because Unity refuses to run a coroutine on an
    /// inactive GameObject. The GameObject is reactivated afterward if the token turns
    /// out to belong to an admin.
    ///
    /// Note: The check runs once at scene load. If the user re-authenticates
    /// with a new token during the same session, reload the scene to re-evaluate.
    /// </summary>
    public class AdminOnly : MonoBehaviour
    {
        private void Awake()
        {
            if (SceneContext.JwtToken == null)
            {
                gameObject.SetActive(false);
                return;
            }

            if (JwksService.IsFetched)
            {
                ApplyAdminVisibility();
                return;
            }

            // Safe default while JWKS is missing: hide now, decide for real once fetched.
            gameObject.SetActive(false);
            WDebug.LogWarning("[AdminOnly] JWKS not fetched yet; fetching before evaluating admin visibility.");

            if (SceneContext.Instance != null)
            {
                SceneContext.Instance.StartCoroutine(FetchJwksThenReveal());
            }
            else
            {
                WDebug.LogError("[AdminOnly] SceneContext.Instance is missing; cannot fetch JWKS, admin-only UI stays hidden.");
            }
        }

        private IEnumerator FetchJwksThenReveal()
        {
            yield return JwksService.FetchJwks();

            if (this == null)
                yield break; // the scene this GameObject lived in was already unloaded

            if (!JwksService.IsFetched)
            {
                WDebug.LogWarning("[AdminOnly] JWKS fetch failed; admin-only UI stays hidden.");
                return;
            }

            ApplyAdminVisibility();
        }

        private void ApplyAdminVisibility()
        {
            gameObject.SetActive(JwtHelper.IsAdmin(SceneContext.JwtToken));
        }
    }
}

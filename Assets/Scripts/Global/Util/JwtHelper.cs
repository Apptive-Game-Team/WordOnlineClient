using System;
using System.Text;
using UnityEngine;

namespace Global.Util
{
    /// <summary>
    /// Utility class for reading JWT token claims on the client side.
    ///
    /// SECURITY NOTE: The JWT signature is intentionally NOT verified here.
    /// This class is used only for UI visibility decisions (e.g. showing admin
    /// panels). All privileged operations must be enforced server-side.
    /// </summary>
    public static class JwtHelper
    {
        [Serializable]
        private class JwtPayload
        {
            public string scope;
        }

        /// <summary>
        /// Decodes the payload of a JWT token without verifying the signature
        /// and returns the decoded JSON string.
        /// </summary>
        public static string DecodePayload(string jwtToken)
        {
            if (string.IsNullOrEmpty(jwtToken))
                return null;

            string[] parts = jwtToken.Split('.');
            if (parts.Length != 3)
                return null;

            string payload = parts[1];

            // Base64URL → Base64 conversion
            payload = payload.Replace('-', '+').Replace('_', '/');
            switch (payload.Length % 4)
            {
                case 2: payload += "=="; break;
                case 3: payload += "=";  break;
            }

            try
            {
                byte[] bytes = Convert.FromBase64String(payload);
                return Encoding.UTF8.GetString(bytes);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Extracts the roles from the JWT payload's "scope" claim.
        /// The scope is a space-separated string, e.g.
        /// "WORDONLINE_ADMIN WORDONLINE_USER WORDONLINE_CICD SUPER_ADMIN".
        /// Returns an empty array if no scope is found.
        /// </summary>
        public static string[] ExtractRoles(string jwtToken)
        {
            string payloadJson = DecodePayload(jwtToken);
            if (string.IsNullOrEmpty(payloadJson))
                return Array.Empty<string>();

            try
            {
                JwtPayload payload = JsonUtility.FromJson<JwtPayload>(payloadJson);
                if (payload == null || string.IsNullOrEmpty(payload.scope))
                    return Array.Empty<string>();

                return payload.scope.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            }
            catch (Exception)
            {
                // Ignore malformed payloads
            }

            return Array.Empty<string>();
        }

        /// <summary>
        /// Returns true when the JWT token's scope contains "WORDONLINE_ADMIN"
        /// or "SUPER_ADMIN" (case-insensitive).
        /// </summary>
        public static bool IsAdmin(string jwtToken)
        {
            string[] roles = ExtractRoles(jwtToken);
            foreach (string role in roles)
            {
                if (string.Equals(role, "WORDONLINE_ADMIN", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(role, "SUPER_ADMIN", StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }
    }
}

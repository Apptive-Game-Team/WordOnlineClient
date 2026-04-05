using System;
using UnityEngine;

namespace Global.Util
{
    /// <summary>
    /// Represents a single key entry in a JSON Web Key Set (JWKS).
    /// </summary>
    [Serializable]
    public class JwksKey
    {
        public string kty; // Key type: "RSA"
        public string kid; // Key ID
        public string use; // Usage: "sig"
        public string alg; // Algorithm: "RS256"
        public string n;   // RSA modulus (Base64URL-encoded)
        public string e;   // RSA exponent (Base64URL-encoded)
    }

    /// <summary>
    /// Represents a JSON Web Key Set (JWKS) response from the account server.
    /// </summary>
    [Serializable]
    internal class JwksResponse
    {
        public JwksKey[] keys;
    }
}

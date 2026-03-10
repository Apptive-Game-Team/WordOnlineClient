using System;
using System.Collections;
using UnityEngine;

namespace Data.Adventures
{
    public abstract class AdventureClient : MonoBehaviour
    {
        public abstract IEnumerator GetAdventure(Action<AdventuresResponse> callback);
    }
}
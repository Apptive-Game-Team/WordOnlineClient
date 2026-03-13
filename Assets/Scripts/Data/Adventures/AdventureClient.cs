using System;
using System.Collections;
using Data.Adventures.Dto;
using UnityEngine;

namespace Data.Adventures
{
    public abstract class AdventureClient : MonoBehaviour
    {
        public abstract IEnumerator GetAdventure(Action<AdventuresResponseDto> callback);
    }
}
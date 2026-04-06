using System.Collections.Generic;

namespace Simulation.Core
{
    /// <summary>
    /// Tracks native and temporary (bonus) element types on a game object.
    /// </summary>
    public class SimElement
    {
        private readonly HashSet<ElementType> _nativeSet = new() { ElementType.NONE };
        private readonly Dictionary<ElementType, HashSet<object>> _bonusSources = new();

        public HashSet<ElementType> Total()
        {
            var result = new HashSet<ElementType>(_nativeSet);
            foreach (var kv in _bonusSources)
            {
                if (kv.Value.Count > 0)
                    result.Add(kv.Key);
            }
            if (result.Count == 0) result.Add(ElementType.NONE);
            return result;
        }

        public bool Has(ElementType e) => _nativeSet.Contains(e) || (_bonusSources.ContainsKey(e) && _bonusSources[e].Count > 0);
        public bool NativeHas(ElementType e) => _nativeSet.Contains(e);

        public void AddNative(ElementType e) { _nativeSet.Remove(ElementType.NONE); _nativeSet.Add(e); }
        public void RemoveNative(ElementType e) { _nativeSet.Remove(e); if (_nativeSet.Count == 0) _nativeSet.Add(ElementType.NONE); }

        public void AddBonus(ElementType e, object source)
        {
            if (!_bonusSources.ContainsKey(e))
                _bonusSources[e] = new HashSet<object>();
            _bonusSources[e].Add(source);
        }

        public void RemoveBonus(ElementType e, object source)
        {
            if (_bonusSources.ContainsKey(e))
                _bonusSources[e].Remove(source);
        }

        public void ClearBonusFrom(object source)
        {
            foreach (var set in _bonusSources.Values)
                set.Remove(source);
        }
    }
}

using System;
using System.Collections.Generic;

namespace Simulation.Core
{
    /// <summary>
    /// Core simulation entity. Mirrors the server's GameObject:
    /// holds components, colliders, position, element, status, and lifecycle.
    /// </summary>
    public class SimGameObject
    {
        private static int _nextId;
        public static void ResetIdCounter() => _nextId = 0;

        public int Id { get; }
        public Master Master { get; }
        public PrefabType Type { get; }
        public SimStatus Status { get; private set; }
        public Effect Effect { get; private set; }
        public SimElement Element { get; } = new();
        public SimVector3 Position { get; private set; }
        public SimWorld World { get; }

        private readonly List<SimComponent> _components = new();
        private readonly List<SimComponent> _toAdd = new();
        private readonly List<SimComponent> _toRemove = new();

        public bool IsActive => !IsDestroyed && IsInitialized;
        public bool IsDestroyed => Status == SimStatus.Destroyed;
        public bool IsInitialized => Status != SimStatus.Initializing;

        // ── Constructors ──

        public SimGameObject(Master master, PrefabType type, SimVector3 position, SimWorld world)
        {
            Status = SimStatus.Initializing;
            Id = _nextId++;
            Master = master;
            Type = type;
            Position = position;
            World = world;
            world.RegisterGameObject(this);
        }

        public SimGameObject(SimGameObject parent, PrefabType type)
            : this(parent.Master, type, parent.Position, parent.World) { }

        public SimGameObject(SimGameObject parent, Master master, PrefabType type)
            : this(master, type, parent.Position, parent.World) { }

        // ── Component access ──

        public T GetComponent<T>() where T : SimComponent
        {
            foreach (var c in _components)
                if (c is T t) return t;
            return null;
        }

        public bool HasComponent<T>() where T : SimComponent
        {
            foreach (var c in _components)
                if (c is T) return true;
            return false;
        }

        public List<T> GetComponents<T>() where T : SimComponent
        {
            var result = new List<T>();
            foreach (var c in _components)
                if (c is T t) result.Add(t);
            return result;
        }

        public void AddComponent(SimComponent component)
        {
            component.GameObject = this;
            _toAdd.Add(component);
        }

        public void RemoveComponent(SimComponent component)
        {
            _toRemove.Add(component);
        }

        // ── State setters ──

        public void SetPosition(SimVector3 pos)
        {
            Position = pos;
            Fix64 dx = SimMath.Abs(pos.X - Fix64.FromInt(SimGameConfig.X_MID));
            Fix64 dy = SimMath.Abs(pos.Y - Fix64.FromInt(SimGameConfig.Y_MID));
            if (dx > Fix64.FromInt(SimGameConfig.X_BOUND) || dy > Fix64.FromInt(SimGameConfig.Y_BOUND))
            {
                Destroy();
                return;
            }
            World.MarkUpdated(this);
        }

        public void SetStatus(SimStatus status)
        {
            if (Status == SimStatus.Destroyed) return;
            Status = status;
            World.MarkUpdated(this);
        }

        public void SetEffect(Effect effect)
        {
            Effect = effect;
            World.MarkUpdated(this);
        }

        public void SetElement(ElementType e) => Element.AddNative(e);

        public void SetElement(HashSet<ElementType> elements)
        {
            foreach (var e in elements) Element.AddNative(e);
        }

        public void AddTempElement(ElementType e, object source) => Element.AddBonus(e, source);
        public void RemoveTempElement(ElementType e, object source) => Element.RemoveBonus(e, source);

        // ── Lifecycle ──

        public void Start()
        {
            SimPrefabFactory.Initialize(this);
            FlushComponentQueues();
            foreach (var c in _components)
                c.Start();
            SetStatus(SimStatus.Idle);
        }

        public void Update()
        {
            FlushComponentQueues();
            foreach (var c in _components)
                c.Update();
        }

        public void Destroy()
        {
            SetStatus(SimStatus.Destroyed);
            OnDestroy();
        }

        private void OnDestroy()
        {
            foreach (var c in _components)
                c.OnDestroy();
        }

        private void FlushComponentQueues()
        {
            if (_toAdd.Count > 0)
            {
                _components.AddRange(_toAdd);
                _toAdd.Clear();
            }
            if (_toRemove.Count > 0)
            {
                foreach (var c in _toRemove)
                    _components.Remove(c);
                _toRemove.Clear();
            }
        }
    }
}

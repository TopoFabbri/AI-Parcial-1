using System;
using System.Collections.Generic;

namespace Model.Tools.ECS.Patron
{
    public class EcsEntity
    {
        private readonly List<Type> componentsType;

        private readonly uint id;

        public EcsEntity()
        {
            id = EntityID.GetNew();
            componentsType = new List<Type>();
        }

        public uint GetID()
        {
            return id;
        }

        public void Dispose()
        {
            componentsType.Clear();
        }

        public void AddComponentType<TComponentType>() where TComponentType : EcsComponent
        {
            AddComponentType(typeof(TComponentType));
        }

        public void AddComponentType(Type componentType)
        {
            componentsType.Add(componentType);
        }

        public bool ContainsComponentType<ComponentType>() where ComponentType : EcsComponent
        {
            return ContainsComponentType(typeof(ComponentType));
        }

        public bool ContainsComponentType(Type componentType)
        {
            return componentsType.Contains(componentType);
        }

        private static class EntityID
        {
            private static uint _lastEntityID;

            internal static uint GetNew()
            {
                return _lastEntityID++;
            }
        }
    }
}
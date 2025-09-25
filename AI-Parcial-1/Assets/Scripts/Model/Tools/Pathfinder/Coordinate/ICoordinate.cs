using System;
using System.Collections.Generic;

namespace Model.Tools.Pathfinder.Coordinate
{
    public interface ICoordinate : IEquatable<ICoordinate>
    {
        int GetDistanceTo(ICoordinate other);
        List<ICoordinate> GetAdjacents();
        void Set(params object[] parameters);
    }
}
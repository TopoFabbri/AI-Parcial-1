using System;
using System.Collections.Generic;

namespace Model.Tools.Pathfinder.Coordinate
{
    public interface ICoordinate : IEquatable<ICoordinate>
    {
        float GetDistanceTo(ICoordinate other);
        List<ICoordinate> GetAdjacents();
    }
}
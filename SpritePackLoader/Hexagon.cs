using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SpritePackLoader.Hexagons
{
    public struct Hexagon
    {
        private const float Sqrt3 = 1.732050808f;
        private const float Sqrt3Over2 = 0.866025403f;
        private static readonly Hexagon[] directions = new Hexagon[6]
        {
            new Hexagon(1, -1, 0), new Hexagon(1, 0, -1), new Hexagon(0, 1, -1),
            new Hexagon(-1, 1, 0), new Hexagon(-1, 0, 1), new Hexagon(0, -1, 1)
        };
        public Hexagon this[int direction] => this + directions[direction];

        public Hexagon(int x, int y, int z) { X = x; Y = y; Z = z; }
        public int X, Y, Z;
        public Vector3 ToVector(float size) => size * new Vector3(1.5f * X, Sqrt3Over2 * X + Sqrt3 * Z);

        public static IEnumerable<Hexagon> Ring(Hexagon center, int radius)
        {
            Hexagon coord = center + directions[4] * radius;
            for (int i = 0; i < 6; i++)
                for (int j = 0; j < radius; j++)
                {
                    yield return coord;
                    coord = coord[i];
                }
        }
        public static IEnumerable<Hexagon> Spiral(Hexagon center, int radius)
        {
            for (int k = 1; k <= radius; k++)
                foreach (Hexagon hexagon in Ring(center, k))
                    yield return hexagon;
        }

        public static Hexagon operator +(Hexagon a, Hexagon b) => new Hexagon(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        public static Hexagon operator *(Hexagon hexagon, int scale) => new Hexagon(hexagon.X * scale, hexagon.Y * scale, hexagon.Z * scale);

    }
}

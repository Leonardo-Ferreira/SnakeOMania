using System;
using System.Drawing;

namespace SnakeOMania.Library.Physics
{
    public class CollisionChecker
    {
        public static bool Collided(int x1, int x2, int xp, int y1, int y2, int yp)
        {
            var AB = Math.Sqrt((x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1));
            var AP = Math.Sqrt((xp - x1) * (xp - x1) + (yp - y1) * (yp - y1));
            var PB = Math.Sqrt((x2 - xp) * (x2 - xp) + (y2 - yp) * (y2 - yp));
            return AB == AP + PB;
        }

        public static bool Collided(Point segmentStart, Point segmentEnd, Point collisionCandidate)
        {
            int x1, x2, xp, y1, y2, yp;
            x1 = segmentStart.X;
            y1 = segmentStart.Y;
            x2 = segmentEnd.X;
            y2 = segmentEnd.Y;
            xp = collisionCandidate.X;
            yp = collisionCandidate.Y;

            return Collided(x1, x2, xp, y1, y2, yp);
        }
    }
}

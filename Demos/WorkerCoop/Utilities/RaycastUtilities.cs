using Client;
using Godot;
using Godot.Collections;
using System.Collections.Generic;
using System.Linq;

public static class RaycastUtilities
{
    //Sorted by scene hierarchy
    public static List<Dictionary> SortedIntersectPoints(this Array<Dictionary> unsortedResult)
    {
        return unsortedResult.ToList().OrderBy(hit => hit, new RayCastHitComparer()).ToList();
    }

    public static Array<Dictionary> PerformIntersectPoint2D(Vector2 clickedPoint, bool collideWithAreas = true)
    {
        ClientGameSession _cgs = ClientGameSession.Singleton;
        var query2D = new PhysicsPointQueryParameters2D()
        {
            CollideWithAreas = true,
            Position = clickedPoint
        };

        var physicsSpace = _cgs.GetViewport().World2D.DirectSpaceState;
        return physicsSpace.IntersectPoint(query2D);
    }
    public static Dictionary PerformIntersectRay3D(Vector2 clickedPoint, bool collideWithAreas = true)
    {
        ClientGameSession _cgs = ClientGameSession.Singleton;
        var space = _cgs.GetViewport().World3D.DirectSpaceState;

        var from = _cgs.Camera3D.ProjectRayOrigin(clickedPoint);
        var to = from + _cgs.Camera3D.ProjectRayNormal(clickedPoint) * _cgs.Camera3D.Far;
        var query3D = new PhysicsRayQueryParameters3D() { From = from, To = to, CollideWithAreas = collideWithAreas };
        return space.IntersectRay(query3D);
    }

    class RayCastHitComparer : IComparer<Dictionary>
    {
        public int Compare(Dictionary? hit1, Dictionary? hit2)
        {
            var area1 = hit1!["collider"].Obj as Area2D;
            var area2 = hit2!["collider"].Obj as Area2D;

            if (area1 != null && area2 != null)
            {
                if (area1.IsGreaterThan(area2))
                {
                    return 1;
                }
                else if (area2.IsGreaterThan(area1))
                {
                    return -1;
                }
            }
            return 0;
        }
    }
}

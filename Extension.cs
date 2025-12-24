using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using System.Collections.Generic;
using System.Linq;

namespace ABS_WIZZ
{
    public static class Extension
    {
        // -----------------------------------
        // GET HOST ROOMS
        // -----------------------------------
        public static IList<Room> GetRooms(this Document doc)
        {
            return new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Rooms)
                .WhereElementIsNotElementType()
                .Cast<Room>()
                .Where(r => r.Area > 0)
                .ToList();
        }

        // ---------------------------------------------
        // ELEMENT LOCATION POINT (FAST)
        // ---------------------------------------------
        public static XYZ GetElementPoint(this Element el)
        {
            if (el.Location is LocationPoint lp)
                return lp.Point;

            if (el.Location is LocationCurve lc)
                return lc.Curve.Evaluate(0.5, true);

            return null;
        }

        // ---------------------------------------------
        // POINT INSIDE BOUNDING BOX
        // ---------------------------------------------
        public static bool IsPointInsideBBox(XYZ p, BoundingBoxXYZ bb)
        {
            if (p == null || bb == null) return false;

            return p.X >= bb.Min.X && p.X <= bb.Max.X &&
                   p.Y >= bb.Min.Y && p.Y <= bb.Max.Y &&
                   p.Z >= bb.Min.Z && p.Z <= bb.Max.Z;
        }

        // ---------------------------------------------
        // GET ELEMENT SOLID (FALLBACK)
        // ---------------------------------------------
        public static Solid GetElementSolid(this Element el)
        {
            Options opt = new Options
            {
                IncludeNonVisibleObjects = false
            };

            GeometryElement geo = el.get_Geometry(opt);
            if (geo == null) return null;

            foreach (GeometryObject obj in geo)
            {
                if (obj is Solid s && s.Volume > 0)
                    return s;

                if (obj is GeometryInstance gi)
                {
                    foreach (GeometryObject g in gi.GetInstanceGeometry())
                    {
                        if (g is Solid si && si.Volume > 0)
                            return si;
                    }
                }
            }

            return null;
        }

        // ---------------------------------------------
        // HOST ROOM BOUNDING BOX
        // ---------------------------------------------
        public static BoundingBoxXYZ GetHostRoomBBox(Room room)
        {
            return room.get_BoundingBox(null);
        }

        // ---------------------------------------------
        // LINKED ROOM BOUNDING BOX (TRANSFORMED)
        // ---------------------------------------------
        public static BoundingBoxXYZ GetLinkedRoomBBox(Room room, RevitLinkInstance link)
        {
            BoundingBoxXYZ bb = room.get_BoundingBox(null);
            if (bb == null) return null;

            Transform t = link.GetTransform();

            return new BoundingBoxXYZ
            {
                Min = t.OfPoint(bb.Min),
                Max = t.OfPoint(bb.Max)
            };
        }

        // ---------------------------------------------
        // SAFE PARAMETER SET
        // ---------------------------------------------
        public static void SetStringParam(this Element el, string name, string value)
        {
            if (el == null || string.IsNullOrWhiteSpace(value)) return;

            Parameter p = el.LookupParameter(name);
            if (p != null && !p.IsReadOnly)
            {
                p.Set(value);
            }
        }
    }
}

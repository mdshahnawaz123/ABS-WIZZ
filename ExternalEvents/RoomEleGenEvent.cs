using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using System;
using System.Linq;
using ABS_WIZZ;

namespace ABS_WIZZ.ExternalEvents
{
    public class RoomEleGenEvent : IExternalEventHandler
    {
        public RoomCheckMode Mode { get; set; }

        public void Execute(UIApplication app)
        {
            Document doc = app.ActiveUIDocument?.Document;
            if (doc == null) return;

            int updated = 0;

            using (Transaction tx = new Transaction(doc, "Assign ABS from Rooms"))
            {
                tx.Start();

                var hostElements = new FilteredElementCollector(doc)
                    .WhereElementIsNotElementType()
                    .Where(e => e.Category != null && !e.ViewSpecific)
                    .ToList();

                if (Mode == RoomCheckMode.Host)
                {
                    // ===============================
                    // HOST ROOM → HOST ELEMENT
                    // ===============================
                    var rooms = new FilteredElementCollector(doc)
                        .OfClass(typeof(SpatialElement))
                        .WhereElementIsNotElementType()
                        .Cast<Room>()
                        .Where(r => r.Area > 0);

                    foreach (Room room in rooms)
                    {
                        string asset = room.LookupParameter("(01)ECD_ABS_L1_Asset")?.AsString();
                        string level = room.LookupParameter("(04)ECD_ABS_L2_Level")?.AsString();
                        string roomNum = room.LookupParameter("(05)ECD_ABS_L3_Room")?.AsString();

                        BoundingBoxXYZ roomBB = Extension.GetHostRoomBBox(room);
                        if (roomBB == null) continue;

                        foreach (Element el in hostElements)
                        {
                            XYZ p = el.GetElementPoint();
                            bool inside = Extension.IsPointInsideBBox(p, roomBB);

                            if (!inside)
                            {
                                // geometry fallback
                                Solid elSolid = el.GetElementSolid();
                                if (elSolid == null) continue;

                                Outline outline = new Outline(roomBB.Min, roomBB.Max);
                                BoundingBoxIntersectsFilter bbFilter =
                                    new BoundingBoxIntersectsFilter(outline);

                                if (!bbFilter.PassesFilter(el))
                                    continue;
                            }

                            el.SetStringParam("(01)ECD_ABS_L1_Asset", asset);
                            el.SetStringParam("(04)ECD_ABS_L2_Level", level);
                            el.SetStringParam("(05)ECD_ABS_L3_Room", roomNum);
                            updated++;
                        }
                    }
                }
                else if (Mode == RoomCheckMode.Linked)
                {
                    // ===============================
                    // LINKED ROOM → HOST ELEMENT
                    // ===============================
                    var links = new FilteredElementCollector(doc)
                        .OfClass(typeof(RevitLinkInstance))
                        .Cast<RevitLinkInstance>();

                    foreach (var link in links)
                    {
                        Document linkDoc = link.GetLinkDocument();
                        if (linkDoc == null) continue;

                        var rooms = new FilteredElementCollector(linkDoc)
                            .OfCategory(BuiltInCategory.OST_Rooms)
                            .WhereElementIsNotElementType()
                            .Cast<Room>()
                            .Where(r => r.Area > 0);

                        foreach (Room room in rooms)
                        {
                            string asset = room.LookupParameter("(01)ECD_ABS_L1_Asset")?.AsString();
                            string level = room.LookupParameter("(04)ECD_ABS_L2_Level")?.AsString();
                            string roomNum = room.LookupParameter("(05)ECD_ABS_L3_Room")?.AsString();

                            BoundingBoxXYZ roomBB =
                                Extension.GetLinkedRoomBBox(room, link);
                            if (roomBB == null) continue;

                            foreach (Element el in hostElements)
                            {
                                XYZ p = el.GetElementPoint();
                                bool inside = Extension.IsPointInsideBBox(p, roomBB);

                                if (!inside)
                                {
                                    Solid elSolid = el.GetElementSolid();
                                    if (elSolid == null) continue;

                                    Outline outline =
                                        new Outline(roomBB.Min, roomBB.Max);
                                    BoundingBoxIntersectsFilter bbFilter =
                                        new BoundingBoxIntersectsFilter(outline);

                                    if (!bbFilter.PassesFilter(el))
                                        continue;
                                }

                                el.SetStringParam("(01)ECD_ABS_L1_Asset", asset);
                                el.SetStringParam("(04)ECD_ABS_L2_Level", level);
                                el.SetStringParam("(05)ECD_ABS_L3_Room", roomNum);
                                updated++;
                            }
                        }
                    }
                }

                tx.Commit();
            }

            TaskDialog.Show(
                "ABS Room Assignment",
                $"Completed successfully.\n\nElements updated: {updated}"
            );
        }

        public string GetName()
        {
            return "ABS Room Element Assignment";
        }
    }
}

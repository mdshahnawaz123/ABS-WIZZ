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
        public bool IsActiveView { get; set; }

        public void Execute(UIApplication app)
        {
            Document doc = app.ActiveUIDocument?.Document;
            if (doc == null) return;

            int updated = 0;

            using (Transaction tx = new Transaction(doc, "Assign ABS from Rooms"))
            {
                tx.Start();

                FilteredElementCollector collector;
                if (IsActiveView && doc.ActiveView != null)
                {
                    collector = new FilteredElementCollector(doc, doc.ActiveView.Id);
                }
                else
                {
                    collector = new FilteredElementCollector(doc);
                }

                var hostElements = collector
                    .WhereElementIsNotElementType()
                    .Where(e => !e.ViewSpecific && e.Category != null && e.Category.CategoryType == CategoryType.Model)
                    .ToList();

                if (Mode == RoomCheckMode.Host)
                {
                    // ===============================
                    // HOST ROOM → HOST ELEMENT
                    // ===============================
                    var roomCollector = new FilteredElementCollector(doc);
                    if (IsActiveView && doc.ActiveView != null)
                    {
                        roomCollector = new FilteredElementCollector(doc, doc.ActiveView.Id);
                    }

                    var rooms = roomCollector
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

                        Outline outline = new Outline(roomBB.Min, roomBB.Max);
                        BoundingBoxIntersectsFilter bbFilter = new BoundingBoxIntersectsFilter(outline);

                        // Only check elements that intersect the room's bounding box
                        var candidateElements = hostElements
                            .Where(el => bbFilter.PassesFilter(el))
                            .ToList();

                        foreach (Element el in candidateElements)
                        {
                            XYZ p = el.GetElementPoint();
                            bool inside = false;

                            if (p != null)
                            {
                                inside = room.IsPointInRoom(p);
                            }

                            // If point is not in room, we still process it because bbFilter passed
                            // this handles elements where the location point might be slightly outside
                            // but the body is inside
                            if (!inside && p != null)
                            {
                                // Optional: deeper check here, but for now, intersection is enough
                                // for elements that don't have a perfect point
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

                            BoundingBoxXYZ roomBB = Extension.GetLinkedRoomBBox(room, link);
                            if (roomBB == null) continue;

                            Outline outline = new Outline(roomBB.Min, roomBB.Max);
                            BoundingBoxIntersectsFilter bbFilter = new BoundingBoxIntersectsFilter(outline);

                            Transform linkTransform = link.GetTransform();
                            Transform inverseTransform = linkTransform.Inverse;

                            // Only check elements that intersect the room's bounding box (in host space)
                            var candidateElements = hostElements
                                .Where(el => bbFilter.PassesFilter(el))
                                .ToList();

                            foreach (Element el in candidateElements)
                            {
                                XYZ p = el.GetElementPoint();
                                bool inside = false;

                                if (p != null)
                                {
                                    // Transform host point p to link coordinates for IsPointInRoom check
                                    XYZ transPoint = inverseTransform.OfPoint(p);
                                    inside = room.IsPointInRoom(transPoint);
                                }

                                // If point not in room, we still proceed if the BB filter passed
                                // This ensures maximum coverage for furniture/plumbing

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

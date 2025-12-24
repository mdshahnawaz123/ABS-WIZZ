using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ABS_WIZZ.ExternalEvents
{
    public class UniqueNumber : IExternalEventHandler
    {
        public RoomCheckMode Mode { get; set; }

        private const string EQUIP_TYPE_PARAM = "(09)ECD_ABS_L6_Equipment_Type";
        private const string UNIQUE_NUMBER_PARAM = "(16)ECD_ABS_L7_Equipment_Unique_Number";

        public void Execute(UIApplication app)
        {
            try
            {
                Document doc = app.ActiveUIDocument?.Document;
                if (doc == null)
                {
                    TaskDialog.Show("ABS WIZZ", "No active document found.");
                    return;
                }

                int totalUpdated = 0;

                using (Transaction tx = new Transaction(doc, "Assign Equipment Unique Numbers"))
                {
                    tx.Start();

                    if (Mode == RoomCheckMode.Host)
                    {
                        totalUpdated = ProcessHostRooms(doc);
                    }
                    else if (Mode == RoomCheckMode.Linked)
                    {
                        totalUpdated = ProcessLinkedRooms(doc);
                    }

                    tx.Commit();
                }

                TaskDialog.Show(
                    "Equipment Unique Numbers",
                    $"Completed successfully.\n\nElements updated: {totalUpdated}"
                );
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", ex.Message);
            }
        }

        public string GetName()
        {
            return "Assign Equipment Unique Numbers";
        }

        // ============================
        // HOST ROOMS
        // ============================
        private int ProcessHostRooms(Document doc)
        {
            int totalUpdated = 0;

            var rooms = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Rooms)
                .WhereElementIsNotElementType()
                .Cast<Room>()
                .Where(r => r.Area > 0);

            var hostElements = GetHostElements(doc);

            foreach (Room room in rooms)
            {
                BoundingBoxXYZ roomBB = Extension.GetHostRoomBBox(room);
                if (roomBB == null) continue;

                List<Element> roomElements = GetElementsInRoom(hostElements, roomBB);
                totalUpdated += AssignUniqueNumbers(roomElements);
            }

            return totalUpdated;
        }

        // ============================
        // LINKED ROOMS
        // ============================
        private int ProcessLinkedRooms(Document doc)
        {
            int totalUpdated = 0;

            var links = new FilteredElementCollector(doc)
                .OfClass(typeof(RevitLinkInstance))
                .Cast<RevitLinkInstance>();

            var hostElements = GetHostElements(doc);

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
                    BoundingBoxXYZ roomBB = Extension.GetLinkedRoomBBox(room, link);
                    if (roomBB == null) continue;

                    List<Element> roomElements = GetElementsInRoom(hostElements, roomBB);
                    totalUpdated += AssignUniqueNumbers(roomElements);
                }
            }

            return totalUpdated;
        }

        // ============================
        // ELEMENTS INSIDE ROOM
        // ============================
        private List<Element> GetElementsInRoom(List<Element> elements, BoundingBoxXYZ roomBB)
        {
            List<Element> result = new List<Element>();

            Outline outline = new Outline(roomBB.Min, roomBB.Max);
            BoundingBoxIntersectsFilter bbFilter = new BoundingBoxIntersectsFilter(outline);

            foreach (Element el in elements)
            {
                XYZ p = el.GetElementPoint();

                // Check point first
                if (p != null && Extension.IsPointInsideBBox(p, roomBB))
                {
                    result.Add(el);
                    continue;
                }

                // Fallback to bounding box intersection
                if (bbFilter.PassesFilter(el))
                {
                    result.Add(el);
                }
            }

            return result;
        }

        // ============================
        // UNIQUE NUMBER ASSIGNMENT
        // ============================
        private int AssignUniqueNumbers(List<Element> roomElements)
        {
            int updated = 0;

            // Filter elements that have Equipment Type parameter
            var elementsWithEquipType = roomElements
                .Where(e => e.LookupParameter(EQUIP_TYPE_PARAM) != null &&
                           !string.IsNullOrWhiteSpace(e.LookupParameter(EQUIP_TYPE_PARAM).AsString()))
                .ToList();

            // Group by Equipment Type
            var groups = elementsWithEquipType
                .GroupBy(e => e.LookupParameter(EQUIP_TYPE_PARAM).AsString());

            foreach (var group in groups)
            {
                // Only assign unique numbers if there's more than 1 element of this type
                if (group.Count() >= 1)
                {
                    int counter = 1;

                    foreach (Element el in group)
                    {
                        Parameter uniqueNumParam = el.LookupParameter(UNIQUE_NUMBER_PARAM);

                        if (uniqueNumParam != null && !uniqueNumParam.IsReadOnly)
                        {
                            // Format as 001, 002, 003, etc.
                            uniqueNumParam.Set(counter.ToString("D3"));
                            updated++;
                            counter++;
                        }
                    }
                }
            }

            return updated;
        }

        // ============================
        // HOST ELEMENT COLLECTOR
        // ============================
        private List<Element> GetHostElements(Document doc)
        {
            return new FilteredElementCollector(doc)
                .WhereElementIsNotElementType()
                .Where(e => e.Category != null &&
                           !e.ViewSpecific &&
                           e.LookupParameter(EQUIP_TYPE_PARAM) != null)
                .ToList();
        }
    }
}
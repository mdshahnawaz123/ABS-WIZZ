using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ABS_WIZZ
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class AbsCommand : IExternalCommand
    {
        public Result Execute(
            ExternalCommandData commandData,
            ref string message,
            ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            try
            {
                // 1️⃣ Collect ALL HOST elements
                List<Element> hostElements =
                    new FilteredElementCollector(doc)
                        .WhereElementIsNotElementType()
                        .Where(e => e.Category != null)
                        .Where(e => e.Location != null || e.get_BoundingBox(null) != null)
                        .OrderBy(e => e.Id.Value)
                        .ToList();

                // 2️⃣ Collect all Revit link instances
                List<RevitLinkInstance> linkInstances =
                    new FilteredElementCollector(doc)
                        .OfClass(typeof(RevitLinkInstance))
                        .Cast<RevitLinkInstance>()
                        .ToList();

                if (!linkInstances.Any())
                {
                    TaskDialog.Show("ABS WIZZ", "No linked models found.");
                    return Result.Succeeded;
                }

                // 🔹 GLOBAL counters PER FAMILY/TYPE
                Dictionary<string, int> familyGlobalCounter =
                    new Dictionary<string, int>();

                // 🔹 MAP: Family → (RoomUniqueKey → AssignedNumber)
                Dictionary<string, Dictionary<string, int>> familyRoomMap =
                    new Dictionary<string, Dictionary<string, int>>();

                using (Transaction tx = new Transaction(doc, "Room + Family Based Numbering"))
                {
                    tx.Start();

                    foreach (RevitLinkInstance linkInstance in linkInstances)
                    {
                        Document linkedDoc = linkInstance.GetLinkDocument();
                        if (linkedDoc == null) continue;

                        Transform linkTransform = linkInstance.GetTotalTransform();

                        // 3️⃣ Collect rooms from LINKED model
                        List<Room> linkedRooms =
                            new FilteredElementCollector(linkedDoc)
                                .OfCategory(BuiltInCategory.OST_Rooms)
                                .WhereElementIsNotElementType()
                                .Cast<Room>()
                                .Where(r => r.Area > 0)
                                .ToList();

                        foreach (Room room in linkedRooms)
                        {
                            string roomKey =
                                $"{room.Number}_{room.Name}";

                            foreach (Element element in hostElements)
                            {
                                XYZ testPoint = GetElementTestPoint(element);
                                if (testPoint == null) continue;

                                XYZ linkedPoint =
                                    linkTransform.Inverse.OfPoint(testPoint);

                                if (!room.IsPointInRoom(linkedPoint))
                                    continue;

                                // 🔹 Get FAMILY / TYPE key
                                string familyKey = GetFamilyTypeKey(element);

                                // Initialize structures
                                if (!familyGlobalCounter.ContainsKey(familyKey))
                                {
                                    familyGlobalCounter[familyKey] = 0;
                                    familyRoomMap[familyKey] =
                                        new Dictionary<string, int>();
                                }

                                // Assign number ONCE per (Family + Room)
                                if (!familyRoomMap[familyKey].ContainsKey(roomKey))
                                {
                                    familyGlobalCounter[familyKey]++;
                                    familyRoomMap[familyKey][roomKey] =
                                        familyGlobalCounter[familyKey];
                                }

                                string formattedValue =
                                    familyRoomMap[familyKey][roomKey].ToString("D3");

                                // 🔹 Write to shared parameter
                                Parameter param =
                                    element.LookupParameter("(16)ECD_ABS_L7_Equipment_Unique_Number");

                                if (param != null && !param.IsReadOnly)
                                {
                                    param.Set(formattedValue);
                                }

                                // 🔹 Write to shared parameter
                                Parameter param2 =
                                    element.LookupParameter("(17)ECD_ABS_L7_Onsite_Equipment_Tag");

                                var value = $"{room.Number}-{formattedValue}";

                                if (param2 != null && !param.IsReadOnly)
                                {
                                    param.Set(value);
                                }
                            }
                        }
                    }

                    tx.Commit();
                }

                TaskDialog.Show(
                    "ABS WIZZ",
                    "Room + Family based numbering completed successfully.");

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.ToString();
                return Result.Failed;
            }
        }

        // ============================================================
        // 🔹 GET TEST POINT (ALL ELEMENT TYPES)
        // ============================================================
        private XYZ GetElementTestPoint(Element element)
        {
            if (element.Location is LocationPoint lp)
                return lp.Point;

            //if (element.Location is LocationCurve lc)
            //    return lc.Curve.Evaluate(0.5, true);

            BoundingBoxXYZ bb = element.get_BoundingBox(null);
            if (bb != null)
                return (bb.Min + bb.Max) * 0.5;

            return null;
        }

        // ============================================================
        // 🔹 FAMILY / TYPE KEY (SAFE FOR ALL ELEMENTS)
        // ============================================================
        private string GetFamilyTypeKey(Element element)
        {
            if (element is FamilyInstance fi)
            {
                return $"{fi.Symbol.Family.Name}_{fi.Symbol.Name}";
            }

            ElementId typeId = element.GetTypeId();
            if (typeId != ElementId.InvalidElementId)
            {
                ElementType type =
                    element.Document.GetElement(typeId) as ElementType;

                if (type != null)
                {
                    return $"{element.Category.Name}_{type.Name}";
                }
            }

            return element.Category.Name;
        }
    }
}

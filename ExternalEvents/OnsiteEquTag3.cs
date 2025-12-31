//using Autodesk.Revit.DB;
//using Autodesk.Revit.DB.Architecture;
//using Autodesk.Revit.UI;
//using System;
//using System.Collections.Generic;
//using System.Linq;

//namespace ABS_WIZZ.ExternalEvents
//{
//    public class OnsiteEquTag : IExternalEventHandler
//    {
//        public RoomCheckMode Mode { get; set; }

//        public void Execute(UIApplication app)
//        {
//            UIDocument uidoc = app.ActiveUIDocument;
//            Document doc = uidoc?.Document;
//            if (doc == null) return;

//            int updated = 0;

//            // COLLECT ALL PHYSICAL ELEMENTS
//            List<Element> hostElements = new FilteredElementCollector(doc)
//                .WhereElementIsNotElementType()
//                .Where(e =>
//                    e.Category != null &&
//                    !e.ViewSpecific &&
//                    e.Location != null)
//                .ToList();

//            using (Transaction tx = new Transaction(doc, "Assign ABS from Equipment"))
//            {
//                tx.Start();

//                if (Mode == RoomCheckMode.Host)
//                {
//                    var rooms = new FilteredElementCollector(doc)
//                        .OfCategory(BuiltInCategory.OST_Rooms)
//                        .WhereElementIsNotElementType()
//                        .Cast<Room>()
//                        .Where(r => r.Area > 0);

//                    foreach (Room room in rooms)
//                    {
//                        BoundingBoxXYZ roomBB = Extension.GetHostRoomBBox(room);
//                        if (roomBB == null) continue;

//                        foreach (Element el in hostElements)
//                        {
//                            XYZ elPoint = GetElementLocationPoint(el);
//                            if (elPoint == null) continue;

//                            if (!Extension.IsPointInsideBBox(elPoint, roomBB))
//                                continue;

//                            if (TryAssignAbsCode(el))
//                                updated++;
//                        }
//                    }
//                }
//                else if (Mode == RoomCheckMode.Linked)
//                {
//                    var links = new FilteredElementCollector(doc)
//                        .OfClass(typeof(RevitLinkInstance))
//                        .Cast<RevitLinkInstance>();

//                    foreach (RevitLinkInstance link in links)
//                    {
//                        Document linkDoc = link.GetLinkDocument();
//                        if (linkDoc == null) continue;

//                        var rooms = new FilteredElementCollector(linkDoc)
//                            .OfCategory(BuiltInCategory.OST_Rooms)
//                            .WhereElementIsNotElementType()
//                            .Cast<Room>()
//                            .Where(r => r.Area > 0);

//                        foreach (Room room in rooms)
//                        {
//                            BoundingBoxXYZ roomBB =
//                                Extension.GetLinkedRoomBBox(room, link);
//                            if (roomBB == null) continue;

//                            foreach (Element el in hostElements)
//                            {
//                                XYZ elPoint = GetElementLocationPoint(el);
//                                if (elPoint == null) continue;

//                                if (!Extension.IsPointInsideBBox(elPoint, roomBB))
//                                    continue;

//                                if (TryAssignAbsCode(el))
//                                    updated++;
//                            }
//                        }
//                    }
//                }

//                tx.Commit();
//            }

//            TaskDialog.Show(
//                "ABS Room Assignment",
//                $"Completed successfully.\n\nElements updated: {updated}"
//            );
//        }

//        // =========================================================
//        // GET RELIABLE LOCATION POINT FOR ANY ELEMENT TYPE
//        // =========================================================
//        private XYZ GetElementLocationPoint(Element el)
//        {
//            if (el.Location is LocationPoint lp)
//                return lp.Point;

//            if (el.Location is LocationCurve lc)
//                return lc.Curve.Evaluate(0.5, true);

//            BoundingBoxXYZ bb = el.get_BoundingBox(null);
//            if (bb != null)
//                return (bb.Min + bb.Max) * 0.5;

//            return null;
//        }

//        // =========================================================
//        // ASSIGN ABS CODE
//        // =========================================================
//        private bool TryAssignAbsCode(Element el)
//        {
//            string asset =
//                el.LookupParameter("(01)ECD_ABS_L1_Asset")?.AsString();
//            string level =
//                el.LookupParameter("(02)ECD_ABS_L2_Level")?.AsString();
//            string roomVal =
//                el.LookupParameter("(05)ECD_ABS_L3_Room")?.AsString();
//            Parameter eqGroup =
//                el.LookupParameter("(06)ECD_ABS_L4_Equipment_Group");
//            string eqSystem =
//                el.LookupParameter("(07)ECD_ABS_L5_Equipment_System")?.AsString();
//            string eqType =
//                el.LookupParameter("(09)ECD_ABS_L6_Equipment_Type")?.AsString();
//            string eqUnique =
//                el.LookupParameter("(10)ECD_ABS_L7_Equipment_Unique_ID")?.AsString();

//            if (string.IsNullOrWhiteSpace(asset) ||
//                string.IsNullOrWhiteSpace(level) ||
//                string.IsNullOrWhiteSpace(roomVal) ||
//                string.IsNullOrWhiteSpace(eqSystem) ||
//                string.IsNullOrWhiteSpace(eqType) ||
//                string.IsNullOrWhiteSpace(eqUnique) ||
//                eqGroup == null)
//            {
//                return false;
//            }

//            string absCode =
//                $"{asset}-{level}-{roomVal}-{eqGroup.AsString()}-{eqSystem}-{eqType}-{eqUnique}";

//            Parameter onsiteParam =
//                el.LookupParameter("(16)ECD_ABS_L7_Equipment_Unique_Number");

//            if (onsiteParam == null || onsiteParam.IsReadOnly)
//                return false;

//            if (onsiteParam.AsString() == absCode)
//                return false;

//            onsiteParam.Set(absCode);
//            return true;
//        }

//        public string GetName()
//        {
//            return "Onsite Equipment ABS Tag Generator";
//        }
//    }
//}

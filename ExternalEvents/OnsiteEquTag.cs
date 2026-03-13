using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ABS_WIZZ.ExternalEvents
{
    public class OnsiteEquTag : IExternalEventHandler
    {
        public RoomCheckMode Mode { get; set; }
        public bool IsActiveView { get; set; }

        public void Execute(UIApplication app)
        {
            Document doc = app.ActiveUIDocument?.Document;
            if (doc == null) return;

            int updated = 0;

            try
            {
                using (Transaction tx = new Transaction(doc, "Onsite Equipment Tag - All Elements"))
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

                    List<Element> elements = collector
                        .WhereElementIsNotElementType()
                        .Where(e =>
                            !e.ViewSpecific &&
                            IsValidPhysicalCategory(e.Category))
                        .ToList();

                    foreach (Element el in elements)
                    {
                        if (!TryAssignAbs(el))
                            continue;

                        updated++;
                    }

                    tx.Commit();
                }

                TaskDialog.Show(
                    "ABS WIZZ",
                    $"Completed successfully.\nElements updated: {updated}"
                );
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error in OnsiteEquTag", ex.ToString());
            }
        }

        // =====================================================
        // ABS ASSIGNMENT (SAFE)
        // =====================================================
        private bool TryAssignAbs(Element el)
        {
            string asset = GetString(el, "(01)ECD_ABS_L1_Asset");
            string level = GetString(el, "(04)ECD_ABS_L2_Level");
            string room = GetString(el, "(05)ECD_ABS_L3_Room");
            string eqGroup = GetString(el, "(06)ECD_ABS_L4_Equipment_Group");
            string eqSystem = GetString(el, "(07)ECD_ABS_L5_Equipment_System");
            string eqType = GetString(el, "(09)ECD_ABS_L6_Equipment_Type");
            string eqUnique = GetString(el, "(16)ECD_ABS_L7_Equipment_Unique_Number");

            if (new[] { asset, level, room, eqGroup, eqSystem, eqType, eqUnique }
                .Any(string.IsNullOrWhiteSpace))
                return false;

            string abs =
                $"{asset}-{level}-{room}-{eqGroup}-{eqSystem}-{eqType}-{eqUnique}";

            Parameter onsiteParam =
                el.LookupParameter("(17)ECD_ABS_L7_Onsite_Equipment_Tag");

            if (onsiteParam == null || onsiteParam.IsReadOnly)
                return false;

            if (onsiteParam.AsString() == abs)
                return false;

            onsiteParam.Set(abs);
            return true;
        }

        // =====================================================
        // SAFE STRING READ
        // =====================================================
        private string GetString(Element el, string paramName)
        {
            return el.LookupParameter(paramName)?.AsString();
        }

        // =====================================================
        // FILTER OUT ANNOTATIONS / EXCEL / IMPORTS
        // =====================================================
        private bool IsValidPhysicalCategory(Category cat)
        {
            if (cat == null) return false;

            // Include ALL model categories
            if (cat.CategoryType == CategoryType.Model)
                return true;

            // Exclude others (Annotation, Analytical, etc.)
            return false;
        }

        public string GetName()
        {
            return "ABS Assigned (All Elements)";
        }
    }
}

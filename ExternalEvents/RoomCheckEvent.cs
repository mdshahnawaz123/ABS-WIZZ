using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using ABS_WIZZ;

namespace ABS_WIZZ.ExternalEvents
{
    public class RoomCheckEvent : IExternalEventHandler
    {
        public RoomCheckMode Mode { get; set; }
        public bool IsActiveView { get; set; }

        public void Execute(UIApplication app)
        {
            try
            {
                Document doc = app.ActiveUIDocument?.Document;
                if (doc == null)
                {
                    TaskDialog.Show("Room Check", "No active document.");
                    return;
                }

                if (Mode == RoomCheckMode.Host)
                {
                    IList<Room> rooms;
                    if (IsActiveView && doc.ActiveView != null)
                    {
                        rooms = new FilteredElementCollector(doc, doc.ActiveView.Id)
                            .OfCategory(BuiltInCategory.OST_Rooms)
                            .WhereElementIsNotElementType()
                            .Cast<Room>()
                            .Where(r => r.Area > 0)
                            .ToList();
                    }
                    else
                    {
                        rooms = doc.GetRooms();
                    }

                    int missing = rooms.Count(r =>
                        string.IsNullOrWhiteSpace(r.LookupParameter("(01)ECD_ABS_L1_Asset")?.AsString()) ||
                        string.IsNullOrWhiteSpace(r.LookupParameter("(04)ECD_ABS_L2_Level")?.AsString()) ||
                        string.IsNullOrWhiteSpace(r.LookupParameter("(05)ECD_ABS_L3_Room")?.AsString()));

                    TaskDialog.Show(
                        "Room Check - HOST",
                        $"Rooms found: {rooms.Count}\nRooms missing ABS data: {missing}"
                    );
                }
                else if (Mode == RoomCheckMode.Linked)
                {
                    TaskDialog.Show(
                        "Room Check - LINKED",
                        "Linked room check is not implemented yet."
                    );
                }
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", ex.Message);
            }
        }

        public string GetName()
        {
            return "ABS Room Check";
        }
    }
}

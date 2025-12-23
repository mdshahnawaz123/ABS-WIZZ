using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using static ABS_WIZZ.Extension;

namespace ABS_WIZZ.ExternalEvents
{
    public class RoomCheckEvent : IExternalEventHandler
    {
        public RoomCheckMode Mode { get; set; }

        public void Execute(UIApplication app)
        {
            try
            {
                UIDocument uidoc = app.ActiveUIDocument;
                Document doc = uidoc.Document;

                switch (Mode)
                {
                    case RoomCheckMode.getRoom:
                        TaskDialog.Show("Room Check", "Checking HOST model rooms...");
                        // 👉 Host room logic here
                        break;

                    case RoomCheckMode.getLinkedRooms:
                        TaskDialog.Show("Room Check", "Checking LINKED model rooms...");
                        // 👉 Linked room logic here
                        break;
                }
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", ex.Message);
            }
        }

        public string GetName()
        {
            return "Room Check Event";
        }
    }
}


using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using static ABS_WIZZ.Extension;

namespace ABS_WIZZ.ExternalEvents
{
    public class RoomGeneratorEvent : IExternalEventHandler
    {
        public RoomCheckMode Mode { get; set; }
        public void Execute(UIApplication app)
        {
            try
            {
                UIDocument uidoc = app.ActiveUIDocument;
                Document doc = uidoc?.Document;

                if (doc == null)
                {
                    TaskDialog.Show("ABS WIZZ", "No active document found.");
                    return;
                }

                IList<Room> rooms = doc.GetRooms();

                if (rooms == null || rooms.Count == 0)
                {
                    TaskDialog.Show("ABS WIZZ", "No rooms found in the host model.");
                    return;
                }

                using (Transaction tx = new Transaction(doc, "ABS Room Code Generation"))
                {
                    tx.Start();

                    foreach (Room room in rooms)
                    {
                        // -----------------------------
                        // GET REQUIRED PARAMETERS
                        // -----------------------------
                        Parameter pAsset =
                            room.LookupParameter("(01)ECD_ABS_L1_Asset");

                        Parameter pLevel =
                            room.LookupParameter("(04)ECD_ABS_L2_Level");

                        Parameter pRoom =
                            room.LookupParameter("(05)ECD_ABS_L3_Room");

                        Parameter pTarget =
                            room.LookupParameter("(17)ECD_ABS_L7_Onsite_Equipment_Tag");

                        // -----------------------------
                        // VALIDATE PARAMETERS
                        // -----------------------------
                        if (pAsset == null || pLevel == null ||
                            pRoom == null || pTarget == null)
                        {
                            // Skip room safely
                            continue;
                        }

                        if (pTarget.IsReadOnly)
                            continue;

                        string asset = pAsset.AsString() ?? string.Empty;
                        string level = pLevel.AsString() ?? string.Empty;
                        string roomNum = pRoom.AsString() ?? string.Empty;

                        if (string.IsNullOrWhiteSpace(asset) ||
                            string.IsNullOrWhiteSpace(level) ||
                            string.IsNullOrWhiteSpace(roomNum))
                            continue;

                        // -----------------------------
                        // GENERATE ROOM CODE
                        // -----------------------------
                        string roomCode = $"{asset}-{level}-{roomNum}";

                        pTarget.Set(roomCode);
                    }

                    tx.Commit();
                }

                TaskDialog.Show("ABS WIZZ", "Room code generation completed successfully.");
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", ex.Message);
            }
        }

        public string GetName()
        {
            return "ABS Room Code Generator";
        }
    }
}

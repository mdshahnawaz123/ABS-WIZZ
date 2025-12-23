using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;

namespace ABS_WIZZ.ExternalEvents
{
    public class RoomGeneratorEvent : IExternalEventHandler
    {
        public void Execute(UIApplication app)
        {
            try
            {
                UIDocument uidoc = app.ActiveUIDocument;
                Document doc = uidoc.Document;

                using (Transaction tx = new Transaction(doc, "Room Code Generation"))
                {
                    tx.Start();

                    IList<Room> rooms = Extension.getRoom(doc);

                    if (rooms == null || rooms.Count == 0)
                    {
                        TaskDialog.Show("ABS WIZZ", "No rooms found.");
                        tx.RollBack();
                        return;
                    }

                    foreach (Room room in rooms)
                    {
                        // 🔹 Safely get parameters
                        Parameter pAsset =
                            room.LookupParameter("(01)ECD_ABS_L1_Asset");

                        Parameter pLevel =
                            room.LookupParameter("(04)ECD_ABS_L2_Level");

                        Parameter pRoom =
                            room.LookupParameter("(05)ECD_ABS_L3_Room");

                        Parameter pTarget =
                            room.LookupParameter("(17)ECD_ABS_L7_Onsite_Equipment_Tag");

                        // 🔴 Validate ALL parameters
                        if (pAsset == null || pLevel == null || pRoom == null || pTarget == null)
                        {
                            TaskDialog.Show("Eoor", "Check Asset code, Level and Room information");
                            continue;
                        }

                        string asset = pAsset.AsString() ?? "";
                        string level = pLevel.AsString() ?? "";
                        string roomNum = pRoom.AsString() ?? "";

                        string roomCode = $"{asset}-{level}-{roomNum}";

                        if (!pTarget.IsReadOnly)
                        {
                            pTarget.Set(roomCode);
                        }
                    }

                    tx.Commit();
                }
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", ex.ToString());
            }
        }

        public string GetName()
        {
            return "Room Generation done!";
        }
    }
}

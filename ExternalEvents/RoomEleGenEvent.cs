using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABS_WIZZ.ExternalEvents
{
    public class RoomEleGenEvent : IExternalEventHandler
    {
        public bool HostModelCheck { get; set; }
        private bool LinkedModelCheck {  get; set; }

        public void Execute(UIApplication app)
        {
            try
                {
                var uidoc = app.ActiveUIDocument;
                var doc = uidoc.Document;
                using (Transaction tx = new Transaction(doc, "ABS Room Elements Assignment"))
                {
                    tx.Start();

                    //Let's Check the information that we can pull from host and Linked Model Rooms

                    if(doc!=null && HostModelCheck is true)
                    {
                        var hostroom = doc.getRoom();
                        if (hostroom.Count > 0)
                        {
                            foreach (Room room in hostroom)
                            {
                                var name = room.Name;
                                TaskDialog.Show("Room Found", $"Room Name: {name}");
                            }
                        }
                        else
                        {
                            TaskDialog.Show("No Rooms Found", "No rooms found in the host model.");
                        }
                        return;
                    }
                    if (doc != null && LinkedModelCheck is true)
                    {
                        TaskDialog.Show("No Rooms Found", "No rooms found in the linked models.");
                        return;
                    }


                    tx.Commit();
                }
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", ex.Message);
            }
        }

        public string GetName()
        {
            return "ABS are Assigned to Room Elements";
        }
    }
}

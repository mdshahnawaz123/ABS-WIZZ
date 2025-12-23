using Autodesk.Revit.Creation;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ABS_WIZZ
{
    public static class Extension
    {
        public static IList<Room> getRoom(this Autodesk.Revit.DB.Document doc)
        {
            return new FilteredElementCollector(doc)
                .OfClass(typeof(SpatialElement))
                .Cast<Room>()
                .ToList();
        }

        //Let's Create an Extension Method for Linked Rooms

        public static IList<Room> getLinkedRooms(this Autodesk.Revit.DB.Document doc)
        {
            List<Room> linkedRooms = new List<Room>();
            // Collect all linked documents
            var linkedDocs = new FilteredElementCollector(doc)
                .OfClass(typeof(RevitLinkInstance))
                .Cast<RevitLinkInstance>()
                .Select(linkInstance => linkInstance.GetLinkDocument())
                .Where(linkedDoc => linkedDoc != null);
            // Iterate through each linked document to collect rooms
            foreach (var linkedDoc in linkedDocs)
            {
                var roomsInLinkedDoc = new FilteredElementCollector(linkedDoc)
                    .OfClass(typeof(SpatialElement))
                    .Cast<Room>()
                    .ToList();
                linkedRooms.AddRange(roomsInLinkedDoc);
            }
            return linkedRooms;
        }
        public enum RoomCheckMode
        {
            getRoom,
            getLinkedRooms
        }

    }
}

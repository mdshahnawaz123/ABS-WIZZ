using ABS_WIZZ.ExternalEvents;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Windows;
using static ABS_WIZZ.Extension;

namespace ABS_WIZZ.UI
{
    public partial class ECD_ABS : Window
    {
        private ExternalEvent roomGenExternalEvent;
        private RoomGeneratorEvent roomGenHandler;

        private ExternalEvent roomEleGenExternalEvent;
        private RoomEleGenEvent roomEleGenHandler;

        private ExternalEvent roomCheckExternalEvent;
        private RoomCheckEvent roomCheckHandler;

        public ECD_ABS(Document doc, UIDocument uidoc)
        {
            InitializeComponent();

            // 🔹 Create handlers
            roomGenHandler = new RoomGeneratorEvent();
            roomEleGenHandler = new RoomEleGenEvent();

            // 🔹 Create ExternalEvents
            roomGenExternalEvent = ExternalEvent.Create(roomGenHandler);
            roomEleGenExternalEvent = ExternalEvent.Create(roomEleGenHandler);

            // 🔹 Create ExternalEvents
            roomCheckHandler = new RoomCheckEvent();
            roomCheckExternalEvent = ExternalEvent.Create(roomCheckHandler);
        }

        private void RoomCodeGen_Click(object sender, RoutedEventArgs e)
        {
            roomGenExternalEvent.Raise();
        }

        private void RoomEleGen_Click(object sender, RoutedEventArgs e)
        {
            roomEleGenExternalEvent.Raise();
        }
        private void LinkedModelCheck(object sender, RoutedEventArgs e)
        {
            roomCheckHandler.Mode = RoomCheckMode.getLinkedRooms;
            roomCheckExternalEvent.Raise();
            TaskDialog.Show("Host Model Rooms", "Functionality to check Linked model rooms is not yet implemented.");
        }
        private void HostModelCheck(object sender, RoutedEventArgs e)
        {
            roomCheckHandler.Mode = RoomCheckMode.getRoom;
            roomCheckExternalEvent.Raise();
            TaskDialog.Show("Host Model Rooms", "Functionality to check host model rooms is not yet implemented.");
        }
    }
}

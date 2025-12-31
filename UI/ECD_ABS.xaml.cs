using ABS_WIZZ.ExternalEvents;
using Autodesk.Revit.UI;
using System.Windows;
using ABS_WIZZ;
using Autodesk.Revit.DB;

namespace ABS_WIZZ.UI
{
    public partial class ECD_ABS : Window
    {
        private readonly ExternalEvent roomCodeGenEvent;
        private readonly RoomGeneratorEvent roomCodeGenHandler;

        private readonly ExternalEvent roomEleGenEvent;
        private readonly RoomEleGenEvent roomEleGenHandler;

        private readonly ExternalEvent roomCheckEvent;
        private readonly RoomCheckEvent roomCheckHandler;

        private readonly ExternalEvent uniqueNumberEvent;
        private readonly UniqueNumber uniqueNumberHandler;

        private readonly ExternalEvent onsiteEquTagEvent;
        private readonly OnsiteEquTag onsiteEquTagHandler;

        public ECD_ABS(Document doc, UIDocument uidoc)
        {
            InitializeComponent();

            // Initialize Room Code Generator
            roomCodeGenHandler = new RoomGeneratorEvent();
            roomCodeGenEvent = ExternalEvent.Create(roomCodeGenHandler);

            // Initialize Room Element Generator
            roomEleGenHandler = new RoomEleGenEvent();
            roomEleGenEvent = ExternalEvent.Create(roomEleGenHandler);

            // Initialize Room Check
            roomCheckHandler = new RoomCheckEvent();
            roomCheckEvent = ExternalEvent.Create(roomCheckHandler);

            // Initialize Unique Number
            uniqueNumberHandler = new UniqueNumber();
            uniqueNumberEvent = ExternalEvent.Create(uniqueNumberHandler);

            onsiteEquTagHandler = new OnsiteEquTag();
            onsiteEquTagEvent = ExternalEvent.Create(onsiteEquTagHandler);
        }

        // ROOM CODE GENERATOR
        private void RoomCodeGen_Click(object sender, RoutedEventArgs e)
        {
            if (RbHost.IsChecked == true)
                roomCodeGenHandler.Mode = RoomCheckMode.Host;
            else if (RbLinked.IsChecked == true)
                roomCodeGenHandler.Mode = RoomCheckMode.Linked;
            else
            {
                TaskDialog.Show("ABS WIZZ", "Please select Host or Linked model.");
                return;
            }

            roomCodeGenEvent.Raise();
        }

        // ROOM ELEMENT ASSIGNMENT
        private void RoomEleGen_Click(object sender, RoutedEventArgs e)
        {
            if (RbHost.IsChecked == true)
                roomEleGenHandler.Mode = RoomCheckMode.Host;
            else if (RbLinked.IsChecked == true)
                roomEleGenHandler.Mode = RoomCheckMode.Linked;
            else
            {
                TaskDialog.Show("ABS WIZZ", "Please select Host or Linked model.");
                return;
            }

            roomEleGenEvent.Raise();
        }

        // EQUIPMENT UNIQUE NUMBER
        private void UniqueNumber_Click(object sender, RoutedEventArgs e)
        {
            if (RbHost.IsChecked == true)
                uniqueNumberHandler.Mode = RoomCheckMode.Host;
            else if (RbLinked.IsChecked == true)
                uniqueNumberHandler.Mode = RoomCheckMode.Linked;
            else
            {
                TaskDialog.Show("ABS WIZZ", "Please select Host or Linked model.");
                return;
            }

            uniqueNumberEvent.Raise();
        }

        // ROOM CHECK
        private void RoomCheck_Click(object sender, RoutedEventArgs e)
        {
            if (RbHost.IsChecked == true)
                roomCheckHandler.Mode = RoomCheckMode.Host;
            else if (RbLinked.IsChecked == true)
                roomCheckHandler.Mode = RoomCheckMode.Linked;
            else
            {
                TaskDialog.Show("ABS WIZZ", "Please select Host or Linked model.");
                return;
            }

            roomCheckEvent.Raise();
        }

        //Onsite Equipment Tag Generator
        private void OnsiteTagGen_Click(object sender, RoutedEventArgs e)
        {
            if (RbHost.IsChecked == true)
                onsiteEquTagHandler.Mode = RoomCheckMode.Host;
            else if (RbLinked.IsChecked == true)
                onsiteEquTagHandler.Mode = RoomCheckMode.Linked;
            else
            {
                TaskDialog.Show("ABS WIZZ", "Please select Host or Linked model.");
                return;
            }

            onsiteEquTagEvent.Raise();
        }


        //Email Notify_Click
        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }
    }
}
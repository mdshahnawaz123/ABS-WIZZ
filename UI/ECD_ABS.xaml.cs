using ABS_WIZZ.ExternalEvents;
using Autodesk.Revit.UI;
using System.Windows;
using ABS_WIZZ;
using Autodesk.Revit.DB;
using System;
using System.Runtime.InteropServices;
using System.Windows.Interop;

// FIX: Alias Drawing to avoid Revit conflicts
using Drawing = System.Drawing;

namespace ABS_WIZZ.UI
{
    public partial class ECD_ABS : Window
    {
        // Windows API
        [DllImport("user32.dll")]
        static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        const int WM_SETICON = 0x80;

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

            roomCodeGenHandler = new RoomGeneratorEvent();
            roomCodeGenEvent = ExternalEvent.Create(roomCodeGenHandler);

            roomEleGenHandler = new RoomEleGenEvent();
            roomEleGenEvent = ExternalEvent.Create(roomEleGenHandler);

            roomCheckHandler = new RoomCheckEvent();
            roomCheckEvent = ExternalEvent.Create(roomCheckHandler);

            uniqueNumberHandler = new UniqueNumber();
            uniqueNumberEvent = ExternalEvent.Create(uniqueNumberHandler);

            onsiteEquTagHandler = new OnsiteEquTag();
            onsiteEquTagEvent = ExternalEvent.Create(onsiteEquTagHandler);
        }

        // ✅ REMOVE REVIT ICON + ADD BDD ICON
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            var hwnd = new WindowInteropHelper(this).Handle;

            // Remove Revit icon
            SendMessage(hwnd, WM_SETICON, (IntPtr)0, IntPtr.Zero);
            SendMessage(hwnd, WM_SETICON, (IntPtr)1, IntPtr.Zero);

            // Add BDD icon
            var icon = CreateTextIcon("B");

            SendMessage(hwnd, WM_SETICON, (IntPtr)0, icon.Handle);
            SendMessage(hwnd, WM_SETICON, (IntPtr)1, icon.Handle);
        }

        // ✅ SAFE ICON GENERATOR (NO REvit conflicts)
        private Drawing.Icon CreateTextIcon(string text)
        {
            int size = 32;
            var bmp = new Drawing.Bitmap(size, size);
            var g = Drawing.Graphics.FromImage(bmp);

            g.SmoothingMode = Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.Clear(Drawing.Color.FromArgb(30, 58, 138)); // blue background

            // BIG B
            using (var fontB = new Drawing.Font("Segoe UI Black", 22, Drawing.FontStyle.Bold, Drawing.GraphicsUnit.Pixel))
            using (var brushWhite = new Drawing.SolidBrush(Drawing.Color.White))
            {
                var rect = new Drawing.Rectangle(0, 0, size, size);

                var format = new Drawing.StringFormat
                {
                    Alignment = Drawing.StringAlignment.Center,
                    LineAlignment = Drawing.StringAlignment.Center
                };

                g.DrawString("B", fontB, brushWhite, rect, format);
            }

            // SMALL DD inside
            using (var fontDD = new Drawing.Font("Segoe UI", 7, Drawing.FontStyle.Bold, Drawing.GraphicsUnit.Pixel))
            using (var brushBlue = new Drawing.SolidBrush(Drawing.Color.FromArgb(30, 58, 138)))
            {
                var rect = new Drawing.Rectangle(0, size / 2, size, size / 2);

                var format = new Drawing.StringFormat
                {
                    Alignment = Drawing.StringAlignment.Center,
                    LineAlignment = Drawing.StringAlignment.Center
                };

                g.DrawString("DD", fontDD, brushBlue, rect, format);
            }

            var hIcon = bmp.GetHicon();
            return Drawing.Icon.FromHandle(hIcon);
        }

        // ================= BUTTON EVENTS =================

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

            roomCodeGenHandler.IsActiveView = CbActiveView.IsChecked == true;
            roomCodeGenEvent.Raise();
        }

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

            roomEleGenHandler.IsActiveView = CbActiveView.IsChecked == true;
            roomEleGenEvent.Raise();
        }

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

            uniqueNumberHandler.IsActiveView = CbActiveView.IsChecked == true;
            uniqueNumberEvent.Raise();
        }

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

            roomCheckHandler.IsActiveView = CbActiveView.IsChecked == true;
            roomCheckEvent.Raise();
        }

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

            onsiteEquTagHandler.IsActiveView = CbActiveView.IsChecked == true;
            onsiteEquTagEvent.Raise();
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(
                new System.Diagnostics.ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }
    }
}
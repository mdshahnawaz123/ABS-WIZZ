using Autodesk.Revit.UI;
using System;
using System.Reflection;
using ABS_WIZZ.Utils;

namespace ABS_WIZZ.App
{
    public class App : IExternalApplication
    {
        private const string TAB_NAME = "BIM Digital Design";
        private const string PANEL_NAME = "ECD";
        private const string BUTTON_NAME = "ABS Wizz";

        public Result OnStartup(UIControlledApplication application)
        {
            try
            {
                // 1. Create Ribbon Tab (safe)
                try
                {
                    application.CreateRibbonTab(TAB_NAME);
                }
                catch
                {
                    // Tab already exists
                }

                // 2. Get or Create Panel
                RibbonPanel panel = null;

                foreach (RibbonPanel p in application.GetRibbonPanels(TAB_NAME))
                {
                    if (p.Name == PANEL_NAME)
                    {
                        panel = p;
                        break;
                    }
                }

                if (panel == null)
                {
                    panel = application.CreateRibbonPanel(TAB_NAME, PANEL_NAME);
                }

                // 3. Prevent duplicate button
                foreach (var item in panel.GetItems())
                {
                    if (item.Name == BUTTON_NAME)
                        return Result.Succeeded;
                }

                // 4. Create Button
                PushButtonData buttonData = new PushButtonData(
                    "ABS_WIZZ_BTN",
                    BUTTON_NAME,
                    Assembly.GetExecutingAssembly().Location,
                    "ABS_WIZZ.Command.ABSCommand"
                );

                PushButton button = panel.AddItem(buttonData) as PushButton;

                // 5. Button Images (Embedded Resources)
                button.LargeImage = ImageUtils.GetEmbeddedImage(
                    "ABS_WIZZ.Resources.abs.png");

                button.Image = ImageUtils.GetEmbeddedImage(
                    "ABS_WIZZ.Resources.abs16.png");

                // 6. Tooltip
                button.ToolTip = "Asset Breakdown Information";
                button.LongDescription = "Generates and validates Asset Breakdown Structure (ABS) data for ECD projects, ensuring consistent asset classification, parameter completeness, and readiness for handover.";

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("ABS Wizz", ex.Message);
                return Result.Failed;
            }
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }
    }
}

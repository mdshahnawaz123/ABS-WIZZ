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
                // Create Tab (safe)
                try
                {
                    application.CreateRibbonTab(TAB_NAME);
                }
                catch { }

                // Get or Create Panel
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
                    panel = application.CreateRibbonPanel(TAB_NAME, PANEL_NAME);

                // Prevent duplicate button
                foreach (var item in panel.GetItems())
                {
                    if (item.Name == BUTTON_NAME)
                        return Result.Succeeded;
                }

                // Button Data
                PushButtonData buttonData = new PushButtonData(
                    "ABS_WIZZ_BTN",
                    BUTTON_NAME,
                    Assembly.GetExecutingAssembly().Location,
                    "ABS-WIZZ.ABSCommand"
                );

                PushButton button = panel.AddItem(buttonData) as PushButton;

                // Images (Embedded Resources)
                button.LargeImage = ImageUtils.GetEmbeddedImage(
                    "ABS_WIZZ.Resources.abs.png");

                button.Image = ImageUtils.GetEmbeddedImage(
                    "ABS_WIZZ.Resources.abs16.png");

                // Optional extras
                button.ToolTip = "Launch ABS Wizz tool";
                button.LongDescription = "Advanced building system wizard for ECD tools.";

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

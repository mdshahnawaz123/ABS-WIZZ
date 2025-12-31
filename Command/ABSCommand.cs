using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Asset.Services;
using ABS_WIZZ.UI;
using System;
using System.Threading.Tasks;
using System.Windows.Interop;

namespace ABS_WIZZ.Command
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class ABSCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uidoc = commandData.Application.ActiveUIDocument;
            var doc = uidoc.Document;

            try
            {
                // 🔐 STEP 1 — License Check (Task.Run fixes deadlock on 2nd+ run)
                var result = Task.Run(async () => await LicenseManager.TryAutoLoginAsync()).Result;

                if (!result.Success)
                {
                    var login = new LoginWindow();

                    // Attach WPF window to Revit
                    var helper = new WindowInteropHelper(login);
                    helper.Owner = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;

                    var dlg = login.ShowDialog();
                    if (dlg != true)
                        return Result.Cancelled;
                }

                // ✅ LICENSE OK — RUN YOUR TOOL
                var frm = new UI.ECD_ABS(doc, uidoc);
                frm.Show();

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("ABS Wizz Error", ex.Message);
                return Result.Failed;
            }
        }
    }
}
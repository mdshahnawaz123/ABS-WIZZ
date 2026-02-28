using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Asset.Services;
using ABS_WIZZ.UI;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace ABS_WIZZ.Command
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class ABSCommand : IExternalCommand
    {
        // ── Singleton window instance ─────────────────────────────────────────
        private static ECD_ABS _window;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                var uidoc = commandData.Application.ActiveUIDocument;
                var doc = uidoc.Document;

                // ── 1. Prevent duplicate windows ──────────────────────────────
                if (_window != null)
                {
                    _window.Activate();
                    _window.Focus();
                    return Result.Succeeded;
                }

                // ── 2. License Check ──────────────────────────────────────────
                var licenseResult = Task.Run(async () =>
                    await LicenseManager.TryAutoLoginAsync()
                ).GetAwaiter().GetResult();

                if (!licenseResult.Success)
                {
                    var login = new LoginWindow();
                    var helper = new WindowInteropHelper(login);
                    helper.Owner = System.Diagnostics.Process
                        .GetCurrentProcess()
                        .MainWindowHandle;

                    bool? dlg = login.ShowDialog();
                    if (dlg != true)
                        return Result.Cancelled;
                }

                // ── 3. Launch Tool ────────────────────────────────────────────
                _window = new ECD_ABS(doc, uidoc);

                // Clear static ref when window is closed
                _window.Closed += (s, e) => _window = null;

                _window.Show();

                return Result.Succeeded;
            }
            catch (OperationCanceledException)
            {
                return Result.Cancelled;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                TaskDialog.Show("ABS Wizz — Error", $"{ex.Message}\n\n{ex.StackTrace}");
                return Result.Failed;
            }
        }
    }
}
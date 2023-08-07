using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using WebApplication13.Models;

namespace WebApplication13.Controllers
{
    public class HomeController : Controller
    {
        private static readonly List<Location> locations = new();
        public ActionResult Index()
        {
            LoadFromExcel();
            return View(locations);
        }

        [HttpPost]
        public ActionResult AddLocation(Location newLocation)
        {
            newLocation.LOCATION_ID = locations.Count + 1;
            locations.Add(newLocation);
            SaveToExcel(locations);
            return RedirectToAction("Index");
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(Location newLocation)
        {
            newLocation.LocationGuid = Guid.NewGuid().ToString();
            locations.Add(newLocation);
            SaveToExcel(locations);
            return RedirectToAction("Index");
        }

        public ActionResult Edit(string locationGuid)
        {
            return HandleLocationAction(locationGuid, (location) => View(location));
        }

        [HttpPost]
        public ActionResult Edit(Location editedLocation, string locationGuid)
        {
            return HandleLocationAction(locationGuid, (location) =>
            {
                location.LOCATION_NAME = editedLocation.LOCATION_NAME;
                location.LOCATION_ID = editedLocation.LOCATION_ID;
                location.IS_CLEARANCE = editedLocation.IS_CLEARANCE;

                SaveToExcel(locations);
                return RedirectToAction("Index");
            });
        }

        public ActionResult Delete(string locationGuid)
        {
            return HandleLocationAction(locationGuid, (location) =>
            {
                locations.Remove(location);
                SaveToExcel(locations);
                return RedirectToAction("Index");
            });
        }

        private ActionResult HandleLocationAction(string locationGuid, Func<Location, ActionResult> action)
        {
            Location ? location = locations.FirstOrDefault(l => l.LocationGuid == locationGuid);
            if (location == null)
            {
                return View("NotFound");
            }
            return action(location);
        }

        private static void PerformExcelOperation(List<Location> dataToSave, Action<ExcelWorksheet, List<Location>> excelOperation)
        {
            string filePath = @"C:\Users\harpu\Downloads\FLOOR_LOCATION1.xlsx";
            using var package = new ExcelPackage(new FileInfo(filePath));
            ExcelWorksheet worksheet = package.Workbook.Worksheets[0];

            excelOperation(worksheet, dataToSave);
            package.Save();
        }

        private static void LoadFromExcel()
        {
            if (locations.Count > 0)
            {
                return;
            }

            PerformExcelOperation(locations, (worksheet, locationsToLoad) =>
            {
                int rowCount = worksheet.Dimension.Rows;
                for (int row = 2; row <= rowCount; row++)
                {
                    Location location = new()
                    {
                        LOCATION_NAME = worksheet.Cells[row, 1].Value?.ToString(),
                        LOCATION_ID = int.TryParse(worksheet.Cells[row, 2].Value?.ToString(), out int locationId) ? locationId : 0,
                        IS_CLEARANCE = string.Equals(worksheet.Cells[row, 3].Value?.ToString(), "Y", StringComparison.OrdinalIgnoreCase),
                        LocationGuid = Guid.NewGuid().ToString()
                    };
                    locationsToLoad.Add(location);
                }
            });
        }

        private static void SaveToExcel(List<Location> locationsToSave)
        {
            PerformExcelOperation(locationsToSave, (worksheet, data) =>
            {
                worksheet.Cells.Clear();
                worksheet.Cells[1, 1].Value = "LOCATION_NAME";
                worksheet.Cells[1, 2].Value = "LOCATION_ID";
                worksheet.Cells[1, 3].Value = "IS_CLEARANCE";

                for (int i = 0; i < data.Count; i++)
                {
                    worksheet.Cells[i + 2, 1].Value = data[i].LOCATION_NAME;
                    worksheet.Cells[i + 2, 2].Value = data[i].LOCATION_ID;
                    worksheet.Cells[i + 2, 3].Value = data[i].IS_CLEARANCE ? "Y" : "N";
                }
            });
        }
    }
}

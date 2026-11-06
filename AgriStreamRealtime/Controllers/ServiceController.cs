using AgriStreamRealtime.Data;
using AgriStreamRealtime.Models;
using AgriStreamRealtime.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace AgriStreamRealtime.Controllers
{
    [Authorize]
    public class ServiceController : Controller
    {

        private readonly ApplicationDbContext _context;
        private readonly BlobStorageService _blobStorageService;
        private readonly TableStorageService _tableStorageService;

        // Constructor to initialize the ServiceController with the ApplicationDbContext and BlobStorageService.
        public ServiceController(
    ApplicationDbContext context,
    BlobStorageService blobStorageService,
    TableStorageService tableStorageService)
        {
            _context = context;
            _blobStorageService = blobStorageService;
            _tableStorageService = tableStorageService;
        }

        // GET: Services
        public async Task<IActionResult> Index()
        {
            var services = await _context.Services
                .OrderBy(g => g.ServiceTitle)
                .ToListAsync();

            foreach (var service in services)
            {
                if (!string.IsNullOrWhiteSpace(service.CoverImageUrl))
                {
                    service.CoverImageUrl =
                        _blobStorageService.GetReadOnlyImageUrl(
                            service.CoverImageUrl);
                }
            }

            return View(services);
        }

        [HttpGet]
        public IActionResult Create()
        {
            // Return the Create view to display the form for adding a new service.
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            Service services, IFormFile? coverImage)
        {
            // Validate the model state and ensure that the price and stock quantity are not negative.
            if (!ModelState.IsValid)
            {
                return View(services);
            }


            // Handle the cover image upload if a file is provided. If the cover image is not null and has a length greater than 0, attempt to upload it to Azure Blob Storage using the BlobStorageService. If the upload is successful, set the CoverImageUrl property of the game to the returned URL. Set the DateAdded property of the game to the current UTC date and time. Add the game to the database context and save changes asynchronously. If any exceptions occur during this process, handle them appropriately by adding model errors and returning the view with the game model.
            try
            {
                if (coverImage != null && coverImage.Length > 0)
                {
                    services.CoverImageUrl =
                        await _blobStorageService.UploadImageAsync(
                            coverImage);
                }

                services.DateAdded = DateTime.UtcNow;

                _context.Services.Add(services);

                await _context.SaveChangesAsync();

                // Record the activity in Azure Table Storage.
                try
                {
                    var log = new MonitoringLog
                    {
                        PartitionKey = "Monitor",
                        RowKey = Guid.NewGuid().ToString(),
                        ServiceId = services.ServiceId,
                        ServiceTitle = services.ServiceTitle,
                        Action = "Service Added",
                        Details = $"Added {services.ServiceTitle} to monitoring.",
                        DateRecorded = DateTimeOffset.UtcNow
                    };

                    await _tableStorageService.LogActivityAsync(log);
                }
                catch (Exception)
                {
                    TempData["WarningMessage"] =
                        "Service saved to SQL, but the audit log could not be recorded.";
                }

                TempData["SuccessMessage"] =
                    "Service successfully saved with its cover image.";

                return RedirectToAction(nameof(Index));
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError("coverImage", ex.Message);
                return View(services);
            }
            catch (Exception)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "An error occurred while saving the service. Please try again.");

                return View(services);
            }
        }
    }
}
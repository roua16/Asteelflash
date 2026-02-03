using ITStockM.Services;
using ITStockM.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Radzen;
using Radzen.Blazor;
using Microsoft.EntityFrameworkCore;


namespace ITStockM.Components.Pages.Infra
{
    public partial class InfraInterface
    {
        [Inject]
        protected IJSRuntime JSRuntime { get; set; }

        [Inject]
        protected DialogService DialogService { get; set; }

        [Inject]
        protected NotificationService NotificationService { get; set; }

        [Inject]
        protected ProtectedLocalStorage LocalStorage { get; set; }

        [Inject]
        public ITStockManagmentService ITStockManagmentService { get; set; }

        [Inject]
        public INotificationService AdminNotificationService { get; set; }

        protected bool errorVisible;
        protected List<string> requestOptions = new List<string>() { "Update project", "Existing Project" };
        protected List<string> materialTypes = new List<string>() { "HardWare", "SoftWare", "Mosue", "KeyBoard", "Laptop", "Mini Pc", "Backpack", "Headphone", "Monitor" };
        protected List<string> status = new List<string>() { "Normal", "Urgent", "Critical" };

        string fileName;
        long? fileSize;
        string filevalue;

        protected List<string> projectNames;




        protected string search = "";



        //Requets list
        protected IEnumerable<Models.ITStockManagment.Request> requests;

        protected RadzenDataGrid<Models.ITStockManagment.Request> grid0;


        //Offers
        protected RadzenDataGrid<Models.ViewModels.InfraViewModel> grid1;

        protected Models.ITStockManagment.Request request;
        protected IEnumerable<Models.ITStockManagment.Offer> offers;

        protected IEnumerable<Models.ViewModels.InfraViewModel> groupedOffers;

        RadzenCarousel carousel;
        RadzenCarousel carousel1;

        bool auto = true;
        bool auto1 = true;

        double interval = 4000;
        double interval1 = 4000;





        int selectedIndex;
        int selectedIndex1;
        protected async Task EditRow(DataGridRowMouseEventArgs<Models.ITStockManagment.Request> args)
        {
            var options = new DialogOptions
            {
                Style = "min-width: 600px;",
                CssClass = "dialog-animation",
                Width = "900px",
                CloseDialogOnOverlayClick = true,
                Resizable = true,
                Draggable = true,
                CloseDialogOnEsc = true
            };

            await DialogService.OpenAsync<EditRequest>("", new Dictionary<string, object> { { "Id", args.Data.Id } }, options);
        }


        protected async Task EditCard(int Id)
        {
            var options = new DialogOptions
            {
                Style = "min-width: 600px;",
                CssClass = "dialog-animation",
                Width = "900px",
                CloseDialogOnOverlayClick = true,
                Resizable = true,
                Draggable = true,
                CloseDialogOnEsc = true

            };
            await DialogService.OpenAsync<EditRequest>("", new Dictionary<string, object> { { "Id", Id } }, options);
        }

        protected async Task GridDeleteButtonClick(MouseEventArgs args, Models.ITStockManagment.Request request)
        {
            try
            {
                if (await DialogService.Confirm("Are you sure you want to delete this record?") == true)
                {
                    var deleteResult = await ITStockManagmentService.DeleteRequest(request.Id);

                    if (deleteResult != null)
                    {
                        await grid0.Reload();
                    }
                }
            }
            catch (Exception ex)
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Error,
                    Summary = $"Error",
                    Detail = $"Unable to delete Request"
                });
            }
        }

        protected override async Task OnInitializedAsync()
        {
            // Initialize all collections to prevent null reference exceptions during rendering
            requests = new List<Models.ITStockManagment.Request>();
            offers = new List<Models.ITStockManagment.Offer>();
            groupedOffers = new List<Models.ViewModels.InfraViewModel>();
            projectNames = new List<string>();

            // Load requests immediately (materialized) to avoid lifetime/deferred-execution issues
            List<Models.ITStockManagment.Request> allRequestsList;
            try
            {
                allRequestsList = await ITStockManagmentService.GetRequestsList(new Query { Expand = "Employee" });
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Warning: failed to load requests, continuing with empty list. Error: {ex.Message}");
                allRequestsList = new List<Models.ITStockManagment.Request>();
            }

            // Get all offers and materialize safely
            List<Models.ITStockManagment.Offer> allOffersList = null;

            try
            {
                allOffersList = await ITStockManagmentService.GetOffersList(new Query { Expand = "Request,Supplier" });
                offers = allOffersList.Where(o => o.Request?.Status != "Done").ToList();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Warning: failed to load offers with includes, falling back. Error: {ex.Message}");

                // Fallback: get offers without includes and join in-memory with requests we already have
                allOffersList = await ITStockManagmentService.GetOffersList();
                offers = allOffersList.Where(o => allRequestsList.FirstOrDefault(r => r.Id == o.RequestId)?.Status != "Done").ToList();
            }


            var requestIdsWithOffers = offers.Select(o => o.RequestId).Distinct().ToList();


            requests = allRequestsList.Where(r => !requestIdsWithOffers.Contains(r.Id) && r.Status != "Done").OrderByDescending(r => r.Date).ToList();

            groupedOffers = offers
                .GroupBy(o => new { o.RequestId, o.Request.Title })
                .Select(group => new
                {
                    RequestId = group.Key.RequestId,
                    RequestTitle = group.Key.Title,
                    OffreId = group.Select(o => o.Id).ToList(),
                    OffersAvai = group.Select(o => o.Id).ToList().Count,
                    offers = group

                }).Where(group => !group.offers.Any(o => o.Selected == true))
                .Select(group => new Models.ViewModels.InfraViewModel
                {
                    RequestId = group.RequestId,
                    RequestTitle = group.RequestTitle,
                    OffreId = group.OffreId,
                    OffersAvai = group.OffreId.Count
                }).ToList();



            request = new Models.ITStockManagment.Request();



            projectNames = (await ITStockManagmentService.GetProjectsList()).Select(p => p.ProjectName).ToList();
        }


        protected async Task FormSubmit()
        {
            try
            {
                await InvokeAsync(async () =>
                {
                    var session = (await LocalStorage.GetAsync<UserSession>("UserSession")).Value;
                    request.EmployeeId = session.Id;
                    request.Date = DateTime.Now;
                    request = await ITStockManagmentService.CreateRequest(request);

                    try
                    {
                        await AdminNotificationService.NotifyRequestSubmittedAsync(request.Id, session.FullName);
                    }
                    catch (Exception ex)
                    {
                        // Never block UI flow on email failures
                        Console.Error.WriteLine($"Admin notification failed for request #{request.Id}: {ex.Message}");
                    }
                    string htmlEmail = $@"<!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <style>
        body {{
            font-family: 'Roboto', Arial, sans-serif;
            line-height: 1.6;
            color: #333333;
            max-width: 600px;
            margin: 0 auto;
            padding: 20px;
            background-color: #f9f9f9;
        }}
        .container {{
            background-color: #ffffff;
            border-radius: 8px;
            box-shadow: 0 2px 10px rgba(0,0,0,0.1);
            overflow: hidden;
        }}
        .header {{
            background: linear-gradient(135deg, #0055a4, #003b75);
            color: white;
            padding: 20px;
            text-align: center;
            position: relative;
        }}
        .header::before {{
            content: '';
            position: absolute;
            top: 0;
            left: 0;
            right: 0;
            bottom: 0;
            background: url('data:image/svg+xml,<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 100 100""><defs><pattern id=""grid"" width=""10"" height=""10"" patternUnits=""userSpaceOnUse""><path d=""M 10 0 L 0 0 0 10"" fill=""none"" stroke=""rgba(255,255,255,0.1)"" stroke-width=""0.5""/></pattern></defs><rect width=""100"" height=""100"" fill=""url(%23grid)""/></svg>');
            opacity: 0.3;
        }}
        .header h1 {{
            font-size: 24px;
            margin: 0;
            font-weight: 700;
            position: relative;
            z-index: 1;
        }}
        .header .subtitle {{
            font-size: 14px;
            opacity: 0.9;
            margin-top: 5px;
            position: relative;
            z-index: 1;
        }}
        .notification-badge {{
            background: linear-gradient(135deg, #dc3545, #c82333);
            color: white;
            padding: 8px 16px;
            border-radius: 20px;
            font-size: 12px;
            font-weight: 600;
            text-transform: uppercase;
            letter-spacing: 0.5px;
            display: inline-block;
            margin-bottom: 20px;
        }}
        .content {{
            padding: 25px;
        }}
        .greeting {{
            font-size: 18px;
            margin-bottom: 20px;
            color: #2c3e50;
        }}
        .request-details {{
            background-color: #f8f9fa;
            padding: 20px;
            border-left: 4px solid #0055a4;
            border-radius: 4px;
            margin: 15px 0;
        }}
        .detail-row {{
            display: flex;
            justify-content: space-between;
            align-items: center;
            padding: 8px 0;
            border-bottom: 1px dashed #e9ecef;
        }}
        .detail-row:last-child {{
            border-bottom: none;
        }}
        .detail-label {{
            font-weight: 600;
            color: #495057;
            font-size: 14px;
        }}
        .detail-value {{
            color: #0055a4;
            font-weight: 500;
            font-size: 14px;
        }}
        .request-id {{
            background: linear-gradient(135deg, #0055a4, #003b75);
            color: white;
            padding: 4px 12px;
            border-radius: 15px;
            font-size: 12px;
            font-weight: 600;
        }}
        .priority-badge {{
            padding: 4px 10px;
            border-radius: 12px;
            font-size: 11px;
            font-weight: 600;
            text-transform: uppercase;
        }}
        .priority-high {{
            background: #fee2e2;
            color: #dc2626;
        }}
        .priority-medium {{
            background: #fef3c7;
            color: #d97706;
        }}
        .priority-low {{
            background: #d1fae5;
            color: #059669;
        }}
        .alert {{
            color: #0055a4;
            font-weight: bold;
            background-color: #e3f2fd;
            padding: 15px;
            border-radius: 4px;
            margin: 15px 0;
            border-left: 4px solid #2196f3;
        }}
        .cta-button {{
            background: linear-gradient(135deg, #0055a4, #003b75);
            color: white;
            text-decoration: none;
            padding: 12px 25px;
            border-radius: 6px;
            font-weight: 600;
            font-size: 14px;
            display: inline-block;
            margin: 20px 0;
            text-align: center;
        }}
        .footer {{
            background-color: #2c3e50;
            color: #ecf0f1;
            padding: 20px;
            text-align: center;
        }}
        .footer {{
            background-color: #f8f9fa;
            padding: 20px;
            text-align: center;
            border-top: 1px solid #e9ecef;
        }}
        .signature {{
            font-weight: bold;
            margin-top: 20px;
            color: #0055a4;
        }}
        @media (max-width: 600px) {{
            .detail-row {{
                flex-direction: column;
                align-items: flex-start;
                gap: 5px;
            }}
        }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>🔔 New Request Alert</h1>
            <div class=""subtitle"">IT Inventory Management</div>
        </div>
        
        <div class=""content"">
            <div class=""notification-badge"">
                ⚡ Action Required
            </div>
            
            <p class=""greeting"">Hello Sabrine,</p>
            
            <p class=""alert"">A new request has been submitted and requires your attention in the IT Inventory Management.</p>
            
            <div class=""request-details"">
                <div class=""detail-row"">
                    <span class=""detail-label"">Request ID:</span>
                    <span class=""detail-value"">
                        <span class=""request-id"">#{request.Id}</span>
                    </span>
                </div>
                 <div class=""detail-row"">
                    <span class=""detail-label"">Title:</span>
                    <span class=""detail-value"">{request.Title}</span>
                </div>
                <div class=""detail-row"">
                    <span class=""detail-label"">Requested By:</span>
                    <span class=""detail-value"">{request.Employee.FullName}</span>
                </div>
                
                <div class=""detail-row"">
                    <span class=""detail-label"">Material Type:</span>
                    <span class=""detail-value"">{request.MaterialType}</span>
                </div>
               
                <div class=""detail-row"">
                    <span class=""detail-label"">Status:</span>
                    <span class=""detail-value"">
                        <span class=""priority-badge priority-{(request.Status?.ToString().ToLower() ?? "medium")}"">
                            {request.Status?.ToString() ?? "Medium"}
                        </span>
                    </span>
                </div>
                <div class=""detail-row"">
                    <span class=""detail-label"">Submitted:</span>
                    <span class=""detail-value"">{DateTime.Now:MMM dd, yyyy 'at' HH:mm}</span>
                </div>
            </div>
                        
        
            
           
        </div>
        
         <div class=""footer"">
            <p>Thank you for your attention.</p>
            <p class=""signature"">Best regards,<br>The AsteelFlash Team</p>
        </div>
    </div>
</body>
</html>";

                    // EmailService.SendEmail("mortadhajouinizlatan@gmail.com", $"New Request (#{request.Id}) - {request.Employee.FullName}", htmlEmail);

                    request = new Models.ITStockManagment.Request();
                    await grid0.Reload();

                    DialogService.Close();


                    await JSRuntime.InvokeVoidAsync("scrollToElement", "scrollback");

                    NotificationService.Notify(new NotificationMessage
                    {
                        Severity = NotificationSeverity.Success,
                        Summary = "Success",
                        Detail = "Request submited successfully",
                        Duration = 4000
                    });






                });
                await DialogService.OpenAsync<LoadingScreen>("", null
               , new DialogOptions() { ShowTitle = false, Style = "width:100%;height:100%;", CloseDialogOnEsc = false });




            }
            catch (Exception ex)
            {
                errorVisible = true;
            }
        }







        protected async Task EditRowOffer(DataGridRowMouseEventArgs<Models.ViewModels.InfraViewModel> args)
        {
            var requestIdsWithOffers = offers.Select(o => o.RequestId).Distinct().ToList();
            var offersIds = offers
            .Where(o => o.RequestId == args.Data.RequestId)
            .Select(o => o.Id)
            .ToList();
            var options = new DialogOptions
            {
                Style = "min-width: 600px;",
                CssClass = "dialog-animation",
                CloseDialogOnOverlayClick = true,
                Resizable = true,
                Draggable = true,
                CloseDialogOnEsc = true

            };
            await DialogService.OpenAsync<AvailableOffers>("", new Dictionary<string, object> { { "Id", offersIds } }, options);
            await grid1.Reload();
        }
        protected async Task EditCardOffer(int Id)
        {
            var requestIdsWithOffers = offers.Select(o => o.RequestId).Distinct().ToList();
            var offersIds = offers
            .Where(o => o.RequestId == Id)
            .Select(o => o.Id)
            .ToList();
            var options = new DialogOptions
            {
                Style = "min-width: 600px;",
                CssClass = "dialog-animation",
                CloseDialogOnOverlayClick = true,
                Resizable = true,
                Draggable = true,
                CloseDialogOnEsc = true
            };
            await DialogService.OpenAsync<AvailableOffers>("", new Dictionary<string, object> { { "Id", offersIds } }, options);
            await grid1.Reload();
        }

        private async Task OnFileUpload(UploadChangeEventArgs args)
        {
            var file = args.Files.FirstOrDefault();
            if (file != null)
            {
                using (var stream = file.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024))
                {
                    var buffer = new byte[stream.Length];
                    await stream.ReadAsync(buffer, 0, buffer.Length);
                    request.File = buffer;

                    request.FileExtension = Path.GetExtension(file.Name);

                    request.FileName = file.Name;

                }
            }
        }







        BadgeStyle GetStatusStyles(string status)
        {
            return status?.ToLower() switch
            {
                "urgent" => BadgeStyle.Warning,
                "normal" => BadgeStyle.Success,
                "critical" => BadgeStyle.Danger,
                "" => BadgeStyle.Info,
                _ => BadgeStyle.Light
            };
        }
        private BadgeStyle GetStatusStyle(string status)
        {
            return status?.ToLower() switch
            {
                "urgent" => BadgeStyle.Warning,
                "normal" => BadgeStyle.Success,
                "critical" => BadgeStyle.Danger,
                _ => BadgeStyle.Secondary
            };
        }




    }

}
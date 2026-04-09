using DocumentFormat.OpenXml.Spreadsheet;
using ITStockM.Services;
using ITStockM.Services.Offers;
using ITStockM.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Radzen;
using Radzen.Blazor;

namespace ITStockM.Components.Pages.Infra
{
    public partial class AvailableOffers
    {
        [Inject]
        protected DialogService DialogService { get; set; } = default!;

        [Inject]
        public IOfferService OfferService { get; set; } = default!;

        [Inject]
        protected ProtectedLocalStorage LocalStorage { get; set; } = default!;

        protected RadzenDataGrid<Models.ITStockManagment.Offer> grid = default!;

        protected IEnumerable<Models.ITStockManagment.Offer> offers = new List<Models.ITStockManagment.Offer>();

        [Parameter]
        public List<int> Id { get; set; } = new();
        protected bool errorVisible;
        RadzenCarousel carousel;
        bool auto = true;
        double interval = 4000;
        int selectedIndex;

        protected override async Task OnInitializedAsync()
        {
            offers = await OfferService.GetOffersByIds(Id);
        }

        [Inject]
        public IEmailService EmailService { get; set; } = default!;

        public async Task ChooseOffer(Models.ITStockManagment.Offer offer)
        {


            bool? confirm = await DialogService.Confirm("Are you sure you want to choose this offer ?", "Offer Selection Confirmation");
            if (confirm != true)
            {
                return;
            }
            DialogService.Close(null);
            await InvokeAsync(async () =>
            {
                offer.Selected = true;
                await OfferService.UpdateOffer(offer.Id, offer);

                foreach (ITStockM.Models.ITStockManagment.Offer item in offers)
                {
                    if (item.Id != offer.Id)
                    {
                        item.Selected = false;
                        await OfferService.UpdateOffer(item.Id, item);
                    }
                }
                var user = (await LocalStorage.GetAsync<UserSession>("UserSession")).Value.FullName;

                await InvokeAsync(async () =>
                {
                    try
                    {
                        var details = $"Request: {offer.Request?.Title ?? "(unknown)"} (#{offer.RequestId})<br/>" +
                                      $"Supplier: {offer.Supplier?.SupplierName ?? "(unknown)"}<br/>" +
                                      $"Price: {offer.Price} TND<br/>" +
                                      $"DeliveryDate: {offer.DeliveryDate}";

                        await EmailService.SendAdminNotificationAsync("Offer selected", details, user);
                    }
                    catch (Exception ex)
                    {
                        // Ignore email failures to avoid breaking the selection flow
                        Console.Error.WriteLine($"Admin notification failed for offer #{offer.Id}: {ex.Message}");
                    }
                });

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
            background: linear-gradient(135deg, #28a745, #20c997);
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
        .confirmation-badge {{
            background: linear-gradient(135deg, #28a745, #20c997);
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
        .selection-details {{
            background-color: #f8f9fa;
            padding: 20px;
            border-left: 4px solid #28a745;
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
            color: #28a745;
            font-weight: 500;
            font-size: 14px;
        }}
        .offer-id {{
            background: linear-gradient(135deg, #28a745, #20c997);
            color: white;
            padding: 4px 12px;
            border-radius: 15px;
            font-size: 12px;
            font-weight: 600;
        }}
        .request-title {{
            background: linear-gradient(135deg, #0055a4, #003b75);
            color: white;
            padding: 4px 12px;
            border-radius: 15px;
            font-size: 12px;
            font-weight: 600;
        }}
        .user-badge {{
            background: linear-gradient(135deg, #6f42c1, #5a32a3);
            color: white;
            padding: 4px 12px;
            border-radius: 15px;
            font-size: 12px;
            font-weight: 600;
        }}
        .alert {{
            color: #28a745;
            font-weight: bold;
            background-color: #d4edda;
            padding: 15px;
            border-radius: 4px;
            margin: 15px 0;
            border-left: 4px solid #28a745;
        }}
        .offer-summary {{
            background-color: #e8f5e8;
            padding: 15px;
            border-radius: 6px;
            margin: 15px 0;
            border: 1px solid #c3e6cb;
        }}
        .offer-summary h4 {{
            margin: 0 0 10px 0;
            color: #155724;
            font-size: 16px;
        }}
        .offer-summary p {{
            margin: 5px 0;
            font-size: 14px;
            color: #155724;
        }}
        .next-steps {{
            background-color: #fff3cd;
            padding: 15px;
            border-radius: 6px;
            margin: 20px 0;
            border-left: 4px solid #ffc107;
        }}
        .next-steps h4 {{
            margin: 0 0 10px 0;
            color: #856404;
            font-size: 16px;
        }}
        .next-steps ul {{
            margin: 10px 0;
            padding-left: 20px;
            color: #856404;
        }}
        .next-steps li {{
            margin: 5px 0;
            font-size: 14px;
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
            <h1>✅ Offer Selected</h1>
            <div class=""subtitle"">IT Inventory Management Management</div>
        </div>
        
        <div class=""content"">
            <div class=""confirmation-badge"">
                🎯 Selection Confirmed
            </div>
            
            <p class=""greeting"">Hello Sabrine,</p>
            
            <p class=""alert"">This email confirms that an offer has been successfully selected for processing.</p>
            
            <div class=""selection-details"">
                <div class=""detail-row"">
                    <span class=""detail-label"">Selected By:</span>
                    <span class=""detail-value"">
                        <span class=""user-badge"">{user}</span>
                    </span>
                </div>
                <div class=""detail-row"">
                    <span class=""detail-label"">Offer ID:</span>
                    <span class=""detail-value"">
                        <span class=""offer-id"">#{offer.Id}</span>
                    </span>
                </div>
                <div class=""detail-row"">
                    <span class=""detail-label"">Request Title:</span>
                    <span class=""detail-value"">
                        <span class=""request-title"">{offer.Request.Title}</span>
                    </span>
                </div>
                <div class=""detail-row"">
                    <span class=""detail-label"">Request ID:</span>
                    <span class=""detail-value"">#{offer.Request.Id}</span>
                </div>
                <div class=""detail-row"">
                    <span class=""detail-label"">Supplier:</span>
                    <span class=""detail-value"">{offer.Supplier?.SupplierName ?? "N/A"}</span>
                </div>
                <div class=""detail-row"">
                    <span class=""detail-label"">Selection Date:</span>
                    <span class=""detail-value"">{DateTime.Now:MMM dd, yyyy 'at' HH:mm}</span>
                </div>
            </div>
            
            <div class=""offer-summary"">
                <h4>📋 Selected Offer Summary</h4>
                <p><strong>Price:</strong> {offer.Price} TND </p>
                <p><strong>Delivery Time:</strong> {offer.DeliveryDate} </p>
                <p><strong>Status:</strong> Selected & Ready for Processing</p>
            </div>
            
        </div>
        
         <div class=""footer"">
            <p>Thank you for your attention.</p>
            <p class=""signature"">Best regards,<br>The AsteelFlash Team</p>
        </div>
    </div>
</body>
</html>";

                // EmailService.SendEmail("mortadhajouinizlatan@gmail.com", $"Offer Selection Confirmation - {user} - Request {offer.Request.Title}", htmlEmail);



                DialogService.Close();
            });

            await DialogService.OpenAsync<LoadingScreen>("", null
                , new DialogOptions() { ShowTitle = false, Style = "width:100%;height:100%;", CloseDialogOnEsc = false });




        }






    }


}
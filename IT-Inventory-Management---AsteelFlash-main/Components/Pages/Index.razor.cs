
using ITStockM.Models.ITStockManagment;
using ITStockM.Models.Constants;
using ITStockM.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

using Radzen;


namespace ITStockM.Components.Pages
{
    public partial class Index
    {

        [Inject]
        protected NavigationManager NavigationManager { get; set; } = default!;
        [Inject]
        public ITStockManagmentService ITStockManagmentService { get; set; } = default!;
        [Inject]
        protected ProtectedLocalStorage LocalStorage { get; set; } = default!;

        private List<string> months = new List<string> {"Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sept", "Oct", "Nov", "Dec" }; 

        protected int totalMaterial;

        protected double totalMaterialAugment;

        protected int totalAssignments;

        protected int totalAssignmentsChart;

        protected double totalAssignmentsAugment;

        protected int pendingRequests;
        
        protected int pendingDeliveries;

        private List<ChartDataItem> inventoryByType = new();

        protected double growthAssignment;

        private List<MonthlyData> monthlyAssignments = new List<MonthlyData> { };

        protected IEnumerable<Request> topRequests = Enumerable.Empty<Request>();

        protected IEnumerable<AssignmentMateriel> AssignmentMateriel = Enumerable.Empty<AssignmentMateriel>();

        protected List<(object AnyData, DateTime Date)> recentActivity = new List<(object AnyData, DateTime Date)> { };

        protected List<string> materialTypes = new List<string> { "All", "HardWare", "SoftWare", "Mouse", "KeyBoard", "Laptop", "Mini Pc", "Backpack", "Headphone", "Monitor", "Network device", "Printer", "Consumables" };
        protected string selectedMaterialType = "All";

        protected List<string> materialConditions = new List<string> { "Good", "Repairing", "Irreparable" };
        protected string selectedMaterialCondition = "Good";

        protected List<Materiel> Materiels = new();

        protected string userRole = string.Empty;

        // id of current user (set from protected local storage UserSession)
        protected int userId; 

        protected IEnumerable<Models.ViewModels.MaterialsListViewModel> materielsList = Enumerable.Empty<Models.ViewModels.MaterialsListViewModel>();

        // Top-3 predicted most-used materiels (usage share = usage / total usage)
        protected List<Models.ViewModels.MaterielUsageViewModel> TopUsedMateriels = new();

        protected async void RedirectPDR()
        {
           
            var userRoleLocal = (await LocalStorage.GetAsync<UserSession>("UserSession")).Value?.Role;
            if (string.Equals(userRoleLocal, UserRoles.PDR, StringComparison.OrdinalIgnoreCase))
            {
                NavigationManager.NavigateTo("/materials-view-interface-pdr");
            }
        }
        protected override async Task OnInitializedAsync()
        {
            


            AssignmentMateriel = await ITStockManagmentService.GetAssignmentMateriels(new Query { Expand = "Materiel, Assignment" });
            var requests = await ITStockManagmentService.GetRequests(new Query { });

            var offers = await ITStockManagmentService.GetOffers(new Query { });

            Materiels = (await ITStockManagmentService.GetMateriels()).ToList();
            

            var assignments = await ITStockManagmentService.GetAssignments(new Query { Expand = "AssignmentMateriels" });

            var deliveryOrderMaterials = (await ITStockManagmentService.GetDeliveryOrderMateriels(new Query { Expand = "DeliveryOrder, Materiel" }));

            var deliveryOrders = await ITStockManagmentService.GetDeliveryOrders(new Query { Expand = "DeliveryOrderMateriels" });

            topRequests = requests.Where(r => r.Status != "Done").OrderByDescending(r => r.Date).Take(5);

            totalMaterial = Materiels.Sum(m => m.QuantityPDRStock + m.QuantityITStock);

            totalMaterialAugment = deliveryOrderMaterials.Where(m => m.DeliveryOrder.Date < DateTime.Now.AddMonths(-1) && m.DeliveryOrder.Date > DateTime.Now.AddMonths(-2)).Sum(m => m.Qte); //Haw jeek zeda

            if(totalMaterialAugment == 0)
            {
                totalMaterialAugment = 100;
            }
            else
            {
                totalMaterialAugment = ((totalMaterial - totalMaterialAugment) / totalMaterialAugment) * 100;
            }
           

        

            totalAssignments = assignments.Where(assm =>  assm.RestoreDate == null ).Count();

            totalAssignmentsAugment = assignments.Where(assm =>   assm.Date < DateTime.Now.AddMonths(-1) && assm.Date > DateTime.Now.AddMonths(-2)).Count();
            if (totalAssignmentsAugment == 0) {
                totalAssignmentsAugment = 100;
            }
            else
            {
                totalAssignmentsAugment = ((totalAssignments - totalAssignmentsAugment) / totalAssignmentsAugment) * 100;
            }

            pendingRequests = requests.Where(r => r.Status != "Done").Count();

            pendingDeliveries = offers.Where(o => o.Selected == true && o.DeliveryDate.CompareTo(DateTime.Today) > 0).Count();

            //Inventory circle
            inventoryByType = new List<ChartDataItem>
                {
                    new ChartDataItem { Category = "HardWare", Value = Materiels.Where(m => m.Type == "HardWare").Sum(m => m.QuantityPDRStock + m.QuantityITStock) },
                    new ChartDataItem { Category = "SoftWare", Value = Materiels.Where(m => m.Type == "SoftWare").Sum(m => m.QuantityPDRStock + m.QuantityITStock) },
                    new ChartDataItem { Category = "Mouse", Value = Materiels.Where(m => m.Type == "Mouse").Sum(m => m.QuantityPDRStock + m.QuantityITStock) },
                    new ChartDataItem { Category = "KeyBoard", Value = Materiels.Where(m => m.Type == "KeyBoard").Sum(m => m.QuantityPDRStock + m.QuantityITStock) },
                    new ChartDataItem { Category = "Laptop", Value = Materiels.Where(m => m.Type == "Laptop").Sum(m => m.QuantityPDRStock + m.QuantityITStock) },
                    new ChartDataItem { Category = "Mini Pc", Value = Materiels.Where(m => m.Type == "Mini Pc").Sum(m => m.QuantityPDRStock + m.QuantityITStock) },
                    new ChartDataItem { Category = "Backpack", Value = Materiels.Where(m => m.Type == "Backpack").Sum(m => m.QuantityPDRStock + m.QuantityITStock) },
                    new ChartDataItem { Category = "Headphone", Value = Materiels.Where(m => m.Type == "Headphone").Sum(m => m.QuantityPDRStock + m.QuantityITStock) },
                    new ChartDataItem { Category = "Monitor", Value = Materiels.Where(m => m.Type == "Monitor").Sum(m => m.QuantityPDRStock + m.QuantityITStock) },
                    new ChartDataItem { Category = "Network device", Value = Materiels.Where(m => m.Type == "Network device").Sum(m => m.QuantityPDRStock + m.QuantityITStock) },
                    new ChartDataItem { Category = "Printer", Value = Materiels.Where(m => m.Type == "Printer").Sum(m => m.QuantityPDRStock + m.QuantityITStock) },
                    new ChartDataItem { Category = "Consumables", Value = Materiels.Where(m => m.Type == "Consumables").Sum(m => m.QuantityPDRStock + m.QuantityITStock) }


                };

            //Assignment graph
            int currentMonth = DateTime.Today.Month - 5;
            for (int j = 0; j < 5; j++)
            {
                int monthIndex = (currentMonth + j + 12) % 12;
                int actualMonth = monthIndex + 1;
                string monthName = months[monthIndex];
                monthlyAssignments.Add(new MonthlyData
                {
                    Month = monthName,
                    Count = AssignmentMateriel
                        .Where(assm => assm.Assignment.Date.Month == actualMonth &&
                                        assm.Assignment.Date > DateTime.Today.AddMonths(-6) &&
                                     assm.Assignment.OnMission == false &&
                                     assm.Assignment.AssignedTo != 6).Sum(assm => assm.Qte)
                });
            }
            totalAssignmentsChart = monthlyAssignments.Select(massm => massm.Count).Sum();
            int oldAssignemnts = AssignmentMateriel
                        .Where(assm => assm.Assignment.Date < DateTime.Today.AddMonths(-6) &&
                                        assm.Assignment.Date > DateTime.Today.AddMonths(-12) &&
                                      assm.Assignment.OnMission == false &&
                                      assm.Assignment.AssignedTo != 6).Sum(assm => assm.Qte);
                
            if (oldAssignemnts != 0)
            {
                growthAssignment = (monthlyAssignments.Select(massm => massm.Count).Sum() / oldAssignemnts) * 100;
            }
            else
            {
                growthAssignment = 100;
            }


            foreach (var req in requests.Where(request => request.ApprovedAt != null).OrderByDescending(request => request.ApprovedAt).Take(5))
            {
                recentActivity.Add((req, req.ApprovedAt.Value)); 
            }

            foreach (var deliveryOrderMaterial in deliveryOrders.OrderByDescending(delo => delo.Date).Take(5))
            {
                recentActivity.Add((deliveryOrderMaterial, deliveryOrderMaterial.Date));
                
            }
            foreach (var assignment in assignments.OrderByDescending(assm => assm.Date).Take(5))
            {
                recentActivity.Add((assignment, assignment.Date));
            }

            recentActivity = recentActivity.OrderByDescending(rc => rc.Date).Take(5).ToList();

            

            materielsList = Materiels.Where(m => m.QuantityITStock != 0 || m.QuantityPDRStock != 0 )
            .GroupBy(m => m.MaterielName)
                           .Select(g => new Models.ViewModels.MaterialsListViewModel
                           {
                               MatName = g.Key,
                               Qte = g.Sum(m => m.QuantityPDRStock + m.QuantityITStock),
                               Type = g.First().Type
                           }).Where(nm => nm.Qte < 10);

            var userSession = (await LocalStorage.GetAsync<UserSession>("UserSession")).Value;
            if (userSession != null)
            {
                userRole = userSession.Role;
                userId = userSession.Id;
            }

            // compute top-3 used materiels and (for Admin/PDR) send daily summary email once per day
            TopUsedMateriels = await ITStockManagmentService.GetTopUsedMateriels(3);
            if (userSession != null &&
                (string.Equals(userRole, UserRoles.Admin, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(userRole, UserRoles.PDR, StringComparison.OrdinalIgnoreCase)))
            {
                // operationNotificationService + IMemoryCache ensure email is sent at most once per day per recipient
                await ITStockManagmentService.SendTopUsedMaterielsEmailIfNotSentToday(userSession.Email);

                // send a single low-stock summary (one email listing all low-stock materiels) when Admin/PDR open dashboard
                await ITStockManagmentService.SendLowStockSummaryEmailIfNotSentToday(userSession.Email);
            }






        }


        protected async Task MaterialConditionsChange()
        {
            if (selectedMaterialCondition == "Good")
            {

                inventoryByType = new List<ChartDataItem>
                {
                    new ChartDataItem { Category = "HardWare", Value = Materiels.Where(m => m.Type == "HardWare").Sum(m => m.QuantityPDRStock + m.QuantityITStock) },
                    new ChartDataItem { Category = "SoftWare", Value = Materiels.Where(m => m.Type == "SoftWare").Sum(m => m.QuantityPDRStock + m.QuantityITStock) },
                    new ChartDataItem { Category = "Mouse", Value = Materiels.Where(m => m.Type == "Mouse").Sum(m => m.QuantityPDRStock + m.QuantityITStock) },
                    new ChartDataItem { Category = "KeyBoard", Value = Materiels.Where(m => m.Type == "KeyBoard").Sum(m => m.QuantityPDRStock + m.QuantityITStock) },
                    new ChartDataItem { Category = "Laptop", Value = Materiels.Where(m => m.Type == "Laptop").Sum(m => m.QuantityPDRStock + m.QuantityITStock) },
                    new ChartDataItem { Category = "Mini Pc", Value = Materiels.Where(m => m.Type == "Mini Pc").Sum(m => m.QuantityPDRStock + m.QuantityITStock) },
                    new ChartDataItem { Category = "Backpack", Value = Materiels.Where(m => m.Type == "Backpack").Sum(m => m.QuantityPDRStock + m.QuantityITStock) },
                    new ChartDataItem { Category = "Headphone", Value = Materiels.Where(m => m.Type == "Headphone").Sum(m => m.QuantityPDRStock + m.QuantityITStock) },
                    new ChartDataItem { Category = "Monitor", Value = Materiels.Where(m => m.Type == "Monitor").Sum(m => m.QuantityPDRStock + m.QuantityITStock) },
                    new ChartDataItem { Category = "Network device", Value = Materiels.Where(m => m.Type == "Network device").Sum(m => m.QuantityPDRStock + m.QuantityITStock) },
                    new ChartDataItem { Category = "Printer", Value = Materiels.Where(m => m.Type == "Printer").Sum(m => m.QuantityPDRStock + m.QuantityITStock) },
                    new ChartDataItem { Category = "Consumables", Value = Materiels.Where(m => m.Type == "Consumables").Sum(m => m.QuantityPDRStock + m.QuantityITStock) }

                };


            }
            else if (selectedMaterialCondition == "Repairing")
            {
               
                inventoryByType = new List<ChartDataItem>
                {
                    new ChartDataItem { Category = "HardWare", Value = Materiels.Where(m => m.Type == "HardWare").Sum(m => m.Repairing_Quantity )},
                    new ChartDataItem { Category = "SoftWare", Value = Materiels.Where(m => m.Type == "SoftWare").Sum(m => m.Repairing_Quantity) },
                    new ChartDataItem { Category = "Mouse", Value = Materiels.Where(m => m.Type == "Mouse").Sum(m => m.Repairing_Quantity) },
                    new ChartDataItem { Category = "KeyBoard", Value = Materiels.Where(m => m.Type == "KeyBoard").Sum(m => m.Repairing_Quantity) },
                    new ChartDataItem { Category = "Laptop", Value = Materiels.Where(m => m.Type == "Laptop").Sum(m => m.Repairing_Quantity) },
                    new ChartDataItem { Category = "Mini Pc", Value = Materiels.Where(m => m.Type == "Mini Pc").Sum(m => m.Repairing_Quantity) },
                    new ChartDataItem { Category = "Backpack", Value = Materiels.Where(m => m.Type == "Backpack").Sum(m => m.Repairing_Quantity) },
                    new ChartDataItem { Category = "Headphone", Value = Materiels.Where(m => m.Type == "Headphone").Sum(m => m.Repairing_Quantity) },
                    new ChartDataItem { Category = "Monitor", Value = Materiels.Where(m => m.Type == "Monitor").Sum(m => m.Repairing_Quantity) },
                    new ChartDataItem { Category = "Network device", Value = Materiels.Where(m => m.Type == "Network device").Sum(m => m.Repairing_Quantity) },
                    new ChartDataItem { Category = "Printer", Value = Materiels.Where(m => m.Type == "Printer").Sum(m => m.Repairing_Quantity) },
                    new ChartDataItem { Category = "Consumables", Value = Materiels.Where(m => m.Type == "Consumables").Sum(m => m.Repairing_Quantity) }

                };
            }
            else 
            {
              
                inventoryByType = new List<ChartDataItem>
                {
                    new ChartDataItem { Category = "HardWare", Value = Materiels.Where(m => m.Type == "HardWare").Sum(m => m.IrreparableQuantity) },
                    new ChartDataItem { Category = "SoftWare", Value = Materiels.Where(m => m.Type == "SoftWare").Sum(m => m.IrreparableQuantity) },
                    new ChartDataItem { Category = "Mouse", Value = Materiels.Where(m => m.Type == "Mouse").Sum(m => m.IrreparableQuantity) },
                    new ChartDataItem { Category = "KeyBoard", Value = Materiels.Where(m => m.Type == "KeyBoard").Sum(m => m.IrreparableQuantity) },
                    new ChartDataItem { Category = "Laptop", Value = Materiels.Where(m => m.Type == "Laptop").Sum(m => m.IrreparableQuantity) },
                    new ChartDataItem { Category = "Mini Pc", Value = Materiels.Where(m => m.Type == "Mini Pc").Sum(m => m.IrreparableQuantity) },
                    new ChartDataItem { Category = "Backpack", Value = Materiels.Where(m => m.Type == "Backpack").Sum(m => m.IrreparableQuantity) },
                    new ChartDataItem { Category = "Headphone", Value = Materiels.Where(m => m.Type == "Headphone").Sum(m => m.IrreparableQuantity) },
                    new ChartDataItem { Category = "Monitor", Value = Materiels.Where(m => m.Type == "Monitor").Sum(m => m.IrreparableQuantity) },
                    new ChartDataItem { Category = "Network device", Value = Materiels.Where(m => m.Type == "Network device").Sum(m => m.IrreparableQuantity) },
                    new ChartDataItem { Category = "Printer", Value = Materiels.Where(m => m.Type == "Printer").Sum(m => m.IrreparableQuantity) },
                    new ChartDataItem { Category = "Consumables", Value = Materiels.Where(m => m.Type == "Consumables").Sum(m => m.IrreparableQuantity) }
                        
                };
            }
            
        }


        protected async Task AssignmentFilter()
        {
            monthlyAssignments.Clear(); 

            if (selectedMaterialType == "All")
            {
                
                int currentMonth = DateTime.Today.Month - 5;
                for (int j = 0; j < 5; j++)
                {
                    int monthIndex = (currentMonth + j + 12) % 12;
                    int actualMonth = monthIndex + 1;
                    string monthName = months[monthIndex];
                    monthlyAssignments.Add(new MonthlyData
                    {
                        Month = monthName,
                        Count = AssignmentMateriel
                            .Where(assm => assm.Assignment.Date.Month == actualMonth &&
                                          assm.Assignment.Date > DateTime.Today.AddMonths(-6) &&
                                         assm.Assignment.OnMission == false &&
                                         assm.Assignment.AssignedTo != 6).Sum(assm => assm.Qte)
                    });
                    totalAssignmentsChart = monthlyAssignments.Select(massm => massm.Count).Sum();
                }

                int oldAssignemnts = AssignmentMateriel
                        .Where(assm => assm.Assignment.Date < DateTime.Today.AddMonths(-6) &&
                                        assm.Assignment.Date > DateTime.Today.AddMonths(-12) &&
                                      assm.Assignment.OnMission == false &&
                                         assm.Assignment.AssignedTo != 6).Sum(assm => assm.Qte);

                if (oldAssignemnts != 0)
                {
                    growthAssignment = (monthlyAssignments.Select(massm => massm.Count).Sum() / oldAssignemnts) * 100;
                }
                else
                {
                    growthAssignment = 100;
                }
            }
            else
            {
                // Filter by material type
                int currentMonth = DateTime.Today.Month - 5;
                for (int j = 0; j < 5; j++)
                {
                    int monthIndex = (currentMonth + j + 12) % 12;
                    int actualMonth = monthIndex + 1;
                    string monthName = months[monthIndex];
                    monthlyAssignments.Add(new MonthlyData
                    {
                        Month = monthName,
                        Count = AssignmentMateriel
                            .Where(assm => assm.Materiel.Type == selectedMaterialType &&
                                         assm.Assignment.Date.Month == actualMonth &&
                                         assm.Assignment.Date > DateTime.Today.AddMonths(-6) &&
                                         assm.Assignment.OnMission == false &&
                                         assm.Assignment.AssignedTo != 6).Sum (assm => assm.Qte)
                    });
                    totalAssignmentsChart = monthlyAssignments.Select(massm => massm.Count).Sum();
                    int oldAssignemnts = AssignmentMateriel
                        .Where(assm => assm.Assignment.Date < DateTime.Today.AddMonths(-6) &&
                                        assm.Assignment.Date > DateTime.Today.AddMonths(-12) &&
                                      assm.Assignment.OnMission == false &&
                                         assm.Assignment.AssignedTo != 6).Sum(assm => assm.Qte);

                    if (oldAssignemnts != 0)
                    {
                        growthAssignment = (monthlyAssignments.Select(massm => massm.Count).Sum() / oldAssignemnts) * 100;
                    }
                    else
                    {
                        growthAssignment = 100;
                    }
                }
            }

            

         
            StateHasChanged();
        }

      

        protected string assignmentText(Assignment assm)
        {
            if (assm?.AssignmentMateriels == null || !assm.AssignmentMateriels.Any())
            {
                return "No materials";
            }

            var materialParts = assm.AssignmentMateriels
                .Where(assmm => assmm.Materiel != null)
                .Select(assmm => $"{assmm.Qte} {assmm.Materiel.MaterielName}")
                .Where(part => !string.IsNullOrWhiteSpace(part));

            return string.Join(", ", materialParts);


        }

        protected string deliveryOrderToText(DeliveryOrder deliveryOrder)
        {
            if (deliveryOrder?.DeliveryOrderMateriels == null || !deliveryOrder.DeliveryOrderMateriels.Any())
            {
                return "No materials";
            }

            var materialParts = deliveryOrder.DeliveryOrderMateriels
                .Where(delOrd => delOrd.Materiel != null)
                .Select(delOrd => $"{delOrd.Qte} {delOrd.Materiel.MaterielName}")
                .Where(part => !string.IsNullOrWhiteSpace(part));

            return string.Join(", ", materialParts);

        }

    

        
        private string GetStatusBadgeClass(string? status)
        {
            return status switch
            {
                "Urgent" => "badge-warning",
                "Normal" => "badge-success",
                "Critical" => "badge-danger",
                _ => "badge-secondary"
            };
        }

    }
    public class ChartDataItem
    {
        public string Category { get; set; } = string.Empty;
        public double Value { get; set; }
    }

    public class MonthlyData
    {
        public string Month { get; set; } = string.Empty;
        public int Count { get; set; }
    }


}
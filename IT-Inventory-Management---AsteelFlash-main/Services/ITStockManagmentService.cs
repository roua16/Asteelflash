using System;
using System.Data;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Components;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Radzen;

using ITStockM.Data;
using ITStockM.Models.ITStockManagment;
using System.Linq.Expressions;
using ITStockM.Models.ViewModels;
using ITStockM.Services.Interfaces;
using ITStockM.Services.Utilities;
using ITStockM.Services.Export;
using ITStockM.Repositories;

namespace ITStockM.Services
{
    public partial class ITStockManagmentService
    {
        ITStockManagmentContext Context
        {
            get
            {
                return context;
            }
        }

        private readonly ITStockManagmentContext context;
        private readonly IServiceScopeFactory scopeFactory;
        private readonly NavigationManager navigationManager;
        private readonly IOperationNotificationService? operationNotificationService;
        private readonly IExportService exportService;

        // Repositories (legacy kept for direct context access in some methods)
        private readonly IAssignmentRepository assignmentRepository;
        private readonly IRequestRepository requestRepository;
        private readonly IDeliveryOrderRepository deliveryOrderRepository;
        private readonly IMaterielRepository materielRepository;

        // New domain services
        private readonly Services.Assignments.IAssignmentService assignmentService;
        private readonly Services.Materiels.IMaterielService materielService;
        private readonly Services.Requests.IRequestService requestService;
        private readonly Services.DeliveryOrders.IDeliveryOrderService deliveryOrderService;

        public ITStockManagmentService(
            ITStockManagmentContext context,
            IServiceScopeFactory scopeFactory,
            NavigationManager navigationManager,
            IExportService exportService,
            IAssignmentRepository assignmentRepository,
            IRequestRepository requestRepository,
            IDeliveryOrderRepository deliveryOrderRepository,
            IMaterielRepository materielRepository,
            Services.Assignments.IAssignmentService assignmentService,
            Services.Requests.IRequestService requestService,
            Services.DeliveryOrders.IDeliveryOrderService deliveryOrderService,
            Services.Materiels.IMaterielService materielService,
            IOperationNotificationService? operationNotificationService = null)
        {
            this.context = context;
            this.scopeFactory = scopeFactory;
            this.navigationManager = navigationManager;
            this.operationNotificationService = operationNotificationService;
            this.exportService = exportService;
            this.assignmentRepository = assignmentRepository;
            this.requestRepository = requestRepository;
            this.deliveryOrderRepository = deliveryOrderRepository;
            this.materielRepository = materielRepository;
            this.assignmentService = assignmentService;
            this.requestService = requestService;
            this.deliveryOrderService = deliveryOrderService;
            this.materielService = materielService;
        }

        public void Reset() => Context.ChangeTracker.Entries().Where(e => e.Entity != null).ToList().ForEach(e => e.State = EntityState.Detached);


        public async Task ExportAssignmentsToExcel(Query query = null, string fileName = null)
            => await exportService.ExportToExcel("export/itstockmanagment/assignments", query, fileName);

        public async Task ExportAssignmentsToCSV(Query query = null, string fileName = null)
            => await exportService.ExportToCSV("export/itstockmanagment/assignments", query, fileName);

        partial void OnAssignmentsRead(ref IQueryable<Assignment> items);

        public async Task<IQueryable<Assignment>> GetAssignments(Query query = null)
        {
            return await assignmentService.GetAssignments(query);
        }

        partial void OnAssignmentGet(Assignment item);
        partial void OnGetAssignmentById(ref IQueryable<Assignment> items);


        public async Task<Assignment> GetAssignmentById(int id)
        {
            return await assignmentService.GetAssignmentById(id) ?? throw new Exception("Item not found");
        }

        partial void OnAssignmentCreated(Assignment item);
        partial void OnAfterAssignmentCreated(Assignment item);

        public async Task<Assignment> CreateAssignment(Assignment assignment)
        {
            OnAssignmentCreated(assignment);
            var created = await assignmentService.CreateAssignment(assignment);
            OnAfterAssignmentCreated(created);
            return created;
        }



        partial void OnAssignmentUpdated(Assignment item);
        partial void OnAfterAssignmentUpdated(Assignment item);

        public async Task<Assignment> UpdateAssignment(int id, Assignment assignment)
        {
            OnAssignmentUpdated(assignment);

            var updated = await assignmentService.UpdateAssignment(id, assignment);

            OnAfterAssignmentUpdated(updated);

            return updated;
        }

        partial void OnAssignmentDeleted(Assignment item);
        partial void OnAfterAssignmentDeleted(Assignment item);

        public async Task<Assignment> DeleteAssignment(int id)
        {
            var deleted = await assignmentService.DeleteAssignment(id);
            OnAssignmentDeleted(deleted);
            OnAfterAssignmentDeleted(deleted);
            return deleted;
        }

        public async Task ExportAssignmentMaterielsToExcel(Query query = null, string fileName = null)
        {
            navigationManager.NavigateTo(query != null ? query.ToUrl($"export/itstockmanagment/assignmentmateriels/excel(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')") : $"export/itstockmanagment/assignmentmateriels/excel(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')", true);
        }

        public async Task ExportAssignmentMaterielsToCSV(Query query = null, string fileName = null)
        {
            navigationManager.NavigateTo(query != null ? query.ToUrl($"export/itstockmanagment/assignmentmateriels/csv(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')") : $"export/itstockmanagment/assignmentmateriels/csv(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')", true);
        }

        partial void OnAssignmentMaterielsRead(ref IQueryable<AssignmentMateriel> items);

        public async Task<IQueryable<AssignmentMateriel>> GetAssignmentMateriels(Query query = null)
        {
            var items = Context.AssignmentMateriels.AsQueryable();

            items = items.Include(i => i.Assignment);
            items = items.Include(i => i.Materiel);
            items = items.Include(i => i.Assignment.Employee);
            items = items.Include(i => i.Assignment.Project);
            items = items.Include(i => i.Assignment.AssignedEmployee);

            if (query != null)
            {
                if (!string.IsNullOrEmpty(query.Expand))
                {
                    var propertiesToExpand = query.Expand.Split(',');
                    foreach (var p in propertiesToExpand)
                    {
                        items = items.Include(p.Trim());
                    }
                }

                items = items.ApplyQuery(query);
            }

            OnAssignmentMaterielsRead(ref items);

            return await Task.FromResult(items);
        }

        partial void OnAssignmentMaterielGet(AssignmentMateriel item);
        partial void OnGetAssignmentMaterielByMaterielIdAndAssignmentId(ref IQueryable<AssignmentMateriel> items);


        public async Task<AssignmentMateriel> GetAssignmentMaterielByMaterielIdAndAssignmentId(int materielid, int assignmentid)
        {
            var items = Context.AssignmentMateriels
                              .AsNoTracking()
                              .Where(i => i.MaterielId == materielid && i.AssignmentId == assignmentid);

            items = items.Include(i => i.Assignment);
            items = items.Include(i => i.Materiel);

            OnGetAssignmentMaterielByMaterielIdAndAssignmentId(ref items);

            var itemToReturn = items.FirstOrDefault();

            OnAssignmentMaterielGet(itemToReturn);

            return await Task.FromResult(itemToReturn);
        }

        partial void OnAssignmentMaterielCreated(AssignmentMateriel item);
        partial void OnAfterAssignmentMaterielCreated(AssignmentMateriel item);

        public async Task<AssignmentMateriel> CreateAssignmentMateriel(AssignmentMateriel assignmentmateriel)
        {
            OnAssignmentMaterielCreated(assignmentmateriel);

            var existingItem = Context.AssignmentMateriels
                              .Where(i => i.MaterielId == assignmentmateriel.MaterielId && i.AssignmentId == assignmentmateriel.AssignmentId)
                              .FirstOrDefault();

            if (existingItem != null)
            {
                throw new Exception("Item already available");
            }

            try
            {
                Context.AssignmentMateriels.Add(assignmentmateriel);
                Context.SaveChanges();
            }
            catch
            {
                Context.Entry(assignmentmateriel).State = EntityState.Detached;
                throw;
            }

            OnAfterAssignmentMaterielCreated(assignmentmateriel);

            return assignmentmateriel;
        }



        partial void OnAssignmentMaterielUpdated(AssignmentMateriel item);
        partial void OnAfterAssignmentMaterielUpdated(AssignmentMateriel item);

        public async Task<AssignmentMateriel> UpdateAssignmentMateriel(int materielid, int assignmentid, AssignmentMateriel assignmentmateriel)
        {
            OnAssignmentMaterielUpdated(assignmentmateriel);

            var itemToUpdate = Context.AssignmentMateriels
                              .Where(i => i.MaterielId == assignmentmateriel.MaterielId && i.AssignmentId == assignmentmateriel.AssignmentId)
                              .FirstOrDefault();

            if (itemToUpdate == null)
            {
                throw new Exception("Item no longer available");
            }

            var entryToUpdate = Context.Entry(itemToUpdate);
            entryToUpdate.CurrentValues.SetValues(assignmentmateriel);
            entryToUpdate.State = EntityState.Modified;

            Context.SaveChanges();

            OnAfterAssignmentMaterielUpdated(assignmentmateriel);

            return assignmentmateriel;
        }

        partial void OnAssignmentMaterielDeleted(AssignmentMateriel item);
        partial void OnAfterAssignmentMaterielDeleted(AssignmentMateriel item);

        public async Task<AssignmentMateriel> DeleteAssignmentMateriel(int materielid, int assignmentid)
        {
            var itemToDelete = Context.AssignmentMateriels
                              .Where(i => i.MaterielId == materielid && i.AssignmentId == assignmentid)
                              .FirstOrDefault();

            if (itemToDelete == null)
            {
                throw new Exception("Item no longer available");
            }

            OnAssignmentMaterielDeleted(itemToDelete);


            Context.AssignmentMateriels.Remove(itemToDelete);

            try
            {
                Context.SaveChanges();
            }
            catch
            {
                Context.Entry(itemToDelete).State = EntityState.Unchanged;
                throw;
            }

            OnAfterAssignmentMaterielDeleted(itemToDelete);

            return itemToDelete;
        }

        public async Task ExportDeliveryOrdersToExcel(Query query = null, string fileName = null)
        {
            navigationManager.NavigateTo(query != null ? query.ToUrl($"export/itstockmanagment/deliveryorders/excel(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')") : $"export/itstockmanagment/deliveryorders/excel(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')", true);
        }

        public async Task ExportDeliveryOrdersToCSV(Query query = null, string fileName = null)
        {
            navigationManager.NavigateTo(query != null ? query.ToUrl($"export/itstockmanagment/deliveryorders/csv(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')") : $"export/itstockmanagment/deliveryorders/csv(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')", true);
        }

        partial void OnDeliveryOrdersRead(ref IQueryable<DeliveryOrder> items);

        public async Task<IQueryable<DeliveryOrder>> GetDeliveryOrders(Query query = null)
        {
            var items = Context.DeliveryOrders.AsQueryable();

            if (query != null && !string.IsNullOrEmpty(query.Expand))
            {
                var propertiesToExpand = query.Expand.Split(',');
                foreach (var p in propertiesToExpand)
                {
                    items = items.Include(p.Trim());
                }
            }
            else
            {
                // Only include related entities if no specific expand is requested
                items = items.Include(i => i.Supplier);
                items = items.Include(i => i.Employee);
            }

            if (query != null)
            {
                items = items.ApplyQuery(query);
            }

            OnDeliveryOrdersRead(ref items);

            return await Task.FromResult(items);
        }

        // Convenience helper that materializes the IQueryable into a List to avoid lifetime and deferred-execution issues
        public async Task<List<DeliveryOrder>> GetDeliveryOrdersList(Query query = null)
        {
            using var scope = scopeFactory.CreateScope();
            var ctx = scope.ServiceProvider.GetRequiredService<ITStockManagmentContext>();
            var items = ctx.DeliveryOrders.AsQueryable();

            if (query != null && !string.IsNullOrEmpty(query.Expand))
            {
                var propertiesToExpand = query.Expand.Split(',');
                foreach (var p in propertiesToExpand)
                {
                    items = items.Include(p.Trim());
                }
            }
            else
            {
                // Only include related entities if no specific expand is requested
                items = items.Include(i => i.Supplier);
                items = items.Include(i => i.Employee);
            }

            if (query != null)
            {
                items = items.ApplyQuery(query);
            }

            OnDeliveryOrdersRead(ref items);

            return await items.ToListAsync();
        }

        partial void OnDeliveryOrderGet(DeliveryOrder item);
        partial void OnGetDeliveryOrderByDeleveryOrderNumber(ref IQueryable<DeliveryOrder> items);


        public async Task<DeliveryOrder> GetDeliveryOrderByDeleveryOrderNumber(string deleveryordernumber)
        {
            var items = Context.DeliveryOrders
                              .AsNoTracking()
                              .Where(i => i.DeleveryOrderNumber == deleveryordernumber);

            items = items.Include(i => i.Supplier);

            OnGetDeliveryOrderByDeleveryOrderNumber(ref items);

            var itemToReturn = items.FirstOrDefault();

            OnDeliveryOrderGet(itemToReturn);

            return await Task.FromResult(itemToReturn);
        }

        partial void OnDeliveryOrderCreated(DeliveryOrder item);
        partial void OnAfterDeliveryOrderCreated(DeliveryOrder item);

        public async Task<DeliveryOrder> CreateDeliveryOrder(DeliveryOrder deliveryorder)
        {
            OnDeliveryOrderCreated(deliveryorder);

            var existingItem = Context.DeliveryOrders
                              .Where(i => i.DeleveryOrderNumber == deliveryorder.DeleveryOrderNumber)
                              .FirstOrDefault();

            if (existingItem != null)
            {
                throw new Exception("Item already available");
            }

            try
            {
                Context.DeliveryOrders.Add(deliveryorder);
                Context.SaveChanges();
            }
            catch
            {
                Context.Entry(deliveryorder).State = EntityState.Detached;
                throw;
            }

            OnAfterDeliveryOrderCreated(deliveryorder);

            // Send email notification
            _ = Task.Run(async () =>
            {
                if (operationNotificationService != null)
                    await operationNotificationService.NotifyDeliveryOrderCreated(deliveryorder);
            });

            return deliveryorder;
        }


        partial void OnDeliveryOrderUpdated(DeliveryOrder item);
        partial void OnAfterDeliveryOrderUpdated(DeliveryOrder item);

        public async Task<DeliveryOrder> UpdateDeliveryOrder(string deleveryordernumber, DeliveryOrder deliveryorder)
        {
            OnDeliveryOrderUpdated(deliveryorder);

            var itemToUpdate = Context.DeliveryOrders
                              .Where(i => i.DeleveryOrderNumber == deliveryorder.DeleveryOrderNumber)
                              .FirstOrDefault();

            if (itemToUpdate == null)
            {
                throw new Exception("Item no longer available");
            }

            var entryToUpdate = Context.Entry(itemToUpdate);
            entryToUpdate.CurrentValues.SetValues(deliveryorder);
            entryToUpdate.State = EntityState.Modified;

            Context.SaveChanges();

            OnAfterDeliveryOrderUpdated(deliveryorder);

            // Send email notification
            _ = Task.Run(async () =>
            {
                if (operationNotificationService != null)
                    await operationNotificationService.NotifyDeliveryOrderUpdated(deliveryorder);
            });

            return deliveryorder;
        }

        partial void OnDeliveryOrderDeleted(DeliveryOrder item);
        partial void OnAfterDeliveryOrderDeleted(DeliveryOrder item);



        public async Task ExportEmployeesToExcel(Query query = null, string fileName = null)
        {
            navigationManager.NavigateTo(query != null ? query.ToUrl($"export/itstockmanagment/employees/excel(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')") : $"export/itstockmanagment/employees/excel(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')", true);
        }

        public async Task ExportEmployeesToCSV(Query query = null, string fileName = null)
        {
            navigationManager.NavigateTo(query != null ? query.ToUrl($"export/itstockmanagment/employees/csv(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')") : $"export/itstockmanagment/employees/csv(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')", true);
        }

        partial void OnEmployeesRead(ref IQueryable<Employee> items);

        public async Task<IQueryable<Employee>> GetEmployees(Query query = null)
        {
            var items = Context.Employees.AsQueryable();


            if (query != null)
            {
                if (!string.IsNullOrEmpty(query.Expand))
                {
                    var propertiesToExpand = query.Expand.Split(',');
                    foreach (var p in propertiesToExpand)
                    {
                        items = items.Include(p.Trim());
                    }
                }

                items = items.ApplyQuery(query);
            }

            OnEmployeesRead(ref items);

            return await Task.FromResult(items);
        }








        public async Task ExportMaterielsToExcel(Query query = null, string fileName = null)
        {
            navigationManager.NavigateTo(query != null ? query.ToUrl($"export/itstockmanagment/materiels/excel(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')") : $"export/itstockmanagment/materiels/excel(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')", true);
        }

        public async Task ExportMaterielsToCSV(Query query = null, string fileName = null)
        {
            navigationManager.NavigateTo(query != null ? query.ToUrl($"export/itstockmanagment/materiels/csv(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')") : $"export/itstockmanagment/materiels/csv(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')", true);
        }



        partial void OnMaterielsRead(ref IQueryable<Materiel> items);

        public async Task<IQueryable<Materiel>> GetMateriels(Query query = null)
        {
            var items = Context.Materiels.AsQueryable();

            items = items.Include(i => i.AssignmentMateriels).ThenInclude(i => i.Assignment);




            if (query != null)
            {
                if (!string.IsNullOrEmpty(query.Expand))
                {
                    var propertiesToExpand = query.Expand.Split(',');
                    foreach (var p in propertiesToExpand)
                    {
                        items = items.Include(p.Trim());
                    }
                }

                items = items.ApplyQuery(query);
            }

            OnMaterielsRead(ref items);

            return await Task.FromResult(items);
        }

        partial void OnMaterielGet(Materiel item);
        partial void OnGetMaterielById(ref IQueryable<Materiel> items);


        public async Task<Materiel> GetMaterielById(int id)
        {
            var items = Context.Materiels
                              .AsNoTracking()
                              .Where(i => i.Id == id);



            OnGetMaterielById(ref items);

            var itemToReturn = items.FirstOrDefault();

            OnMaterielGet(itemToReturn);

            return await Task.FromResult(itemToReturn);
        }
        public async Task<Materiel> GetMaterielByName(string name)
        {
            var items = Context.Materiels
                              .AsNoTracking()
                              .Where(i => i.MaterielName == name);



            OnGetMaterielById(ref items);

            var itemToReturn = items.FirstOrDefault();

            OnMaterielGet(itemToReturn);

            return await Task.FromResult(itemToReturn);
        }

        partial void OnMaterielCreated(Materiel item);
        partial void OnAfterMaterielCreated(Materiel item);

        public async Task<Materiel> CreateMateriel(Materiel materiel)
        {
            OnMaterielCreated(materiel);

            var existingItem = Context.Materiels
                              .Where(i => i.Id == materiel.Id)
                              .FirstOrDefault();

            if (existingItem != null)
            {
                throw new Exception("Material Item already available");
            }

            try
            {
                Context.Materiels.Add(materiel);
                Context.SaveChanges();
            }
            catch
            {
                Context.Entry(materiel).State = EntityState.Detached;
                throw;
            }

            OnAfterMaterielCreated(materiel);

            // Send email notification
            _ = Task.Run(async () =>
            {
                if (operationNotificationService != null)
                    await operationNotificationService.NotifyMaterielCreated(materiel);
            });

            return materiel;
        }

        /// <summary>
        /// Register a material that was returned but is not part of the assignment (create if missing or update existing quantities).
        /// Defaults: Type='Unknown', Warranty = 1 year from now.
        /// </summary>
        public async Task<Materiel> RegisterReturnedMaterial(string materielName, int qty, string condition)
        {
            if (string.IsNullOrWhiteSpace(materielName) || qty <= 0) throw new ArgumentException("Invalid material or quantity");

            var existing = Context.Materiels.FirstOrDefault(m => m.MaterielName == materielName);
            if (existing == null)
            {
                var newMat = new Materiel
                {
                    MaterielName = materielName,
                    Type = "Unknown",
                    QuantityITStock = 0,
                    QuantityPDRStock = 0,
                    IrreparableQuantity = 0,
                    Repairing_Quantity = 0,
                    Warranty = DateTime.UtcNow.AddYears(1)
                };

                // apply returned qty according to condition
                if (string.Equals(condition, "Good Conditions", StringComparison.OrdinalIgnoreCase))
                    newMat.QuantityITStock += qty;
                else if (string.Equals(condition, "Need to be repaired", StringComparison.OrdinalIgnoreCase))
                    newMat.Repairing_Quantity += qty;
                else
                    newMat.IrreparableQuantity += qty;

                Context.Materiels.Add(newMat);
                Context.SaveChanges();

                // notify
                _ = Task.Run(async () =>
                {
                    if (operationNotificationService != null)
                        await operationNotificationService.NotifyMaterielCreated(newMat);
                });

                return newMat;
            }
            else
            {
                // update existing
                if (string.Equals(condition, "Good Conditions", StringComparison.OrdinalIgnoreCase))
                    existing.QuantityITStock += qty;
                else if (string.Equals(condition, "Need to be repaired", StringComparison.OrdinalIgnoreCase))
                    existing.Repairing_Quantity += qty;
                else
                    existing.IrreparableQuantity += qty;

                Context.SaveChanges();

                _ = Task.Run(async () =>
                {
                    if (operationNotificationService != null)
                        await operationNotificationService.NotifyMaterielUpdated(existing);
                });

                return existing;
            }
        }

        partial void OnMaterielUpdated(Materiel item);
        partial void OnAfterMaterielUpdated(Materiel item);

        public async Task<Materiel> UpdateMateriel(int id, Materiel materiel)
        {
            OnMaterielUpdated(materiel);

            var itemToUpdate = Context.Materiels
                              .Where(i => i.Id == materiel.Id)
                              .FirstOrDefault();

            if (itemToUpdate == null)
            {
                throw new Exception("Item no longer available");
            }

            var entryToUpdate = Context.Entry(itemToUpdate);

            // capture previous totals to detect crossing threshold
            var previousTotal = itemToUpdate.QuantityITStock + itemToUpdate.QuantityPDRStock;

            entryToUpdate.CurrentValues.SetValues(materiel);
            entryToUpdate.State = EntityState.Modified;

            Context.SaveChanges();

            OnAfterMaterielUpdated(materiel);

            // Send material updated notification
            _ = Task.Run(async () =>
            {
                if (operationNotificationService != null)
                    await operationNotificationService.NotifyMaterielUpdated(materiel);
            });

            // Send low-stock notification when crossing the configured threshold (default 10)
            var threshold = int.TryParse(Environment.GetEnvironmentVariable("LOW_STOCK_THRESHOLD"), out var envThreshold) ? envThreshold : 10;
            var newTotal = materiel.QuantityITStock + materiel.QuantityPDRStock;

            if (previousTotal >= threshold && newTotal < threshold)
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        if (operationNotificationService != null)
                            await operationNotificationService.NotifyMaterielLowStock(materiel, threshold);
                    }
                    catch (Exception ex)
                    {
                        // ensure we don't break the main flow
                    }
                });
            }

            return materiel;
        }

        partial void OnMaterielDeleted(Materiel item);
        partial void OnAfterMaterielDeleted(Materiel item);



        public async Task ExportOffersToExcel(Query query = null, string fileName = null)
        {
            navigationManager.NavigateTo(query != null ? query.ToUrl($"export/itstockmanagment/offers/excel(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')") : $"export/itstockmanagment/offers/excel(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')", true);
        }

        public async Task ExportOffersToCSV(Query query = null, string fileName = null)
        {
            navigationManager.NavigateTo(query != null ? query.ToUrl($"export/itstockmanagment/offers/csv(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')") : $"export/itstockmanagment/offers/csv(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')", true);
        }

        partial void OnOffersRead(ref IQueryable<Offer> items);

        public async Task<IQueryable<Offer>> GetOffers(Query query = null)
        {
            var items = Context.Offers.AsQueryable();

            items = items.Include(i => i.Request);
            items = items.Include(i => i.Supplier);

            if (query != null)
            {
                if (!string.IsNullOrEmpty(query.Expand))
                {
                    var propertiesToExpand = query.Expand.Split(',');
                    foreach (var p in propertiesToExpand)
                    {
                        items = items.Include(p.Trim());
                    }
                }

                items = items.ApplyQuery(query);
            }

            OnOffersRead(ref items);

            return await Task.FromResult(items);
        }

        // Convenience helper that materializes the IQueryable into a List to avoid lifetime and deferred-execution issues
        public async Task<List<Offer>> GetOffersList(Query query = null)
        {
            using var scope = scopeFactory.CreateScope();
            var ctx = scope.ServiceProvider.GetRequiredService<ITStockManagmentContext>();
            var items = ctx.Offers.AsQueryable();

            items = items.Include(i => i.Request);
            items = items.Include(i => i.Supplier);

            if (query != null)
            {
                if (!string.IsNullOrEmpty(query.Expand))
                {
                    var propertiesToExpand = query.Expand.Split(',');
                    foreach (var p in propertiesToExpand)
                    {
                        items = items.Include(p.Trim());
                    }
                }

                items = items.ApplyQuery(query);
            }

            OnOffersRead(ref items);

            return await items.ToListAsync();
        }

        partial void OnOfferGet(Offer item);
        partial void OnGetOfferById(ref IQueryable<Offer> items);


        public async Task<Offer> GetOfferById(int id)
        {
            var items = Context.Offers
                              .AsNoTracking()
                              .Where(i => i.Id == id);

            items = items.Include(i => i.Request);
            items = items.Include(i => i.Supplier);

            OnGetOfferById(ref items);

            var itemToReturn = items.FirstOrDefault();

            OnOfferGet(itemToReturn);

            return await Task.FromResult(itemToReturn);
        }

        partial void OnOfferCreated(Offer item);
        partial void OnAfterOfferCreated(Offer item);

        public async Task<Offer> CreateOffer(Offer offer)
        {
            OnOfferCreated(offer);

            var existingItem = Context.Offers
                              .Where(i => i.Id == offer.Id)
                              .FirstOrDefault();

            if (existingItem != null)
            {
                throw new Exception("Item already available");
            }

            try
            {
                Context.Offers.Add(offer);
                Context.SaveChanges();
            }
            catch
            {
                Context.Entry(offer).State = EntityState.Detached;
                throw;
            }

            OnAfterOfferCreated(offer);

            // Send email notification
            _ = Task.Run(async () =>
            {
                if (operationNotificationService != null)
                    await operationNotificationService.NotifyOfferCreated(offer);
            });

            return offer;
        }



        public async Task<Offer> UpdateOffer(int id, Offer offer)
        {


            var itemToUpdate = Context.Offers
                              .Where(i => i.Id == offer.Id)
                              .FirstOrDefault();

            if (itemToUpdate == null)
            {
                throw new Exception("Item no longer available");
            }

            var entryToUpdate = Context.Entry(itemToUpdate);
            entryToUpdate.CurrentValues.SetValues(offer);
            entryToUpdate.State = EntityState.Modified;

            Context.SaveChanges();

            // Send email notification
            _ = Task.Run(async () =>
            {
                if (operationNotificationService != null)
                    await operationNotificationService.NotifyOfferUpdated(offer);
            });

            return offer;
        }

        partial void OnOfferDeleted(Offer item);
        partial void OnAfterOfferDeleted(Offer item);

        public async Task<Offer> DeleteOffer(int id)
        {
            var itemToDelete = Context.Offers
                              .Where(i => i.Id == id)
                              .FirstOrDefault();

            if (itemToDelete == null)
            {
                throw new Exception("Item no longer available");
            }

            OnOfferDeleted(itemToDelete);


            Context.Offers.Remove(itemToDelete);

            try
            {
                Context.SaveChanges();
            }
            catch
            {
                Context.Entry(itemToDelete).State = EntityState.Unchanged;
                throw;
            }

            OnAfterOfferDeleted(itemToDelete);

            // Send email notification
            _ = Task.Run(async () =>
            {
                if (operationNotificationService != null)
                    await operationNotificationService.NotifyOfferDeleted(id);
            });

            return itemToDelete;
        }

        public async Task ExportRequestsToExcel(Query query = null, string fileName = null)
        {
            navigationManager.NavigateTo(query != null ? query.ToUrl($"export/itstockmanagment/requests/excel(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')") : $"export/itstockmanagment/requests/excel(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')", true);
        }

        public async Task ExportRequestsToCSV(Query query = null, string fileName = null)
        {
            navigationManager.NavigateTo(query != null ? query.ToUrl($"export/itstockmanagment/requests/csv(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')") : $"export/itstockmanagment/requests/csv(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')", true);
        }

        partial void OnRequestsRead(ref IQueryable<Request> items);

        public async Task<IQueryable<Request>> GetRequests(Query query = null)
        {
            IQueryable<Request> items = Context.Requests.AsQueryable();

            items = items.Include(i => i.Employee);


            if (query != null)
            {
                if (!string.IsNullOrEmpty(query.Expand))
                {
                    var propertiesToExpand = query.Expand.Split(',');
                    foreach (var p in propertiesToExpand)
                    {
                        items = items.Include(p.Trim());
                    }
                }

                items = items.ApplyQuery(query);
            }

            OnRequestsRead(ref items);

            return await Task.FromResult(items);
        }

        // Convenience helper that materializes the IQueryable into a List to avoid lifetime and deferred-execution issues
        public async Task<List<Request>> GetRequestsList(Query query = null)
        {
            using var scope = scopeFactory.CreateScope();
            var ctx = scope.ServiceProvider.GetRequiredService<ITStockManagmentContext>();
            IQueryable<Request> items = ctx.Requests.AsQueryable();

            items = items.Include(i => i.Employee);

            if (query != null)
            {
                if (!string.IsNullOrEmpty(query.Expand))
                {
                    var propertiesToExpand = query.Expand.Split(',');
                    foreach (var p in propertiesToExpand)
                    {
                        items = items.Include(p.Trim());
                    }
                }

                items = items.ApplyQuery(query);
            }

            OnRequestsRead(ref items);

            return await items.ToListAsync();
        }

        partial void OnRequestGet(Request item);
        partial void OnGetRequestById(ref IQueryable<Request> items);


        public async Task<Request> GetRequestById(int id)
        {
            IQueryable<Request> items = Context.Requests
                              .AsNoTracking()
                              .Where(i => i.Id == id);

            items = items.Include(i => i.Employee);

            OnGetRequestById(ref items);

            var itemToReturn = items.FirstOrDefault();

            OnRequestGet(itemToReturn);

            return await Task.FromResult(itemToReturn);
        }

        partial void OnRequestCreated(Request item);
        partial void OnAfterRequestCreated(Request item);

        public async Task<Request> CreateRequest(Request request)
        {
            OnRequestCreated(request);

            var existingItem = Context.Requests
                              .Where(i => i.Id == request.Id)
                              .FirstOrDefault();

            if (existingItem != null)
            {
                throw new Exception("Item already available");
            }

            // If File isn't provided, use an empty byte array so DB insert won't fail on a non-null column
            request.File ??= Array.Empty<byte>();

            try
            {
                Context.Requests.Add(request);
                Context.SaveChanges();
            }
            catch
            {
                Context.Entry(request).State = EntityState.Detached;
                throw;
            }

            OnAfterRequestCreated(request);

            // Send email notification
            _ = Task.Run(async () =>
            {
                if (operationNotificationService != null)
                    await operationNotificationService.NotifyRequestCreated(request);
            });

            return request;
        }

        partial void OnRequestUpdated(Request item);
        partial void OnAfterRequestUpdated(Request item);

        public async Task<Request> UpdateRequest(int id, Request request)
        {
            OnRequestUpdated(request);

            var itemToUpdate = Context.Requests
                              .Where(i => i.Id == request.Id)
                              .FirstOrDefault();

            if (itemToUpdate == null)
            {
                throw new Exception("Item no longer available");
            }

            var entryToUpdate = Context.Entry(itemToUpdate);
            var existingFile = itemToUpdate.File;
            entryToUpdate.CurrentValues.SetValues(request);
            // Preserve existing file if the incoming request does not provide a new file
            if (request.File == null)
            {
                entryToUpdate.Property("File").CurrentValue = existingFile;
                entryToUpdate.Property("File").IsModified = false;
            }
            entryToUpdate.State = EntityState.Modified;

            Context.SaveChanges();

            OnAfterRequestUpdated(request);

            // Send email notification
            _ = Task.Run(async () =>
            {
                if (operationNotificationService != null)
                    await operationNotificationService.NotifyRequestUpdated(request);
            });

            return request;
        }

        partial void OnRequestDeleted(Request item);
        partial void OnAfterRequestDeleted(Request item);

        public async Task<Request> DeleteRequest(int id)
        {
            var itemToDelete = Context.Requests
                              .Where(i => i.Id == id)
                              .Include(i => i.Offers)
                              .FirstOrDefault();

            if (itemToDelete == null)
            {
                throw new Exception("Item no longer available");
            }

            OnRequestDeleted(itemToDelete);


            Context.Requests.Remove(itemToDelete);

            try
            {
                Context.SaveChanges();
            }
            catch
            {
                Context.Entry(itemToDelete).State = EntityState.Unchanged;
                throw;
            }

            OnAfterRequestDeleted(itemToDelete);

            // Send email notification
            _ = Task.Run(async () =>
            {
                if (operationNotificationService != null)
                    await operationNotificationService.NotifyRequestDeleted(id);
            });

            return itemToDelete;
        }

        public async Task ExportSuppliersToExcel(Query query = null, string fileName = null)
        {
            navigationManager.NavigateTo(query != null ? query.ToUrl($"export/itstockmanagment/suppliers/excel(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')") : $"export/itstockmanagment/suppliers/excel(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')", true);
        }

        public async Task ExportSuppliersToCSV(Query query = null, string fileName = null)
        {
            navigationManager.NavigateTo(query != null ? query.ToUrl($"export/itstockmanagment/suppliers/csv(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')") : $"export/itstockmanagment/suppliers/csv(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')", true);
        }

        partial void OnSuppliersRead(ref IQueryable<Supplier> items);

        public async Task<IQueryable<Supplier>> GetSuppliers(Query query = null)
        {
            var items = Context.Suppliers.AsQueryable();


            if (query != null)
            {
                if (!string.IsNullOrEmpty(query.Expand))
                {
                    var propertiesToExpand = query.Expand.Split(',');
                    foreach (var p in propertiesToExpand)
                    {
                        items = items.Include(p.Trim());
                    }
                }

                items = items.ApplyQuery(query);
            }

            OnSuppliersRead(ref items);

            return await Task.FromResult(items);
        }

        partial void OnSupplierGet(Supplier item);
        partial void OnGetSupplierBySupplierName(ref IQueryable<Supplier> items);


        public async Task<Supplier> GetSupplierBySupplierName(string suppliername)
        {
            var items = Context.Suppliers
                              .AsNoTracking()
                              .Where(i => i.SupplierName == suppliername);


            OnGetSupplierBySupplierName(ref items);

            var itemToReturn = items.FirstOrDefault();

            OnSupplierGet(itemToReturn);

            return await Task.FromResult(itemToReturn);
        }

        partial void OnSupplierCreated(Supplier item);
        partial void OnAfterSupplierCreated(Supplier item);

        public async Task<Supplier> CreateSupplier(Supplier supplier)
        {
            OnSupplierCreated(supplier);

            var existingItem = Context.Suppliers
                              .Where(i => i.SupplierName == supplier.SupplierName)
                              .FirstOrDefault();

            if (existingItem != null)
            {
                throw new Exception("Item already available");
            }

            try
            {
                Context.Suppliers.Add(supplier);
                Context.SaveChanges();
            }
            catch
            {
                Context.Entry(supplier).State = EntityState.Detached;
                throw;
            }

            OnAfterSupplierCreated(supplier);

            // Send email notification
            _ = Task.Run(async () =>
            {
                if (operationNotificationService != null)
                    await operationNotificationService.NotifySupplierCreated(supplier);
            });

            return supplier;
        }

        partial void OnSupplierUpdated(Supplier item);
        partial void OnAfterSupplierUpdated(Supplier item);


        //Addded by ME
        public async Task<IEnumerable<Offer>> GetOffersByIds(List<int> ids)
        {
            var items = Context.Offers
                            .AsNoTracking()
                            .Where(i => ids.Contains(i.Id))
                            .Include(i => i.Request)
                            .Include(i => i.Supplier);

            return await Task.FromResult(items.ToList());
        }

        public async Task<Supplier> UpdateSupplier(string suppliername, Supplier supplier)
        {
            OnSupplierUpdated(supplier);

            var itemToUpdate = Context.Suppliers
                              .Where(i => i.SupplierName == supplier.SupplierName)
                              .FirstOrDefault();

            if (itemToUpdate == null)
            {
                throw new Exception("Item no longer available");
            }

            var entryToUpdate = Context.Entry(itemToUpdate);
            entryToUpdate.CurrentValues.SetValues(supplier);
            entryToUpdate.State = EntityState.Modified;

            Context.SaveChanges();

            OnAfterSupplierUpdated(supplier);

            // Send email notification
            _ = Task.Run(async () =>
            {
                if (operationNotificationService != null)
                    await operationNotificationService.NotifySupplierUpdated(supplier);
            });

            return supplier;
        }

        partial void OnSupplierDeleted(Supplier item);
        partial void OnAfterSupplierDeleted(Supplier item);

        public async Task<Supplier> DeleteSupplier(string suppliername)
        {
            var itemToDelete = Context.Suppliers
                              .Where(i => i.SupplierName == suppliername)
                              .Include(i => i.DeliveryOrders)
                              .Include(i => i.Offers)
                              .FirstOrDefault();

            if (itemToDelete == null)
            {
                throw new Exception("Item no longer available");
            }

            OnSupplierDeleted(itemToDelete);


            Context.Suppliers.Remove(itemToDelete);

            try
            {
                Context.SaveChanges();
            }
            catch
            {
                Context.Entry(itemToDelete).State = EntityState.Unchanged;
                throw;
            }

            OnAfterSupplierDeleted(itemToDelete);

            // Send email notification
            _ = Task.Run(async () =>
            {
                if (operationNotificationService != null)
                    await operationNotificationService.NotifySupplierDeleted(suppliername);
            });

            return itemToDelete;
        }

        public async Task ExportProjectsToExcel(Query query = null, string fileName = null)
        {
            navigationManager.NavigateTo(query != null ? query.ToUrl($"export/itstockmanagment/projects/excel(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')") : $"export/itstockmanagment/projects/excel(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')", true);
        }

        public async Task ExportProjectsToCSV(Query query = null, string fileName = null)
        {
            navigationManager.NavigateTo(query != null ? query.ToUrl($"export/itstockmanagment/projects/csv(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')") : $"export/itstockmanagment/projects/csv(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')", true);
        }

        partial void OnProjectsRead(ref IQueryable<Project> items);

        public async Task<IQueryable<Project>> GetProjects(Query query = null)
        {
            var items = Context.Projects.AsQueryable();


            if (query != null)
            {
                if (!string.IsNullOrEmpty(query.Expand))
                {
                    var propertiesToExpand = query.Expand.Split(',');
                    foreach (var p in propertiesToExpand)
                    {
                        items = items.Include(p.Trim());
                    }
                }

                items = items.ApplyQuery(query);
            }

            OnProjectsRead(ref items);

            return await Task.FromResult(items);
        }

        // Convenience helper that materializes the IQueryable into a List to avoid lifetime and deferred-execution issues
        public async Task<List<Project>> GetProjectsList(Query query = null)
        {
            using var scope = scopeFactory.CreateScope();
            var ctx = scope.ServiceProvider.GetRequiredService<ITStockManagmentContext>();
            var items = ctx.Projects.AsQueryable();


            if (query != null)
            {
                if (!string.IsNullOrEmpty(query.Expand))
                {
                    var propertiesToExpand = query.Expand.Split(',');
                    foreach (var p in propertiesToExpand)
                    {
                        items = items.Include(p.Trim());
                    }
                }

                items = items.ApplyQuery(query);
            }

            OnProjectsRead(ref items);

            return await items.ToListAsync();
        }
        public async Task<DeliveryOrder> DeleteDeliveryOrder(string deleveryordernumber)
        {
            var itemToDelete = Context.DeliveryOrders
                              .Where(i => i.DeleveryOrderNumber == deleveryordernumber)
                              .Include(i => i.DeliveryOrderMateriels)

                              .FirstOrDefault();

            if (itemToDelete == null)
            {
                throw new Exception("Item no longer available");
            }

            OnDeliveryOrderDeleted(itemToDelete);


            Context.DeliveryOrders.Remove(itemToDelete);

            try
            {
                Context.SaveChanges();
            }
            catch
            {
                Context.Entry(itemToDelete).State = EntityState.Unchanged;
                throw;
            }

            OnAfterDeliveryOrderDeleted(itemToDelete);

            // Send email notification
            _ = Task.Run(async () =>
            {
                if (operationNotificationService != null)
                    await operationNotificationService.NotifyDeliveryOrderDeleted(deleveryordernumber);
            });

            return itemToDelete;
        }

        public async Task ExportDeliveryOrderMaterielsToExcel(Query query = null, string fileName = null)
        {
            navigationManager.NavigateTo(query != null ? query.ToUrl($"export/itstockmanagment/deliveryordermateriels/excel(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')") : $"export/itstockmanagment/deliveryordermateriels/excel(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')", true);
        }

        public async Task ExportDeliveryOrderMaterielsToCSV(Query query = null, string fileName = null)
        {
            navigationManager.NavigateTo(query != null ? query.ToUrl($"export/itstockmanagment/deliveryordermateriels/csv(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')") : $"export/itstockmanagment/deliveryordermateriels/csv(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')", true);
        }

        partial void OnDeliveryOrderMaterielsRead(ref IQueryable<DeliveryOrderMateriel> items);

        public async Task<IQueryable<DeliveryOrderMateriel>> GetDeliveryOrderMateriels(Query query = null)
        {
            var items = Context.DeliveryOrderMateriels.AsQueryable();

            items = items.Include(i => i.DeliveryOrder).ThenInclude(i => i.Employee);

            items = items.Include(i => i.Materiel);

            if (query != null)
            {
                if (!string.IsNullOrEmpty(query.Expand))
                {
                    var propertiesToExpand = query.Expand.Split(',');
                    foreach (var p in propertiesToExpand)
                    {
                        items = items.Include(p.Trim());
                    }
                }

                items = items.ApplyQuery(query);
            }

            OnDeliveryOrderMaterielsRead(ref items);

            return await Task.FromResult(items);
        }

        partial void OnDeliveryOrderMaterielGet(DeliveryOrderMateriel item);
        partial void OnGetDeliveryOrderMaterielByMaterielIdAndDeliveryOrderNumber(ref IQueryable<DeliveryOrderMateriel> items);


        public async Task<DeliveryOrderMateriel> GetDeliveryOrderMaterielByMaterielIdAndDeliveryOrderNumber(int materielid, string deliveryordernumber)
        {
            var items = Context.DeliveryOrderMateriels
                              .AsNoTracking()
                              .Where(i => i.MaterielId == materielid && i.DeliveryOrderNumber == deliveryordernumber);

            items = items.Include(i => i.DeliveryOrder);
            items = items.Include(i => i.Materiel);

            OnGetDeliveryOrderMaterielByMaterielIdAndDeliveryOrderNumber(ref items);

            var itemToReturn = items.FirstOrDefault();

            OnDeliveryOrderMaterielGet(itemToReturn);

            return await Task.FromResult(itemToReturn);
        }

        partial void OnDeliveryOrderMaterielCreated(DeliveryOrderMateriel item);
        partial void OnAfterDeliveryOrderMaterielCreated(DeliveryOrderMateriel item);

        public async Task<DeliveryOrderMateriel> CreateDeliveryOrderMateriel(DeliveryOrderMateriel deliveryordermateriel)
        {
            OnDeliveryOrderMaterielCreated(deliveryordermateriel);

            var existingItem = Context.DeliveryOrderMateriels
                              .Where(i => i.MaterielId == deliveryordermateriel.MaterielId && i.DeliveryOrderNumber == deliveryordermateriel.DeliveryOrderNumber)
                              .FirstOrDefault();

            if (existingItem != null)
            {
                throw new Exception("DelieryOrderMaterial Item already available");
            }

            try
            {
                Context.DeliveryOrderMateriels.Add(deliveryordermateriel);
                Context.SaveChanges();
            }
            catch
            {
                Context.Entry(deliveryordermateriel).State = EntityState.Detached;
                throw;
            }

            OnAfterDeliveryOrderMaterielCreated(deliveryordermateriel);

            return deliveryordermateriel;
        }

        partial void OnDeliveryOrderMaterielUpdated(DeliveryOrderMateriel item);
        partial void OnAfterDeliveryOrderMaterielUpdated(DeliveryOrderMateriel item);

        public async Task<DeliveryOrderMateriel> UpdateDeliveryOrderMateriel(int materielid, string deliveryordernumber, DeliveryOrderMateriel deliveryordermateriel)
        {
            OnDeliveryOrderMaterielUpdated(deliveryordermateriel);

            var itemToUpdate = Context.DeliveryOrderMateriels
                              .Where(i => i.MaterielId == deliveryordermateriel.MaterielId && i.DeliveryOrderNumber == deliveryordermateriel.DeliveryOrderNumber)
                              .FirstOrDefault();

            if (itemToUpdate == null)
            {
                throw new Exception("Item no longer available");
            }

            var entryToUpdate = Context.Entry(itemToUpdate);
            entryToUpdate.CurrentValues.SetValues(deliveryordermateriel);
            entryToUpdate.State = EntityState.Modified;

            Context.SaveChanges();

            OnAfterDeliveryOrderMaterielUpdated(deliveryordermateriel);

            return deliveryordermateriel;
        }

        partial void OnDeliveryOrderMaterielDeleted(DeliveryOrderMateriel item);
        partial void OnAfterDeliveryOrderMaterielDeleted(DeliveryOrderMateriel item);

        public async Task<DeliveryOrderMateriel> DeleteDeliveryOrderMateriel(int materielid, string deliveryordernumber)
        {
            var itemToDelete = Context.DeliveryOrderMateriels
                              .Where(i => i.MaterielId == materielid && i.DeliveryOrderNumber == deliveryordernumber)
                              .FirstOrDefault();

            if (itemToDelete == null)
            {
                throw new Exception("Item no longer available");
            }

            OnDeliveryOrderMaterielDeleted(itemToDelete);


            Context.DeliveryOrderMateriels.Remove(itemToDelete);

            try
            {
                Context.SaveChanges();
            }
            catch
            {
                Context.Entry(itemToDelete).State = EntityState.Unchanged;
                throw;
            }

            OnAfterDeliveryOrderMaterielDeleted(itemToDelete);

            return itemToDelete;
        }

        public async Task<Materiel> DeleteMateriel(int id)
        {
            var itemToDelete = Context.Materiels
                              .Where(i => i.Id == id)
                              .Include(i => i.AssignmentMateriels)
                              .Include(i => i.DeliveryOrderMateriels)
                              .FirstOrDefault();

            if (itemToDelete == null)
            {
                throw new Exception("Item no longer available");
            }

            OnMaterielDeleted(itemToDelete);


            Context.Materiels.Remove(itemToDelete);

            try
            {
                Context.SaveChanges();
            }
            catch
            {
                Context.Entry(itemToDelete).State = EntityState.Unchanged;
                throw;
            }

            OnAfterMaterielDeleted(itemToDelete);

            return itemToDelete;
        }



        partial void OnProjectGet(Project item);
        partial void OnGetProjectById(ref IQueryable<Project> items);


        public async Task<Project> GetProjectById(int id)
        {
            var items = Context.Projects
                              .AsNoTracking()
                              .Where(i => i.Id == id);


            OnGetProjectById(ref items);

            var itemToReturn = items.FirstOrDefault();

            OnProjectGet(itemToReturn);

            return await Task.FromResult(itemToReturn);
        }

        partial void OnProjectCreated(Project item);
        partial void OnAfterProjectCreated(Project item);

        public async Task<Project> CreateProject(Project project)
        {
            OnProjectCreated(project);

            var existingItem = Context.Projects
                              .Where(i => i.Id == project.Id)
                              .FirstOrDefault();

            if (existingItem != null)
            {
                throw new Exception("Item already available");
            }

            try
            {
                Context.Projects.Add(project);
                Context.SaveChanges();
            }
            catch
            {
                Context.Entry(project).State = EntityState.Detached;
                throw;
            }

            OnAfterProjectCreated(project);

            return project;
        }

        partial void OnProjectUpdated(Project item);
        partial void OnAfterProjectUpdated(Project item);

        public async Task<Project> UpdateProject(int id, Project project)
        {
            OnProjectUpdated(project);

            var itemToUpdate = Context.Projects
                              .Where(i => i.Id == project.Id)
                              .FirstOrDefault();

            if (itemToUpdate == null)
            {
                throw new Exception("Item no longer available");
            }

            var entryToUpdate = Context.Entry(itemToUpdate);
            entryToUpdate.CurrentValues.SetValues(project);
            entryToUpdate.State = EntityState.Modified;

            Context.SaveChanges();

            OnAfterProjectUpdated(project);

            return project;
        }

        partial void OnProjectDeleted(Project item);
        partial void OnAfterProjectDeleted(Project item);

        public async Task<Project> DeleteProject(int id)
        {
            var itemToDelete = Context.Projects
                              .Where(i => i.Id == id)
                              .Include(i => i.Assignments)
                              .FirstOrDefault();

            if (itemToDelete == null)
            {
                throw new Exception("Item no longer available");
            }

            OnProjectDeleted(itemToDelete);


            Context.Projects.Remove(itemToDelete);

            try
            {
                Context.SaveChanges();
            }
            catch
            {
                Context.Entry(itemToDelete).State = EntityState.Unchanged;
                throw;
            }

            OnAfterProjectDeleted(itemToDelete);

            return itemToDelete;
        }






        //IT view excel
        public async Task ExportMaterielsITToExcel(Query query = null, string fileName = null)
        {
            navigationManager.NavigateTo(query != null ? query.ToUrl($"export/itstockmanagment/materielsIT/excel(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')") : $"export/itstockmanagment/materielsIT/excel(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')", true);
        }
        public async Task ExportMaterielsITToCSV(Query query = null, string fileName = null)
        {
            navigationManager.NavigateTo(query != null ? query.ToUrl($"export/itstockmanagment/materielsIT/csv(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')") : $"export/itstockmanagment/materielsIT/csv(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')", true);

        }


        //pdr view excel
        public async Task ExportMaterielsPDRToExcel(Query query = null, string fileName = null)
        {
            navigationManager.NavigateTo(query != null ? query.ToUrl($"export/itstockmanagment/materielsPDR/excel(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')") : $"export/itstockmanagment/materielsPDR/excel(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')", true);
        }
        public async Task ExportMaterielsPDRToCSV(Query query = null, string fileName = null)
        {
            navigationManager.NavigateTo(query != null ? query.ToUrl($"export/itstockmanagment/materielsPDR/csv(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')") : $"export/itstockmanagment/materielsPDR/csv(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')", true);

        }

        //archived requests excel
        public async Task ExportArchivedRequestsToExcel(Query query = null, string fileName = null)
        {
            navigationManager.NavigateTo(query != null ? query.ToUrl($"export/itstockmanagment/archived-requests/excel(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')") : $"export/itstockmanagment/archived-requests/excel(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')", true);
        }
        public async Task ExportArchivedRequestsToCSV(Query query = null, string fileName = null)
        {
            navigationManager.NavigateTo(query != null ? query.ToUrl($"export/itstockmanagment/archived-requests/csv(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')") : $"export/itstockmanagment/archived-requests/csv(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')", true);

        }




    }
}
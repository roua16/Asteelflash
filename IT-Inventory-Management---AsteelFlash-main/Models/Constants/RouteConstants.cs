namespace ITStockM.Models.Constants
{
    /// <summary>
    /// Defines route constants for the application
    /// </summary>
    public static class RouteConstants
    {
        // Authentication
        public const string Login = "/login";
        public const string Logout = "/logout";
        public const string AccessDenied = "/access-denied";

        // Dashboard
        public const string Dashboard = "/";
        
        // Materials Management
        public const string MaterialsIT = "/materials/it-stock";
        public const string MaterialsPDR = "/materials/pdr-stock";
        public const string MaterialsManagement = "/admin/materials";

        // Assignments
        public const string Assignments = "/assignments";
        public const string AssignmentsArchive = "/assignments/archive";
        public const string AssignmentsMissions = "/assignments/missions";
        public const string AssignmentsMissionsArchive = "/assignments/missions/archive";
        public const string AssignmentsManagement = "/admin/assignments";

        // Requests
        public const string Requests = "/requests";
        public const string RequestsArchived = "/requests/archived";
        public const string RequestsManagement = "/admin/requests";

        // Infrastructure
        public const string InfraInterface = "/infra";
        public const string AvailableOffers = "/infra/offers";

        // Purchasing
        public const string PurchasePage = "/purchasing";
        public const string Suppliers = "/purchasing/suppliers";
        public const string SuppliersManagement = "/admin/suppliers";

        // Delivery Orders
        public const string DeliveryOrders = "/delivery-orders";
        public const string DeliveryOrderHistory = "/delivery-orders/history";
        public const string PendingDeliveries = "/delivery-orders/pending";
        public const string DeliveryOrdersManagement = "/admin/delivery-orders";

        // Administration
        public const string EmployeesManagement = "/admin/employees";
        public const string ProjectsManagement = "/admin/projects";
        public const string OffersManagement = "/admin/offers";
    }
}

using ITStockM.Domain.Base;

namespace ITStockM.Domain.Events
{
    /// <summary>
    /// Raised when an asset is assigned to an employee.
    /// Used for tracking asset custody and notifications.
    /// </summary>
    public class AssetAssignedEvent : DomainEvent
    {
        public int MaterielId { get; }
        public int AssignmentId { get; }
        public int AssignedToEmployeeId { get; }
        public int AssignedByEmployeeId { get; }
        public DateTime AssignmentDate { get; }

        public AssetAssignedEvent(
            int materielId,
            int assignmentId,
            int assignedToEmployeeId,
            int assignedByEmployeeId)
        {
            MaterielId = materielId;
            AssignmentId = assignmentId;
            AssignedToEmployeeId = assignedToEmployeeId;
            AssignedByEmployeeId = assignedByEmployeeId;
            AssignmentDate = DateTime.UtcNow;
        }
    }
}

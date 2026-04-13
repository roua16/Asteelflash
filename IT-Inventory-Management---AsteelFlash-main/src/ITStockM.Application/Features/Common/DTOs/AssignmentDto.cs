using System;

namespace ITStockM.Application.Features.Common.DTOs
{
    public class AssignmentDto
    {
        public int Id { get; set; }
        public int? AssignedTo { get; set; }
        public int AssignedBy { get; set; }
        public int? ProjectId { get; set; }
        public DateTime Date { get; set; }
        public string Descipriton { get; set; }
        public bool OnMission { get; set; }
        public DateTime? RestoreDateLimit { get; set; }
        public DateTime? RestoreDate { get; set; }
    }
}
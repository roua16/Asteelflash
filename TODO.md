# TODO.md - Fix PDR Stock Operation EF Tracking Error

## Completed: 0/5

### 1. Create TODO.md [✅ COMPLETED]

### 2. Implement AsNoTracking in MaterielService.GetMateriels() [✅ COMPLETED]
- Edit \`src/ITStockM.Infrastructure/Services/MaterielService.cs\`
- Override \`GetAll()\` to force \`.AsNoTracking()\` on queries for UI grids

### 3. Refactor BaseCrudService.Update() to merge pattern [✅ COMPLETED]
- Edit \`src/ITStockM.Infrastructure/Services/BaseCrudService.cs\`
- Change Update: load existing → copy properties from DTO → Update(existing)

### 4. Enable EF sensitive logging in appsettings [✅ COMPLETED]
- Edit \`src/ITStockM.WebApi/appsettings.Development.json\`
- Added EF Core detailed logging for debugging

### 5. Test & Verify
- Navigate to \`/materials-view-interface-pdr\`
- Perform all PDR operations (ReceiveToPdr, DistributeToIt, SendToRepair, MarkIrreparable, RepairToPdr)
- Confirm no tracking errors
- Verify data updates correctly
- Test grid reloads properly
- Mark complete & attempt_completion


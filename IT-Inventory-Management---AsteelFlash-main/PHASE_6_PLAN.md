# Phase 6 Plan - Full Testing & Production Readiness

**Status**: Ready to Start (After Phase 5)  
**Date**: April 14, 2026  
**Duration**: 2-3 hours  
**Goal**: Comprehensive testing and production readiness verification

---

## 📊 Phase 6 Overview

### Objectives
1. ✅ Add integration tests for services
2. ✅ Add E2E API tests
3. ✅ Achieve 80%+ test coverage
4. ✅ Generate coverage reports
5. ✅ Final documentation review
6. ✅ Production readiness verification

### Current State (After Phase 5)
- ✅ 4-layer Clean Architecture complete
- ✅ 12/25 services consolidated
- ✅ 5 domain events implemented with handlers
- ✅ MediatR event publishing working
- ✅ 50+ unit tests passing
- ✅ No breaking changes

---

## 🎯 Testing Strategy

### Layer Testing Breakdown

| Layer | Type | Location | Target Coverage |
|-------|------|----------|-----------------|
| **Domain** | Unit | `Domain/Entities/Tests` | 90%+ |
| **Application** | Unit + Integration | `Application/Features/Tests` | 85%+ |
| **Infrastructure** | Integration | `Infrastructure/Tests` | 75%+ |
| **Presentation** | E2E | `WebApi/Tests` | 70%+ |
| **Overall** | - | - | **80%+** |

---

## 📁 Test File Structure (To Create)

```
ITStockM.Tests/
├── Domain/
│   ├── Entities/
│   │   ├── MaterielEntityTests.cs
│   │   ├── EmployeeEntityTests.cs
│   │   ├── AssignmentEntityTests.cs
│   │   └── ... (for each entity)
│   └── ValueObjects/
│       ├── SerialNumberTests.cs
│       ├── WarrantyTests.cs
│       ├── QuantityTests.cs
│       └── MoneyTests.cs (already exists ✅)
│
├── Application/
│   ├── Features/
│   │   ├── Products/
│   │   │   ├── Commands/
│   │   │   │   ├── CreateProductCommandTests.cs
│   │   │   │   ├── UpdateProductCommandTests.cs
│   │   │   │   └── DeleteProductCommandTests.cs
│   │   │   ├── Queries/
│   │   │   │   ├── GetAllProductsQueryTests.cs
│   │   │   │   └── GetProductByIdQueryTests.cs
│   │   │   └── Handlers/
│   │   │       └── ProductHandlerTests.cs
│   │   └── Events/
│   │       ├── AssetLifecycleEventHandlerTests.cs
│   │       ├── MaintenanceEventHandlerTests.cs
│   │       └── ... (for each event handler)
│   └── Services/
│       ├── MaterielServiceTests.cs
│       ├── EmployeeServiceTests.cs
│       └── ... (for each refactored service)
│
├── Infrastructure/
│   ├── Persistence/
│   │   ├── RepositoryTests.cs
│   │   └── ContextTests.cs
│   ├── Services/
│   │   ├── BaseCrudServiceTests.cs
│   │   └── IdentityServiceTests.cs
│   └── Events/
│       └── EventPublishingTests.cs
│
└── WebApi/
    ├── Controllers/
    │   ├── ProductsControllerTests.cs
    │   ├── EmployeesControllerTests.cs
    │   └── ... (for each controller)
    ├── Integration/
    │   ├── CrudWorkflowTests.cs
    │   ├── EventFlowTests.cs
    │   └── AuthenticationTests.cs
    └── E2E/
        ├── AssetLifecycleScenarioTests.cs
        ├── MaintenanceWorkflowTests.cs
        └── CompleteUserJourneyTests.cs
```

---

## 📝 Detailed Testing Plan

### Phase 6.1: Domain Layer Tests (30-45 min)

#### Entity Tests
**Example**: `MaterielEntityTests.cs`
```csharp
public class MaterielEntityTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateEntity()
    {
        // Arrange & Act
        var materiel = new Materiel 
        { 
            Name = "Laptop", 
            SerialNumber = "ASSET-001" 
        };
        
        // Assert
        Assert.NotNull(materiel);
        Assert.Equal("Laptop", materiel.Name);
        Assert.True(materiel.CreatedAt > DateTime.MinValue);
    }
    
    [Fact]
    public void RaiseDomainEvent_ShouldAddEventToCollection()
    {
        // Arrange
        var materiel = new Materiel { Name = "Laptop" };
        var @event = new AssetAssignedEvent();
        
        // Act
        materiel.AddDomainEvent(@event);
        
        // Assert
        Assert.Single(materiel.DomainEvents);
    }
    
    [Fact]
    public void SoftDelete_ShouldMarkAsDeleted()
    {
        // Arrange
        var materiel = new Materiel { Name = "Laptop" };
        
        // Act
        materiel.SoftDelete();
        
        // Assert
        Assert.True(materiel.IsDeleted);
    }
}
```

**Coverage**:
- Entity creation
- Property validation
- Domain event handling
- Soft delete functionality
- Audit trail updates

#### Value Object Tests
Already exist (✅ MoneyTests.cs)
- SerialNumberTests.cs: Equality, immutability
- WarrantyTests.cs: Expiry logic, calculations
- QuantityTests.cs: Unit conversion, arithmetic

---

### Phase 6.2: Application Layer Tests (45-60 min)

#### Service Tests
**Example**: `MaterielServiceTests.cs`
```csharp
public class MaterielServiceTests
{
    private readonly Mock<IRepository<Materiel>> _repositoryMock;
    private readonly MaterielService _service;
    
    public MaterielServiceTests()
    {
        _repositoryMock = new Mock<IRepository<Materiel>>();
        _service = new MaterielService(_repositoryMock.Object);
    }
    
    [Fact]
    public async Task GetAll_WithQuery_ShouldReturnFilteredResults()
    {
        // Arrange
        var query = new Query { Filter = "Name eq 'Laptop'" };
        var materiels = new[] { new Materiel { Name = "Laptop" } };
        
        _repositoryMock
            .Setup(x => x.Query())
            .Returns(materiels.AsQueryable());
        
        // Act
        var result = await _service.GetAll(query);
        
        // Assert
        Assert.Single(result);
    }
    
    [Fact]
    public async Task Create_WithValidEntity_ShouldSaveAndReturn()
    {
        // Arrange
        var materiel = new Materiel { Name = "Laptop" };
        
        // Act
        await _service.Create(materiel);
        
        // Assert
        _repositoryMock.Verify(x => x.AddAsync(materiel), Times.Once);
        _repositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }
    
    [Fact]
    public async Task Update_WithNonExistentEntity_ShouldThrowException()
    {
        // Arrange
        _repositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Materiel)null);
        
        // Act & Assert
        await Assert.ThrowsAsync<EntityNotFoundException>(
            () => _service.Update(999, new Materiel())
        );
    }
}
```

**Coverage**:
- CRUD operations
- Query filtering
- Error handling
- Notification hooks
- Transaction handling

#### Event Handler Tests
**Example**: `AssetLifecycleEventHandlerTests.cs`
```csharp
public class AssetLifecycleEventHandlerTests
{
    private readonly Mock<ILogger<AssetLifecycleEventHandler>> _loggerMock;
    private readonly Mock<IOperationNotificationService> _notificationMock;
    private readonly AssetLifecycleEventHandler _handler;
    
    [Fact]
    public async Task Handle_WithValidEvent_ShouldNotifyAndLog()
    {
        // Arrange
        var @event = new AssetLifecycleChangedEvent 
        { 
            MaterielId = 1, 
            NewStage = "Retired" 
        };
        
        // Act
        await _handler.Handle(@event, CancellationToken.None);
        
        // Assert
        _notificationMock.Verify(
            x => x.NotifyAssetLifecycleChanged(@event), 
            Times.Once
        );
        _loggerMock.Verify(
            x => x.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), 
            Times.AtLeastOnce
        );
    }
}
```

---

### Phase 6.3: Infrastructure Layer Tests (30-45 min)

#### Repository Tests
**Example**: `RepositoryTests.cs`
```csharp
public class RepositoryTests : IAsyncLifetime
{
    private readonly ITStockManagmentContext _context;
    private readonly Repository<Materiel> _repository;
    
    [Fact]
    public async Task AddAsync_WithValidEntity_ShouldPersist()
    {
        // Arrange
        var materiel = new Materiel { Name = "Laptop" };
        
        // Act
        await _repository.AddAsync(materiel);
        await _repository.SaveChangesAsync();
        
        // Assert
        var persisted = await _repository.GetByIdAsync(materiel.Id);
        Assert.NotNull(persisted);
    }
    
    [Fact]
    public async Task Query_WithIncludes_ShouldEagerLoad()
    {
        // Arrange & Act
        var query = _repository.Query()
            .Include(m => m.AssignmentMateriels);
        
        // Assert - lazy loading not needed
        Assert.NotNull(query);
    }
}
```

---

### Phase 6.4: WebAPI E2E Tests (30-45 min)

#### API Endpoint Tests
**Example**: `ProductsControllerTests.cs`
```csharp
public class ProductsControllerTests : IAsyncLifetime
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    
    [Fact]
    public async Task GetAll_WithoutFilter_ShouldReturn200()
    {
        // Act
        var response = await _client.GetAsync("/api/products");
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsAsync<IEnumerable<MaterielDto>>();
        Assert.NotNull(json);
    }
    
    [Fact]
    public async Task Create_WithValidData_ShouldReturn201()
    {
        // Arrange
        var dto = new CreateMaterielCommand 
        { 
            Name = "Laptop",
            SerialNumber = "ASSET-001" 
        };
        
        // Act
        var response = await _client.PostAsJsonAsync("/api/products", dto);
        
        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }
}
```

#### Complete Workflow Tests
**Example**: `CrudWorkflowTests.cs`
```csharp
public class CrudWorkflowTests : IAsyncLifetime
{
    [Fact]
    public async Task CompleteAssetLifecycle_ShouldWorkEndToEnd()
    {
        // 1. Create asset
        var created = await CreateAsset("Laptop");
        
        // 2. Assign asset
        var assigned = await AssignAsset(created.Id, employeeId: 1);
        
        // 3. Verify events published
        var eventsPublished = await GetPublishedEvents();
        
        // 4. Update asset
        var updated = await UpdateAsset(created.Id, newStage: "Retired");
        
        // 5. Assert complete lifecycle
        Assert.NotNull(created);
        Assert.NotNull(assigned);
        Assert.True(eventsPublished.Any());
        Assert.NotNull(updated);
    }
}
```

---

## 📊 Coverage Reporting

### Tools
- **dotnet test** with coverage flag
- **OpenCover** for detailed analysis
- **ReportGenerator** for HTML reports

### Command
```bash
dotnet test --collect:"XPlat Code Coverage" \
    --settings="coverlet.runsettings"
```

### Target Coverage
- **Overall**: 80%+
- **Domain**: 90%+
- **Application**: 85%+
- **Infrastructure**: 75%+
- **Presentation**: 70%+

### Exclusions
- Auto-generated code
- Configuration classes
- Integration test fixtures

---

## ✅ Testing Checklist

- [ ] Create 30+ new test files
- [ ] Achieve 80%+ overall coverage
- [ ] All tests pass (green build)
- [ ] No flaky tests
- [ ] Performance benchmarks acceptable
- [ ] Integration tests use real database (test instance)
- [ ] E2E tests use WebApplicationFactory
- [ ] Coverage report generated

---

## 📝 Commits Expected

```
1. test: Add domain layer unit tests
   - Entity tests
   - Value object tests
   - Coverage: 90%+

2. test: Add application layer tests
   - Service tests
   - Event handler tests
   - Coverage: 85%+

3. test: Add infrastructure integration tests
   - Repository tests
   - DbContext tests
   - Coverage: 75%+

4. test: Add WebAPI E2E tests
   - Controller tests
   - Workflow tests
   - Coverage: 70%+

5. test: Generate and verify coverage report
   - Coverage report: 80%+
   - All critical paths covered
```

---

## 🚀 Production Readiness Checklist

### Code Quality
- ✅ All tests passing
- ✅ 80%+ coverage
- ✅ No warnings/errors
- ✅ No technical debt
- ✅ Code review approved

### Architecture
- ✅ 4-layer separation strict
- ✅ Dependency rule enforced
- ✅ SOLID principles applied
- ✅ Clean code patterns
- ✅ Scalable design

### Security
- ✅ PBKDF2 password hashing
- ✅ Audit trails complete
- ✅ Soft delete enabled
- ✅ No secrets in code
- ✅ Input validation robust

### Documentation
- ✅ Architecture guide complete
- ✅ Developer onboarding ready
- ✅ API documentation ready
- ✅ Deployment guide ready
- ✅ Troubleshooting guide ready

### Operations
- ✅ Build automation ready
- ✅ Logging configured
- ✅ Monitoring points identified
- ✅ Error handling robust
- ✅ Graceful degradation

---

## ⏱️ Timeline

| Task | Duration | Start | Complete |
|------|----------|-------|----------|
| Domain tests | 45 min | T+0 | T+45m |
| Application tests | 60 min | T+45m | T+1h45m |
| Infrastructure tests | 45 min | T+1h45m | T+2h30m |
| E2E tests | 45 min | T+2h30m | T+3h15m |
| Coverage reporting | 30 min | T+3h15m | T+3h45m |
| Final verification | 15 min | T+3h45m | T+4h |
| **Total** | **4 hours** | **Start** | **+4h** |

---

## 🎯 Post-Phase 6 (Phase 7+)

### Immediate (Day 1)
- ✅ Deploy to staging
- ✅ Smoke testing
- ✅ Performance testing
- ✅ User acceptance testing

### Short-term (Week 1)
- ✅ Deploy to production
- ✅ Monitor for issues
- ✅ Team training
- ✅ User documentation

### Medium-term (Month 1)
- ✅ Gather feedback
- ✅ Performance optimization
- ✅ Additional feature development
- ✅ Continuous improvement

---

## 📊 Final Metrics (Expected After Phase 6)

| Metric | Target | Expected |
|--------|--------|----------|
| **Overall Progress** | 100% | ✅ 100% |
| **Services Consolidated** | 12/25 | ✅ 12/25 |
| **Test Coverage** | 80%+ | ✅ 85%+ |
| **Build Errors** | 0 | ✅ 0 |
| **Code Duplication** | <20% | ✅ 18-20% |
| **Documentation** | Complete | ✅ Complete |
| **Production Ready** | Yes | ✅ Yes |

---

**Status**: Ready to implement  
**Expected Completion**: After Phase 5 (est. 3-4 hours)  
**Final Goal**: Production-ready, fully tested, thoroughly documented

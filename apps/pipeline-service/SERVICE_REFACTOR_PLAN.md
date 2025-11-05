# Service Architecture Refactor Plan

## Overview

This document outlines a refactor plan to transition from our current entity-centric service architecture to a responsibility-centric architecture combined with the Unit of Work pattern. This change will improve code organization, testability, and transaction management.

## Current Architecture Issues

### Entity-Centric Problems
- **Poor Cohesion**: Services mix multiple business concerns (e.g., JobService handles creation + scheduling + sitemap management)
- **Tight Coupling**: Services directly access multiple repositories, violating service boundaries
- **Transaction Complexity**: Cross-repository operations require manual transaction management
- **Responsibility Confusion**: Single services handling lifecycle, scheduling, execution, and coordination

### Specific Issues Identified
1. `JobService` mixes job creation with sitemap creation (violates service boundaries)
2. `JobExecutionService` handles both execution tracking AND job scheduling updates
3. `JobDispatchService` has dependencies on all other services
4. Cross-repository transactions are managed manually with scopes

## Target Architecture: Responsibility-Centric Services

### Mental Model Shift
**From**: "One service per domain entity" (noun-based)
**To**: "One service per business capability" (verb-based)

### Core Principles
1. **High Cohesion**: Each service focuses on a single business responsibility
2. **Loose Coupling**: Services depend on abstractions, not concrete implementations
3. **Clear Boundaries**: Each service owns its specific business capability
4. **Atomic Operations**: Use Unit of Work for cross-entity transactions

## Proposed Service Structure

### 1. JobAggregateService
**Responsibility**: Manage complete job lifecycle operations spanning multiple entities

**Methods**:
```csharp
Task<CreateJobResponse> CreateJobWithSitemapsAsync(CreateJobCommand command, CancellationToken ct)
Task<Job?> GetJobByIdAsync(Guid jobId, CancellationToken ct)
Task<Job?> GetNextJobForExecutionAsync(CancellationToken ct)
Task<bool> ActivateJobAsync(Guid jobId, CancellationToken ct)
Task<bool> DeactivateJobAsync(Guid jobId, CancellationToken ct)
Task<JobWithSitemaps> GetJobWithSitemapsAsync(Guid jobId, CancellationToken ct)
```

**Dependencies**: 
- `IUnitOfWork` (for atomic operations across Job + JobSitemap repositories)

### 2. JobSchedulingService
**Responsibility**: Pure scheduling logic and cron management

**Methods**:
```csharp
DateTimeOffset? CalculateNextExecution(string cronExpression)
Task<bool> UpdateJobNextExecutionAsync(Guid jobId, DateTimeOffset nextExecution, CancellationToken ct)
Task<List<Job>> GetJobsDueForExecutionAsync(CancellationToken ct)
bool IsJobReadyForExecution(Job job, TimeSpan tolerance = default)
Task<bool> MarkJobAsExecutedAsync(Guid jobId, CancellationToken ct)
Task<bool> DisableJobAfterExecution(Guid jobId, CancellationToken ct)
```

**Dependencies**: 
- `IUnitOfWork` or `IJobRepository` (single entity operations)

### 3. JobExecutionService
**Responsibility**: Track execution state and lifecycle (NO scheduling logic)

**Methods**:
```csharp
Task<JobExecution> CreateExecutionAsync(Guid jobId, CancellationToken ct)
Task<bool> UpdateExecutionStatusAsync(Guid executionId, JobExecutionStatus status, CancellationToken ct)
Task<JobExecution?> GetExecutionByIdAsync(Guid executionId, CancellationToken ct)
Task<List<JobExecution>> GetExecutionsForJobAsync(Guid jobId, CancellationToken ct)
Task<bool> CompleteExecutionAsync(Guid executionId, CancellationToken ct)
Task<bool> FailExecutionAsync(Guid executionId, string? error, CancellationToken ct)
```

**Dependencies**: 
- `IJobExecutionRepository` (single entity focus)

### 4. JobDispatchService
**Responsibility**: Orchestrate the dispatch process (coordination, not implementation)

**Methods**:
```csharp
Task DispatchNextJobAsync(CancellationToken ct)
Task DispatchJobAsync(Guid jobId, CancellationToken ct)
Task<bool> CanDispatchJobAsync(Guid jobId, CancellationToken ct)
```

**Orchestration Flow**:
1. `JobAggregateService.GetNextJobForExecutionAsync()`
2. `JobSchedulingService.IsJobReadyForExecution()`
3. `JobExecutionService.CreateExecutionAsync()`
4. `JobSitemapService.GetSitemapUrlsForJobAsync()`
5. Queue the pipeline contexts
6. `JobExecutionService.UpdateExecutionStatusAsync()`
7. `JobSchedulingService.UpdateJobNextExecutionAsync()`

**Dependencies**: 
- All other job services (for orchestration)
- `IQueueClient`

### 5. JobSitemapService
**Responsibility**: Pure sitemap processing logic (NO CRUD operations)

**Methods**:
```csharp
Task<List<string>> GetSitemapUrlsForJobAsync(Guid jobId, CancellationToken ct)
Task<List<string>> LoadUrlsFromSitemapAsync(string sitemapUrl, CancellationToken ct)
Task<bool> ValidateSitemapUrlAsync(string sitemapUrl, CancellationToken ct)
Task<List<string>> LoadUrlsFromMultipleSitemapsAsync(IEnumerable<string> sitemapUrls, CancellationToken ct)
```

**Dependencies**: 
- `IJobSitemapRepository` (for reading sitemaps)
- `ISitemapLoader` (for processing sitemap files)

## Unit of Work Pattern Implementation

### Interface Definition
```csharp
public interface IUnitOfWork : IDisposable
{
    IJobRepository JobRepository { get; }
    IJobSitemapRepository JobSitemapRepository { get; }
    IJobExecutionRepository JobExecutionRepository { get; }
    
    Task<int> CommitAsync(CancellationToken cancellationToken);
    Task RollbackAsync(CancellationToken cancellationToken);
}
```

### Benefits
- **Atomic Transactions**: Ensure consistency across multiple repositories
- **Cleaner Code**: No more manual scope management
- **Better Testing**: Easy to mock entire unit of work
- **Clear Transaction Boundaries**: Each service method becomes a transaction

### Services That Need UoW
- **JobAggregateService**: Yes (cross-entity operations)
- **JobSchedulingService**: Maybe (depending on scheduling complexity)
- **JobExecutionService**: No (single entity operations)
- **JobDispatchService**: Maybe (depending on orchestration needs)

## Migration Plan

### Phase 1: Implement Unit of Work Pattern
1. Create `IUnitOfWork` interface
2. Implement `UnitOfWork` class with Entity Framework
3. Update DI container to register UoW
4. Create integration tests for UoW

### Phase 2: Refactor JobAggregateService
1. Create new `JobAggregateService` 
2. Move job creation logic from `JobService`
3. Use UoW for atomic job + sitemap creation
4. Update `CreateJobHandler` to use new service
5. Update tests

### Phase 3: Refactor JobSchedulingService
1. Create new `JobSchedulingService`
2. Move scheduling logic from `JobService` and `JobExecutionService`
3. Move cron calculation logic from static service
4. Update `JobDispatchService` to use new service
5. Update tests

### Phase 4: Refactor JobExecutionService
1. Remove scheduling logic from existing `JobExecutionService`
2. Focus purely on execution state management
3. Update `JobDispatchService` to coordinate with `JobSchedulingService`
4. Update tests

### Phase 5: Refactor JobDispatchService
1. Convert to pure orchestration service
2. Remove business logic, delegate to appropriate services
3. Update scheduling service to use new architecture
4. Update tests

### Phase 6: Refactor JobSitemapService
1. Remove CRUD operations (move to repositories)
2. Focus purely on sitemap processing logic
3. Update all consumers of sitemap service
4. Update tests

### Phase 7: Clean Up Legacy Services
1. Remove old `JobService` (functionality moved to aggregate service)
2. Update all references and dependency injection
3. Remove obsolete tests
4. Update documentation

## Testing Strategy

### Unit Testing
- Each service can be tested independently
- Mock dependencies easily (especially UoW)
- Test business logic without database concerns

### Integration Testing
- Test UoW transaction behavior
- Test service coordination in `JobDispatchService`
- Test complete job creation flow

### Migration Testing
- Ensure no regression during refactor
- Test both old and new implementations side-by-side during transition
- Performance testing to ensure no degradation

## Benefits of New Architecture

### Code Quality
- **High Cohesion**: Each service has focused responsibility
- **Loose Coupling**: Clear boundaries between services
- **Testability**: Easy to mock and unit test
- **Maintainability**: Changes are localized to specific services

### Business Value
- **Atomic Operations**: No more race conditions in job creation
- **Clear Workflows**: Easy to understand dispatch process
- **Extensibility**: Easy to add new job types or scheduling logic
- **Reliability**: Better transaction management

### Developer Experience
- **Clear Mental Model**: Responsibility-based organization
- **Easier Debugging**: Clear separation of concerns
- **Better Error Handling**: Centralized transaction management
- **Simpler Testing**: Focused, independent services

## Risk Mitigation

### Technical Risks
- **Database Performance**: Monitor UoW transaction overhead
- **Memory Usage**: Ensure UoW instances are properly disposed
- **Complexity**: Incremental migration to manage complexity

### Business Risks
- **Feature Regression**: Comprehensive testing during migration
- **Performance Degradation**: Performance testing at each phase
- **Deployment Issues**: Feature flags for gradual rollout

## Success Metrics

### Code Quality Metrics
- Reduced cyclomatic complexity in services
- Improved test coverage
- Reduced coupling between services

### Business Metrics
- Zero race conditions in job creation
- Improved system reliability
- Faster development of new features

## Conclusion

This refactor will transform our codebase from an entity-centric architecture with transaction management issues to a clean, responsibility-centric architecture with proper transaction boundaries. The migration plan ensures minimal risk while delivering significant long-term benefits in maintainability, testability, and reliability.

The combination of responsibility-centric services and the Unit of Work pattern provides a solid foundation for future development and helps solve our current architectural challenges.
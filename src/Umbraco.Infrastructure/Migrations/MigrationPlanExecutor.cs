using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Migrations;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Scoping;

namespace Umbraco.Cms.Infrastructure.Migrations;

public class MigrationPlanExecutor : IMigrationPlanExecutor
{
    private readonly ILogger<MigrationPlanExecutor> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IMigrationBuilder _migrationBuilder;
    private readonly IUmbracoDatabaseFactory _databaseFactory;
    private readonly IScopeAccessor _scopeAccessor;
    private readonly ICoreScopeProvider _scopeProvider;

    public MigrationPlanExecutor(
        ICoreScopeProvider scopeProvider,
        IScopeAccessor scopeAccessor,
        ILoggerFactory loggerFactory,
        IMigrationBuilder migrationBuilder,
        IUmbracoDatabaseFactory databaseFactory)
    {
        _scopeProvider = scopeProvider;
        _scopeAccessor = scopeAccessor;
        _loggerFactory = loggerFactory;
        _migrationBuilder = migrationBuilder;
        _databaseFactory = databaseFactory;
        _logger = _loggerFactory.CreateLogger<MigrationPlanExecutor>();
    }

    /// <summary>
    ///     Executes the plan.
    /// </summary>
    /// <param name="scope">A scope.</param>
    /// <param name="fromState">The state to start execution at.</param>
    /// <param name="migrationBuilder">A migration builder.</param>
    /// <param name="logger">A logger.</param>
    /// <param name="loggerFactory"></param>
    /// <returns>The final state.</returns>
    /// <remarks>The plan executes within the scope, which must then be completed.</remarks>
    public ExecutedMigrationPlan Execute(MigrationPlan plan, string fromState)
    {
        plan.Validate();

        _logger.LogInformation("Starting '{MigrationName}'...", plan.Name);
        var nextState = fromState;

        _logger.LogInformation("At {OrigState}", string.IsNullOrWhiteSpace(nextState) ? "origin" : nextState);

        if (!plan.Transitions.TryGetValue(nextState, out MigrationPlan.Transition? transition))
        {
            plan.ThrowOnUnknownInitialState(nextState);
        }

        List<MigrationPlan.Transition> completedTransitions = new();

        while (transition != null)
        {
            _logger.LogInformation("Execute {MigrationType}", transition.MigrationType.Name);

            try
            {
                if (transition.MigrationType.IsAssignableTo(typeof(UnscopedMigrationBase)))
                {
                    RunUnscopedMigration(transition.MigrationType, plan);
                }
                else
                {
                    RunScopedMigration(transition.MigrationType, plan);
                }
            }
            catch (Exception exception)
            {
                // We have to always return something, so whatever running this has a chance to save the state we got to.
                return new ExecutedMigrationPlan
                {
                    Successful = false,
                    Exception = exception,
                    InitialState = fromState,
                    FinalState = transition.SourceState,
                    CompletedTransitions = completedTransitions,
                    Plan = plan,
                };
            }

            // The plan migration (transition), completed, so we'll add this to our list so we can return this at some point.
            completedTransitions.Add(transition);
            nextState = transition.TargetState;

            _logger.LogInformation("At {OrigState}", nextState);

            // throw a raw exception here: this should never happen as the plan has
            // been validated - this is just a paranoid safety test
            if (!plan.Transitions.TryGetValue(nextState, out transition))
            {
                return new ExecutedMigrationPlan
                {
                    Successful = false,
                    Exception = new InvalidOperationException($"Unknown state \"{nextState}\"."),
                    InitialState = fromState,
                    // We were unable to get the next transition, and we never executed it, so final state is source state.
                    FinalState = completedTransitions.Last().TargetState,
                    CompletedTransitions = completedTransitions,
                    Plan = plan,
                };
            }
        }

        _logger.LogInformation("Done");

        return new ExecutedMigrationPlan
        {
            Successful = true,
            InitialState = fromState,
            FinalState = transition?.TargetState ?? completedTransitions.Last().TargetState,
            CompletedTransitions = completedTransitions,
            Plan = plan,
        };
    }

    private void RunUnscopedMigration(Type migrationType, MigrationPlan plan)
    {
        using IUmbracoDatabase database = _databaseFactory.CreateDatabase();
        var context = new MigrationContext(plan, database, _loggerFactory.CreateLogger<MigrationContext>());

        MigrationBase migration = _migrationBuilder.Build(migrationType, context);

        // We run the migration with no scope or suppressed notifications, it's up to the migrations themselves to handle this.
        migration.Run();
    }

    private void RunScopedMigration(Type migrationType, MigrationPlan plan)
    {
        // We want to suppress scope (service, etc...) notifications during a migration plan
        // execution. This is because if a package that doesn't have their migration plan
        // executed is listening to service notifications to perform some persistence logic,
        // that packages notification handlers may explode because that package isn't fully installed yet.
        using ICoreScope scope = _scopeProvider.CreateCoreScope();
        using (scope.Notifications.Suppress())
        {
            var context = new MigrationContext(plan, _scopeAccessor.AmbientScope?.Database, _loggerFactory.CreateLogger<MigrationContext>());
            MigrationBase migration = _migrationBuilder.Build(migrationType, context);

            migration.Run();

            scope.Complete();
        }
    }
}

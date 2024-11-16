using Serilog;

namespace Rocket.Surgery.Extensions.Testing;

public class AutoSubstituteTestContext(Action<AutoSubstituteTestContext, LoggerConfiguration>? configureLogger)
    : RocketSurgeryTestContext<AutoSubstituteTestContext>(configureLogger), IAutoSubstituteTestContext;

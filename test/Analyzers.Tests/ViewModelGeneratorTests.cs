using Analyzers.Tests.Helpers;
using FluentAssertions;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Rocket.Surgery.Extensions.Testing.Analyzers;
using Xunit;
using Xunit.Abstractions;

namespace Analyzers.Tests
{
    public class InheritFromGeneratorTests : GeneratorTest
    {
        public InheritFromGeneratorTests([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper, LogLevel.Trace)
        {
            WithGenerator<AbcGenerator>();
        }

        [Fact]
        public async Task Should_Add_Sources_For_XUnit()
        {
            AddReferences(typeof(FactAttribute), typeof(ITest));

            var expected = @"
#nullable enable
using System;
using MediatR;
using Rocket.Surgery.LaunchPad.Foundation;

namespace Sample.Core.Operations.Rockets
{
    public static partial class CreateRocket
    {
        public partial record Request
        {
            public string SerialNumber { get; set; }

            public Request With(Model value) => this with {SerialNumber = value.SerialNumber};
        }
    }
}
#nullable restore
";

            var result = await GenerateAsync();
            result.TryGetResult<AbcGenerator>(out var output).Should().BeTrue();
            result.EnsureDiagnosticSeverity();
            result.AssertGeneratedAsExpected<InheritFromGenerator>(expected);
        }

        [Fact]
        public async Task Should_Inherit_Multiple_With_Method_For_Record()
        {
            var source = @"
using System;
using MediatR;
using Rocket.Surgery.LaunchPad.Foundation;

namespace Sample.Core.Operations.Rockets
{
    public static partial class CreateRocket
    {
        public partial record Model
        {
            public string SerialNumber { get; set; }
        }

        public partial record Other
        {
            public string OtherNumber { get; set; }
        }

        [InheritFrom(typeof(Model))]
        [InheritFrom(typeof(Other))]
        public partial record Request : IRequest<Response>
        {
            public Guid Id { get; init; }
        }

        public partial record Response {}
    }
}
";

            var expected = @"
#nullable enable
using System;
using MediatR;
using Rocket.Surgery.LaunchPad.Foundation;

namespace Sample.Core.Operations.Rockets
{
    public static partial class CreateRocket
    {
        public partial record Request
        {
            public string SerialNumber { get; set; }

            public Request With(Model value) => this with {SerialNumber = value.SerialNumber};
            public string OtherNumber { get; set; }

            public Request With(Other value) => this with {OtherNumber = value.OtherNumber};
        }
    }
}
#nullable restore
";

            var result = await GenerateAsync(source);
            result.EnsureDiagnosticSeverity();
            result.AssertGeneratedAsExpected<InheritFromGenerator>(expected);
        }

        [Fact]
        public async Task Should_Generate_With_Method_For_Record_That_Inherits()
        {
            var source = @"
using System;
using MediatR;
using Rocket.Surgery.LaunchPad.Foundation;

namespace Sample.Core.Operations.Rockets
{
    public static partial class CreateRocket
    {
        public partial record Model
        {
            public string SerialNumber { get; set; }
        }

        [InheritFrom(typeof(Model))]
        public partial record Request : Model, IRequest<Response>
        {
            public Guid Id { get; init; }
        }

        public partial record Response {}
    }
}
";

            var expected = @"
#nullable enable
using System;
using MediatR;
using Rocket.Surgery.LaunchPad.Foundation;

namespace Sample.Core.Operations.Rockets
{
    public static partial class CreateRocket
    {
        public partial record Request
        {
            public Request With(Model value) => this with {SerialNumber = value.SerialNumber};
        }
    }
}
#nullable restore
";

            var result = await GenerateAsync(source);
            result.EnsureDiagnosticSeverity();
            result.AssertGeneratedAsExpected<InheritFromGenerator>(expected);
        }

        [Fact]
        public async Task Should_Generate_With_Method_For_Class()
        {
            var source = @"
using System;
using MediatR;
using Rocket.Surgery.LaunchPad.Foundation;

namespace Sample.Core.Operations.Rockets
{
    public static partial class CreateRocket
    {
        public partial class Model
        {
            public string SerialNumber { get; set; }
        }

        [InheritFrom(typeof(Model))]
        public partial class Request : IRequest<Response>
        {
            public Guid Id { get; init; }
        }

        public partial record Response {}
    }
}
";

            var expected = @"
#nullable enable
using System;
using MediatR;
using Rocket.Surgery.LaunchPad.Foundation;

namespace Sample.Core.Operations.Rockets
{
    public static partial class CreateRocket
    {
        public partial class Request
        {
            public string SerialNumber { get; set; }

            public Request With(Model value) => new Request{Id = this.Id, SerialNumber = value.SerialNumber};
        }
    }
}
#nullable restore
";

            var result = await GenerateAsync(source);
            result.EnsureDiagnosticSeverity();
            result.AssertGeneratedAsExpected<InheritFromGenerator>(expected);
        }
    }
}

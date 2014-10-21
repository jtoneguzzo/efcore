// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.FunctionalTests;

namespace Microsoft.Data.Entity.InMemory.FunctionalTests
{
    public class InMemoryIncludeTest : IncludeTestBase<InMemoryNorthwindQueryFixture>
    {
        public InMemoryIncludeTest(InMemoryNorthwindQueryFixture fixture)
            : base(fixture)
        {
        }
    }
}

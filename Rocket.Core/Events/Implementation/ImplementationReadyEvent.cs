﻿using Rocket.API;
using Rocket.API.Eventing;

namespace Rocket.Core.Events.Implementation
{
    public class ImplementationReadyEvent : Event
    {
        public IImplementation Implementation { get; }

        public ImplementationReadyEvent(IImplementation implementation) : this(implementation, true)
        {

        }

        public ImplementationReadyEvent(IImplementation implementation, bool global = true) : base(global)
        {
            Implementation = implementation;
        }

        public ImplementationReadyEvent(IImplementation implementation, EventExecutionTargetContext executionTarget = EventExecutionTargetContext.Sync, bool global = true) : base(executionTarget, global)
        {
            Implementation = implementation;
        }

        public ImplementationReadyEvent(IImplementation implementation, string name = null, EventExecutionTargetContext executionTarget = EventExecutionTargetContext.Sync, bool global = true) : base(name, executionTarget, global)
        {
            Implementation = implementation;
        }
    }
}
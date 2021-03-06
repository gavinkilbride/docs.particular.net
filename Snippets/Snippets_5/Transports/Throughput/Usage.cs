﻿namespace Snippets5.Transports.Throughput
{
    using NServiceBus.Unicast;

    public class Usage
    {

        public Usage()
        {
            UnicastBus bus = null;

            #region ChangeThroughput
            //bus is an instance of NServiceBus.Unicast.UnicastBus
            bus.Transport.ChangeMaximumMessageThroughputPerSecond(10);
            bus.Transport.ChangeMaximumConcurrencyLevel(5);
            #endregion

            #region ReadThroughput

            //bus is an instance of NServiceBus.Unicast.UnicastBus
            int messageThroughputPerSecond = bus.Transport.MaximumMessageThroughputPerSecond;
            int maximumConcurrencyLevel = bus.Transport.MaximumConcurrencyLevel;

            #endregion
        }

    }
}
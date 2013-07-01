//
//      Copyright (C) 2012 DataStax Inc.
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
//
﻿using System.Collections.Generic;


namespace Cassandra
{
    /// <summary>
    ///  A Round-robin load balancing policy. <p> This policy queries nodes in a
    ///  round-robin fashion. For a given query, if an host fail, the next one
    ///  (following the round-robin order) is tried, until all hosts have been tried.
    ///  </p><p> This policy is not datacenter aware and will include every known
    ///  Cassandra host in its round robin algorithm. If you use multiple datacenter
    ///  this will be inefficient and you will want to use the
    ///  <link>DCAwareRoundRobinPolicy</link> load balancing policy instead.</p>
    /// </summary>
    public class RoundRobinPolicy : ILoadBalancingPolicy
    {
        /// <summary>
        ///  Creates a load balancing policy that picks host to query in a round robin
        ///  fashion (on all the hosts of the Cassandra cluster).
        /// </summary>
        public RoundRobinPolicy() { }

        Cluster _cluster;
        int _startidx = -1;

        public void Initialize(Cluster cluster)
        {
            this._cluster = cluster;
        }


        /// <summary>
        ///  Return the HostDistance for the provided host. <p> This policy consider all
        ///  nodes as local. This is generally the right thing to do in a single
        ///  datacenter deployement. If you use multiple datacenter, see
        ///  <link>DCAwareRoundRobinPolicy</link> instead.</p>
        /// </summary>
        /// <param name="host"> the host of which to return the distance of. </param>
        /// 
        /// <returns>the HostDistance to <code>host</code>.</returns>
        public HostDistance Distance(Host host)
        {
            return HostDistance.Local;
        }

        /// <summary>
        ///  Returns the hosts to use for a new query. <p> The returned plan will try each
        ///  known host of the cluster. Upon each call to this method, the ith host of the
        ///  plans returned will cycle over all the host of the cluster in a round-robin
        ///  fashion.</p>
        /// </summary>
        /// <param name="query"> the query for which to build the plan. </param>
        /// 
        /// <returns>a new query plan, i.e. an iterator indicating which host to try
        ///  first for querying, which one to use as failover, etc...</returns>
        public IEnumerable<Host> NewQueryPlan(Query query)
        {
            List<Host> copyOfHosts = new List<Host>(_cluster.Metadata.AllHosts());
            for (int i = 0; i < copyOfHosts.Count; i++)
            {
                if (_startidx == -1)
                    _startidx = StaticRandom.Instance.Next(copyOfHosts.Count);

                _startidx %= copyOfHosts.Count;

                var h = copyOfHosts[_startidx];

                _startidx++;

                if (h.IsConsiderablyUp)
                    yield return h;

            }
        }
    }
}

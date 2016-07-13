using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderSwichPrefer.Model
{
    /// <summary>
    /// The <source, destionation> pair characterized by their source node, destionation node,
    /// and carried demand.
    /// </summary>
    class Pair
    {
        /// <summary>
        /// The source node of a pair
        /// </summary>
        private Node sourceNode;
        public Node SourceNode
        {
            get { return sourceNode; }
            set { sourceNode = value; }
        }

        /// <summary>
        /// The destination node of a pair
        /// </summary>
        private Node destinaNode;
        public Node DestinaNode
        {
            get { return destinaNode; }
            set { destinaNode = value; }
        }

        /// <summary>
        /// The demand carried on by this pair
        /// </summary>
        private int totalDemand;
        public int TotalDemand
        {
            get { return totalDemand; }
            set { totalDemand = value; }
        }

        private bool isAllDemandAssigned;
        /// <summary>
        /// Is the aggregated traffic granted successfully
        /// </summary>
        public bool IsAllDemandAssigned
        {
            get { return isAllDemandAssigned; }
            set { isAllDemandAssigned = value; }
        }

        /// <summary>
        /// The set of k-shortest paths for this pair, classed by the path length;
        /// ordered by path's length
        /// </summary>
        private List<Path> pathSet;
        public List<Path> PathSet
        {
            get { return pathSet; }
        }

        private Model.Request[] requestSet;
        /// <summary>
        /// The set of requests that belong to this <source, destination> pair
        /// </summary>
        public Model.Request[] RequestSet
        {
            get { return requestSet; }
            set { requestSet = value; }
        }

        private Request aggregatedTraffic;
        /// <summary>
        /// A request represente the aggregated traffic on this node pair
        /// </summary>
        public Request AggregatedTraffic
        {
            get { return aggregatedTraffic; }
            set { aggregatedTraffic = value; }
        }

        private List<Request> grantedRequests;
        public List<Request> GrantedRequests
        {
            get { return grantedRequests; }
            set { grantedRequests = value; }
        }

        private int grantedDemand;
        /// <summary>
        /// The volumn of granted traffic
        /// </summary>
        public int GrantedDemand
        {
            get { return grantedDemand; }
            set { grantedDemand = value; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="source">source node</param>
        /// <param name="destination">destination node</param>
        public Pair(Node source, Node destination)
        {
            sourceNode = source;
            destinaNode = destination;
            IsAllDemandAssigned = false;
            pathSet = new List<Path>();
            grantedRequests = new List<Request>();
            grantedDemand = 0;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="source">source node</param>
        /// <param name="destination">destination node</param>
        /// <param name="demand">traffic demand need to be granted</param>
        public Pair(Node source, Node destination, int demand)
            : this(source, destination)
        {
            totalDemand = demand;
        }

        /// <summary>
        /// Determine wether two pair objects are same pair
        /// </summary>
        /// <param name="comparePair">The pair needs to be compared with</param>
        /// <returns>True or False</returns>
        public bool equalsTo(Pair comparePair)
        {
            if (comparePair.sourceNode == sourceNode && comparePair.destinaNode == destinaNode)
            {
                return true;
            }
            if (comparePair.sourceNode == destinaNode && comparePair.destinaNode == sourceNode)
            {
                return true;
            }
            return false;
        }
    }
}

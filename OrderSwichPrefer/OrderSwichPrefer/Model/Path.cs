using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderSwichPrefer.Model
{
    /// <summary>
    /// 
    /// </summary>
    class Path
    {
        /// <summary>
        /// path id
        /// </summary>
        private int id;
        public int Id
        {
            get { return id; }
            set { id = value; }
        }

        /// <summary>
        /// The <source, destionation> pair that this path generated for
        /// </summary>
        private Pair nodePair;
        public Pair NodePair
        {
            get { return nodePair; }
            set { nodePair = value; }
        }

        /// <summary>
        /// The set of links on this path
        /// </summary>
        private List<Link> links;
        internal List<Link> Links
        {
            get { return links; }
            set 
            {
                links = new List<Link>();
                links.AddRange(value);
            }
        }

        private BitArray linkUsage;
        public BitArray LinkUsage
        {
            get { return linkUsage; }
            set { linkUsage = value; }
        }

        /// <summary>
        /// The weight of this path
        /// </summary>
        private double weight;
        public double Weight
        {
            get { return weight; }
            set { weight = value; }
        }

        private int weightForSlice;
        /// <summary>
        /// The weight for each slices
        /// </summary>
        public int WeightForSlice
        {
            get { return weightForSlice; }
            set { weightForSlice = value; }
        }

        private BitArray assignedSlices;
        /// <summary>
        /// Wavelength assigned to this path
        /// </summary>
        public BitArray AssignedSlices
        {
            get { return assignedSlices; }
            set
            {
                assignedSlices = new BitArray(Graph.numSlices, false);
                assignedSlices.Or(value);
            }
        }

        private int usedNumSlices;
        /// <summary>
        /// Number of slices used on this path
        /// </summary>
        public int UsedNumSlices
        {
            get { return usedNumSlices; }
            set { usedNumSlices = value; }
        }

        private int length;
        /// <summary>
        /// The length of path (km)
        /// </summary>
        public int Length
        {
            get { return length; }
            set { length = value; }
        }

        private int demand;
        /// <summary>
        /// Demand assigned on this path
        /// </summary>
        public int Demand
        {
            get { return demand; }
            set { demand = value; }
        }

        private Dictionary<Model.Path, int> neighbors;
        /// <summary>
        /// The paths share links with this path, and the number of links shared with path
        /// </summary>
        public Dictionary<Model.Path, int> Neighbors
        {
            get { return neighbors; }
            set { neighbors = value; }
        }


        private List<Request> usedByRequests;
        /// <summary>
        /// The set requests that use this path
        /// </summary>
        public List<Request> UsedByRequests
        {
            get { return usedByRequests; }
        }

        //Constructor
        public Path(int id, Pair nodePair)
            : this()
        {
            this.id = id;
            this.nodePair = nodePair;
        }

        //Constructor
        public Path()
        {
            links = new List<Link>();
            assignedSlices = new BitArray(Graph.numSlices, false);
            neighbors = new Dictionary<Path, int>();
            usedByRequests = new List<Request>();
            usedNumSlices = 0;
            weightForSlice = 0;
            demand = 0;
        }


        public List<Link> AddLinks(Link link)
        {
            if (!this.links.Contains(link))
            {
                this.links.Add(link);
            }
            return this.links;
        }

        public void AssignSlices(int startIndex, int numSlices)
        {
            for (int i = startIndex; i < startIndex + numSlices; i++)
            {
                if (assignedSlices[i]) throw new System.ArgumentException("Cannot assign traffic to slice that already been used!");
                assignedSlices[i] = true;
            }
        }
    }
}

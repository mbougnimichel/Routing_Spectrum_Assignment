using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderSwichPrefer.Model
{
    class Request
    {
        private int id;
        /// <summary>
        /// 
        /// </summary>
        public int Id
        {
            get { return id; }
            set { id = value; }
        }
        private Pair nodePair;
        /// <summary>
        /// 
        /// </summary>
        public Pair NodePair
        {
            get { return nodePair; }
            set { nodePair = value; }
        }
        private int demand;
        /// <summary>
        /// 
        /// </summary>
        public int Demand
        {
            get { return demand; }
            set { demand = value; }
        }
        private bool isGranted;
        /// <summary>
        /// 
        /// </summary>
        public bool IsGranted
        {
            get { return isGranted; }
            set { isGranted = value; }
        }
        private Path usedPath;
        /// <summary>
        /// 
        /// </summary>
        public Path UsedPath
        {
            get { return usedPath; }
            set { usedPath = value; }
        }
        private int sliceStartIndex;
        /// <summary>
        /// 
        /// </summary>
        public int SliceStartIndex
        {
            get { return sliceStartIndex; }
            set { sliceStartIndex = value; }
        }
        private int numSlicesUsage;
        /// <summary>
        /// 
        /// </summary>
        public int NumSlicesUsage
        {
            get { return numSlicesUsage; }
            set { numSlicesUsage = value; }
        }
        private Modulation usedModulation;
        /// <summary>
        /// 
        /// </summary>
        public Modulation UsedModulation
        {
            get { return usedModulation; }
            set { usedModulation = value; }
        }
        private int numRG;
        /// <summary>
        /// 
        /// </summary>
        public int NumRG
        {
            get { return numRG; }
            set { numRG = value; }
        }

        public Request(int id, Pair nodePair, int demand)
        {
            this.id = id;
            this.nodePair = nodePair;
            this.demand = demand;
        }
    }
}

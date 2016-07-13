using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderSwichPrefer.Model
{
    class PairSolution
    {
        private Model.Pair pair;
        /// <summary>
        /// Pair
        /// </summary>
        public Model.Pair Pair
        {
            get { return pair; }
            set { pair = value; }
        }

        private bool isTrafficGranted;
        /// <summary>
        /// Express whether traffic on this pair is granted
        /// </summary>
        public bool IsTrafficGranted
        {
          get { return isTrafficGranted; }
          set { isTrafficGranted = value; }
        }
        
        private int pathId;
        /// <summary>
        /// The id of used path for this pair
        /// </summary>
        public int PathId
        {
          get { return pathId; }
          set { pathId = value; }
        }

        private int modulationIndex;
        /// <summary>
        /// modulation selection
        /// </summary>
        public int ModulationIndex
        {
            get { return modulationIndex; }
            set { modulationIndex = value; }
        }

        private BitArray slices;
        /// <summary>
        /// slices selection
        /// </summary>
        public BitArray Slices
        {
            get { return slices; }
        }

        private int numRG;
        /// <summary>
        /// number of necessary regenerators
        /// </summary>
        public int NumRG
        {
            get { return numRG; }
            set { numRG = value; }
        }

        private int usedNumSlices;
        public int UsedNumSlices
        {
            get { return usedNumSlices; }
        }

        public PairSolution(Model.Pair pair)
        {
            this.pair = pair;
            this.isTrafficGranted = false;
        }

        public PairSolution(Model.Pair pair, int pathId, int modulationIndex, BitArray slices, int numSlices, int numRG)
            : this(pair)
        {
            this.pathId = pathId;
            this.modulationIndex = modulationIndex;
            this.slices = new BitArray(slices.Count, false);
            this.slices.Or(slices);
            this.usedNumSlices = numSlices;
            this.numRG = numRG;
        }

        public void setSlices(int startIndex, int numSlices)
        {
            slices = new BitArray(Graph.numSlices, false);
            for (int i = startIndex; i < startIndex + numSlices; i++)
            {
                slices[i] = true;
            }
            usedNumSlices = numSlices;
        }
    }
}

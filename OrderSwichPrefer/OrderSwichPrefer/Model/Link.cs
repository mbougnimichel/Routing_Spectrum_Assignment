using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderSwichPrefer.Model
{
    /// <summary>
    /// The link Object in a graph characterized by an unique id, two adjacent nodes,
    /// demand capacity and wavelength assigned to this link.
    /// </summary>
    class Link
    {
        /// <summary>
        /// link id
        /// </summary>
        private int id;
        public int Id
        {
            get { return id; }
            set { id = value; }
        }

        /// <summary>
        /// One of the two nodes adjacent with this link
        /// </summary>
        private Node node1;
        public Node Node1
        {
            get { return node1; }
            set { node1 = value; }
        }

        /// <summary>
        /// One of the two nodes adjacent with this link
        /// </summary>
        private Node node2;
        public Node Node2
        {
            get { return node2; }
            set { node2 = value; }
        }

        private int length;
        /// <summary>
        /// The real lenth (km) of this link
        /// </summary>
        public int Length
        {
            get { return length; }
            set { length = value; }
        }
        

        /// <summary>
        /// The wavelength assigned on this link
        /// </summary>
        private BitArray assignedSlices;
        public BitArray AssignedSlices
        {
            get { return assignedSlices; }
        }

        /// <summary>
        /// The loaded traffic on this link
        /// </summary>
        private int loadDemand;
        public int LoadDemand
        {
            get { return loadDemand; }
            set { loadDemand = value; }
        }

        //Constructor
        public Link(int id)
        {
            this.Id = id;
            assignedSlices = new BitArray(Graph.numSlices, false);
            loadDemand = 0;
        }

        /// <summary>
        /// Determine whether a node is adjacent to this link
        /// </summary>
        /// <param name="nodeId">The node need to be determined</param>
        /// <returns>True or False</returns>
        public bool AdjacentTo(int nodeId)
        {
            if (nodeId == node1.Id || nodeId == node2.Id)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Assign a slices suqence to the link.
        /// </summary>
        /// <param name="startId">The started index of slices sequence needs to be assigned</param>
        /// <param name="numSlices">Number of slices need to be assign on this link</param>
        /// <returns>If slices assigned successfully, return TRUE; else return FALSE</returns>
        public bool AssignSlices(int startIndex, int numSlices)
        {
            bool isSuccess = true;
            for (int i = startIndex; i < startIndex + numSlices; i++)
            {
                if (assignedSlices[i])
                {
                    isSuccess = false;
                    throw new System.ArgumentException("Cannot assign traffic to slice that already been used!");
                }
                assignedSlices[i] = true;
            }
            return isSuccess;
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderSwichPrefer.Model
{
    class Modulation
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

        private int speed;
        /// <summary>
        /// 
        /// </summary>
        public int Speed
        {
            get { return speed; }
            set { speed = value; }
        }

        private int reach;
        /// <summary>
        /// 
        /// </summary>
        public int Reach
        {
            get { return reach; }
            set { reach = value; }
        }

        private string name;
        /// <summary>
        /// 
        /// </summary>
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        private int numSlices;
        /// <summary>
        /// 
        /// </summary>
        public int NumSlices
        {
            get { return numSlices; }
            set { numSlices = value; }
        }

        public Modulation(int id, int speed, string name, int numSlices, int reach)
        {
            this.id = id;
            this.speed = speed;
            this.reach = reach;
            this.numSlices = numSlices;
            this.name = name;
        }
    }
}

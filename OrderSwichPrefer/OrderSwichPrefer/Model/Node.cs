using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderSwichPrefer.Model
{
    /// <summary>
    /// The node Object in a graph characterized by an unique id, a set of adjacent links(dictionary).
    /// </summary>
    class Node
    {
        /// <summary>
        /// Node id
        /// </summary>
        private int id;
        public int Id
        {
            get { return id; }
            set { id = value; }
        }

        /// <summary>
        /// The links adjacent with this link
        /// </summary>
        private Dictionary<int, Link> adjacentLinks;
        internal Dictionary<int, Link> AdjacentLinks
        {
            get { return adjacentLinks; }
            set { adjacentLinks = value; }
        }

        //Constructor
        public Node(int id)
        {
            this.Id = id;
            adjacentLinks = new Dictionary<int, Link>();
        }

        /// <summary>
        /// Generate a new set of adjacent links from the set of links
        /// </summary>
        /// <param name="links">The set of links in a given graph</param>
        /// <returns>A dictionary of adjacent links</returns>
        public Dictionary<int, Link> GenerateAdjacentLinks(HashSet<Link> links)
        {
            adjacentLinks.Clear();
            //check each link, and add the ones adjacent with this node into adjacentLinks dictionary
            foreach (Link link in links)
            {
                if (link.AdjacentTo(id)) adjacentLinks.Add(link.Id, link);
            }
            return adjacentLinks;
        }

        /// <summary>
        /// Add a link in the set of adjacent links
        /// </summary>
        /// <param name="link">The link needs to be added</param>
        /// <returns>A dictionary of adjacent links</returns>
        public Dictionary<int, Link> AddLink(Link link)
        {
            if (link != null)
            {
                adjacentLinks.Add(link.Id, link);
            }
            return adjacentLinks;
        }

    }
}

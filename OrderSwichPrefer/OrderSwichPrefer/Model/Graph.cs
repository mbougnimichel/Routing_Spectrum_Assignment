using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderSwichPrefer.Model
{
    class Graph
    {
        public static int numSlices;

        /// <summary>
        /// The set of links
        /// </summary>
        private Dictionary<int, Link> linkSet;
        public Dictionary<int, Link> LinkSet
        {
            get { return linkSet; }
        }

        /// <summary>
        /// The set of nodes
        /// </summary>
        private Dictionary<int, Node> nodeSet;
        public Dictionary<int, Node> NodeSet
        {
            get { return nodeSet; }
        }

        /// <summary>
        /// The set of <source, destination> pairs
        /// </summary>
        private List<Pair> pairSet;
        public List<Pair> PairSet
        {
            get { return pairSet; }
        }

        private Dictionary<int, Path> pathSet;
        public Dictionary<int, Path> PathSet
        {
            get { return pathSet; }
        }

        private Dictionary<int, Request> requestSet;
        public Dictionary<int, Request> RequestSet
        {
            get { return requestSet; }
            set { requestSet = value; }
        }

        private Modulation[] modulationSet;
        /// <summary>
        /// The set of modulations
        /// </summary>
        internal Modulation[] ModulationSet
        {
            get { return modulationSet; }
        }

        /// <summary>
        /// The number of available wavelength
        /// </summary>
        private int[] slices;
        public int[] Slices
        {
            get { return slices; }
        }

        private int totalDemand;
        /// <summary>
        /// The total demand on the graph
        /// </summary>
        public int TotalDemand
        {
            get { return totalDemand; }
        }

        private int grantedDemand;
        /// <summary>
        /// Grant of Service
        /// </summary>
        public int GrantedDemand
        {
            get { return grantedDemand; }
            set { grantedDemand = value; }
        }

        private Dictionary<string, Pair> nodePairs;

        public Graph(string topologyFile)
        {
            nodePairs = new Dictionary<string, Pair>();
            //read file
            string line;
            System.IO.StreamReader file = new System.IO.StreamReader(topologyFile);

            numSlices = int.Parse(file.ReadLine());
            file.ReadLine();
            file.ReadLine();
            string nodeString = file.ReadLine();

            line = file.ReadLine();
            string linkString = line;
            while (!(line = file.ReadLine()).StartsWith("[<"))
            {
                linkString = linkString + line;
            }

            this.totalDemand = 0;
            this.grantedDemand = 0;
            string trafficString = line;
            while (!(line = file.ReadLine()).StartsWith("[["))
            {
                trafficString = trafficString + "|" + line;
            }

            //read pathInformation
            string pathString = line;
            while ((line = file.ReadLine()) != null)
            {
                pathString = pathString + line;
            }

            //generate a graph
            generateSlicesArray();
            generateNodes(nodeString);
            generateLinks(linkString, nodeSet);
            generateTraffic(trafficString);
            generatePaths(pathString);

            file.Close();

        }

        /// <summary>
        /// Generate a array contains the index of each slices
        /// </summary>
        private void generateSlicesArray()
        {
            slices = new int[numSlices];
            for (int i = 0; i < numSlices; i++)
            {
                slices[i] = i;
            }
        }

        /// <summary>
        /// Generate the nodes set of this graph
        /// </summary>
        /// <param name="nodeString">The information of nodes(Array format)</param>
        private void generateNodes(string nodeString)
        {
            nodeString = nodeString.Replace("[", "");
            nodeString = nodeString.Replace("]", "");
            string[] nodeArray = nodeString.Split(',');

            Dictionary<int, Node> nodeSet = new Dictionary<int, Node>();
            for (int i = 0; i < nodeArray.Length; i++)
            {
                int nodeId = int.Parse(nodeArray[i]);
                nodeSet.Add(nodeId, new Node(nodeId));
            }
            this.nodeSet = nodeSet;
        }

        /// <summary>
        /// Generate the set of links, and fulfill the adjecent links for each node
        /// </summary>
        /// <param name="linkString">The information of links(Array format)</param>
        /// <param name="nodeSet">The nodes set</param>
        private void generateLinks(string linkString, Dictionary<int, Node> nodeSet)
        {
            linkString = linkString.Replace("],[", "|");
            linkString = linkString.Replace("[", "");
            linkString = linkString.Replace("]", "");
            string[] linkAarry = linkString.Split('|');

            Dictionary<int, Link> linkSet = new Dictionary<int, Link>();
            int linkIdIndex = 0;
            for (int i = 0; i < linkAarry.Length; i++)
            {
                //split link's information
                string[] linkInfos = linkAarry[i].Split(',');
                //get adjecent nodes
                Node node1 = nodeSet[int.Parse(linkInfos[0])];
                Node node2 = nodeSet[int.Parse(linkInfos[1])];
                int length = int.Parse(linkInfos[2]);
                //generate links(one link, one fiber)
                for (int fiberCount = 0; fiberCount < int.Parse(linkInfos[3]); fiberCount++)
                {
                    Link link = new Link(linkIdIndex);
                    link.Node1 = node1;
                    link.Node2 = node2;
                    link.Length = length;
                    node1.AddLink(link);
                    node2.AddLink(link);
                    linkSet.Add(linkIdIndex, link);
                    linkIdIndex++;
                }

            }
            this.linkSet = linkSet;
        }

        /// <summary>
        /// Generate traffic on the given network, and set k-shortest paths for each node pair.
        /// </summary>
        /// <param name="trafficString"></param>
        /// <param name="pathString"></param>
        public void generateTraffic(string trafficString)
        {
            //Generate pairs and put them into a deictionary
            pairSet = new List<Pair>();
            requestSet = new Dictionary<int,Request>();

            trafficString = trafficString.Replace("[", "");
            trafficString = trafficString.Replace("]", "");
            trafficString = trafficString.Replace("<", "");
            trafficString = trafficString.Replace(">", "");
            string[] trafficAarry = trafficString.Split('|');
            for (int i = 0; i < trafficAarry.Length; i++)
            {
                string[] perNode = trafficAarry[i].Split(',');
                Node sourceNode = nodeSet[int.Parse(perNode[0]) - 1];
                Node destinaNode = nodeSet[int.Parse(perNode[1]) - 1];
                Pair pair = new Pair(sourceNode, destinaNode, 0);
                nodePairs.Add(sourceNode.Id + "," + destinaNode.Id, pair);
                pairSet.Add(pair);
                for (int j = 2; j < perNode.Length; j++)
                {
                    int trafficDemand = int.Parse(perNode[j]);
                    pair.TotalDemand += trafficDemand;
                    this.totalDemand += trafficDemand;
                }
                //set requests
                Request aggregatedRequest = new Request(requestSet.Count, pair, pair.TotalDemand);
                requestSet.Add(aggregatedRequest.Id, aggregatedRequest);
                pair.AggregatedTraffic = aggregatedRequest;
                int requestNum = pair.TotalDemand / 100;
                pair.RequestSet = new Request[requestNum];
                for (int j = 0; j < requestNum; j++)
                {
                    Request request = new Request(requestSet.Count, pair, 100);
                    pair.RequestSet[j] = request;
                    requestSet.Add(request.Id, request);
                }

            }
        }

        public void generatePaths(string pathString)
        {
            this.pathSet = new Dictionary<int, Path>();
            //Generate paths
            pathString = pathString.Replace("]],[<", "|").Replace("],[", ";").Replace(">,[", "?");
            pathString = pathString.Replace("[", "").Replace("]", "");
            pathString = pathString.Replace("<", "");
            string[] pathsPerNode = pathString.Split('|');
            int pathIdIndex = 0;
            for (int i = 0; i < pathsPerNode.Length; i++)
            {
                string[] pairObjInfo = pathsPerNode[i].Split('?');
                string[] pathsArray = pairObjInfo[1].Split(';');

                if (!nodePairs.ContainsKey(pairObjInfo[0])) continue;
                Pair pair = nodePairs[pairObjInfo[0]];
                //generate new paths, and classify them by their length
                for (int j = 0; j < pathsArray.Length; j++)
                {
                    Path path = new Path(pathIdIndex, pair);
                    string[] linkIds = pathsArray[j].Split(',');
                    path.LinkUsage = new BitArray(linkSet.Count, false);

                    for (int l = 0; l < linkIds.Length; l++)
                    {
                        int linkId = int.Parse(linkIds[l]);
                        Model.Link link = linkSet[linkId];
                        path.AddLinks(link);
                        path.Length += link.Length;
                        path.LinkUsage[linkId] = true;
                    }
                    
                    pair.PathSet.Add(path);
                    pathSet.Add(pathIdIndex, path);
                    pathIdIndex++;

                    if (path.Length == 1 || j >= Program.KSHORTESTPATH) break;
                }
            }
        }

        /// <summary>
        /// ModulationInfo must be given with an order!!!
        /// Assume the first one is the most prefered one!!!
        /// </summary>
        /// <param name="modulationInfo"></param>
        public void generateModulations(string[] modulationInfo)
        {
            modulationSet = new Modulation[modulationInfo.Length];
            for (int i = 0; i < modulationInfo.Length; i++)
            {
                string[] info = modulationInfo[i].Split(',');
                Modulation modu = new Modulation(i, int.Parse(info[0]), info[1], int.Parse(info[2]), int.Parse(info[3]));
                modulationSet[i] = modu;
            }
        }

        /// <summary>
        /// Calculate the number of granted services
        /// </summary>
        /// <returns>The number of granted services</returns>
        public int UpdateGrantOfService()
        {
            int grantOfService = 0;
            for (int i = 0; i < pairSet.Count; i++)
            {
                grantOfService += pairSet[i].GrantedDemand;
            }

            return grantOfService;
        }

        /// <summary>
        /// Set up neighbors for each path
        /// </summary>
        public void SetNeighbors()
        {
            foreach (KeyValuePair<int, Model.Path> entry in pathSet)
            {
                Model.Path path = entry.Value;
                foreach (KeyValuePair<int, Model.Path> neighborEntry in pathSet)
                {
                    Model.Path neighbor = neighborEntry.Value;
                    if (neighbor == path) continue;
                    if (path.Neighbors.ContainsKey(neighbor)) continue;

                    int sharedLinksNum = path.Links.Intersect(neighbor.Links).Count();
                    if (sharedLinksNum > 0)
                    {
                        path.Neighbors.Add(neighbor, sharedLinksNum);
                        neighbor.Neighbors.Add(path, sharedLinksNum);
                    }

                }
            }

        }

    }
}

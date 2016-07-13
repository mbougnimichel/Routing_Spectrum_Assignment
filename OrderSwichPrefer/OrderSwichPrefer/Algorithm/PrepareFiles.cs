using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderSwichPrefer.Algorithm
{
    class PrepareFiles
    {
        private static PrepareFiles prepareFiles;

        public static PrepareFiles getInstance()
        {
            if (prepareFiles == null)
            {
                prepareFiles = new PrepareFiles();
            } 
            return new PrepareFiles();
        }

        /// <summary>
        /// generate data for Yan's k-shortestpath algorthim,
        /// and data for greedy without paths information
        /// </summary>
        /// <param name="fileName">The original data file</param>
        public void PrepareFileForKShortestPath(string fileName)
        {
            //read files
            string line;
            System.IO.StreamReader readFile = new System.IO.StreamReader(fileName);

            // Write the string to a file.
            System.IO.StreamWriter writeToFile = new System.IO.StreamWriter(fileName.Replace("original", "kShortestPath"));
            System.IO.StreamWriter writeToFileGreedy = new System.IO.StreamWriter(fileName.Replace("original", "greedy"));

            //slices
            line = readFile.ReadLine();
            writeToFileGreedy.WriteLine(line);

            //number of nodes
            line = readFile.ReadLine();
            int nodeNum = int.Parse(line);
            writeToFileGreedy.WriteLine(line);
            writeToFile.WriteLine(line);
            writeToFile.WriteLine();

            //number of links
            line = readFile.ReadLine();
            writeToFileGreedy.WriteLine(line);

            //the set of nodes
            line = readFile.ReadLine();
            line = line.Replace("[", "").Replace("]", "");
            string[] nodeSetString = line.Split(',');
            string newNodeSetString = "[";
            for (int i = 0; i < nodeSetString.Length; i++)
            {
                newNodeSetString += (int.Parse(nodeSetString[i]));
                if (i < nodeSetString.Length - 1) newNodeSetString += ",";
            }
            newNodeSetString += "]";
            writeToFileGreedy.WriteLine(newNodeSetString);

            //the set of links
            string nodesGreedy = "[";
            string[] nodes;
            int node1Id, node2Id;
            string line1, line2;
            while (!(line = readFile.ReadLine()).EndsWith("]]"))
            {
                line = line.Replace("[", "").Replace("],", "");
                nodes = line.Split(',');
                node1Id = int.Parse(nodes[0]);
                node2Id = int.Parse(nodes[1]);
                line1 = node1Id + " " + node2Id + " 1";
                line2 = node2Id + " " + node1Id + " 1";
                writeToFile.WriteLine(line1);
                writeToFile.WriteLine(line2);
                nodesGreedy += "[" + node1Id + "," + node2Id + "," + 1 + "],";
                writeToFileGreedy.WriteLine(nodesGreedy);
                nodesGreedy = "";
            }
            line = line.Replace("[", "").Replace("]]", "");
            nodes = line.Split(',');
            node1Id = int.Parse(nodes[0]);
            node2Id = int.Parse(nodes[1]);
            line1 = node1Id + " " + node2Id + " 1";
            line2 = node2Id + " " + node1Id + " 1";
            writeToFile.WriteLine(line1);
            writeToFile.WriteLine(line2);
            nodesGreedy += "[" + node1Id + "," + node2Id + "," + 1 + "]]";
            writeToFileGreedy.WriteLine(nodesGreedy);

            //read and write the traffic matrix
            Dictionary<string, List<int>> pairSet = new Dictionary<string, List<int>>();
            while ((line = readFile.ReadLine()) != null)
            {
                line = line.Replace("[", "").Replace("],", "").Replace("]","");
                string[] trafficTokens = line.Split(',');
                string pairString = trafficTokens[0] + ',' + trafficTokens[1];
                List<int> trafficList;
                if (!pairSet.ContainsKey(pairString))
                {
                    trafficList = new List<int>();
                    pairSet.Add(pairString, trafficList);
                }
                else trafficList = pairSet[pairString];
                trafficList.Add((int)Double.Parse(trafficTokens[2]) /2 * 40);
            }

            string trafficString = "[";
            int counter = 0;
            foreach(KeyValuePair<string, List<int>> entry in pairSet)
            {
                counter ++;
                trafficString += "<" + entry.Key + ">,";
                List<int> trafficList = entry.Value;
                for(int i=0;i<trafficList.Count;i++)
                {
                    trafficString += trafficList[i];
                    if(i < trafficList.Count - 1) trafficString += ',';
                }
                if(counter == pairSet.Count) trafficString += ']';
                writeToFileGreedy.WriteLine(trafficString);
                trafficString = "";
            }

            readFile.Close();
            writeToFileGreedy.Close();
            writeToFile.Close();

        }

        /// <summary>
        /// Read Paths generated by Yen's Algorithm, and write them into target data file
        /// </summary>
        /// <param name="fileName">The name of the file to read paths</param>
        public void ReadShortestPathsResults(string fileName)
        {
            //read greedy file
            string line;
            Dictionary<string, List<Model.Link>> linkDic = new Dictionary<string, List<Model.Link>>();
            Dictionary<int, Model.Node> nodeSet = new Dictionary<int, Model.Node>();
            System.IO.StreamReader readFileGreedy = new System.IO.StreamReader(fileName.Replace("result", "greedy"));
            readFileGreedy.ReadLine();
            readFileGreedy.ReadLine();
            readFileGreedy.ReadLine();
            readFileGreedy.ReadLine();
            line = readFileGreedy.ReadLine();

            int linkIndex = 0;
            do
            {
                line = line.Replace("[", "").Replace("],", "").Replace("]", "");
                string[] nodeString = line.Split(',');
                int node1Id = int.Parse(nodeString[0].Replace("<",""));
                if (!nodeSet.ContainsKey(node1Id)) nodeSet.Add(node1Id, new Model.Node(node1Id));
                int node2Id = int.Parse(nodeString[1].Replace(">", ""));
                if (!nodeSet.ContainsKey(node2Id)) nodeSet.Add(node2Id, new Model.Node(node2Id));
                int fiberNum = int.Parse(nodeString[3]);

                string linkKey = node1Id + "," + node2Id;
                for (int i = 0; i < fiberNum; i++)
                {
                    Model.Link link = new Model.Link(linkIndex);
                    if (linkDic.ContainsKey(linkKey))
                    {
                        linkDic[linkKey].Add(link);
                    }
                    else
                    {
                        List<Model.Link> linkList = new List<Model.Link>();
                        linkList.Add(link);
                        linkDic.Add(linkKey, linkList);
                    }
                    linkIndex++;
                }
                line = readFileGreedy.ReadLine();
            } while (!line.StartsWith("[<"));
            readFileGreedy.Close();

            //read files
            System.IO.StreamReader readFile = new System.IO.StreamReader(fileName);

            // Write the string to a file.
            System.IO.StreamWriter writeToFile = new System.IO.StreamWriter(fileName.Replace("result", "greedy"), true);

            line = readFile.ReadLine();
            string pathLine = "[";
            while (line != null)
            {
                line = line.Replace("],[", "|");
                line = line.Replace("[", "").Replace("]", "");
                string[] paths = line.Split('|');

                pathLine += readOnePathForMultiFiber(paths, linkDic);
                line = readFile.ReadLine();
                if (line != null) pathLine += ",";
                else pathLine += "]";
                writeToFile.WriteLine(pathLine);
                pathLine = "";
            }

            writeToFile.Close();
            readFile.Close();
        }

        /// <summary>
        /// Check the number of fibers between every two nodes, consider different fiber as different edge
        /// </summary>
        /// <param name="path">The given path to check</param>
        /// <param name="linkDic">The link set of the given graph. Key is 'node1Id,node2Id'</param>
        /// <returns>A string of paths</returns>
        private string readOnePathForMultiFiber(string[] path, Dictionary<string, List<Model.Link>> linkDic)
        {
            string pathString = "[";
            for (int pathIndex = 0; pathIndex < path.Length; pathIndex++)
            {
                string[] nodeString = path[pathIndex].Split(',');
                int i = 0, j = 1;
                List<Model.Path> pathList = new List<Model.Path>();
                pathList.Add(new Model.Path());

                while (j < nodeString.Length)
                {
                    string linkKey1 = nodeString[i] + "," + nodeString[j];
                    string linkKey2 = nodeString[j] + "," + nodeString[i];

                    string linkKey = linkDic.ContainsKey(linkKey1) ? linkKey1 : linkKey2;
                    copyForMultiFibers(pathList, linkDic[linkKey]);
                    i++;
                    j++;
                }
                //generate path string
                if (pathIndex == 0) pathString += "<" + nodeString[0] + "," + nodeString[nodeString.Length - 1] + ">,";

                for (int n = 0; n < pathList.Count; n++)
                {
                    string singlePath = "[";
                    for (int p = 0; p<pathList[n].Links.Count; p++)
                    {
                        singlePath += pathList[n].Links[p].Id + ",";
                    }
                    singlePath = singlePath.Remove(singlePath.Length - 1);
                    singlePath += "]";
                    pathString += singlePath;
                    if (n < pathList.Count - 1) pathString += ",";
                }

                if (pathIndex == path.Length - 1) pathString += "]";
                else pathString += ",";
            }
            return pathString;
        }


        private void copyForMultiFibers(List<Model.Path> pathList, List<Model.Link> linkList)
        {
            int originalSize = pathList.Count;
            int copyCount = linkList.Count;

            for (int i = 1; i < copyCount; i++)
            {
                for (int j = 0; j < originalSize; j++)
                {
                    Model.Path path = new Model.Path();
                    for (int l = 0; l < pathList[j].Links.Count; l++)
                    {
                        path.AddLinks(pathList[j].Links[l]);
                    }
                    path.AddLinks(linkList[i]);
                    pathList.Add(path);
                }
            }

            for (int i = 0; i < originalSize; i++)
            {
                pathList[i].AddLinks(linkList[0]);
            }
        }

        /// <summary>
        /// Write results into a file
        /// </summary>
        /// <param name="graph">The given graph</param>
        /// <param name="fileName">The name of file to write results</param>
        public void writeGreedyResult(Model.Graph graph, String fileName)
        {
            Common commonMothods = new Common();
            System.IO.StreamWriter writeToFile = new System.IO.StreamWriter(fileName);

            //output demand for each pair
            int grantedDemand = 0;
            int totalDemand = 0;
            int[] slicesPerModulation = new int[graph.ModulationSet.Length];
            int[] demandPerModulation = new int[graph.ModulationSet.Length];
            int numRG = 0;
            writeToFile.Write("{0,8}", "Pair");
            writeToFile.Write("{0,8}", "Demand");
            writeToFile.Write("{0,10}", "Granted");
            writeToFile.Write("{0,15}", "Shortest Path");
            writeToFile.Write("{0,8}", "Length");
            writeToFile.Write("{0,15}", "Aggregated  ");
            writeToFile.WriteLine("Used Path <path.id, path.length, modulation, #slices, #RG>");
            for (int i = 0; i < graph.PairSet.Count; i++)
            {
                Model.Pair pair = graph.PairSet[i];
                writeToFile.Write("{0,8}", "<" + pair.SourceNode.Id + "," + pair.DestinaNode.Id + ">");
                writeToFile.Write("{0,8}", " " + pair.TotalDemand);
                writeToFile.Write("{0,10}", " " + pair.GrantedDemand);
                writeToFile.Write("{0,15}", " " + pair.PathSet[0].Id);
                writeToFile.Write("{0,8}", " " + pair.PathSet[0].Length);

                bool aggregated = pair.AggregatedTraffic.IsGranted;
                writeToFile.Write("{0,15}", aggregated + "  ");

                writeToFile.Write("[");
                for (int j = 0; j < pair.GrantedRequests.Count; j++)
                {
                    Model.Request request = pair.GrantedRequests[j];
                    writeToFile.Write("<" + request.UsedPath.Id + "," + request.UsedPath.Length + ","
                            + request.UsedModulation.Name + "," + request.NumSlicesUsage + "," + request.NumRG + ">");
                    numRG += request.NumRG;
                }
                writeToFile.WriteLine("]");
                
                
                totalDemand += pair.TotalDemand;
                grantedDemand += pair.GrantedDemand;
                writeToFile.WriteLine();
            }
            writeToFile.WriteLine();

            //output link information
            BitArray cloneArray = new BitArray(Model.Graph.numSlices, false);
            writeToFile.WriteLine("Link -> Used Slices");
            foreach (KeyValuePair<int, Model.Link> entry in graph.LinkSet)
            {
                Model.Link link = entry.Value;
                cloneArray.Or(link.AssignedSlices);
                writeToFile.Write("{0,8}", link.Id + "<" + link.Node1.Id + "," + link.Node2.Id + ">");
                int numSlices = commonMothods.GetBitArrayValue(link.AssignedSlices);
                writeToFile.Write(" " + numSlices + ":");
                writeToFile.Write("[");
                int counter = 0;
                for (int i = 0; i < link.AssignedSlices.Count; i++)
                {
                    if (link.AssignedSlices[i] == true)
                    {
                        writeToFile.Write(i);
                        counter++;
                        if (counter < numSlices) writeToFile.Write(",");
                    }
                    
                }
                writeToFile.WriteLine("]");
            }
            writeToFile.WriteLine();

            writeToFile.WriteLine("Total Demand: " + totalDemand);
            writeToFile.WriteLine("Available Slices: " + graph.Slices.Length);
            writeToFile.WriteLine("Granted Demand: " + grantedDemand);
            writeToFile.WriteLine("RG:" + numRG);
            writeToFile.WriteLine("Used Slices: " + commonMothods.GetBitArrayValue(cloneArray));
            writeToFile.WriteLine("CPU Time:" + Program.cpuTime);

            writeToFile.Close();
        }

        /// <summary>
        /// Write results into a file
        /// </summary>
        /// <param name="graph">The given graph</param>
        /// <param name="fileName">The name of file to write results</param>
        public void writeTabuResult(Model.Graph graph, String fileName)
        {
            Common commonMothods = new Common();
            System.IO.StreamWriter writeToFile = new System.IO.StreamWriter(fileName);

            //output demand for each pair
            int grantedDemand = 0;
            int totalDemand = 0;
            int[] slicesPerModulation = new int[graph.ModulationSet.Length];
            int[] demandPerModulation = new int[graph.ModulationSet.Length];
            int numRG = 0;
            writeToFile.Write("{0,8}", "Pair");
            writeToFile.Write("{0,8}", "Demand");
            writeToFile.Write("{0,10}", "Granted");
            writeToFile.Write("{0,15}", "Shortest Path");
            writeToFile.Write("{0,8}", "Length");
            writeToFile.Write("{0,15}", "Aggregated  ");
            writeToFile.WriteLine("Used Path <path.id, path.length, modulation, #slices, #RG>");
            for (int i = 0; i < graph.PairSet.Count; i++)
            {
                Model.Pair pair = graph.PairSet[i];
                writeToFile.Write("{0,8}", "<" + pair.SourceNode.Id + "," + pair.DestinaNode.Id + ">");
                writeToFile.Write("{0,8}", " " + pair.TotalDemand);
                writeToFile.Write("{0,10}", " " + pair.GrantedDemand);
                writeToFile.Write("{0,15}", " " + pair.PathSet[0].Id);
                writeToFile.Write("{0,8}", " " + pair.PathSet[0].Length);

                writeToFile.Write("{0,15}", pair.AggregatedTraffic.IsGranted + "  ");

                writeToFile.Write("[");
                for (int j = 0; j < pair.GrantedRequests.Count; j++)
                {
                    Model.Request request = pair.GrantedRequests[j];
                    writeToFile.Write("<" + request.UsedPath.Id + "," + request.UsedPath.Length + ","
                            + request.UsedModulation.Name + "," + request.NumSlicesUsage + "," + request.NumRG + ">");
                    numRG += request.NumRG;
                }
                writeToFile.WriteLine("]");
                
                totalDemand += pair.TotalDemand;
                grantedDemand += pair.GrantedDemand;
                writeToFile.WriteLine();
            }
            writeToFile.WriteLine();

            //output link information
            BitArray cloneArray = new BitArray(Model.Graph.numSlices, false);
            writeToFile.WriteLine("Link -> Used Slices");
            foreach (KeyValuePair<int, Model.Link> entry in graph.LinkSet)
            {
                Model.Link link = entry.Value;
                cloneArray.Or(link.AssignedSlices);
                writeToFile.Write("{0,8}", link.Id + "<" + link.Node1.Id + "," + link.Node2.Id + ">");
                int numSlices = commonMothods.GetBitArrayValue(link.AssignedSlices);
                writeToFile.Write(" " + numSlices + ":");
                writeToFile.Write("[");
                int counter = 0;
                for (int i = 0; i < link.AssignedSlices.Count; i++)
                {
                    if (link.AssignedSlices[i] == true)
                    {
                        writeToFile.Write(i);
                        counter++;
                        if (counter < numSlices) writeToFile.Write(",");
                    }
                    
                }
                writeToFile.WriteLine("]");
            }
            writeToFile.WriteLine();

            writeToFile.WriteLine("Path information");
            foreach(KeyValuePair<int, Model.Path> entry in graph.PathSet)
            {
                Model.Path path = entry.Value;
                Model.Pair pair = path.NodePair;

                if (path.UsedByRequests.Count <= 0) continue;

                writeToFile.Write(path.Id + "; Pair <" + pair.SourceNode.Id + "," + pair.DestinaNode.Id + ">; Requests: [");
                for (int r = 0; r < path.UsedByRequests.Count; r++)
                {
                    Model.Request request = path.UsedByRequests[r];
                    writeToFile.Write("<" + request.Id + "," + request.Demand + "," + request.SliceStartIndex + "," + request.NumSlicesUsage + ">");
                }
                writeToFile.WriteLine("]");

                //link info
                writeToFile.Write("links:<");
                for (int l = 0; l < path.Links.Count; l++)
                {
                    if (l > 0) writeToFile.Write(",");
                    writeToFile.Write(path.Links[l].Id);
                }
                writeToFile.WriteLine(">");
                //slices info
                writeToFile.Write("Slices:<");
                for (int s = 0; s < path.AssignedSlices.Count; s++)
                {
                    if (path.AssignedSlices[s])
                        writeToFile.Write(s + ",");
                }
                writeToFile.WriteLine(">");
                writeToFile.WriteLine();
            }

            writeToFile.WriteLine("Available Slices on each path");
            for (int i = 0; i < graph.PairSet.Count; i++)
            {
                Model.Pair pair = graph.PairSet[i];
                writeToFile.WriteLine("Pair <" + pair.SourceNode.Id + "," + pair.DestinaNode.Id + ">: ");
                for (int j = 0; j < pair.PathSet.Count; j++)
                {
                    Model.Path path = pair.PathSet[j];
                    writeToFile.Write("Path " + path.Id + " : ");
                    cloneArray = new BitArray(Model.Graph.numSlices, false);
                    for (int l = 0; l < path.Links.Count; l++)
                    {
                        cloneArray.Or(path.Links[l].AssignedSlices);
                    }
                    int availableNum = 0;
                    int preIndex = 0;
                    string availableSlices = "[<";
                    for (int s = 0; s < cloneArray.Count; s++)
                    {
                        if (!cloneArray[s])
                        {
                            availableNum++;
                            if (preIndex + 1 == s && preIndex > 0) availableSlices += ",";
                            else if (preIndex > 0) availableSlices += "><";
                            availableSlices += s;
                            preIndex = s;
                        }
                    }
                    availableSlices += ">]";
                    writeToFile.WriteLine(availableNum + " " + availableSlices);
                }
                writeToFile.WriteLine();
            }

            //Requests information
            writeToFile.WriteLine("RequestId, <sourceNodeId, destinationNodeId>, <sliceStartIndex, slicesNum>, <Modulation, RG>, <PathId, length, [linkIds]>");
            for (int i = 0; i < graph.RequestSet.Count; i++)
            {
                Model.Request request = graph.RequestSet[i];
                if (request.IsGranted)
                {
                    writeToFile.Write(request.Id + ",<" + request.NodePair.SourceNode.Id + "," + request.NodePair.DestinaNode.Id + ">");
                    writeToFile.Write(",<" + request.SliceStartIndex + "," + request.NumSlicesUsage + ">,<" + request.UsedModulation.Name + "," + request.NumRG + ">");
                    writeToFile.Write(",<"+request.UsedPath.Id + "," + request.UsedPath.Length + ",[");
                    for (int j = 0; j < request.UsedPath.Links.Count; j++)
                    {
                        Model.Link link = request.UsedPath.Links[j];
                        if (j > 0) writeToFile.Write(",");
                        writeToFile.Write(link.Id);
                    }
                    writeToFile.WriteLine("]>");
                }
                writeToFile.WriteLine();
            }

            writeToFile.WriteLine("Total Demand: " + totalDemand);
            writeToFile.WriteLine("Available Slices: " + graph.Slices.Length);
            writeToFile.WriteLine("Granted Demand: " + grantedDemand);
            writeToFile.WriteLine("RG:" + numRG);
            writeToFile.WriteLine("Used Slices: " + commonMothods.GetBitArrayValue(cloneArray));
            writeToFile.WriteLine("CPU Time:" + Program.cpuTime);

            writeToFile.Close();
        }

        public void writeLenghtCurveForPairs(Model.Graph graph, String fileName)
        {
            System.IO.StreamWriter writeToFile = new System.IO.StreamWriter(fileName);

            writeToFile.WriteLine("<Source,Destination> L_SP L_Used");
            for (int i = 0; i < graph.PairSet.Count; i++)
            {
                Model.Pair pair = graph.PairSet[i];

                writeToFile.Write("<" + pair.SourceNode.Id + "," + pair.DestinaNode.Id + "> " + pair.PathSet[0].Length);
                if (pair.IsAllDemandAssigned) writeToFile.Write(" " + pair.AggregatedTraffic.UsedPath.Length);
                writeToFile.WriteLine();
            }

                writeToFile.Close();
        }

        public void writeDemandCurveForPairs(Model.Graph graph, String fileName)
        {
            System.IO.StreamWriter writeToFile = new System.IO.StreamWriter(fileName);

            writeToFile.WriteLine("<Source,Destination> Total Granted");
            for (int i = 0; i < graph.PairSet.Count; i++)
            {
                Model.Pair pair = graph.PairSet[i];

                writeToFile.Write("<" + pair.SourceNode.Id + "," + pair.DestinaNode.Id + "> " + pair.TotalDemand);
                if (pair.IsAllDemandAssigned) writeToFile.Write(" " + pair.GrantedDemand);
                writeToFile.WriteLine();
            }

            writeToFile.Close();
        }

        public void writeModulationChoicesSumaryForAggregatedTraffic(Model.Graph graph, String fileName)
        {
            Common commonMothods = new Common();
            System.IO.StreamWriter writeToFile = new System.IO.StreamWriter(fileName);
            
            Dictionary<int, int> preferedModulationRG = new Dictionary<int,int>();
            Dictionary<int, int> preferedModulationSlices = new Dictionary<int,int>();


            for (int i = 0; i < graph.PairSet.Count; i++)
            {
                Model.Request request = graph.PairSet[i].AggregatedTraffic;
                int minRG_RG = int.MaxValue;
                int minSlicesNum_RG = int.MaxValue;
                int minRG_slices = int.MaxValue;
                int minSlicesNum_slices = int.MaxValue;
                Model.Modulation bestModutionRG = null;
                Model.Modulation bestModutionSlices = null;
                for (int j = 0; j < graph.ModulationSet.Length; j++)
                {
                    Model.Modulation modulation = graph.ModulationSet[j];
                    int numRG = commonMothods.GetRGNum(request.NodePair.PathSet[0], modulation);
                    int slicesNum = commonMothods.GetSlicesNum(request, modulation);
                    //Prefer less RG
                    if (numRG < minRG_RG) 
                    {
                        minRG_RG = numRG;
                        minSlicesNum_RG = slicesNum;
                        bestModutionRG = modulation;
                    }
                    else if (numRG == minRG_RG && slicesNum <= minSlicesNum_RG)
                    {
                        minSlicesNum_RG = slicesNum;
                        bestModutionRG = modulation;
                    }
                    //RG free, prefer less #slices
                    if (slicesNum < minSlicesNum_slices)
                    {
                        minSlicesNum_slices = slicesNum;
                        minRG_slices = numRG;
                        bestModutionSlices = modulation;
                    }
                    else if (numRG <= minRG_slices && slicesNum == minSlicesNum_slices)
                    {
                        minRG_slices = numRG;
                        bestModutionSlices = modulation;
                    }
                }

                preferedModulationRG.Add(request.Id, bestModutionRG.Id);
                preferedModulationSlices.Add(request.Id, bestModutionSlices.Id);
            }

            //write table for less RG
            writeToFile.WriteLine("Less RG first");
            writeToFile.WriteLine("Name #Requests #Demand");
            int[,] modu_predic = new int[graph.ModulationSet.Length, 2];
            int[,] modu_used = new int[graph.ModulationSet.Length, 2];
            foreach(KeyValuePair<int, int> entry in preferedModulationRG)
            {
                Model.Request request = graph.RequestSet[entry.Key];
                modu_predic[entry.Value, 0]++;
                modu_predic[entry.Value, 1] += request.Demand;

                if (request.IsGranted)
                {
                    modu_used[request.UsedModulation.Id, 0]++;
                    modu_used[request.UsedModulation.Id, 1] += request.Demand;
                }
                
            }

            for(int i = 0; i < modu_predic.GetLength(0); i++)
            {
                writeToFile.WriteLine(graph.ModulationSet[i].Name + " " + modu_predic[i, 0] + "/" + modu_used[i, 0] + " " + modu_predic[i, 1] + "/" + modu_predic[i, 1]);
            }

            //write table for less slices, RG free
            writeToFile.WriteLine("Less Slices first");
            writeToFile.WriteLine("Name #Requests #Demand");
            modu_predic = new int[graph.ModulationSet.Length, 2];
            modu_used = new int[graph.ModulationSet.Length, 2];
            foreach (KeyValuePair<int, int> entry in preferedModulationSlices)
            {
                Model.Request request = graph.RequestSet[entry.Key];
                modu_predic[entry.Value, 0]++;
                modu_predic[entry.Value, 1] += request.Demand;

                if (request.IsGranted)
                {
                    modu_used[request.UsedModulation.Id, 0]++;
                    modu_used[request.UsedModulation.Id, 1] += request.Demand;
                }
            }

            for (int i = 0; i < modu_predic.GetLength(0); i++)
            {
                writeToFile.WriteLine(graph.ModulationSet[i].Name + " " + modu_predic[i, 0] + "/" + modu_used[i, 0] + " " + modu_predic[i, 1] + "/" + modu_predic[i, 1]);
            }

            //write requests infomation for less RG preferance
            writeToFile.WriteLine("Less RG first");
            writeToFile.WriteLine("<Source,Destination>; Prefered Choices; Usage Information");
            foreach(KeyValuePair<int, int> entry in preferedModulationRG)
            {
                Model.Request request = graph.RequestSet[entry.Key];
                Model.Modulation modulation = graph.ModulationSet[entry.Value];
                writeToFile.Write("<" + request.NodePair.SourceNode.Id + "," + request.NodePair.DestinaNode.Id + ">; " + modulation.Name + " " 
                    + commonMothods.GetRGNum(request.NodePair.PathSet[0], modulation) + " " + commonMothods.GetSlicesNum(request, modulation));
                if (request.IsGranted)
                {
                    writeToFile.Write("; " + request.UsedModulation.Name + " " + request.NumRG + " " + request.NumSlicesUsage);
                }
                writeToFile.WriteLine();
            }

            //write requests infomation for less RG preferance
            writeToFile.WriteLine("Less Slices first");
            writeToFile.WriteLine("<Source,Destination>; Prefered Choices; Usage Information");
            foreach (KeyValuePair<int, int> entry in preferedModulationSlices)
            {
                Model.Request request = graph.RequestSet[entry.Key];
                Model.Modulation modulation = graph.ModulationSet[entry.Value];
                writeToFile.Write("<" + request.NodePair.SourceNode.Id + "," + request.NodePair.DestinaNode.Id + ">; " + modulation.Name + " "
                    + commonMothods.GetRGNum(request.NodePair.PathSet[0], modulation) + " " + commonMothods.GetSlicesNum(request, modulation));
                if (request.IsGranted)
                {
                    writeToFile.Write("; " + request.UsedModulation.Name + " " + request.NumRG + " " + request.NumSlicesUsage);
                }
                writeToFile.WriteLine();
            }
        }

        public void writeHopsTrafficGraph(Model.Graph graph, String fileName)
        {
            System.IO.StreamWriter writeToFile = new System.IO.StreamWriter(fileName);
            //Key is num of hops
            Dictionary<int, int> GoS = new Dictionary<int, int>();
            Dictionary<int, int> Throughput = new Dictionary<int, int>();

            writeToFile.WriteLine("Hops, GoS, Throughput");
            for (int i = 0; i < graph.RequestSet.Count; i++)
            {
                Model.Request request = graph.RequestSet[i];
                if (request.IsGranted)
                {
                    int hops = request.UsedPath.Links.Count;
                    if (!GoS.ContainsKey(hops)) GoS.Add(hops, 1);
                    else GoS[hops] += 1;

                    if (!Throughput.ContainsKey(hops)) Throughput.Add(hops, request.Demand);
                    else Throughput[hops] += request.Demand;
                }
            }

            foreach (KeyValuePair<int, int> entry in GoS) 
            {
                int hops = entry.Key;
                writeToFile.WriteLine(hops + "," + GoS[hops] + "," + Throughput[hops]);
            }

            writeToFile.Close();
        }
    }
}

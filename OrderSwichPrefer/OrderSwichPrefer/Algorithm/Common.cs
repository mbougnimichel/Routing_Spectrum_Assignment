using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderSwichPrefer.Algorithm
{
    class Common
    {
        /// <summary>
        /// Calculate how many elements are set as 'true' in a given bitarray
        /// </summary>
        /// <param name="elements">The given bitarray</param>
        /// <returns></returns>
        public int GetBitArrayValue(BitArray elements)
        {
            int value = 0;

            foreach(Boolean element in elements)
            {
                if(element) value++;
            }

            return value;
        }

        /// <summary>
        /// Find available slices in the given range of the slices usage table for a particular reqeust
        /// </summary>
        /// <param name="slicesUsage">The slices usage table</param>
        /// <param name="linkUsage">The bitarray indicate the links used by the given request</param>
        /// <param name="numSlices">The number of slices used by the given reqeust</param>
        /// <param name="startSliceIndex">The start index (included) of the given searching range</param>
        /// <param name="endSliceIndex">The end index (not included) of the given searching range</param>
        /// <returns>The start index of available slices</returns>
        public int FindAvailableSlicesForRequest(BitArray[] slicesUsage, BitArray linkUsage, int numSlices, int startSliceIndex, int endSliceIndex, int[,] slicesReqeustsTable, Dictionary<int, Model.Request> requests, Model.Request request)
        {
            int count = 0;
            for (int i = startSliceIndex; i < endSliceIndex; i++)
            {
                BitArray usedSlices = null;
                bool availableFlag = true;

                if (i > 0 && count == 0)
                {
                    usedSlices = slicesUsage[i - 1];
                    for (int j = 0; j < usedSlices.Length; j++)
                    {
                        if (linkUsage[j] && usedSlices[j])
                        {
                            Model.Request preRequest = requests[slicesReqeustsTable[j, i - 1]];
                            if (preRequest.NodePair != request.NodePair || preRequest.UsedPath != request.UsedPath)
                            {
                                availableFlag = false;
                                count = 0;
                                break;
                            }
                        }
                    }                }

                if (!availableFlag) continue;

                usedSlices = slicesUsage[i];
                count++;
                for (int j = 0; j < usedSlices.Length; j++)
                {
                    if (linkUsage[j] && usedSlices[j])
                    {
                        count = 0;
                        break;
                    }
                }

                if (count == numSlices && i < slicesUsage.Length - 1)
                {
                    usedSlices = slicesUsage[i + 1];
                    for (int j = 0; j < usedSlices.Length; j++)
                    {
                        if (linkUsage[j] && usedSlices[j])
                        {
                            Model.Request nextRequest = requests[slicesReqeustsTable[j, i + 1]];
                            if (nextRequest.NodePair != request.NodePair || nextRequest.UsedPath != request.UsedPath)
                            {
                                availableFlag = false;
                                count = 0;
                                break;
                            }
                        }
                    }
                }

                if (count == numSlices) return i - count + 1;
            }

            return -1;
        }

        /// <summary>
        /// Generate a slices-requests relation table
        /// </summary>
        public int[,] GenerateSlicesRequestsTable(List<Model.Request> grantedRequests, int linkNum)
        {
            int[,] slicesReqeustsTable = new int[linkNum, Model.Graph.numSlices];
            for (int i = 0; i < slicesReqeustsTable.GetLength(0); i++)
            {
                for (int j = 0; j < slicesReqeustsTable.GetLength(1); j++)
                {
                    slicesReqeustsTable[i, j] = -1;
                }
            }

            for (int i = 0; i < grantedRequests.Count; i++)
            {
                Model.Request request = grantedRequests[i];
                foreach (Model.Link link in request.UsedPath.Links)
                {
                    int index = request.SliceStartIndex;
                    int endIndex = index + request.NumSlicesUsage;
                    while (index < endIndex)
                    {
                        slicesReqeustsTable[link.Id, index] = request.Id;
                        index++;
                    }

                }
            }
            return slicesReqeustsTable;
        }

        /// <summary>
        /// Generate a slices usage table;
        /// The slices usage information on links must be updated!!!
        /// </summary>
        public BitArray[] GenerateSlicesUsageTable(Dictionary<int, Model.Link> links)
        {
            BitArray[] slicesUsageTable = new BitArray[Model.Graph.numSlices];
            for (int i = 0; i < Model.Graph.numSlices; i++)
            {
                slicesUsageTable[i] = new BitArray(links.Count, false);
            }

            for (int i = 0; i < links.Count; i++)
            {
                Model.Link link = links[i];
                for (int j = 0; j < link.AssignedSlices.Count; j++)
                {
                    if (link.AssignedSlices[j]) slicesUsageTable[j][link.Id] = true;
                }
            }
            return slicesUsageTable;
        }

        /// <summary>
        /// Get all the denied aggregated traffic, put them into deniedRequests dictionary;
        /// Put granted traffic into grantedRequestList in an order
        /// </summary>
        /// <param name="aggregated">Whether we are dealing with aggregated traffic</param>
        public void GetDeniedAndGrantedRequests(bool aggregated, List<Model.Pair> pairList, List<Model.Request> grantedRequests, Dictionary<int, Model.Request> deniedRequests)
        {
            for (int i = 0; i < pairList.Count; i++)
            {
                Model.Pair pair = pairList[i];
                grantedRequests.AddRange(pair.GrantedRequests);

                if (aggregated)
                {
                    if (!pair.IsAllDemandAssigned) deniedRequests.Add(pair.AggregatedTraffic.Id, pair.AggregatedTraffic);
                }
                else
                {
                    if (pair.GrantedDemand < pair.TotalDemand)
                    {
                        AddSeparatedRequestIntoDeny(pair, deniedRequests);
                    }
                }
            }
        }

        /// <summary>
        /// Add separated requests into deniedRequest dictionary
        /// </summary>
        /// <param name="pair"></param>
        public void AddSeparatedRequestIntoDeny(Model.Pair pair, Dictionary<int , Model.Request> deniedRequests)
        {
            for (int i = 0; i < pair.RequestSet.Length; i++)
            {
                Model.Request request = pair.RequestSet[i];
                if (!request.IsGranted && !deniedRequests.ContainsKey(request.Id)) deniedRequests.Add(request.Id, request);
            }
        }

        /// <summary>
        /// Insert a given reqeust into slicesReqesutsTable and slicesUsageTable;
        /// Update the given request information, but not accosiated pair, path, links and graph
        /// </summary>
        /// <param name="request">The given reqeust</param>
        /// <param name="path">The path used by the given reqeust</param>
        /// <param name="modulation">The modulation used by the given reqeust</param>
        /// <param name="startIndex">The slices started index of the given reqeust</param>
        /// <param name="numRG">The number of RG needed by the given reqeust</param>
        public void InsertTraffic(Model.Request request, Model.Path path, Model.Modulation modulation, int startIndex, int numSlices, int numRG, BitArray[] slicesUsage, int[,] requestGrantedTable)
        {
            request.IsGranted = true;
            request.UsedPath = path;
            request.UsedModulation = modulation;
            request.NumSlicesUsage = numSlices;
            request.SliceStartIndex = startIndex;
            request.NumRG = numRG;

            int endIndex = startIndex + numSlices;
            int id = request.Id;
            for (int i = startIndex; i < endIndex; i++)
            {
                foreach (Model.Link link in request.UsedPath.Links)
                {
                    requestGrantedTable[link.Id, i] = id;
                    slicesUsage[i][link.Id] = true;
                }
            }
        }

        /// <summary>
        /// Create a new array of bitarray. The length of new array equal to the size 
        /// of each bitarray in the given array, and the size of each bitarray in the
        /// new array is equal the the length of the given array.
        /// </summary>
        /// <param name="originalBitArray">The given array of bitarray. In this array, 
        /// the size of each bitarray must be same.</param>
        /// <returns></returns>
        public BitArray[] SwitchDimension(BitArray[] originalBitArray)
        {
            BitArray[] newBitArray = new BitArray[originalBitArray[0].Count];

            for (int i = 0; i < newBitArray.Length; i++)
            {
                newBitArray[i] = new BitArray(originalBitArray.Length);
                for (int j = 0; j < originalBitArray.Length; j++)
                {
                    newBitArray[i][j] = originalBitArray[j][i];
                }
            }

            return newBitArray;
        }

        /// <summary>
        /// Sort (source,distination) pairs by the length of their shoretest path(decreasing); 
        /// break the tie by their traffic demand(decreasing)
        /// </summary>
        /// <param name="pairSet">The set of pairs</param>
        /// <returns>The ordered pair array</returns>
        public Model.Pair[] SortByShortestPath(List<Model.Pair> pairSet)
        {
            pairSet.Sort(delegate(Model.Pair pair1, Model.Pair pair2)
            {
                int length1 = pair1.PathSet[0].Length;
                int length2 = pair2.PathSet[0].Length;
                if (length1 < length2) return 1;
                else if (length1 > length2) return -1;
                else
                {
                    int demand1 = pair1.TotalDemand;
                    int demand2 = pair2.TotalDemand;
                    if (demand1 > demand2) return -1;
                    else if (demand1 < demand2) return 1;
                    return 0;
                }
            });
            Model.Pair[] pairOrder = pairSet.ToArray();
            return pairOrder;
        }

        /// <summary>
        /// Sort (source,distination) pairs by their traffic demand(decreasing); 
        /// break the tie by the lenght of their shortest path(decreasing)
        /// </summary>
        /// <param name="pairSet">The set of pairs</param>
        /// <returns>The ordered pair array</returns>
        public Model.Pair[] SortByDemand(List<Model.Pair> pairSet)
        {
            pairSet.Sort(delegate(Model.Pair p1, Model.Pair p2)
            {
                int demand1 = p1.TotalDemand;
                int demand2 = p2.TotalDemand;
                if (demand1 > demand2) return -1;
                else if (demand1 < demand2) return 1;
                else 
                {
                    int length1 = p1.PathSet[0].Length;
                    int length2 = p2.PathSet[0].Length;
                    if (length1 < length2) return 1;
                    else if (length1 > length2) return -1;
                    return 0;
                }
            });
            Model.Pair[] pairOrder = pairSet.ToArray();
            return pairOrder;
        }

        /// <summary>
        /// Select candidate modulations for a given path on a given path that needs less RGs than a given number
        /// </summary>
        /// <param name="path">The given path</param>
        /// <param name="modulations">The set of mulations</param>
        /// <param name="allowRG">The given number of RG</param>
        /// <returns>The candidate modulations, and the number of RG for each modulations</returns>
        public List<Tuple<Model.Modulation, int>> SelectModulations(Model.Path path, Model.Modulation[] modulations, int? allowRG)
        {
            if (!allowRG.HasValue) allowRG = int.MaxValue;

            List<Tuple<Model.Modulation, int>> selectedModulation = new List<Tuple<Model.Modulation, int>>(); ;
            for (int m = 0; m < modulations.Length; m++)
            {
                if (allowRG < 0) break;
                int RGCounter = 0;
                Model.Modulation modulation = modulations[m];
                int numRG = (int)Math.Floor(path.Length * 1.00 / modulation.Reach);

                if (numRG <= allowRG)
                {
                    //Go through all the links on the path, make sure the needed number of RG is indeed 
                    //equal to allowRG
                    bool isCandidate = true;
                    int length = 0;
                    Model.Link previousLink = null;
                    for (int l = 0; l < path.Links.Count; l++)
                    {
                        Model.Link currentLink = path.Links[l];
                        length += currentLink.Length;
                        //When length larger than modulation's reach, add a regenerator
                        if (length >= modulation.Reach)
                        {
                            //The length of first link larger than modulation's reach, no need to go on
                            if (previousLink == null)
                            {
                                isCandidate = false;
                                break;
                            }
                            else
                            {
                                //Get the node need to equip a regenerator
                                RGCounter++;
                                length = 0;
                                //The number of regenerator add already larger than what allowed, no need to go on
                                if (RGCounter > allowRG)
                                {
                                    isCandidate = false;
                                    break;
                                }
                            }
                        }
                        previousLink = currentLink;
                    }
                    if (isCandidate)
                    {
                        allowRG = RGCounter - 1;
                        selectedModulation.Add(new Tuple<Model.Modulation, int>(modulations[m], RGCounter));
                    }

                }
            }
            return selectedModulation;
        }

        /// <summary>
        /// Determine whether two BitArray are equal
        /// </summary>
        /// <param name="array1">The first BitArray</param>
        /// <param name="array2">The second BitArray</param>
        /// <returns>True for equal; False for not equal</returns>
        public bool Equals(BitArray array1, BitArray array2)
        {
            if (array1.Count != array2.Count) return false;
            else 
            {
                for (int i = 0; i < array1.Count; i++)
                {
                    if (array1[i] != array2[i]) return false;
                }
            }
            return true;
        }


        public String BitArrayToString(BitArray array)
        {
            String arrayString = "";
            foreach(bool bit in array)
            {
                if(bit) arrayString += "1,";
                else arrayString += "0,";
            }
            arrayString = arrayString.Remove(arrayString.Length - 1);
            return arrayString;
        }

        public String SwitchListToBinaryString(List<Model.Link> linkList, int linksNum)
        {
            String binaryString = "";

            int[] binaryArray = new int[linksNum];
            foreach (Model.Link link in linkList)
            {
                binaryArray[link.Id] = 1;
            }
            for (int i = 0; i < binaryArray.Length; i++)
            {
                binaryString += binaryArray[i] + ",";
            }
            binaryString = binaryString.Remove(binaryString.Length - 1);
            return binaryString;
        }

        /// <summary>
        /// Calculate the number of RG for a given path by using a given modulation
        /// </summary>
        /// <param name="path">The given path</param>
        /// <param name="modulation">The given modulation</param>
        /// <returns></returns>
        public int GetRGNum(Model.Path path, Model.Modulation modulation)
        {
            int length = 0;
            Model.Link previousLink = null;
            int RGCounter = 0;
            for (int l = 0; l < path.Links.Count; l++)
            {
                Model.Link currentLink = path.Links[l];
                length += currentLink.Length;
                //When length larger than modulation's reach, add a regenerator
                if (length >= modulation.Reach)
                {
                    //The length of first link larger than modulation's reach, no need to go on
                    if (previousLink == null)
                    {
                        return -1;
                    }
                    else
                    {
                        //Get the node need to equip a regenerator
                        RGCounter++;
                        length = 0;
                    }
                }
                previousLink = currentLink;
            }
            return RGCounter;
        }

        /// <summary>
        /// Calculate the number of slices needed to grant a given request by using a given modulation
        /// </summary>
        /// <param name="request">The given request</param>
        /// <param name="modulation">The given modulation</param>
        /// <returns></returns>
        public int GetSlicesNum(Model.Request request, Model.Modulation modulation)
        {
            int slicesNum = 0;
            slicesNum = ((int)Math.Ceiling(request.Demand * 1.00 / modulation.Speed)) * modulation.NumSlices;
            return slicesNum;
        }

        /// <summary>
        /// Use separated requests instead of aggrageted requests for granted traffic
        /// </summary>
        /// <param name="pairSet">The pairs on the given network</param>
        public void SeparateRequestsFromAggrageted(List<Model.Pair> pairSet)
        {
            foreach(Model.Pair pair in pairSet)
            {
                if (pair.GrantedRequests.Count == 1 && pair.GrantedRequests[0] == pair.AggregatedTraffic)
                {
                    Model.Request aggRequest = pair.AggregatedTraffic;
                    pair.GrantedRequests.Clear();
                    int startIndex = aggRequest.SliceStartIndex;
                    for(int i=0;i<pair.RequestSet.Length;i++)
                    {
                        Model.Request sepRequest = pair.RequestSet[i];
                        sepRequest.UsedPath = aggRequest.UsedPath;
                        sepRequest.UsedModulation = aggRequest.UsedModulation;
                        sepRequest.NumRG = aggRequest.NumRG;
                        sepRequest.NumSlicesUsage = GetSlicesNum(sepRequest, aggRequest.UsedModulation);
                        sepRequest.SliceStartIndex = startIndex;
                        sepRequest.IsGranted = true;

                        startIndex += sepRequest.NumSlicesUsage;
                        pair.GrantedRequests.Add(sepRequest);
                    }

                    aggRequest.IsGranted = false;
                    aggRequest.NumRG = 0;
                    aggRequest.NumSlicesUsage = 0;
                    aggRequest.SliceStartIndex = 0;
                    aggRequest.UsedModulation = null;
                    aggRequest.UsedPath = null;
                }
            }
        }
    }
}

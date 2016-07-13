using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace OrderSwichPrefer.Algorithm
{
    class OrderSwichGreedy
    {
        private Dictionary<int, Model.Request> deniedRequests;
        private Model.Graph graph;
        private Common commonMethod;
        //[link, slice]
        private int[,] slicesReqeustsTable;
        //[slice][link]
        private BitArray[] slicesUsageTable;
        private List<Model.Request> grantedRequestList;
        private List<Model.Request> bestSolution;
        private StreamWriter Logger;
        private Stopwatch stopWatch;
        private Dictionary<string, int> forbiddenSet;

        private List<int> throughputList;
        private List<int> GoSAgg;
        private List<int> GoSSep;
        private List<int> positiveMovesNum;

        public OrderSwichGreedy(Model.Graph graph, String logFileName) 
        {
            ConfigLogger(logFileName);
            this.graph = graph;
            this.commonMethod = new Common();
        }

        private void ConfigLogger(String logFileName)
        {
            Logger = new StreamWriter(logFileName);
            stopWatch = new Stopwatch();
        }

        public void Close()
        {
            if (Logger != null) Logger.Close();
        }

        private void PrepareAlgorithm(bool aggregated)
        {
            if (!aggregated) commonMethod.SeparateRequestsFromAggrageted(graph.PairSet);

            forbiddenSet = new Dictionary<string, int>();
            //Get all the denied requests
            deniedRequests = new Dictionary<int, Model.Request>();
            grantedRequestList = new List<Model.Request>();
            commonMethod.GetDeniedAndGrantedRequests(aggregated, graph.PairSet, grantedRequestList, deniedRequests);

            //Genearate Slices Usage Table
            slicesReqeustsTable = commonMethod.GenerateSlicesRequestsTable(grantedRequestList, graph.LinkSet.Count);
            slicesUsageTable = commonMethod.GenerateSlicesUsageTable(graph.LinkSet);

            bestSolution = new List<Model.Request>();
            UpdateBestSolution();
        }

        public void RunAlgorithm(bool aggregated)
        {
            PrepareAlgorithm(aggregated);
            //Main Algorithm
            graph.GrantedDemand = SwichOrder(aggregated);

            //Update network information with the best solution
            UpdateGlobalInformation();
        }

        public void PrintOutThroughputForIterations(String fileName) 
        {
            StreamWriter throughputLogger = new StreamWriter(fileName);
            throughputLogger.WriteLine("Iteration, Throughput, GoSAgg, GoSSep");
            for (int i = 0; i < throughputList.Count; i++) 
            {
                throughputLogger.WriteLine(i + ", " + throughputList[i] + ", " + GoSAgg[i] + ", " + GoSSep[i]);
            }
            throughputLogger.Close();
        }

        public void PrintOutPositiveMovesForIterations(String fileName)
        {
            StreamWriter positiveMovesLogger = new StreamWriter(fileName);
            positiveMovesLogger.WriteLine("Iteration, Positive Moves Num");
            for (int i = 0; i < positiveMovesNum.Count; i++)
            {
                positiveMovesLogger.WriteLine(i + ", " + positiveMovesNum[i]);
            }
            positiveMovesLogger.Close();
        }

        /// <summary>
        /// Main part for the Algorithm
        /// </summary>
        /// <returns></returns>
        private int SwichOrder(bool aggreagated)
        {
            Random random = new Random();

            int noImproveNum = 0;
            int bestThroughput = graph.GrantedDemand;
            int currentThroughput = bestThroughput;
            double cpuTime = 0;
            int iteration = 0;

            Console.WriteLine("Start to switch reqeusts order...");
            Logger.WriteLine("Start to switch reqeusts order...");
            Console.WriteLine();
            Logger.WriteLine();
            throughputList = new List<int>();
            GoSAgg = new List<int>();
            GoSSep = new List<int>();
            throughputList.Add(currentThroughput);
            GetGoS();
            positiveMovesNum = new List<int>();

            while (noImproveNum < Program.META_ITERATION_NUM && currentThroughput < graph.TotalDemand)
            {
                Console.WriteLine("No improve iteration: " + noImproveNum);
                Logger.WriteLine("No improve iteration: " + noImproveNum);

                Model.Request bestReqeust = null;
                int bestStartSliceIndex = -1;
                Model.Modulation bestModulation = null;
                Model.Path bestPath = null;
                int bestRG = 0;
                int bestNumSlices = 0;
                int minRemovalTraffic = int.MaxValue;

                List<int> positiveMoveRequestId = new List<int>();

                //Get best request to insert into table
                Console.WriteLine("Granted reqeusts: " + grantedRequestList.Count + "; Denied reqeusts: " + deniedRequests.Count);
                Logger.WriteLine("Granted reqeusts: " + grantedRequestList.Count + "; Denied reqeusts: " + deniedRequests.Count);
                stopWatch.Start();
                foreach(KeyValuePair<int, Model.Request> entry in deniedRequests)
                {
                    Model.Request request = entry.Value;

                    //if (forbiddenSet.ContainsKey(generateForbiddenKey(request.Id))) continue;

                    Console.WriteLine("Trying to find best position for request:" + request.Id);
                    for (int p = 0; p < request.NodePair.PathSet.Count; p++)
                    {
                        Model.Path path = request.NodePair.PathSet[p];
                        if (forbiddenSet.ContainsKey(generateForbiddenKey(request.NodePair.SourceNode.Id, request.NodePair.DestinaNode.Id, path.Id))) continue;

                        int minSlicesNum = int.MaxValue;
                        /*********************************************************************************************************** 
                         * The order of modulation is really important!!! Should be ordered as necessary slices will always be increasing
                         ***********************************************************************************************************/
                        for (int m = 0; m < graph.ModulationSet.Length; m++)
                        {
                            Model.Modulation modulation = graph.ModulationSet[m];
                            int RGNum = commonMethod.GetRGNum(path, modulation);
                            if (RGNum == -1) continue;

                            int slicesNum = commonMethod.GetSlicesNum(request, modulation);
                            if (slicesNum < minSlicesNum)
                            {
                                //Find best position for request with selected path and modulation
                                Tuple<int, int> positionIndexAndDeniedTraffic = FindBestPosition(path, slicesNum, request);

                                int reducedTraffic = positionIndexAndDeniedTraffic.Item2 - request.Demand;
                                if (reducedTraffic < minRemovalTraffic || (reducedTraffic == minRemovalTraffic && path.Length > bestPath.Length)) 
                                {
                                    minRemovalTraffic = reducedTraffic;
                                    bestModulation = modulation;
                                    bestPath = path;
                                    bestReqeust = request;
                                    bestStartSliceIndex = positionIndexAndDeniedTraffic.Item1;
                                    bestRG = RGNum;
                                    bestNumSlices = slicesNum;
                                }

                                //Calculate positive moves
                                if (reducedTraffic < 0) 
                                {
                                    if (!positiveMoveRequestId.Contains(request.Id)) positiveMoveRequestId.Add(request.Id);
                                }
                            }
                            else if (request == bestReqeust && path == bestPath && slicesNum == bestNumSlices && RGNum <= bestRG)
                            {
                                bestModulation = modulation;
                            }
                        }
                    }   
                }
                Console.WriteLine();
                stopWatch.Stop();
                cpuTime = stopWatch.ElapsedMilliseconds / 1000.00;
                stopWatch.Reset();
                //Output positive moves
                positiveMovesNum.Add(positiveMoveRequestId.Count);

                if (bestReqeust != null)
                {
                    //Log information
                    Console.WriteLine("Best request to insert found! Take " + cpuTime + " sec. Removed requests : ");
                    Logger.WriteLine("Best request to insert found! Take " + cpuTime + " sec. Removed requests : ");

                    //int forbiddenNum = random.Next(Program.META_FORBIDDEN_MIN, Program.META_FORBIDDEN_MAX);
                    int forbiddenNum = random.Next(Program.META_FORBIDDEN_MIN, Program.META_FORBIDDEN_MAX);
                    string forbiddenKey = generateForbiddenKey(bestReqeust.NodePair.SourceNode.Id, bestReqeust.NodePair.DestinaNode.Id, bestPath.Id);
                    if (!forbiddenSet.ContainsKey(forbiddenKey)) forbiddenSet.Add(forbiddenKey, forbiddenNum);
                    else forbiddenSet[forbiddenKey] = forbiddenNum;

                    //Remove requests
                    List<int> removalRequests = GetRemovalRequests(bestStartSliceIndex, bestNumSlices, bestPath.LinkUsage, bestReqeust, bestPath.Id);
                    int removalTraffic = 0;
                    int positionInGrantedList = grantedRequestList.Count;
                    for (int i = 0; i < removalRequests.Count; i++)
                    {
                        Model.Request request = graph.RequestSet[removalRequests[i]];
                        Console.WriteLine("[" + request.Id + ",<" + request.NodePair.SourceNode.Id + "," + request.NodePair.DestinaNode.Id + ">,Path:" + request.UsedPath.Id + "," + request.UsedModulation.Name + ",RG:" + request.NumRG + "]");
                        Logger.WriteLine("[" + request.Id + ",<" + request.NodePair.SourceNode.Id + "," + request.NodePair.DestinaNode.Id + ">,Path:" + request.UsedPath.Id + "," + request.UsedModulation.Name + ",RG:" + request.NumRG + "]");

                        removalTraffic += request.Demand;
                        RemoveTrafficFromTables(request);
                        //deniedRequests.Add(request.Id, request);
                        if (aggreagated) deniedRequests.Add(request.Id, request);
                        else 
                        {
                            if (request.Demand == request.NodePair.TotalDemand) AddSeparatedRequestIntoDeny(request.NodePair);
                            else deniedRequests.Add(request.Id, request);
                        }

                        int position = grantedRequestList.IndexOf(request);
                        if (position < positionInGrantedList) positionInGrantedList = position;
                        grantedRequestList.RemoveAt(position);
                    }

                    //Insert reqeust
                    commonMethod.InsertTraffic(bestReqeust, bestPath, bestModulation, bestStartSliceIndex, bestNumSlices, bestRG, slicesUsageTable, slicesReqeustsTable);
                    grantedRequestList.Insert(positionInGrantedList, bestReqeust);
                    deniedRequests.Remove(bestReqeust.Id);

                    Console.WriteLine("Insert request:");
                    Logger.WriteLine("Insert request:");
                    Console.WriteLine("[" + bestReqeust.Id + ",<" + bestReqeust.NodePair.SourceNode.Id + "," + bestReqeust.NodePair.DestinaNode.Id + ">,Path:" + bestReqeust.UsedPath.Id + "," + bestReqeust.UsedModulation.Name + ",RG:" + bestReqeust.NumRG + "]");
                    Logger.WriteLine("[" + bestReqeust.Id + ",<" + bestReqeust.NodePair.SourceNode.Id + "," + bestReqeust.NodePair.DestinaNode.Id + ">,Path:" + bestReqeust.UsedPath.Id + "," + bestReqeust.UsedModulation.Name + ",RG:" + bestReqeust.NumRG + "]");

                    //Push slices usage to bottom
                    Console.Write("Pushing fragments...");
                    Logger.Write("Pushing fragments...");
                    stopWatch.Start();

                    PushSlicesUsageToBottom();
                    currentThroughput = currentThroughput - removalTraffic + bestReqeust.Demand;

                    stopWatch.Stop();
                    cpuTime = stopWatch.ElapsedMilliseconds / 1000.00;
                    stopWatch.Reset();
                    Console.WriteLine("Take " + cpuTime + " sec");
                    Logger.WriteLine("Take " + cpuTime + " sec");

                    noImproveNum++;
                    //Store best solution
                    if(currentThroughput > bestThroughput)
                    {
                        bestThroughput = currentThroughput;
                        UpdateBestSolution();
                        noImproveNum = 0;
                    }

                    Console.WriteLine("Current Throughput: " + currentThroughput + "; Best Throughput: " + bestThroughput);
                    Logger.WriteLine("Current Throughput: " + currentThroughput + "; Best Throughput: " + bestThroughput);

                    //Force one request at top
                    Console.WriteLine("Force one request at top...");
                    Logger.WriteLine("Force one request at top...");
                    bestReqeust = null;
                    bestPath = null;
                    bestModulation = null;
                    bestRG = 0;
                    bestNumSlices = 0;
                    minRemovalTraffic = int.MaxValue;
                    foreach (KeyValuePair<int, Model.Request> entry in deniedRequests)
                    {
                        Model.Request request = entry.Value;
                        for (int p = 0; p < request.NodePair.PathSet.Count; p++)
                        {
                            Model.Path path = request.NodePair.PathSet[p];
                            int minSlicesNum = int.MaxValue;
                            /*********************************************************************************************************** 
                             * The order of modulation is really important!!! Should be ordered as necessary slices will always be increasing
                             ***********************************************************************************************************/
                            for (int m = 0; m < graph.ModulationSet.Length; m++)
                            {
                                Model.Modulation modulation = graph.ModulationSet[m];
                                int RGNum = commonMethod.GetRGNum(path, modulation);
                                if (RGNum == -1) continue;

                                int slicesNum = commonMethod.GetSlicesNum(request, modulation);
                                if (slicesNum < minSlicesNum)
                                {
                                    removalRequests = GetRemovalRequests(Model.Graph.numSlices - slicesNum, slicesNum, path.LinkUsage, request, path.Id);
                                    int reducedTraffic = -request.Demand;
                                    for (int r = 0; r < removalRequests.Count; r++)
                                    {
                                        reducedTraffic += graph.RequestSet[removalRequests[r]].Demand;
                                    }

                                    if (reducedTraffic < minRemovalTraffic || (reducedTraffic == minRemovalTraffic && path.Length > bestPath.Length))
                                    {
                                        if (removalRequests.Count == 1)
                                        {
                                            Model.Request removeRequest = graph.RequestSet[removalRequests[0]];
                                            if (removeRequest.UsedPath == path && removeRequest.NumSlicesUsage == slicesNum && removeRequest.SliceStartIndex == Model.Graph.numSlices - slicesNum)
                                                continue;
                                        }

                                        minRemovalTraffic = reducedTraffic;
                                        bestModulation = modulation;
                                        bestPath = path;
                                        bestReqeust = request;
                                        bestRG = RGNum;
                                        bestNumSlices = slicesNum;
                                    }
                                }
                                else if (request == bestReqeust && path == bestPath && slicesNum == bestNumSlices && RGNum <= bestRG)
                                {
                                    bestModulation = modulation;
                                }
                            }
                        } 
                    }
                    if (bestReqeust != null)
                    {
                        Console.WriteLine("Remove requests...");
                        Logger.WriteLine("Remove requests...");
                        //Remove requests
                        removalRequests = GetRemovalRequests(Model.Graph.numSlices - bestNumSlices, bestNumSlices, bestPath.LinkUsage, bestReqeust, bestPath.Id);
                        removalTraffic = 0;
                        for (int i = 0; i < removalRequests.Count; i++)
                        {
                            Model.Request request = graph.RequestSet[removalRequests[i]];
                            Console.WriteLine("[" + request.Id + ",<" + request.NodePair.SourceNode.Id + "," + request.NodePair.DestinaNode.Id + ">,Path:" + request.UsedPath.Id + "," + request.UsedModulation.Name + ",RG:" + request.NumRG + "]");
                            Logger.WriteLine("[" + request.Id + ",<" + request.NodePair.SourceNode.Id + "," + request.NodePair.DestinaNode.Id + ">,Path:" + request.UsedPath.Id + "," + request.UsedModulation.Name + ",RG:" + request.NumRG + "]");

                            removalTraffic += request.Demand;
                            RemoveTrafficFromTables(request);
                            if (aggreagated) deniedRequests.Add(request.Id, request);
                            else
                            {
                                if (request.Demand == request.NodePair.TotalDemand) AddSeparatedRequestIntoDeny(request.NodePair);
                                else deniedRequests.Add(request.Id, request);
                            }
                            grantedRequestList.Remove(request);
                        }

                        //Insert reqeust
                        commonMethod.InsertTraffic(bestReqeust, bestPath, bestModulation, Model.Graph.numSlices - bestNumSlices, bestNumSlices, bestRG, slicesUsageTable, slicesReqeustsTable);
                        grantedRequestList.Add(bestReqeust);
                        deniedRequests.Remove(bestReqeust.Id);
                        currentThroughput = currentThroughput - removalTraffic + bestReqeust.Demand;
                        Console.WriteLine("Insert request:");
                        Logger.WriteLine("Insert request:");
                        Console.WriteLine("[" + bestReqeust.Id + ",<" + bestReqeust.NodePair.SourceNode.Id + "," + bestReqeust.NodePair.DestinaNode.Id + ">,Path:" + bestReqeust.UsedPath.Id + "," + bestReqeust.UsedModulation.Name + ",RG:" + bestReqeust.NumRG + "]");
                        Logger.WriteLine("[" + bestReqeust.Id + ",<" + bestReqeust.NodePair.SourceNode.Id + "," + bestReqeust.NodePair.DestinaNode.Id + ">,Path:" + bestReqeust.UsedPath.Id + "," + bestReqeust.UsedModulation.Name + ",RG:" + bestReqeust.NumRG + "]");
                        Console.WriteLine("Current Throughput: " + currentThroughput + "; Best Throughput: " + bestThroughput);
                        Logger.WriteLine("Current Throughput: " + currentThroughput + "; Best Throughput: " + bestThroughput);
                    }
                    //Store best solution
                    if (currentThroughput > bestThroughput)
                    {
                        bestThroughput = currentThroughput;
                        UpdateBestSolution();
                    }
                }
                else noImproveNum++;

                iteration++;
                throughputList.Add(currentThroughput);
                GetGoS();

                UpdateForbiddenSet();
                Console.WriteLine();
                Logger.WriteLine();
            }
            Console.WriteLine("End to switch reqeusts order!");
            Logger.WriteLine("End to switch reqeusts order!");

            if (iteration < Program.META_ITERATION_NUM) 
            {
                Console.WriteLine("Mistakes!!!");
            }

            return bestThroughput;
        }

        private string generateForbiddenKey(int sourceId, int destinationId, int pathId) 
        {
            string key = "";
            key = sourceId + "," + destinationId + "," + pathId;
            return key;
        }

        private void UpdateGlobalInformation()
        {
            //Clear all the expired information
            for (int i = 0; i < graph.LinkSet.Count; i++)
            {
                graph.LinkSet[i].AssignedSlices.SetAll(false);
            }
            foreach (KeyValuePair<int, Model.Path> entry in graph.PathSet)
            {
                Model.Path path = entry.Value;
                path.AssignedSlices.SetAll(false);
                path.UsedByRequests.Clear();
                path.UsedNumSlices = 0;
            }
            for (int i = 0; i < graph.PairSet.Count; i++)
            {
                Model.Pair pair = graph.PairSet[i];
                pair.IsAllDemandAssigned = false;
                pair.GrantedDemand = 0;
                pair.GrantedRequests.Clear();
            }
            foreach (KeyValuePair<int, Model.Request> entry in graph.RequestSet)
            {
                Model.Request request = entry.Value;
                request.IsGranted = false;
                request.NumRG = 0;
                request.NumSlicesUsage = 0;
                request.SliceStartIndex = 0;
                request.UsedModulation = null;
                request.UsedPath = null;
            }

            //Update with new information
            for (int i = 0; i < bestSolution.Count; i++)
            {
                Model.Request request = bestSolution[i];
                Model.Request originalRequest = graph.RequestSet[request.Id];
                originalRequest.IsGranted = true;
                originalRequest.NumRG = request.NumRG;
                originalRequest.NumSlicesUsage = request.NumSlicesUsage;
                originalRequest.SliceStartIndex = request.SliceStartIndex;
                originalRequest.UsedModulation = request.UsedModulation;
                originalRequest.UsedPath = request.UsedPath;

                Model.Pair pair = originalRequest.NodePair;
                pair.GrantedRequests.Add(originalRequest);
                pair.GrantedDemand += originalRequest.Demand;
                if (pair.TotalDemand == pair.GrantedDemand) pair.IsAllDemandAssigned = true;

                Model.Path path = originalRequest.UsedPath;
                path.AssignSlices(originalRequest.SliceStartIndex, originalRequest.NumSlicesUsage);
                path.UsedByRequests.Add(originalRequest);
                path.UsedNumSlices += originalRequest.NumSlicesUsage;

                for (int j = 0; j < path.Links.Count; j++)
                {
                    path.Links[j].AssignSlices(originalRequest.SliceStartIndex, originalRequest.NumSlicesUsage);
                }
            }
        }

        /// <summary>
        /// Find the best position to insert a request which need a given number of slices on a given path.
        /// </summary>
        /// <param name="path">The given path.</param>
        /// <param name="slicesNum">The given number of slices.</param>
        /// <returns>Tuple: firt item - The start index of slices, second item - The traffic volumn need to be removed</returns>
        private Tuple<int, int> FindBestPosition(Model.Path path, int slicesNum, Model.Request currentRequest)
        {
            int sliceIndex = 0;
            int sliceEndIndex = Model.Graph.numSlices - slicesNum + 1;
            int bestStartIndex = 0;
            int minRemovalTraffic = int.MaxValue;
            int minRemovalLength = int.MaxValue;

            while (sliceIndex < sliceEndIndex)
            {
                int removalTraffic = 0;
                int removalLength = 0;
                List<int> removalRequests = GetRemovalRequests(sliceIndex, slicesNum, path.LinkUsage, currentRequest, path.Id);
                for (int i = 0; i < removalRequests.Count; i++)
                {
                    Model.Request request = graph.RequestSet[removalRequests[i]];
                    removalTraffic += request.Demand;
                    removalLength += request.UsedPath.Length;
                }

                if (removalTraffic < minRemovalTraffic || (removalTraffic == minRemovalTraffic && removalLength < minRemovalLength))
                {
                    if (removalTraffic == currentRequest.Demand && currentRequest.NodePair == graph.RequestSet[removalRequests[0]].NodePair)
                    {
                        sliceIndex++;
                        continue;
                    }
                    minRemovalTraffic = removalTraffic;
                    bestStartIndex = sliceIndex;
                    minRemovalLength = removalLength;
                }
                sliceIndex++;
            }
            return new Tuple<int, int>(bestStartIndex, minRemovalTraffic);
        }

        /// <summary>
        /// Get the list of requests need to be removed if we trying to grant a requests which need a given number of slices
        /// on a given set of links start from a given position in slicesUsageTable
        /// </summary>
        /// <param name="startSliceIndex">The given position</param>
        /// <param name="slicesNum">The given number of slices need to be used</param>
        /// <param name="links">The given set of links</param>
        /// <returns>The list of requests need to be removed</returns>
        private List<int> GetRemovalRequests(int startSliceIndex, int slicesNum, BitArray linkUsage, Model.Request request, int pathId)
        {
            int endIndex = startSliceIndex + slicesNum;
            List<int> removalRequests = new List<int>();

            for (int i = startSliceIndex; i < endIndex; i++)
            {
                for (int j = 0; j < linkUsage.Length; j++)
                {
                    if (linkUsage[j] && slicesUsageTable[i][j]) 
                    {
                        int requestId = slicesReqeustsTable[j, i];
                        if (requestId == -1)
                        {
                            Console.WriteLine("Mistake!!!");
                        }
                        if (!removalRequests.Contains(requestId))
                            removalRequests.Add(requestId);
                    }
                }
            }

            //Check guard band
            int preIndex = startSliceIndex - 1;
            if (preIndex >= 0) 
            {
                for (int j = 0; j < linkUsage.Length; j++)
                {
                    if (linkUsage[j] && slicesUsageTable[preIndex][j])
                    {
                        Model.Request preRequest = graph.RequestSet[slicesReqeustsTable[j, preIndex]];
                        if (request.NodePair != preRequest.NodePair && preRequest.UsedPath.Id != pathId)
                        {
                            if (!removalRequests.Contains(preRequest.Id))
                                removalRequests.Add(preRequest.Id);
                        }
                    }
                }
            }
            if(endIndex < Model.Graph.numSlices) 
            {
                for (int j = 0; j < linkUsage.Length; j++)
                {
                    if (linkUsage[j] && slicesUsageTable[endIndex][j])
                    {
                        Model.Request nextRequest = graph.RequestSet[slicesReqeustsTable[j, endIndex]];
                        if (request.NodePair != nextRequest.NodePair || nextRequest.UsedPath.Id != pathId)
                        {
                            if (!removalRequests.Contains(nextRequest.Id))
                                removalRequests.Add(nextRequest.Id);
                        }
                    }
                }
            }
            

            return removalRequests;
        }

        /// <summary>
        /// Remove a given request from slicesRequestsTable and slicesUsageTable;
        /// Update the given request's information; but not associated pair, path, links and graph
        /// </summary>
        /// <param name="request">The given reqeust</param>
        private void RemoveTrafficFromTables(Model.Request request)
        {
            int endIndex = request.SliceStartIndex + request.NumSlicesUsage;
            for (int i = request.SliceStartIndex; i < endIndex; i++)
            {
                foreach (Model.Link link in request.UsedPath.Links)
                { 
                    slicesReqeustsTable[link.Id, i] = -1;
                    slicesUsageTable[i][link.Id] = false;
                }
            }
            request.IsGranted = false;
            request.UsedPath = null;
            request.UsedModulation = null;
            request.SliceStartIndex = 0;
            request.NumSlicesUsage = 0;
            request.NumRG = 0;
        }

        /// <summary>
        /// Push to bottom
        /// </summary>
        private void PushSlicesUsageToBottom()
        {
            for (int i = 0; i < grantedRequestList.Count; i++)
            {
                Model.Request request = grantedRequestList[i];
                //Remove from tables
                int startIndex = request.SliceStartIndex;
                int numSlices = request.NumSlicesUsage;
                int numRG = request.NumRG;
                Model.Modulation modulation = request.UsedModulation;
                Model.Path path = request.UsedPath;
                RemoveTrafficFromTables(request);

                int index = commonMethod.FindAvailableSlicesForRequest(slicesUsageTable, path.LinkUsage, numSlices, 0, startIndex, slicesReqeustsTable, graph.RequestSet, request);
                if (index == -1)
                {
                    commonMethod.InsertTraffic(request, path, modulation, startIndex, numSlices, numRG, slicesUsageTable, slicesReqeustsTable);
                }
                else 
                {
                    commonMethod.InsertTraffic(request, path, modulation, index, numSlices, numRG, slicesUsageTable, slicesReqeustsTable);
                }
            }
        }

        /// <summary>
        /// Update the global best solution
        /// </summary>
        private void UpdateBestSolution()
        {
            bestSolution.Clear();

            for (int i = 0; i < grantedRequestList.Count; i++)
            {
                Model.Request request = grantedRequestList[i];
                Model.Request requestCopy = new Model.Request(request.Id, request.NodePair, request.Demand);
                requestCopy.IsGranted = true;
                requestCopy.NumRG = request.NumRG;
                requestCopy.NumSlicesUsage = request.NumSlicesUsage;
                requestCopy.SliceStartIndex = request.SliceStartIndex;
                requestCopy.UsedModulation = request.UsedModulation;
                requestCopy.UsedPath = request.UsedPath;
                bestSolution.Add(requestCopy);
            }
        }

        private void AddSeparatedRequestIntoDeny(Model.Pair pair) 
        {
            for (int i = 0; i < pair.RequestSet.Length; i++)
            {
                Model.Request request = pair.RequestSet[i];
                if (!request.IsGranted)  deniedRequests.Add(request.Id, request);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void UpdateForbiddenSet()
        {
            List<string> removeKeys = new List<string>();
            //Update forbidden set
            List<string> keys = forbiddenSet.Keys.ToList();
            for (int i = 0; i < keys.Count; i++)
            {
                int remainIter = forbiddenSet[keys[i]] - 1;
                forbiddenSet[keys[i]] = remainIter;
                if (remainIter <= 0) removeKeys.Add(keys[i]);
            }
            for (int i = 0; i < removeKeys.Count; i++)
            {
                forbiddenSet.Remove(removeKeys[i]);
            }
        }

        private void CheckSlicesValidation(List<Model.Request> requestList)
        {
            BitArray[] slicesChecking = new BitArray[graph.LinkSet.Count];
            for (int i = 0; i < slicesChecking.Length; i++)
            {
                slicesChecking[i] = new BitArray(Model.Graph.numSlices, false);
            }

            for (int i = 0; i < requestList.Count; i++)
            {
                Model.Request request = requestList[i];
                BitArray assignedSlices = new BitArray(Model.Graph.numSlices, false);
                BitArray cloneArray = new BitArray(Model.Graph.numSlices, false);
                foreach (Model.Link link in request.UsedPath.Links)
                {
                    for (int j = request.SliceStartIndex; j < request.SliceStartIndex + request.NumSlicesUsage; j++)
                    {
                        if (slicesChecking[link.Id][j])
                        {
                            Console.WriteLine("Mistake!!!!");
                        }
                        else
                        {
                            slicesChecking[link.Id][j] = true;
                        }
                    }
                }
            }
        }

        private void GetGoS()
        {
            GoSAgg.Add(grantedRequestList.Count);
            int numSepRequest = 0;
            foreach(Model.Request request in grantedRequestList)
            {
                if (request.NodePair.AggregatedTraffic == request)
                {
                    numSepRequest += request.NodePair.RequestSet.Length;
                }
                else numSepRequest++;
            }
            GoSSep.Add(numSepRequest);
        }

        private void CheckSlicesRequestTable()
        {
            for (int r = 0; r < grantedRequestList.Count; r++)
            {
                Model.Request request = grantedRequestList[r];
                for (int i = 0; i < slicesReqeustsTable.GetLength(0); i++)
                {
                    for (int j = 0; j < slicesReqeustsTable.GetLength(1); j++)
                    {
                        if (slicesReqeustsTable[i, j] == request.Id)
                        {
                            if (!request.UsedPath.Links.Contains(graph.LinkSet[i]))
                            {
                                Console.WriteLine("mistakes!!!");
                            }
                            if (j < request.SliceStartIndex || j >= request.SliceStartIndex + request.NumSlicesUsage)
                            {
                                Console.WriteLine("mistakes!!!");
                            }
                        }
                    }
                }
            }
        }

        private void checkDeniedReqeusts(int currentThroughput) 
        {
            int grantedTraffic = 0;
            foreach (Model.Request request in grantedRequestList)
            {
                grantedTraffic += request.Demand;
            }
            if (grantedTraffic != currentThroughput)
            {
                Console.WriteLine("Mistakes!!!");
            }

            int deniedTraffic = 0;
            foreach (KeyValuePair<int, Model.Request> entry in deniedRequests)
            {
                Model.Request request = entry.Value;
                deniedTraffic += request.Demand;
            }
            if (currentThroughput + deniedTraffic != graph.TotalDemand)
            {
                Console.WriteLine("Mistakes!!!");
            }
        }
    }
}

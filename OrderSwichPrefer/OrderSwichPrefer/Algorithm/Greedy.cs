using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace OrderSwichPrefer.Algorithm
{
    class Greedy
    {
        private Model.Graph graph;
        private Common commonMethods;

        private BitArray[] slicesUsage;
        private int[,] requestsGrantedTable;
        private List<Model.Request> grantedRequests;
        private Dictionary<int, Model.Request> deniedRequests;

        public Greedy(Model.Graph graph)
        {
            this.graph = graph;
            this.commonMethods = new Common();

            graph.SetNeighbors();
        }

        private void prepareGreedy(bool aggregated)
        {
            grantedRequests = new List<Model.Request>();
            deniedRequests = new Dictionary<int, Model.Request>();
            commonMethods.GetDeniedAndGrantedRequests(aggregated, graph.PairSet, grantedRequests, deniedRequests);

            slicesUsage = commonMethods.GenerateSlicesUsageTable(graph.LinkSet);
            requestsGrantedTable = commonMethods.GenerateSlicesRequestsTable(grantedRequests, graph.LinkSet.Count);
        }

        /// <summary>
        /// Grant traffic for whole graph with 0 RG
        /// </summary>
        /// <param name="allowRG">The number of RG</param>
        public void GrantAggragetedTrafficGlobal()
        {
            //Sort node pairs
            commonMethods.SortByDemand(graph.PairSet);
            prepareGreedy(true);

            for (int i = 0; i < graph.PairSet.Count; i++)
            {
                Model.Pair pair = graph.PairSet[i];
                GrantAggragetedTrafficForPair(pair.AggregatedTraffic, 0);
            }
        }

        /// <summary>
        /// Grant traffic for pairs that cannot granted with aggrageted traffic
        /// </summary>
        public void GrantSeparatedRequests()
        {
            //Sort node pairs by path
            commonMethods.SortByShortestPath(graph.PairSet);
            prepareGreedy(false);

            for (int i = graph.PairSet.Count - 1; i >= 0; i--)
            {
                Model.Pair pair = graph.PairSet[i];
                if (pair.GrantedDemand < pair.TotalDemand)
                {
                    pair.RequestSet.OrderByDescending(item => item.Demand);
                    for (int j = 0; j < pair.RequestSet.Length; j++)
                    {
                        GrantAggragetedTrafficForPair(pair.RequestSet[j], null);
                    }
                }
                
            }
        }

        /// <summary>
        /// Grant traffic for a given node pair with 0 RG
        /// </summary>
        /// <param name="pair">The given node pair</param>
        /// <param name="allowRG">The number of RG</param>
        private void GrantAggragetedTrafficForPair(Model.Request request, int? allowRG)
        {
            Model.Pair pair = request.NodePair;
            for (int i = 0; i < pair.PathSet.Count; i++)
            {
                Model.Path path = pair.PathSet[i];
                //select modulations, minimize slices
                List<Tuple<Model.Modulation, int>> modulations = commonMethods.SelectModulations(path, graph.ModulationSet, allowRG);
                if (modulations.Count == 0) break;

                //Calculate the number of slices needed
                Model.Modulation modulation = null;
                int startIndex = -1;
                int numSlices = 0;
                int numRG = 0;
                for (int j = 0; j < modulations.Count; j++)
                {
                    modulation = modulations[j].Item1;
                    numSlices = commonMethods.GetSlicesNum(request, modulation);
                    numRG = modulations[j].Item2;

                    //Find available sequence of slices
                    startIndex = commonMethods.FindAvailableSlicesForRequest(slicesUsage, path.LinkUsage, numSlices, 0, Model.Graph.numSlices - numSlices + 1, requestsGrantedTable, graph.RequestSet, request);
                    if (startIndex != -1) break;
                }

                //Grant traffic
                if (startIndex > -1)
                {
                    for (int l = 0; l < path.Links.Count; l++)
                    {
                        path.Links[l].AssignSlices(startIndex, numSlices);

                    }

                    path.UsedByRequests.Add(request);
                    path.AssignSlices(startIndex, numSlices);
                    path.UsedNumSlices += numSlices;
                    path.Demand += request.Demand;

                    //Update slicesUsage table and requestsGranted table
                    commonMethods.InsertTraffic(request, path, modulation, startIndex, numSlices, numRG, slicesUsage, requestsGrantedTable);
                    
                    pair.GrantedDemand += request.Demand;
                    pair.GrantedRequests.Add(request);
                    if (pair.GrantedDemand == pair.TotalDemand) pair.IsAllDemandAssigned = true;
                    graph.GrantedDemand += request.Demand;
                    break;
                }
                
            }
        }
    }
}

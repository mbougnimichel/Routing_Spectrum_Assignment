using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace OrderSwichPrefer
{
    class Program
    {
        public const int KSHORTESTPATH = 30;
        public static double cpuTime = 0.0;
        public const int META_ITERATION_NUM = 1000;
        public static int META_FORBIDDEN_MAX = Convert.ToInt32(Math.Ceiling(0.15 * META_ITERATION_NUM)) + 2;
        public static int META_FORBIDDEN_MIN = Convert.ToInt32(Math.Ceiling(0.15 * META_ITERATION_NUM)) - 2;
        public const int GUARD_BAND = 1;

        static void Main(string[] args)
        {
            Program maxRSA = new Program();
            string[] files = { "RSA_USA_0.dat", "RSA_USA_1.dat", "RSA_USA_2.dat", "RSA_USA_3.dat", "RSA_USA_4.dat",
                            "RSA_USA_5.dat", "RSA_USA_6.dat", "RSA_USA_7.dat", "RSA_USA_8.dat", "RSA_USA_9.dat"};
            //string[] files = { "RSA_USA_0.dat"};

            for (int i = 0; i < files.Length; i++)
            {
                String fileName = "../../Data/original/" + files[i];
                //maxRWA.PrepareDataForYensAlgorithm(fileName);
                //maxRWA.PrepareDataForGreedy(fileName);
                fileName = fileName.Replace("original", "greedy");
                Model.Graph graph = new Model.Graph(fileName);
                maxRSA.SetModulations(graph);

                maxRSA.CombineAlogrithms(graph, fileName);
            }

        }

        public void PrepareDataForYensAlgorithm(string fileName)
        {
            Algorithm.PrepareFiles.getInstance().PrepareFileForKShortestPath(fileName);
        }

        public void PrepareDataForGreedy(string fileName)
        {
            Algorithm.PrepareFiles.getInstance().ReadShortestPathsResults(fileName.Replace("original", "result"));
        }

        /// <summary>
        /// Set modulatons by order
        /// </summary>
        /// <param name="graph"></param>
        public void SetModulations(Model.Graph graph)
        {
//            string[] moduStrings = new string[4];
//            moduStrings[0] = "200,16QAM,3,600";
//            moduStrings[1] = "150,8QAM,3,1000";
//            moduStrings[2] = "100,QPSK_100,3,1500";
//            moduStrings[3] = "40,QPSK_40,2,2000";

            string[] moduStrings = new string[1];
            moduStrings[0] = "100,QPSK_100,3,1500";
            graph.generateModulations(moduStrings);
        }


        private void StrechDistance(Model.Graph graph)
        {
            foreach (KeyValuePair<int, Model.Link> entry in graph.LinkSet)
            {
                entry.Value.Length = entry.Value.Length / 5;
            }
            foreach (KeyValuePair<int, Model.Path> entry in graph.PathSet)
            {
                entry.Value.Length = entry.Value.Length / 5;
            }
        }

        private void CombineAlogrithms(Model.Graph graph, String fileName)
        {
            Stopwatch stopWatch = new Stopwatch();
            StrechDistance(graph);

            Algorithm.Greedy greedyAlg = new Algorithm.Greedy(graph);
            Algorithm.OrderSwichGreedy tabu = new Algorithm.OrderSwichGreedy(graph, fileName+".tabuLog");
            
            //Run Greedy for aggregated traffic
            stopWatch.Start();
            greedyAlg.GrantAggragetedTrafficGlobal();
            stopWatch.Stop();
            Algorithm.PrepareFiles.getInstance().writeGreedyResult(graph, fileName + ".aggGreedy");

            //Run tabu for aggregated traffic
            stopWatch.Start();
            tabu.RunAlgorithm(true);
            stopWatch.Stop();
            Algorithm.PrepareFiles.getInstance().writeTabuResult(graph, fileName + ".aggTabu");
            tabu.PrintOutThroughputForIterations(fileName + ".aggThoughput.csv");

            //Run Greedy for separeted traffic
            stopWatch.Start();
            greedyAlg.GrantSeparatedRequests();
            stopWatch.Stop();
            Algorithm.PrepareFiles.getInstance().writeGreedyResult(graph, fileName + ".sepGreedy");

            //Run tabu for separated traffic
            stopWatch.Start();
            tabu.RunAlgorithm(false);
            tabu.Close();
            stopWatch.Stop();
            cpuTime = stopWatch.ElapsedMilliseconds / 1000.00;
            Algorithm.PrepareFiles.getInstance().writeTabuResult(graph, fileName + ".sepTabu");
            tabu.PrintOutThroughputForIterations(fileName + ".sepThoughput.csv");
            tabu.PrintOutPositiveMovesForIterations(fileName + ".sepPositiveMoves.csv");
        }

    }
}

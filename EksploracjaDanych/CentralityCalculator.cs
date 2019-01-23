using System;
using System.Collections.Generic;
using System.Linq;
using QuickGraph;
using QuickGraph.Algorithms.RankedShortestPath;

namespace EksploracjaDanych
{
    public static class CentralityCalculator
    {
        public static void CalcBetweennessCentrality(Map map, int iterations, out IDictionary<long, double> centralityWageMap)
        {
            var alg = PrepareShoterstPathAlgorithm(map);
            
            var r = new Random();
            var ways = map.Ways.ToList();

            var nodeWages = map.Nodes.Select(a => a.Key).ToDictionary(a => a, a => 0);

            RunInIterations(iterations, 100, out centralityWageMap, 
                () => MainAction(alg, ways, nodeWages, r),
                currentIteration => RefreshAction(nodeWages, currentIteration));
            
            Logger.Log($"Centralność max: {centralityWageMap.Max(a => a.Value)}, " +
                       $"średnia: {centralityWageMap.Average(a => a.Value)}");
        }

        private static void MainAction(HoffmanPavleyRankedShortestPathAlgorithm<long, TaggedEdge<long, double>> alg, IReadOnlyList<Way> ways, Dictionary<long, int> nodeWages, Random r)
        {
            var r1 = ways[r.Next(ways.Count)];
            var r2 = ways[r.Next(ways.Count)];

            if (r1.Nodes.Contains(r2.Start)) return;

            alg.Compute(r1.Start, r2.Start);
            foreach (var shortestPath in alg.ComputedShortestPaths)
            {
                foreach (var edge in shortestPath)
                {
                    nodeWages[edge.Source]++;
                }
            }
        }

        private static Dictionary<long, double> RefreshAction(Dictionary<long, int> nodeWages, int currentIteration)
        {
            return nodeWages
                .Where(a => a.Value > 0)
                .OrderBy(a => a.Value)
                .ToDictionary(a => a.Key, a => a.Value * 1.0 / currentIteration);
        }

        private static void RunInIterations(int iterations, int breaks, out IDictionary<long, double> centralityWageMap, Action mainAction, Func<int, Dictionary<long, double>> refreshAction)
        {
            var proc = 0;
            var time = DateTime.Now;

            for (var i = 0; i < iterations; i++)
            {
                var nProc = 100 * i / iterations;
                if (nProc != proc)
                {
                    proc = nProc;
                    var estimate = new DateTime((DateTime.Now - time).Ticks / proc * (100 - proc));
                    Logger.Log(proc + "%, pozostało: " + estimate.ToString("mm:ss"));
                }

                mainAction();

                if (i % breaks == breaks - 1)
                    centralityWageMap = refreshAction(i);
            }

            centralityWageMap = refreshAction(iterations);
        }

        private static HoffmanPavleyRankedShortestPathAlgorithm<long, TaggedEdge<long, double>> PrepareShoterstPathAlgorithm(Map map)
        {
            var pathGraph = new BidirectionalGraph<long, TaggedEdge<long, double>>();

            foreach (var vertex in map.GetEdges().Keys)
            {
                pathGraph.AddVertex(vertex);
            }

            foreach (var keyValuePair in map.GetEdgesWithDistance(true))
            {
                foreach (var way in keyValuePair.Value)
                {
                    var edge = new TaggedEdge<long, double>(keyValuePair.Key, way.Item1, way.Item2);
                    pathGraph.AddEdge(edge);
                }

            }

            var alg = new HoffmanPavleyRankedShortestPathAlgorithm<long, TaggedEdge<long, double>>(pathGraph, e => e.Tag);
            return alg;
        }

        public static Dictionary<long, double> CalcClusteringCoefficient(Map map)
        {
            Logger.Log("Calc clustering coefficient");

            var result = new Dictionary<long, double>();

            var edges = map.GetEdges();
            foreach (var edgeKey in edges.Keys)
            {
                var nodes = edges[edgeKey];

                var sum = 0;

                for (var i = 0; i < nodes.Count; i++)
                {
                    for (var j = i + 1; j < nodes.Count; j++)
                    {
                        var vi = nodes[i];
                        var vj = nodes[j];
                        if (edges.ContainsKey(vi) && edges[vi].Contains(vj))
                            sum++;
                    }
                }

                if (nodes.Count > 1)
                    result[edgeKey] = 2.0 * sum / nodes.Count / (nodes.Count - 1);
            }

            Logger.Log(string.Empty);

            return result;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;

namespace BCIEssentials.Utilities
{
    public static class GraphUtilitiesTSP
    {
        private int[,] adjacencyMatrix;
        private int nodeCount;

        public SetUpModifiedTSP(int[,] upperTriangularMatrix)
        {
            nodeCount = upperTriangularMatrix.GetLength(0);
            //I don't think I need this
            adjacencyMatrix = ConvertToFullMatrix(upperTriangularMatrix);
        }

        private int[,] ConvertToFullMatrix(int[,] upperTriangularMatrix)
        {
            int[,] fullMatrix = new int[nodeCount, nodeCount];
            for (int i = 0; i < nodeCount; i++)
            {
                for (int j = i; j < nodeCount; j++)
                {
                    fullMatrix[i, j] = upperTriangularMatrix[i, j];
                    fullMatrix[j, i] = upperTriangularMatrix[i, j];
                }
            }
            return fullMatrix;
        }

        public List<int> SolveModifiedTSP()
        {
            List<int> tour = new List<int> { 0 };
            HashSet<int> unvisitedNodes = new HashSet<int>(Enumerable.Range(1, nodeCount - 1));

            while (unvisitedNodes.Count > 0)
            {
                int currentNode = tour[tour.Count - 1];
                int nextNode = FindFurthestUnvisitedNode(currentNode, unvisitedNodes);
                tour.Add(nextNode);
                unvisitedNodes.Remove(nextNode);
            }

            tour.Add(0); // Return to the starting node
            return tour;
        }

         private int FindFurthestUnvisitedNode(int currentNode, HashSet<int> unvisitedNodes)
        {
            int maxDistance = int.MinValue;
            int furthestNode = -1;

            foreach (int node in unvisitedNodes)
            {
                int distance = adjacencyMatrix[currentNode, node];
                if (distance > maxDistance || (distance == maxDistance && node < furthestNode))
                {
                    maxDistance = distance;
                    furthestNode = node;
                }
            }

            return furthestNode;
        }

        public int CalculateTourLength(List<int> tour)
        {
            int totalLength = 0;
            for (int i = 0; i < tour.Count - 1; i++)
            {
                totalLength += adjacencyMatrix[tour[i], tour[i + 1]];
            }
            return totalLength;
        }

    }

}
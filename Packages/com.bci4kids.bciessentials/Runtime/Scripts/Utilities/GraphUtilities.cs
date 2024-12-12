using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using UnityEngine;

namespace BCIEssentials.Utilities
{
    public class GraphUtilitiesTSP
    {
        private float[,] adjacencyMatrix;
        private int nodeCount;

        // public int SetUpModifiedTSP(float[,] upperTriangularMatrix)
        // {
        //     nodeCount = upperTriangularMatrix.GetLength(0);
        //     return nodeCount;
        //     //I don't think I need this
        //     // adjacencyMatrix = ConvertToFullMatrix(upperTriangularMatrix);
        // }

        private float[,] ConvertToFullMatrix(float[,] upperTriangularMatrix)
        {
            float[,] fullMatrix = new float[nodeCount, nodeCount];
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

        #region Travelling Salesman Problem (Max)

        public List<int> SolveModifiedTSP(float[,] upperTriangularMatrix, int startNode)
        {
            nodeCount = upperTriangularMatrix.GetLength(0);
            adjacencyMatrix = ConvertToFullMatrix(upperTriangularMatrix);
            List<int> tour = new List<int> { startNode };
            HashSet<int> unvisitedNodes = new HashSet<int>(Enumerable.Range(0, nodeCount));
            unvisitedNodes.Remove(startNode);

            while (unvisitedNodes.Count > 0)
            {
                int currentNode = tour[tour.Count - 1];
                int nextNode = FindFurthestUnvisitedNode(currentNode, unvisitedNodes);
                tour.Add(nextNode);
                unvisitedNodes.Remove(nextNode);
            }

            //tour.Add(0); // Return to the starting node
            return tour;
        }

         private int FindFurthestUnvisitedNode(int currentNode, HashSet<int> unvisitedNodes)
        {
            float maxDistance = float.MinValue;
            int furthestNode = -1;

            foreach (int node in unvisitedNodes)
            {
                float distance = adjacencyMatrix[currentNode, node];
                // UnityEngine.Debug.Log("IN TSP!!! Distance between " + currentNode + " and " + node + " is " + distance);
                if (distance > maxDistance || (distance == maxDistance && node < furthestNode))
                {
                    maxDistance = distance;
                    furthestNode = node;
                }
            }

            return furthestNode;
        }

        public float CalculateTourLength(List<int> tour)
        {
            float totalLength = 0;
            for (int i = 0; i < tour.Count - 1; i++)
            {
                totalLength += adjacencyMatrix[tour[i], tour[i + 1]];
            }
            return totalLength;
        }

        #endregion


        #region Laplacian Graph Partitioning
        

        #endregion

    }

}
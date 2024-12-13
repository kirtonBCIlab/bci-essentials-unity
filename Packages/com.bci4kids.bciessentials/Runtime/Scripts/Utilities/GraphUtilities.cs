using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Single;
using UnityEngine;

namespace BCIEssentials.Utilities
{
    public class GraphUtilities
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
        
        //Placeholder code
        public (List<int> subset1, List<int> subset2) LaplaceGraphPartition(float[,] adjacencyMatrix)
        {
            int n = adjacencyMatrix.GetLength(0);

            // Step 1: Compute the Laplacian matrix
            var laplacianMatrix = ComputeLaplacianMatrix(adjacencyMatrix);

            // Step 2: Compute eigenvalues and eigenvectors
            var eigen = laplacianMatrix.Evd();

            // Step 3: Use the Fiedler vector to partition the graph
            var fiedlerVector = eigen.EigenVectors.Column(1); // Second smallest eigenvalue

            var subset1 = new List<int>();
            var subset2 = new List<int>();

            for (int i = 0; i < n; i++)
            {
                if (fiedlerVector[i] < 0)
                {
                    subset1.Add(i);
                }
                else
                {
                    subset2.Add(i);
                }
            }

            return (subset1, subset2);
        }

        public (int[] subset1, int[] subset2) LaplaceGP(float[,] adjacencyMatrix)
        {
            int n = adjacencyMatrix.GetLength(0);

            // Step 1: Compute the Laplacian matrix
            var laplacianMatrix = ComputeLaplacianMatrix(adjacencyMatrix);

            // Step 2: Compute eigenvalues and eigenvectors
            var eigen = laplacianMatrix.Evd();

            // Step 3: Use the Fiedler vector to partition the graph
            var fiedlerVector = eigen.EigenVectors.Column(1); // Second smallest eigenvalue

            var subset1 = new List<int>();
            var subset2 = new List<int>();

            // Step 4: Balance the subsets
            var nodes = fiedlerVector.Select((value, index) => new { Index = index, Value = value })
                                     .OrderBy(node => node.Value)
                                     .ToList();

            for (int i = 0; i < n / 2; i++)
            {
                subset1.Add(nodes[i].Index);
            }

            for (int i = n / 2; i < n; i++)
            {
                subset2.Add(nodes[i].Index);
            }

            return (subset1.ToArray(), subset2.ToArray());
        }

        private Matrix<float> ComputeLaplacianMatrix(float[,] adjacencyMatrix)
        {
            int n = adjacencyMatrix.GetLength(0);
            var laplacianMatrix = DenseMatrix.OfArray(adjacencyMatrix);
            var degreeMatrix = DenseMatrix.Create(n, n, 0);

            for (int i = 0; i < n; i++)
            {
                float degree = 0;
                for (int j = 0; j < n; j++)
                {
                    degree += adjacencyMatrix[i, j];
                }
                degreeMatrix[i, i] = degree;
            }

            laplacianMatrix = degreeMatrix - laplacianMatrix;
            return laplacianMatrix;
        }

        public float GetLPSubsetWeight(float[,] adjacencyMatrix, List<int> subset)
        {
            float weight = 0;
            foreach (int i in subset)
            {
                foreach (int j in subset)
                {
                    weight += adjacencyMatrix[i, j];
                }
            }
            return weight;
        }

        public float CalculateCutCost(float[,] adjacencyMatrix, List<int> subset1, List<int> subset2)
        {
            float cutCost = 0;
            foreach (int i in subset1)
            {
                foreach (int j in subset2)
                {
                    cutCost += adjacencyMatrix[i, j];
                }
            }
            return cutCost;
        }

        #endregion

    }

}
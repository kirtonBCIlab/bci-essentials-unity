using System.Collections.Generic;
using UnityEngine;

namespace BCIEssentials.Utilities
{
    public static class ContextAwareUtilities
    {
        public static int[] CalculateGraphTSP
        (
            List<GameObject> nodes,
            ref int lastTourEndNode,
            bool debugPrint = false
        )
        {
            //Get the world points of each item with respect to the camera
            // var cameraTranfsorm = Camera.main.transform;
            List<Vector2> correctedNodePositions = CalculateOffsetFromCamera(nodes, Camera.main);
            int numNodes = nodes.Count;
            float[,] objectWeights = new float[numNodes, numNodes];
        
            foreach (var node in nodes)
            {
                //use Vector3.Angle to get the angle between every object in the scene. Store this as weights in a graph
                //This is a symmetric matrix, so we only need to calculate the upper triangle
                for (int i = 0; i < numNodes; i++)
                {
                    if (i == nodes.IndexOf(node))
                    {
                        objectWeights[nodes.IndexOf(node), i] = 0;
                    }
                    else
                    {
                        objectWeights[nodes.IndexOf(node), i] = Vector3.Angle(correctedNodePositions[nodes.IndexOf(node)], correctedNodePositions[i]);
                    }
                }
            }

            if (debugPrint)
            {
                //Print the weights in the upper triangle matrix
                for (int i = 0; i < numNodes; i++)
                {
                    for (int j = 0; j < numNodes; j++)
                    {
                        if (j >= i && objectWeights[i, j] != 0)
                        {
                            Debug.Log($"Angle (weight) between {nodes[i].name} and {nodes[j].name}: {objectWeights[i, j]}");
                        }
                    }
                }
            }


            GraphUtilities tsp = new GraphUtilities();
            
            var startNode = Random.Range(0, numNodes);
            //Make sure the start node and last node of the tour are not the same
            if(startNode == lastTourEndNode)
            {
                //chose a different start node
                startNode = (startNode+1) % numNodes;
            }
          
            var tour = tsp.SolveModifiedTSP(objectWeights, startNode);
            lastTourEndNode = tour[^1];

            if(debugPrint)
            {
                Debug.Log("The start node is " + startNode.ToString());
                            //Print out the tour
                for (int i = 0; i < tour.Count; i++)
                {
                    Debug.Log("The tour is " + tour[i].ToString());
                }
                Debug.Log("Tour length is: " + tsp.CalculateTourLength(tour));
            }

            return tour.ToArray();
        }


        public static (int[] subset1, int[] subset2) CalculateGraphPartition
        (
            List<GameObject> nodes
        )
        {
            // Get 2D screen positions
            List<Vector2> screenPositions = CalculateOffsetFromCamera(nodes, Camera.main);
            int numNodes = nodes.Count;
            float[,] objectWeights = new float[numNodes, numNodes];

            foreach (var node in nodes)
            {
                int i = nodes.IndexOf(node);
                for (int j = 0; j < numNodes; j++)
                {
                    if (i == j)
                    {
                        objectWeights[i, j] = 0;
                    }
                    else
                    {
                        // Calculate 2D screen-space distance
                        float distance = Vector2.Distance(
                            screenPositions[i], 
                            screenPositions[j]
                        );
                        // Convert distance to weight (inverse relationship)
                        objectWeights[i, j] = 1.0f / (distance + 1.0f);
                    }
                }
            }

            var lpPart = new GraphUtilities();
            return lpPart.LaplaceGP(objectWeights);
        }


        public static int[,] SubsetToRandomMatrix(int[] subset)
        {
            // Debug.Log("Original Subset" + string.Join(",",subset));
            int[] permutationArray = ArrayUtilities.GenerateRNRA_FisherYates(subset.Length,0,subset.Length-1);
            int[] subsetPermutated = new int[subset.Length];
            //Apply the permutation to the subset
            for (int i = 0; i < subset.Length; i++)
            {
                subsetPermutated[i] = subset[permutationArray[i]];
            }
            // Debug.Log("Shuffled Subset" + string.Join(",",subsetPermutated));
            var numRows = (int)Mathf.Floor(Mathf.Sqrt(subset.Length));
            var numCols = (int)Mathf.Ceil((float)subset.Length / (float)numRows);
            var newMatrix = new int[numRows, numCols];

            for (int i = 0; i < numRows; i++)
            {
                for (int j = 0; j < numCols; j++)
                {
                    int index = i * numCols + j;
                    if (index < subset.Length)
                    {
                        newMatrix[i, j] = subsetPermutated[index];
                    }
                    else
                    {
                        newMatrix[i, j] = -100;
                    }

                }
            } 

            // Debug.Log("New Matrix:\n" + ArrayUtilities.FormatMatrix(newMatrix));
            return newMatrix;
        }


        public static List<Vector2> CalculateOffsetFromCamera
        (
            List<GameObject> goList,
            Camera camera
        )
        {
            List<Vector2> screenPositions = new();
            foreach (var obj in goList)
            {
                if (obj.TryGetComponent<RectTransform>(out var rectTransform))
                {
                    // UI Elements - get screen position directly from RectTransform
                    RectTransformUtility.ScreenPointToWorldPointInRectangle(
                        rectTransform,
                        rectTransform.position,
                        camera,
                        out Vector3 screenPos
                    );
                    screenPositions.Add(new(screenPos.x, screenPos.y));
                }
                else
                {
                    // 3D Objects - project to screen space
                    Vector3 screenPos = camera.WorldToScreenPoint(obj.transform.position);
                    screenPositions.Add(screenPos);
                }
            }
            return screenPositions;
        }
    }
}
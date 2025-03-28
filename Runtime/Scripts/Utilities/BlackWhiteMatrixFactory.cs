using System;
using System.Linq;
using UnityEngine;

namespace BCIEssentials.Utilities
{
    public class BlackWhiteMatrixFactory
    {
        public int MatrixWidth { get; private set; }
        public int MatrixHeight { get; private set; }
        public int MatrixArea => MatrixWidth * MatrixHeight;

        private System.Random _random = new();
        private int _sourceGridWidth;
        private int _sourceGridArea;


        public BlackWhiteMatrixFactory
        (
            int[,] grid, bool printMatrixShape = false
        )
        : this(
            grid.GetWidth(), grid.GetHeight(),
            printMatrixShape
        ) {}

        public BlackWhiteMatrixFactory
        (
            int gridWidth, int gridHeight,
            bool printMatrixShape = false
        )
        {
            _sourceGridWidth = gridWidth;
            _sourceGridArea = gridWidth * gridHeight;

            double maxArea = Math.Ceiling(_sourceGridArea / 2f);

            MatrixWidth = (int)Math.Ceiling(Math.Sqrt(maxArea));
            MatrixHeight = MatrixWidth;
            
            while (maxArea < (MatrixHeight * (MatrixWidth - 1)))
            {
                MatrixHeight--;
            }

            if (printMatrixShape)
            {
                Debug.Log(
                    $"There are {MatrixHeight} rows and {MatrixWidth}"
                    + $" columns in the BW matrices"
                );
            }
        }


        public (int[,] blackMatrix, int[,] whiteMatrix) CreateShuffledMatrices
        (
            bool printTaggedLists = false
        )
        {
            var gridIndices = Enumerable.Range(0, _sourceGridArea);
            int[] shuffledArray = gridIndices.OrderBy(
                _ => _random.Next()
            ).ToArray();

            int[] blackList = new int[MatrixArea];
            blackList.Fill(-1);
            blackList.FillFrom(
                shuffledArray.Where(IndexIsBlack).ToArray()
            );

            int[] whiteList = new int[MatrixArea];
            whiteList.Fill(-1);
            whiteList.FillFrom(
                shuffledArray.Where(IndexIsWhite).ToArray()
            );

            if (printTaggedLists)
            {
                Debug.Log($"Black indices: [{string.Join(", ", blackList)}]");
                Debug.Log($"White indices: [{string.Join(", ", whiteList)}]");
            }

            return (
                blackList.To2D(MatrixHeight, MatrixWidth),
                whiteList.To2D(MatrixHeight, MatrixWidth)
            );
        }
        

        private bool IndexIsBlack(int index)
        {
            if (_sourceGridWidth % 2 == 1)
                return index % 2 == 0;
            else
            {
                int rowNumber = index / _sourceGridWidth;
                return ((index - (rowNumber % 2)) % 2) == 0;
            };
        }
        private bool IndexIsWhite(int i) => !IndexIsBlack(i);
    }
}
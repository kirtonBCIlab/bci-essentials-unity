using UnityEngine;

namespace BCIEssentials.Utilities
{
    public class ColorContrast : MonoBehaviour 
    {
        private Color flashOnColor;
        private int contrastLevel;

        public void SetContrast(int _contrastLevel)
        {
            contrastLevel = _contrastLevel;
        }

        public int GetContrast()
        {
            return contrastLevel;
        }

        // public Color Red()
        // {      
        //     //R = 255 B/G = 206, L = 217 , 14.97:1 #FFCECE
        //     Color maxColor = new Color(1f, 0.8078f, 0.8078f, 1f);

        //     //R = 255, G/B = 107, Luminance = 170, 7.56:1, #FF6B6B
        //     Color halfColor = new Color(1F, 0.4196f, 0.4196f, 1f);

        //     //R = 210, G/B = 0, Luminance = 99, 3.74:1, #D20000
        //     Color quarterColor = new Color(0.8235f, 0f, 0f, 1f);

        //     //R = 97 G/B = 0, Luminance = 46, 1.5:1, #610000
        //     Color tenthColor = new Color(0.3804f, 0f, 0f, 1f);
    
        //     if(GetContrast() == 100)
        //     {
        //         flashOnColor = maxColor;
        //     }

        //     if(GetContrast() == 50)
        //     {
        //         flashOnColor = halfColor;
        //     }

        //     if(GetContrast() == 25)
        //     {
        //         flashOnColor = quarterColor;
        //     }

        //     if(GetContrast() == 10)
        //     {
        //         flashOnColor = tenthColor;
        //     }
        //     return flashOnColor;
        // }

        //Equal log steps 2, 3.91, 7.66, 15
        public Color Grey()
        {
            //R/G/B = 218, Luminance = 205, 15.02:1, #DADADA
            Color _maximum = new Color(0.8549f, 0.8549f, 0.8549f, 1f);

            //R/G/B = 156, Luminance = 147, 7.64:1, #9C9CBC
            Color step1 = new Color(0.601176f, 0.6011176f, 0.601176f, 1f);

            //R/G/B = 107, Luminance = 101, #6B6B6B
            Color step2 = new Color(0.4196f, 0.4196f, 0.4196f, 1f);

            //R/G/B = 63, L = 59, 1.99:1, #3F3F3F
            Color _minimum = new Color(0.2471f, 0.2471f, 0.2471f, 1f);

            Color _off = new Color(0,0,0,0);

            if(GetContrast() == 100)
            {
                flashOnColor = _maximum;
            }

            if(GetContrast() == 50) 
            {
                flashOnColor = step1;
            }

            if(GetContrast() == 25)
            {
                flashOnColor = step2;
            }
            
            if(GetContrast() == 10)
            {
                flashOnColor = _minimum;
            }

            if(GetContrast() == 0)
            {
                flashOnColor = _off;
            }

            return flashOnColor;
        }

        //Equal Log steps 2, 3.91, 7.66, 15
        public Color Green()
        {
            //R 0, G 253, B 0, L 119, 15.05:1, #00FD00
            Color _maximum = new Color(0f, 0.9922f, 0f, 1f);

            //R 0 , G 182, B 0 , L 86, 7.69:1, #00B600
            Color step1 = new Color(0f, 0.7137f, 0f, 1f);

            //R 0, G 125, B 0 , L 59, 3.91:1, #007D00
            Color step2 = new Color(0f, 0.4902f, 0f, 1f);

            //R 0, G 75, B0, L24, 2:1, #004B00
            Color _minimum = new Color(0f, 0.2941f, 0f, 1f);

            Color _off = new Color(0,0,0,0);

            if(GetContrast() == 100)
            {
                flashOnColor = _maximum;
            }

            if(GetContrast() == 50) 
            {
                flashOnColor = step1;
            }

            if(GetContrast() == 25)
            {
                flashOnColor = step2;
            }
            
            if(GetContrast() == 10)
            {
                flashOnColor = _minimum;
            }

            if(GetContrast() == 0)
            {
                flashOnColor = _off;
            }
            return flashOnColor;
        }

//         public Color Blue()
//         {
//             //R 214, G 214, B 255, L 221, 15.05:1, #D6D6FF
//             Color maxColor = new Color(0.8392f, 0.8392f, 1f, 1f);

//             //R 143, G 143, B 255, L 187, 7.54:1, #8F8FFF
//             Color halfColor = new Color(0.56078f, 0.56078f, 1f, 1f);

//             //R 75, G 75, B 155, L 155, 3.74:1, #4B4BFF
//             Color quarterColor = new Color(0.2941f, 0.2941f, 1f, 1f);

//             //R 0, G 0, B 163, L 77, 1.52:1, #0000A3
//             Color tenthColor = new Color(0f, 0f, 0.6392f, 1f);

//             if(GetContrast() == 100)
//             {
//                 flashOnColor = maxColor;
//             }

//             if(GetContrast() == 50) 
//             {
//                 flashOnColor = halfColor;
//             }

//             if(GetContrast() == 25)
//             {
//                 flashOnColor = quarterColor;
//             }
            
//             if(GetContrast() == 10)
//             {
//                 flashOnColor = tenthColor;
//             }
//             return flashOnColor;
//         }
    }
 }

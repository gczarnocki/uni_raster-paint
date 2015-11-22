using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace RasterPaint.Objects
{
    public class Node
    {
        public const int NodesCount = 8;

        public Color? Color { get; set; }
        public int ColorCount { get; set; } = 0;
        public bool Leaf { get; set; } = false;
        public int Level { get; set; }

        public Node[] Children;

        public Node()
        {
            Children = new Node[NodesCount]; // initialized to null;
        }
    }

    public class Octree
    {
        public const int NodesCount = 8;

        public Node Root { get; set; }
        public int ColorsCount { get; set; } = 0;

        public Octree(WriteableBitmap wbm)
        {
            GenerateOctreeForBitmap(wbm);
        }

        public void Insert(Color color)
        {
            int[] indexes = GetIndexesForPixel(color);

            if (Root == null)
            {
                Initialize();
            }

            Node lastNode = Root;

            for (int i = 0; i < indexes.Length; i++)
            {
                if (lastNode != null)
                {
                    var node = InsertInternal(ref lastNode.Children[indexes[i]], color, i + 1);
                    lastNode = node;
                }
            }
        }

        public void GenerateOctreeForBitmap(WriteableBitmap wbm)
        {
            unsafe
            {
                using (var context = wbm.GetBitmapContext())
                {
                    for (int i = 0; i < context.Width; i++)
                    {
                        for (int j = 0; j < context.Height; j++)
                        {
                            var c = context.Pixels[j * context.Width + i];
                            var a = (byte)(c >> 24);

                            // Prevent division by zero
                            int ai = a;
                            if (ai == 0)
                            {
                                ai = 1;
                            }

                            // Scale inverse alpha to use cheap integer mul bit shift
                            ai = ((255 << 8) / ai);
                            var color = Color.FromArgb(a,
                                (byte)((((c >> 16) & 0xFF) * ai) >> 8),
                                (byte)((((c >> 8) & 0xFF) * ai) >> 8),
                                (byte)((((c & 0xFF) * ai) >> 8)));

                            Insert(color);
                        }
                    }
                }
            }
        }

        public void ReduceOctree(int k)
        {
            
        }

        public Node InsertInternal(ref Node node, Color c, int level)
        {
            // returns a node which was created;

            if (node == null)
            {
                node = new Node();
            }
            
            node.Level = level;

            if (level == 8)
            {
                if (node.Color == c)
                {
                    node.ColorCount++;
                }
                else
                {
                    node.Color = c;
                    node.ColorCount = 1;
                    ColorsCount++;
                }

                node.Leaf = true;
            }

            return node;
        }

        public static int[] GetIndexesForPixel(Color c)
        {
            int[] indexesArray = new int[NodesCount];

            for (int k = NodesCount - 1; k >= 0; k--)
            {
                indexesArray[k] = GetIndexForPixel(k, c);
            }

            return indexesArray;
        }

        public static int GetIndexForPixel(int k, Color c)
        {
            var r = (c.R & (1 << k)) > 0 ? 1 : 0;
            var g = (c.G & (1 << k)) > 0 ? 1 : 0;
            var b = (c.B & (1 << k)) > 0 ? 1 : 0;

            return b | g << 1 | r << 2;
        }

        public void Initialize()
        {
            Root = new Node();
            Root.Children = new Node[8];
        }
    }
}

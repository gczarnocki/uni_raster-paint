using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace RasterPaint.Objects
{
    public static class Identifiers
    {
        public static uint Identifier { get; set; } = 1;

        public static uint NewId()
        {
            return Identifier++;
        }
    }

    public class Node : IComparable<Node>
    {
        public const int NodesCount = 8;

        public Color? Color { get; set; }
        public uint PixelsCount { get; set; } = 0;
        public bool Leaf { get; set; } = false;
        public Node Parent { get; set; }
        public int Level { get; set; }
        public uint Id { get; set; }

        public uint R { get; set; }
        public uint G { get; set; }
        public uint B { get; set; }

        public Node[] Children;

        public Node()
        {
            Children = new Node[NodesCount]; // initialized to null;
            Id = Identifiers.NewId();
        }

        public int CompareTo(Node other)
        {
            if (PixelsCount == other.PixelsCount) return 0;

            return PixelsCount < other.PixelsCount ? -1 : 1;
        }

        public void RemoveChildren(out uint colorsToRemove, ref List<Node>[] allLevelsArray) // for a given node;
        {
            colorsToRemove = 0;

            if (Children != null)
            {
                uint childrenSum = (uint)Children.Count(x => x != null); // elements checked here;
                uint pixelsCount = (uint)Children.Where(x => x != null).Sum(x => x.PixelsCount);
                colorsToRemove = childrenSum;

                if (childrenSum > 0)
                {
                    foreach(var child in Children.Where(c => c != null))
                    {
                        R += child.R;
                        G += child.G;
                        B += child.B;

                        allLevelsArray[Level + 1].Remove(child);
                    }

                    R /= childrenSum;
                    G /= childrenSum;
                    B /= childrenSum;
                }

                Children = null;

                Leaf = true;
                PixelsCount = pixelsCount;
            }
            
        }
    }

    public class Octree
    {
        public const int NodesCount = 8;

        private List<Node>[] allLevelsArray;

        // this is an array with lists of all nodes from all levels;

        public Node Root { get; set; }
        public uint ColorsCount { get; set; } = 0;

        public Octree(WriteableBitmap wbm)
        {
            InitializeAllLevelsArray();
            GenerateOctreeForBitmap(wbm);
            
            if(Root != null)
            {
                Root.Parent = null;
            }
        }

        public List<Node>[] AllLevelsArray
        {
            get { return allLevelsArray; }
        }


        private void InitializeAllLevelsArray()
        {
            allLevelsArray = new List<Node>[NodesCount + 1];

            for(int i = 0; i < allLevelsArray.Count(); i++)
            {
                allLevelsArray[i] = new List<Node>();
            }
        }

        private void Insert(Color color)
        {
            int[] indexes = GetIndexesForPixel(color);

            if (Root == null)
            {
                InitializeRoot();
            }

            Node lastNode = Root;

            for (int i = 0; i < indexes.Length; i++)
            {
                if (lastNode != null)
                {
                    var node = InsertInternal(ref lastNode.Children[indexes[i]], lastNode, ref allLevelsArray, color, i + 1);
                    lastNode = node;
                }
            }
        }

        private void GenerateOctreeForBitmap(WriteableBitmap wbm)
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

                            int ai = a;
                            if (ai == 0)
                            {
                                ai = 1;
                            }

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
            for (int i = NodesCount; i >= 1; i--)
            {
                ReduceOctreeInternal(k, level: i, allLevelsArray: ref allLevelsArray); // redukcja danego poziomu;
            }
        }

        private void ReduceOctreeInternal(int k, int level, ref List<Node>[] allLevelsArray)
        {

            foreach(var item in allLevelsArray[level].OrderBy(x => x.PixelsCount).Select(x => x.Parent).Distinct().AsParallel()) // level--;
            {
                uint colorsToRemove;
                item.RemoveChildren(out colorsToRemove, ref allLevelsArray);

                allLevelsArray[level - 1].Add(item);

                ColorsCount = ColorsCount - (colorsToRemove - 1);
            }
        }

        private Node InsertInternal(ref Node node, Node parent, ref List<Node>[] allLevelsArray, Color c, int level)
        {
            // returns a node which was created;

            if (node == null)
            {
                node = new Node();
            }
            
            node.Level = level;
            node.Parent = parent;

            if (level == 8)
            {
                if (node.Color == c)
                {
                    node.PixelsCount++;
                }
                else
                {
                    node.Color = c;
                    node.PixelsCount = 1;
                    ColorsCount++;

                    allLevelsArray[level].Add(node);
                }

                node.R += c.R;
                node.G += c.G;
                node.B += c.B;

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

        private void InitializeRoot()
        {
            Root = new Node();
            Root.Children = new Node[8];
        }
    }
}

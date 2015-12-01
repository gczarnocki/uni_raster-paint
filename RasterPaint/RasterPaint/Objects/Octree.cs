using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace RasterPaint.Objects
{
    public static class IdHelper
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

        private static readonly Byte[] Mask = new Byte[] { 0x80, 0x40, 0x20, 0x10, 0x08, 0x04, 0x02, 0x01 };

        public Color? Color { get; set; }
        public uint PixelsCount { get; set; }
        public bool Leaf { get; set; }
        public Node Parent { get; set; }
        public int Level { private get; set; }
        private uint Id { get; set; }
        private bool IsDeleted { get; set; } = false;

        public uint R { get; set; }
        public uint G { get; set; }
        public uint B { get; set; }

        public Node[] Children;

        public Node()
        {
            Children = new Node[NodesCount]; // initialized to null;
            Id = IdHelper.NewId();
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
                // List<Node> childrenToRemove = new List<Node>();
                var children = Children.Where(x => x != null).ToList();

                uint childrenSum = (uint)children.Count();
                uint pixelsCount = (uint)children.Sum(x => x.PixelsCount);
                colorsToRemove = childrenSum - 1;

                if (childrenSum > 0)
                {
                    foreach (var child in children)
                    {
                        R += child.R;
                        G += child.G;
                        B += child.B;

                        child.IsDeleted = true;
                        // childrenToRemove.Add(child);
                    }

                    if (childrenSum > 1)
                    {
                        R /= childrenSum;
                        G /= childrenSum;
                        B /= childrenSum;
                    }

                    /* foreach (var item in childrenToRemove.AsParallel())
                    {
                        allLevelsArray[Level + 1].Remove(item);
                    } */
                }

                Children = null;

                Leaf = true;
                PixelsCount = pixelsCount;
            }
        }
    }

    public class Octree
    {
        private static readonly Byte[] Mask = new byte[] { 0x80, 0x40, 0x20, 0x10, 0x08, 0x04, 0x02, 0x01 };
        private const int NodesCount = 8;

        private List<Node>[] _allLevelsArray; // this is an array with lists of all nodes from all levels;

        private Node Root { get; set; }
        public uint ColorsCount { get; set; }
        public WriteableBitmap LoadedBitmap { get; set; }

        public Octree(WriteableBitmap wbm)
        {
            LoadedBitmap = wbm.Clone();

            InitializeAllLevelsArray();
            GenerateOctreeForBitmap(wbm);
            
            if(Root != null)
            {
                Root.Parent = null;
            }
        }

        private void InitializeRoot()
        {
            Root = new Node { Children = new Node[8] };
        }

        private void InitializeAllLevelsArray()
        {
            _allLevelsArray = new List<Node>[NodesCount + 1];

            for(int i = 0; i < _allLevelsArray.Count(); i++)
            {
                _allLevelsArray[i] = new List<Node>();
            }

            // _allLevelsArray[0].Add(Root);
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
                    var node = InsertInternal(ref lastNode.Children[indexes[i]], lastNode, color, i + 1);
                    lastNode = node;
                }
            }
        }

        // private Node InsertInternal(ref Node node, Node parent, ref List<Node>[] levelsArray, Color c, int level)
        private Node InsertInternal(ref Node node, Node parent, Color c, int level)
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
                    node.R = c.R;
                    node.G = c.G;
                    node.B = c.B;
                    node.Color = c;

                    node.PixelsCount = 1;
                    ColorsCount++;

                    _allLevelsArray[level].Add(node);
                }

                node.Leaf = true;
            }

            return node;
        }

        private void ReduceOctree(int colorsCount)
        {
            for (int i = NodesCount; i >= 1; i--)
            {
                ReduceOctreeInternal(colorsCount, i);
                Trace.WriteLine($"After reduction: {ColorsCount} [i = {i}]");

                if (ColorsCount <= colorsCount)
                {
                    Trace.WriteLine($"Colors Count: {ColorsCount}");
                    break;
                }
            }
        }

        private void ReduceOctreeInternal(int colorsCount, int level) // redukcja tego poziomu przez rodziców;
        {
            var parents = _allLevelsArray[level].OrderBy(x => x.PixelsCount).Select(x => x.Parent).Distinct();

            foreach (var item in parents)
            {
                uint colorsToRemove;
                item.RemoveChildren(out colorsToRemove, ref _allLevelsArray);

                _allLevelsArray[level - 1].Add(item);

                ColorsCount -= colorsToRemove;

                if (ColorsCount <= colorsCount) // wychodzę z funkcji, gdy już jest okej;
                {
                    Trace.WriteLine($"Break. ColorsCount = {ColorsCount}, k = {colorsCount}");
                    break;
                }
            }
        }

        public WriteableBitmap GenerateBitmapFromOctree(int colorsCount)
        {
            ReduceOctree(colorsCount);

            unsafe
            {
                using (var context = LoadedBitmap.GetBitmapContext())
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

                            var indexes = GetIndexesForPixel(color);

                            var newColor = GetNewColorFromIndexes(indexes);

                            context.Pixels[j * context.Width + i] = (255 << 24) | (newColor.R << 16) | (newColor.G << 8) | newColor.B;
                        }
                    }
                }

                return LoadedBitmap;
            }
        }

        private Color GetNewColorFromIndexes(int[] indexes)
        {
            int i = 0;
            Node node = Root;

            while (!node.Leaf)
            {
                var index = indexes[i++];
                node = node.Children[index];
            }

            if (node.Leaf)
            {
                return Color.FromArgb((int)node.R, (int)node.G, (int)node.B);
            }

            throw new ArgumentOutOfRangeException();
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

        public static int GetIndexForPixel(int level, Color color)
        {
            //var r = (c.R & (1 << k)) > 0 ? 1 : 0;
            //var g = (c.G & (1 << k)) > 0 ? 1 : 0;
            //var b = (c.B & (1 << k)) > 0 ? 1 : 0;

            //return b | g << 1 | r << 2;

            return 
                ((color.R & Mask[level]) == Mask[level] ? 4 : 0) |
                ((color.G & Mask[level]) == Mask[level] ? 2 : 0) |
                ((color.B & Mask[level]) == Mask[level] ? 1 : 0);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RasterPaint.Utilities;

namespace RasterPaint.Objects
{
    public class Node : IComparable<Node>
    {
        public const int NodesCount = 8;

        private static readonly byte[] Mask = new byte[] { 0x80, 0x40, 0x20, 0x10, 0x08, 0x04, 0x02, 0x01 };

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
                    }

                    if (childrenSum > 1)
                    {
                        R /= childrenSum;
                        G /= childrenSum;
                        B /= childrenSum;
                    }
                }

                Children = null;

                Leaf = true;
                PixelsCount = pixelsCount;
            }
        }

        public void RemoveChildrenForTests(int colorsCountToBe, ref uint currentColorsCount)
        {
            // Jeżeli dzieci jest tyle, że usunięcie ich (czyli usunięcie kolorów) nie sprawi, że osiągniemy
            // wartości kolorów, której poszukujemy, można wykonać pełną redukcję (wszystkich dzieci).  Zna-
            // czy to, że Children == null, Leaf = true, aktualizacja PixelCount; sprawdzamy to do  momentu,
            // aż usunięcie wszystkich dzieci może wyjść poza poszukiwaną wartość; jeśli tak się dzieje,  to
            // tworzymy nowy węzeł, gdzie zgromadzimy  wszystkie wartości, by potem wpisać je  do pierwszego
            // dziecka, które nie zostało jeszcze usunięte;

            if (currentColorsCount != colorsCountToBe)
            {
                if (Children != null)
                {
                    var children = Children.Where(x => x != null).ToList();

                    uint childrenSum = (uint)children.Count();
                    uint pixelsCount = (uint)children.Sum(x => x.PixelsCount);

                    if (childrenSum > 0) // mamy jakiekolwiek dzieci;
                    {
                        if (currentColorsCount - childrenSum + 1 >= colorsCountToBe) // redukcja pełna;
                        // usuwamy tyle kolorów, ile dzieci, ale dodajemy jednego rodzica;
                        {
                            foreach (var child in children)
                            {
                                R += child.R;
                                G += child.G;
                                B += child.B;

                                child.IsDeleted = true;
                            }

                            if (childrenSum > 1)
                            {
                                R /= childrenSum;
                                G /= childrenSum;
                                B /= childrenSum;
                            }

                            Leaf = true;
                            Children = null;
                            PixelsCount = pixelsCount;

                            currentColorsCount = currentColorsCount - childrenSum + 1;
                        }
                        else // redukcja z przekazaniem wartości jednemu z dzieci;
                        {
                            Node node = new Node
                            {
                                Leaf = true,
                                Level = Level,
                                Children = null,
                                IsDeleted = false,
                            };

                            uint counter = 0;

                            foreach (var child in children)
                            {
                                node.R += child.R;
                                node.G += child.G;
                                node.B += child.B;
                                node.PixelsCount += child.PixelsCount;

                                child.IsDeleted = true;

                                currentColorsCount--;
                                counter++;

                                if (currentColorsCount == colorsCountToBe)
                                {
                                    break;
                                }
                            }

                            var firstNonDeletedChild = children.FirstOrDefault(x => x.IsDeleted == false); // find the first child that is not deleted;

                            if (firstNonDeletedChild != null)
                            {
                                counter++; // because we want have also firstNonDeletedChild;

                                firstNonDeletedChild.R += node.R;
                                firstNonDeletedChild.G += node.G;
                                firstNonDeletedChild.B += node.B;
                                firstNonDeletedChild.PixelsCount += node.PixelsCount;

                                firstNonDeletedChild.R /= counter;
                                firstNonDeletedChild.G /= counter;
                                firstNonDeletedChild.B /= counter;
                            }
                            else
                            {
                                Trace.WriteLine("Exceptionally.");
                            }
                        }
                    }
                }
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Xml.Serialization;

namespace RasterPaint.Objects
{
    public class SerializableObject
    {
        public List<MyPolygon> AllPolygons = new List<MyPolygon>();
        public List<MyLine> AllLines = new List<MyLine>();
        public List<MyPoint> AllPoints = new List<MyPoint>();
        public Color BackgroundColor;
        public Color GridColor;
        public int GridSize;
        public bool ShowGrid;

        public SerializableObject()
        {
            
        }  

        public SerializableObject(List<MyObject> listOfAllObjects, Color backgroundColor, Color gridColor, int gridSize, bool showGrid)
        {
            foreach (var item in listOfAllObjects)
            {
                if(item is MyPolygon) AllPolygons.Add(item as MyPolygon);
                else if (item is MyPoint) AllPoints.Add(item as MyPoint);
                else if(item is MyLine) AllLines.Add(item as MyLine);
            }

            BackgroundColor = backgroundColor;
            GridColor = gridColor;
            GridSize = gridSize;
            ShowGrid = showGrid;
        }
    }
}

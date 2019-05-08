using System;
using System.Drawing;

namespace GpxPathViewer
{
    public class TimeNode : Node
    {
        public DateTime Time { get; set; }
    }

    public class Node
    {
        public long NodeId { get; set; }
        public double Lat { get; set; }
        public double Lon { get; set; }

        public Point Loc { get; set; }

        public double DistanceTo(Node node) => DistanceTo(Lat, Lon, node.Lat, node.Lon);

        private static double DistanceTo(double lat1, double lon1, double lat2, double lon2, char unit = 'K')
        {
            if (lat1 == lat2 && lon1 == lon2)
                return 0;

            var rlat1 = Math.PI * lat1 / 180;
            var rlat2 = Math.PI * lat2 / 180;
            var theta = lon1 - lon2;
            var rtheta = Math.PI * theta / 180;
            var dist =
                Math.Sin(rlat1) * Math.Sin(rlat2) + Math.Cos(rlat1) *
                Math.Cos(rlat2) * Math.Cos(rtheta);
            dist = Math.Acos(dist);
            dist = dist * 180 / Math.PI;
            dist = dist * 60 * 1.1515;

            switch (unit)
            {
                case 'K': //Kilometers -> default
                    return dist * 1.609344;
                case 'N': //Nautical Miles 
                    return dist * 0.8684;
                case 'M': //Miles
                    return dist;
            }

            return dist;
        }
    }
}
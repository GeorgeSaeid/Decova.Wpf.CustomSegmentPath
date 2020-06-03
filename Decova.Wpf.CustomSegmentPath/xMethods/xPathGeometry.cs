using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TextOnAPath
{
    public static class xPathGeometry
    {
        public static List<Point> GetSegmentsIntersectionsWithPath(this PathGeometry flattenedPath,  double segmentLength)
        {
            List<Point> intersectionPoints = new List<Point>();


            #region get path effective points
            //####################################################################
            List<Point> effectivePoints = flattenedPath.GetDirectionEffectivePoints();

            if (effectivePoints == null || effectivePoints.Count < 2)
                return intersectionPoints;

            //####################################################################
            #endregion

            Point currentSegmentStartPoint = effectivePoints[0];
            intersectionPoints.Add(currentSegmentStartPoint);

            // find point on flattened path that is segment length away from current point
            int effectivePointIndex = 0;
            int segmentIndex = 1;

            while (effectivePointIndex < effectivePoints.Count - 1)
            {
                Point effectivePoint = effectivePoints[effectivePointIndex];
                Point nextEffectivePoint = effectivePoints[effectivePointIndex + 1];

                // Draw the hypothetical circle whose center as the current effective point and with a radius
                // of the the segment length. Then try each line between each two successive effective point
                // until getting an intersection with the circle.
                Point? intersectionPoint = 
                    GetIntersectionOfSegmentAndCircle( effectivePoint,
                                                       nextEffectivePoint, 
                                                       circleCenter: currentSegmentStartPoint, 
                                                       circleRadius: segmentLength);


                #region if no intersection between the line of current two successive effective points and the circle,
                // then just try the next line.
                //####################################################################
                if (intersectionPoint == null)
                {
                    effectivePointIndex++;
                }
                //####################################################################
                #endregion

                else
                {
                    intersectionPoints.Add((Point)intersectionPoint);

                    // Start with the end of the last segment to be the start
                    // of the next one.
                    currentSegmentStartPoint = (Point)intersectionPoint;


                    // Set the next segment start point as the intersection point of the 
                    // last drawn line instead of the latest effective point to draw the next circle
                    // from.
                    effectivePoints[effectivePointIndex] = currentSegmentStartPoint;
                    segmentIndex++;
                }
            }

            return intersectionPoints;
        }

        static List<Point> GetDirectionEffectivePoints(this PathGeometry flattenedPath)
        {
            List<Point> flattenedPathPoints = new List<Point>();

            // for flattened geometry there should be just one PathFigure in the Figures
            if (flattenedPath.Figures.Count != 1)
                return null;

            PathFigure pathFigure = flattenedPath.Figures[0];

            flattenedPathPoints.Add(pathFigure.StartPoint);

            // SegmentsCollection should contain PolyLineSegment and LineSegment
            foreach (PathSegment pathSegment in pathFigure.Segments)
            {
                #region if PolyLine segment / add each point on it
                //####################################################################
                if (pathSegment is PolyLineSegment)
                {
                    PolyLineSegment seg = pathSegment as PolyLineSegment;

                    foreach (Point point in seg.Points)
                    {
                        flattenedPathPoints.Add(point);
                    }
                }
                //####################################################################
                #endregion

                #region if line segment / add its end point
                //####################################################################
                else if (pathSegment is LineSegment)
                {
                    LineSegment seg = pathSegment as LineSegment;
                    flattenedPathPoints.Add(seg.Point);
                }
                //####################################################################
                #endregion

                else
                    throw new Exception("GetIntersectionPoint - unexpected path segment type: " + pathSegment.ToString());

            }

            return (flattenedPathPoints);
        }

        static Point? GetIntersectionOfSegmentAndCircle(Point segmentPoint1,
                                                        Point segmentPoint2,
                                                        Point circleCenter,
                                                        double circleRadius)
        {
            // linear equation for segment: y = mx + b
            double slope = (segmentPoint2.Y - segmentPoint1.Y) / (segmentPoint2.X - segmentPoint1.X);
            double intercept = segmentPoint1.Y - (slope * segmentPoint1.X);

            // special case when segment is vertically oriented
            if (double.IsInfinity(slope))
            {
                double root = Math.Pow(circleRadius, 2.0) - Math.Pow(segmentPoint1.X - circleCenter.X, 2.0);

                if (root < 0)
                    return null;

                // soln 1
                double SolnX1 = segmentPoint1.X;
                double SolnY1 = circleCenter.Y - Math.Sqrt(root);
                Point Soln1 = new Point(SolnX1, SolnY1);

                // have valid result if point is between two segment points
                if (SolnX1.IsWithinInclusive(segmentPoint1.X, segmentPoint2.X) &&
                    SolnY1.IsWithinInclusive(segmentPoint1.Y, segmentPoint2.Y))
                //if (ValidSoln(Soln1, SegmentPoint1, SegmentPoint2, CircleCenter))
                {
                    // found solution
                    return (Soln1);
                }

                // soln 2
                double SolnX2 = segmentPoint1.X;
                double SolnY2 = circleCenter.Y + Math.Sqrt(root);
                Point Soln2 = new Point(SolnX2, SolnY2);

                // have valid result if point is between two segment points
                if (SolnX2.IsWithinInclusive(segmentPoint1.X, segmentPoint2.X) &&
                    SolnY2.IsWithinInclusive(segmentPoint1.Y, segmentPoint2.Y))
                //if (ValidSoln(Soln2, SegmentPoint1, SegmentPoint2, CircleCenter))
                {
                    // found solution
                    return (Soln2);
                }
            }
            else
            {
                // use soln to quadradratic equation to solve intersection of segment and circle:
                // x = (-b +/ sqrt(b^2-4ac))/(2a)
                double a = 1 + Math.Pow(slope, 2.0);
                double b = (-2 * circleCenter.X) + (2 * (intercept - circleCenter.Y) * slope);
                double c = Math.Pow(circleCenter.X, 2.0)
                         + Math.Pow(intercept - circleCenter.Y, 2.0)
                         - Math.Pow(circleRadius, 2.0);

                // check for no solutions, is sqrt negative?
                double root = Math.Pow(b, 2.0) - (4 * a * c);

                if (root < 0)
                    return null;

                // we might have two solns...

                // soln 1
                double SolnX1 = (-b + Math.Sqrt(root)) / (2 * a);
                double SolnY1 = slope * SolnX1 + intercept;
                Point Soln1 = new Point(SolnX1, SolnY1);

                // have valid result if point is between two segment points
                if (SolnX1.IsWithinInclusive(segmentPoint1.X, segmentPoint2.X) &&
                    SolnY1.IsWithinInclusive(segmentPoint1.Y, segmentPoint2.Y))
                {
                    // found solution
                    return (Soln1);
                }

                // soln 2
                double SolnX2 = (-b - Math.Sqrt(root)) / (2 * a);
                double SolnY2 = slope * SolnX2 + intercept;
                Point Soln2 = new Point(SolnX2, SolnY2);

                // have valid result if point is between two segment points
                if (SolnX2.IsWithinInclusive(segmentPoint1.X, segmentPoint2.X) &&
                    SolnY2.IsWithinInclusive(segmentPoint1.Y, segmentPoint2.Y))
                {
                    // found solution
                    return (Soln2);
                }
            }

            // shouldn't get here...but in case
            return null;
        }
    }
}

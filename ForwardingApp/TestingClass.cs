using System;
using System.Collections.Generic;

namespace ASolute_Mobile
{
    public class TestingClass
    {
        public class People
        {
            public string name { get; set; }
        }


        public class Word
        {
            public string boundingBox { get; set; }
            public string text { get; set; }
        }

        public class Line
        {
            public string boundingBox { get; set; }
            public List<Word> words { get; set; }
        }

        public class Region
        {
            public string boundingBox { get; set; }
            public List<Line> lines { get; set; }
        }

        public class testing
        {
            public string language { get; set; }
            public double textAngle { get; set; }
            public string orientation { get; set; }
            public List<Region> regions { get; set; }
        }

        public class Image
        {
            public string content { get; set; }
        }

        public class Feature
        {
            public string type { get; set; }
            public int maxResults { get; set; }
        }

        public class Request
        {
            public Image image { get; set; }
            public List<Feature> features { get; set; }
        }

        public class RootObject
        {
            public List<Request> requests { get; set; }
        }

        public class Vertex
        {
            public int x { get; set; }
            public int y { get; set; }
        }

        public class BoundingPoly
        {
            public List<Vertex> vertices { get; set; }
        }

        public class TextAnnotation
        {
            public string locale { get; set; }
            public string description { get; set; }
            public BoundingPoly boundingPoly { get; set; }
        }

        public class Respons
        {
            public List<TextAnnotation> textAnnotations { get; set; }
        }

        public class Google
        {
            public List<Respons> responses { get; set; }
        }
    }
}

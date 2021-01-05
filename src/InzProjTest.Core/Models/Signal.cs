using System;

namespace InzProjTest.Core.Models
{
    public class Signal
    {
        public string Filename { get; set; }
        public string Filepath { get; set; }
        public float[] Data { get; set; }
        public int SampleRate { get; set; }
    }
}

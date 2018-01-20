﻿namespace SkeletonGameManager.WPF.ViewModels.Machine
{
    [PropertyChanged.AddINotifyPropertyChangedInterface]
    public class SwitchViewModel
    {
        public string Name { get; set; }
        public string Number { get; set; }
        public string Tags { get; set; }
        public string Type { get; set; }

        public string[] BallSearch = new string[] { null, null };

        private bool reset;
        public bool Reset
        {
            get { return reset; }
            set
            {
                reset = value;
                if (Reset) BallSearch[0] = "reset";
                else BallSearch[0] = null;
            }
        }

        private bool stop;
        public bool Stop
        {
            get { return stop; }
            set
            {
                stop = value;
                if (Stop) BallSearch[1] = "stop";
                else BallSearch[1] = null;
            }
        }
    }
}

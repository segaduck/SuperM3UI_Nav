using System.Collections.Generic;

namespace SuperM3UI_Nav.ViewModel
{
    class ControlVM : Utilities.ViewModelBase
    {
        public static List<InputOption> inputOptions = new List<InputOption>
        {
            new InputOption { index =0, name = "Common Controls"},    // 0
            new InputOption { index =1, name = "Spikeout"},           // 1
            new InputOption { index =2, name = "Fighting Games-1P"},  // 2
            new InputOption { index =3, name = "Fighting Games-2P"},  // 3
            new InputOption { index =4, name = "Virtua Striker-1P"},  // 4
            new InputOption { index =5, name = "Virtua Striker-2P"},  // 5
            //new InputOption { index =6, name = "Racing Games"},
            //new InputOption { index =7, name = "Virtual On"},
        };

        public class InputOption
        {
            public int index { get; set; }
            public string name { get; set; }
        }

        public static List<SpikeoutOption> spikeoutOptions = new List<SpikeoutOption>
        {
            new SpikeoutOption { index =0, name = "Single Mode"},            // 0
            new SpikeoutOption { index =1, name = "Master Mode (Network)"},  // 1
            new SpikeoutOption { index =2, name = "Slave Mode (Network)"},   // 2
        };

        public class SpikeoutOption
        {
            public int index { get; set; }
            public string name { get; set; }
        }
    }
}

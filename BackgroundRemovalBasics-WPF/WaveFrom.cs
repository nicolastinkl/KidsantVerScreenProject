using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Microsoft.Samples.Kinect.BackgroundRemovalBasics
{
    public class GetOutEventArgs : System.EventArgs
    {
        public string waveContent;

    }
    public partial class WaveFrom : Form
    {
     

        public event EventHandler<GetOutEventArgs> OnReceiveSuccess;
        public WaveFrom()
        {
            InitializeComponent();
        }

        private void WaveFrom_Load(object sender, EventArgs e)
        {

           // SoundUtil.SoundUtil.Recording(new SoundUtil.SoundUtil.SoundReceivedDelegate(OnSoundReceived));
             
           
        }

        public void sendWaveID(Int32 id)
        {
            if (id > 0 )
            { 
                SoundUtil.SoundUtil.Sounding(id);
            }
        }
        
        delegate void delOnSoundReceived(Int32 id);
        public void OnSoundReceived(Int32 id)
        {
            
            if (id > 0)
            {
                String value = Convert.ToString(id);
                Console.WriteLine(value); 
                GetOutEventArgs events = new GetOutEventArgs();
                events.waveContent = value;
                if (value != null)
                    OnReceiveSuccess(this, events);
            }
        }

        
    }
}

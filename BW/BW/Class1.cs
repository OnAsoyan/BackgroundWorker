using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BW
{
    public class CustomBackgroundWorker
    {
        //
        private static readonly object objLock = new object();
        public bool CancellationPending { get; private set; }
        public bool IsBusy { get; private set; } = false;
        public bool WorkerReportsProgress { get; set; }
        public bool WorkerSupportsCancellation { get; set; }
        private DoWorkEventArgs doWorkEventArgs = new DoWorkEventArgs();
        private RunWorkerCompletedEventArgs runWorkerCompletedEventArgs = new RunWorkerCompletedEventArgs();
        public delegate void DoWorkEventHandler(object sender, DoWorkEventArgs e);
        public delegate void ProgressChangedEventHandler(object sender, ProgressChangedEventArgs e);
        public delegate void RunWorkerCompletedEventHandler(object sender, RunWorkerCompletedEventArgs e);
        public event ProgressChangedEventHandler ProgressChanged;
        public event RunWorkerCompletedEventHandler RunWorkerCompleted;
        public event DoWorkEventHandler DoWork;
        public void ReportProgress(int percentProgress, object userState)
        {
            if (!WorkerReportsProgress)
                return;
            ProgressChangedEventArgs e = new ProgressChangedEventArgs();
            e.UserState = userState;
            e.ProgressPercentage = percentProgress;
            doWorkEventArgs.UIcontext.Send((noteUsed) => ProgressChanged(this, e), null);
        }//
        public void RunWorkerAsync()
        {
            SynchronizationContext InterfaceContext = SynchronizationContext.Current;
            doWorkEventArgs.UIcontext = InterfaceContext;
            ThreadPool.QueueUserWorkItem((notUsed) => OnDoWork(doWorkEventArgs));
        }
        protected virtual void OnRunWorkerCompleted(RunWorkerCompletedEventArgs e)
        {
            RunWorkerCompleted(this, runWorkerCompletedEventArgs);
        }
        protected virtual void OnDoWork(DoWorkEventArgs e)
        {
            this.IsBusy = true;
            this.CancellationPending = false;
            DoWork(this, e);
            runWorkerCompletedEventArgs.Result = e.Result;
            OnRunWorkerCompleted(runWorkerCompletedEventArgs);
            this.IsBusy = false;
        }
        public void CancelAsync()
        {
            if (WorkerSupportsCancellation)
            {

                this.CancellationPending = true;
            }
            else
            {
                throw new InvalidOperationException("This BackgroundWorker doesn't support cancellation.");
            }
        }
    }
    public class RunWorkerCompletedEventArgs
    {
        public object Result { get; set; }
    }
    public class ProgressChangedEventArgs : EventArgs
    {
        public int ProgressPercentage { get; set; }
        public object UserState { get; set; }
    }
    public class DoWorkEventArgs : EventArgs
    {
        public object Result { get; set; }
        public SynchronizationContext UIcontext { get; set; }
    }

}
 

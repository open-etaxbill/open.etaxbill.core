using System;
using System.ServiceProcess;

namespace OpenTax.Engine.Collector
{
    public partial class eTaxCollector : ServiceBase
    {
        //-------------------------------------------------------------------------------------------------------------------------
        // 
        //-------------------------------------------------------------------------------------------------------------------------
        public eTaxCollector()
        {
            InitializeComponent();
        }

        //-------------------------------------------------------------------------------------------------------------------------
        // 
        //-------------------------------------------------------------------------------------------------------------------------
        private OpenTax.Engine.Collector.Worker m_collectWorker = null;
        private OpenTax.Engine.Collector.Worker CollectWorker
        {
            get
            {
                if (m_collectWorker == null)
                    m_collectWorker = new OpenTax.Engine.Collector.Worker();

                return m_collectWorker;
            }
        }

        private OpenTax.Engine.Collector.Host m_collectHoster = null;
        private OpenTax.Engine.Collector.Host CollectHoster
        {
            get
            {
                if (m_collectHoster == null)
                    m_collectHoster = new OpenTax.Engine.Collector.Host();

                return m_collectHoster;
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------
        // 
        //-------------------------------------------------------------------------------------------------------------------------
        protected override void OnStart(string[] args)
        {
            ELogger.SNG.WriteLog("server service start...");

            CollectHoster.Start();
            CollectWorker.Start();

            base.OnStart(args);
        }

        protected override void OnStop()
        {
            base.OnStop();

            CollectWorker.Stop();
            CollectHoster.Stop();

            ELogger.SNG.WriteLog("server service stop...");
        }

        //-------------------------------------------------------------------------------------------------------------------------
        // 
        //-------------------------------------------------------------------------------------------------------------------------
    }
}
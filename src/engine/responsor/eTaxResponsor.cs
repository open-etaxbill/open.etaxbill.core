using System.ServiceProcess;

namespace OpenTax.Engine.Responsor
{
    public partial class eTaxResponsor : ServiceBase
    {
        //-------------------------------------------------------------------------------------------------------------------------
        // 
        //-------------------------------------------------------------------------------------------------------------------------
        public eTaxResponsor()
        {
            InitializeComponent();
        }

        //-------------------------------------------------------------------------------------------------------------------------
        // 
        //-------------------------------------------------------------------------------------------------------------------------
        private OpenTax.Channel.Interface.IResponsor m_iresponsor = null;
        private OpenTax.Channel.Interface.IResponsor IResponsor
        {
            get
            {
                if (m_iresponsor == null)
                    m_iresponsor = new OpenTax.Channel.Interface.IResponsor();

                return m_iresponsor;
            }
        }

        private OpenTax.Engine.Library.UAppHelper m_appHelper = null;
        public OpenTax.Engine.Library.UAppHelper UAppHelper
        {
            get
            {
                if (m_appHelper == null)
                    m_appHelper = new OpenTax.Engine.Library.UAppHelper(IResponsor.Manager);

                return m_appHelper;
            }
        }

        private OpenTax.Engine.Responsor.WebListener m_responseWorker = null;
        private OpenTax.Engine.Responsor.WebListener ResponseWorker
        {
            get
            {
                if (m_responseWorker == null)
                    m_responseWorker = new OpenTax.Engine.Responsor.Worker(UAppHelper.HostAddress, UAppHelper.PortNumber, UAppHelper.WebFolder);

                return m_responseWorker;
            }
        }

        private OpenTax.Engine.Responsor.Host m_responseHoster = null;
        private OpenTax.Engine.Responsor.Host ResponseHoster
        {
            get
            {
                if (m_responseHoster == null)
                    m_responseHoster = new OpenTax.Engine.Responsor.Host();

                return m_responseHoster;
            }
        }
        
        //-------------------------------------------------------------------------------------------------------------------------
        // 
        //-------------------------------------------------------------------------------------------------------------------------
        protected override void OnStart(string[] args)
        {
            ELogger.SNG.WriteLog("server service start...");

            ResponseHoster.Start();
            ResponseWorker.Start();
 
            base.OnStart(args);
        }

        protected override void OnStop()
        {
            base.OnStop();

            ResponseWorker.Stop();
            ResponseHoster.Stop();

            ELogger.SNG.WriteLog("server service stop...");
        }
    
        //-------------------------------------------------------------------------------------------------------------------------
        // 
        //-------------------------------------------------------------------------------------------------------------------------
    }
}